using VContainer;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.UI.Lobby
{
    public class LobbyLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<LobbyView>();
            builder.RegisterEntryPoint<LobbyPresenter>();
        }
    }
}