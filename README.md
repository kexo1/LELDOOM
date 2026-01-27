<div align="center">

# LELDOOM  
### Fast‑paced arena‑style FPS game

Pickup 5 different permanent abilities every wave  

![Abilities](readmeFiles/abilities.gif)

Action gameplay with a smooth movement system  

![Gameplay](readmeFiles/action_gameplay.gif)

Find beers for temporary boosts  

![Pickables](readmeFiles/pickables.gif)

</div>

<div align="left">

## Features
* Fast and smooth movement with bunny‑hop and sliding mechanics  
* 5 abilities to choose from  
* 6 different weapons (one is hidden somewhere)  
* 4 detailed locations with beer spawns  
* 3 unique enemies with special abilities; enemies get more difficult every wave  
* 15 waves, and every three waves you get a new weapon  

## Download
* Download from [Itch.io](https://kexo1.itch.io/leldoom)  
* Or clone the repository locally:  
`https://github.com/kexo1/LELDOOM.git`

## More about this game
> This game is a simple project of mine; it will never be released on any gaming platform.  
* I made this game as my final school‑year work, learning the basics of Unity and C# by myself.  
* Developing this game took approximately half a year.  
* You can view my Trello board [here](https://trello.com/b/rUmtAu4S/leldoom).  
* If you find any problem, solution, or need help with something, message me on Discord: _kexo

## Needs reworking and fixing
* Weapons aren’t made in Scriptable Objects, which makes the code a lot messier.  
* `PlayerMovement` and sliding scripts need to be merged into one.  
* Sliding sometimes bugs out on slopes.  
* Optimize the `PlayerMovement` script.  
* Completely rework ceiling detection when the player is under something.  
* Make bunny‑hop functions more readable.  
* Rewrite the settings save system, replacing `Prefs` with JSON saving.  
* Optimize interaction detection.  
* Rewrite the wave script.  
* Create scripts that manage basic and repetitive operations (StateManagers).  
* Agent colliders are moving from their place.  
* `OnCollisionEnter` or `OnTriggerEnter` functions should be only in one script rather than on multiple scripts.  
* SphereCast functions aren’t working properly for enemy sight detection; Raycast is not ideal for projectiles.  
* The game isn’t optimized; I’ve tried occlusion culling, batching, GPU instancing, setting static objects—nothing helped; it’s likely a deeper Unity issue.

## Credits
* [ScreamingBrainStudios](https://screamingbrainstudios.itch.io/) – old‑school textures  
* [Elbolilloduro](https://elbolilloduro.itch.io/) – PSX‑style models  
* [GGBotNet](https://ggbot.itch.io/) – PSX‑style cars  
* [TheRealSwirls](https://therealswirls.itch.io/) – PSX horror music  
* [Doctor_sci3nce](https://doctor-sci3nce.itch.io/) – PSX‑style ammo and weapons  
* [obliviist](https://obliviist.itch.io/) – Capybara model :3  
* [Wildenza](https://wildenza.itch.io/) – PSX Malorian gun model  
* [mextie](https://mextie.itch.io/) – Glock model  
* [Kamelionn](https://kamelionn.itch.io/) – PSX‑style Tommy Gun and MG3 weapon models  
* [forst](https://assetstore.unity.com/publishers/408) – Detailed tree models  
* [Staggart Creations](https://assetstore.unity.com/publishers/15580) – Stylized grass shader  
* [ATOMICU3D](https://assetstore.unity.com/publishers/13222) – Towers pack  
* [onemansymphony](https://onemansymphony.bandcamp.com/music) – DOOM‑like soundtrack  

Thank you, DDuck, for helping me fix certain problems I encountered.
