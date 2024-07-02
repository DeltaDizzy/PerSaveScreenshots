# Per-Save Screenshots

Per-Save Screenshots makes it so that when you take a screenshot, it is placed into a folder within the game's Screenshots folder that corresponds to your saved game. No more melting pot of all your screenshots, letting you find the ones you want quicker and easier.

## Dependencies (none included)
- Shabby

## Installation
### CKAN (heavily recommended)
Open CKAN, search for "Per-Save Screenshots", click the checkbox to the left, and click the green checkmark on the top of the window. 

### Manual
Download the mod from [the releases page](https://github.com/DeltaDizzy/PerSaveScreenshots/releases) and install it as you would any other. Shabby is not bundled, as it was easier to list it as a dependency on CKAN than to hunt down a download link for it. Conformal Decals and Deferred (and perhaps other mods) both provide it, so getting it from one of their releases or from the CKAN source is recommended. No support for manual installs will be provided, if you run into issues just use CKAN.

## Changelog
- v1.0 (September 1st, 2023)
  - Initial Release
- v1.0.1 (September 19th, 2023)
  - Properly set UI hide flag 
- v1.1 (June 2nd, 2024)
  - Add Depth Mode
    - Depth Mode saves the [Depth buffer](https://en.wikipedia.org/wiki/Z-buffering) as an additional screenshot to [save name]/depth every time you take a screenshot while it is enabled. These depth buffer images can be used in imagine editing software to create a depth of field effect. They will likely need contrast stretching, but to preserve game performance, plugin complexity, and artistic freedom, the buffer images are provided raw and "as-is". Depth Mode does **not** currently support Screenshot Supersize, as all data manipulation and processing is done manually. Depth Mode is disabled by default, but can be enabled by using ModuleManager or editing GameData/PerSaveScreenshots/PerSaveScreenshots.cfg. Feedback on Depth Mode and sugegstions for improvement are welcome.
  - Add dependency on Shabby
    - Required to load the Depth Mode shader  
