using Badeend.ValueCollections;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace TofuPatcher
{
    /// <summary>
    /// DTO object containing all of the text strings that are available in a DialogueInfo record
    /// </summary>
    /// <param name="Responses">The text string for each response in the record</param>
    /// <param name="Prompt">The prompt that triggers the responses</param>
    public readonly record struct DialogueInfoTexts(
        ValueList<string?> Responses,
        string? Prompt = null
    );

    /// <summary>
    /// DTO object containing the original text values, the processed values, and the associcated mod context
    /// </summary>
    /// <typeparam name="TMajor">The record type</typeparam>
    /// <typeparam name="TMajorGetter">The record getter type</typeparam>
    /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
    /// <param name="Context">The mod context for the record</param>
    /// <param name="Original">The original text values of the record</param>
    /// <param name="Fixed">The processed text values</param>
    public readonly record struct FixedDialogue<TMajor, TMajorGetter, TValue>(
        IModContext<ISkyrimMod, ISkyrimModGetter, TMajor, TMajorGetter> Context,
        TValue Original,
        TValue Fixed
    )
        where TMajor : TMajorGetter
        where TMajorGetter : IDialogGetter
        where TValue : notnull
    {
        public bool Equals(FixedDialogue<TMajor, TMajorGetter, TValue> other) =>
            Context.Record.FormKey == other.Context.Record.FormKey
            && Original.Equals(other.Original)
            && Fixed.Equals(other.Fixed);

        public override int GetHashCode() =>
            HashCode.Combine(
                Context.Record.FormKey.GetHashCode(),
                Original.GetHashCode(),
                Fixed.GetHashCode()
            );
    }
}
