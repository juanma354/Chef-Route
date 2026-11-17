using ChefRoute.Servicios;
using ChefRoute.Web;

var constructorAplicacion = WebApplication.CreateBuilder(args);

constructorAplicacion.Services.AddSingleton<ServicioChefRoute>();

var aplicacionWeb = constructorAplicacion.Build();

aplicacionWeb.UseDefaultFiles();
aplicacionWeb.UseStaticFiles();

aplicacionWeb.MapEndpointsChefRoute();

aplicacionWeb.Run();
