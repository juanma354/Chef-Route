using ChefRoute.Algoritmos;
using ChefRoute.Entrada;
using ChefRoute.Estructuras;
using ChefRoute.Grafos;
using ChefRoute.Modelos;
using ChefRoute.Resultados;

namespace ChefRoute.Servicios;

public sealed class ServicioChefRoute
{
    public const int VueltasPorDefecto = 2;
    public const int MaximoVueltas = 10;

    public ResultadoAnalisis Analizar(RecetaDto receta, int cantidadVueltas = VueltasPorDefecto)
    {
        ArgumentNullException.ThrowIfNull(receta);

        var grafoDeTareas = CargadorReceta.ConstruirGrafo(receta);
        var nombrePlato = string.IsNullOrWhiteSpace(receta.Plato) ? "Receta sin nombre" : receta.Plato.Trim();

        return Analizar(grafoDeTareas, nombrePlato, cantidadVueltas);
    }

    public ResultadoAnalisis Analizar(GrafoDirigido grafoDeTareas, string nombrePlato, int cantidadVueltas = VueltasPorDefecto)
    {
        ArgumentNullException.ThrowIfNull(grafoDeTareas);

        cantidadVueltas = Math.Clamp(cantidadVueltas, 1, MaximoVueltas);

        var nombresTareas = ObtenerNombresTareas(grafoDeTareas);
        var dependenciasInfo = ObtenerDependencias(grafoDeTareas);

        var resultadoCiclo = new DetectorCiclos().Detectar(grafoDeTareas);

        if (resultadoCiclo.TieneCiclo)
        {
            var nombresTareasDelCiclo = resultadoCiclo.Ciclo.ToArray();

            return new ResultadoAnalisis(
                nombrePlato,
                EsValida: false,
                Mensaje: $"Receta imposible: dependencia circular {string.Join(" → ", nombresTareasDelCiclo)}.",
                nombresTareas,
                dependenciasInfo,
                nombresTareasDelCiclo,
                Array.Empty<string>(),
                Array.Empty<PasoRondaInfo>(),
                cantidadVueltas);
        }

        var ordenPreparacion = new OrdenadorTopologico().Ordenar(grafoDeTareas);
        var simuladorRonda = new SimuladorRonda();
        var rondaCircular = simuladorRonda.CargarRonda(ordenPreparacion);
        var pasosSimulados = simuladorRonda.Simular(rondaCircular, cantidadVueltas);

        return new ResultadoAnalisis(
            nombrePlato,
            EsValida: true,
            Mensaje: "Receta válida: no hay dependencias circulares.",
            nombresTareas,
            dependenciasInfo,
            Array.Empty<string>(),
            ConvertirOrden(ordenPreparacion),
            ConvertirPasos(pasosSimulados),
            cantidadVueltas);
    }

    private static string[] ObtenerNombresTareas(GrafoDirigido grafoDeTareas)
    {
        var nombresTareas = new string[grafoDeTareas.CantidadVertices];
        var posicionEnArreglo = 0;

        for (var nodoVertice = grafoDeTareas.Vertices.Cabeza; nodoVertice is not null; nodoVertice = nodoVertice.Siguiente)
        {
            nombresTareas[posicionEnArreglo++] = nodoVertice.Valor.Tarea.Nombre;
        }

        return nombresTareas;
    }

    private static AristaInfo[] ObtenerDependencias(GrafoDirigido grafoDeTareas)
    {
        var dependenciasEncontradas = new ListaEnlazadaSimple<AristaInfo>();

        for (var nodoVertice = grafoDeTareas.Vertices.Cabeza; nodoVertice is not null; nodoVertice = nodoVertice.Siguiente)
        {
            for (var nodoArista = nodoVertice.Valor.Adyacentes.Cabeza; nodoArista is not null; nodoArista = nodoArista.Siguiente)
            {
                dependenciasEncontradas.InsertarFinal(new AristaInfo(
                    nodoArista.Valor.Origen.Tarea.Nombre,
                    nodoArista.Valor.Destino.Tarea.Nombre,
                    nodoArista.Valor.TiempoMinutos));
            }
        }

        return dependenciasEncontradas.ToArray();
    }

    private static string[] ConvertirOrden(ListaEnlazadaSimple<Tarea> ordenPreparacion)
    {
        var nombresTareasOrdenadas = new string[ordenPreparacion.Cantidad];
        var posicionEnArreglo = 0;

        for (var nodoTarea = ordenPreparacion.Cabeza; nodoTarea is not null; nodoTarea = nodoTarea.Siguiente)
        {
            nombresTareasOrdenadas[posicionEnArreglo++] = nodoTarea.Valor.Nombre;
        }

        return nombresTareasOrdenadas;
    }

    private static PasoRondaInfo[] ConvertirPasos(ListaEnlazadaSimple<PasoRonda> pasosSimulados)
    {
        var pasosConvertidos = new PasoRondaInfo[pasosSimulados.Cantidad];
        var posicionEnArreglo = 0;

        for (var nodoPaso = pasosSimulados.Cabeza; nodoPaso is not null; nodoPaso = nodoPaso.Siguiente)
        {
            var pasoRonda = nodoPaso.Valor;

            pasosConvertidos[posicionEnArreglo++] = new PasoRondaInfo(
                pasoRonda.Numero,
                pasoRonda.Vuelta,
                pasoRonda.Posicion,
                pasoRonda.Estacion.Nombre,
                pasoRonda.Siguiente.Nombre);
        }

        return pasosConvertidos;
    }
}
