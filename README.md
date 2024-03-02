![](logo.png)
# KindredArenas for V Rising
KindredArenas is a server modification for VRising PvP servers.
The function is to keep all players PvP protected except in designated Arenas.

Also, thanks to the V Rising modding and server communities for ideas and requests!

## Commands
- `.arena (on/off)`
  - turns on or off all arenas
- `.arena create (Name) (radius)`
  - makes a PvP arena centered at your location with the specified name and radius size
  - Example: *.arena create Test 10*
  - Shortcut: *.arena add*
- `.arena remove (name)`
  - deletes a named arena
  - Example: *.arena remove Test*
  - Shortcut: *.arena delete*
- `.arena list`
  - Lists all PvP arenas with details
- `.arena center (name)`
  - changes the center of a PvP arena to your current position
  - Example: *.arena center Test*
- `.arena radius (name) (radius)`
  - changes the radius of a PvP arena to the amount specified. If the (radius) field is left blank, it will change the radius to the different between the center position and your new current position.
  - Example: *.arena radius Test 10*
  - Example 2: *.arena radius Test* (this one will set the radius edge to your current position)
- `.arena enable (name)`
  - Enables a specified PvP arena
  - Example: *.arena enable Test*
- `.arena disable (name)`
  - Disables a specified PvP arena
  - Example: *.arena disable Test*
- `.arena teleport (name)`
  - Teleports you to the center of the named PvP arena
  - Example: *.arena teleport Test*
  - Shortcut: *.arena tp*
  
  
  
## Eventual To-Do
- make the opposite happen- pvp everywhere except for safe zones.
- make non-circle shaped arenas.