using FluentAssertions;
using Plugin.Maui.Calendar.Tests.TestableCode.Models;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Models;

public class ShownDatesChangedEventArgsTests
{
	[Fact]
	public void Constructor_ShouldSetVisibleDates()
	{
		// Arrange
		var startDate = new DateTime(2024, 5, 1, 8, 30, 0);
		var endDate = new DateTime(2024, 5, 31, 17, 45, 0);

		// Act
		var eventArgs = new ShownDatesChangedEventArgs(startDate, endDate);

		// Assert
		eventArgs.VisibleStartDate.Should().Be(startDate.Date);
		eventArgs.VisibleEndDate.Should().Be(endDate.Date);
	}

	[Fact]
	public void Constructor_ShouldHandleSameDateValues()
	{
		// Arrange
		var shownDate = new DateTime(2024, 6, 15, 12, 0, 0);

		// Act
		var eventArgs = new ShownDatesChangedEventArgs(shownDate, shownDate);

		// Assert
		eventArgs.VisibleStartDate.Should().Be(shownDate.Date);
		eventArgs.VisibleEndDate.Should().Be(shownDate.Date);
	}
}
