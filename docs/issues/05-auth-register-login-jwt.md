# Registro y login con JWT

## Objetivo
Permitir que los usuarios se registren e inicien sesión de forma segura.

## Detalle técnico
Endpoints:
- `POST /auth/register` con `{ email, displayName, password }` → 201 con `{ token, user: { id, email, displayName, isAdmin } }`
- `POST /auth/login` con `{ email, password }` → 200 con la misma respuesta
- `GET /auth/me` (protegido) → datos del usuario autenticado

Reglas:
- Email con formato válido y normalizado en minúsculas; `displayName` de 2 a 40 caracteres; contraseña de al menos 8 caracteres.
- Contraseña guardada con hash usando `PasswordHasher<T>` de ASP.NET Core Identity (solo el hasher, sin el sistema Identity completo).
- Email duplicado → 409 con mensaje en español.
- Credenciales inválidas → 401 con el mismo mensaje sea el email inexistente o la contraseña incorrecta (para no revelar qué emails existen).
- Token JWT con expiración de 60 minutos. Emisor, audiencia y clave de firma se leen de configuración; la clave nunca va en el repo.
- Administrador global: si el email registrado coincide con la configuración `Admin:Email`, el usuario se crea con `IsAdmin = true`. Así el administrador (el dueño del proyecto) se define por configuración. El token incluye un claim de rol `Admin` para ese usuario.
- Errores de validación con el formato estándar `ProblemDetails`, mensajes en español.

## Criterios de aceptación
- [ ] Los tres endpoints funcionan según lo descrito
- [ ] Ninguna contraseña se guarda en texto plano
- [ ] Un endpoint protegido responde 401 sin token o con token vencido
- [ ] Existe una política de autorización `AdminOnly` lista para usar en los issues siguientes
- [ ] Swagger permite probar endpoints protegidos con el token

## Tests requeridos
- Registro correcto devuelve token
- Email duplicado → 409
- Login con contraseña incorrecta → 401
- Login con email inexistente → 401 con el mismo mensaje
- Registro con el email de `Admin:Email` → `IsAdmin = true`
- Validaciones: email inválido y contraseña corta

## Fuera de alcance
Recuperar contraseña, confirmar email, refresh tokens y login con redes sociales.

## Depende de
Issues 2 (entidades) y 4 (integración continua).
