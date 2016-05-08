using System;
using System.Collections.Generic;
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
            Sessions = new List<ClientSession>();
            _subject = new Subject<SessionPacket>();
        }

        #endregion

        #region Properties

        public List<ClientSession> Sessions { get; set; }

        #endregion

        #region Methods

        public bool Broadcast(string message)
        {
            return Broadcast(null, message);
        }

        public bool Broadcast(ClientSession client, string message, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1)
        {
            SessionPacket packet = new SessionPacket(client, message, receiver, characterName, characterId);
            _subject.OnNext(packet);
            return true;
        }

        public virtual void RegisterSession(ClientSession session)
        {
            _subject.Subscribe(s => session.CallbackSessionRequest(s));
            Sessions.Add(session);
        }

        public virtual void UnregisterSession(ClientSession session)
        {
            session.Dispose();
            Sessions.Remove(session);
        }

        #endregion
    }
}