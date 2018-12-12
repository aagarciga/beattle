# SPA UI
.Net Core 2.2 Web API + Angular 6 + Bootstrap 4

## Productivity Tips

### Run the front end independently

As per the [documentation](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa/?view=aspnetcore-2.2), 
the project is configured to start the front end in the background when ASP.NET Core starts in development mode. 
This feature is designed with productivity in mind. However, when making frequent back end changes productivity 
can suffer as it takes up to 10 seconds to launch the application after a back end change.

You can launch the front end independently by updating the **Configure** method within the **Startup** class.

	// spa.UseAngularCliServer(npmScript: "start");
	spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");

Then, to launch the application, open a command line to start the front end:

	cd ClientApp
	npm start

Next, open a second command line and start the back end:

	dotnet run

This will ensure your application will launch quickly when making either front end or back end changes.

### Launch the back end with *dotnet watch*

Currently, changes to back end files require restarting the ASP.NET Core server. We can avoid this altogether by running:

	dotnet watch run

The watch tool monitors backend files for changes, automatically building and restarting the server when changes are detected. Should the build fail, the tool simply waits for a new change, rebuilds, and continues normally.

By running the front end independently and launching the back end with dotnet watch, changes to the front end and back end will be built and available immediately, leaving you free to write more code!

## Commands History

	dotnet restore

	dotnet add package AutoMapper --version 8.0.0
