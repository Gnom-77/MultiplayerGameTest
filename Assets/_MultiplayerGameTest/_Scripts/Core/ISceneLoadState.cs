using System;
using System.Collections.Generic;

namespace _MultiplayerGameTest._Scripts.Core
{
    public interface ISceneLoadState
    {
        bool IsLoading { get; }
        string SceneName { get; }

        IReadOnlyDictionary<ulong, ClientLoadStatus> ClientStatuses { get; }

        event Action Changed;
    }
}