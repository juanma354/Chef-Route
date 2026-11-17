namespace ChefRoute.Entrada;

public sealed class RecetaDto
{
    public string? Plato { get; set; }

    public string[]? Tareas { get; set; }

    public DependenciaDto[]? Dependencias { get; set; }
}
