
using KindredArenas;
using KindredArenas.Data;
using ProjectM;
using ProjectM.Network;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

class PvpArenaService
{
    private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    private static readonly string ZONES_PATH = Path.Combine(CONFIG_PATH, "arenas.json");

    readonly List<Entity> users = [];
    readonly List<PvpArena> arenas = [
        new PvpArena { Name = "Colliseum", Location = new float2(-1002.2413f, -514.1175f), Radius = 18f, Enabled = true }
        ];

    public struct PvpArena
    {
        public string Name { get; set; }
        public float2 Location { get; set; }
        public float Radius { get; set; }
        public bool Enabled { get; set; }
    }

    bool pvpArenasOn = true;

    public PvpArenaService()
    {
        foreach (var userEntity in Helper.GetEntitiesByComponentType<User>())
        {
            AddUser(userEntity);
        }

        LoadPvpArenas();
    }

    public void AddUser(Entity userEntity)
    {
        if(!users.Contains(userEntity))
        {
            users.Add(userEntity);
        }
    }

    public IEnumerable<PvpArena> GetPvpArenas()
    {
        return arenas;
    }

    void LoadPvpArenas()
    {
        if (File.Exists(ZONES_PATH))
        {
            var json = File.ReadAllText(ZONES_PATH);
            arenas.Clear();
            arenas.AddRange(JsonSerializer.Deserialize<PvpArena[]>(json, new JsonSerializerOptions { Converters = { new Float2Converter() }}));
        }
        else
        {
            SavePvpArenas();
        }
    }

    void SavePvpArenas()
    {
        if (!Directory.Exists(CONFIG_PATH)) Directory.CreateDirectory(CONFIG_PATH);

        var options = new JsonSerializerOptions
        {
            Converters = { new Float2Converter() },
            WriteIndented = true,
        };

        var json = JsonSerializer.Serialize(arenas, options);
        File.WriteAllText(ZONES_PATH, json);
    }

    bool RetrieveArena(string name, out PvpArena arena, out int arenaIndex)
    {
        name = name.ToLowerInvariant();
        arena = arenas.Find(z => z.Name.ToLowerInvariant() == name);
        arenaIndex = arenas.IndexOf(arena);
        return arenaIndex != -1;
    }

    public bool CreatePvpArena(string name, float2 location, float radius)
    {
        if (RetrieveArena(name, out var arena, out var arenaIndex)) return false;

        arena = new PvpArena
        {
            Name = name,
            Location = location.xy,
            Radius = radius,
            Enabled = true
        };
        arenas.Add(arena);
        SavePvpArenas();
        Core.Log.LogInfo($"PvP arena '{name}' created at {location} with a radius of {radius}");
        return true;
    }

    public bool RemovePvpArena(string name)
    {
        var nameLower = name.ToLowerInvariant();
        int numRemoved = arenas.RemoveAll(z => z.Name.ToLowerInvariant() == nameLower);
        if(numRemoved > 0)
        {
            Core.Log.LogInfo($"PvP arena '{name}' removed");
        }
        SavePvpArenas();
        return numRemoved > 0;
    }

    public bool ChangePvpArenaCenter(string name, float2 location)
    {
        if (!RetrieveArena(name, out var arena, out var arenaIndex)) return false;

        arena.Location = location;
        arenas[arenaIndex] = arena;
        SavePvpArenas();
        Core.Log.LogInfo($"PvP arena '{name}' center changed to {location}");
        return true;
    }

    public bool ChangePvpArenaRadius(string name, float radius)
    {
        if (!RetrieveArena(name, out var arena, out var arenaIndex)) return false;

        arena.Radius = radius;
        arenas[arenaIndex] = arena;
        SavePvpArenas();
        Core.Log.LogInfo($"PvP arena '{name}' radius changed to {radius}");
        return true;
    }

    public bool ChangePvpArenaRadius(string name, float2 location, out float newRadius)
    {
        if (!RetrieveArena(name, out var arena, out var arenaIndex))
        {
            newRadius = 0;
            return false;
        }

        arena.Radius = Vector2.Distance(location, arena.Location);
        arenas[arenaIndex] = arena;
        SavePvpArenas();
        newRadius = arena.Radius;
        Core.Log.LogInfo($"PvP arena '{name}' radius changed to {newRadius}");
        return true;
    }

    public bool EnablePvpArena(string name)
    {
        if (!RetrieveArena(name, out var arena, out var arenaIndex)) return false;

        arena.Enabled = true;
        arenas[arenaIndex] = arena;
        SavePvpArenas();
        Core.Log.LogInfo($"PvP arena '{name}' enabled");
        return true;
    }

    public bool DisablePvpArena(string name)
    {
        if (!RetrieveArena(name, out var arena, out var arenaIndex)) return false;

        arena.Enabled = false;
        arenas[arenaIndex] = arena;
        SavePvpArenas();
        Core.Log.LogInfo($"PvP arena '{name}' disabled");
        return true;
    }

    public void EnablePvpArenas()
    {
        pvpArenasOn = true;
        UpdatePvpStatues();
        SavePvpArenas();
        Core.Log.LogInfo("PvP arenas enabled");
    }

    public void DisablePvpArenas()
    {
        pvpArenasOn = false;
        UpdatePvpStatues();
        SavePvpArenas();
        Core.Log.LogInfo("PvP arenas disabled");
    }

    bool IsInPvpArena(float2 pos)
    {
        foreach(var arena in arenas)
        {
            if (arena.Enabled && Vector2.Distance(arena.Location, pos) < arena.Radius)
            {
                return true;
            }
        }
        return false;
    }

    public void UpdatePvpStatues()
    {
        foreach (var userEntity in users)
        {
            var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();
            if (charEntity == Entity.Null) continue;

            if (pvpArenasOn)
            {
                var playerCharacter = charEntity.Read<PlayerCharacter>();
                var playerPos = charEntity.Read<Translation>().Value;
                if (IsInPvpArena(playerPos.xz))
                {
                    Buffs.RemoveBuff(charEntity, Prefabs.Buff_General_PvPProtected);
                }
                else if (!Buffs.HasBuff(charEntity, Prefabs.Buff_General_PvPProtected))
                {
                    Buffs.AddBuff(playerCharacter.UserEntity, charEntity, Prefabs.Buff_General_PvPProtected, true);
                }
            }
            else
            {
                if (!Buffs.HasBuff(charEntity, Prefabs.Buff_General_PvPProtected))
                {
                    var playerCharacter = charEntity.Read<PlayerCharacter>();
                    Buffs.AddBuff(playerCharacter.UserEntity, charEntity, Prefabs.Buff_General_PvPProtected, true);
                }
            }
        }
    }
}