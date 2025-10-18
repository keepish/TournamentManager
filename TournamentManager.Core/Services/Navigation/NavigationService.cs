using TournamentManager.Core.Models;
using TournamentManager.Core.Models.Responses;

namespace TournamentManager.Core.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        public event Action<UserInfo> OnNavigateToMain;
        public event Action OnNavigateToLogin;

        public void NavigateToLogin()
        {
            OnNavigateToLogin?.Invoke();
        }

        public void NavigateToMain(UserInfo user)
        {
            OnNavigateToMain?.Invoke(user);
        }
    }
}
