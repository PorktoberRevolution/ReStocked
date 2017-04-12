
# Proposed Repository Structure

* Distribution
  * readme.txt
  * changelog.txt
  * license.txt
  * GameData
    * ReStock
      * Assets: all model and texture files
        * <Part Category>
          * <Size Category>
            * Assets
      * Patches: patches to enable the replacement of the bits

* Assets: author source files
  * Author: A folder per author
    * license.txt (license your source files!)
    * whatever asset file structure you want

# Proposed Asset Naming Conventions

restock-<descriptor>-<sizecategory>-<id>

## Examples:

### Largest 3.75m Tank

restock-fueltank-375-1.mu
restock-fueltank-375-1.dds
restock-fueltank-375-1-n.dds
restock-fueltank-375-1-e.dds

### Small radial Battery

restock-battery-radial-1.mu
restock-battery-radial-1.dds
restock-battery-radial-1-n.dds
restock-battery-radial-1-e.dds
