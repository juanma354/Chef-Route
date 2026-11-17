namespace ChefRoute.Excepciones;

public sealed class CicloDetectadoException : Exception
{
    public CicloDetectadoException(string[] nombresTareasDelCiclo)
        : base($"La receta es imposible: dependencia circular {string.Join(" → ", nombresTareasDelCiclo)}.")
    {
        Ciclo = nombresTareasDelCiclo;
    }

    public string[] Ciclo { get; }
}
