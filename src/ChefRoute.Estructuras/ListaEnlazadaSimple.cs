using System.Collections;

namespace ChefRoute.Estructuras;

public sealed class ListaEnlazadaSimple<T> : IEnumerable<T>
{
    private NodoLista<T>? _nodoCabeza;
    private NodoLista<T>? _nodoCola;

    public int Cantidad { get; private set; }

    public bool EstaVacia => Cantidad == 0;

    public NodoLista<T>? Cabeza => _nodoCabeza;

    public void InsertarInicio(T valorNuevo)
    {
        var nodoNuevo = new NodoLista<T>(valorNuevo) { Siguiente = _nodoCabeza };
        _nodoCabeza = nodoNuevo;
        _nodoCola ??= nodoNuevo;
        Cantidad++;
    }

    public void InsertarFinal(T valorNuevo)
    {
        var nodoNuevo = new NodoLista<T>(valorNuevo);

        if (_nodoCola is null)
        {
            _nodoCabeza = nodoNuevo;
        }
        else
        {
            _nodoCola.Siguiente = nodoNuevo;
        }

        _nodoCola = nodoNuevo;
        Cantidad++;
    }

    public bool Eliminar(T valorAEliminar)
    {
        var comparadorDeIgualdad = EqualityComparer<T>.Default;
        NodoLista<T>? nodoAnterior = null;
        var nodoActual = _nodoCabeza;

        while (nodoActual is not null)
        {
            if (comparadorDeIgualdad.Equals(nodoActual.Valor, valorAEliminar))
            {
                if (nodoAnterior is null)
                {
                    _nodoCabeza = nodoActual.Siguiente;
                }
                else
                {
                    nodoAnterior.Siguiente = nodoActual.Siguiente;
                }

                if (ReferenceEquals(nodoActual, _nodoCola))
                {
                    _nodoCola = nodoAnterior;
                }

                Cantidad--;
                return true;
            }

            nodoAnterior = nodoActual;
            nodoActual = nodoActual.Siguiente;
        }

        return false;
    }

    public T? Buscar(Predicate<T> condicionDeBusqueda)
    {
        var nodoActual = _nodoCabeza;

        while (nodoActual is not null)
        {
            if (condicionDeBusqueda(nodoActual.Valor))
            {
                return nodoActual.Valor;
            }

            nodoActual = nodoActual.Siguiente;
        }

        return default;
    }

    public T[] ToArray()
    {
        var arregloResultado = new T[Cantidad];
        var posicionEnArreglo = 0;

        for (var nodoActual = _nodoCabeza; nodoActual is not null; nodoActual = nodoActual.Siguiente)
        {
            arregloResultado[posicionEnArreglo++] = nodoActual.Valor;
        }

        return arregloResultado;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var nodoActual = _nodoCabeza;

        while (nodoActual is not null)
        {
            yield return nodoActual.Valor;
            nodoActual = nodoActual.Siguiente;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
