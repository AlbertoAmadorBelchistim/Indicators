---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: DeltaStrength.cs
name: Delta Strength
category: VolumeOrderFlow
score_current: 5/10
version: Estable
recommended_action: Descartar
description: ¿Qué velas cierran con un delta que está *casi* en su extremo
  (MaxDelta/MinDelta), pero no *exactamente* en él?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: Concepto de normalización de delta arruinado por una lógica de
  filtro de 'banda' (ej. 90-98%) que ignora la señal de agotamiento (98-100%).
file_state: Estable
score_potential: 5/10
effort: N/A
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-17
official_code_date: 2025-04-23
user_modification_date: null
---

## 🟦 Delta Strength (5/10)

**Nombre del archivo:** [`DeltaStrength.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DeltaStrength.cs)  
**Nombre del indicador:** Delta Strength  
**Web oficial:** [ATAS - Delta Strength](https://help.atas.net/support/solutions/articles/72000602363)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Qué velas cierran con un delta que está *casi* en su extremo (MaxDelta/MinDelta), pero no *exactamente* en él?

![DeltaStrength](../../img/DeltaStrength.png)

---

### ⚙️ Parámetros configurables

* **MaxFilter**: Porcentaje máximo del delta respecto al máximo/mínimo delta intrabarra (ej. 98).
* **MinFilter**: Porcentaje mínimo del delta respecto al máximo/mínimo delta intrabarra (ej. 90).
* **PosFilter / NegFilter**: Filtrado por tipo de vela (alcista, bajista o cualquiera).

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Indicadores basados en fuerza de delta relativa.

---

### 🧠 Uso más frecuente

* (Teórico) Detectar velas donde el delta estuvo cerca de su máximo extremo posible, pero no llegó a cerrrar en él.
* (Teórico) Identificar divergencias delta/vela (ej. vela alcista pero con un punto de "Delta Strength" negativo).

---

### 📊 Nivel de relevancia
🔟 **5 / 10**

⛔ **Lógica Confusa:** El indicador *solo* dibuja un punto si el delta está **DENTRO** del rango `MinFilter` y `MaxFilter` (ej. entre 90% y 98%). No señala el agotamiento extremo (99-100%), lo que lo hace muy poco intuitivo.
⛔ **Implementación Pobre:** Los filtros son confusos y la lógica de comparación de porcentajes de valores negativos es difícil de leer.
✅ La idea de medir la "calidad" del cierre del delta es buena.

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna fiable.** Debido a su lógica confusa (solo marca la banda 90-98%, no 90-100%), es difícil construir una estrategia robusta a su alrededor. Un scalper está más interesado en el agotamiento total (100%) que este indicador ignora.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No recomendado.** Es preferible usar `DeltaModif` con umbrales dinámicos, que es una herramienta mucho más clara para medir el delta extremo.

---

### 🧪 Notas de desarrollo

* El indicador calcula si el delta de la vela está dentro de un rango porcentual del máximo/mínimo delta de esa misma vela.
* **Lógica (Positiva):** Dibuja un punto si `Delta >= MaxDelta * (MinFilter/100)` Y `Delta <= MaxDelta * (MaxFilter/100)`.
* **Lógica (Negativa):** Dibuja un punto si `Delta >= MinDelta * (MinFilter/100)` Y `Delta <= MinDelta * (MaxFilter/100)` (comparando valores negativos).
* Si cumple las condiciones, dibuja un `VisualMode.Dots` por encima de la vela (si es delta negativo) o por debajo (si es delta positivo).

---

### 🛠️ Propuestas de mejora

* **Reescribir la lógica:** Cambiar el filtro para que sea `Delta >= MaxDelta * (MinFilter/100)`. Esto marcaría todo lo que supere el 90% (por ejemplo), lo cual es mucho más útil.
* Añadir opción para mostrar una **etiqueta de valor delta exacto** o su porcentaje.
* Añadir soporte para alertas sonoras.

---
---

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

Este indicador es un buen ejemplo de una idea interesante con una ejecución fallida. La idea de "normalizar" el delta de la vela contra su propio potencial (Max/Min Delta) es inteligente. Te diría si el cierre fue "fuerte" o "débil" en relación con la batalla interna de esa vela.

Sin embargo, la implementación es extraña. Al usar un `MinFilter` y un `MaxFilter`, el indicador no marca el "delta fuerte", sino que marca un "delta en una banda específica de fuerza". Si `MinFilter=90` y `MaxFilter=98`, un delta del 99% (agotamiento total) *no* se marcará.

Esto lo hace poco fiable y confuso. Un scalper no quiere una herramienta que *a veces* le avisa del peligro; quiere una que siempre lo haga.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.** Es una herramienta confusa que ignora las señales más importantes (agotamiento al 100%).

Un scalper obtendrá información infinitamente superior usando `DeltaModif` (con sus umbrales dinámicos) y la función de `Absorption` que has añadido, la cual detecta "colas" de delta (una forma mucho más robusta de ver un "cierre débil").

**Acción:** **Descartar (lógica confusa).**

**¿Merece la pena arreglarlo?** **No.** La funcionalidad que intenta ofrecer (medir la calidad del cierre del delta) ya está cubierta de mejor manera por la función de `Absorption` en tu `DeltaModif`.