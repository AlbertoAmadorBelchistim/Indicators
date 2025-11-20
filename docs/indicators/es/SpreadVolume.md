---
cs_file: SpreadVolume.cs
name: Spread Volume
category: Utility
group: Utility
subgroup: Stats
score_current: 7/10
version: Stable
recommended_action: Conservar
description: Visualiza el volumen ejecutado en el Ask y el Bid por separado, dibujado
  como histogramas en el spread.
gemini_summary: Visualización compleja 'On Chart'. Buen manejo de concurrencia (locks).
  UX mejorable.
file_state: Estable
score_potential: 8/10
effort: Medio
action_priority: P3
analysis_date: 2025-11-18
official_code_date: 2025-10-20
---

## 🟦 Spread Volume (7/10)

**Nombre del archivo:** [`SpreadVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/SpreadVolume.cs)  
**Nombre del indicador:** Spread Volume  
**Web oficial:** [ATAS — Spread Volume](https://help.atas.net/support/solutions/articles/72000602630)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 20/10/2025  

> **La Pregunta Clave:** ¿Quién está agrediendo más dentro del spread actual, los compradores (Ask) o los vendedores (Bid)?

![SpreadVolume](../../img/SpreadVolume.png)

---

### ⚙️ Parámetros configurables

* **Colors**: Compra, Venta, Texto.  
* **Dimensiones**: Spacing (espaciado), Width (ancho de barras), Offset (distancia al precio actual).  

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Microestructura de mercado y visualización de agresividad.

---

### 🧠 Uso más frecuente

* **Micro-Soportes:** Ver gran volumen en el Bid (rojo) que no baja el precio → Absorción pasiva de ventas.  
* **Escalera de Precios:** Ver cómo el volumen se desplaza tick a tick.  

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Detalle Extremo:** Muestra lo que pasa "dentro" de la vela.  
✅ **Seguridad:** Usa `lock` para gestionar la colección de trades, evitando errores de concurrencia comunes en indicadores de alto flujo.  
⛔ **Ruido Visual:** Dibuja rectángulos flotantes que pueden tapar el precio o confundirse con Footprint. Requiere configuración cuidadosa.  
⛔ **Limpieza:** Borra datos antiguos (`RemoveRange`) para no saturar RAM, lo cual es buena práctica.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Scalping de Spread:** En activos lentos (como Bonos), ver el desequilibrio inmediato Bid/Ask para robar un tick.  
* **Rejection:** Gran volumen en un lado del spread seguido de movimiento contrario rápido.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Width**: `15`.  
* **Offset**: `50` (Alejarlo un poco de la vela actual para ver el precio claro).  

---

### 🧪 Notas de desarrollo

* **Arquitectura:** Escucha `OnCumulativeTrade` (ticks agregados) para construir su propia base de datos en memoria (`_prints`).  
* **Render:** Dibuja todo manualmente en `OnRender`.  
* **Gestión de Memoria:** Mantiene una lista de `_prints` limitada a 200 elementos. Esto es excelente para el rendimiento: solo le importa el "ahora", no el histórico de hace 3 días.  

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta técnica bien codificada. El desarrollador entendió que para scalping de spread solo necesitas los últimos datos, por lo que la limpieza automática de la lista es un acierto de diseño.

**Propuestas de Mejora:**
* **Filtro de Valor:** Mostrar solo barras si el volumen supera X, para limpiar ruido.
* **Delta Mode:** Opción de mostrar solo la diferencia (Delta) en el spread en lugar de los dos lados.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.** Especialmente para scalpers de "Tick" o DOM que quieren una referencia visual en el gráfico.

**Acción:** **Conservar.**

