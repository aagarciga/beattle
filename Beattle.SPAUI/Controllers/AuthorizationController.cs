using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.Extensions.Options;
using Beattle.Application.Interfaces;
using Beattle.Identity;
using System.Security.Claims;

namespace Beattle.SPAUI.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class AuthorizationController : Controller
    {
        private readonly IOptions<IdentityOptions> options;
        private readonly SignInManager<IApplicationUser> signInManager;
        private readonly UserManager<IApplicationUser> userManager;

        public AuthorizationController(
            IOptions<IdentityOptions> options,
            SignInManager<IApplicationUser> signInManager,
            UserManager<IApplicationUser> userManager)
        {
            this.options = options;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Exchange(OpenIdConnectRequest openIdConnectRequest)
        {

            if (openIdConnectRequest.IsPasswordGrantType())
            {
                return await ExchangeIfPasswordGrantType(openIdConnectRequest);
            }
            else if (openIdConnectRequest.IsRefreshTokenGrantType())
            {
                return await ExchangeIfRefreshTokenGrantType(openIdConnectRequest);
            }
            return ErrorResult("The specified grant type is not supported", OpenIdConnectConstants.Errors.UnsupportedGrantType);

        }

        private async Task<IActionResult> ExchangeIfRefreshTokenGrantType(OpenIdConnectRequest request)
        {
            // Retrieve the claims principal stored in the refresh token.
            AuthenticateResult info = await HttpContext.AuthenticateAsync(OpenIddictServerDefaults.AuthenticationScheme);

            // Retrieve the user profile corresponding to the refresh token.
            // Note: if you want to automatically invalidate the refresh token
            // when the user password/roles change, use the following line instead:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            IApplicationUser user = await userManager.GetUserAsync(info.Principal);
            if (user == null)
            {
                return ErrorResult("The token is no longer valid");
            }

            // Ensure the user is still allowed to sign in.
            if (!await signInManager.CanSignInAsync(user))
            {
                return ErrorResult("The user is no longer allowed to sign in");
            }

            // Create a new authentication ticket, but reuse the properties stored
            // in the refresh token, including the scopes originally granted.
            var ticket = await CreateTicketAsync(request, user as ApplicationUser);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> ExchangeIfPasswordGrantType(OpenIdConnectRequest request)
        {
            IApplicationUser user = await userManager.FindByEmailAsync(request.Username)
                    ?? await userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return ErrorResult("Please check that your email and password is correct");
            }
            if (!user.IsEnabled)
            {
                return ErrorResult("The specified user account is disabled");
            }

            // Validate the username/password parameters
            Microsoft.AspNetCore.Identity.SignInResult signInResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            // Ensure the user is not already locked out
            if (signInResult.IsLockedOut)
            {
                return ErrorResult("The specified user account has been suspended");
            }

            // Reject the token request if two-factor authentication has been enabled by the user
            if (signInResult.RequiresTwoFactor)
            {
                return ErrorResult("Invalid login procedure. Use two-factor authentication instead");
            }

            // Ensure the user is allowed to sign in.
            if (signInResult.IsNotAllowed)
            {
                return ErrorResult("The specified user is not allowed to sign in");
            }

            if (!signInResult.Succeeded)
            {
                return ErrorResult("Please check that your email and password is correct");
            }

            // Create a new authentication ticket.
            AuthenticationTicket ticket = await CreateTicketAsync(request, user as ApplicationUser);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private BadRequestObjectResult ErrorResult(string message, string openIdError = OpenIdConnectConstants.Errors.InvalidGrant)
        {
            return BadRequest(new OpenIdConnectResponse
            {
                Error = openIdError,
                ErrorDescription = message
            });
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest openIdConnectRequest, ApplicationUser applicationUser)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            ClaimsPrincipal principal = await signInManager.CreateUserPrincipalAsync(applicationUser);

            // Create a new authentication ticket holding the user identity.
            AuthenticationTicket ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), OpenIddictServerDefaults.AuthenticationScheme);


            //if (!request.IsRefreshTokenGrantType())
            //{
            // Set the list of scopes granted to the client application.
            // Note: the offline_access scope must be granted
            // to allow OpenIddict to return a refresh token.
            ticket.SetScopes(new[]
            {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Phone,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles
            }.Intersect(openIdConnectRequest.GetScopes()));
            //}

            //ticket.SetResources("beattle-webapi");

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == options.Value.ClaimsIdentity.SecurityStampClaimType)
                    continue;

                List<string> destinations = new List<string> { OpenIdConnectConstants.Destinations.AccessToken };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Subject && ticket.HasScope(OpenIdConnectConstants.Scopes.OpenId)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)) ||
                    (claim.Type == ApplicationClaimType.Authorization && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            ClaimsIdentity identity = principal.Identity as ClaimsIdentity;


            if (ticket.HasScope(OpenIdConnectConstants.Scopes.Profile))
            {

                if (!string.IsNullOrWhiteSpace(applicationUser.Name))
                    identity.AddClaim(ApplicationClaimType.Name, applicationUser.Name, OpenIdConnectConstants.Destinations.IdentityToken);

                if (!string.IsNullOrWhiteSpace(applicationUser.Configuration))
                    identity.AddClaim(ApplicationClaimType.Setting, applicationUser.Configuration, OpenIdConnectConstants.Destinations.IdentityToken);
            }

            if (ticket.HasScope(OpenIdConnectConstants.Scopes.Email))
            {
                if (!string.IsNullOrWhiteSpace(applicationUser.Email))
                    identity.AddClaim(ApplicationClaimType.Email, applicationUser.Email, OpenIdConnectConstants.Destinations.IdentityToken);
            }

            if (ticket.HasScope(OpenIdConnectConstants.Scopes.Phone))
            {
                if (!string.IsNullOrWhiteSpace(applicationUser.PhoneNumber))
                    identity.AddClaim(ApplicationClaimType.PhoneNumber, applicationUser.PhoneNumber, OpenIdConnectConstants.Destinations.IdentityToken);
            }

            return ticket;
        }
    }
}