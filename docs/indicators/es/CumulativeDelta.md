---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: CumulativeDelta.cs
name: CVD - Cumulative Volume Delta
category: VolumeOrderFlow
score_current: 9/10
version: Estable
recommended_action: 'Conservar'
description: >-
  ¿Cuál es el delta acumulado (la agresión neta) desde el inicio de la sesión?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: >-
  'Herramienta 'Core' (P1) de Order Flow que acumula el delta para' detectar divergencias ('la guerra'), con una función profesional clave 'CustomSession'.
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-17
official_code_date: 2025-11-13
user_modification_date: null
---

## 🟦 CVD - Cumulative Volume Delta (9/10)

**Nombre del archivo:** [`CumulativeDelta.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CumulativeDelta.cs)  
**Nombre del indicador:** CVD - Cumulative Volume Delta  
**Web oficial:** [ATAS — CVD - Cumulative Volume Delta](https://help.atas.net/support/solutions/articles/72000602360-cumulative-volume-delta)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 13/11/2025  

> **La Pregunta Clave:** ¿Cuál es el delta acumulado (la agresión neta) desde el inicio de la sesión?

![Cumulative Volume Delta](../../img/CumulativeDelta.png)

---

### ⚙️ Parámetros configurables

* **Mode**: Tipo de visualización (Candles, Bars, Line).
* **SessionCumDeltaMode**: Tipo de sesión para reiniciar la acumulación (`None` / `Default` / `Custom`).
* **CustomSessionStart**: Hora de inicio para la sesión personalizada (ej. 09:30).
* **PosColor / NegColor**: Colores para delta positivo o negativo.
* **Alerts**: Parámetros para alertas sonoras/visuales basadas en el *delta de la vela individual* (`ChangeSize`).

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Delta acumulado por sesión o total.

---

### 🧠 Uso más frecuente

* Acumular el **delta por sesión o de forma continua** para detectar la presión agresiva neta.
* Identificar **divergencias entre el precio y el delta acumulado** (la señal de CVD más clásica).
* Visualizar el "esfuerzo vs. resultado" del flujo de órdenes (ej. si el CVD sube pero el precio no, indica absorción).

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Herramienta "Core":** Uno de los indicadores más importantes y utilizados en Order Flow.  
✅ **Altamente Configurable:** El modo `CustomSession` es una función profesional clave, que permite aislar el delta del RTH (Regular Trading Hours).  
✅ Imprescindible para detectar divergencias y absorción a nivel "macro" de la sesión.  
⛔ Requiere una configuración correcta del modo de sesión para ser útil (el modo `None` no suele servir para scalping).

---

### 🎯 Estrategias de scalping donde se aplica

* **Reversión por Divergencia:** La estrategia más potente. Si el precio hace un nuevo máximo (HH) pero el CVD hace un máximo más bajo (LH), es una divergencia bajista.
* **Confirmación de Breakout:** Una ruptura de precio acompañada de un CVD que también rompe su propia estructura (haciendo un nuevo máximo/mínimo) tiene más probabilidad de éxito.
* **Detección de Absorción:** Si el precio baja a un soporte y el CVD cae con fuerza, pero el precio deja de caer, indica absorción pasiva de los vendedores.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Mode**: `Line` (es la más clara para ver divergencias).
* **SessionCumDeltaMode**: `CustomSession`.
* **CustomSessionStart**: `09:30:00` (o `15:30:00` si tu ATAS está en UTC). **Esta es la clave:** aísla el delta solo del mercado RTH, ignorando el ruido de la noche.
* **UseScale**: `true`.

✅ Esta configuración permite leer la agresión neta de la sesión RTH, que es la más relevante.

---

### 🧪 Notas de desarrollo

* El indicador acumula `candle.Delta` en una variable (`_cumDelta`) barra a barra.
* La función `CheckStartBar(bar)` gestiona cuándo reiniciar el acumulador (`_cumDelta`) a cero, basándose en la configuración de `SessionCumDeltaMode`.
* Los tres modos de visualización (Candles, Bars, Line) usan la misma data de `_cumDelta`, solo cambia cómo se dibuja.
* La lógica de reinicio para `CustomSession` es correcta y usa el `InstrumentInfo.TimeZone` para convertir la hora UTC de la vela a la hora local del instrumento antes de comparar.

---

### 🛠️ Propuestas de mejora

* Añadir opción para **acumular delta por bloques de tiempo (ej. cada 5 minutos)**.
* Mostrar **etiquetas de divergencia automáticas** en el gráfico.

---
---

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

Este es un pilar del trading de Order Flow, y su nota 9/10 está totalmente justificada. Es el complemento "macro" del `DeltaModif` (que es "micro", por barra).

* `DeltaModif` te muestra la **batalla** (la agresión en cada vela).
* `CVD` te muestra la **guerra** (quién va ganando la batalla de agresión en toda la sesión).

Su uso principal e indispensable es la **detección de divergencias**. Cuando el precio y el delta acumulado se desacoplan, es una de las señales más potentes de que la tendencia está perdiendo fuerza y es probable una reversión.

La función clave que lo hace una herramienta profesional es `SessionCumDeltaMode = CustomSession`. Un scalper de S&P 500 (`ES`) no opera el CVD de la sesión Globex (noche), sino que lo reinicia exactamente a las 9:30 ET (apertura RTH) para ver quién gana la batalla *cuando el dinero real está en juego*.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una herramienta principal, indispensable.**

Mientras que `DeltaModif` te ayuda a *temporizar* la entrada (ej. una vela de delta extremo), el `CVD` te da el *contexto* para esa entrada (ej. "estás entrando en un soporte *con* una divergencia alcista de CVD en 1M").

**Acción:** **Conservar (Herramienta Principal).**