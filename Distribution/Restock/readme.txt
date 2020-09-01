=============
RESTOCK 1.2.0
=============

Restock is a project to improve the art of the Kerbal Space Program part set. Some of the revisions are based on the canceled Part Overhaul project by Porkjet.

Parts should behave almost exactly the same as they do in stock KSP - there are no gameplay or collider changes except as indicated in the NOTES section below.

Our goal is to bringing consistency, better and more educational detail and prettier rockets to the KSP experience.

============
DEPENDENCIES
============

Required:
- ModuleManager (4.1.3)

=======
CREDITS
=======

Art
- Chris Adderley (Nertea)
- Andrew Cassidy (Cineboxandrew)
- Beale
- blowfish
- Passinglurker
- Porkjet
- riocrokite

Drag cube wizardry
- DMagic

Plugin code
- blowfish

Lead Testing
- Tyko
- Poodmund

============
INSTALLATION
============

To install, drag the ReStock folder and ModuleManager DLL from the GameData folder you downloaded into your Kerbal Space Program GameData folder. If installed correctly, you should see:
- Kerbal Space Program
    - GameData
        - Squad
        - ReStock
            - Assets
            - Patches
            - etc.
        - ModuleManager.4.1.3.dll

===============
ASSET BLACKLIST
===============

Restock contains a plugin that stops replaced stock assets from loading, reducing the memory footprint of the mod. The list of blacklisted assets can be found in GameData/ReStock/Restock.restockblacklist.
If your mod requires a stock texture, you can create a xxx.restockwhitelist file anywhere in your GameData distribution. Each line in this file can target an asset or a folder, for example adding this line to the whitelist file:

Squad/Parts/Aero/HeatShield/HeatShield3.mu

will cause the size 3 heatshield .mu file to be whitelisted and loaded into the game. You can also whitelist entire folders as follows:

Squad/Parts/Aero/HeatShield/

=====
NOTES
=====

The DTS-M1 Antenna's exact stock colliders were not retained due to technical issues and had to be approximated.

============
LOCALIZATION
============

This mod primarily uses vanilla-provided localization, but for additional strings, it provides localization support for the following languages:
- English
- German (woeller)
- French (Challyss and vinix38)
- Brazilian Portuguese (AluisioASG)
- Spanish (Niako)
- Simplified Chinese (Duck1998)

=========
LICENSING
=========

See the license.txt file for more information.

Any bundled mods are distributed under their own license:
- ModuleManager by blowfish and sarbian is distributed under a Creative Commons Sharealike license. More details, including source code, can be found here: https://forum.kerbalspaceprogram.com/index.php?/topic/50533-*
