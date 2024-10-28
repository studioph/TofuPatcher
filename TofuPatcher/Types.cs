using Badeend.ValueCollections;

namespace TofuPatcher
{
    /// <summary>
    /// DTO object containing all of the text strings that are available in a DialogueInfo record
    /// </summary>
    /// <param name="Responses">The text string for each response in the record</param>
    /// <param name="Prompt">The prompt that triggers the responses</param>
    public sealed record DialogueInfoTexts(string? Prompt, ValueList<string?> Responses);

    public sealed record BookTexts(string? Name, string? BookText);

    /// <summary>
    /// DTO object containing the original text values and the processed values
    /// </summary>
    /// <typeparam name="TValue">The type containing the text values for the record. Could be a single string or another object holding multiple values</typeparam>
    public sealed record FixedText<TValue>(TValue Original, TValue Fixed)
        where TValue : notnull;

