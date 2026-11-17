using ChefRoute.Estructuras;
using ChefRoute.Excepciones;
using ChefRoute.Modelos;

namespace ChefRoute.Grafos;

public sealed class GrafoDirigido
{
    public ListaEnlazadaSimple<Vertice> Vertices { get; } = new();

    public int CantidadVertices => Vertices.Cantidad;

    public int CantidadAristas { get; private set; }

    public Vertice AgregarTarea(string? nombreTarea, string descripcionTarea = "", int tiempoEstimadoEnMinutos = 0)
    {
        if (string.IsNullOrWhiteSpace(nombreTarea))
        {
            throw new RecetaInvalidaException("Hay una tarea sin nombre en la receta.");
        }

        var nombreSinEspacios = nombreTarea.Trim();

        if (BuscarVertice(nombreSinEspacios) is not null)
        {
            throw new RecetaInvalidaException($"La tarea \"{nombreSinEspacios}\" está repetida.");
        }

        var tareaNueva = new Tarea(Vertices.Cantidad + 1, nombreSinEspacios, descripcionTarea, tiempoEstimadoEnMinutos);
        var verticeNuevo = new Vertice(tareaNueva);
        Vertices.InsertarFinal(verticeNuevo);
        return verticeNuevo;
    }

    public Vertice? BuscarVertice(string nombreTarea)
    {
        return Vertices.Buscar(vertice => string.Equals(vertice.Tarea.Nombre, nombreTarea.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void AgregarDependencia(string? nombreTareaOrigen, string? nombreTareaDestino, int tiempoEnMinutos)
    {
        if (string.IsNullOrWhiteSpace(nombreTareaOrigen) || string.IsNullOrWhiteSpace(nombreTareaDestino))
        {
            throw new RecetaInvalidaException("Cada dependencia debe tener los campos \"desde\" y \"hacia\".");
        }

        if (tiempoEnMinutos < 0)
        {
            throw new RecetaInvalidaException(
                $"La dependencia \"{nombreTareaOrigen}\" → \"{nombreTareaDestino}\" tiene un tiempo negativo ({tiempoEnMinutos}).");
        }

        var verticeOrigen = BuscarVertice(nombreTareaOrigen)
            ?? throw new RecetaInvalidaException($"La dependencia usa la tarea \"{nombreTareaOrigen}\", que no está en \"tareas\".");

        var verticeDestino = BuscarVertice(nombreTareaDestino)
            ?? throw new RecetaInvalidaException($"La dependencia usa la tarea \"{nombreTareaDestino}\", que no está en \"tareas\".");

        verticeOrigen.Adyacentes.InsertarFinal(new Arista(verticeOrigen, verticeDestino, tiempoEnMinutos));
        CantidadAristas++;
    }

    public void ReiniciarEstados()
    {
        for (var nodoVertice = Vertices.Cabeza; nodoVertice is not null; nodoVertice = nodoVertice.Siguiente)
        {
            nodoVertice.Valor.Estado = EstadoVisita.NoVisitado;
        }
    }
}
