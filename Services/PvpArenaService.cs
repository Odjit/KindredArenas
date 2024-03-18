
using KindredArenas;
using KindredArenas.Data;
using ProjectM;
using ProjectM.Network;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;

namespace KindredArenas.Services
{
    class PvpArenaService : BaseZoneService
    {
        protected override string ZONES_PATH => Path.Combine(CONFIG_PATH, "arenas.json");
        protected override string ZONE_SERVICE_NAME => "PvpArenaService";

        public bool PvpArenasOn { get; private set; }

        public PvpArenaService() : base()
        {
            PvpArenasOn = true;
        }

        protected override List<Zone> GetDefaultZones()
        {
            return [
                new Zone { Name = "Colliseum", Location = new float2(-1002.2413f, -514.1175f), Radius = 18f, Enabled = true }
            ];
        }

        public void EnablePvpArenas()
        {
            PvpArenasOn = true;
            Core.Log.LogInfo("PvP arenas enabled");
        }

        public void DisablePvpArenas()
        {
            PvpArenasOn = false;
            Core.Log.LogInfo("PvP arenas disabled");
        }
    }
}