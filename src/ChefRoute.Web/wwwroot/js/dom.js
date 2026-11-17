const ESPACIO_DE_NOMBRES_SVG = 'http://www.w3.org/2000/svg';

function asignarAtributosEHijos(nodo, atributos, hijos) {
  for (const [nombreAtributo, valorAtributo] of Object.entries(atributos)) {
    if (valorAtributo === null || valorAtributo === undefined || valorAtributo === false) continue;
    nodo.setAttribute(nombreAtributo, valorAtributo === true ? '' : String(valorAtributo));
  }
  for (const hijo of hijos.flat()) {
    if (hijo === null || hijo === undefined || hijo === false) continue;
    nodo.append(hijo instanceof Node ? hijo : document.createTextNode(String(hijo)));
  }
  return nodo;
}

export function crearElemento(etiqueta, atributos = {}, ...hijos) {
  return asignarAtributosEHijos(document.createElement(etiqueta), atributos, hijos);
}

export function crearElementoSvg(etiqueta, atributos = {}, ...hijos) {
  return asignarAtributosEHijos(document.createElementNS(ESPACIO_DE_NOMBRES_SVG, etiqueta), atributos, hijos);
}

export function vaciarContenido(nodo) {
  nodo.replaceChildren();
}

export function acortarTexto(texto, longitudMaxima) {
  return texto.length > longitudMaxima ? `${texto.slice(0, longitudMaxima - 1)}…` : texto;
}
