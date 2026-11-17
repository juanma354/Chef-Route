namespace ChefRoute.Estructuras;

public sealed class NodoLista<T>
{
    public NodoLista(T valorAlmacenado)
    {
        Valor = valorAlmacenado;
    }

    public T Valor { get; set; }

    public NodoLista<T>? Siguiente { get; set; }
}
