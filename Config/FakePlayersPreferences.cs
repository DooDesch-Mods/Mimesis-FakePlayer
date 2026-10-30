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
		private static HostSetting<string> _namePrefix;
		private static HostSetting<float> _joinIntervalSeconds;
		private static HostSetting<float> _requestTimeoutSeconds;
		private static HostSetting<float> _heartbeatIntervalSeconds;
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
			_namePrefix = _hostSettings.Register(CreateEntry("NamePrefix", "FakePlayer", "Fake Player Name Prefix", "Host setting, next lobby. Prefix before each simulated player's number. 1-32 characters, no control characters. Default: FakePlayer"), "FakePlayer", value => !string.IsNullOrWhiteSpace(value) && value.Length <= 32 && !System.Linq.Enumerable.Any(value, char.IsControl), legacy: false);
			_joinIntervalSeconds = _hostSettings.Register(CreateEntry("JoinIntervalSeconds", 0f, "Join Interval", "Host setting, live for subsequent joins. Delay between new fake players, 0-30 seconds. Zero joins at most one per frame. Default: 0"), 0f, value => value >= 0 && value <= 30, live: true, legacy: false);
			_requestTimeoutSeconds = _hostSettings.Register(CreateEntry("RequestTimeoutSeconds", 30f, "Request Timeout", "Host setting, next lobby. Time allowed for admission and room-transition responses, 1-300 seconds. Default: 30"), 30f, value => value >= 1 && value <= 300, legacy: false);
			_heartbeatIntervalSeconds = _hostSettings.Register(CreateEntry("HeartbeatIntervalSeconds", 10f, "Heartbeat Interval", "Host setting, next lobby. Heartbeat interval, 0.25-10 seconds. Default: 10"), 10f, value => value >= 0.25f && value <= 10, legacy: false);
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
		internal static string NamePrefix => _namePrefix?.Value ?? "FakePlayer";
		internal static float JoinIntervalSeconds => _joinIntervalSeconds?.Value ?? 0;
		internal static float RequestTimeoutSeconds => _requestTimeoutSeconds?.Value ?? 30;
		internal static float HeartbeatIntervalSeconds => _heartbeatIntervalSeconds?.Value ?? 10;
	}
}

