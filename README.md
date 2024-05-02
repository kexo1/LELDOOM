
<div align = center>

# LELDOOM
### Fast-paced arena type FPS game

Pickup 5 different permament abilities every wave
<img src="readmeFiles/abilities.gif" alt="Abilities"/>

Action gameplay with smooth movement system
<img src="readmeFiles/action_gameplay.gif" alt="Gameplay"/>

Find beers for temporary boosts
<img src="readmeFiles/pickables.gif" alt="Pickables"/>

<div align = left>

##  Features
* Fast and smooth movement with bunny-hop and sliding mechanics
* 5 abilities to choose from
* 6 different weapons (one is hidden somewhere)
* 4 detailed locations with beer spawns
* 3 unique enemies with special abilities, enemies get more difficult every wave
* 15 waves, every three waves you get new weapon

## Download
* Clone repository locally: 
`https://github.com/kexo1/LELDOOM.git`

##  More about this game
> This game is a simple project of mine, it's never going to be released on any gaming platform.
* I made this game as my final school year work, I had to learn basics of Unity and C# by myself
* Developing this game took approximately half a year
* You can view my trello board [here](https://trello.com/b/rUmtAu4S/leldoom)
* If you find any problem, solution, or you need help with something, message me at discord: _kexo

## Needs reworking and fixing
* Weapons aren't made in scriptable objects, which makes the code a lot messier
* PlayerMovement and Sliding scripts need to be merged into one
* Sliding sometimes bugs out on slopes
* Optimize PlayerMovement script
* Completely rework ceiling detection when player is under something
* Make bunny-hop functions more readable
* Rewrite settings save system, replace Prefs with JSON saving
* Optimize interaction detection
* Rewrite Wave script
* Make scripts that manage basic and repetetive operations (StateManagers)
* Agent colliders are moving from it's place
* OnCollisionEnter or OnTriggerEnter functions should be only in one script rather than on multiple scripts
* SphereCast functions not working properly on enemy sight detection, RayCast function is not ideal for projectiles
* Game isn't optimized, I've tried occlusion culling, batching, GPU instancing, setting static objects - nothing helped, more likely deeper Unity issue

## Credits
* [ScreamingBrainStudios](https://screamingbrainstudios.itch.io/) - old school textures
* [Elbolilloduro](https://elbolilloduro.itch.io/) - PSX style models
* [GGBotNet](https://ggbot.itch.io/) - PSX style cars
* [TheRealSwirls](https://therealswirls.itch.io/) - PSX horror music
* [Doctor_sci3nce](https://doctor-sci3nce.itch.io/) - PSX style ammo and weapons
* [obliviist](https://obliviist.itch.io/) - Capybara model :3
* [Wildenza](https://wildenza.itch.io/) - PSX Malorian gun model
* [mextie](https://mextie.itch.io/) - Glock model
* [Kamelionn](https://kamelionn.itch.io/) - PSX style TommyGun and MG3 weapon models
* [forst](https://assetstore.unity.com/publishers/408) - Detailed tree models
* [Staggart Creations](https://assetstore.unity.com/publishers/15580) - Stylized grass shader
* [ATOMICU3D](https://assetstore.unity.com/publishers/13222) - Towers pack
* [onemansymphony](https://onemansymphony.bandcamp.com/music) - DOOM like soundtrack
* Thank you DDuck for helping me fix certains problems I encountered
