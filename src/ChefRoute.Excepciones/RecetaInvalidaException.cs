namespace ChefRoute.Excepciones;

public sealed class RecetaInvalidaException : Exception
{
    public RecetaInvalidaException(string mensajeDeError) : base(mensajeDeError)
    {
    }

    public RecetaInvalidaException(string mensajeDeError, Exception excepcionOriginal) : base(mensajeDeError, excepcionOriginal)
    {
    }
}
