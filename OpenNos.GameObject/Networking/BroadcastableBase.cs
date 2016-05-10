using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace OpenNos.GameObject
{
    public abstract class BroadcastableBase
    {
        #region Members

        private ISubject<SessionPacket> _subject;

        #endregion

        #region Instantiation

        public BroadcastableBase()
        {
            Sessions = new Dictionary<ClientSession, IDisposable>();
            _subject = new Subject<SessionPacket>();
        }

        #endregion

        #region Properties

        public IDictionary<ClientSession, IDisposable> Sessions { get; set; }

        #endregion

        #region Methods

        public bool Broadcast(string message)
        {
            return Broadcast(null, message);
        }

        public bool Broadcast(ClientSession client, string message, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1)
        {
            switch (receiver)
            {
                case ReceiverType.All:
                    for (int i = Sessions.Keys.Where(s => s != null && s.Character != null).Count() - 1; i >= 0; i--)
                        Sessions.Keys.Where(s => s != null).ElementAt(i).Client.SendPacket(message);
                    break;

                case ReceiverType.AllExceptMe:
                    for (int i = Sessions.Keys.Where(s => s != null && s.Character != null && s != client).Count() - 1; i >= 0; i--)
                        Sessions.Keys.Where(s => s != null && s != client).ElementAt(i).Client.SendPacket(message);
                    break;

                case ReceiverType.OnlyMe:
                    client.Client.SendPacket(message);
                    break;

                case ReceiverType.OnlySomeone:
                    ClientSession targetSession = Sessions.Keys.FirstOrDefault(s => s.Character != null && (s.Character.Name.Equals(characterName) || s.Character.CharacterId.Equals(characterId)));

                    if (targetSession == null) return false;

                    targetSession.Client.SendPacket(message);
                    return true;

                case ReceiverType.AllNoEmoBlocked:
                    foreach (ClientSession session in Sessions.Keys.Where(s => s.Character != null && s.Character.MapId.Equals(client.Character.MapId) && !s.Character.EmoticonsBlocked))
                        session.Client.SendPacket(message);
                    break;

                case ReceiverType.AllNoHeroBlocked:
                    foreach (ClientSession session in Sessions.Keys.Where(s => s.Character != null && !s.Character.HeroChatBlocked))
                        session.Client.SendPacket(message);
                    break;

                case ReceiverType.Group:
                    foreach (ClientSession session in Sessions.Keys.Where(s => s.Character != null && s.Character.Group != null && s.Character.Group.GroupId.Equals(client.Character.Group.GroupId)))
                        session.Client.SendPacket(message);
                    break;
            }

            return true;
        }

        public virtual void RegisterSession(ClientSession session)
        {
            if (!Sessions.ContainsKey(session))
            {
                Sessions.Add(session, _subject.Subscribe(s => session.CallbackSessionRequest(s)));
            }
        }

        public virtual void UnregisterSession(ClientSession session)
        {
            if (Sessions.ContainsKey(session))
            {
                Sessions[session].Dispose();
                Sessions.Remove(session);
            }
        }

        #endregion
    }
}