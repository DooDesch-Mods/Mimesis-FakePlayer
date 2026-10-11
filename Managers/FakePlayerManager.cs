using System;
using System.Collections.Generic;
using System.Reflection;
using FakePlayers.Config;
using MelonLoader;
using ReluProtocol.C2S;
using ReluProtocol.Enum;

namespace FakePlayers.Managers
{
    internal static class FakePlayerManager
    {
        private static readonly List<FakeSession> _fakeSessions = new List<FakeSession>();
        private static readonly FieldInfo SessionManagerField = typeof(VWorld).GetField("_sessionManager", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo DispatchersField = typeof(VWorld).GetField("_serverDispatchers", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo VersionField = typeof(VWorld).GetField("Version", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo PlayerField = typeof(SessionContext).GetField("_vPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo InviteField = typeof(Hub).GetField("steamInviteDispatcher", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo HostContextField = typeof(SessionManager).GetField("_hostSessionContext", BindingFlags.NonPublic | BindingFlags.Instance);
        private static VWorld _world;
        private static SessionManager _sessionManager;
        private static ServerDispatchManager _dispatchers;
        private static int _remaining;
        private static int _created;
        private static bool _removing;

        internal static bool TryGetName(ulong steamID, out string name)
        {
            foreach (var session in _fakeSessions)
            {
                if (session.Context is SessionContext context && context.SteamID == steamID && steamID != 0)
                {
                    name = session.Name;
                    return true;
                }
            }
            name = null;
            return false;
        }

        internal static void CreateFakePlayers(VWorld world, GameSessionInfo gameSessionInfo)
        {
            _world = world;
            _sessionManager = (SessionManager)(SessionManagerField ?? throw new MissingFieldException("VWorld", "_sessionManager")).GetValue(world);
            _dispatchers = (ServerDispatchManager)(DispatchersField ?? throw new MissingFieldException("VWorld", "_serverDispatchers")).GetValue(world);
            _remaining = FakePlayersPreferences.Enabled ? FakePlayersPreferences.FakePlayerCount : 0;
            _created = 0;
            if (_remaining > 0) MelonLogger.Msg($"Waiting for the host avatar before joining {_remaining} fake players.");
        }

        private static VPlayer GetHostPlayer()
        {
            var context = (SessionContext)(HostContextField ?? throw new MissingFieldException("SessionManager", "_hostSessionContext")).GetValue(_sessionManager);
            return context == null ? null : (VPlayer)(PlayerField ?? throw new MissingFieldException("SessionContext", "_vPlayer")).GetValue(context);
        }

        internal static void Update()
        {
            if (_world == null) return;
            try
            {
                if (_removing) { RemoveFakePlayers(null); return; }
                var host = GetHostPlayer();
                if (_remaining > 0 && host != null && host.LevelLoadCompleted)
                {
                    int number = ++_created;
                    _remaining--;
                    var session = new FakeSession(_sessionManager.GetNewSessionID(), _dispatchers, $"FakePlayer{number}");
                    _fakeSessions.Add(session);
                    var context = new SessionContext(session);
                    _sessionManager.Add(context);
                    session.Request(new JoinServerReq
                    {
                        guid = Guid.NewGuid().ToString(),
                        steamID = 76561198000000000UL + (ulong)number - 1,
                        nickName = session.Name,
                        clientVersion = (string)(VersionField ?? throw new MissingFieldException("VWorld", "Version")).GetValue(_world),
                        roomPassword = ((SteamInviteDispatcher)(InviteField ?? throw new MissingFieldException("Hub", "steamInviteDispatcher")).GetValue(Hub.s)).RoomPassword,
                        isHost = false
                    });
                }
                foreach (var session in _fakeSessions.ToArray())
                {
                    if (session.Failed)
                    {
                        if (!session.RemovalRequested)
                        {
                            _sessionManager.Remove(session.ID, DisconnectReason.KickByServer);
                            session.RemovalRequested = true;
                            MelonLogger.Msg($"Rejected fake session removal requested: {session.Name}, session {session.ID}.");
                        }
                        if (_sessionManager.GetSession(session.ID) == null)
                        {
                            _fakeSessions.Remove(session);
                            MelonLogger.Msg($"Rejected fake session removal confirmed: {session.Name}, session {session.ID}.");
                        }
                        continue;
                    }
                    session.Update(host);
                }
            }
            catch (Exception error)
            {
                _remaining = 0;
                MelonLogger.Error($"Fake player update failed; further joins stopped: {error}");
                _removing = true;
            }
        }

        internal static bool RemoveFakePlayers(GameSessionInfo gameSessionInfo)
        {
            _remaining = 0;
            _removing = true;
            foreach (var session in _fakeSessions.ToArray())
            {
                try
                {
                    if (!session.RemovalRequested)
                    {
                        session.RemovalRequested = true;
                        session.Close(DisconnectReason.KickByServer);
                        _sessionManager.Remove(session.ID, DisconnectReason.KickByServer);
                        MelonLogger.Msg($"Fake session removal requested: {session.Name}, session {session.ID}.");
                    }
                    if (_sessionManager.GetSession(session.ID) == null) _fakeSessions.Remove(session);
                }
                catch (Exception error)
                {
                    MelonLogger.Error($"Fake session removal failed: {session.Name}, session {session.ID}: {error}");
                }
            }
            if (_fakeSessions.Count != 0) return false;
            _world = null;
            _sessionManager = null;
            _dispatchers = null;
            _removing = false;
            MelonLogger.Msg("Fake session removal confirmed.");
            return true;
        }
    }
}
