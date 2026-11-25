# Integración continua: build y tests en cada PR

## Objetivo
Que GitHub compile y ejecute los tests automáticamente en cada pull request. Es la red de seguridad del proyecto: un PR en rojo no se acepta.

## Detalle técnico
- Archivo `.github/workflows/ci.yml`
- Se ejecuta en `pull_request` hacia `main` y en `push` a `main`
- Job `backend` en `ubuntu-latest`:
  1. Checkout
  2. `actions/setup-dotnet` con la misma versión del proyecto
  3. `dotnet restore`, `dotnet build --no-restore`, `dotnet test --no-build`
- Los tests del CI no deben necesitar SQL Server. Si más adelante se agregan tests de integración con base de datos, se usará un contenedor de servicio de SQL Server en el workflow (en otro issue).
- El job `frontend` se agrega en el issue del esqueleto de Angular.
- Agregar al README una insignia (badge) con el estado del CI.

## Criterios de aceptación
- [ ] El workflow corre automáticamente al abrir este mismo PR y termina en verde
- [ ] Se comprobó que un test fallido deja el workflow en rojo (en un commit temporal que luego se revierte; se documenta en el PR, sin dejar el test roto)
- [ ] El badge del README muestra el estado
- [ ] La descripción del PR explica cómo marcar el check como obligatorio en la protección de `main` (Settings → Branches), para que el usuario lo active

## Tests requeridos
Ninguno nuevo; este issue ejecuta los existentes.

## Fuera de alcance
Despliegue y análisis de cobertura.

## Depende de
Issue 3 (calculadora de puntos), para que haya tests reales que ejecutar.
