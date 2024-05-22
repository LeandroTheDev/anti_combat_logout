using Rocket.API;
namespace AntiCombatLogout
{
    public class AntiCombatLogouConfiguration : IRocketPluginConfiguration
    {
        public string PlayersFolder = "C:\\SteamLibrary\\steamapps\\common\\U3DS\\Servers\\myserver\\Players";
        public string LevelName = "PEI";
        public uint CombatTickrateDuration = 900;
        public uint ServerTickrate = 60;
        public int InventoryMaxPage = 10;
        public int InventoryMaxX = 50;
        public int InventoryMaxY = 50;
        public bool DebugExtended = false;
        public void LoadDefaults()
        {
        }
    }
}
