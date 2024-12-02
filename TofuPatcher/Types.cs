using Badeend.ValueCollections;
using Mutagen.Bethesda.Strings;

namespace TofuPatcher
{
    /// <summary>
    /// DTO object containing all of the text strings that are available in a DialogueInfo record
    /// </summary>
    /// <param name="Responses">The text string for each response in the record</param>
    /// <param name="Prompt">The prompt that triggers the responses</param>
    public sealed record DialogueInfoTexts(
        TranslatedStringWrapper Prompt,
        ValueList<string?> Responses
    );

    public sealed record BookTexts(string? Name, string? BookText);

    /// <summary>
    /// DTO object containing the original text values and the processed values
    /// </summary>
    /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
    public sealed record FixedText<TValue>(TValue Original, TValue Fixed)
        where TValue : notnull;

    /// <summary>
    /// Wrapper for TranslatedStrings due to weird operator conversion behavior with nulls
    /// </summary>
    /// <param name="Value">The underlying string value</param>
    public sealed record TranslatedStringWrapper(string? Value)
    {
        public static implicit operator TranslatedStringWrapper(string? str) => new(str);

        public static implicit operator string?(TranslatedStringWrapper wrapper) => wrapper.Value;

        public static implicit operator TranslatedStringWrapper(TranslatedString? translated) =>
            new(translated?.String);

        public static implicit operator TranslatedString?(TranslatedStringWrapper wrapper) =>
            wrapper.Value is null ? (TranslatedString?)null : wrapper.Value; // ?? does not work here even with cast
    }
}
