namespace Prode.Application.Tournaments;

// Mensajes que ve el usuario final.
public static class TournamentMessages
{
    public const string ValidationFailed = "El archivo del torneo tiene datos inválidos. No se guardó nada.";
    public const string NameRequired = "El nombre del torneo es obligatorio.";
    public const string NameTooLong = "El nombre del torneo no puede superar los 100 caracteres.";
    public const string TeamsRequired = "El torneo debe tener al menos dos equipos.";
    public const string MatchesRequired = "El torneo debe tener al menos un partido.";
    public const string TeamNameRequired = "El nombre del equipo es obligatorio.";
    public const string TeamNameTooLong = "El nombre del equipo no puede superar los 100 caracteres.";
    public const string TeamCodeInvalid = "El código del equipo debe tener exactamente 3 letras (ej. ARG).";
    public const string MatchTeamCodeRequired = "El código del equipo del partido es obligatorio.";
    public const string TeamPlaysItself = "Un equipo no puede jugar contra sí mismo.";
    public const string KickoffNotUtc = "La fecha del partido debe estar en UTC con formato ISO 8601 (ej. 2026-06-15T19:00:00Z).";
    public const string StageInvalid = "La fase del partido debe ser 'Group' o 'Knockout'.";
    public const string TournamentNotFound = "El torneo no existe.";
    public const string MatchStatusInvalid = "El estado debe ser 'Scheduled' o 'Finished'.";

    public static string DuplicateTeamCode(string code) => $"El código de equipo '{code}' está repetido en el archivo.";
    public static string DuplicateTeamName(string name) => $"El equipo '{name}' está repetido en el archivo.";
    public static string UnknownTeamCode(string code) => $"El código de equipo '{code}' no corresponde a ningún equipo del archivo.";
}
