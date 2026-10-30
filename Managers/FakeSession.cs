using System;
using System.Collections.Generic;
using System.Net;
using MelonLoader;
using FakePlayers.Config;
using ReluNetwork.ConstEnum;
using ReluProtocol;
using ReluProtocol.C2S;
using ReluProtocol.Enum;
using UnityEngine;

namespace FakePlayers.Managers
{
    // VirtualAcceptSession is reserved for the game's local host.
    internal sealed class FakeSession : ISession
    {
        private readonly ServerDispatchManager _dispatchers;
        private readonly Queue<IMsg> _requests = new Queue<IMsg>();
        private bool _running = true;
        private bool _joined;
        private VRoomType? _nextRoom;
        private float _deadline;
        private float _nextHeartbeat;
        private string _pending;

        public int ID { get; }
        public IContext Context { get; private set; }
        internal string Name { get; }
        internal bool Failed { get; private set; }
        internal bool RemovalRequested { get; set; }
        internal int HeartbeatAcknowledgements { get; private set; }

        internal FakeSession(int id, ServerDispatchManager dispatchers, string name)
        {
            ID = id;
            _dispatchers = dispatchers;
            Name = name;
        }

        internal void Request(IMsg message)
        {
            if (ReluProtocol.Version.IsEnable) message.hashCode = ReluProtocol.Version.GetProtocolHashCode();
            _requests.Enqueue(message);
            if (message is HeartBeatReq) return;
            _pending = message.GetType().Name;
            _deadline = Time.realtimeSinceStartup + FakePlayersPreferences.RequestTimeoutSeconds;
            MelonLogger.Msg($"Fake request: {Name}, {_pending}, session {ID}.");
        }

        internal void Update(VPlayer host)
        {
            if (!_running) return;
            if (_pending != null && Time.realtimeSinceStartup >= _deadline)
            {
                Fail($"{_pending} timed out after {FakePlayersPreferences.RequestTimeoutSeconds:F2} seconds");
                return;
            }
            if (_joined && Time.realtimeSinceStartup >= _nextHeartbeat)
            {
                Request(new HeartBeatReq());
                _nextHeartbeat = Time.realtimeSinceStartup + FakePlayersPreferences.HeartbeatIntervalSeconds;
            }
            var context = (SessionContext)Context;
            if (_nextRoom.HasValue && !context.ExistPlayer() && host != null && host.LevelLoadCompleted && host.VRoom.Property.vRoomType == _nextRoom.Value)
            {
                switch (_nextRoom.Value)
                {
                    case VRoomType.Maintenance: Request(new EnterMaintenanceRoomReq { playerUID = context.GetPlayerUID() }); break;
                    case VRoomType.Waiting: Request(new EnterWaitingRoomReq()); break;
                    case VRoomType.Game: Request(new EnterDungeonReq { roomID = host.VRoom.RoomID }); break;
                    case VRoomType.DeathMatch: Request(new EnterDeathMatchRoomReq()); break;
                    default: Fail($"Unsupported room type {_nextRoom.Value}"); return;
                }
                _nextRoom = null;
            }
            while (_running && _requests.Count > 0) _dispatchers.Dispatch(this, _requests.Dequeue());
        }

        private void Receive(IMsg message)
        {
            bool enter = message is EnterMaintenanceRoomRes || message is EnterWaitingRoomRes ||
                         message is EnterDungeonRes || message is EnterDeathMatchRoomRes;
            if (message is JoinServerRes || enter || message is LevelLoadCompleteRes)
            {
                var response = (IResMsg)message;
                if (response.errorCode != MsgErrorCode.Success)
                {
                    Fail($"{message.GetType().Name}: {response.errorCode}", response.errorCode == MsgErrorCode.PlayerCountExceeded);
                    return;
                }
                _pending = null;
                if (message is JoinServerRes)
                {
                    _joined = true;
                    Request(new CurrentRoomInfoReq());
                }
                else if (enter) Request(new LevelLoadCompleteReq());
                else if (message is LevelLoadCompleteRes loaded)
                {
                    if (loaded.selfInfo.isHost) { Fail("Fake actor was classified as host"); return; }
                    MelonLogger.Msg($"Fake avatar entered: {Name}, SteamID {((SessionContext)Context).SteamID}, UID {((SessionContext)Context).GetPlayerUID()}, actor {loaded.selfInfo.actorID}.");
                }
            }
            else if (message is HeartBeatRes heartbeat)
            {
                if (heartbeat.errorCode != MsgErrorCode.Success) { Fail($"HeartBeatRes: {heartbeat.errorCode}"); return; }
                if (++HeartbeatAcknowledgements == 1) MelonLogger.Msg($"Fake heartbeat confirmed: {Name}, session {ID}.");
            }
            else if (message is CurrentRoomInfoRes mode)
            {
                if (mode.errorCode != MsgErrorCode.Success) { Fail($"CurrentRoomInfoRes: {mode.errorCode}"); return; }
                if (mode.roomInfo == null || mode.roomInfo.roomType == VRoomType.Invalid)
                {
                    Fail("CurrentRoomInfoRes has no joinable room");
                    return;
                }
                _pending = null;
                _nextRoom = mode.roomInfo.roomType;
            }
            else if (message is MoveToMaintenanceRoomSig) _nextRoom = VRoomType.Maintenance;
            else if (message is MoveToWaitingRoomSig) _nextRoom = VRoomType.Waiting;
            else if (message is MoveToDungeonSig) _nextRoom = VRoomType.Game;
            else if (message is MoveToDeathMatchSig) _nextRoom = VRoomType.DeathMatch;
        }

        private void Fail(string cause, bool full = false)
        {
            if (Failed) return;
            if (full) MelonLogger.Warning($"Fake join rejected by player limit: {Name}, session {ID}: {cause}");
            else MelonLogger.Error($"Fake session failed: {Name}, session {ID}: {cause}");
            Failed = true;
            Close(DisconnectReason.KickByServer);
        }

        public SendResult SendLink(params IMsg[] messages)
        {
            if (!_running) return SendResult.NotConnect;
            foreach (var message in messages) Receive(message);
            return SendResult.Success;
        }
        public SendResult SendError<TResponse>(MsgErrorCode errorCode, string file = "", int line = 0) where TResponse : IResMsg, new()
            => SendLink(new TResponse { errorCode = errorCode });
        public bool SendLoopback(IMsg message) => _running && _dispatchers.LoopbackLink(this, message);
        public void Connect(IConnection connection) => throw new InvalidOperationException("FakeSession uses an in-process transport.");
        public void SetContext(IContext context) => Context = context;
        public void BeginClose(DisconnectReason reason) => Close(reason);
        public void Close(DisconnectReason reason) { _running = false; _requests.Clear(); }
        public bool IsRunning() => _running;
        public IPEndPoint GetRemoteEndPoint() => null;
    }
}
