using System;
using _MultiplayerGameTest._Scripts.Core.Identity;
using _MultiplayerGameTest._Scripts.Core.Networking;
using _MultiplayerGameTest._Scripts.Transports.Unity;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.Bootstrap
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private NetworkSettings networkSettings;
        [SerializeField] private NetworkManager networkManager;
        
        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }
        
        protected override void Configure(IContainerBuilder builder)
        {
            var options = LaunchOptionsParser.Parse(Environment.GetCommandLineArgs(), networkSettings);
            Debug.Log($"[Boot] Launch options: {options}");
            
            builder.RegisterInstance(networkSettings);
            builder.RegisterInstance(options);
            builder.RegisterComponent(networkManager);
            
            RegisterTransport(builder, options.Transport);
            
            builder.Register<LocalPlayerIdentityProvider>(Lifetime.Singleton).As<IPlayerIdentityProvider>();
            builder.Register<ConnectionApprovalHandler>(Lifetime.Singleton);
            builder.RegisterEntryPoint<NetworkSessionService>().As<INetworkSessionService>();            
            
            builder.RegisterEntryPoint<EntryPoint>();
        }

        private static void RegisterTransport(IContainerBuilder builder, TransportKind kind)
        {
            switch (kind)
            {
                case TransportKind.Steam:
                    Debug.LogWarning("Steam transport is not implemented yet, falling back to Unity Transport.");
                    goto default;
                case TransportKind.UnityTransport:
                default:
                    builder.Register<UnityTransportProvider>(Lifetime.Singleton).As<INetworkTransportProvider>();
                    break;
            }
        }
    }
}