using _MultiplayerGameTest._Scripts.Core.Pause;
using VContainer;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.UI.Pause
{
    /// <summary>
    /// Scene-level scope for the game scene. Parent is the persistent root scope.
    /// </summary>
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PauseView>();
            builder.RegisterEntryPoint<PausePresenter>();
        }
    }
}