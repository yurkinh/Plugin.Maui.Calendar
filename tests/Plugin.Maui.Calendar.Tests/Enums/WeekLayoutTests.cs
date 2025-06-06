using FluentAssertions;
using Plugin.Maui.Calendar.Tests.TestableCode.Enums;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Enums;

public class WeekLayoutTests
{
    [Fact]
    public void WeekLayout_ShouldHaveExpectedValues()
    {
        // Act & Assert
        Enum.GetValues<WeekLayout>().Should().Contain(WeekLayout.Week);
        Enum.GetValues<WeekLayout>().Should().Contain(WeekLayout.TwoWeek);
        Enum.GetValues<WeekLayout>().Should().Contain(WeekLayout.Month);
    }

    [Fact]
    public void WeekLayout_ShouldHaveCorrectCount()
    {
        // Act
        var values = Enum.GetValues<WeekLayout>();

        // Assert
        values.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(WeekLayout.Week, 0)]
    [InlineData(WeekLayout.TwoWeek, 1)]
    [InlineData(WeekLayout.Month, 2)]
    public void WeekLayout_ShouldHaveExpectedIntegerValues(WeekLayout layout, int expectedValue)
    {
        // Act & Assert
        ((int)layout).Should().Be(expectedValue);
    }

    [Fact]
    public void WeekLayout_ShouldBeConvertibleToString()
    {
        // Arrange
        var layout = WeekLayout.Week;

        // Act
        var stringValue = layout.ToString();

        // Assert
        stringValue.Should().Be("Week");
    }

    [Fact]
    public void WeekLayout_ShouldSupportParsing()
    {
        // Act & Assert
        Enum.Parse<WeekLayout>("Week").Should().Be(WeekLayout.Week);
        Enum.Parse<WeekLayout>("TwoWeek").Should().Be(WeekLayout.TwoWeek);
        Enum.Parse<WeekLayout>("Month").Should().Be(WeekLayout.Month);
    }

    [Fact]
    public void WeekLayout_ParseInvalidValue_ShouldThrowException()
    {
        // Act & Assert
        Action act = () => Enum.Parse<WeekLayout>("InvalidValue");
        act.Should().Throw<ArgumentException>();
    }
}