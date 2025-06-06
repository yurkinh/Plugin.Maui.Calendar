using FluentAssertions;
using Plugin.Maui.Calendar.Tests.TestableCode.Enums;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Enums;

public class EventIndicatorTypeTests
{
    [Fact]
    public void EventIndicatorType_ShouldHaveExpectedValues()
    {
        // Act & Assert
        Enum.GetValues<EventIndicatorType>().Should().Contain(EventIndicatorType.BottomDot);
        Enum.GetValues<EventIndicatorType>().Should().Contain(EventIndicatorType.TopDot);
        Enum.GetValues<EventIndicatorType>().Should().Contain(EventIndicatorType.Background);
        Enum.GetValues<EventIndicatorType>().Should().Contain(EventIndicatorType.BackgroundFull);
    }

    [Fact]
    public void EventIndicatorType_ShouldHaveCorrectCount()
    {
        // Act
        var values = Enum.GetValues<EventIndicatorType>();

        // Assert
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(EventIndicatorType.BottomDot, 0)]
    [InlineData(EventIndicatorType.TopDot, 1)]
    [InlineData(EventIndicatorType.Background, 2)]
    [InlineData(EventIndicatorType.BackgroundFull, 3)]
    public void EventIndicatorType_ShouldHaveExpectedIntegerValues(EventIndicatorType type, int expectedValue)
    {
        // Act & Assert
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void EventIndicatorType_ShouldBeConvertibleToString()
    {
        // Arrange
        var type = EventIndicatorType.BottomDot;

        // Act
        var stringValue = type.ToString();

        // Assert
        stringValue.Should().Be("BottomDot");
    }

    [Fact]
    public void EventIndicatorType_ShouldSupportParsing()
    {
        // Act & Assert
        Enum.Parse<EventIndicatorType>("BottomDot").Should().Be(EventIndicatorType.BottomDot);
        Enum.Parse<EventIndicatorType>("TopDot").Should().Be(EventIndicatorType.TopDot);
        Enum.Parse<EventIndicatorType>("Background").Should().Be(EventIndicatorType.Background);
        Enum.Parse<EventIndicatorType>("BackgroundFull").Should().Be(EventIndicatorType.BackgroundFull);
    }

    [Fact]
    public void EventIndicatorType_ParseInvalidValue_ShouldThrowException()
    {
        // Act & Assert
        Action act = () => Enum.Parse<EventIndicatorType>("InvalidValue");
        act.Should().Throw<ArgumentException>();
    }
}