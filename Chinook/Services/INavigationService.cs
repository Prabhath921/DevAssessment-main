using Chinook.Models;

namespace Chinook.Services
{
    public interface INavigationService
    {
        event EventHandler? PlaylistChanged;
        List<Models.Playlist> GetUserPlayLists();
        Task TriggerPlaylistChanged(string userId);
        Task<List<Models.Playlist>> GetUserPlayListsAsync(string userId);
    }
}
