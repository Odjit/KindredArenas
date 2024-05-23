using KindredArenas.Services;
using KindredArenass.Commands.Converters;
using System.Text;
using VampireCommandFramework;

namespace KindredArenas.Commands
{
    internal class PvpRegionCommands
    {
        [Command("listpvpregions", description: "Lists all regions which are always PvP/PvE")]
        public static void ListPvpRegions(ChatCommandContext ctx)
        {
            var sb = new StringBuilder();
            sb.AppendLine("PvP/PvE Regions");
            foreach (var (region, state) in Core.PvpRegionsService.GetRegionStates())
            {
                var alwaysPvp = state == PvpRegionsService.PvpRegionType.AlwaysPvP;
                sb.AppendLine($"<color=yellow>{region}</color> - {(alwaysPvp ? "<color=red>PvP Always</color>" : "<color=green>PvE Always</color>")}");
            }
            ctx.Reply(sb.ToString());
        }

        [Command("addpvpregion", description: "Sets a region to always have <color=red>PvP</color>", adminOnly: true)]
        public static void AddPvpRegion(ChatCommandContext ctx, FoundRegion region)
        {
            Core.PvpRegionsService.SetRegionPvpState(region.Value, PvpRegionsService.PvpRegionType.AlwaysPvP);
            ctx.Reply($"Region <color=yellow>{region.Name}</color> set to always be <color=red>PvP</color>");
        }

        [Command("addpveregion", description: "Sets a region to always have <color=green>PvE</color>", adminOnly: true)]
        public static void AddPveRegion(ChatCommandContext ctx, FoundRegion region)
        {
            Core.PvpRegionsService.SetRegionPvpState(region.Value, PvpRegionsService.PvpRegionType.AlwaysPvE);
            ctx.Reply($"Region <color=yellow>{region.Name}</color> set to always be <color=green>PvE</color>");
        }

        [Command("addneutralregion", description: "Sets a region to be based on the current PvP state", adminOnly: true)]
        public static void AddNeutralRegion(ChatCommandContext ctx, FoundRegion region)
        {
            Core.PvpRegionsService.SetRegionPvpState(region.Value, PvpRegionsService.PvpRegionType.BasedOnCurrentPvPState);
            ctx.Reply($"Region <color=yellow>{region.Name}</color> set to be based on the current PvP status");
        }
    }
}
