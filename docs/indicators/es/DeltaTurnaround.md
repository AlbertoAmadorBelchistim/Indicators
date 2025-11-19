---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: DeltaTurnaround.cs
name: Delta Turnaround
category: VolumeOrderFlow
score_current: 6/10
version: Latest
recommended_action: 'Descartar'
description: >-
  '¿Se ha producido un patrón de giro de 3 velas (dos en una dirección,' una en la opuesta) confirmado por el delta?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: >-
  'Patrón de giro de 3 velas válido pero 'hard-codeado', haciéndolo 100%' redundante frente a la flexibilidad del indicador 'BarsPattern'.
file_state: Estable
score_potential: 6/10
effort: N/A
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-17
official_code_date: 2025-07-31
user_modification_date: null
---

## 🟦 Delta Turnaround (6/10)

**Nombre del archivo:** [`DeltaTurnaround.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DeltaTurnaround.cs)  
**Nombre del indicador:** Delta Turnaround  
**Web oficial:** [ATAS — Delta Turnaround](https://help.atas.net/support/solutions/articles/72000602364)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 31/07/2025

> **La Pregunta Clave:** ¿Se ha producido un patrón de giro de 3 velas (dos en una dirección, una en la opuesta) confirmado por el delta?

![DeltaTurnaround](../../img/DeltaTurnaround.png)

---

### ⚙️ Parámetros configurables

* **UseAlerts**: Activar alertas sonoras.
* **AlertOnNewCandle**: Lanzar alerta al abrir la siguiente vela (en lugar de en la vela de la señal).
* **AlertFile**: Archivo de sonido para la alerta.
* **AlertBGColor / AlertForeColor**: Colores del pop-up de alerta.
* *Nota: La lógica del patrón (nº de velas, etc.) no es configurable.*

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Señales de giro basadas en delta y estructura.

---

### 🧠 Uso más frecuente

* Detectar patrones de reversión en el delta tras dos velas fuertes en dirección contraria.
* Visualizar señales de giro con confirmación por delta negativo (venta) o positivo (compra)
* Lanzar alertas cuando se forma el patrón completo de vuelta delta.

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ **Patrón Válido:** El patrón que busca (estructura de 2 velas + 1 giro con delta) es un setup de scalping válido.  
✅ **Simple:** Visualización muy limpia (flechas arriba/abajo).  
⛔ **Extremadamente Rígido:** El patrón está "hard-codeado" (2 velas previas, 1 de giro). No se puede configurar para buscar giros de 3 velas, o con un delta mínimo.  
⛔ **Redundante:** El indicador `BarsPattern` puede ser configurado para encontrar este exacto patrón (y miles más) con mucha más flexibilidad.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Vuelta bajista (flecha roja):** Dos velas alcistas, seguidas de una vela bajista que hace un nuevo máximo (o igual) y tiene delta negativo.
* **Vuelta alcista (flecha verde):** Dos velas bajistas, seguidas de una vela alcista que hace un nuevo mínimo (o igual) y tiene delta positivo.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **UseAlerts**: `true`
* **AlertOnNewCandle**: `false` (Para recibir la alerta en la vela de señal, no una vela tarde).

---

### 🧪 Notas de desarrollo

* El indicador busca dos patrones fijos en `OnCalculate`:
    * **Giro bajista (Flecha Roja):**
        * `prevCandle` es alcista (Close > Open).
        * `prev2Candle` es alcista (Close > Open).
        * `candle` es bajista (Close < Open).
        * `candle.High >= prevCandle.High` (Fallo en el nuevo máximo).
        * `candle.Delta < 0` (Confirmación de agresión vendedora).
    * **Giro alcista (Flecha Verde):**
        * `prevCandle` es bajista (Close < Open).
        * `prev2Candle` es bajista (Close < Open).
        * `candle` es alcista (Close > Open).
        * `candle.Low <= prevCandle.Low` (Fallo en el nuevo mínimo).
        * `candle.Delta > 0` (Confirmación de agresión compradora).

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Alerta Genérica (Bug):** Si `AlertOnNewCandle = true`, el texto de la alerta es `"Delta turnaround signal."`, sin diferenciar si es alcista o bajista. (Las alertas inmediatas, `AlertOnNewCandle = false`, sí están diferenciadas correctamente en el código).
* **Rigidez:** Como se mencionó, el patrón no es configurable (nº de velas, umbral de delta, etc.).

---

### 🛠️ Propuestas de mejora

* Reparar el bug del texto de la alerta genérica en modo `AlertOnNewCandle`.
* *Propuesta conceptual:* Descartar este indicador y replicar su lógica en `BarsPattern` para mayor flexibilidad.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Este indicador es un buen ejemplo de una herramienta que hace una sola cosa específica. El patrón que busca (una micro-tendencia de 2 velas que falla en el tercer intento, con el delta confirmando el fallo) es un setup de reversión de libro de texto.

El problema es su rigidez. ¿Qué pasa si la micro-tendencia tenía 3 velas? ¿O 4? ¿Qué pasa si el delta es positivo, pero la vela de giro es una absorción masiva? El indicador ignora todos estos matices.

Es un indicador obsoleto porque su funcionalidad ha sido completamente superada por el indicador `BarsPattern`. Con `BarsPattern`, podrías construir este exacto setup (y 100 variaciones del mismo) de forma mucho más robusta y flexible.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No. Es redundante y demasiado rígido.**

Un scalper necesita flexibilidad. Las herramientas que ya hemos decidido "Conservar" (como `BarsPattern`, `DeltaModif` y `CMS`) te permiten identificar este mismo patrón de giro de forma mucho más fiable y contextual.

No hay razón para usar este indicador si ya tienes `BarsPattern` en tu arsenal.

**Acción:** **Descartar (Rígido / Redundante).**

**¿Merece la pena arreglarlo?** **No.** Arreglarlo implicaría añadirle parámetros (nº de velas, filtro de delta), lo que lo convertiría en una versión "lite" de `BarsPattern`. Es mejor usar `BarsPattern` directamente.