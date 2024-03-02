using Unity.Collections;
using Unity.Entities;
using Il2CppInterop.Runtime;
namespace KindredArenas
{
    internal class Helper
    {
        public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
        {
            EntityQueryOptions options = EntityQueryOptions.Default;
            if (includeAll) options |= EntityQueryOptions.IncludeAll;
            if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
            if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
            if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
            if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

            EntityQueryDesc queryDesc = new()
            {
                All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite), new(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite) },
                Options = options
            };

            var query = Core.EntityManager.CreateEntityQuery(queryDesc);

            var entities = query.ToEntityArray(Allocator.Temp);
            return entities;
        }

        public static NativeArray<Entity> GetEntitiesByComponentType<T1>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
        {
            EntityQueryOptions options = EntityQueryOptions.Default;
            if (includeAll) options |= EntityQueryOptions.IncludeAll;
            if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
            if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
            if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
            if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

            EntityQueryDesc queryDesc = new()
            {
                All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
                Options = options
            };

            var query = Core.EntityManager.CreateEntityQuery(queryDesc);

            var entities = query.ToEntityArray(Allocator.Temp);
            return entities;
        }
    }
}
