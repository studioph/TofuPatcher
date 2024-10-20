using Badeend.ValueCollections;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace TofuPatcher
{
    /// <summary>
    /// Class for patching named records
    /// </summary>
    /// <param name="transforms"></param>
    public class NamedRecordTextPatcher(IEnumerable<Func<string?, string?>> transforms)
        : IRecordTextPatcher<INamed, INamedGetter, string>
    {
        private readonly IEnumerable<Func<string?, string?>> _transforms = transforms;

        public bool Filter(INamedGetter record) => !record.Name.IsNullOrWhitespace();

        public void Patch(string fixedText, INamed target)
        {
            target.Name = fixedText;
            var formKey = ((IMajorRecordGetter)target).FormKey;
            Console.WriteLine($"Patched {formKey}");
        }

        public FixedText<INamed, INamedGetter, string> Process(
            IModContext<ISkyrimMod, ISkyrimModGetter, INamed, INamedGetter> context
        )
        {
            var original = context.Record.Name;
            var processed = original.Transform(_transforms);
            return new FixedText<INamed, INamedGetter, string>(
                context,
                original ?? string.Empty, // Shouldn't ever be null due to pre-filter but need to make compiler happy
                processed ?? string.Empty
            );
        }
    }

    /// <summary>
    /// Class for patching dialogue info records (also called dialog responses)
    /// </summary>
    /// <param name="transforms"></param>
    public class DialogueInfoTextPatcher(IEnumerable<Func<string?, string?>> transforms)
        : IRecordTextPatcher<IDialogResponses, IDialogResponsesGetter, DialogueInfoTexts>
    {
        private readonly IEnumerable<Func<string?, string?>> _transforms = transforms;

        public bool Filter(IDialogResponsesGetter record)
        {
            var hasPrompt = !record.Prompt?.String.IsNullOrWhitespace() ?? false;
            var hasResponses = record.Responses.Any(response =>
                !response.Text.String.IsNullOrWhitespace()
            );

            return hasPrompt || hasResponses;
        }

        public void Patch(DialogueInfoTexts fixedDialogue, IDialogResponses target)
        {
            target.Prompt = fixedDialogue.Prompt;

            for (int i = 0; i < fixedDialogue.Responses.Count; i++)
            {
                target.Responses[i].Text = fixedDialogue.Responses[i];
            }
            Console.WriteLine($"Patched INFO:{target.FormKey}");
        }

        public FixedText<IDialogResponses, IDialogResponsesGetter, DialogueInfoTexts> Process(
            IModContext<
                ISkyrimMod,
                ISkyrimModGetter,
                IDialogResponses,
                IDialogResponsesGetter
            > context
        )
        {
            var prompt = context.Record.Prompt?.String;
            var responses = context.Record.Responses.Select(response => response.Text.String);
            var original = new DialogueInfoTexts(prompt, responses.ToValueList());

            var processed = new DialogueInfoTexts(
                prompt.Transform(_transforms),
                responses.Select(response => response.Transform(_transforms)).ToValueList()
            );

            return new FixedText<IDialogResponses, IDialogResponsesGetter, DialogueInfoTexts>(
                context,
                original,
                processed
            );
        }
    }

