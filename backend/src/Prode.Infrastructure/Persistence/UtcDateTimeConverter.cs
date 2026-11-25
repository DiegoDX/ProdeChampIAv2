using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Prode.Infrastructure.Persistence;

// Guarda las fechas en UTC y, al leerlas, las marca como UTC
// (SQL Server no guarda el DateTimeKind y EF las devolvería como Unspecified).
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc),
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
