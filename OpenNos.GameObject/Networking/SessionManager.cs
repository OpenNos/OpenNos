using OpenNos.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace OpenNos.GameObject
{
    public class SessionManager
    {
        #region Members

        protected Type _packetHandler;
        protected ConcurrentDictionary<long, ClientSession> _sessions = new ConcurrentDictionary<long, ClientSession>();

        #endregion

        #region Instantiation

        public SessionManager(Type packetHandler, bool isWorldServer)
        {
            _packetHandler = packetHandler;
            IsWorldServer = isWorldServer;
        }

        #endregion

        #region Properties

        public bool IsWorldServer { get; set; }

        #endregion

        #region Methods

        public void AddSession(INetworkClient customClient)
        {
            Logger.Log.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + customClient.ClientId);

            ClientSession session = IntializeNewSession(customClient);

            if (session != null && IsWorldServer)
            {
                if (!_sessions.TryAdd(customClient.ClientId, session))
                {
                    Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), customClient.ClientId);
                    customClient.Disconnect();
                    _sessions.TryRemove(customClient.ClientId, out session);
                    return;
                }
            }
        }

        public void RemoveSession(INetworkClient client)
        {
            ClientSession session;
            _sessions.TryRemove(client.ClientId, out session);

            // check if session hasnt been already removed
            if (session != null)
            {
                session.IsDisposing = true;

                if (IsWorldServer)
                {
                    if (session.HasSelectedCharacter)
                    {
                        if (ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(session.Character.CharacterId)))
                        {
                            ServerManager.Instance.GroupLeave(session);
                        }

                        session.Character.Save();

                        // only remove the character from map if the character has been set
                        session.CurrentMap?.Broadcast(session, session.Character.GenerateOut(), ReceiverType.AllExceptMe);
                    }
                }

                session.Destroy();
                client.Disconnect();
                Logger.Log.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + client.ClientId);
                session = null;
            }
        }

        protected virtual ClientSession IntializeNewSession(INetworkClient client)
        {
            return new ClientSession(client);
        }

        public virtual void StopServer()
        {
            _sessions.Clear();
            ServerManager.Instance.StopServer();
        }

        #endregion
    }
}