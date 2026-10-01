using System.Reflection;
using FluentAssertions;
using Microsoft.Maui.Controls;
using Plugin.Maui.Calendar.Styles;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Styles;

/// <summary>
/// Verifies the default styles of the calendar. A setter whose value is not of its property's type is
/// dropped by MAUI (with JIT) and crashes a Native AOT app when the style is applied, so every value must
/// already have the property's exact type.
/// </summary>
public class DefaultStylesTests
{
    public static TheoryData<string> Styles =>
        [.. typeof(DefaultStyles).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(Style))
            .Select(property => property.Name)];

    static IEnumerable<Setter> SettersOf(Style? style)
    {
        for (; style is not null; style = style.BasedOn)
        {
            foreach (var setter in style.Setters)
            {
                yield return setter;
            }
        }
    }

    [Theory]
    [MemberData(nameof(Styles))]
    public void EverySetterValue_HasItsPropertysType(string styleName)
    {
        var style = (Style)typeof(DefaultStyles).GetProperty(styleName)!.GetValue(null)!;

        SettersOf(style).Should().NotBeEmpty().And.AllSatisfy(setter =>
            setter.Value.Should().BeAssignableTo(setter.Property.ReturnType,
                $"{styleName} sets {setter.Property.PropertyName}"));
    }

    [Fact]
    public void Styles_AreDiscovered() => Styles.Count.Should().Be(11);
}
