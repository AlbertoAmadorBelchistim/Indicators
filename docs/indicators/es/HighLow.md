---
cs_file: HighLow.cs
name: Highest High / Lowest Low Over N Bars
category: Structure
group: Structure
subgroup: Static Levels
score_current: 7/10
version: ATAS Official
recommended_action: Conservar
description: ¿Cuál es el rango de precio (máximo más alto y mínimo más bajo) de las
  últimas N barras?
gemini_summary: Implementación 'Core' y estable del canal de precios (Donchian Channel),
  que traza el High más alto y el Low más bajo de las últimas N barras.
file_state: Estable
score_potential: 7/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-17
official_code_date: 2025-04-23
---

## 🟦 Highest High / Lowest Low Over N Bars (7/10)

**Nombre del archivo:** [`HighLow.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/HighLow.cs)  
**Nombre del indicador:** Highest High / Lowest Low Over N Bars  
**Web oficial:** [ATAS — Highest High / Lowest Low Over N Bars](https://help.atas.net/support/solutions/articles/72000602244)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Cuál es el rango de precio (máximo más alto y mínimo más bajo) de las últimas N barras?

![Highest High / Lowest Low Over N Bars](../../img/HighLow.png)

---

### ⚙️ Parámetros configurables

* **Period**: Número de barras para calcular el máximo y mínimo (por defecto: 15)

---

### 🧭 Clasificación
📂 Levels — Indicadores de extremos móviles (canales dinámicos)

---

### 🧠 Uso más frecuente

* Identificar el **rango dinámico** de precios (Canal Donchian)
* Marcar niveles de **soporte y resistencia dinámicos**
* Utilizar como base para breakout, reversión o trailing

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Herramienta "Core" de Niveles**: Es el Canal Donchian/Price Channel estándar.  
✅ Fácil de interpretar visualmente.  
✅ Sirve como base para múltiples estrategias técnicas.  
⛔ No considera volumen ni momentum.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Breakout por canal**: entrada si el precio supera el máximo/mínimo del periodo
* **Falso Breakout (Reversión)**: Vender si el precio toca el canal superior y es rechazado.
* **Trailing stop estructural**: usar el mínimo como trailing dinámico en largos

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Period**: `15` a `20`
* Combinar con Delta o DOM Strength para validar la agresión en extremos

---

### 🧪 Notas de desarrollo

* El indicador almacena los High y Low de cada vela en series internas (`_highSeries`, `_lowSeries`).
* Utiliza las funciones optimizadas `_highSeries.MAX(_period, bar)` y `_lowSeries.MIN(_period, bar)` para calcular los extremos de la ventana.
* Dibuja dos líneas (`_maxSeries` y `_minSeries`) que representan el canal.
* Está diseñado específicamente para `High` y `Low`; no está pensado para ser configurable a `Close` (para eso existe `Highest.cs`).

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Esta es una herramienta "Core" de niveles. Es la implementación estándar y robusta del canal de precios (a menudo llamado Canal Donchian). Dibuja el máximo más alto y el mínimo más bajo de las últimas N barras.

El `.md` original menciona que "no permite elegir el tipo de precio". Esto no es una incoherencia, sino la *definición* del indicador. Se llama "Highest **High** / Lowest **Low**". Su propósito es trazar los extremos del precio (las mechas), que es lo que la mayoría de los traders de breakout buscan.

Es estable, ligero y hace exactamente lo que promete.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una herramienta de contexto "Core".**

Define el "campo de juego" inmediato. Es fundamental para estrategias de ruptura (breakout) y de reversión en los extremos del rango.

**Acción:** **Conservar (Herramienta de Contexto).**