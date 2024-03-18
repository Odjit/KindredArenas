using ProjectM;
using System.Linq;
using System.Text;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredArenas.Commands
{
    [CommandGroup("elysium")]
    internal class ElysiumCommands
    {

        [Command("on", description: "Turns on Elysiums", adminOnly: true)]
        public static void ElysiumsOn(ChatCommandContext ctx)
        {
            Core.ElysiumService.EnableElysiums();

            ctx.Reply("Elysisums turned on");
        }

        [Command("off", description: "Turns off Elysiums", adminOnly: true)]
        public static void PvPArenaOff(ChatCommandContext ctx)
        {
            Core.ElysiumService.DisableElysiums();

            ctx.Reply("Elysiums turned off");
        }

        [Command("create", "add", description: "Creates an elysium centered at your current location", adminOnly: true)]
        public static void CreateElysium(ChatCommandContext ctx, string name, float radius)
        {
            var pos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value.xz;
            if (Core.ElysiumService.CreateZone(name, pos, radius))
            {
                ctx.Reply($"Elysium '{name}' created at ({pos.x}, {pos.y}) with a radius of {radius}");
            }
            else
            {
                ctx.Reply($"Elysium '{name}' already exists");
            }
        }

        [Command("remove", "delete", description: "Removes an Elsyium", adminOnly: true)]
        public static void RemoveElsyium(ChatCommandContext ctx, string name)
        {
            if (Core.ElysiumService.RemoveZone(name))
            {
                ctx.Reply($"Elysium '{name}' removed");
            }
            else
            {
                ctx.Reply($"Elysium '{name}' not found");
            }
        }

        [Command("list", description: "Lists all Elysiums", adminOnly: true)]
        public static void ListElsyiums(ChatCommandContext ctx)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Elsyiums");
            int count = 0;
            foreach (var elsyium in Core.ElysiumService.GetZones())
            {
                sb.AppendLine($"{elsyium.Name} - Location: ({elsyium.Location.x}, {elsyium.Location.y}) Radius: {elsyium.Radius} Enabled: {elsyium.Enabled}");
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

        [Command("center", description: "Changes the center of an Elsyium", adminOnly: true)]
        public static void ChangeElysiumCenter(ChatCommandContext ctx, string name)
        {
            var pos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value.xz;
            if (Core.ElysiumService.ChangeZoneCenter(name, pos))
            {
                ctx.Reply($"Elysium '{name}' center changed to ({pos.x}, {pos.y})");
            }
            else
            {
                ctx.Reply($"Elysium '{name}' not found");
            }
        }

        [Command("radius", description: "Changes the radius of a PvP arena", adminOnly: true)]
        public static void ChangeElsyiumRadius(ChatCommandContext ctx, string name, float? radius = null)
        {
            if (radius != null && Core.ElysiumService.ChangeZoneRadius(name, radius.Value))
            {
                ctx.Reply($"Elysium '{name}' radius changed to {radius}");
            }
            else if (Core.ElysiumService.ChangeZoneRadius(name, ctx.Event.SenderCharacterEntity.Read<Translation>().Value.xz, out var newRadius))
            {
                ctx.Reply($"Elysium '{name}' radius changed to {newRadius}");
            }
            else
            {
                ctx.Reply($"Elysium '{name}' not found");
            }
        }

        [Command("enable", description: "Enables an Elsyium", adminOnly: true)]
        public static void EnablePvPArena(ChatCommandContext ctx, string name)
        {
            if (Core.ElysiumService.EnableZone(name))
            {
                ctx.Reply($"Elysium '{name}' enabled");
            }
            else
            {
                ctx.Reply($"Elysium '{name}' not found");
            }
        }

        [Command("disable", description: "Disables an Elsyium", adminOnly: true)]
        public static void DisablePvPArena(ChatCommandContext ctx, string name)
        {
            if (Core.ElysiumService.DisableZone(name))
            {
                ctx.Reply($"Elysium '{name}' disabled");
            }
            else
            {
                ctx.Reply($"Elysium '{name}' not found");
            }
        }

        [Command("teleport", "tp", description: "Teleports you to the named Elysium", adminOnly: true)]
        public static void TeleportToElysium(ChatCommandContext ctx, string name)
        {
            var nameLower = name.ToLowerInvariant();
            if (!Core.ElysiumService.GetZones().Any(x => x.Name.ToLowerInvariant() == nameLower))
            {
                ctx.Reply($"No elysium found matching '{name}'");
                return;
            }

            var pvpLocation = Core.ElysiumService.GetZones().First(x => x.Name.ToLowerInvariant() == nameLower).Location;
            var translation = Core.EntityManager.GetComponentData<Translation>(ctx.Event.SenderCharacterEntity);
            Core.EntityManager.SetComponentData(ctx.Event.SenderCharacterEntity, new LastTranslation { Value = new(pvpLocation.x, 0, pvpLocation.y) });
            Core.EntityManager.SetComponentData(ctx.Event.SenderCharacterEntity, new Translation { Value = new(pvpLocation.x, 0, pvpLocation.y) });
            ctx.Reply("Teleported to Elysium");
        }
    }
}
