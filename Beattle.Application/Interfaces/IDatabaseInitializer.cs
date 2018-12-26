using System.Threading.Tasks;

namespace Beattle.Application.Interfaces
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync(string[] adminRoleClaims);
    }
}
