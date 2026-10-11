using FakePlayers.Managers;
using HarmonyLib;

namespace FakePlayers.Patches
{
    [HarmonyPatch(typeof(GameMainBase), "ResolveNickName", new[] { typeof(string), typeof(string) })]
    internal static class PlayerNamePatch
    {
        private static bool Prefix(string steamID, ref string __result)
        {
            if (!ulong.TryParse(steamID, out var id) || !FakePlayerManager.TryGetName(id, out var name)) return true;
            __result = name;
            return false;
        }
    }
}
