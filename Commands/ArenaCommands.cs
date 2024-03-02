using ProjectM;
using System.Linq;
using System.Text;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredArenas
{

    [CommandGroup("arena")]
    internal class ArenaCommands
    {
        [Command("on", description: "Turns on PvP arenas", adminOnly: true)]
        public static void PvPArenaOn(ChatCommandContext ctx)
        {
            Core.PvpArenaService.EnablePvpArenas();

            ctx.Reply("PvP arena turned on");
        }

        [Command("off", description: "Turns off PvP arenas", adminOnly: true)]
        public static void PvPArenaOff(ChatCommandContext ctx)
        {
            Core.PvpArenaService.DisablePvpArenas();

            ctx.Reply("PvP arena turned off");
        }

        [Command("create", "add", description: "Creates a PvP arena centered at your current location", adminOnly: true)]
        public static void CreatePvPArena(ChatCommandContext ctx, string name, float radius)
        {
            var pos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value.xz;
            if(Core.PvpArenaService.CreatePvpArena(name, pos, radius))
            {
                ctx.Reply($"PvP arena '{name}' created at ({pos.x}, {pos.y}) with a radius of {radius}");
            }
            else
            {
                ctx.Reply($"PvP arena '{name}' already exists");
            }
        }

        [Command("remove", "delete", description: "Removes a PvP arena", adminOnly: true)]
        public static void RemovePvPArena(ChatCommandContext ctx, string name)
        {
            if(Core.PvpArenaService.RemovePvpArena(name))
            {
                ctx.Reply($"PvP arena '{name}' removed");
            }
            else
            {
                ctx.Reply($"PvP arena '{name}' not found");
            }
        }

        [Command("list", description: "Lists all PvP arenas", adminOnly: true)]
        public static void ListPvPArenas(ChatCommandContext ctx)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PvP Arenas");
            int count = 0;
            foreach (var arena in Core.PvpArenaService.GetPvpArenas())
            {
                sb.AppendLine($"{arena.Name} - Location: ({arena.Location.x}, {arena.Location.y}) Radius: {arena.Radius} Enabled: {arena.Enabled}");
                count++;
                if (count % 7 == 0)
                {
                    ctx.Reply(sb.ToString());
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
            {
                ctx.Reply(sb.ToString());
            }
        }

        [Command("center", description: "Changes the center of a PvP arena", adminOnly: true)]
        public static void ChangePvPArenaCenter(ChatCommandContext ctx, string name)
        {
            var pos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value.xz;
            if (Core.PvpArenaService.ChangePvpArenaCenter(name, pos))
            {
                ctx.Reply($"PvP arena '{name}' center changed to ({pos.x}, {pos.y})");
            }
            else
            {
                ctx.Reply($"PvP arena '{name}' not found");
            }
        }

        [Command("radius", description: "Changes the radius of a PvP arena", adminOnly: true)]
        public static void ChangePvPArenaRadius(ChatCommandContext ctx, string name, float? radius=null)
        {
            if(radius!=null && Core.PvpArenaService.ChangePvpArenaRadius(name, radius.Value))
            {
                ctx.Reply($"PvP arena '{name}' radius changed to {radius}");
            }
            else if(Core.PvpArenaService.ChangePvpArenaRadius(name, ctx.Event.SenderCharacterEntity.Read<Translation>().Value.xz, out var newRadius))
            {
                ctx.Reply($"PvP arena '{name}' radius changed to {newRadius}");
            }
            else
            {
                ctx.Reply($"PvP arena '{name}' not found");
            }
        }

        [Command("enable", description: "Enables a PvP arena", adminOnly: true)]
        public static void EnablePvPArena(ChatCommandContext ctx, string name)
        {
            if(Core.PvpArenaService.EnablePvpArena(name))
            {
                ctx.Reply($"PvP arena '{name}' enabled");
            }
            else
            {
                ctx.Reply($"PvP arena '{name}' not found");
            }
        }

        [Command("disable", description: "Disables a PvP arena", adminOnly: true)]
        public static void DisablePvPArena(ChatCommandContext ctx, string name)
        {
            if(Core.PvpArenaService.DisablePvpArena(name))
            {
                ctx.Reply($"PvP arena '{name}' disabled");
            }
            else
            {
                ctx.Reply($"PvP arena '{name}' not found");
            }
        }       

        [Command("teleport", "tp", description: "Teleports you to the named PvP arena", adminOnly: true)]
        public static void TeleportToPvPArena(ChatCommandContext ctx, string name)
        {
            var nameLower = name.ToLowerInvariant();
            if(!Core.PvpArenaService.GetPvpArenas().Any(x => x.Name.ToLowerInvariant() == nameLower))
            {
                ctx.Reply($"No PvP arena found matching '{name}'");
                return;
            }

            var pvpLocation = Core.PvpArenaService.GetPvpArenas().First(x => x.Name.ToLowerInvariant() == nameLower).Location;
            var translation = Core.EntityManager.GetComponentData<Translation>(ctx.Event.SenderCharacterEntity);
            Core.EntityManager.SetComponentData(ctx.Event.SenderCharacterEntity, new LastTranslation { Value = new(pvpLocation.x, 0, pvpLocation.y) });
            Core.EntityManager.SetComponentData(ctx.Event.SenderCharacterEntity, new Translation { Value = new(pvpLocation.x, 0, pvpLocation.y) });
            ctx.Reply("Teleported to PvP arena");
        }


    }
}
