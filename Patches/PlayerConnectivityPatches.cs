using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;

namespace KindredArenas.Patches;


[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class OnUserConnected_Patch
{
	public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
	{
		try
		{
			var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			var serverClient = __instance._ApprovedUsersLookup[userIndex];
			var userEntity = serverClient.UserEntity;
			var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
			bool isNewVampire = userData.CharacterName.IsEmpty;

			if (isNewVampire)
			{
				if(!PvpUpdater.Initialized)
				{
                    Core.InitializeAfterLoaded();
                    PvpUpdater.Initialized = true;
                }

				Core.PvpService.AddUser(userEntity);
			}
		}
		catch (Exception e)
		{
			Core.Log.LogError($"Failure in {nameof(ServerBootstrapSystem.OnUserConnected)}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
		}
	}
}


[HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
public static class PvpUpdater
{
	public static bool Initialized = false;
    [HarmonyPostfix]
    public static void Update()
    {
		if(Initialized)
			Core.PvpService.Update();
    }
}

[HarmonyPatch(typeof(SpawnTeamSystem_OnPersistenceLoad), nameof(SpawnTeamSystem_OnPersistenceLoad.OnUpdate))]
public static class InitializationPatch
{
    [HarmonyPostfix]
    public static void OneShot_AfterLoad_InitializationPatch()
    {
		Core.InitializeAfterLoaded();
        PvpUpdater.Initialized = true;
        Plugin.Harmony.Unpatch(typeof(SpawnTeamSystem_OnPersistenceLoad).GetMethod("OnUpdate"), typeof(InitializationPatch).GetMethod("OneShot_AfterLoad_InitializationPatch"));
    }
}
