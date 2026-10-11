using System;
using HarmonyLib;
using FakePlayers.Config;
using FakePlayers.Managers;
using MelonLoader;
using ReluProtocol.Enum;

namespace FakePlayers.Patches
{
	/// <summary>
	/// Patches for VWorld to inject fake players after the game session is initialized.
	/// </summary>
	internal static class VWorldPatches
	{
		private static VWorld _createdWorld;
		private static GameSessionInfo _createdSessionInfo;

		internal static void OnSettingsChanged()
		{
			if (!FakePlayersPreferences.IsHostSession && !CleanupCreatedPlayers())
			{
				MelonLogger.Warning("Fake session cleanup remains incomplete after settings reset; new fake creation is blocked.");
			}
		}

		private static bool CleanupCreatedPlayers()
		{
			if (_createdWorld == null) return true;
			if (!FakePlayerManager.RemoveFakePlayers(_createdSessionInfo)) return false;
			_createdWorld = null;
			_createdSessionInfo = null;
			return true;
		}

		[HarmonyPatch(typeof(VWorld), "OnDestroy")]
		internal static class DestroyPatch
		{
			[HarmonyPrefix]
			private static void Prefix(VWorld __instance)
			{
				if (ReferenceEquals(_createdWorld, __instance) && !CleanupCreatedPlayers())
				{
					MelonLogger.Warning("Fake session cleanup incomplete before VWorld.OnDestroy; continuing vanilla world teardown.");
				}
			}
		}

		/// <summary>
		/// Patch RegistPlayer to add fake players after the host player is registered.
		/// </summary>
		[HarmonyPatch(typeof(VWorld), "RegistPlayer")]
		internal static class RegistPlayerPatch
		{
			private static void Postfix(VWorld __instance, ulong steamID, bool isHost, MsgErrorCode __result)
			{
				// Only add fake players if the host registration was successful and we haven't created them yet
				if (!FakePlayersPreferences.Enabled || __result != MsgErrorCode.Success || !isHost || ReferenceEquals(_createdWorld, __instance))
				{
					return;
				}

				try
				{
					// Get VRoomManager from VWorld
					var vRoomManager = __instance.VRoomManager;
					if (vRoomManager == null)
					{
						MelonLogger.Warning("VRoomManager is null, cannot add fake players");
						return;
					}

					// Get GameSessionInfo from VRoomManager using reflection
					var gameSessionInfoField = typeof(VRoomManager).GetField("_gameSessionInfo", 
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					if (gameSessionInfoField == null)
					{
						MelonLogger.Error("Could not find _gameSessionInfo field in VRoomManager");
						return;
					}

					var gameSessionInfo = gameSessionInfoField.GetValue(vRoomManager) as GameSessionInfo;
					if (gameSessionInfo == null)
					{
						MelonLogger.Error("_gameSessionInfo is null in VRoomManager");
						return;
					}

					// Create fake players immediately (the game should be ready by now)
					if (!CleanupCreatedPlayers())
					{
						MelonLogger.Error("Cannot create fake players in the new world: previous fake session cleanup is incomplete.");
						return;
					}
					_createdWorld = __instance;
					_createdSessionInfo = gameSessionInfo;
					FakePlayerManager.CreateFakePlayers(__instance, gameSessionInfo);
				}
				catch (Exception ex)
				{
					MelonLogger.Error($"Error in RegistPlayerPatch: {ex.Message}\n{ex.StackTrace}");
				}
			}
		}
	}
}

