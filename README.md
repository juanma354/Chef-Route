# ChefRoute (C# .NET 8)

Sistema de coordinación de recetas modelado como **grafo dirigido ponderado**.

## Cómo ejecutarlo

Requisito: **.NET 8 SDK**.

```bash
# Frontend web (abre http://localhost:5080)
dotnet run --project src/ChefRoute.Web

# Versión de consola con menú interactivo
dotnet run --project src/ChefRoute.Consola
```

En el frontend puedes elegir un ejemplo, pegar tu JSON o abrir un archivo `.json`.
Los ejemplos están en la carpeta `Datos/` (`receta_ejemplo.json` y `receta_ciclica.json`).

## Estructura (un proyecto por responsabilidad)

Cada módulo es un proyecto independiente de la solución (sin carpetas de solución):

```
src/
├── ChefRoute.Estructuras/   NodoLista<T>, ListaEnlazadaSimple<T>, ListaEnlazadaCircular<T>
├── ChefRoute.Excepciones/   RecetaInvalidaException, CicloDetectadoException
├── ChefRoute.Modelos/       Tarea, Arista, Vertice, EstadoVisita
├── ChefRoute.Grafos/        GrafoDirigido (lista de adyacencia propia)
├── ChefRoute.Algoritmos/    DetectorCiclos, OrdenadorTopologico, SimuladorRonda
├── ChefRoute.Entrada/       RecetaDto, DependenciaDto, CargadorReceta (JSON + try/catch)
├── ChefRoute.Resultados/    ResultadoAnalisis, AristaInfo, PasoRondaInfo
├── ChefRoute.Servicios/     ServicioChefRoute (orquesta los 3 módulos)
├── ChefRoute.Consola/       Menú interactivo
└── ChefRoute.Web/           API mínima + frontend (wwwroot)
```

Dependencias entre proyectos (sin ciclos):

```
Estructuras, Excepciones, Resultados        (no dependen de nadie)
Modelos      → Estructuras
Grafos       → Estructuras, Excepciones, Modelos
Algoritmos   → Estructuras, Excepciones, Grafos, Modelos
Entrada      → Excepciones, Grafos
Servicios    → Algoritmos, Entrada, Estructuras, Grafos, Modelos, Resultados
Consola      → Algoritmos, Entrada, Excepciones, Grafos, Servicios
Web          → Entrada, Excepciones, Servicios
```

## Recursividad

* `DetectorCiclos.Visitar`: DFS recursivo con 3 estados (NoVisitado, EnProceso, Visitado). El ciclo
  se reconstruye al desenrollar la recursión (A → B → C → A).
* `OrdenadorTopologico.Visitar`: DFS recursivo; cada tarea se inserta al inicio de la
  `ListaEnlazadaSimple<Tarea>` al terminar de explorar sus sucesores (post-orden inverso).

## Cumplimiento de la rúbrica

| Criterio | Dónde |
|---|---|
| Estructuras propias | `ChefRoute.Estructuras` (sin `List<T>` ni `LinkedList<T>`; el único arreglo es `ToArray()` en la frontera de salida JSON) |
| Grafo dirigido | `GrafoDirigido` + `Vertice.Adyacentes` = `ListaEnlazadaSimple<Arista>` con peso en minutos |
| DFS / topológico | `DetectorCiclos`, `OrdenadorTopologico` |
| Lista circular / ronda | `ListaEnlazadaCircular<Tarea>` + `SimuladorRonda` (criterio de salida: nº de vueltas) |
| JSON | `CargadorReceta` (rutas relativas, `try/catch`, validaciones con mensajes claros) |
| Calidad / consola | Un proyecto por módulo, un archivo por clase, nombres descriptivos, `MenuConsola` y frontend web |

## API (la usa el frontend)

* `GET  /api/ejemplos/{ejemplo|ciclica}` → receta de ejemplo
* `POST /api/analizar?vueltas=2` → body: JSON de la receta; responde validez, ciclo, orden de preparación y pasos de la ronda
