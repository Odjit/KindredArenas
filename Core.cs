using System.Runtime.CompilerServices;
using BepInEx.Logging;
using ProjectM;
using Unity.Entities;

namespace KindredArenas;

internal static class Core
{
	public static World Server { get; } = GetWorld("Server") ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");

	public static EntityManager EntityManager { get; } = Server.EntityManager;
	public static ServerGameSettingsSystem ServerGameSettingsSystem { get; private set; }

    public static ManualLogSource Log { get; } = Plugin.PluginLog;

    public static Services.PvpService PvpService { get; internal set; }
    public static Services.PvpArenaService PvpArenaService { get; internal set; }
	public static Services.ElysiumService ElysiumService { get; internal set; }

	public static void LogException(System.Exception e, [CallerMemberName] string caller = null)
	{
		Core.Log.LogError($"Failure in {caller}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
	}


	internal static void InitializeAfterLoaded()
	{
		if (_hasInitialized) return;

		ServerGameSettingsSystem = Server.GetExistingSystem<ServerGameSettingsSystem>();
		PvpService = new();
        PvpArenaService = new();
		ElysiumService = new();
		_hasInitialized = true;
		Log.LogInfo($"{nameof(InitializeAfterLoaded)} completed");
	}
	private static bool _hasInitialized = false;
	public static bool HasInitialized => _hasInitialized;

	private static World GetWorld(string name)
	{
		foreach (var world in World.s_AllWorlds)
		{
			if (world.Name == name)
			{
				return world;
			}
		}

		return null;
	}
}
