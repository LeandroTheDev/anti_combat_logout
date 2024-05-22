# Anti Combat Logout
Players entering in combat will need to wait a configurable amount of seconds beforing exiting from the game, 
if the player exit during this time all their inventory will be dropped and cleaned.

### Configuration
- PlayersFolder: location of players folders, this is necessary because when player disconnect in combat we need to clean their inventory
- LevelName: we use this to locate the player inventory by the level  to not clean other levels if exist
- CombatTickrateDuration: the tickrate amount to player exit from combat, this is based on ``ServerTickrate``
for example, on a server with 60 frames per second and you want 15 seconds of combat make the calculation: 60 x 15 = 900
- ServerTickrate: used only for calculations, make sure this is the same as ``MaxFrames`` from ``Rocket.config``
- InventoryMaxPage: larger numbers of this will make the server lag on cleaning inventory, my recomendation is 10
if you have mods that increase the page amount consider changing this.
- InventoryMaxX/InventoryMaxY: the biggest storage size on the server.

### Library
This mods also works like a library, if you create a reference into your project you can use the ``AntiCombatLogoutTools``
with this class you can access events and variables to manipulate the combat players
```css
/// Contains all players in combat mode
readonly public static Dictionary<string, bool> CombatPlayersId = new();
/// Called every time a player enter in combat
public static event Action<string>? PlayerEnterInCombat;
/// Called every time a player exit from combat
public static event Action<string>? PlayerExitFromCombat;
/// Called when a player disconnect in combat
public static event Action<string>? PlayerCombatLogout;
```

There is others functions like: ``PlayerEnteringInCombat`` ``PlayerExitingFromCombat`` ``PlayerLogoutInCombat`` this functions exists only for invoking this events
and updating the ``CombatPlayersId``

# Building

*Windows*: The project uses dotnet 4.8, consider installing into your machine, you need visual studio, simple open the solution file open the Build section and hit the build button (ctrl + shift + b) or you can do into powershell the command dotnet build -c Debug if you have installed dotnet 4.8.

*Linux*: Install dotnet-sdk from your distro package manager, open the root folder of this project and type ``dotnet build -c Debug``.

FTM License.
