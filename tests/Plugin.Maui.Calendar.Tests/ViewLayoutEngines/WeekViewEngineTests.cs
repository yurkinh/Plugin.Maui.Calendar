using FluentAssertions;
using Plugin.Maui.Calendar.Controls.ViewLayoutEngines;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.ViewLayoutEngines;

public class WeekViewEngineTests
{
	[Fact]
	public void GetLastDate_ShouldBeThirteenDaysAfterFirstDate_ForTwoWeekLayout()
	{
		// Arrange
		var engine = new WeekViewEngine(2, DayOfWeek.Sunday);
		var shownDate = new DateTime(2024, 5, 15);

		// Act
		var firstDate = engine.GetFirstDate(shownDate);
		var lastDate = engine.GetLastDate(shownDate);

		// Assert
		lastDate.Should().Be(firstDate.AddDays(13));
	}

	[Fact]
	public void GetLastDate_ShouldClampToDateTimeMaxValueDate()
	{
		// Arrange
		var engine = new WeekViewEngine(2, DayOfWeek.Sunday);

		// Act
		var lastDate = engine.GetLastDate(DateTime.MaxValue);

		// Assert
		lastDate.Should().Be(DateTime.MaxValue.Date);
	}
}
