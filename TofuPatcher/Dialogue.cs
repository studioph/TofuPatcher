using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using AnyAscii;
using Badeend.ValueCollections;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace TofuPatcher
{
    /// <summary>
    /// Generic interface to abstract patching text strings in dialogue records
    /// </summary>
    /// <typeparam name="TMajor">The record type</typeparam>
    /// <typeparam name="TMajorGetter">The record getter type</typeparam>
    /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
    public interface IDialoguePatcher<TMajor, TMajorGetter, TValue>
        where TMajor : TMajorGetter
        where TMajorGetter : IDialogGetter
        where TValue : notnull
    {
        /// <summary>
        /// Transforms the text values from the record by trimming whitespace and transliterating invalid characters
        /// </summary>
        /// <param name="context">The mod context for the record</param>
        /// <returns>A DTO containing the original values, processed values, and the mod context</returns>
        public FixedDialogue<TMajor, TMajorGetter, TValue> Process(
            IModContext<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter> context
        );

        /// <summary>
        /// Optionally checks any criteria to filter out the record before processing for efficiency
        /// </summary>
        /// <param name="record">The record to check</param>
        /// <returns>False if the record should be skipped</returns>
        public bool Filter(TMajorGetter record);

        /// <summary>
        /// Patches the record with the updated text strings
        /// </summary>
        /// <param name="fixedDialogue">The processed text values</param>
        /// <param name="target">The target record to update</param>
        public void Patch(TValue fixedDialogue, TMajor target);
    }

    /// <summary>
    /// Patcher for fixing Dialogue Topic records
    /// </summary>
    public class DialogueTopicPatcher : IDialoguePatcher<IDialogTopic, IDialogTopicGetter, string>
    {
        public static readonly DialogueTopicPatcher Instance = new();

        public FixedDialogue<IDialogTopic, IDialogTopicGetter, string> Process(
            IModContext<ISkyrimMod, ISkyrimModGetter, IDialogTopic, IDialogTopicGetter> context
        )
        {
            var topicName = context.Record.Name?.String;
            var fixedText = topicName?.ToAscii();
            /*
                Shouldn't ever have records with NULL Name property due to Filter(),
                but still need to handle it to decouple the method from Filter()
            */
            return new FixedDialogue<IDialogTopic, IDialogTopicGetter, string>(
                context,
                topicName ?? string.Empty,
                fixedText ?? string.Empty
            );
        }

        public bool Filter(IDialogTopicGetter record) =>
            !record.Name?.String.IsNullOrWhitespace() ?? false;

        public void Patch(string fixedDialogue, IDialogTopic target)
        {
            target.Name = fixedDialogue;
            Console.WriteLine($"Patched DIAL:{target.FormKey}");
        }
    }

    /// <summary>
    /// Patcher for fixing Dialogue Info records (also known as Dialogue Responses)
    /// </summary>
    public class DialogueInfoPatcher
        : IDialoguePatcher<IDialogResponses, IDialogResponsesGetter, DialogueInfoTexts>
    {
        public static readonly DialogueInfoPatcher Instance = new();

        public FixedDialogue<IDialogResponses, IDialogResponsesGetter, DialogueInfoTexts> Process(
            IModContext<
                ISkyrimMod,
                ISkyrimModGetter,
                IDialogResponses,
                IDialogResponsesGetter
            > context
        )
        {
            var record = context.Record;
            var originalTexts = new DialogueInfoTexts(
                record.Responses.Select(response => response.Text.String).ToValueList(),
                record.Prompt?.String
            );

            var fixedResponses = originalTexts.Responses.Select(text => text.ToAscii());
            var fixedTexts = new DialogueInfoTexts(
                fixedResponses.ToValueList(),
                originalTexts.Prompt.ToAscii()
            );

            return new FixedDialogue<IDialogResponses, IDialogResponsesGetter, DialogueInfoTexts>(
                context,
                originalTexts,
                fixedTexts
            );
        }

        public bool Filter(IDialogResponsesGetter record) => true;

        public void Patch(DialogueInfoTexts fixedDialogue, IDialogResponses target)
        {
            target.Prompt = fixedDialogue.Prompt;

            var zipped = target.Responses.Zip(fixedDialogue.Responses);
            foreach (var (response, fixedText) in zipped)
            {
                response.Text = fixedText;
            }
            Console.WriteLine($"Patched INFO:{target.FormKey}");
        }
    }

    /// <summary>
    /// Pipelines patching dialogue records.
    /// Can apply common pre-processing filters to all records on top of patcher-specific filters.
    /// Will also track the number of updated records.
    /// </summary>
    public class DialoguePatcherPipeline
    {
        private readonly ISkyrimMod PatchMod;
        private readonly IEnumerable<Func<IModContext<IDialogGetter>, bool>> Filters;
        public uint PatchedCount { get; private set; } = 0;

        /// <summary>
        /// Create a new pipeline instance with the given mutable mod and optional filters
        /// </summary>
        /// <param name="patchMod">The mutable mod object that updates will be written to</param>
        /// <param name="filters">Optional common filters that will be applied to all records pre-preprocessing regardless of type</param>
        public DialoguePatcherPipeline(
            ISkyrimMod patchMod,
            params Func<IModContext<IDialogGetter>, bool>[] filters
        )
        {
            PatchMod = patchMod;
            Filters = filters;
        }

        public DialoguePatcherPipeline(
            ISkyrimMod patchMod,
            IEnumerable<Func<IModContext<IDialogGetter>, bool>> filters
        )
            : this(patchMod, filters.ToArray()) { }

        /// <summary>
        /// Applies multiple filters against a mod context
        /// </summary>
        /// <param name="context">The mod context to check</param>
        /// <returns>True if the context satisfies all of the conditions</returns>
        public bool FilterCommon<TMajor>(IModContext<TMajor> context)
            where TMajor : IDialogGetter
        {
            foreach (var predicate in Filters)
            {
                if (!predicate((IModContext<IDialogGetter>)context))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Processes records and returns the results that require overriding.
        /// </summary>
        /// <typeparam name="TMajor">The record type</typeparam>
        /// <typeparam name="TMajorGetter">The record getter type</typeparam>
        /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
        /// <param name="patcher">The patcher instance for the record type</param>
        /// <param name="contexts">The record contexts to process</param>
        /// <returns>The processed records that should be overridden</returns>
        public IEnumerable<FixedDialogue<TMajor, TMajorGetter, TValue>> GetRecordsToPatch<
            TMajor,
            TMajorGetter,
            TValue
        >(
            IDialoguePatcher<TMajor, TMajorGetter, TValue> patcher,
            IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter>> contexts
        )
            where TMajor : TMajorGetter
            where TMajorGetter : IDialogGetter
            where TValue : notnull =>
            contexts
                .Where(context => FilterCommon(context))
                .Where(context => patcher.Filter(context.Record))
                .Select(patcher.Process)
                .Where(result => !result.Fixed.Equals(result.Original));

        /// <summary>
        /// Patches dialogue records that have updated text
        /// </summary>
        /// <typeparam name="TMajor">The record type</typeparam>
        /// <typeparam name="TMajorGetter">The record getter type</typeparam>
        /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
        /// <param name="patcher">The patcher instance for the record type</param>
        /// <param name="contexts">The record contexts to process</param>
        public void PatchRecords<TMajor, TMajorGetter, TValue>(
            IDialoguePatcher<TMajor, TMajorGetter, TValue> patcher,
            IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter>> contexts
        )
            where TMajor : TMajorGetter
            where TMajorGetter : IDialogGetter
            where TValue : notnull
        {
            var itemsToPatch = GetRecordsToPatch(patcher, contexts);

            foreach (var item in itemsToPatch)
            {
                var target = item.Context.GetOrAddAsOverride(PatchMod);
                patcher.Patch(item.Fixed, target);
                PatchedCount++;
            }
        }
    }

    /// <summary>
    /// Utility class for dialogue text manipulation
    /// </summary>
    public static class DialogueUtil
    {
        /// <summary>
        /// The set of valid characters that will display properly in-game
        /// </summary>
        private static readonly IReadOnlySet<char> ValidChars =
            "`1234567890-=~!@#$%^&*():_+QWERTYUIOP[]ASDFGHJKL;'\"ZXCVBNM,./qwertyuiop{}\\asdfghjklzxcvbnm<>?|¡¢£¤¥¦§¨©ª«®¯°²³´¶·¸¹º»¼½¾¿ÄÀÁÂÃÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ ÿ ".ToFrozenSet();

        /// <summary>
        /// Converts a string that may contain invalid characters to ASCII
        /// </summary>
        /// <param name="text">The string to modify</param>
        /// <returns>The transliterated string</returns>
        [return: NotNullIfNotNull(nameof(text))]
        public static string? ToAscii(this string? text)
        {
            if (text is null)
            {
                return null;
            }

            var invalidChars = text.ToFrozenSet().Except(ValidChars);
            var fixedText = invalidChars.Aggregate(
                text.Trim(),
                (current, character) =>
                    current.Replace($"{character}", $"{character}".Transliterate())
            );
            return fixedText;
        }
    }
}
