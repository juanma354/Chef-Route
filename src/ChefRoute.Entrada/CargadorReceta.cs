using System.Text.Json;
using ChefRoute.Excepciones;
using ChefRoute.Grafos;

namespace ChefRoute.Entrada;

public static class CargadorReceta
{
    private static readonly JsonSerializerOptions OpcionesDeserializacionJson = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static RecetaDto DesdeArchivo(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            throw new RecetaInvalidaException("Debes indicar la ruta del archivo JSON.");
        }

        var rutaAbsoluta = ResolverRuta(rutaArchivo.Trim().Trim('"'));

        try
        {
            if (!File.Exists(rutaAbsoluta))
            {
                throw new RecetaInvalidaException($"No se encontró el archivo: {rutaAbsoluta}");
            }

            return DesdeJson(File.ReadAllText(rutaAbsoluta));
        }
        catch (IOException errorDeLectura)
        {
            throw new RecetaInvalidaException($"No se pudo leer el archivo: {errorDeLectura.Message}", errorDeLectura);
        }
        catch (UnauthorizedAccessException errorDePermisos)
        {
            throw new RecetaInvalidaException($"Sin permisos para leer el archivo: {errorDePermisos.Message}", errorDePermisos);
        }
    }

    public static RecetaDto DesdeJson(string contenidoJson)
    {
        if (string.IsNullOrWhiteSpace(contenidoJson))
        {
            throw new RecetaInvalidaException("El JSON está vacío.");
        }

        try
        {
            return JsonSerializer.Deserialize<RecetaDto>(contenidoJson, OpcionesDeserializacionJson)
                ?? throw new RecetaInvalidaException("El JSON no contiene una receta.");
        }
        catch (JsonException errorDeFormato)
        {
            throw new RecetaInvalidaException($"JSON inválido: {errorDeFormato.Message}", errorDeFormato);
        }
    }

    public static GrafoDirigido ConstruirGrafo(RecetaDto receta)
    {
        ArgumentNullException.ThrowIfNull(receta);

        if (receta.Tareas is null || receta.Tareas.Length == 0)
        {
            throw new RecetaInvalidaException("La receta debe incluir al menos una tarea en el campo \"tareas\".");
        }

        var grafoDeTareas = new GrafoDirigido();

        foreach (var nombreTarea in receta.Tareas)
        {
            grafoDeTareas.AgregarTarea(nombreTarea);
        }

        if (receta.Dependencias is not null)
        {
            foreach (var dependencia in receta.Dependencias)
            {
                if (dependencia is null)
                {
                    throw new RecetaInvalidaException("Hay una dependencia vacía en el campo \"dependencias\".");
                }

                grafoDeTareas.AgregarDependencia(dependencia.Desde, dependencia.Hacia, dependencia.TiempoMinutos);
            }
        }

        return grafoDeTareas;
    }

    private static string ResolverRuta(string rutaArchivo)
    {
        if (Path.IsPathRooted(rutaArchivo))
        {
            return rutaArchivo;
        }

        var rutaDesdeDirectorioActual = Path.GetFullPath(rutaArchivo);

        if (File.Exists(rutaDesdeDirectorioActual))
        {
            return rutaDesdeDirectorioActual;
        }

        return Path.Combine(AppContext.BaseDirectory, rutaArchivo);
    }
}
