using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class NavigationService : INavigationService
    {
        IDbContextFactory<ChinookContext> DbFactory { get; set; }
        private List<Playlist> Playlists;
        public NavigationService(IDbContextFactory<ChinookContext> dbFactory)
        {
            DbFactory = dbFactory;
        }
        public async Task<List<Models.Playlist>> GetUserPlayListsAsync(string userId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();
            return DbContext.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId)).ToList();
        }

        public async Task TriggerPlaylistChanged(string userId)
        {
            Playlists = await GetUserPlayListsAsync(userId);
            this.PlaylistChanged?.Invoke(this, EventArgs.Empty);
        }

        public List<Models.Playlist> GetUserPlayLists()
        {
            return Playlists;
        }

        public event EventHandler? PlaylistChanged;
    }
}
