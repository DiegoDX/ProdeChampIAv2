# Angular: ligas, pronósticos y ranking

## Objetivo
Que el prode se pueda usar de punta a punta desde el navegador. Después de este issue, el grupo de amigos puede jugar.

## Detalle técnico
Pantallas:
- **Crear liga**: nombre y torneo (lista desde `GET /tournaments`); al crear, muestra el código de invitación con un botón para copiarlo.
- **Unirse a una liga**: campo para el código.
- **Detalle de liga** con dos pestañas:
  - **Partidos**: próximos y jugados, agrupados por fecha. En los próximos, dos campos numéricos para el pronóstico y un botón Guardar. En los que ya comenzaron, el pronóstico queda en solo lectura con un candado; en los finalizados se ven el resultado real y los puntos obtenidos. Desde un partido comenzado se pueden ver los pronósticos de los demás.
  - **Ranking**: tabla con posición, nombre, puntos, exactos y aciertos de ganador; la fila del usuario actual resaltada.
- Horas mostradas en la zona horaria del navegador.
- Diseño adaptable a celular (la mayoría lo usará desde el teléfono).
- Estados de carga y mensajes de error visibles en español.

## Criterios de aceptación
- [ ] Flujo completo: crear liga, copiar código, unirse con otro usuario, pronosticar y ver el ranking
- [ ] Un partido comenzado no permite editar el pronóstico desde la interfaz
- [ ] Se ve bien en un ancho de celular (aprox. 375px)

## Tests requeridos
- Componente de pronóstico: deshabilitado cuando el partido comenzó
- Componente de ranking: resalta al usuario actual
- Servicios de ligas y pronósticos con `HttpClient` simulado

## Fuera de alcance
Campeón y goleador, notificaciones y modo oscuro.

## Depende de
Issue 10 (esqueleto de Angular).
