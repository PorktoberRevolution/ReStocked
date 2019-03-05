# Restock [![Build Status](https://travis-ci.com/PorktoberRevolution/ReStocked.svg?branch=prod)](https://travis-ci.com/PorktoberRevolution/ReStocked)

It's no secret that some of KSP's artwork is not great. Placeholder art made by a number of amateur artists throughout the game's development has resulted in a scattered mess of styles and qualities which is most evident in the part artwork.

The goal of this project is to improve the art of Kerbal Space Program with the following goals in mind:
* Create a unity of design and style for all parts
* Preserve the general aesthetic of KSP
* Create consistency in the detail level of all parts
* Do not affect gameplay or part balance

## Restock+

We believe that there are a number of places where KSP is missing key parts - for example, where a stack class is missing parts that exist in other classes. Restock+ aims to create parts that fill this niche, and is a wholly optional project.

## Contributing

If you want to contribute fixes and config improvements, such as new localization, make a pull request against the `master` branch - we will
If you are interested in contributing artistically to the revamp, please contact us.

### Current Contributors

#### Art
- Andrew Cassidy
- Chris Adderley (Nertea)
- Beale
- blowfish
- Passinglurker
- Porkjet
- riocrokite

#### Drag cube wizardry
- DMagic

#### Plugin code
- blowfish

#### Lead Testing
- Tyko
- Poodmund

### Repository Structure
* Distribution
  * Restock
    * readme.txt
    * changelog.txt
    * license.txt
    * GameData
      * ReStock
        * **Assets:** all model and texture files
        * **FX:** new effects
        * **Localization:** new localization data
        * **Patches:** ModuleManager patches to enable art replacements
        * **PatchesMH:** ModuleManager patches to enable art replacements for Making History
  * RestockPlus
    * readme.txt
    * changelog.txt
    * license.txt
    * GameData
      * ReStockPlus
        * **Assets**: all models and texture files
        * **FX**: new effects
        * **Localization**: new localization data
        * **Parts**: config files for new parts
        * **Patches**: ModuleManager patches that change parts
