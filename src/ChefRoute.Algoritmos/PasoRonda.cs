using ChefRoute.Modelos;

namespace ChefRoute.Algoritmos;

public sealed record PasoRonda(int Numero, int Vuelta, int Posicion, Tarea Estacion, Tarea Siguiente);
