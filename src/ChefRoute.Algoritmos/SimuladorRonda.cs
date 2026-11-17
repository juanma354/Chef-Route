using ChefRoute.Estructuras;
using ChefRoute.Modelos;

namespace ChefRoute.Algoritmos;

public sealed class SimuladorRonda
{
    public ListaEnlazadaCircular<Tarea> CargarRonda(ListaEnlazadaSimple<Tarea> ordenPreparacion)
    {
        ArgumentNullException.ThrowIfNull(ordenPreparacion);

        var rondaCircular = new ListaEnlazadaCircular<Tarea>();

        for (var nodoTarea = ordenPreparacion.Cabeza; nodoTarea is not null; nodoTarea = nodoTarea.Siguiente)
        {
            rondaCircular.Agregar(nodoTarea.Valor);
        }

        return rondaCircular;
    }

    public ListaEnlazadaSimple<PasoRonda> Simular(ListaEnlazadaCircular<Tarea> rondaCircular, int cantidadVueltas)
    {
        ArgumentNullException.ThrowIfNull(rondaCircular);
        ArgumentOutOfRangeException.ThrowIfLessThan(cantidadVueltas, 1);

        var pasosSimulados = new ListaEnlazadaSimple<PasoRonda>();

        if (rondaCircular.EstaVacia)
        {
            return pasosSimulados;
        }

        var totalDePasos = rondaCircular.Cantidad * cantidadVueltas;
        var nodoActual = rondaCircular.Cabeza!;

        for (var indicePaso = 0; indicePaso < totalDePasos; indicePaso++)
        {
            var nodoSiguiente = nodoActual.Siguiente!;
            var numeroVuelta = (indicePaso / rondaCircular.Cantidad) + 1;
            var posicionEnRonda = (indicePaso % rondaCircular.Cantidad) + 1;

            pasosSimulados.InsertarFinal(new PasoRonda(indicePaso + 1, numeroVuelta, posicionEnRonda, nodoActual.Valor, nodoSiguiente.Valor));

            nodoActual = nodoSiguiente;
        }

        return pasosSimulados;
    }
}
