using ChefRoute.Algoritmos;
using ChefRoute.Entrada;
using ChefRoute.Excepciones;
using ChefRoute.Grafos;
using ChefRoute.Servicios;

namespace ChefRoute.Consola;

internal sealed class MenuConsola
{
    private const string RutaPorDefecto = "Datos/receta_ejemplo.json";

    private RecetaDto? _recetaCargada;
    private GrafoDirigido? _grafoCargado;

    public void Ejecutar()
    {
        var usuarioQuiereSalir = false;

        while (!usuarioQuiereSalir)
        {
            MostrarMenu();
            var opcionElegida = Console.ReadLine()?.Trim();
            Console.WriteLine();

            try
            {
                usuarioQuiereSalir = ProcesarOpcion(opcionElegida);
            }
            catch (RecetaInvalidaException errorDeReceta)
            {
                EscribirConColor($"✖ {errorDeReceta.Message}", ConsoleColor.Red);
            }
            catch (CicloDetectadoException errorDeCiclo)
            {
                EscribirConColor($"✖ {errorDeCiclo.Message}", ConsoleColor.Red);
            }

            if (!usuarioQuiereSalir)
            {
                Console.WriteLine();
                Console.Write("Presiona Enter para continuar...");
                Console.ReadLine();
            }
        }

        Console.WriteLine("¡Buen servicio, chef!");
    }

    private bool ProcesarOpcion(string? opcionElegida)
    {
        switch (opcionElegida)
        {
            case "1":
                CargarReceta();
                break;
            case "2":
                MostrarGrafo();
                break;
            case "3":
                DetectarCiclos();
                break;
            case "4":
                MostrarSecuencia();
                break;
            case "5":
                SimularRonda();
                break;
            case "0":
                return true;
            default:
                EscribirConColor("Opción no válida. Elige un número del menú.", ConsoleColor.Yellow);
                break;
        }

        return false;
    }

    private static void MostrarMenu()
    {
        Console.Clear();
        EscribirConColor("╔══════════════════════════════════════╗", ConsoleColor.DarkYellow);
        EscribirConColor("║            C H E F R O U T E         ║", ConsoleColor.DarkYellow);
        EscribirConColor("╚══════════════════════════════════════╝", ConsoleColor.DarkYellow);
        Console.WriteLine("  1. Cargar receta desde JSON");
        Console.WriteLine("  2. Mostrar grafo (lista de adyacencia)");
        Console.WriteLine("  3. Detectar recetas imposibles (DFS)");
        Console.WriteLine("  4. Secuencia de preparación (orden topológico)");
        Console.WriteLine("  5. Ronda de supervisión (lista circular)");
        Console.WriteLine("  0. Salir");
        Console.WriteLine();
        Console.Write("Elige una opción: ");
    }

    private void CargarReceta()
    {
        Console.Write($"Ruta del JSON [Enter = {RutaPorDefecto}]: ");
        var rutaArchivoJson = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(rutaArchivoJson))
        {
            rutaArchivoJson = RutaPorDefecto;
        }

        var recetaLeida = CargadorReceta.DesdeArchivo(rutaArchivoJson);
        var grafoConstruido = CargadorReceta.ConstruirGrafo(recetaLeida);

        _recetaCargada = recetaLeida;
        _grafoCargado = grafoConstruido;

        EscribirConColor(
            $"✔ Receta \"{recetaLeida.Plato}\" cargada: {grafoConstruido.CantidadVertices} tareas y {grafoConstruido.CantidadAristas} dependencias.",
            ConsoleColor.Green);
    }

    private void MostrarGrafo()
    {
        if (!HayRecetaCargada())
        {
            return;
        }

        Console.WriteLine($"Plato: {_recetaCargada?.Plato}");
        Console.WriteLine("Lista de adyacencia (tarea → [dependiente, minutos]):");
        Console.WriteLine();

        for (var nodoVertice = _grafoCargado!.Vertices.Cabeza; nodoVertice is not null; nodoVertice = nodoVertice.Siguiente)
        {
            Console.Write($"  {nodoVertice.Valor.Tarea.Nombre} → ");

            if (nodoVertice.Valor.Adyacentes.EstaVacia)
            {
                Console.WriteLine("(sin dependientes)");
                continue;
            }

            for (var nodoArista = nodoVertice.Valor.Adyacentes.Cabeza; nodoArista is not null; nodoArista = nodoArista.Siguiente)
            {
                Console.Write($"[{nodoArista.Valor.Destino.Tarea.Nombre}, {nodoArista.Valor.TiempoMinutos} min]");
                Console.Write(nodoArista.Siguiente is null ? Environment.NewLine : " ");
            }
        }
    }

    private void DetectarCiclos()
    {
        if (!HayRecetaCargada())
        {
            return;
        }

        var resultadoDeteccion = new DetectorCiclos().Detectar(_grafoCargado!);

        if (resultadoDeteccion.TieneCiclo)
        {
            EscribirConColor("✖ RECETA IMPOSIBLE: se detectó una dependencia circular.", ConsoleColor.Red);
            Console.WriteLine($"  Ciclo: {string.Join(" → ", resultadoDeteccion.Ciclo)}");
        }
        else
        {
            EscribirConColor("✔ Receta válida: no hay dependencias circulares.", ConsoleColor.Green);
        }
    }

    private void MostrarSecuencia()
    {
        if (!HayRecetaCargada())
        {
            return;
        }

        var ordenPreparacion = new OrdenadorTopologico().Ordenar(_grafoCargado!);

        Console.WriteLine("Secuencia de preparación:");
        var numeroDePaso = 1;

        for (var nodoTarea = ordenPreparacion.Cabeza; nodoTarea is not null; nodoTarea = nodoTarea.Siguiente)
        {
            Console.WriteLine($"  {numeroDePaso++,2}. {nodoTarea.Valor.Nombre}");
        }
    }

    private void SimularRonda()
    {
        if (!HayRecetaCargada())
        {
            return;
        }

        var ordenPreparacion = new OrdenadorTopologico().Ordenar(_grafoCargado!);
        var simuladorRonda = new SimuladorRonda();
        var rondaCircular = simuladorRonda.CargarRonda(ordenPreparacion);

        Console.Write($"¿Cuántas vueltas? [Enter = {ServicioChefRoute.VueltasPorDefecto}]: ");
        var cantidadVueltas = int.TryParse(Console.ReadLine(), out var vueltasIngresadas) && vueltasIngresadas > 0
            ? Math.Min(vueltasIngresadas, ServicioChefRoute.MaximoVueltas)
            : ServicioChefRoute.VueltasPorDefecto;

        Console.WriteLine();

        var pasosSimulados = simuladorRonda.Simular(rondaCircular, cantidadVueltas);

        for (var nodoPaso = pasosSimulados.Cabeza; nodoPaso is not null; nodoPaso = nodoPaso.Siguiente)
        {
            var pasoRonda = nodoPaso.Valor;
            var textoCierreDeVuelta = pasoRonda.Posicion == rondaCircular.Cantidad ? "  ↻ vuelve a la cabeza" : string.Empty;

            Console.WriteLine(
                $"  Paso {pasoRonda.Numero,2} · Vuelta {pasoRonda.Vuelta} · supervisa «{pasoRonda.Estacion.Nombre}» → «{pasoRonda.Siguiente.Nombre}»{textoCierreDeVuelta}");
        }
    }

    private bool HayRecetaCargada()
    {
        if (_grafoCargado is not null)
        {
            return true;
        }

        EscribirConColor("Primero carga una receta con la opción 1.", ConsoleColor.Yellow);
        return false;
    }

    private static void EscribirConColor(string mensaje, ConsoleColor colorDelTexto)
    {
        var colorOriginal = Console.ForegroundColor;
        Console.ForegroundColor = colorDelTexto;
        Console.WriteLine(mensaje);
        Console.ForegroundColor = colorOriginal;
    }
}
