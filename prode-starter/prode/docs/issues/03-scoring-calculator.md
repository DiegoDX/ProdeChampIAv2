# Calculadora de puntos (lógica de dominio pura con tests)

## Objetivo
Implementar las reglas de puntaje como lógica pura, sin base de datos ni HTTP. Es el corazón del prode y la pieza más fácil de explicar en una entrevista.

## Reglas
| Acierto | Puntos |
|---|---|
| Resultado exacto | 5 |
| Solo ganador o empate correcto | 3 |
| Nada | 0 |
| Campeón correcto | 10 |
| Goleador correcto | 10 |

- El resultado exacto vale 5 en total; nunca se suma con los 3 del ganador.
- En eliminatorias, el resultado que cuenta es el del final del tiempo reglamentario más la prórroga. Los penales no cuentan. Por eso un partido de eliminatoria puede terminar en empate para el prode.

## Detalle técnico
- En `Prode.Domain`, una clase `ScoringRules` con las constantes de puntos (5, 3, 10, 10), para cambiar los valores en un solo lugar.
- Una clase `PredictionScorer` con:
  - `int ScoreMatch(int predictedHome, int predictedAway, int actualHome, int actualAway)`
  - `int ScoreSpecial(int? predictedChampionId, int? actualChampionId, int? predictedTopScorerId, IReadOnlyCollection<int> actualTopScorerIds)`
- El goleador acepta varios ids reales porque puede haber empate entre goleadores; si el pronóstico coincide con cualquiera, suma.
- Si el valor real todavía no existe (null o lista vacía), esa parte suma 0.

## Criterios de aceptación
- [ ] `ScoringRules` y `PredictionScorer` viven en `Prode.Domain` sin dependencias externas
- [ ] Todas las reglas de la tabla están implementadas
- [ ] Todos los tests listados abajo pasan

## Tests requeridos (usar `[Theory]` con casos)
- Exacto: pronóstico 2-1, real 2-1 → 5
- Solo ganador local: 2-0 vs 3-1 → 3
- Solo ganador visitante: 0-1 vs 1-3 → 3
- Empate no exacto: 1-1 vs 2-2 → 3
- Empate exacto: 0-0 vs 0-0 → 5
- Fallo: 2-1 vs 0-1 → 0
- Fallo de empate: 1-1 vs 2-1 → 0
- Campeón correcto → 10; incorrecto → 0; sin campeón real todavía → 0
- Goleador correcto → 10; con goleadores empatados, elegir cualquiera de ellos → 10
- Campeón y goleador correctos → 20

## Fuera de alcance
Guardar puntos en la base de datos (issue de resultados y ranking) y la pantalla de campeón y goleador.

## Depende de
Issue 1 (esqueleto de la solución).
