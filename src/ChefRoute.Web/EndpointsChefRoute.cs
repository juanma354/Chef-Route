using ChefRoute.Entrada;
using ChefRoute.Excepciones;
using ChefRoute.Servicios;

namespace ChefRoute.Web;

public static class EndpointsChefRoute
{
    public static void MapEndpointsChefRoute(this WebApplication aplicacionWeb)
    {
        aplicacionWeb.MapGet("/api/ejemplos/{nombreEjemplo}", ObtenerEjemplo);
        aplicacionWeb.MapPost("/api/analizar", AnalizarAsync);
    }

    private static IResult ObtenerEjemplo(string nombreEjemplo)
    {
        var nombreArchivoEjemplo = nombreEjemplo.ToLowerInvariant() switch
        {
            "ejemplo" => "receta_ejemplo.json",
            "ciclica" => "receta_ciclica.json",
            _ => null
        };

        if (nombreArchivoEjemplo is null)
        {
            return Results.NotFound(new { error = $"No existe el ejemplo \"{nombreEjemplo}\"." });
        }

        try
        {
            var recetaDeEjemplo = CargadorReceta.DesdeArchivo(Path.Combine("Datos", nombreArchivoEjemplo));
            return Results.Ok(recetaDeEjemplo);
        }
        catch (RecetaInvalidaException errorDeReceta)
        {
            return Results.Problem(errorDeReceta.Message);
        }
    }

    private static async Task<IResult> AnalizarAsync(HttpRequest peticionHttp, ServicioChefRoute servicioChefRoute, int? vueltas)
    {
        try
        {
            using var lectorDelCuerpo = new StreamReader(peticionHttp.Body);
            var contenidoJson = await lectorDelCuerpo.ReadToEndAsync();

            var receta = CargadorReceta.DesdeJson(contenidoJson);
            var resultadoAnalisis = servicioChefRoute.Analizar(receta, vueltas ?? ServicioChefRoute.VueltasPorDefecto);

            return Results.Ok(resultadoAnalisis);
        }
        catch (RecetaInvalidaException errorDeReceta)
        {
            return Results.BadRequest(new { error = errorDeReceta.Message });
        }
    }
}
