using System.IO;

namespace KindredArenas.Services
{
    internal class ElysiumService : BaseZoneService
    {
        protected override string ZONES_PATH => Path.Combine(CONFIG_PATH, "elysium.json");
        protected override string ZONE_SERVICE_NAME => "ElysiumService";

        public bool PvpElysiumOn { get; private set; }

        public ElysiumService() : base()
        {
            PvpElysiumOn = true;
        }

        public void EnableElysiums()
        {
            PvpElysiumOn = true;
            Core.Log.LogInfo("Elysiums enabled");
        }

        public void DisableElysiums()
        {
            PvpElysiumOn = false;
            Core.Log.LogInfo("Elysiums disabled");
        }
    }
}
