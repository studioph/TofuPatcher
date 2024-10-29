using FluentAssertions;

namespace TofuPatcher.Tests
{
    public class TextUtilTests
    {
        [Fact]
        public void ToAscii_InputIsNull_ReturnsNull()
        {
            TextUtil.ToAscii(null).Should().BeNull();
        }

        [Fact]
        public void ToAscii_InputIsValid_ReturnsUnchanged()
        {
            string validString = "hello";
            validString.ToAscii().Should().BeSameAs(validString);
        }

        [Fact]
        public void ToAscii_InputHasInvalidChars_ReturnsValidChars()
        {
            string invalidString = "‘hello’";
            string expected = "'hello'";

            invalidString.ToAscii().Should().Be(expected);
        }

        [Fact]
        public void Transform_ReturnsUnchangedIfNoFunctions()
        {
            string original = "hello";
            original.Transform([]).Should().BeSameAs(original);
        }

        [Fact]
        public void Transform_ReturnsUnchangedIfFunctionsReturnUnchanged()
        {
            string original = "hello";
            original.Transform([str => str?.Trim()]).Should().BeSameAs(original);
        }

        [Fact]
        public void Transform_AppliesAllFunctions()
        {
            string original = " hello ";
            string expected = "HELLO";
            Func<string?, string?>[] transforms = [str => str?.ToUpper(), str => str?.Trim()];

            original.Transform(transforms).Should().Be(expected);
        }
    }
}
