using System.Collections;

namespace ChefRoute.Estructuras;

public sealed class ListaEnlazadaCircular<T> : IEnumerable<T>
{
    private NodoLista<T>? _nodoUltimo;

    public int Cantidad { get; private set; }

    public bool EstaVacia => Cantidad == 0;

    public NodoLista<T>? Cabeza => _nodoUltimo?.Siguiente;

    public void Agregar(T valorNuevo)
    {
        var nodoNuevo = new NodoLista<T>(valorNuevo);

        if (_nodoUltimo is null)
        {
            nodoNuevo.Siguiente = nodoNuevo;
        }
        else
        {
            nodoNuevo.Siguiente = _nodoUltimo.Siguiente;
            _nodoUltimo.Siguiente = nodoNuevo;
        }

        _nodoUltimo = nodoNuevo;
        Cantidad++;
    }

    public bool Eliminar(T valorAEliminar)
    {
        if (_nodoUltimo is null)
        {
            return false;
        }

        var comparadorDeIgualdad = EqualityComparer<T>.Default;
        var nodoAnterior = _nodoUltimo;
        var nodoActual = _nodoUltimo.Siguiente!;

        for (var nodosRevisados = 0; nodosRevisados < Cantidad; nodosRevisados++)
        {
            if (comparadorDeIgualdad.Equals(nodoActual.Valor, valorAEliminar))
            {
                if (Cantidad == 1)
                {
                    _nodoUltimo = null;
                }
                else
                {
                    nodoAnterior.Siguiente = nodoActual.Siguiente;

                    if (ReferenceEquals(nodoActual, _nodoUltimo))
                    {
                        _nodoUltimo = nodoAnterior;
                    }
                }

                Cantidad--;
                return true;
            }

            nodoAnterior = nodoActual;
            nodoActual = nodoActual.Siguiente!;
        }

        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (_nodoUltimo is null)
        {
            yield break;
        }

        var nodoActual = _nodoUltimo.Siguiente!;

        for (var nodosRecorridos = 0; nodosRecorridos < Cantidad; nodosRecorridos++)
        {
            yield return nodoActual.Valor;
            nodoActual = nodoActual.Siguiente!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
