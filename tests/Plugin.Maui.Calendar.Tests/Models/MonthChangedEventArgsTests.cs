using FluentAssertions;
using Plugin.Maui.Calendar.Tests.TestableCode.Models;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Models;

public class MonthChangedEventArgsTests
{
    [Fact]
    public void Constructor_ShouldSetOldAndNewMonth()
    {
        // Arrange
        var oldMonth = new DateOnly(2024, 1, 1);
        var newMonth = new DateOnly(2024, 2, 1);

        // Act
        var eventArgs = new MonthChangedEventArgs(oldMonth, newMonth);

        // Assert
        eventArgs.OldMonth.Should().Be(oldMonth);
        eventArgs.NewMonth.Should().Be(newMonth);
    }

    [Fact]
    public void Constructor_ShouldHandleSameMonthValues()
    {
        // Arrange
        var month = new DateOnly(2024, 6, 1);

        // Act
        var eventArgs = new MonthChangedEventArgs(month, month);

        // Assert
        eventArgs.OldMonth.Should().Be(month);
        eventArgs.NewMonth.Should().Be(month);
    }

    [Fact]
    public void Constructor_ShouldHandleYearTransition()
    {
        // Arrange
        var oldMonth = new DateOnly(2023, 12, 1);
        var newMonth = new DateOnly(2024, 1, 1);

        // Act
        var eventArgs = new MonthChangedEventArgs(oldMonth, newMonth);

        // Assert
        eventArgs.OldMonth.Should().Be(oldMonth);
        eventArgs.NewMonth.Should().Be(newMonth);
        eventArgs.OldMonth.Year.Should().Be(2023);
        eventArgs.NewMonth.Year.Should().Be(2024);
    }

    [Theory]
    [InlineData(2024, 1, 2024, 2)]
    [InlineData(2023, 12, 2024, 1)]
    [InlineData(2024, 6, 2024, 6)]
    public void Constructor_ShouldWorkWithVariousMonthCombinations(int oldYear, int oldMonth, int newYear, int newMonth)
    {
        // Arrange
        var oldMonthDate = new DateOnly(oldYear, oldMonth, 1);
        var newMonthDate = new DateOnly(newYear, newMonth, 1);

        // Act
        var eventArgs = new MonthChangedEventArgs(oldMonthDate, newMonthDate);

        // Assert
        eventArgs.OldMonth.Should().Be(oldMonthDate);
        eventArgs.NewMonth.Should().Be(newMonthDate);
    }
}