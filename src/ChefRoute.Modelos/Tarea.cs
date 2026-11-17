namespace ChefRoute.Modelos;

public sealed class Tarea
{
    public Tarea(int identificador, string nombre, string descripcion = "", int tiempoEstimadoMinutos = 0)
    {
        Id = identificador;
        Nombre = nombre;
        Descripcion = descripcion;
        TiempoEstimadoMinutos = tiempoEstimadoMinutos;
    }

    public int Id { get; }

    public string Nombre { get; }

    public string Descripcion { get; }

    public int TiempoEstimadoMinutos { get; }

    public override string ToString() => Nombre;
}
