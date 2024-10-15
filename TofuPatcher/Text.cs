using System.Diagnostics.CodeAnalysis;
using AnyAscii;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace TofuPatcher
{
    /// <summary>
    /// Generic interface to abstract patching text strings in records
    /// </summary>
    /// <typeparam name="TMajor">The record type or category (aspect)</typeparam>
    /// <typeparam name="TMajorGetter">The record/category/aspect getter type</typeparam>
    /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
    public interface IRecordTextPatcher<TMajor, TMajorGetter, TValue>
        where TMajor : TMajorGetter
        where TMajorGetter : IMajorRecordQueryableGetter
        where TValue : notnull
    {
        /// <summary>
        /// Optionally checks any criteria to filter out the record before processing for efficiency
        /// </summary>
        /// <param name="record">The record to check</param>
        /// <returns>False if the record should be skipped</returns>
        public bool Filter(TMajorGetter record);

        /// <summary>
        /// Patches the record with the updated text strings
        /// </summary>
        /// <param name="fixedText">The processed text values</param>
        /// <param name="target">The target record to update</param>
        public void Patch(TValue fixedText, TMajor target);

        /// <summary>
        /// Transforms the text values from the record by applying one or more transformation functions
        /// </summary>
        /// <param name="context">The mod context for the record</param>
        /// <returns>A DTO containing the original values, processed values, and the mod context</returns>
        public FixedText<TMajor, TMajorGetter, TValue> Process(
            IModContext<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter> context
        );
    }

    /// <summary>
    /// Class to streamline patching multiple record types
    /// </summary>
    /// <param name="patchMod">The mutable mod object to write changes to</param>
    /// <param name="filters">Common filters to apply to each record/context before processing</param>
    public class TextPatcherPipeline(
        ISkyrimMod patchMod,
        params Func<IModContext<IMajorRecordQueryableGetter>, bool>[] filters
    )
    {
        private readonly ISkyrimMod _patchMod = patchMod;

        private readonly IEnumerable<
            Func<IModContext<IMajorRecordQueryableGetter>, bool>
        > _filters = filters;
        public uint PatchedCount { get; private set; } = 0;

        /// <summary>
        /// Applies multiple filters against a mod context
        /// </summary>
        /// <param name="context">The mod context to check</param>
        /// <returns>True if the context satisfies all of the conditions</returns>
        public bool FilterCommon<TMajor>(IModContext<TMajor> context)
            where TMajor : IMajorRecordQueryableGetter =>
            _filters.All(predicate => predicate((IModContext<IMajorRecordQueryableGetter>)context));

        /// <summary>
        /// Processes records with the given patcher instance
        /// </summary>
        /// <typeparam name="TMajor">The record type</typeparam>
        /// <typeparam name="TMajorGetter">The record getter type</typeparam>
        /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
        /// <param name="patcher">The patcher instance to use</param>
        /// <param name="contexts">The record contexts to process</param>
        /// <returns>The records that should be patched along with the updated values</returns>
        public IEnumerable<FixedText<TMajor, TMajorGetter, TValue>> GetRecordsToPatch<
            TMajor,
            TMajorGetter,
            TValue
        >(
            IRecordTextPatcher<TMajor, TMajorGetter, TValue> patcher,
            IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter>> contexts
        )
            where TMajor : TMajorGetter
            where TMajorGetter : IMajorRecordQueryableGetter
            where TValue : notnull =>
            contexts
                .Where(FilterCommon)
                .Where(context => patcher.Filter(context.Record))
                .Select(patcher.Process)
                .Where(result => !result.Fixed.Equals(result.Original));

        public void PatchRecords<TMajor, TMajorGetter, TValue>(
            IRecordTextPatcher<TMajor, TMajorGetter, TValue> patcher,
            IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter>> contexts
        )
            where TMajor : TMajorGetter
            where TMajorGetter : IMajorRecordQueryableGetter
            where TValue : notnull
        {
            var itemsToPatch = GetRecordsToPatch(patcher, contexts);

            foreach (var item in itemsToPatch)
            {
                var target = item.Context.GetOrAddAsOverride(_patchMod);
                patcher.Patch(item.Fixed, target);
                PatchedCount++;
            }
        }
    }

    public static class TextUtil
    {
        /// <summary>
        /// The set of valid characters that will display properly in-game
        /// </summary>
        private static readonly string _validChars =
            "`1234567890-=~!@#$%^&*():_+QWERTYUIOP[]ASDFGHJKL;'\"ZXCVBNM,./qwertyuiop{}\\asdfghjklzxcvbnm<>?|¡¢£¤¥¦§¨©ª«®¯°²³´¶·¸¹º»¼½¾¿ÄÀÁÂÃÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ ÿ ";

        /// <summary>
        /// Applies multiple transformation functions to a string
        /// </summary>
        /// <param name="str">The string to transform</param>
        /// <param name="transforms">The functions to operate on the string</param>
        /// <returns>The result string after all transformations have been applied</returns>
        [return: NotNullIfNotNull(nameof(str))]
        public static string? Transform(
            this string? str,
            IEnumerable<Func<string?, string?>> transforms
        ) => transforms.Aggregate(str, (current, func) => func(current));

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

            var invalidChars = text.Distinct().Except(_validChars);
            var fixedText = invalidChars.Aggregate(
                text,
                (current, character) =>
                    current.Replace($"{character}", $"{character}".Transliterate())
            );
            return fixedText;
        }
    }
}
