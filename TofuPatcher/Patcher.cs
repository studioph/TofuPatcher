using Badeend.ValueCollections;
using Mutagen.Bethesda.Plugins.Aspects;
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

        public FixedText<string> Apply(INamedGetter record)
        {
            var original = record.Name;
            var processed = original.Transform(_transforms);
            return new FixedText<string>(
                // Shouldn't ever be null due to pre-filter but need to make compiler happy
                original ?? string.Empty,
                processed ?? string.Empty
            );
        }

        public bool Filter(INamedGetter record) => !record.Name.IsNullOrWhitespace();

        public void Patch(INamed target, FixedText<string> fixedText) =>
            target.Name = fixedText.Fixed;
    }

    /// <summary>
    /// Class for patching dialogue info records (also called dialog responses)
    /// </summary>
    /// <param name="transforms"></param>
    public class DialogueInfoTextPatcher(IEnumerable<Func<string?, string?>> transforms)
        : IRecordTextPatcher<IDialogResponses, IDialogResponsesGetter, DialogueInfoTexts>
    {
        private readonly IEnumerable<Func<string?, string?>> _transforms = transforms;

        public FixedText<DialogueInfoTexts> Apply(IDialogResponsesGetter record)
        {
            var prompt = record.Prompt?.String;
            var responses = record.Responses.Select(response => response.Text.String);
            var original = new DialogueInfoTexts(prompt, responses.ToValueList());

            var processed = new DialogueInfoTexts(
                prompt.Transform(_transforms),
                responses.Select(response => response.Transform(_transforms)).ToValueList()
            );

            return new FixedText<DialogueInfoTexts>(original, processed);
        }

        public bool Filter(IDialogResponsesGetter record)
        {
            var hasPrompt = !record.Prompt?.String.IsNullOrWhitespace() ?? false;
            var hasResponses = record.Responses.Any(response =>
                !response.Text.String.IsNullOrWhitespace()
            );

            return hasPrompt || hasResponses;
        }

        public void Patch(IDialogResponses target, FixedText<DialogueInfoTexts> values)
        {
            var fixedDialogue = values.Fixed;
            target.Prompt = fixedDialogue.Prompt;

            for (int i = 0; i < fixedDialogue.Responses.Count; i++)
            {
                target.Responses[i].Text = fixedDialogue.Responses[i];
            }
        }
    }

    /// <summary>
    /// Class for patching Book records
    /// </summary>
    /// <param name="transforms"></param>
    public class BookTextPatcher(IEnumerable<Func<string?, string?>> transforms)
        : IRecordTextPatcher<IBook, IBookGetter, BookTexts>
    {
        private readonly IEnumerable<Func<string?, string?>> _transforms = transforms;

        public FixedText<BookTexts> Apply(IBookGetter record)
        {
            var original = new BookTexts(record.Name?.String, record.BookText.String);

            var processed = new BookTexts(
                original.Name.Transform(_transforms),
                original.BookText.Transform(_transforms)
            );

            return new FixedText<BookTexts>(original, processed);
        }

        public bool Filter(IBookGetter book) =>
            !((INamedGetter)book).Name.IsNullOrWhitespace()
            || !book.BookText.String.IsNullOrWhitespace();

        public void Patch(IBook target, FixedText<BookTexts> values)
        {
            var fixedTexts = values.Fixed;
            target.Name = fixedTexts.Name;
            target.BookText = fixedTexts.BookText;
        }
    }
}
