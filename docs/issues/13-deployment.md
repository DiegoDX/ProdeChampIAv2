# Despliegue

## Objetivo
Publicar la aplicación en internet para que el grupo de amigos la use y para mostrarla en el portfolio.

## Decisión pendiente
El proveedor de hosting (backend, base de datos y frontend) se decide al llegar a este issue, comparando costos y opciones gratuitas vigentes en ese momento. Antes de implementar, se propone la opción elegida con su costo mensual estimado y se espera la aprobación del usuario.

## Detalle técnico
- Backend y base de datos SQL Server accesibles en la nube.
- Frontend servido como sitio estático.
- Secretos (cadena de conexión, clave JWT, `Admin:Email`) como variables de entorno del proveedor, nunca en el repo.
- Migraciones aplicadas de forma controlada (no automáticamente en cada arranque).
- CORS configurado para el dominio del frontend publicado.
- Despliegue automático desde `main` con GitHub Actions, solo si el CI pasa.

## Criterios de aceptación
- [ ] La aplicación funciona en una URL pública con HTTPS
- [ ] El README tiene el enlace a la app y una sección "Arquitectura y despliegue"
- [ ] Existe `docs/deployment.md` con los pasos para volver a desplegar desde cero
- [ ] Ningún secreto en el repositorio

## Tests requeridos
- Verificación manual documentada en el PR: registro, crear liga, pronosticar y ver el ranking en la URL pública.

## Fuera de alcance
Dominio propio, monitoreo avanzado y escalado.

## Depende de
Issue 12 (campeón y goleador).
