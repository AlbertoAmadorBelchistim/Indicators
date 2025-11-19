---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: MacdLeader.cs
name: MACD Leader
category: Momentum
score_current: 7/10
version: ATAS Official
recommended_action: 'Conservar'
description: >-
  ¿Cuál es una versión "adelantada" del MACD que incorpora la diferencia entre el precio y sus EMAs?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: >-
  Implementación estable de un concepto de "MACD + residuales de EMA" diseñado para adelantar las señales. Lógica compleja pero funcional.
file_state: Estable
score_potential: 7/10
effort: N/A
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-17
official_code_date: 2025-04-23
user_modification_date: null
---

## 🟦 MACD Leader (7/10)

**Nombre del archivo:** [`MacdLeader.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MacdLeader.cs)    
**Nombre del indicador:** MACD Leader    
**Web oficial:** [ATAS — MACD Leader](https://help.atas.net/support/solutions/articles/72000602419)    
**Compatibilidad:** ATAS versión estable y superiores.    
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es una versión "adelantada" del MACD que incorpora la diferencia entre el precio y sus EMAs?

![MACDLeader](../../img/MACDLeader.png) 

---

### ⚙️ Parámetros configurables

* **MacdPeriod**: Periodo de señal del MACD (por defecto: 9)
* **MacdShortPeriod**: Periodo corto del MACD y de las EMAs rápidas auxiliares (por defecto: 12)
* **MacdLongPeriod**: Periodo largo del MACD y de las EMAs lentas auxiliares (por defecto: 26)

---

### 🧭 Clasificación
📂 Momentum — Variante del MACD con alisamiento doble de EMAs

---

### 🧠 Uso más frecuente

* Identificar señales adelantadas respecto al MACD clásico
* Detectar aceleraciones y giros de momentum antes que el histograma convencional
* Medir la diferencia entre impulso de corto y largo plazo con ajuste suave

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Más reactivo que el MACD clásico gracias al alisamiento diferencial  
✅ Puede anticipar cambios de tendencia leves  
⛔ Su interpretación requiere mayor comprensión del funcionamiento interno  

---

### 🎯 Estrategias de scalping donde se aplica

* **Confirmación anticipada**: entrada cuando el valor de `MACD Leader` gira antes que el MACD convencional
* **Divergencia temprana**: cuando el `MACD Leader` se desacopla del histograma MACD clásico
* **Detección de aceleraciones**: si el valor se separa rápidamente de cero

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **MacdPeriod**: `5`
* **MacdShortPeriod**: `8`
* **MacdLongPeriod**: `21`

---

### 🧪 Notas de desarrollo

* Combina la lógica del MACD con dos EMAs adicionales que calculan la diferencia entre el valor y su propia EMA
* La fórmula base es: `Línea = MACD + (EMA_Residual_Corta - EMA_Residual_Larga)`
* `EMA_Residual` se calcula como: `EMA(precio - EMA(precio))`
* Se visualiza en un nuevo panel con dos series: la línea "Leader" (`RenderSeries`) y el histograma MACD convencional (`_macd.DataSeries[2]`)
* Reutiliza internamente un objeto `MACD`

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Este es un indicador de momentum avanzado con una implementación estable y segura. El código `MacdLeader.cs` revela una fórmula interesante:

1.  Calcula un MACD estándar (`_macd.Calculate(bar, value);`).
2.  Calcula dos "residuales" de EMA. Un residual es la diferencia entre el precio y su EMA, que luego es suavizada por otra EMA:
    * `_shortEmaSmooth.Calculate(bar, value - _shortEma[bar]);`
    * `_longEmaSmooth.Calculate(bar, value - _longEma[bar]);`
3.  La línea final es la suma del MACD y la diferencia de estos residuales:
    * `_renderSeries[bar] = _macd[bar] + _shortEmaSmooth[bar] - _longEmaSmooth[bar];`

El objetivo es usar la "aceleración" (los residuales) para adelantar la señal del MACD. El código es estable y no tiene riesgos. Su único "code smell" es el acoplamiento de parámetros (ej. `MacdShortPeriod` controla 3 EMAs distintas), pero es un defecto menor.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Su naturaleza "adelantada" está diseñada específicamente para capturar giros de momentum más rápido que un MACD estándar, lo cual es ideal para scalping.

**Acción:** **Conservar (Concepto avanzado y estable).**

