using System.Diagnostics.CodeAnalysis;
using AnyAscii;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Synthesis.Util;

namespace TofuPatcher
{
    /// <summary>
    /// Generic interface to abstract patching text strings in records
    /// </summary>
    /// <typeparam name="TMajor">The record type or category (aspect)</typeparam>
    /// <typeparam name="TMajorGetter">The record/category/aspect getter type</typeparam>
    /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
    public interface IRecordTextPatcher<TMajor, TMajorGetter, TValue>
        : IConditionalTransformPatcher<TMajor, TMajorGetter, FixedText<TValue>>
        where TMajor : TMajorGetter
        where TMajorGetter : IMajorRecordQueryableGetter
        where TValue : notnull
    {
        // Sadly, cannot use default interface method for `ShouldPatch` here...
    }

    /// <summary>
    /// Class to streamline patching multiple record types
    /// </summary>
    /// <param name="patchMod">The mutable mod object to write changes to</param>
    /// <param name="filters">Common filters to apply to each record/context before processing</param>
    public class TextPatcherPipeline(
        ISkyrimMod patchMod,
        params Func<IModContext<IMajorRecordQueryableGetter>, bool>[] filters
    ) : ConditionalTransformPatcherPipeline<ISkyrimMod, ISkyrimModGetter>(patchMod)
    {
        private readonly IEnumerable<
            Func<IModContext<IMajorRecordQueryableGetter>, bool>
        > _filters = filters;

        /// <summary>
        /// Applies multiple filters against a mod context
        /// </summary>
        /// <param name="context">The mod context to check</param>
        /// <returns>True if the context satisfies all of the conditions</returns>
        public bool FilterCommon<TMajorGetter>(IModContext<TMajorGetter> context)
            where TMajorGetter : IMajorRecordQueryableGetter =>
            _filters.All(predicate => predicate((IModContext<IMajorRecordQueryableGetter>)context));

        // TODO: Consider adding pipeline-level pre-filter to base classes
        public new IEnumerable<
            PatchingData<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter, TValue>
        > GetRecordsToPatch<TMajor, TMajorGetter, TValue>(
            IConditionalTransformPatcher<TMajor, TMajorGetter, TValue> patcher,
            IEnumerable<IModContext<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter>> records
        )
            where TMajor : TMajorGetter
            where TMajorGetter : IMajorRecordQueryableGetter
            where TValue : notnull => base.GetRecordsToPatch(patcher, records.Where(FilterCommon));
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
        /// Converts a string that may contain invalid characters to ASCII.
        ///
        /// If there are no invalid characters, the original string is returned unchanged
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

            // No invalid chars, string is unchanged
            if (!invalidChars.Any())
            {
                return text;
            }

            var fixedText = invalidChars.Aggregate(
                text,
                (current, character) =>
                    current.Replace($"{character}", $"{character}".Transliterate())
            );
            return fixedText;
        }
    }
}
