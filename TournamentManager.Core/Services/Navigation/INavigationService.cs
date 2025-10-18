using TournamentManager.Core.Models;
using TournamentManager.Core.Models.Responses;

namespace TournamentManager.Core.Services.Navigation
{
    public interface INavigationService
    {
        event Action<UserInfo> OnNavigateToMain;
        event Action OnNavigateToLogin;

        void NavigateToMain(UserInfo user);
        void NavigateToLogin();
    }
}
