---
cs_file: DeltaColoredCandles.cs
name: Delta Colored Candles
category: Order Flow
group: Order Flow
subgroup: Delta
score_current: 3/10
version: Estable
recommended_action: Descartar
description: '¿Cuál es la intensidad del momentum del delta en relación con un máximo fijo?'
gemini_summary: "Inútil en la práctica. Requiere que el usuario 'adivine' y configure manualmente un valor de 'MaxDelta' fijo. Si el mercado cambia de volatilidad, el indicador deja de funcionar."
comparison_group: "Bar Delta"
competitor_notes: "Mala arquitectura de software. DeltaModif es superior."
reusable_code: null
file_state: Estable (Diseño Pobre)
score_potential: 3/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-19
official_code_date: 2025-04-23
---

## ⛔ Delta Colored Candles (3/10)

**Nombre del archivo:** [`DeltaColoredCandles.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DeltaColoredCandles.cs)  
**Nombre del indicador:** Delta Colored Candles  
**Web oficial:** [ATAS — Delta Colored Candles](https://help.atas.net/support/solutions/articles/72000618743)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la intensidad del *momentum* del delta (delta acumulado en N barras) en relación con un máximo fijo?

![DeltaColoredCandles](../../img/DeltaColoredCandles.png)

---

### ⚙️ Parámetros configurables

* **Period**: Número de barras para acumular el delta (por defecto: 14).
* **MaxDelta**: Delta máximo esperado para escalar el color (por defecto: 600).
* **ColorScheme**: Esquema de color del heatmap (`RedToDarkToGreen`, `GreenToRed`, etc.).

---

### 🧭 Clasificación
**Grupo:** Order Flow
**Subgrupo:** Delta (Por Barra)

---

### 🧠 Uso más frecuente

* **(Teórico)** Visualizar la **intensidad de la agresión neta** en forma de color, suavizada en un período.
* **(Teórico)** Detectar acumulaciones o momentos de delta extremo de forma visual.

---

### 📊 Nivel de relevancia
3️⃣ **3 / 10 (MAL DISEÑO)**

✅ **Buena Idea:** El concepto de medir el *momentum* del delta (acumulando N barras) es teóricamente útil.
⛔ **Fallo de Implementación:** El indicador es **impractical** para scalping. Requiere que el usuario defina un `MaxDelta` **fijo**.
* Un `MaxDelta` de 600 puede ser correcto para el *pre-market*, pero en la apertura (RTH) se saturará al instante (todo verde/rojo brillante).
* Si lo ajustas a 2000 para la apertura, al mediodía el indicador se verá gris (sin señal).
⛔ Requiere calibración manual *constante*.

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna fiable.**
* En la práctica, las estrategias basadas en "color" fallan porque el color depende de un parámetro arbitrario (`MaxDelta`) y no de la realidad del mercado.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No recomendado.**
* No existe una configuración "set and forget". Tendrías que estar cambiando el `MaxDelta` cada 30 minutos según la volatilidad.

---

### 🧪 Notas de desarrollo

* El indicador calcula la suma del delta en una ventana de `Period` barras: `_delta.CalcSum(_period, bar)`.
* Escala el resultado con respecto a un `MaxDelta` fijo: `sumDelta * 100 / MaxDelta`.
* Ajusta el valor visual (rate) como: `rate = 50 + (percent / 2)`.
* El color resultante se obtiene mediante `HeatmapExtensions.GetColor()`.

---

### 🛠️ Propuestas de mejora

* **Para hacerlo viable (Esfuerzo Medio):** Reemplazar el `MaxDelta` fijo por un cálculo **dinámico**. Usar una desviación estándar (StDev) del delta reciente para normalizar el color (Z-Score visual).
* *Nota:* Dado que ya tenemos `DeltaModif`, invertir tiempo en arreglar esto es baja prioridad.

---

### 💎 Valor Reutilizable

Ninguno. La lógica de heatmap estático es inferior a las visualizaciones dinámicas modernas.

---

### ✍️ La opinión de Gemini sobre el Indicador

Este indicador tiene una **idea de 9/10** arruinada por una **implementación de 2/10**.

La idea de un "CVD de momentum" es excelente, pero la ejecución es prehistórica. Al basar la escala de colores en un valor manual y fijo, el indicador se vuelve inútil en un mercado real que cambia de régimen de volatilidad constantemente (como el paso de Overnight a RTH).

Es el equivalente a un velocímetro que requiere que *tú* le digas cuál es la velocidad máxima del coche para poder mover la aguja.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No. No en su estado actual.**

Un scalper necesita herramientas que se adapten a la volatilidad del momento automáticamente (como las bandas de Bollinger o los canales dinámicos). Una herramienta estática es un obstáculo.

**Acción:** **DESCARTAR.**