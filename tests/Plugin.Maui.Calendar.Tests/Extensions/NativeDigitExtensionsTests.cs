using System.Globalization;
using FluentAssertions;
using Plugin.Maui.Calendar.Shared.Extensions;
using Xunit;

namespace Plugin.Maui.Calendar.Tests.Extensions;

/// <summary>
/// Verifies <see cref="NativeDigitExtensions"/>, which writes dates and numbers in the digits of a
/// culture. These tests run on .NET without a platform, where the digits come from the culture's
/// <see cref="NumberFormatInfo.NativeDigits"/> (iOS and Mac Catalyst use their own table).
/// </summary>
public class NativeDigitExtensionsTests
{
    static readonly CultureInfo egyptianArabic = new("ar-EG");
    static readonly CultureInfo english = new("en-US");

    [Fact]
    public void Date_InACultureWithNativeDigits_ReplacesOnlyTheDigits()
    {
        new DateTime(2025, 5, 7).ToNativeDigitString("d MMMM yyyy", egyptianArabic).Should().Be("٧ مايو ٢٠٢٥");
    }

    [Fact]
    public void Date_InACultureWithWesternDigits_IsFormattedAsIs()
    {
        new DateTime(2025, 5, 7).ToNativeDigitString("d MMM yyyy", english).Should().Be("7 May 2025");
    }

    [Theory]
    [InlineData(0, "٠")]
    [InlineData(2025, "٢٠٢٥")]
    [InlineData(-31, "-٣١")]
    public void Number_InACultureWithNativeDigits_ReplacesOnlyTheDigits(int number, string expected)
    {
        number.ToNativeDigitString(egyptianArabic).Should().Be(expected);
    }

    [Fact]
    public void Number_InACultureWithWesternDigits_IsWrittenAsIs()
    {
        2025.ToNativeDigitString(english).Should().Be("2025");
    }
}
