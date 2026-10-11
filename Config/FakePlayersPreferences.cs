using System;
using DooDesch;
using MelonLoader;

namespace FakePlayers.Config
{
	internal static class FakePlayersPreferences
	{
		private const string CategoryId = "FakePlayers";

		private static MelonPreferences_Category _category;
		private static MelonPreferences_Entry<int> _fakePlayerCount;
		private static MelonPreferences_Entry<bool> _enabled;
		private static HostSetting<bool> _effectiveEnabled;
		private static HostSetting<int> _effectiveFakePlayerCount;
		private static HostSettings _hostSettings;
		internal static event Action EffectiveValuesChanged;
		internal static bool IsHostSession => _hostSettings != null && _hostSettings.IsInSession && _hostSettings.IsHost;

		internal static void Initialize()
		{
			if (_category != null)
			{
				return;
			}

			_category = MelonPreferences.CreateCategory(CategoryId, "FakePlayers");
			_enabled = CreateEntry("Enabled", true, "Enable Fake Players", 
				"Enable or disable fake player emulation for testing.");
			_fakePlayerCount = CreateEntry("FakePlayerCount", 3, "Fake Player Count", 
				"The number of fake players to spawn. Default: 3 (for a total of 4 players including yourself). Minimum: 0, Maximum: 32762.");
			
			// Validate and clamp the value
			if (_fakePlayerCount.Value < 0)
			{
				MelonLogger.Warning($"FakePlayerCount value {_fakePlayerCount.Value} is below minimum of 0. Clamping to 0.");
				_fakePlayerCount.Value = 0;
			}
			else if (_fakePlayerCount.Value > (32766 - 4))
			{
				MelonLogger.Warning($"FakePlayerCount value {_fakePlayerCount.Value} exceeds maximum of (32766 - 4). Clamping to (32766 - 4).");
				_fakePlayerCount.Value = (32766 - 4);
			}

			_hostSettings = new HostSettings(CategoryId);
			_effectiveEnabled = _hostSettings.Register(_enabled, false);
			_effectiveFakePlayerCount = _hostSettings.Register(_fakePlayerCount, 0, value => value >= 0 && value <= 32762);
			_hostSettings.EffectiveValuesChanged += () => EffectiveValuesChanged?.Invoke();
		}

		private static MelonPreferences_Entry<T> CreateEntry<T>(string identifier, T defaultValue, string displayName, string description = null)
		{
			if (_category == null)
			{
				throw new InvalidOperationException("Preference category not initialized.");
			}

			return _category.CreateEntry(identifier, defaultValue, displayName, description);
		}

		internal static bool Enabled => _effectiveEnabled?.Value ?? _enabled?.Value ?? true;
		internal static int FakePlayerCount => _effectiveFakePlayerCount?.Value ?? _fakePlayerCount?.Value ?? 3;
	}
}

