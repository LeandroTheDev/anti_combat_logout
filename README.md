# Anti Combat Logout
Players entering in combat will need to wait a configurable amount of seconds beforing exiting from the game, 
if the player exit during this time all their inventory will be dropped and cleaned.

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

*Linux*: Unfortunately versions lower than 6 of dotnet do not have support for linux, the best thing you can do is install dotnet 6 or the lowest possible version on your distro and try to compile in dotnet 6 using the command dotnet build -c Debug, this can cause problems within rocket loader.

FTM License.
