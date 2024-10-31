using FluentAssertions;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;

namespace TofuPatcher.Tests;

public sealed record NamedGetter(string? Name) : INamedGetter
{
    string INamedRequiredGetter.Name => Name ?? string.Empty;
}

public class NamedPatcherTests
{
    private static readonly NamedRecordTextPatcher _patcher =
        new([TextUtil.ToAscii, str => str?.Trim()]);
    public static object[][] FilterTestData =
    [
        [new NamedGetter(null), false],
        [new NamedGetter(string.Empty), false],
        [new NamedGetter("   "), false],
        [new NamedGetter(" hello "), true],
        [new NamedGetter("‘hello’"), true],
    ];

    [Theory]
    [MemberData(nameof(FilterTestData))]
    public void Filter_ShouldExcludeBlankNames(INamedGetter record, bool expected)
    {
        _patcher.Filter(record).Should().Be(expected);
    }

    public static object[][] ApplyTestData =
    [
        [new NamedGetter(null), new FixedText<string>(string.Empty, string.Empty)],
        [new NamedGetter(string.Empty), new FixedText<string>(string.Empty, string.Empty)],
        [new NamedGetter("   "), new FixedText<string>("   ", string.Empty)],
        [new NamedGetter(" hello "), new FixedText<string>(" hello ", "hello")],
        [new NamedGetter("‘hello’"), new FixedText<string>("‘hello’", "'hello'")],
    ];

    [Theory]
    [MemberData(nameof(ApplyTestData))]
    public void Apply_ShouldApplyTransformations(INamedGetter record, FixedText<string> expected)
    {
        _patcher.Apply(record).Should().Be(expected);
    }
}

public class DialogueInfoPatcherTests
{
    private static readonly DialogueInfoTextPatcher _patcher =
        new([TextUtil.ToAscii, str => str?.Trim()]);

    public static object[][] FilterTestData =
    [
        [Factory(null, []), false],
        [Factory(null, [" Response1 "]), true],
        [Factory(" Prompt ", []), true],
        [Factory("‘Prompt", ["Response1’"]), true],
    ];

    [Theory]
    [MemberData(nameof(FilterTestData))]
    public void Filter_ShouldExcludeEmptyPromptAndNoResponses(
        IDialogResponsesGetter record,
        bool expected
    )
    {
        _patcher.Filter(record).Should().Be(expected);
    }

    public static object[][] ApplyTestData =
    [
        [
            Factory(null, []),
            new FixedText<DialogueInfoTexts>(
                new DialogueInfoTexts(null, []),
                new DialogueInfoTexts(null, [])
            ),
        ],
        [
            Factory(null, [" Response1 "]),
            new FixedText<DialogueInfoTexts>(
                new DialogueInfoTexts(null, [" Response1 "]),
                new DialogueInfoTexts(null, ["Response1"])
            ),
        ],
        [
            Factory(" Prompt ", []),
            new FixedText<DialogueInfoTexts>(
                new DialogueInfoTexts(" Prompt ", []),
                new DialogueInfoTexts("Prompt", [])
            ),
        ],
        [
            Factory("‘Prompt", ["Response1’"]),
            new FixedText<DialogueInfoTexts>(
                new DialogueInfoTexts("‘Prompt", ["Response1’"]),
                new DialogueInfoTexts("'Prompt", ["Response1'"])
            ),
        ],
    ];

    [Theory]
    [MemberData(nameof(ApplyTestData))]
    public void Apply_ShouldTransformPromptAndResponses(
        IDialogResponsesGetter record,
        FixedText<DialogueInfoTexts> expected
    )
    {
        _patcher.Apply(record).Should().Be(expected);
    }

    public static object[][] PatchTestData =
    [
        [
            Factory(null, [" Response1 "]),
            new FixedText<DialogueInfoTexts>(
                new DialogueInfoTexts(null, [" Response1 "]),
                new DialogueInfoTexts(null, ["Response1"])
            ),
            Factory(null, ["Response1"]),
        ],
        [
            Factory(" Prompt ", []),
            new FixedText<DialogueInfoTexts>(
                new DialogueInfoTexts(" Prompt ", []),
                new DialogueInfoTexts("Prompt", [])
            ),
            Factory("Prompt", []),
        ],
        [
            Factory("‘Prompt", ["Response1’"]),
            new FixedText<DialogueInfoTexts>(
                new DialogueInfoTexts("‘Prompt", ["Response1’"]),
                new DialogueInfoTexts("'Prompt", ["Response1'"])
            ),
            Factory("'Prompt", ["Response1'"]),
        ],
    ];

    [Theory]
    [MemberData(nameof(PatchTestData))]
    public void Patch_ShouldMatchResponses(
        IDialogResponses target,
        FixedText<DialogueInfoTexts> values,
        IDialogResponsesGetter expected
    )
    {
        _patcher.Patch(target, values);
        target.Should().Be(expected);
    }

    private static IDialogResponses Factory(string? prompt, IEnumerable<string?> responses)
    {
        DialogResponses record =
            new(FormKey.Factory("123456:Test.esp"), SkyrimRelease.SkyrimSE) { Prompt = prompt };
        foreach (var responseText in responses)
        {
            DialogResponse response = new() { Text = responseText };
            record.Responses.Add(response);
        }
        return record;
    }
}
