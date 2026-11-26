using Prode.Domain.Scoring;

namespace Prode.UnitTests.Domain;

public class PredictionScorerTests
{
    [Theory]
    [InlineData(2, 1, 2, 1, 5)] // Resultado exacto
    [InlineData(2, 0, 3, 1, 3)] // Solo ganador local
    [InlineData(0, 1, 1, 3, 3)] // Solo ganador visitante
    [InlineData(1, 1, 2, 2, 3)] // Empate no exacto
    [InlineData(0, 0, 0, 0, 5)] // Empate exacto
    [InlineData(2, 1, 0, 1, 0)] // Fallo
    [InlineData(1, 1, 2, 1, 0)] // Fallo de empate
    public void ScoreMatch_ReturnsExpectedPoints(
        int predictedHome, int predictedAway, int actualHome, int actualAway, int expected)
    {
        var points = PredictionScorer.ScoreMatch(predictedHome, predictedAway, actualHome, actualAway);

        Assert.Equal(expected, points);
    }

    [Theory]
    [InlineData(7, 7, 10)]     // Campeón correcto
    [InlineData(7, 3, 0)]      // Campeón incorrecto
    [InlineData(7, null, 0)]   // Sin campeón real todavía
    [InlineData(null, 7, 0)]   // Sin pronóstico de campeón
    public void ScoreChampion_ReturnsExpectedPoints(int? predicted, int? actual, int expected)
    {
        var points = PredictionScorer.ScoreChampion(predicted, actual);

        Assert.Equal(expected, points);
    }

    [Theory]
    [InlineData("Messi", new[] { "Messi" }, 10)]                  // Goleador correcto
    [InlineData("Mbappé", new[] { "Messi", "Mbappé" }, 10)]       // Goleadores empatados, elige uno
    [InlineData("Messi", new[] { "Messi", "Mbappé" }, 10)]        // Goleadores empatados, elige el otro
    [InlineData(" messi ", new[] { "Messi" }, 10)]                // Ignora mayúsculas y espacios
    [InlineData("Kane", new[] { "Messi" }, 0)]                    // Goleador incorrecto
    [InlineData("Messi", new string[0], 0)]                       // Sin goleador real todavía (lista vacía)
    [InlineData(null, new[] { "Messi" }, 0)]                      // Sin pronóstico de goleador
    public void ScoreTopScorer_ReturnsExpectedPoints(string? predicted, string[] actual, int expected)
    {
        var points = PredictionScorer.ScoreTopScorer(predicted, actual);

        Assert.Equal(expected, points);
    }

    [Fact]
    public void ScoreTopScorer_WithoutActualTopScorers_ReturnsZero()
    {
        Assert.Equal(0, PredictionScorer.ScoreTopScorer("Messi", null));
    }

    [Theory]
    [InlineData(7, 7, "Messi", 20)] // Campeón y goleador correctos
    [InlineData(7, 7, "Kane", 10)]  // Solo campeón
    [InlineData(7, 3, "Messi", 10)] // Solo goleador
    [InlineData(7, 3, "Kane", 0)]   // Ninguno
    public void ScoreSpecial_SumsChampionAndTopScorer(
        int? predictedChampionId, int? actualChampionId, string predictedTopScorer, int expected)
    {
        var points = PredictionScorer.ScoreSpecial(
            predictedChampionId, actualChampionId, predictedTopScorer, new[] { "Messi" });

        Assert.Equal(expected, points);
    }
}
