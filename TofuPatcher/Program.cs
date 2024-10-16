using System.Collections.Frozen;
using System.Diagnostics;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace TofuPatcher
{
    public class Program
    {
        static Lazy<TofuPatcherSettings> _settings = null!;
        static TofuPatcherSettings Settings => _settings.Value;

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline
                .Instance.AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetAutogeneratedSettings(
                    nickname: "TofuPatcherSettings",
                    path: "TofuPatcherSettings.json",
                    out _settings
                )
                .SetTypicalOpen(GameRelease.SkyrimSE, "TofuPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var excludeMods = Settings.ExcludeMods.ToFrozenSet();

            var transforms = new List<Func<string?, string?>> { TextUtil.ToAscii };

            if (Settings.TrimWhitespace)
            {
                transforms.Add(str => str?.Trim());
            }

#if DEBUG
            long startTime = Stopwatch.GetTimestamp();
#endif

            var namedPatcher = new NamedRecordTextPatcher(transforms);
            var infoPatcher = new DialogueInfoTextPatcher(transforms);

            var pipeline = new TextPatcherPipeline(
                state.PatchMod,
                context => !excludeMods.Contains(context.ModKey)
            );

            var namedRecords = state
                .LoadOrder.PriorityOrder.WinningContextOverrides<
                    ISkyrimMod,
                    ISkyrimModGetter,
                    INamed,
                    INamedGetter
                >(state.LinkCache)
                .Where(context => context.Record is IMajorRecordGetter); // Extra check since INamedGetter doesn't inherit from IMajorRecordGetter
            pipeline.PatchRecords(namedPatcher, namedRecords);

            var dialogueInfos = state
                .LoadOrder.PriorityOrder.DialogResponses()
                .WinningContextOverrides(state.LinkCache);
            pipeline.PatchRecords(infoPatcher, dialogueInfos);

#if DEBUG
            TimeSpan elaspedTime = Stopwatch.GetElapsedTime(startTime);
            Console.WriteLine($"Patcher took {elaspedTime.TotalSeconds}s");
#endif

            Console.WriteLine($"Patched {pipeline.PatchedCount} total records");
        }
    }
}
