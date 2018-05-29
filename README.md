# Restock
Replacing and upgrading the art of Kerbal Space Program

### Goals

The goal of this project is to improve the art of Kerbal Space Program with the following goals in mind
* Create a unity of design and style for all parts
* Preserve the general aesthetic of KSP
* Create consistency in the detail level of all parts
* Do not affect gameplay or part balance


### Restock+

We believe that there are a number of places where KSP is missing key parts - for example, where a stack class is missing parts that exist in other classes. Restock+ aims to create parts that fill this niche, and is a wholly optional project

## Contributors


If you want to contribute, contact us.

## Repository Structure
* Distribution
  * readme.txt
  * changelog.txt
  * license.txt
  * GameData
    * ReStock
      * Assets: all model and texture files
      * Patches: ModuleManager patches to enable art replacements
    * ReStockPlus
      * Assets: all models and texture files
      * Parts: config files for new parts
      * Patches: ModuleManager patches that change stock parts

* Assets: author source files
  * Author: A folder per author
    * license.txt (license your source files!)
    * whatever asset file structure you want

# Proposed Asset Naming Conventions

restock-`<descriptor>`-`<sizecategory>`-`<id>`-`<suffix>`

Suffix is like -n for nomral map, -e for emissive, etc

## Examples:
#### Largest 3.75m Tank
* restock-fueltank-375-1.mu
* restock-fueltank-375-1.dds
* restock-fueltank-375-1-n.dds
* restock-fueltank-375-1-e.dds
#### Small radial Battery
* restock-battery-radial-1.mu
* restock-battery-radial-1.dds
* restock-battery-radial-1-n.dds
* restock-battery-radial-1-e.dds
