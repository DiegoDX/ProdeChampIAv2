# Esqueleto de Angular con login y registro

## Objetivo
Crear el frontend base conectado al backend, con autenticación funcionando.

## Detalle técnico
- Proyecto Angular (versión estable más reciente) en `/frontend`, con componentes *standalone* y rutas.
- Interfaz en español. Nombres de componentes, servicios y variables en inglés.
- Estructura de carpetas:
  - `core/` (servicios globales, interceptores, guards)
  - `features/auth/` (login y registro)
  - `features/leagues/` (lista de ligas)
  - `shared/` (componentes reutilizables)
- `AuthService`: login, registro y logout; guarda el token en `localStorage` y expone el usuario actual.
- Interceptor HTTP que agrega `Authorization: Bearer <token>` y, ante un 401, cierra la sesión y redirige al login.
- `authGuard` que protege las rutas privadas.
- URL del API en los archivos de `environment`.
- Pantallas: login, registro e inicio con "Mis ligas" (usa `GET /leagues`).
- Formularios reactivos con validación y mensajes de error en español.
- En el backend: habilitar CORS para el origen del frontend en desarrollo.
- En el CI (`ci.yml`): agregar el job `frontend` con `npm ci`, build y tests sin modo watch.

## Criterios de aceptación
- [ ] Un usuario puede registrarse, iniciar sesión, ver sus ligas y cerrar sesión
- [ ] Sin sesión, las rutas privadas redirigen al login
- [ ] El job `frontend` del CI corre en verde
- [ ] El README tiene una sección "Ejecutar el frontend"

## Tests requeridos
- `AuthService`: guarda y borra el token
- `authGuard`: permite con sesión y redirige sin sesión
- Interceptor: agrega el encabezado cuando hay token

## Fuera de alcance
Pantallas de ligas, pronósticos y ranking, y diseño visual elaborado.

## Depende de
Issue 9 (resultados y ranking).
