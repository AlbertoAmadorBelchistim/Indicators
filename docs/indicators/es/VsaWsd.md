---
# 1. IDENTIFICACIÓN
cs_file: VsaWsd.cs
name: VSA – WSD Histogram
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Price Action
subgroup: Candle Analysis
comparison_group: "Candle Structure"

# 3. VALORACIÓN (Score & Priority)
score_current: 2/10
score_potential: 2/10
file_state: Estable (Conceptualmente Engañoso)
effort: N/A
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Descartar

# 5. ANÁLISIS
description: ¿Cómo se desglosa el rango de la vela (cuerpo vs mechas) comparado con su volatilidad promedio?
gemini_summary: "Falso indicador VSA. Tras auditoría de código, se confirma que NO utiliza datos de volumen real. La línea amarilla ('AvgVolume') es en realidad un ATR (Promedio de Rango) y los histogramas miden ticks de movimiento. Es una herramienta de Price Action pura mal etiquetada que induce a error al trader."
competitor_notes: "Inferior a un ATR estándar o a la lectura de velas japonesa."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-12-11
official_code_date: 2025-05-08
---

## 💀 VSA – WSD Histogram (2/10)

**Nombre del archivo:** [`VsaWsd.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VsaWsd.cs)  
**Nombre del indicador:** VSA – WSD Histogram  
**Web oficial:** [ATAS — VSA – WSD Histogram](https://help.atas.net/support/solutions/articles/72000602501)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-05-08  

> **La Pregunta Clave:** ¿Cómo se desglosa el rango de la vela (cuerpo vs mechas) comparado con su volatilidad promedio?

![VsaWsd](../../img/VsaWsd.png)

---

### ⚙️ Parámetros configurables

* **Period:** Periodo de la media móvil. **Atención:** No es una media de volumen, sino una media del Rango (ATR).
* **Visuals:** Colores para mechas y cuerpo.

---

### 🧭 Clasificación
**Grupo:** Structure  
**Subgrupo:** Candle Analysis  
**Comparison Group:** "Candle Structure"  

---

### 🧠 Uso más frecuente

* **(Erróneo):** Intentar leer volumen con él.
* **(Real):** Medir la volatilidad. Ver si la vela actual es más grande o pequeña que el promedio de las últimas X velas.

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

⛔ **Engañoso:** El código asigna el valor `(High - Low)` a una variable llamada `volume`, lo que confunde totalmente su propósito. Es un indicador de Rango disfrazado.  
⛔ **Sin Volumen:** Los puntos de señal (WSD) se calculan exclusivamente comparando si el rango se contrae (`HighLow < Prev`), ignorando la actividad real de contratación.  
⛔ **Redundante:** Desglosa la vela en partes (mecha arriba, abajo, cuerpo), información que ya es evidente en el gráfico principal.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Usarlo creyendo que analiza volumen es peligroso.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Eliminar.**

---

### 🧪 Notas de desarrollo

* **Hallazgo Crítico:** En la línea 128, el código hace `var volume = (candle.High - candle.Low) / _tickSize;`. Esto confirma que **ignora el volumen real** y usa el rango en ticks para todos los cálculos posteriores, incluida la línea promedio.
* * Calcula el tamaño de mechas y cuerpo en ticks.  
* Genera señales (`_dotsBuy`, `_dotsSell`) basadas en la contracción del rango.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Naming:** Debería llamarse "Candle Range Breakdown" o "Detailed ATR". Llamarlo "VSA" (Volume Spread Analysis) es técnicamente incorrecto al carecer de componente de Volumen.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

Al no usar volumen real, no sirve para Order Flow ni VSA. Es solo una regla para medir velas.


---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Información falsa (dice ser volumen, es rango).

**Acción:** **Descartar**