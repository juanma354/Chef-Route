using ChefRoute.Estructuras;
using ChefRoute.Grafos;
using ChefRoute.Modelos;

namespace ChefRoute.Algoritmos;

public sealed class DetectorCiclos
{
    private ListaEnlazadaSimple<string> _nombresTareasDelCiclo = new();
    private Vertice? _verticeDondeIniciaElCiclo;
    private bool _cicloCompleto;

    public ResultadoCiclo Detectar(GrafoDirigido grafoDeTareas)
    {
        ArgumentNullException.ThrowIfNull(grafoDeTareas);

        grafoDeTareas.ReiniciarEstados();
        _nombresTareasDelCiclo = new ListaEnlazadaSimple<string>();
        _verticeDondeIniciaElCiclo = null;
        _cicloCompleto = false;

        for (var nodoVertice = grafoDeTareas.Vertices.Cabeza; nodoVertice is not null; nodoVertice = nodoVertice.Siguiente)
        {
            if (nodoVertice.Valor.Estado == EstadoVisita.NoVisitado && Visitar(nodoVertice.Valor))
            {
                grafoDeTareas.ReiniciarEstados();
                return new ResultadoCiclo(true, _nombresTareasDelCiclo);
            }
        }

        grafoDeTareas.ReiniciarEstados();
        return new ResultadoCiclo(false, new ListaEnlazadaSimple<string>());
    }

    private bool Visitar(Vertice verticeActual)
    {
        verticeActual.Estado = EstadoVisita.EnProceso;

        for (var nodoArista = verticeActual.Adyacentes.Cabeza; nodoArista is not null; nodoArista = nodoArista.Siguiente)
        {
            var verticeDestino = nodoArista.Valor.Destino;

            if (verticeDestino.Estado == EstadoVisita.EnProceso)
            {
                RegistrarCiclo(verticeActual, verticeDestino);
                return true;
            }

            if (verticeDestino.Estado == EstadoVisita.NoVisitado && Visitar(verticeDestino))
            {
                ExtenderCiclo(verticeActual);
                return true;
            }
        }

        verticeActual.Estado = EstadoVisita.Visitado;
        return false;
    }

    private void RegistrarCiclo(Vertice verticeOrigen, Vertice verticeDestino)
    {
        _nombresTareasDelCiclo.InsertarFinal(verticeDestino.Tarea.Nombre);
        _nombresTareasDelCiclo.InsertarInicio(verticeOrigen.Tarea.Nombre);
        _verticeDondeIniciaElCiclo = verticeDestino;
        _cicloCompleto = ReferenceEquals(verticeOrigen, verticeDestino);
    }

    private void ExtenderCiclo(Vertice verticeActual)
    {
        if (_cicloCompleto)
        {
            return;
        }

        _nombresTareasDelCiclo.InsertarInicio(verticeActual.Tarea.Nombre);

        if (ReferenceEquals(verticeActual, _verticeDondeIniciaElCiclo))
        {
            _cicloCompleto = true;
        }
    }
}
