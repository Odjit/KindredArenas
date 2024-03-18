using KindredArenas.Data;
using ProjectM;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using ProjectM.Network;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace KindredArenas.Services
{
    public enum DaysOfTheWeek
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }

    internal class PvpService
    {
        protected static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
        protected static readonly string PVP_TIMES_PATH = Path.Combine(CONFIG_PATH, "pvpTimes.json");

        readonly List<Entity> users = [];

        readonly List<PvpTime> pvpTimes = [];

        public struct PvpTime
        {
            public DaysOfTheWeek DaysOfTheWeek { get; set; }
            public int StartHour { get; set; }
            public int StartMinute { get; set; }
            public int EndHour { get; set; }
            public int EndMinute { get; set; }
        }

        public PvpService()
        {
            foreach (var userEntity in Helper.GetEntitiesByComponentType<User>())
            {
                AddUser(userEntity);
            }

            LoadPvpTimes();
        }

        public IEnumerable<PvpTime> GetPvpTimes()
        {
            return pvpTimes;
        }

        void LoadPvpTimes()
        {
            if (File.Exists(PVP_TIMES_PATH))
            {
                var json = File.ReadAllText(PVP_TIMES_PATH);
                pvpTimes.Clear();
                pvpTimes.AddRange(JsonSerializer.Deserialize<PvpTime[]>(json, new JsonSerializerOptions { Converters = { new DaysOfTheWeekConverter() } }));
            }
            else
            {
                SavePvpTimes();
            }
        }

        void SavePvpTimes()
        {
            if (!Directory.Exists(CONFIG_PATH)) Directory.CreateDirectory(CONFIG_PATH);

            var options = new JsonSerializerOptions
            {
                Converters = { new DaysOfTheWeekConverter() },
                WriteIndented = true,
            };

            var json = JsonSerializer.Serialize(pvpTimes, options);
            File.WriteAllText(PVP_TIMES_PATH, json);
        }

        public void AddPvpTime(PvpTime pvpTime)
        {
            pvpTimes.Add(pvpTime);
            SavePvpTimes();
        }

        public bool RemovePvpTime(int pvpTimeIndex)
        {
            if (pvpTimeIndex < 0 || pvpTimeIndex >= pvpTimes.Count)
            {
                return false;
            }
            pvpTimes.RemoveAt(pvpTimeIndex);
            SavePvpTimes();
            return true;
        }

        public void AddUser(Entity userEntity)
        {
            if (!users.Contains(userEntity))
            {
                users.Add(userEntity);
            }
        }

        public bool IsPvpActive()
        {
            // Get the current day of the week and time and check if it falls within any of the pvp times
            var now = DateTime.Now;
            var day = (DaysOfTheWeek)(1 << (int)now.DayOfWeek);
            foreach(var pvpTime in pvpTimes)
            {
                if (pvpTime.DaysOfTheWeek.HasFlag(day))
                {
                    var start = new DateTime(now.Year, now.Month, now.Day, pvpTime.StartHour, pvpTime.StartMinute, 0);
                    var end = new DateTime(now.Year, now.Month, now.Day, pvpTime.EndHour, pvpTime.EndMinute, 0);

                    if(end < start)
                    {
                        end = end.AddDays(1);
                    }

                    if (now >= start && now <= end)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Update()
        {
            UpdatePvpStatuses();
        }

        void UpdatePvpStatuses()
        {
            foreach (var userEntity in users)
            {
                var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();
                if (charEntity == Entity.Null) continue;

                if (IsPvpActive())
                {
                    var playerCharacter = charEntity.Read<PlayerCharacter>();
                    var playerPos = charEntity.Read<Translation>().Value;
                    if(Core.ElysiumService.PvpElysiumOn && Core.ElysiumService.IsInZone(playerPos.xz))
                    {
                        Buffs.AddBuff(playerCharacter.UserEntity, charEntity, Prefabs.Buff_General_PvPProtected, true);
                    }
                    else if (Buffs.GetBuffDuration(charEntity, Prefabs.Buff_General_PvPProtected) < 0)
                    {
                        Buffs.RemoveBuff(charEntity, Prefabs.Buff_General_PvPProtected);
                    }
                }
                else
                {
                    if (Core.PvpArenaService.PvpArenasOn)
                    {
                        var playerCharacter = charEntity.Read<PlayerCharacter>();
                        var playerPos = charEntity.Read<Translation>().Value;
                        if (Core.PvpArenaService.IsInZone(playerPos.xz))
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
    }

    internal class DaysOfTheWeekConverter : JsonConverter<DaysOfTheWeek>
    {
        public override DaysOfTheWeek Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }

            reader.Read();

            DaysOfTheWeek value = 0;

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException();
                }

                value |= reader.GetString() switch
                {
                    "Sunday" => DaysOfTheWeek.Sunday,
                    "Monday" => DaysOfTheWeek.Monday,
                    "Tuesday" => DaysOfTheWeek.Tuesday,
                    "Wednesday" => DaysOfTheWeek.Wednesday,
                    "Thursday" => DaysOfTheWeek.Thursday,
                    "Friday" => DaysOfTheWeek.Friday,
                    "Saturday" => DaysOfTheWeek.Saturday,
                    _ => throw new JsonException(),
                };

                reader.Read();
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, DaysOfTheWeek value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            if (value.HasFlag(DaysOfTheWeek.Sunday))
                writer.WriteStringValue("Sunday");
            if (value.HasFlag(DaysOfTheWeek.Monday))
                writer.WriteStringValue("Monday");
            if (value.HasFlag(DaysOfTheWeek.Tuesday))
                writer.WriteStringValue("Tuesday");
            if (value.HasFlag(DaysOfTheWeek.Wednesday))
                writer.WriteStringValue("Wednesday");
            if (value.HasFlag(DaysOfTheWeek.Thursday))
                writer.WriteStringValue("Thursday");
            if (value.HasFlag(DaysOfTheWeek.Friday))
                writer.WriteStringValue("Friday");
            if (value.HasFlag(DaysOfTheWeek.Saturday))
                writer.WriteStringValue("Saturday");

            writer.WriteEndArray();
        }
    }

}
