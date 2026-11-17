using VContainer;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.UI.MainMenu
{
    public class MainMenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<MainMenuView>();
            builder.RegisterEntryPoint<MainMenuPresenter>();
        }
    }
}