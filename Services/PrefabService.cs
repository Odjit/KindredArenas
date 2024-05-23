using System.Collections.Generic;
using ProjectM;
using Stunlock.Core;

namespace KindredArenas.Services;

internal class PrefabService
{
    readonly PrefabCollectionSystem collectionSystem;
	public PrefabCollectionSystem CollectionSystem => collectionSystem;

	internal Dictionary<string, (string Name, PrefabGUID Prefab)> AllNameToGuid { get; init; } = [];
	internal Dictionary<string, (string Name, PrefabGUID Prefab)> SpawnableNameToGuid { get; init; } = [];

	internal PrefabService()
	{
		collectionSystem = Core.Server.GetExistingSystemManaged<PrefabCollectionSystem>();

		var allPrefabs = collectionSystem.NameToPrefabGuidDictionary;
		Core.Log.LogDebug($"All prefabs: {allPrefabs.Count}");
		foreach (var kvp in allPrefabs)
		{
			bool success = AllNameToGuid.TryAdd(kvp.Key.ToLowerInvariant(), (kvp.Key, kvp.Value));
			if (!success)
			{
				Core.Log.LogDebug($"{kvp.Key} exist already, skipping.");
			}
		}

		var spawnable = collectionSystem.SpawnableNameToPrefabGuidDictionary;
		Core.Log.LogDebug($"Spawnable prefabs: {spawnable.Count}");
		foreach (var kvp in spawnable)
		{
			bool success = SpawnableNameToGuid.TryAdd(kvp.Key.ToLowerInvariant(), (kvp.Key, kvp.Value));
			if (!success)
			{
				Core.Log.LogDebug($"{kvp.Key} exist already, skipping.");
			}
		}
	}

	internal bool TryGetBuff(string input, out PrefabGUID prefab)
	{
		var lower = input.ToLowerInvariant();
		var output = AllNameToGuid.TryGetValue(lower, out var guidRec) ||
					 AllNameToGuid.TryGetValue($"buff_{lower}", out guidRec) ||
					 AllNameToGuid.TryGetValue($"equipbuff_{lower}", out guidRec);
		prefab = guidRec.Prefab;
		return output && guidRec.Name.ToLowerInvariant().Contains("buff");
	}

	internal bool TryGetItem(string input, out PrefabGUID prefab)
	{
		var lower = input.ToLowerInvariant();
		var output = SpawnableNameToGuid.TryGetValue(lower, out var guidRec) || SpawnableNameToGuid.TryGetValue($"item_{lower}", out guidRec);
		prefab = guidRec.Prefab;
		return output;
	}
}
