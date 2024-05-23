using System;
using System.Linq;
using ProjectM.Terrain;
using VampireCommandFramework;

namespace KindredArenass.Commands.Converters;


public record struct FoundRegion(WorldRegionType Value, string Name);
public class FoundRegionConverter : CommandArgumentConverter<FoundRegion>
{
	public override FoundRegion Parse(ICommandContext ctx, string input)
	{
		var region = Enum.GetValues(typeof(WorldRegionType)).Cast<WorldRegionType>().FirstOrDefault(x => x.ToString().Equals(input, StringComparison.OrdinalIgnoreCase));

		if (region != WorldRegionType.None)
			return new FoundRegion(region, region.ToString());

		input = input.ToLowerInvariant();

		switch(input)
		{
			case "start":
				return new FoundRegion(WorldRegionType.StartCave, "StartCave");
			case "farbane":
			case "woods":
				return new FoundRegion(WorldRegionType.FarbaneWoods, "FarbaneWoods");
			case "dunley":
			case "farmlands":
				return new FoundRegion(WorldRegionType.DunleyFarmlands, "DunleyFarmlands");
			case "cursed":
			case "forest":
				return new FoundRegion(WorldRegionType.CursedForest, "CursedForest");
			case "hallowed":
			case "mountain":
			case "mountains":
				return new FoundRegion(WorldRegionType.HallowedMountains, "HallowedMountain");
			case "silverlight":
			case "hills":
				return new FoundRegion(WorldRegionType.SilverlightHills, "SilverlightHills");
			case "gloomrotsouth":
			case "gloomrot south":
			case "south":
				return new FoundRegion(WorldRegionType.Gloomrot_South, "Gloomrot_South");
			case "gloomrotnorth":
			case "gloomrot north":
			case "north":
				return new FoundRegion(WorldRegionType.Gloomrot_North, "Gloomrot_North");
			case "mortium":
			case "ruins":
			case "ruinsofmortium":
				return new FoundRegion(WorldRegionType.RuinsOfMortium, "RuinsOfMortium");
			case "none":
				return new FoundRegion(WorldRegionType.None, "None");
		}

		throw ctx.Error("Could not find region");
	}
}
