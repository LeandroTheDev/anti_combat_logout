extern alias UnityEngineCoreModule;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityCoreModule = UnityEngineCoreModule.UnityEngine;

namespace AntiCombatLogout
{
    public class AntiCombatLogoutPlugin : RocketPlugin<AntiCombatLogouConfiguration>
    {
        private readonly Dictionary<string, byte> previousHealth = new();
        private readonly Dictionary<string, CancellationTokenSource> combatTimer = new();
        public override void LoadPlugin()
        {
            base.LoadPlugin();
            Rocket.Unturned.U.Events.OnPlayerConnected += OnPlayerConnected;
            Rocket.Unturned.U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            Logger.Log("AntiCombatLogout by LeandroTheDev");
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            // Add the combat players variable
            AntiCombatLogoutTools.CombatPlayersId.Add(player.Id, false);
            // Add the player to the previous health memory
            previousHealth.Add(player.Id, player.Health);
            // Instanciate the update event
            player.Events.OnUpdateHealth += HealthUpdate;
        }
        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            // If player disconnect in combat sucide then
            if (AntiCombatLogoutTools.CombatPlayersId[player.Id])
            {
                AntiCombatLogoutTools.PlayerLogoutInCombat(player.Id);
                // Inform all players
                UnturnedChat.Say(Translate("Player_Exited_In_combat", player.DisplayName));

                // If any task exist for the player cancel it
                if (combatTimer.TryGetValue(player.Id, out CancellationTokenSource _taskSource)) _taskSource.Cancel();

                // Dropping Normal items and Equipped items
                for (byte i = 0; i < Configuration.Instance.InventoryMaxPage; i++)
                {
                    for (byte j = 0; j < Configuration.Instance.InventoryMaxX; j++)
                    {
                        for (byte k = 0; k < Configuration.Instance.InventoryMaxY; k++)
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
                string clothingPath = Path.Combine(Configuration.Instance.PlayersFolder, $"{player.Id}_0", Configuration.Instance.LevelName, "Player", "Clothing.dat");
                if (File.Exists(clothingPath)) File.Delete(clothingPath);

                // Deleting player inventory
                string inventoryPath = Path.Combine(Configuration.Instance.PlayersFolder, $"{player.Id}_0", Configuration.Instance.LevelName, "Player", "Inventory.dat");
                if (File.Exists(inventoryPath)) File.Delete(inventoryPath);
            };

            // Remove the update event for the player to release memory
            player.Events.OnUpdateHealth -= HealthUpdate;

            // Remove it from timer memory
            combatTimer.Remove(player.Id);

            // Remove previous player health from the memory
            previousHealth.Remove(player.Id);

            // Remove it from combat memory
            AntiCombatLogoutTools.CombatPlayersId.Remove(player.Id);
        }

        private void HealthUpdate(UnturnedPlayer player, byte health)
        {
            if (previousHealth[player.Id] > health)
            {
                // If player is already in combat dont inform it again
                if (!AntiCombatLogoutTools.CombatPlayersId[player.Id]) UnturnedChat.Say(player, Translate("Entering_Combat", Configuration.Instance.CombatSecondsDuration), Palette.COLOR_R);

                // If any task exist for the player cancel it
                if (combatTimer.TryGetValue(player.Id, out CancellationTokenSource _taskSource))
                {
                    _taskSource.Cancel();
                    _taskSource.Dispose();
                    // Remove it from timer memory
                    combatTimer.Remove(player.Id);
                };

                // Create the cancellation token
                CancellationTokenSource taskSource = new();
                // Add it to the combat timer
                combatTimer.Add(player.Id, taskSource);

                // Create the delay method
                Task.Delay(Configuration.Instance.CombatSecondsDuration * 1000, taskSource.Token).ContinueWith((task) =>
                {
                    if (task.IsCanceled) return;
                    // Run in unity main thread because UnturnedChat only works in main thread
                    TaskDispatcher.QueueOnMainThread(() =>
                    {
                        // Inform the player
                        UnturnedChat.Say(player, Translate("No_Longer_Combat"), Palette.COLOR_G);
                        // Remove it from combat
                        AntiCombatLogoutTools.PlayerExitingFromCombat(player.Id);
                    });
                });

                // Add player to the combat variables
                AntiCombatLogoutTools.PlayerEnteringInCombat(player.Id);
            }
        }

        public override TranslationList DefaultTranslations => new()
        {
            { "Entering_Combat", "You are in combat, don't logout for {0} seconds" },
            { "No_Longer_Combat", "You are no longer in combat!" },
            { "Player_Exited_In_combat", "{0} exited in combat, what a shame" }
        };
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
