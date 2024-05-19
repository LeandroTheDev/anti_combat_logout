using Rocket.API;
namespace AntiCombatLogout
{
    public class AntiCombatLogouConfiguration : IRocketPluginConfiguration
    {
        public string PlayersFolder = "C:\\SteamLibrary\\steamapps\\common\\U3DS\\Servers\\myserver\\Players";
        public string LevelName = "PEI";
        public int CombatSecondsDuration = 15;
        public int InventoryMaxPage = 50;
        public int InventoryMaxX = 50;
        public int InventoryMaxY = 50;
        public void LoadDefaults()
        {
        }
    }
}
