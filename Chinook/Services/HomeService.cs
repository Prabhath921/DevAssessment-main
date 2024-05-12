using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class HomeService : IHomeService
    {
        IDbContextFactory<ChinookContext> DbFactory { get; set; }
        public HomeService(IDbContextFactory<ChinookContext> dbFactory)
        {
            DbFactory = dbFactory;
        }

        public async Task<List<Artist>> GetArtistsAsync()
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            var users = dbContext.Users.Include(a => a.UserPlaylists).ToList();
            List<Artist> Artists = dbContext.Artists.ToList();
            Artists.ForEach(async artist => artist.Albums = await GetAlbumsForArtist(artist.ArtistId));
            return Artists;
        }

        private async Task<List<Album>> GetAlbumsForArtist(long artistId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }
    }
}
