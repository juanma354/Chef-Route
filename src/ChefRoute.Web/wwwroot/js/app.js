import { analizar, cargarEjemplo } from './api.js';
import { dibujarGrafo } from './vistaGrafo.js';
import { VistaRonda } from './vistaRonda.js';
import { crearElemento, vaciarContenido } from './dom.js';

const obtenerPorId = (idElemento) => document.getElementById(idElemento);

const elementos = {
  selectorEjemplo: obtenerPorId('selectorEjemplo'),
  entradaArchivoJson: obtenerPorId('entradaArchivoJson'),
  campoVueltas: obtenerPorId('campoVueltas'),
  areaTextoJson: obtenerPorId('areaTextoJson'),
  botonAnalizar: obtenerPorId('botonAnalizar'),
  mensajeDeError: obtenerPorId('mensajeDeError'),
  panelResultados: obtenerPorId('panelResultados'),
  nombrePlato: obtenerPorId('nombrePlato'),
  insigniaEstado: obtenerPorId('insigniaEstado'),
  mensajeEstado: obtenerPorId('mensajeEstado'),
  listaCiclo: obtenerPorId('listaCiclo'),
  contenedorGrafo: obtenerPorId('contenedorGrafo'),
  seccionOrden: obtenerPorId('seccionOrden'),
  listaOrden: obtenerPorId('listaOrden'),
  seccionRonda: obtenerPorId('seccionRonda'),
  botonReproducir: obtenerPorId('botonReproducir'),
  botonSiguientePaso: obtenerPorId('botonSiguientePaso'),
  botonReiniciar: obtenerPorId('botonReiniciar'),
  selectorVelocidad: obtenerPorId('selectorVelocidad'),
  mensajeEstadoRonda: obtenerPorId('mensajeEstadoRonda'),
  lienzoRonda: obtenerPorId('lienzoRonda'),
  bitacoraRonda: obtenerPorId('bitacoraRonda')
};

const vistaRonda = new VistaRonda(
  elementos.lienzoRonda,
  elementos.bitacoraRonda,
  elementos.mensajeEstadoRonda,
  (estaReproduciendo) => {
    elementos.botonReproducir.textContent = estaReproduciendo ? '⏸ Pausar' : '▶ Reproducir';
  }
);

function mostrarError(textoError) {
  elementos.mensajeDeError.textContent = textoError;
  elementos.mensajeDeError.hidden = false;
}

function limpiarError() {
  elementos.mensajeDeError.hidden = true;
  elementos.mensajeDeError.textContent = '';
}

function obtenerCantidadVueltasSeleccionada() {
  const vueltasIngresadas = Number.parseInt(elementos.campoVueltas.value, 10);
  return Number.isNaN(vueltasIngresadas) ? 2 : Math.min(10, Math.max(1, vueltasIngresadas));
}

async function cargarYAnalizarEjemplo(nombreEjemplo) {
  try {
    limpiarError();
    elementos.areaTextoJson.value = await cargarEjemplo(nombreEjemplo);
    await ejecutarAnalisis();
  } catch (error) {
    mostrarError(error.message);
  }
}

async function ejecutarAnalisis() {
  limpiarError();
  elementos.botonAnalizar.disabled = true;

  try {
    const resultadoAnalisis = await analizar(elementos.areaTextoJson.value, obtenerCantidadVueltasSeleccionada());
    mostrarResultado(resultadoAnalisis);
  } catch (error) {
    vistaRonda.detener();
    elementos.panelResultados.hidden = true;
    mostrarError(error.message);
  } finally {
    elementos.botonAnalizar.disabled = false;
  }
}

function mostrarResultado(resultadoAnalisis) {
  elementos.panelResultados.hidden = false;

  elementos.nombrePlato.textContent = resultadoAnalisis.plato;
  elementos.insigniaEstado.textContent = resultadoAnalisis.esValida ? '✔ Receta válida' : '✖ Receta imposible';
  elementos.insigniaEstado.className = `insignia ${resultadoAnalisis.esValida ? 'insignia--ok' : 'insignia--mal'}`;
  elementos.mensajeEstado.textContent = resultadoAnalisis.mensaje;

  mostrarCiclo(resultadoAnalisis.ciclo);
  dibujarGrafo(
    elementos.contenedorGrafo,
    resultadoAnalisis.tareas,
    resultadoAnalisis.aristas,
    resultadoAnalisis.ciclo,
    resultadoAnalisis.esValida
  );

  elementos.seccionOrden.hidden = !resultadoAnalisis.esValida;
  elementos.seccionRonda.hidden = !resultadoAnalisis.esValida;

  if (resultadoAnalisis.esValida) {
    mostrarOrden(resultadoAnalisis.ordenPreparacion);
    vistaRonda.cargar(resultadoAnalisis.ordenPreparacion, resultadoAnalisis.ronda);
  } else {
    vistaRonda.detener();
  }
}

function mostrarCiclo(nombresTareasDelCiclo) {
  vaciarContenido(elementos.listaCiclo);
  elementos.listaCiclo.hidden = nombresTareasDelCiclo.length === 0;

  nombresTareasDelCiclo.forEach((nombreTarea, indiceTarea) => {
    if (indiceTarea > 0) {
      elementos.listaCiclo.append(crearElemento('li', { class: 'chips__flecha', 'aria-hidden': 'true' }, '→'));
    }
    elementos.listaCiclo.append(crearElemento('li', {}, nombreTarea));
  });
}

function mostrarOrden(nombresTareasOrdenadas) {
  vaciarContenido(elementos.listaOrden);
  for (const nombreTarea of nombresTareasOrdenadas) {
    elementos.listaOrden.append(crearElemento('li', {}, nombreTarea));
  }
}

elementos.botonAnalizar.addEventListener('click', ejecutarAnalisis);

elementos.areaTextoJson.addEventListener('keydown', (eventoTeclado) => {
  if ((eventoTeclado.ctrlKey || eventoTeclado.metaKey) && eventoTeclado.key === 'Enter') {
    eventoTeclado.preventDefault();
    ejecutarAnalisis();
  }
});

elementos.selectorEjemplo.addEventListener('change', () => cargarYAnalizarEjemplo(elementos.selectorEjemplo.value));

elementos.entradaArchivoJson.addEventListener('change', async () => {
  const archivoSeleccionado = elementos.entradaArchivoJson.files?.[0];
  if (!archivoSeleccionado) return;

  try {
    elementos.areaTextoJson.value = await archivoSeleccionado.text();
    await ejecutarAnalisis();
  } catch {
    mostrarError('No se pudo leer el archivo seleccionado.');
  } finally {
    elementos.entradaArchivoJson.value = '';
  }
});

elementos.botonReproducir.addEventListener('click', () => vistaRonda.alternar());
elementos.botonSiguientePaso.addEventListener('click', () => {
  vistaRonda.detener();
  vistaRonda.siguiente();
});
elementos.botonReiniciar.addEventListener('click', () => vistaRonda.reiniciar());
elementos.selectorVelocidad.addEventListener('change', () => vistaRonda.fijarVelocidad(Number(elementos.selectorVelocidad.value)));

cargarYAnalizarEjemplo(elementos.selectorEjemplo.value);
