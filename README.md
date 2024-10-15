# TofuPatcher

An adaptation of [Tofu Detective](https://github.com/krypto5863/Tofu-Detective) as a Synthesis Patcher. While Tofu Detective has the capability to ouput a patch mod plugin, I wanted to have a way for users like myself who already use Synthesis in their workflow to be able to benefit.

This patcher is meant to be simpler and more straightforward than Tofu Detective, but does not offer all of the features Tofu Detective does as a standalone utility. See below for details.

### What this will do
- Convert any invalid characters found in dialogue text and item names to the equivalent character that the game will display
- Trim excess whitespace from strings

Text fields that are null (not set in the record) or all whitespace are ignored.

### What this will *not* do
- Fix any typos or other grammatical errors present in the text (see the example from Beyond Reach below)

## Usage
- Add to your Synthesis pipeline using the patcher browser
- If using multiple Synthesis groups, run this patcher in the same group as other patchers that modify dialogue, or named records (like Books) to ensure changes are merged properly.

The patcher will log which records it updated. These can be viewed in the Synthesis log files or in the UI itself.

### Available Patcher Settings
 - **`Mods to exclude`**: A list of modkeys (i.e. plugin filenames) to exclude from patching. Winning records from these mods will be skipped.
 - **`Trim whitespace`**: Trims excess whitespace from text in addition to fixing invalid characters. Default is on, but if you don't want the patcher to do this you can turn it off.
   - Since trimming whitespace also means the patcher will touch *a lot* more records, you can also try turning this off if you get a `TooManyMasters` error in your Synthesis group/pipeline.
   - For example, in my own setup (~1500 plugins) the difference is ~4600 records changed (~12s) with trimming vs 75 without (~6s)

## Caveats
Like Tofu Detective, this patcher is english-centric and will not work for translations. I also do not have plans to support multiple languages. You can configure the patcher to skip any translation plugins by adding them to the `Exclude mods` list in the patcher settings.

Unlike Tofu Detective, the patcher will not provide detailed information about what was changed for each record (i.e. excess whitespace trimmed vs invalid chars). It will track and log the total number of records patched, but will not provide a breakdown of record types or any other statistics.

## Should I use this vs Tofu Detective?

If you want more detailed information about each record, what is wrong with it, or to inspect the text, use Tofu Detective. If you just want to fix/cleanup records across your load order, use this patcher.

## Possible future features
- Custom transliteration mappings
- Fix simple typos?

This is my first patcher with user settings, not to mention it's more subjective than my previous patchers, so feedback during the early stages is welcome so that this can evolve into something more useful for more people.

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