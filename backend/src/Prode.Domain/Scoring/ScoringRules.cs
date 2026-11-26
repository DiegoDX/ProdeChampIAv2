namespace Prode.Domain.Scoring;

// Valores de puntaje del prode, centralizados para cambiarlos en un solo lugar.
public static class ScoringRules
{
    // Resultado exacto. No se suma con el de ganador o empate.
    public const int ExactScore = 5;

    // Solo ganador o empate correcto.
    public const int CorrectOutcome = 3;

    public const int Champion = 10;

    public const int TopScorer = 10;
}
