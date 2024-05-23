extern alias UnityEngineCoreModule;

using Rocket.API;
using Rocket.API.Collections;
using Rocket.API.Extensions;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using UnityCoreModule = UnityEngineCoreModule.UnityEngine;

namespace AntiCombatLogout
{
    public class AntiCombatLogoutPlugin : RocketPlugin<AntiCombatLogouConfiguration>
    {
        private CombatPlayer? combatPlayer;
        public override void LoadPlugin()
        {
            base.LoadPlugin();
            // Add a reference from combat player to unity thread
            combatPlayer = gameObject.AddComponent<CombatPlayer>();
            // Instanciate the plugin into the combatPlayer
            combatPlayer.InstanciatePlugin(this);

            // Connection event
            Rocket.Unturned.U.Events.OnPlayerConnected += combatPlayer.PlayerConnected;
            // Disconnection event
            Rocket.Unturned.U.Events.OnPlayerDisconnected += combatPlayer.PlayerDisconnect;

            Logger.Log("AntiCombatLogout by LeandroTheDev");
        }

        public override void UnloadPlugin(PluginState state = PluginState.Unloaded)
        {
            // Remove events
            if (combatPlayer != null)
            {
                Rocket.Unturned.U.Events.OnPlayerConnected -= combatPlayer.PlayerConnected;
                Rocket.Unturned.U.Events.OnPlayerDisconnected -= combatPlayer.PlayerDisconnect;
            }
            // Remove the component reference
            gameObject.TryRemoveComponent<CombatPlayer>();
            base.UnloadPlugin(state);
        }

        public override TranslationList DefaultTranslations => new()
        {
            { "Entering_Combat", "You are in combat, don't logout for {0} seconds" },
            { "No_Longer_Combat", "You are no longer in combat!" },
            { "Player_Exited_In_combat", "{0} exited in combat, what a shame" }
        };
    }
    class CombatPlayer : MonoBehaviour
    {
        /// <summary>
        /// Stores the main plugin from Rocket
        /// </summary>
        AntiCombatLogoutPlugin? plugin;
        /// <summary>
        /// All players online and previous life before getting updated
        /// </summary>
        private readonly Dictionary<string, byte> previousHealth = new();
        /// <summary>
        /// All players in combat will store the tickrate here
        /// </summary>
        private Dictionary<UnturnedPlayer, uint> combatTimer = new();

        #region debug
        private int tickrateDebug = 0;
        private int tickrateDebugPassed = 0;
        #endregion

        public void Update()
        {
            if (plugin == null) return;

            #region debug
            if (plugin.Configuration.Instance.DebugExtended)
            {
                tickrateDebug++;
                if (tickrateDebug == plugin.Configuration.Instance.ServerTickrate)
                {
                    tickrateDebug = 0;
                    Logger.Log($"[AntiCombatLogout] Seconds passed: {tickrateDebugPassed}");
                    tickrateDebugPassed++;
                }
            }
            #endregion

            if (combatTimer.Count == 0) return;
    
            List<UnturnedPlayer> playersToRemove = new();
            Dictionary<UnturnedPlayer, uint> combatTimersNew = new(combatTimer);

            // Swipe all active combat timers
            foreach (KeyValuePair<UnturnedPlayer, uint> keyValue in combatTimer)
            {
                // Reduce tickrate
                uint tickrate = keyValue.Value - 1;

                if (tickrate <= 0)
                {
                    // Inform player
                    UnturnedChat.Say(keyValue.Key, plugin.Translate("No_Longer_Combat"), Palette.COLOR_G);
                    // Globally remove from combat
                    AntiCombatLogoutTools.PlayerExitingFromCombat(keyValue.Key.Id);
                    // Add to remove list
                    playersToRemove.Add(keyValue.Key);
                }
                // Update tickrate
                else combatTimersNew[keyValue.Key] = tickrate;
            }

            // Remove players from combat timer
            foreach (UnturnedPlayer player in playersToRemove) combatTimersNew.Remove(player);

            // Actually update the combat timer after all
            combatTimer = combatTimersNew;
        }

        public void InstanciatePlugin(AntiCombatLogoutPlugin _plugin) => plugin = _plugin;
        public void PlayerConnected(UnturnedPlayer player)
        {
            // Add the combat players variable
            AntiCombatLogoutTools.CombatPlayersId.Add(player.Id, false);
            // Add the player to the previous health memory
            previousHealth.Add(player.Id, player.Health);
            // Instanciate the update event
            player.Events.OnUpdateHealth += PlayerHealthUpdated;
        }
        public void PlayerDisconnect(UnturnedPlayer player)
        {
            if (plugin == null) return;

            // If player disconnect in combat lets drop their items
            if (AntiCombatLogoutTools.CombatPlayersId[player.Id])
            {
                AntiCombatLogoutTools.PlayerLogoutInCombat(player.Id);
                // Inform all players
                UnturnedChat.Say(plugin.Translate("Player_Exited_In_combat", player.DisplayName));

                // Dropping Normal items and Equipped items
                for (byte i = 0; i < plugin.Configuration.Instance.InventoryMaxPage; i++)
                {
                    for (byte j = 0; j < plugin.Configuration.Instance.InventoryMaxX; j++)
                    {
                        for (byte k = 0; k < plugin.Configuration.Instance.InventoryMaxY; k++)
                        {
                            player.Inventory.ReceiveDropItem(i, j, k);
                        }
                    }
                }
                // Dropping Equipped Clothes
                {
                    #region shirt
                    ItemAsset shirt = player.Inventory.player.clothing.shirtAsset;
                    if (shirt != null)
                    {
                        Item shirtItem = new(shirt.id, true, shirt.quality);
                        ItemManager.dropItem(shirtItem, player.Position, false, true, false);
                    }
                    #endregion
                    #region pant
                    ItemAsset pant = player.Inventory.player.clothing.pantsAsset;
                    if (pant != null)
                    {
                        Item pantItem = new(pant.id, true, pant.quality);
                        ItemManager.dropItem(pantItem, player.Position, false, true, false);
                    }
                    #endregion
                    #region backpack
                    ItemAsset backpack = player.Inventory.player.clothing.backpackAsset;
                    if (backpack != null)
                    {
                        Item backpackItem = new(backpack.id, true, backpack.quality);
                        ItemManager.dropItem(backpackItem, player.Position, false, true, false);
                    }
                    #endregion
                    #region glasses
                    ItemAsset glasses = player.Inventory.player.clothing.glassesAsset;
                    if (glasses != null)
                    {
                        Item glassesItem = new(glasses.id, true, glasses.quality);
                        ItemManager.dropItem(glassesItem, player.Position, false, true, false);
                    }
                    #endregion
                    #region hat
                    ItemAsset hat = player.Inventory.player.clothing.hatAsset;
                    if (hat != null)
                    {
                        Item hatItem = new(hat.id, true, hat.quality);
                        ItemManager.dropItem(hatItem, player.Position, false, true, false);
                    }
                    #endregion
                    #region mask
                    ItemAsset mask = player.Inventory.player.clothing.maskAsset;
                    if (mask != null)
                    {
                        Item maskItem = new(mask.id, true, mask.quality);
                        ItemManager.dropItem(maskItem, player.Position, false, true, false);
                    }
                    #endregion
                    #region vest
                    ItemAsset vest = player.Inventory.player.clothing.vestAsset;
                    if (vest != null)
                    {
                        Item vestItem = new(vest.id, true, vest.quality);
                        ItemManager.dropItem(vestItem, player.Position, false, true, false);
                    }
                    #endregion
                }

                // Deleting player clothing
                string clothingPath = Path.Combine(plugin.Configuration.Instance.PlayersFolder, $"{player.Id}_0", plugin.Configuration.Instance.LevelName, "Player", "Clothing.dat");
                if (File.Exists(clothingPath)) File.Delete(clothingPath);

                // Deleting player inventory
                string inventoryPath = Path.Combine(plugin.Configuration.Instance.PlayersFolder, $"{player.Id}_0", plugin.Configuration.Instance.LevelName, "Player", "Inventory.dat");
                if (File.Exists(inventoryPath)) File.Delete(inventoryPath);
            };

            // Remove the update event for the player to release memory
            player.Events.OnUpdateHealth -= PlayerHealthUpdated;

            // Remove it from timer memory
            combatTimer.Remove(player);

            // Remove previous player health from the memory
            previousHealth.Remove(player.Id);

            // Remove it from combat memory
            AntiCombatLogoutTools.CombatPlayersId.Remove(player.Id);
        }
        public void PlayerHealthUpdated(UnturnedPlayer player, byte health)
        {
            if (plugin == null) return;

            if (previousHealth[player.Id] > health)
            {
                // Check if player is currently in combat mode
                if (combatTimer.TryGetValue(player, out _)) combatTimer[player] = plugin.Configuration.Instance.CombatTickrateDuration;
                else
                {
                    UnturnedChat.Say(player, plugin.Translate("Entering_Combat", plugin.Configuration.Instance.CombatTickrateDuration / plugin.Configuration.Instance.ServerTickrate), Palette.COLOR_R);
                    combatTimer.Add(player, plugin.Configuration.Instance.CombatTickrateDuration);
                }
                // Add player to the combat variables
                AntiCombatLogoutTools.PlayerEnteringInCombat(player.Id);
            }
        }
    }

    static public class AntiCombatLogoutTools
    {
        /// <summary>
        /// Contains all players in combat mode
        /// </summary>
        readonly public static Dictionary<string, bool> CombatPlayersId = new();

        /// <summary>
        /// Called every time a player enter in combat
        /// </summary>
        public static event Action<string>? PlayerEnterInCombat;
        /// <summary>
        /// Called every time a player exit from combat
        /// </summary>
        public static event Action<string>? PlayerExitFromCombat;
        /// <summary>
        /// Called when a player disconnect in combat
        /// </summary>
        public static event Action<string>? PlayerCombatLogout;

        public static void PlayerEnteringInCombat(string playerId)
        {
            CombatPlayersId[playerId] = true;
            PlayerEnterInCombat?.Invoke(playerId);
        }
        public static void PlayerExitingFromCombat(string playerId)
        {
            CombatPlayersId[playerId] = false;
            PlayerExitFromCombat?.Invoke(playerId);
        }
        public static void PlayerLogoutInCombat(string playerId)
        {
            PlayerCombatLogout?.Invoke(playerId);
        }
    }
}
