using Chinook.Models;

namespace Chinook.Services
{
    public interface IHomeService
    {
        Task<List<Artist>> GetArtistsAsync();
    }
}
