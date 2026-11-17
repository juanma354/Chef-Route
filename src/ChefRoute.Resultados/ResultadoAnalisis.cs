namespace ChefRoute.Resultados;

public sealed record ResultadoAnalisis(
    string Plato,
    bool EsValida,
    string Mensaje,
    string[] Tareas,
    AristaInfo[] Aristas,
    string[] Ciclo,
    string[] OrdenPreparacion,
    PasoRondaInfo[] Ronda,
    int Vueltas);
