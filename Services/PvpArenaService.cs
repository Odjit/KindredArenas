
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

namespace KindredArenas.Services
{
    class PvpArenaService : BaseZoneService
    {
        protected override string ZONES_PATH => Path.Combine(CONFIG_PATH, "arenas.json");
        protected override string ZONE_SERVICE_NAME => "PvpArenaService";

        readonly List<Entity> users = [];

        bool pvpArenasOn = true;

        public PvpArenaService() : base()
        {
            
            foreach (var userEntity in Helper.GetEntitiesByComponentType<User>())
            {
                AddUser(userEntity);
            }
        }

        protected override List<Zone> GetDefaultZones()
        {
            return [
                new Zone { Name = "Colliseum", Location = new float2(-1002.2413f, -514.1175f), Radius = 18f, Enabled = true }
            ];
        }

        public void AddUser(Entity userEntity)
        {
            if (!users.Contains(userEntity))
            {
                users.Add(userEntity);
            }
        }

        public void EnablePvpArenas()
        {
            pvpArenasOn = true;
            UpdatePvpStatues();
            Core.Log.LogInfo("PvP arenas enabled");
        }

        public void DisablePvpArenas()
        {
            pvpArenasOn = false;
            UpdatePvpStatues();
            Core.Log.LogInfo("PvP arenas disabled");
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
                    if (IsInZone(playerPos.xz))
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