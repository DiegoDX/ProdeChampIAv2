using Prode.Infrastructure.Persistence;

namespace Prode.UnitTests.Infrastructure;

public class UtcDateTimeConverterTests
{
    private readonly UtcDateTimeConverter _converter = new();

    [Fact]
    public void FromProvider_MarksValueAsUtc()
    {
        var stored = new DateTime(2026, 6, 11, 16, 0, 0, DateTimeKind.Unspecified);

        var result = (DateTime)_converter.ConvertFromProvider(stored)!;

        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(stored.Ticks, result.Ticks);
    }

    [Fact]
    public void ToProvider_KeepsUtcValueUnchanged()
    {
        var utc = new DateTime(2026, 6, 11, 16, 0, 0, DateTimeKind.Utc);

        var result = (DateTime)_converter.ConvertToProvider(utc)!;

        Assert.Equal(utc.Ticks, result.Ticks);
    }

    [Fact]
    public void ToProvider_ConvertsLocalValueToUtc()
    {
        var local = new DateTime(2026, 6, 11, 13, 0, 0, DateTimeKind.Local);

        var result = (DateTime)_converter.ConvertToProvider(local)!;

        Assert.Equal(local.ToUniversalTime().Ticks, result.Ticks);
    }
}
