# Prode

Aplicación web para pronósticos de fútbol entre amigos (prode). Un grupo crea una liga sobre un torneo (Mundial, Copa América), cada uno pronostica los partidos, elige campeón y goleador, y gana quien más puntos acumule.

## Stack
- Backend: .NET (LTS), C#, EF Core, SQL Server, en /backend
- Frontend: Angular, en /frontend
- Tests: xUnit (backend)

## Idioma y convenciones
- Código en inglés: nombres de clases, entidades, propiedades, métodos, variables, tablas y columnas de la base de datos (ej. `User`, `League`, `Match`, `Prediction`).
- Comentarios en el código, README, documentación, issues y descripciones de PR en español.
- Mensajes de commit en inglés, en imperativo y cortos (ej. "Add scoring calculator").
- Responde siempre en español al conversar conmigo, aunque el código esté en inglés.
- Los mensajes que ve el usuario final de la app, en español.

## Arquitectura (backend)
Clean Architecture con cuatro proyectos: Domain, Application, Infrastructure, Api.
- Domain no depende de nadie.
- Application depende solo de Domain.
- Infrastructure implementa las interfaces definidas en Application.
- Api solo orquesta; sin lógica de negocio en controladores.
- La hora actual se obtiene con `TimeProvider` inyectado, nunca con `DateTime.UtcNow` directo.

## Reglas del prode (lógica de dominio pura, sin BD ni HTTP)
- Resultado exacto: 5 puntos (no se suma con el de ganador).
- Solo ganador o empate correcto: 3 puntos.
- Campeón correcto: 10 puntos. Goleador correcto: 10 puntos (si hay goleadores empatados, cualquiera cuenta).
- Los pronósticos de un partido se cierran al comenzar el partido (comparar en UTC).
- Campeón y goleador se cierran al comenzar el primer partido del torneo.
- En eliminatorias cuenta el resultado al final del tiempo reglamentario más la prórroga; los penales no cuentan.
- Desempate del ranking: más resultados exactos, luego más aciertos de ganador; si persiste, comparten posición (1, 1, 3).
- Administrador global único, definido por la configuración `Admin:Email`. Solo él carga torneos y resultados.

## Forma de trabajo
- Un issue = una rama = un PR. Rama: feature/<n>-<descripcion-corta>.
- El PR debe cerrar su issue ("Closes #n") y explicar qué se hizo y por qué.
- Todo cambio de lógica lleva tests. No hacer merge con el CI en rojo.
- Nunca borrar, desactivar ni debilitar un test para que pase.
- No agregar funcionalidad fuera del alcance del issue.
- Nunca guardar contraseñas, claves ni cadenas de conexión reales en el repo.

## Skills del proyecto
- `nuevo-issue`: redactar y crear un issue a partir de una idea.
- `escribir-tests`: escribir los tests de un issue antes del código.
- `implementar-issue`: implementar un issue y abrir el PR.
- `revisar-pr`: revisar un PR (usar en una sesión nueva, no en la que escribió el código).
- `actualizar-docs`: actualizar documentación después de un merge.
- `angular-convenciones`: convenciones del frontend (desde el issue del esqueleto de Angular).
