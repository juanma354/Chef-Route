import { crearElementoSvg, crearElemento, vaciarContenido, acortarTexto } from './dom.js';

const ANCHO_LIENZO = 760;
const ALTO_LIENZO = 520;
const CENTRO_ANILLO = { x: ANCHO_LIENZO / 2, y: ALTO_LIENZO / 2 };
const RADIO_ANILLO = 170;
const RADIO_NODO = 18;
const LONGITUD_MAXIMA_NOMBRE = 26;

export class VistaRonda {
  #contenedorLienzo;
  #listaBitacora;
  #elementoEstado;
  #alCambiarEstadoReproduccion;

  #nombresEstaciones = [];
  #pasosRonda = [];
  #gruposEstacionPorNombre = new Map();
  #flechasEntreEstaciones = [];
  #iconoChef = null;
  #indicePasoActual = -1;
  #idTemporizador = null;
  #milisegundosPorPaso = 900;

  constructor(contenedorLienzo, listaBitacora, elementoEstado, alCambiarEstadoReproduccion) {
    this.#contenedorLienzo = contenedorLienzo;
    this.#listaBitacora = listaBitacora;
    this.#elementoEstado = elementoEstado;
    this.#alCambiarEstadoReproduccion = alCambiarEstadoReproduccion;
  }

  get reproduciendo() {
    return this.#idTemporizador !== null;
  }

  cargar(nombresEstaciones, pasosRonda) {
    this.detener();
    this.#nombresEstaciones = nombresEstaciones;
    this.#pasosRonda = pasosRonda;
    this.#dibujarRonda();
    this.reiniciar();
  }

  fijarVelocidad(milisegundosPorPaso) {
    this.#milisegundosPorPaso = milisegundosPorPaso;
    if (this.reproduciendo) {
      this.#detenerTemporizador();
      this.#iniciarTemporizador();
    }
  }

  alternar() {
    if (this.reproduciendo) {
      this.detener();
      return;
    }
    if (this.#indicePasoActual >= this.#pasosRonda.length - 1) this.reiniciar();
    this.#iniciarTemporizador();
    this.#alCambiarEstadoReproduccion(true);
    this.siguiente();
  }

  detener() {
    this.#detenerTemporizador();
    this.#alCambiarEstadoReproduccion(false);
  }

  reiniciar() {
    this.detener();
    this.#indicePasoActual = -1;
    vaciarContenido(this.#listaBitacora);
    this.#resaltarPasoActual(null);
    this.#elementoEstado.textContent = this.#pasosRonda.length
      ? `Listo para empezar: ${this.#nombresEstaciones.length} estaciones, ${this.#obtenerTotalVueltas()} vueltas.`
      : 'No hay pasos para simular.';
  }

  siguiente() {
    if (this.#indicePasoActual + 1 >= this.#pasosRonda.length) {
      this.detener();
      this.#elementoEstado.textContent = `Ronda terminada tras ${this.#obtenerTotalVueltas()} vueltas.`;
      return false;
    }

    this.#indicePasoActual++;
    const pasoActual = this.#pasosRonda[this.#indicePasoActual];
    this.#resaltarPasoActual(pasoActual);

    const cierraLaVuelta = pasoActual.posicion === this.#nombresEstaciones.length;
    this.#listaBitacora.append(
      crearElemento(
        'li',
        {},
        `Paso ${pasoActual.numero} · Vuelta ${pasoActual.vuelta}: supervisa «${pasoActual.estacion}» → siguiente «${pasoActual.siguiente}»`,
        cierraLaVuelta ? crearElemento('div', { class: 'vuelta' }, '↻ El último nodo apunta de nuevo a la cabeza') : null
      )
    );
    this.#listaBitacora.scrollTop = this.#listaBitacora.scrollHeight;
    this.#elementoEstado.textContent = `Vuelta ${pasoActual.vuelta} de ${this.#obtenerTotalVueltas()} · Paso ${pasoActual.numero} de ${this.#pasosRonda.length}`;

    return true;
  }

  #obtenerTotalVueltas() {
    return this.#pasosRonda.length ? this.#pasosRonda[this.#pasosRonda.length - 1].vuelta : 0;
  }

  #iniciarTemporizador() {
    this.#idTemporizador = setInterval(() => {
      if (!this.siguiente()) this.#detenerTemporizador();
    }, this.#milisegundosPorPaso);
  }

  #detenerTemporizador() {
    if (this.#idTemporizador !== null) {
      clearInterval(this.#idTemporizador);
      this.#idTemporizador = null;
    }
  }

  #resaltarPasoActual(pasoActual) {
    for (const grupoEstacion of this.#gruposEstacionPorNombre.values()) grupoEstacion.classList.remove('estacion--activa');
    for (const flecha of this.#flechasEntreEstaciones) flecha.classList.remove('cuerda--activa');

    if (!pasoActual || !this.#iconoChef) {
      if (this.#iconoChef) this.#iconoChef.style.opacity = '0';
      return;
    }

    this.#gruposEstacionPorNombre.get(pasoActual.estacion)?.classList.add('estacion--activa');
    this.#flechasEntreEstaciones[pasoActual.posicion - 1]?.classList.add('cuerda--activa');

    const puntoEstacion = this.#calcularPuntoEnAnillo(pasoActual.posicion - 1);
    this.#iconoChef.style.opacity = '1';
    this.#iconoChef.style.transform = `translate(${puntoEstacion.x}px, ${puntoEstacion.y - RADIO_NODO - 8}px)`;
  }

  #calcularPuntoEnAnillo(indiceEstacion) {
    const anguloEnRadianes = -Math.PI / 2 + (2 * Math.PI * indiceEstacion) / this.#nombresEstaciones.length;
    return {
      x: CENTRO_ANILLO.x + RADIO_ANILLO * Math.cos(anguloEnRadianes),
      y: CENTRO_ANILLO.y + RADIO_ANILLO * Math.sin(anguloEnRadianes),
      cos: Math.cos(anguloEnRadianes),
      sin: Math.sin(anguloEnRadianes)
    };
  }

  #dibujarRonda() {
    vaciarContenido(this.#contenedorLienzo);
    this.#gruposEstacionPorNombre = new Map();
    this.#flechasEntreEstaciones = [];
    this.#iconoChef = null;

    const cantidadEstaciones = this.#nombresEstaciones.length;
    if (cantidadEstaciones === 0) return;

    const lienzoSvg = crearElementoSvg('svg', {
      viewBox: `0 0 ${ANCHO_LIENZO} ${ALTO_LIENZO}`,
      role: 'img',
      'aria-label': 'Lista circular de estaciones supervisadas por el chef'
    });

    lienzoSvg.append(
      crearElementoSvg(
        'defs',
        {},
        crearElementoSvg(
          'marker',
          { id: 'flecha-ronda', viewBox: '0 0 10 10', refX: 9, refY: 5, markerWidth: 7, markerHeight: 7, orient: 'auto-start-reverse' },
          crearElementoSvg('path', { d: 'M0 0 L10 5 L0 10 z', class: 'flecha-ronda' })
        )
      )
    );

    const capaFlechas = crearElementoSvg('g');
    const capaEstaciones = crearElementoSvg('g');
    lienzoSvg.append(capaFlechas, capaEstaciones);

    for (let indiceEstacion = 0; indiceEstacion < cantidadEstaciones; indiceEstacion++) {
      capaFlechas.append(this.#crearFlechaEntreEstaciones(indiceEstacion, cantidadEstaciones));
    }

    this.#nombresEstaciones.forEach((nombreEstacion, indiceEstacion) => {
      const puntoEstacion = this.#calcularPuntoEnAnillo(indiceEstacion);
      const alineacionEtiqueta = puntoEstacion.cos > 0.25 ? 'start' : puntoEstacion.cos < -0.25 ? 'end' : 'middle';
      const desplazamientoEtiquetaX =
        alineacionEtiqueta === 'start' ? RADIO_NODO + 10 : alineacionEtiqueta === 'end' ? -(RADIO_NODO + 10) : 0;
      const desplazamientoEtiquetaY =
        alineacionEtiqueta === 'middle' ? (puntoEstacion.sin > 0 ? RADIO_NODO + 16 : -(RADIO_NODO + 16)) : 0;

      const grupoEstacion = crearElementoSvg(
        'g',
        { class: 'estacion', transform: `translate(${puntoEstacion.x} ${puntoEstacion.y})` },
        crearElementoSvg('title', {}, nombreEstacion),
        crearElementoSvg('circle', { r: RADIO_NODO }),
        crearElementoSvg('text', { class: 'num' }, indiceEstacion + 1),
        crearElementoSvg(
          'text',
          { class: 'etiqueta', x: desplazamientoEtiquetaX, y: desplazamientoEtiquetaY, 'text-anchor': alineacionEtiqueta },
          acortarTexto(nombreEstacion, LONGITUD_MAXIMA_NOMBRE)
        )
      );

      this.#gruposEstacionPorNombre.set(nombreEstacion, grupoEstacion);
      capaEstaciones.append(grupoEstacion);
    });

    this.#iconoChef = crearElementoSvg('text', { class: 'chef', style: 'opacity:0' }, '👨‍🍳');
    lienzoSvg.append(this.#iconoChef);
    this.#contenedorLienzo.append(lienzoSvg);
  }

  #crearFlechaEntreEstaciones(indiceEstacion, cantidadEstaciones) {
    if (cantidadEstaciones === 1) {
      const puntoUnicaEstacion = this.#calcularPuntoEnAnillo(0);
      const flechaAutorreferencia = crearElementoSvg('path', {
        d: `M ${puntoUnicaEstacion.x - 8} ${puntoUnicaEstacion.y - RADIO_NODO} C ${puntoUnicaEstacion.x - 40} ${puntoUnicaEstacion.y - 70}, ${puntoUnicaEstacion.x + 40} ${puntoUnicaEstacion.y - 70}, ${puntoUnicaEstacion.x + 8} ${puntoUnicaEstacion.y - RADIO_NODO}`,
        class: 'cuerda',
        'marker-end': 'url(#flecha-ronda)'
      });
      this.#flechasEntreEstaciones.push(flechaAutorreferencia);
      return flechaAutorreferencia;
    }

    const puntoEstacionOrigen = this.#calcularPuntoEnAnillo(indiceEstacion);
    const puntoEstacionDestino = this.#calcularPuntoEnAnillo((indiceEstacion + 1) % cantidadEstaciones);
    const puntoMedioX = (puntoEstacionOrigen.x + puntoEstacionDestino.x) / 2;
    const puntoMedioY = (puntoEstacionOrigen.y + puntoEstacionDestino.y) / 2;
    const distanciaAlCentro = Math.hypot(puntoMedioX - CENTRO_ANILLO.x, puntoMedioY - CENTRO_ANILLO.y);

    let puntoControlX;
    let puntoControlY;
    if (distanciaAlCentro < 10) {
      const diferenciaX = puntoEstacionDestino.x - puntoEstacionOrigen.x;
      const diferenciaY = puntoEstacionDestino.y - puntoEstacionOrigen.y;
      const distanciaEntreEstaciones = Math.hypot(diferenciaX, diferenciaY) || 1;
      puntoControlX = puntoMedioX + (-diferenciaY / distanciaEntreEstaciones) * 50;
      puntoControlY = puntoMedioY + (diferenciaX / distanciaEntreEstaciones) * 50;
    } else {
      puntoControlX = CENTRO_ANILLO.x + (puntoMedioX - CENTRO_ANILLO.x) * 1.15;
      puntoControlY = CENTRO_ANILLO.y + (puntoMedioY - CENTRO_ANILLO.y) * 1.15;
    }

    const recortarHastaBordeDelNodo = (puntoPartida) => {
      const diferenciaX = puntoControlX - puntoPartida.x;
      const diferenciaY = puntoControlY - puntoPartida.y;
      const distanciaAlControl = Math.hypot(diferenciaX, diferenciaY) || 1;
      return {
        x: puntoPartida.x + (diferenciaX / distanciaAlControl) * RADIO_NODO,
        y: puntoPartida.y + (diferenciaY / distanciaAlControl) * RADIO_NODO
      };
    };
    const puntoInicio = recortarHastaBordeDelNodo(puntoEstacionOrigen);
    const puntoFin = recortarHastaBordeDelNodo(puntoEstacionDestino);

    const flecha = crearElementoSvg('path', {
      d: `M ${puntoInicio.x} ${puntoInicio.y} Q ${puntoControlX} ${puntoControlY} ${puntoFin.x} ${puntoFin.y}`,
      class: 'cuerda',
      'marker-end': 'url(#flecha-ronda)'
    });
    this.#flechasEntreEstaciones.push(flecha);
    return flecha;
  }
}
