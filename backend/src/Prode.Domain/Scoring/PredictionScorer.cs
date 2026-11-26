namespace Prode.Domain.Scoring;

// Calcula los puntos de los pronósticos. Lógica pura: sin base de datos ni HTTP.
public static class PredictionScorer
{
    // En eliminatorias, el resultado real es el del final del tiempo reglamentario más la prórroga
    // (sin penales), así que un empate es posible y se puntúa igual que en fase de grupos.
    public static int ScoreMatch(int predictedHome, int predictedAway, int actualHome, int actualAway)
    {
        if (predictedHome == actualHome && predictedAway == actualAway)
        {
            return ScoringRules.ExactScore;
        }

        var predictedOutcome = Math.Sign(predictedHome - predictedAway);
        var actualOutcome = Math.Sign(actualHome - actualAway);

        return predictedOutcome == actualOutcome ? ScoringRules.CorrectOutcome : 0;
    }

    // Si todavía no hay campeón real (o no se pronosticó), suma 0.
    public static int ScoreChampion(int? predictedChampionId, int? actualChampionId)
    {
        if (predictedChampionId is null || actualChampionId is null)
        {
            return 0;
        }

        return predictedChampionId == actualChampionId ? ScoringRules.Champion : 0;
    }

    // Si hay goleadores empatados, acertar cualquiera de ellos cuenta.
    // Si todavía no hay goleador real (null o lista vacía), suma 0.
    // Los nombres se comparan sin distinguir mayúsculas e ignorando espacios al inicio y al final.
    public static int ScoreTopScorer(string? predictedTopScorer, IReadOnlyCollection<string>? actualTopScorers)
    {
        if (string.IsNullOrWhiteSpace(predictedTopScorer) || actualTopScorers is null || actualTopScorers.Count == 0)
        {
            return 0;
        }

        var predicted = predictedTopScorer.Trim();
        var hit = actualTopScorers.Any(actual =>
            string.Equals(actual?.Trim(), predicted, StringComparison.OrdinalIgnoreCase));

        return hit ? ScoringRules.TopScorer : 0;
    }

    // Puntos especiales del torneo: campeón más goleador.
    public static int ScoreSpecial(
        int? predictedChampionId,
        int? actualChampionId,
        string? predictedTopScorer,
        IReadOnlyCollection<string>? actualTopScorers)
    {
        return ScoreChampion(predictedChampionId, actualChampionId)
            + ScoreTopScorer(predictedTopScorer, actualTopScorers);
    }
}
