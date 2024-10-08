using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.WPF.Reflection.Attributes;

namespace TofuPatcher
{
    public class TofuPatcherSettings
    {
        [SettingName("Mods to exclude")]
        public IEnumerable<ModKey> ExcludeMods = new List<ModKey>();

        [SettingName("Records to exclude")]
        public IEnumerable<IFormLinkGetter<IDialogGetter>> ExcludeRecords =
            new List<IFormLinkGetter<IDialogGetter>>();
    }
}
