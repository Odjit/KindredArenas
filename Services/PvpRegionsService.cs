using ProjectM;
using ProjectM.Network;
using ProjectM.Terrain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unity.Entities;

namespace KindredArenas.Services;

internal class PvpRegionsService
{
    protected static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
    protected static readonly string REGIONS_PATH = Path.Combine(CONFIG_PATH, "pvpRegions.json");

    public enum PvpRegionType
    {
        BasedOnCurrentPvPState,
        AlwaysPvP,
        AlwaysPvE,
    }

    Dictionary<WorldRegionType, PvpRegionType> regionPvpSetting = [];

    public PvpRegionsService()
    {
        LoadPvpRegions();
    }

    void LoadPvpRegions()
    {
        if (File.Exists(REGIONS_PATH))
        {
            var json = File.ReadAllText(REGIONS_PATH);
            regionPvpSetting = JsonSerializer.Deserialize<Dictionary<WorldRegionType, PvpRegionType>>(json, new JsonSerializerOptions { Converters = { new PvpRegionTypeConverter() } });
        }
        else
        {
            SavePvpRegions();
        }
    }

    void SavePvpRegions()
    {
        var json = JsonSerializer.Serialize(regionPvpSetting, new JsonSerializerOptions { WriteIndented = true, Converters = { new PvpRegionTypeConverter() } });
        File.WriteAllText(REGIONS_PATH, json);
    }

    public IEnumerable<(WorldRegionType region, PvpRegionType state)> GetRegionStates()
    {
        foreach(var item in regionPvpSetting)
        {
            yield return (item.Key, item.Value);
        }
    }

    public PvpRegionType GetPlayerRegionPvpState(Entity entity)
    {
        var userEntity = entity.Read<PlayerCharacter>().UserEntity;
        var region = userEntity.Read<CurrentWorldRegion>().CurrentRegion;
        if (!regionPvpSetting.TryGetValue(region, out var pvpSetting))
            pvpSetting = PvpRegionType.BasedOnCurrentPvPState;
        return pvpSetting;
    }

    public void SetRegionPvpState(WorldRegionType region, PvpRegionType pvpSetting)
    {
        if(pvpSetting == PvpRegionType.BasedOnCurrentPvPState)
            regionPvpSetting.Remove(region);
        else
            regionPvpSetting[region] = pvpSetting;
        SavePvpRegions();
    }

    public class PvpRegionTypeConverter : JsonConverter<PvpRegionType>
    {
        public override PvpRegionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected String Type not {reader.TokenType} for PvpRegionType");
            }
            var value = reader.GetString();

            if (!Enum.TryParse<PvpRegionType>(value, out var result))
            {
                throw new JsonException($"Invalid PvpRegionType: {value}");
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, PvpRegionType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
