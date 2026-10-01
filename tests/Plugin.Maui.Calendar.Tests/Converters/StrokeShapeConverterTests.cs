using System.Globalization;
using FluentAssertions;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Plugin.Maui.Calendar.Converters;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Converters;

/// <summary>Verifies <see cref="StrokeShapeConverter"/>, which turns the day corner radius into the cell's shape.</summary>
public class StrokeShapeConverterTests
{
    readonly StrokeShapeConverter converter = new();

    [Fact]
    public void Convert_CornerRadius_GivesARoundRectangleWithThatRadius()
    {
        var shape = converter.Convert(12f, typeof(IShape), null, CultureInfo.InvariantCulture);

        shape.Should().BeOfType<RoundRectangle>()
            .Which.CornerRadius.Should().Be(new Microsoft.Maui.CornerRadius(12));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(12d)]
    [InlineData("12")]
    public void Convert_AnythingButAFloat_GivesASquareRectangle(object? value)
    {
        var shape = converter.Convert(value, typeof(IShape), null, CultureInfo.InvariantCulture);

        shape.Should().BeOfType<RoundRectangle>()
            .Which.CornerRadius.Should().Be(new Microsoft.Maui.CornerRadius(0));
    }

    [Fact]
    public void ConvertBack_DoesNothing()
    {
        converter.ConvertBack(new RoundRectangle(), typeof(float), null, CultureInfo.InvariantCulture)
            .Should().BeSameAs(Binding.DoNothing);
    }
}
