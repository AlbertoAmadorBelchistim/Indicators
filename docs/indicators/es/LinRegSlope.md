---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: LinRegSlope.cs
name: Linear Regression Slope
category: Trend
score_current: 1/10
version: ATAS Official
recommended_action: Reparar
description: ¿Cuál es la pendiente (dirección e intensidad) de la tendencia en el período reciente?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: ROTO. Causa una división por cero si Period=1, un valor permitido por la UI [Range(1, ...)], resultando en un crash.
file_state: Roto
score_potential: 7/10
effort: Bajo
action_priority: P1
# --- Control de Versiones ---
analysis_date: 2025-11-17
official_code_date: 2025-04-23
user_modification_date: null
---

## 🟦 Linear Regression Slope (1/10)

**Nombre del archivo:** [`LinRegSlope.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/LinRegSlope.cs)  
**Nombre del indicador:** Linear Regression Slope  
**Web oficial:** [ATAS — Linear Regression Slope](https://help.atas.net/support/solutions/articles/72000602416)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Cuál es la pendiente (dirección e intensidad) de la tendencia en el período reciente?

![LinearRegressionSlope](../../img/LinearRegressionSlope.png)

---

### ⚙️ Parámetros configurables

* **Period**: Número de barras usadas para calcular la pendiente (por defecto: 14)

---

### 🧭 Clasificación
📂 Trend — Pendiente de la recta de regresión lineal

---

### 🧠 Uso más frecuente

* Evaluar la **dirección e intensidad** de una tendencia en el precio
* Confirmar la **fuerza del movimiento** según si la pendiente se mantiene positiva o negativa
* Filtrar señales de entrada basándose en si la pendiente supera un umbral mínimo

---

### 📊 Nivel de relevancia
🔟 **1 / 10**

✅ Mide tendencia de forma cuantitativa y objetiva (conceptual)  
✅ Apto para sistemas algorítmicos como filtro (conceptual)  
⛔ **BUG CRÍTICO:** Causa un **crash** por división por cero si `Period` se establece en 1  
⛔ No distingue consolidaciones de pendiente cercana a cero

---

### 🎯 Estrategias de scalping donde se aplica

* **Filtro de entrada**: solo operar a favor de la pendiente (si estuviera reparado)
* **Confirmación de continuación**: mantener posición mientras la pendiente no cambie (si estuviera reparado)

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Period**: `10` (¡No usar `1`!)

---

### 🧪 Notas de desarrollo

* Calcula la pendiente (`slope`) de la regresión lineal clásica en cada barra
* Usa sumatorias predefinidas para `x`, `x²` y `x*y`
* El resultado representa la **variación promedio por barra** del precio
* Tiene un **bug fatal** de división por cero si `Period = 1`

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Este indicador está **completamente roto** y es peligroso de usar. El bug es evidente en el código `LinRegSlope.cs`.

El parámetro `Period` tiene un `[Range(1, 10000)]` en sus atributos, permitiendo explícitamente al usuario seleccionar `Period = 1`.

Si `Period = 1`, la fórmula del `divisor` (`var divisor = sumX * sumX - Period * Period * (Period - 1m) * (2 * Period - 1) / 6;`) se evalúa a 0. Esto lleva inevitablemente a un **crash por división por cero** en la línea final de `OnCalculate` (`... / divisor`).

Un indicador que crashea la plataforma con una configuración permitida por su propia UI es inaceptable.

**Propuesta de Reparación (P1):**
* **URGENTE (Opción 1):** Cambiar el rango del parámetro a `[Range(2, 10000)]`.
* **URGENTE (Opción 2):** Añadir una validación en `OnCalculate`: `if (divisor == 0) { this[bar] = 0; return; }`.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No (Está Roto).**

Es un concepto útil para medir momentum, pero actualmente es una bomba de tiempo que puede crashear la plataforma.

**Acción:** **Reparar (Bug P1 - Crash por división por cero).**

