using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.WPF.Reflection.Attributes;

namespace TofuPatcher
{
    public class TofuPatcherSettings
    {
        [SettingName("Mods to exclude")]
        public IEnumerable<ModKey> ExcludeMods = [];

        [Tooltip(
            "Trim excess whitespace from text. Turn off if you don't want this or if you get TooManyMasters error"
        )]
        public bool TrimWhitespace = true;
    }
}
