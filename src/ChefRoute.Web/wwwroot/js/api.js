async function obtenerMensajeDeError(respuesta) {
  try {
    const cuerpoRespuesta = await respuesta.json();
    return cuerpoRespuesta.error ?? cuerpoRespuesta.detail ?? cuerpoRespuesta.title ?? `Error ${respuesta.status}`;
  } catch {
    return `Error ${respuesta.status}`;
  }
}

export async function cargarEjemplo(nombreEjemplo) {
  const respuesta = await fetch(`/api/ejemplos/${encodeURIComponent(nombreEjemplo)}`);
  if (!respuesta.ok) throw new Error(await obtenerMensajeDeError(respuesta));
  return JSON.stringify(await respuesta.json(), null, 2);
}

export async function analizar(textoJson, cantidadVueltas) {
  const respuesta = await fetch(`/api/analizar?vueltas=${encodeURIComponent(cantidadVueltas)}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: textoJson
  });
  if (!respuesta.ok) throw new Error(await obtenerMensajeDeError(respuesta));
  return respuesta.json();
}
