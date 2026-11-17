namespace ChefRoute.Modelos;

public sealed class Arista
{
    public Arista(Vertice verticeOrigen, Vertice verticeDestino, int tiempoEnMinutos)
    {
        Origen = verticeOrigen;
        Destino = verticeDestino;
        TiempoMinutos = tiempoEnMinutos;
    }

    public Vertice Origen { get; }

    public Vertice Destino { get; }

    public int TiempoMinutos { get; }
}
