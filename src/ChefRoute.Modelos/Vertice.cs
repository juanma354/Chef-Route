using ChefRoute.Estructuras;

namespace ChefRoute.Modelos;

public sealed class Vertice
{
    public Vertice(Tarea tareaAsociada)
    {
        Tarea = tareaAsociada;
    }

    public Tarea Tarea { get; }

    public ListaEnlazadaSimple<Arista> Adyacentes { get; } = new();

    public EstadoVisita Estado { get; set; } = EstadoVisita.NoVisitado;
}
