using ChefRoute.Estructuras;
using ChefRoute.Excepciones;
using ChefRoute.Grafos;
using ChefRoute.Modelos;

namespace ChefRoute.Algoritmos;

public sealed class OrdenadorTopologico
{
    public ListaEnlazadaSimple<Tarea> Ordenar(GrafoDirigido grafoDeTareas)
    {
        ArgumentNullException.ThrowIfNull(grafoDeTareas);

        var resultadoDeteccion = new DetectorCiclos().Detectar(grafoDeTareas);

        if (resultadoDeteccion.TieneCiclo)
        {
            throw new CicloDetectadoException(resultadoDeteccion.Ciclo.ToArray());
        }

        grafoDeTareas.ReiniciarEstados();
        var tareasOrdenadas = new ListaEnlazadaSimple<Tarea>();

        for (var nodoVertice = grafoDeTareas.Vertices.Cabeza; nodoVertice is not null; nodoVertice = nodoVertice.Siguiente)
        {
            if (nodoVertice.Valor.Estado == EstadoVisita.NoVisitado)
            {
                Visitar(nodoVertice.Valor, tareasOrdenadas);
            }
        }

        grafoDeTareas.ReiniciarEstados();
        return tareasOrdenadas;
    }

    private static void Visitar(Vertice verticeActual, ListaEnlazadaSimple<Tarea> tareasOrdenadas)
    {
        verticeActual.Estado = EstadoVisita.Visitado;

        for (var nodoArista = verticeActual.Adyacentes.Cabeza; nodoArista is not null; nodoArista = nodoArista.Siguiente)
        {
            var verticeDestino = nodoArista.Valor.Destino;

            if (verticeDestino.Estado == EstadoVisita.NoVisitado)
            {
                Visitar(verticeDestino, tareasOrdenadas);
            }
        }

        tareasOrdenadas.InsertarInicio(verticeActual.Tarea);
    }
}
