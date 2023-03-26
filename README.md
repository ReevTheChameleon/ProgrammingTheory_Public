Unity Junior Programmer Pathway Final Project: Programming Theory
===========================================================================
Release v1.2  
by Reev the Chameleon

About
---------------------------------------------------------------------------
This project started out as a final project required to complete the
Unity [Junior Programmer Pathway on Unity Learn](https://learn.unity.com/pathway/junior-programmer)
as part of my studying of how to make a game by Unity. As a final project,
I intended to make it into a small yet fully playable game.
It took me quite longer than expected, but here it is!
I would be happy if you enjoy playing it!

Please view and download the latest release of the game [here](https://github.com/ReevTheChameleon/ProgrammingTheory_Public/releases/latest).  
Or play on WebGL at [Unity Play](https://play.unity.com/mg/other/1-1-wgzzd) or at [itch.io](https://reevthechameleon.itch.io/3-digit-maze)

Note: Because the game was originally intended to be published on WebGL,
there is no quit button in Windows version. Please resort to alt+F4 to quit the game
(for now).

Project Error?
---------------------------------------------------------------------------
**Files in this repository represent only the portion of my project that can be
publicly exposed.**

While I have a fully functional project locally and would like to share it with you
in its entirety, I could not do so due to many complications. In short, you can view
most of the source code of the game here, but if you download the project and tries
to open in Unity Editor, it may not work.

Some of the aforemention complications are:

### Dependency
If you download this project and try to open it in Unity Editor, you would be
greeted with an error message box complaining about "Unity Package Manager Error"

This happens because the project uses code from my other package: UsefulScriptPackages v1.19.1,
which exists in my *another* repository, and I feel that it is redundant to duplicate
that entire repository here.

However, this is not very difficult to resolve:
1) Quit Unity first.
2) Go to my [Unity-Useful-Scripts repository](https://github.com/ReevTheChameleon/Unity-Useful-Scripts)
and download it as zip file.
3) Unzip the downloaded package and make note of it location.
4) Open this project again. This time when the message box occurs, choose "Continue".
5) The next message box will appear, asking whether you want to Enter Safe Mode or not.
Choose "Enter Safe Mode".
6) Unity Editor will open in safe mode. Navigate to menu bar: Window -> Package Manager.
7) Click the small + button near the top-left of the Package Manager window
and Select "Add package from disk..."
8) Browse to the location of UsefulScripts Package you just downloaded,
then select "package.json" and click Open.

Unity should then install the package, recompile the scripts, then the error should go away
and you should be able to continue opening the project.

### Missing Assets
I use a lot of third-party assets in this game. However, while being free,
most of them contain licenses or terms of use that prevent redistribution, and
from their point of view, exposing it raw in public git repository can be considered as such.
As a result, I have to exclude many assets from the repository, to the point that
it may not be playable in the Editor.

Here are the list of assets that are included in the released game, but are
excluded from this repository. You can obtain them for free (under conditions),
and view their corresponding licenses from the links below:

#### [textures.com](https://www.textures.com/)
- Room Ceiling Texture: [Rock Cliff Jungle](https://www.textures.com/download/PBR0227/133283)
- Room Wall Texture: [Slate Pavement 3](https://www.textures.com/download/PBR1084/143624)
- Room Floor Texture: [Stone Wall Cladding 2](https://www.textures.com/download/PBR0357/137040)
- Locked Door: [Old Tavern Door](https://www.textures.com/download/PBR0913/139951)
- Exit Portal & Pedestal: [Polished White Marble Slab](https://www.textures.com/download/PBR0499/138510)

Note: To download, you have to register an account first. Then you will gain
15 free credits every day, which you can then use to download their textures.

#### [Unity Asset Store](https://assetstore.unity.com/)
- Intro Scene BGM: Night Ambient 5 - [25 Fantasy RPG Game Tracks Music Pack (alkakrab)](https://assetstore.unity.com/packages/audio/music/25-fantasy-rpg-game-tracks-music-pack-240154)
- Main Scene BGM: Ambient 9 - [25 Fantasy RPG Game Tracks Music Pack (alkakrab)](https://assetstore.unity.com/packages/audio/music/25-fantasy-rpg-game-tracks-music-pack-240154)
- Outro Scene BGM: Piano Instrumental 1 - [Piano Instrumental Music Loops (alkakrab)](https://assetstore.unity.com/packages/audio/music/piano-instrumental-music-loops-240221)
- Portal Reaction SFX: Arcane Beacon - [Shapeforms Audio Free Sound Effects (shapeforms)](https://assetstore.unity.com/packages/audio/sound-fx/shapeforms-audio-free-sound-effects-183649)
- Portal Touch SFX: Glyph Activation Warm Aura - [Shapeforms Audio Free Sound Effects (shapeforms)](https://assetstore.unity.com/packages/audio/sound-fx/shapeforms-audio-free-sound-effects-183649)
- Player Damage SFX: PUNCH_PERCUSSIVE_HEAVY_08 - [Shapeforms Audio Free Sound Effects Asset (shapeforms)](https://assetstore.unity.com/packages/audio/sound-fx/shapeforms-audio-free-sound-effects-183649)
- Candle Pick SFX: UI Message Appear 01 - [Shapeforms Audio Free Sound Effects Asset (shapeforms)](https://assetstore.unity.com/packages/audio/sound-fx/shapeforms-audio-free-sound-effects-183649)
- Potion Pick SFX: Xylo_13 - [Free SFX Package (Bleep Blop Audio)](https://assetstore.unity.com/packages/audio/sound-fx/free-sfx-package-5178)
- Interaction SFX: Energy_Bling - [Epic Arsenal - Essential Elements - Demo Packs Asset (Epic Sounds and FX)](https://assetstore.unity.com/packages/audio/sound-fx/epic-arsenal-essential-elements-demo-packs-38428)
- Footstep SFX: Floor_step12 - [Classic Footstep SFX (Matthew Anett)](https://assetstore.unity.com/packages/audio/sound-fx/classic-footstep-sfx-173668)
- Torch Flame Texture: flame02 - [Unity Particle Pack Asset (Unity Technologies Inc.)](https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325)

#### [mixamo.com](https://www.mixamo.com/#/)
- Player Character: YBot
- Player Animation: Locomotion Pack, Opening, Walking Backward, Getting Hit, Dying

Note: You would need to register and log in to download.

About License
---------------------------------------------------------------------------
Unless specified in THIRDPARTYNOTICE.md, the content in this project is licensed
under MIT license, one of the most permissive open source license there is!
In short, you can use content in here, both freely and commercially,
and can even modify it to your need, as long as you retain the copyright notice
and the permissive notice text found in LICENSE.md file.

However, I would also be happy if the code in this package somehow helps you write 
code that solve your specific problems while making game (making game is hard).
In that case, since you do not use the content here directly, you technically
do not need to give attribution, although a little comment would be
very appreciated.

Buy Me a Coffee
---------------------------------------------------------------------------
If you enjoy playing the game, 
or if you find the code here useful and want to support me,
you may want to "buy me a coffee" at:
 
[<img src="https://storage.ko-fi.com/cdn/brandasset/kofi_button_stroke.png" width="20%"/>](https://ko-fi.com/reevthechameleon)  
[ko-fi.com/reevthechameleon](https://ko-fi.com/reevthechameleon)

Note: Sometimes I may buy an iced-chocolate instead.
