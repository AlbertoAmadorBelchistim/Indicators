---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: TVI.cs
name: Trade Volume Index
category: Volume
score_current: 7/10
version: Stable
recommended_action: Conservar
description: ¿Se está acumulando o distribuyendo el volumen basándose en la dirección del tick?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: "Acumulador de volumen simple basado en ticks. Similar al OBV pero con granularidad tick a tick."
file_state: Estable
score_potential: 7/10
effort: Bajo
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-18
official_code_date: 2025-04-23
user_modification_date: null
---

## 🟦 Trade Volume Index (TVI) (7/10)

**Nombre del archivo:** [`TVI.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/TVI.cs)  
**Nombre del indicador:** Trade Volume Index  
**Web oficial:** [ATAS — Trade Volume Index](https://help.atas.net/support/solutions/articles/72000602296)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Se está acumulando o distribuyendo el volumen basándose en la dirección del tick?

![TVI](../../img/TVI.png)

---

### ⚙️ Parámetros configurables

* **Ninguno**: Es un acumulador puro.

---

### 🧭 Clasificación
📂 Volume — Indicador de flujo de volumen acumulado (Cumulative Volume).

---

### 🧠 Uso más frecuente

* **Divergencia:** Precio hace nuevo máximo, TVI no → Distribución oculta.  
* **Confirmación:** Rompimiento de resistencia con TVI rompiendo su propia resistencia previa.  

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Granularidad:** A diferencia del OBV (que usa el cierre de la vela), el TVI detecta movimientos intra-vela si se usa en gráficos de ticks o rangos.  
⛔ **Deriva:** Como todo acumulador, tiende a derivar hacia el infinito. Difícil de leer sin contexto histórico.  
⛔ **Reset:** No tiene opción de reiniciar al inicio de la sesión.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Wyckoff Spring:** Precio rompe soporte, TVI se mantiene fuerte (no cae tanto) → Absorción.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **N/A**: No configurable.

---

### 🧪 Notas de desarrollo

* **Lógica:** Compara `value` (precio actual, suele ser Close) con el anterior.
    * Si sube > TickSize: Suma Volumen.
    * Si baja > TickSize: Resta Volumen.
    * Si el cambio es pequeño (<= TickSize): Mantiene valor anterior.
* **Bug Potencial:** La lógica `value - prev == TickSize` asume igualdad exacta de coma flotante, lo cual es arriesgado, aunque con `decimal` suele funcionar.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una variante del OBV. Útil, pero el Delta Acumulado (Cumulative Delta) suele ser superior porque distingue agresor (Bid/Ask) real, mientras que el TVI infiere dirección por el movimiento del precio (que puede ser engañoso con órdenes limitadas).

**Propuestas de Mejora:**
* **Session Reset:** Añadir opción para reiniciar a 0 cada día.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Moderadamente.** El Cumulative Delta es mejor.

**Acción:** **Conservar.**