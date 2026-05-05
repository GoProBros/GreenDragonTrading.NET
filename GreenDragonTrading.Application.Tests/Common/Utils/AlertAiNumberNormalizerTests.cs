using GreenDragonTrading.Application.Common.Utils;
using Xunit;

namespace GreenDragonTrading.Application.Tests.Common.Utils;

public class AlertAiNumberNormalizerTests
{
    [Theory]
    [InlineData("75", "75000")]
    [InlineData("75.75", "75750")]
    [InlineData("75,75", "75750")]
    [InlineData("757", "757000")]
    [InlineData("7575", "7575")]
    [InlineData("Gia 75", "Gia 75000")]
    [InlineData("Gia 75%", "Gia 75%")]
    public void NormalizeMessage_ShouldScaleUnderFourDigits(string input, string expected)
    {
        var result = AlertAiNumberNormalizer.NormalizeMessage(input);
        Assert.Equal(expected, result);
    }
}
