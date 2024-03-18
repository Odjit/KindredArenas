using KindredArenas.Data;
using ProjectM;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using ProjectM.Network;

namespace KindredArenas.Services
{
    internal class PvpService
    {
        readonly List<Entity> users = [];

        public PvpService()
        {
            foreach (var userEntity in Helper.GetEntitiesByComponentType<User>())
            {
                AddUser(userEntity);
            }
        }

        public void AddUser(Entity userEntity)
        {
            if (!users.Contains(userEntity))
            {
                users.Add(userEntity);
            }
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
