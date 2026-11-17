using ChefRoute.Estructuras;

namespace ChefRoute.Algoritmos;

public sealed class ResultadoCiclo
{
    public ResultadoCiclo(bool tieneCiclo, ListaEnlazadaSimple<string> nombresTareasDelCiclo)
    {
        TieneCiclo = tieneCiclo;
        Ciclo = nombresTareasDelCiclo;
    }

    public bool TieneCiclo { get; }

    public ListaEnlazadaSimple<string> Ciclo { get; }
}
