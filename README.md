# TofuPatcher

An adaptation of [Tofu Detective](https://github.com/krypto5863/Tofu-Detective) as a Synthesis Patcher. While Tofu Detective already has the capability to ouput a patch mod plugin, I wanted to have a way for those like me who already use Synthesis in their workflow to be able to benefit.

This patcher is meant to be simpler and more straightforward than Tofu Detective, but does not offer all of the features Tofu Detective does as a standalone utility. See below for details.

### What this will do
- Convert any invalid characters found in dialogue text to the equivalent character that the game will display
- Trim excess whitespace from strings

### What this will *not* do
- Fix any typos or other grammatical errors present in the text (see the example from Beyond Reach below)

## Usage
- Add to your Synthesis pipeline using the patcher browser
- If using multiple Synthesis groups, run this patcher in the same group as other patchers that modify dialogue to ensure changes are merged properly.

The patcher will log which dialogue topics/infos it updated. These can be viewed in the Synthesis log files or in the UI itself.

### Available Patcher Settings
 - **`Exclude mods`**: A list of modkeys (i.e. plugin filenames) to exclude from patching. Any dialogue records in these mods will be skipped.
 - **`Exclude records`**: A list of dialogue records to exclude from patching.
   - **NOTE** the list of available records can take a *reallly looong* time to load given the sheer number of them. Might consider making it an external config file instead for this reason

## Caveats
Like Tofu Detective, this patcher is english-centric and will not work for translations. I also do not have plans to support multiple languages. You can configure the patcher to skip any translation plugins by adding them to the `Exclude mods` list in the patcher settings.

Unlike Tofu Detective, the patcher will not provide detailed information about what was changed for each record (i.e. excess whitespace trimmed vs invalid chars). It will track and log the total number of records patched, but will not provide a breakdown of record types or any other statistics.

## Should I use this vs Tofu Detective?

If you want more detailed information about each dialogue record, what is wrong with it, or to inspect the text, use Tofu Detective. If you just want to cleanup dialogue records across your load order, use this patcher.

## Possible future features
- Custom transliteration mappings
- Fix simple typos?

This is my first patcher with user settings, not to mention more subjective than my previous patchers, so feedback during the early stages is welcome so that this can evolve into something more useful for more people.

## Reporting Bugs/Issues
Please include the following to help me help you:
- Synthesis log file(s)
- `Plugins.txt`
- Specific record(s) that are problematic
  - xEdit screenshots not required, but appreciated

## Credits
**krypto5863** for creating Tofu Detective and the original logic of fixing the dialogue texts.

## Examples

![3DNPC](/examples/3dnpc.jpg)

![Beyond Reach](/examples/arnima.jpg)