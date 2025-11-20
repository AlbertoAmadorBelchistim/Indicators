---
cs_file: MacdCloud.cs
name: MACD Cloud
category: Oscillators
group: Oscillators
subgroup: MACD
score_current: 7/10
version: Stable
recommended_action: Conservar
description: ¿Cuál es la tendencia visual definida por el cruce de dos medias ponderadas
  (WMA)?
gemini_summary: Cruce de WMAs con relleno de color (Cloud). Simple, visual y efectivo
  para seguimiento de tendencia.
file_state: Estable
score_potential: 7/10
effort: Bajo
action_priority: N/A
analysis_date: 2025-11-19
user_modification_date: 2025-11-19
---

## 🟦 MACD Cloud (7/10)

**Nombre del indicador:** MACD Cloud  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código:** 19/11/2025 (Custom)  

> **La Pregunta Clave:** ¿Cuál es la tendencia visual definida por el cruce de dos medias ponderadas (WMA)?

![MacdCloud](../../img/MacdCloud.png)

---

### ⚙️ Parámetros configurables

* **Fast/Slow Period**: Periodos de las WMAs.
* **EMA Period**: Una tercera línea de filtro.
* **Colors**: Colores de la nube alcista (Verde) y bajista (Roja).

---

### 🧭 Clasificación
📂 Trend — Sistema de cruce de medias visual.

---

### 🧠 Uso más frecuente

* **Filtro de Fondo:** Si la nube es verde, solo buscar compras.
* **Cruce Visual:** El cambio de color de la nube es la señal de entrada/salida.

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Claridad Visual:** El relleno (`RangeDataSeries`) hace que sea imposible ignorar la tendencia.  
✅ **WMA:** Usa medias ponderadas, que son más rápidas que las SMA.  
⛔ **Nombre Confuso:** Se llama "MACD Cloud" pero no calcula la convergencia/divergencia real (MACD), sino el espacio entre las medias. Es más bien un "WMA Cross Cloud".

---

### 🎯 Estrategias de scalping donde se aplica

* **Trend Surfing:** Mantener la posición mientras el precio no cierre al otro lado de la nube.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Fast**: `9`.
* **Slow**: `20`.
* **EMA**: `50` (Como referencia de largo plazo).

---

### 🧪 Notas de desarrollo

* **Implementación:** Usa dos `RangeDataSeries` (`_cloudSeriesGreen`, `_cloudSeriesRed`). Activa una y oculta la otra según quién esté arriba.
* **Código Limpio:** Estructura simple y eficiente.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es un indicador "bonito" y funcional. Bueno para traders que necesitan ayudas visuales fuertes para respetar la tendencia.

**Propuestas de Mejora:**
* **Renombrar:** Llamarlo "WMA Cloud" para ser precisos.
* **Alertas:** Añadir alerta al cambio de color de la nube.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.** Para filtrar operaciones contra tendencia.

**Acción:** **Conservar.**