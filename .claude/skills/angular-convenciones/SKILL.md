---
name: angular-convenciones
description: Convenciones del frontend Angular del proyecto Prode (estructura, componentes, servicios, estilos y pruebas). Úsalo al crear o modificar cualquier código dentro de /frontend.
---

# Convenciones del frontend Angular

Aplica estas reglas a todo el código en `/frontend`, además de las de `CLAUDE.md`.

## Estructura
- `src/app/core/`: servicios globales, interceptores, guards y modelos compartidos.
- `src/app/features/<feature>/`: una carpeta por funcionalidad (`auth`, `leagues`, `predictions`, `ranking`), con sus componentes, servicios y rutas.
- `src/app/shared/`: componentes, pipes y utilidades reutilizables sin lógica de negocio.

## Componentes
- Solo componentes *standalone*; sin `NgModule`.
- Rutas de cada feature con carga diferida (`loadComponent` o `loadChildren`).
- Formularios reactivos, nunca formularios por plantilla.
- Componentes de presentación sin llamadas HTTP: reciben datos por `input` y emiten eventos por `output`.
- Usa signals para el estado local y del servicio cuando simplifique el código.

## Servicios y datos
- Toda llamada HTTP vive en un servicio de la feature, nunca en el componente.
- Interfaces TypeScript que reflejan los DTO del backend, en inglés.
- La URL del API sale de `environment`, nunca escrita a mano.
- Las fechas llegan en UTC y se muestran en la zona horaria del navegador.

## Interfaz
- Textos visibles en español; nombres de código en inglés.
- Diseño adaptable a celular primero (375px de ancho como referencia).
- Siempre mostrar estado de carga y mensajes de error en español.

## Pruebas
- Cada servicio con pruebas usando `HttpClient` simulado.
- Componentes con lógica (deshabilitar, resaltar, validar) con al menos una prueba de esa lógica.
- Ejecutar `ng build` y `ng test --watch=false` antes de abrir el PR.
