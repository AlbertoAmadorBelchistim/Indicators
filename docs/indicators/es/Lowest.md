---
cs_file: Lowest.cs
name: Lowest
category: Structure
group: Structure
subgroup: Static Levels
score_current: 6/10
version: Estable
recommended_action: Conservar
description: ¿Cuál es el valor más bajo (mínimo) en las últimas N barras?
gemini_summary: "Indicador 'bloque de construcción'. Funcional y estable. Redundante con Donchian."
comparison_group: "Price Extremes"
competitor_notes: "Inferior a Donchian Channel."
reusable_code: null
file_state: Estable
score_potential: 6/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-17
official_code_date: 23/04/2025
---

## 🟦 Lowest (6/10)

**Nombre del archivo:** [`Lowest.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Lowest.cs)  
**Nombre del indicador:** Lowest  
**Web oficial:** [ATAS — Lowest](https://help.atas.net/support/solutions/articles/72000602417)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Cuál es el valor más bajo (mínimo) en las últimas N barras?

![Lowest](../../img/Lowest.png)

---

### ⚙️ Parámetros configurables

* **Period**: Número de barras hacia atrás para buscar el mínimo valor en la serie de entrada (por defecto: 10)

---

### 🧭 Clasificación
📂 Level — Indicadores que marcan niveles de precio relevantes (mínimos, máximos, pivotes)

---

### 🧠 Uso más frecuente

* Determinar el **mínimo local** de un rango para detectar soportes
* Generar señales de compra cuando el precio rompe por encima del mínimo anterior
* Servir como base para otros indicadores (e.g. %K, trailing stops, breakout)

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Simple y eficiente para detección de extremos
✅ Útil como componente en estrategias más complejas
⛔ Por sí solo, no ofrece contexto ni validación adicional

---

### 🎯 Estrategias de scalping donde se aplica

* **Confirmación de giro**: si el precio deja de hacer mínimos decrecientes
* **Entrada por breakout**: tras superación del último mínimo importante
* **Trailing stop dinámico**: ajustar stop al mínimo de las últimas `n` velas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Period**: `10`

---

### 🧪 Notas de desarrollo

* Calcula el mínimo en una ventana móvil de longitud `Period`
* Usa `SourceDataSeries` como entrada de datos
* Utiliza `Math.Max` y `Math.Min` para determinar el rango de inicio y el conteo, manejando correctamente las barras iniciales donde `bar < Period`
* El valor se actualiza en `this[bar]` con el mínimo encontrado

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Este es un indicador de "bloque de construcción" simple pero fundamental. Su implementación en `Lowest.cs` es robusta y segura.

Destaca el uso de `var start = Math.Max(0, bar - Period + 1);` y `var count = Math.Min(bar + 1, Period);` para definir el rango de la ventana. Esto previene elegantemente cualquier error de índice fuera de rango en las primeras barras del gráfico, un error común en indicadores de ventana móvil.

Sus limitaciones son conceptuales, no técnicas: solo opera sobre la `SourceDataSeries` (normalmente el 'Close') y no permite, por ejemplo, encontrar el mínimo del 'Low'. Sin embargo, para lo que está diseñado, es estable.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Moderadamente.**

No es una estrategia en sí misma, pero es un componente esencial y estable para crear `Trailing Stops` o sistemas de reversión a la media.

**Acción:** **Conservar (Bloque de construcción estable).**

