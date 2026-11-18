namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public class SessionEndNotice
    {
        private SessionEndReason? _pending;

        public void Set(SessionEndReason reason) => _pending = reason;

        public bool TryConsume(out SessionEndReason reason)
        {
            reason = _pending.GetValueOrDefault();
            var has = _pending.HasValue;
            _pending = null;
            return has;
        }
    }
}