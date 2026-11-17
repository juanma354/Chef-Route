import { crearElementoSvg, vaciarContenido, acortarTexto } from './dom.js';

const ALTO_NODO = 38;
const SEPARACION_HORIZONTAL = 90;
const SEPARACION_VERTICAL = 26;
const MARGEN = 28;
const LONGITUD_MAXIMA_NOMBRE = 26;

const calcularAnchoNodo = (nombreTarea) =>
  Math.min(210, Math.max(96, acortarTexto(nombreTarea, LONGITUD_MAXIMA_NOMBRE).length * 7.6 + 26));

const crearClaveDeArista = (nombreOrigen, nombreDestino) => `${nombreOrigen}\u0000${nombreDestino}`;

export function dibujarGrafo(contenedorGrafo, nombresTareas, dependencias, nombresCiclo, recetaEsValida) {
  vaciarContenido(contenedorGrafo);
  if (nombresTareas.length === 0) return;

  const anchosPorTarea = new Map(nombresTareas.map((nombreTarea) => [nombreTarea, calcularAnchoNodo(nombreTarea)]));
  const disposicionNodos = recetaEsValida
    ? disponerPorNiveles(nombresTareas, dependencias, anchosPorTarea)
    : disponerEnCirculo(nombresTareas, anchosPorTarea);

  const clavesAristasDelCiclo = new Set();
  for (let indiceTarea = 0; indiceTarea < nombresCiclo.length - 1; indiceTarea++) {
    clavesAristasDelCiclo.add(crearClaveDeArista(nombresCiclo[indiceTarea], nombresCiclo[indiceTarea + 1]));
  }
  const tareasDelCiclo = new Set(nombresCiclo);

  const lienzoSvg = crearElementoSvg('svg', {
    viewBox: `0 0 ${disposicionNodos.ancho} ${disposicionNodos.alto}`,
    role: 'img',
    'aria-label': 'Grafo dirigido de tareas y dependencias'
  });

  lienzoSvg.append(definirMarcadoresDeFlecha());

  const capaAristas = crearElementoSvg('g');
  const capaNodos = crearElementoSvg('g');
  lienzoSvg.append(capaAristas, capaNodos);

  for (const dependencia of dependencias) {
    const posicionOrigen = disposicionNodos.posiciones.get(dependencia.desde);
    const posicionDestino = disposicionNodos.posiciones.get(dependencia.hacia);
    if (!posicionOrigen || !posicionDestino) continue;

    const perteneceAlCiclo = clavesAristasDelCiclo.has(crearClaveDeArista(dependencia.desde, dependencia.hacia));
    const trazoDeArista = recetaEsValida
      ? crearCurvaHorizontal(posicionOrigen, posicionDestino)
      : crearCurvaCircular(posicionOrigen, posicionDestino);

    capaAristas.append(
      crearElementoSvg('path', {
        d: trazoDeArista.datosRuta,
        class: perteneceAlCiclo ? 'arista arista--ciclo' : 'arista',
        'marker-end': perteneceAlCiclo ? 'url(#flecha-ciclo)' : 'url(#flecha)'
      }),
      crearElementoSvg(
        'text',
        { x: trazoDeArista.posicionEtiqueta.x, y: trazoDeArista.posicionEtiqueta.y, class: 'peso' },
        `${dependencia.tiempoMinutos} min`
      )
    );
  }

  for (const nombreTarea of nombresTareas) {
    const posicionTarea = disposicionNodos.posiciones.get(nombreTarea);
    capaNodos.append(
      crearElementoSvg(
        'g',
        {
          class: tareasDelCiclo.has(nombreTarea) ? 'nodo nodo--ciclo' : 'nodo',
          transform: `translate(${posicionTarea.x} ${posicionTarea.y})`
        },
        crearElementoSvg('title', {}, nombreTarea),
        crearElementoSvg('rect', {
          x: -posicionTarea.ancho / 2,
          y: -posicionTarea.alto / 2,
          width: posicionTarea.ancho,
          height: posicionTarea.alto,
          rx: 10
        }),
        crearElementoSvg('text', {}, acortarTexto(nombreTarea, LONGITUD_MAXIMA_NOMBRE))
      )
    );
  }

  contenedorGrafo.append(lienzoSvg);
}

function definirMarcadoresDeFlecha() {
  const crearMarcadorFlecha = (idMarcador, claseCss) =>
    crearElementoSvg(
      'marker',
      { id: idMarcador, viewBox: '0 0 10 10', refX: 9, refY: 5, markerWidth: 7, markerHeight: 7, orient: 'auto-start-reverse' },
      crearElementoSvg('path', { d: 'M0 0 L10 5 L0 10 z', class: claseCss })
    );
  return crearElementoSvg('defs', {}, crearMarcadorFlecha('flecha', 'flecha'), crearMarcadorFlecha('flecha-ciclo', 'flecha flecha--ciclo'));
}

function disponerPorNiveles(nombresTareas, dependencias, anchosPorTarea) {
  const nivelPorTarea = new Map(nombresTareas.map((nombreTarea) => [nombreTarea, 0]));

  for (let iteracion = 0; iteracion < nombresTareas.length; iteracion++) {
    let huboCambioDeNivel = false;
    for (const { desde, hacia } of dependencias) {
      const nivelCandidato = nivelPorTarea.get(desde) + 1;
      if (nivelCandidato > nivelPorTarea.get(hacia)) {
        nivelPorTarea.set(hacia, nivelCandidato);
        huboCambioDeNivel = true;
      }
    }
    if (!huboCambioDeNivel) break;
  }

  const columnas = [];
  for (const nombreTarea of nombresTareas) {
    const nivelDeTarea = nivelPorTarea.get(nombreTarea);
    (columnas[nivelDeTarea] ??= []).push(nombreTarea);
  }

  const calcularAltoColumna = (columna) => columna.length * ALTO_NODO + (columna.length - 1) * SEPARACION_VERTICAL;
  const altoMaximoColumna = Math.max(...columnas.map(calcularAltoColumna));
  const posiciones = new Map();
  let posicionXColumna = MARGEN;

  for (const columna of columnas) {
    const anchoColumna = Math.max(...columna.map((nombreTarea) => anchosPorTarea.get(nombreTarea)));
    const posicionYInicial = MARGEN + (altoMaximoColumna - calcularAltoColumna(columna)) / 2;

    columna.forEach((nombreTarea, indiceEnColumna) => {
      posiciones.set(nombreTarea, {
        x: posicionXColumna + anchoColumna / 2,
        y: posicionYInicial + indiceEnColumna * (ALTO_NODO + SEPARACION_VERTICAL) + ALTO_NODO / 2,
        ancho: anchosPorTarea.get(nombreTarea),
        alto: ALTO_NODO
      });
    });

    posicionXColumna += anchoColumna + SEPARACION_HORIZONTAL;
  }

  return {
    posiciones,
    ancho: posicionXColumna - SEPARACION_HORIZONTAL + MARGEN,
    alto: altoMaximoColumna + MARGEN * 2
  };
}

function crearCurvaHorizontal(posicionOrigen, posicionDestino) {
  const xSalida = posicionOrigen.x + posicionOrigen.ancho / 2;
  const ySalida = posicionOrigen.y;
  const xLlegada = posicionDestino.x - posicionDestino.ancho / 2;
  const yLlegada = posicionDestino.y;
  const desplazamientoControl = Math.max(30, (xLlegada - xSalida) / 2);

  return {
    datosRuta: `M ${xSalida} ${ySalida} C ${xSalida + desplazamientoControl} ${ySalida}, ${xLlegada - desplazamientoControl} ${yLlegada}, ${xLlegada} ${yLlegada}`,
    posicionEtiqueta: { x: (xSalida + xLlegada) / 2, y: (ySalida + yLlegada) / 2 - 4 }
  };
}

function disponerEnCirculo(nombresTareas, anchosPorTarea) {
  const cantidadTareas = nombresTareas.length;
  const anchoMaximoNodo = Math.max(...nombresTareas.map((nombreTarea) => anchosPorTarea.get(nombreTarea)));
  const radioHorizontal = Math.max(150, cantidadTareas * 32);
  const radioVertical = Math.max(110, cantidadTareas * 22);
  const centroX = MARGEN + anchoMaximoNodo / 2 + radioHorizontal;
  const centroY = MARGEN + 30 + radioVertical;
  const posiciones = new Map();

  nombresTareas.forEach((nombreTarea, indiceTarea) => {
    const anguloEnRadianes = -Math.PI / 2 + (2 * Math.PI * indiceTarea) / cantidadTareas;
    posiciones.set(nombreTarea, {
      x: centroX + radioHorizontal * Math.cos(anguloEnRadianes),
      y: centroY + radioVertical * Math.sin(anguloEnRadianes),
      ancho: anchosPorTarea.get(nombreTarea),
      alto: ALTO_NODO
    });
  });

  return {
    posiciones,
    ancho: centroX * 2,
    alto: centroY + radioVertical + ALTO_NODO / 2 + MARGEN
  };
}

function calcularPuntoEnBorde(posicionNodo, destinoX, destinoY) {
  const diferenciaX = destinoX - posicionNodo.x;
  const diferenciaY = destinoY - posicionNodo.y;
  if (diferenciaX === 0 && diferenciaY === 0) return { x: posicionNodo.x, y: posicionNodo.y };

  const escalaEnX = diferenciaX !== 0 ? posicionNodo.ancho / 2 / Math.abs(diferenciaX) : Infinity;
  const escalaEnY = diferenciaY !== 0 ? posicionNodo.alto / 2 / Math.abs(diferenciaY) : Infinity;
  const escalaMinima = Math.min(escalaEnX, escalaEnY);
  return { x: posicionNodo.x + diferenciaX * escalaMinima, y: posicionNodo.y + diferenciaY * escalaMinima };
}

function crearCurvaCircular(posicionOrigen, posicionDestino) {
  if (posicionOrigen === posicionDestino) {
    const yBordeSuperior = posicionOrigen.y - posicionOrigen.alto / 2;
    return {
      datosRuta: `M ${posicionOrigen.x - 12} ${yBordeSuperior} C ${posicionOrigen.x - 34} ${yBordeSuperior - 44}, ${posicionOrigen.x + 34} ${yBordeSuperior - 44}, ${posicionOrigen.x + 12} ${yBordeSuperior}`,
      posicionEtiqueta: { x: posicionOrigen.x, y: yBordeSuperior - 36 }
    };
  }

  const puntoMedioX = (posicionOrigen.x + posicionDestino.x) / 2;
  const puntoMedioY = (posicionOrigen.y + posicionDestino.y) / 2;
  const diferenciaX = posicionDestino.x - posicionOrigen.x;
  const diferenciaY = posicionDestino.y - posicionOrigen.y;
  const distanciaEntreNodos = Math.hypot(diferenciaX, diferenciaY) || 1;
  const puntoControlX = puntoMedioX + (-diferenciaY / distanciaEntreNodos) * 26;
  const puntoControlY = puntoMedioY + (diferenciaX / distanciaEntreNodos) * 26;

  const puntoInicio = calcularPuntoEnBorde(posicionOrigen, puntoControlX, puntoControlY);
  const puntoFin = calcularPuntoEnBorde(posicionDestino, puntoControlX, puntoControlY);

  return {
    datosRuta: `M ${puntoInicio.x} ${puntoInicio.y} Q ${puntoControlX} ${puntoControlY} ${puntoFin.x} ${puntoFin.y}`,
    posicionEtiqueta: {
      x: 0.25 * puntoInicio.x + 0.5 * puntoControlX + 0.25 * puntoFin.x,
      y: 0.25 * puntoInicio.y + 0.5 * puntoControlY + 0.25 * puntoFin.y - 4
    }
  };
}
