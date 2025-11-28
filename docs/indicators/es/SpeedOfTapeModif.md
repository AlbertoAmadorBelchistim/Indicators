---
cs_file: SpeedOfTapeLab.cs
name: Speed of Tape (Lab)
group: Order Flow
subgroup: Volume
score_current: 6/10
version: Lab (v1.5)
recommended_action: Conservar (Reserva / Educativo)
description: ¿Cuál es la velocidad de ejecución del mercado calculada por interpolación?
gemini_summary: "Versión con filtro de delta matemáticamente corregida pero tácticamente insuficiente. Usa interpolación lineal para 'adivinar' la velocidad en ventanas de tiempo menores a la vela. Útil para análisis general, inútil para trigger HFT."
comparison_group: "Tape Speed"
competitor_notes: "Inferior a la V2. Mientras la V2 ve la realidad tick a tick, esta versión ve un promedio suavizado que oculta las ráfagas institucionales."
reusable_code: "Algoritmo de interpolación lineal (Regla de 3) para ventanas de tiempo."
file_state: Estable
score_potential: 6/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-28
official_code_date: Desconocida
---

## 🧪 Speed of Tape (Lab) (6/10)

**Nombre del archivo:** [`SpeedOfTapeLab.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/SpeedOfTapeLab.cs)  
**Nombre del indicador:** Speed of Tape (Lab)  
**Web oficial base:** [ATAS — Speed of Tape](https://help.atas.net/support/solutions/articles/72000602472)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código modificado:** 23/11/2025  

> **La Pregunta Clave:** ¿Se está acelerando el mercado (estimación basada en velas)?

![SpeedOfTapeLab](../../img/SpeedOfTapeLab.png)

---

### ⚙️ Parámetros configurables

* **AutoFilter:** Activa el promedio móvil para el umbral.
* **TimeWindowSec:** Ventana de tiempo a analizar (Se interpola si es menor que la vela).
* **Calculation Type:** `Volume`, `Ticks`, `Buys`, `Sells`, `Delta`.
* **Show PaintBars:** Pinta la vela si supera el umbral.
* **Draw Signal Lines:** Dibuja marcadores triangulares.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Tape Speed"  

---

### 🧠 Uso más frecuente

* **Comparativa Académica:** Útil para demostrar por qué NO se deben usar indicadores basados en velas para medir velocidad de alta frecuencia.
* **Swing Trading:** En timeframes altos (ej. 1 Hora) donde la precisión del milisegundo no importa tanto, su lógica de promedio es aceptable.

---

### 📊 Nivel de relevancia
🔟 **6 / 10 (NO APTO PARA SCALPING)**


✅ **Ligereza:** Consume casi cero recursos al no pedir ticks al servidor.  
⛔ **Falsedad de Datos (Interpolación):** Asume que el volumen se distribuye uniformemente. Si hubo una ráfaga de 1000 contratos en 1 segundo y luego nada en los siguientes 59 segundos (vela de 1 min), este indicador dirá que hubo una velocidad constante y baja de "16 contratos/segundo". **Diluye la señal crítica.** ⛔ **Ceguera HFT:** Incapaz de detectar "Icebergs" o algoritmos de ejecución rápida dentro de la vela.

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** No usar para tomar decisiones de entrada en gráficos de ticks o segundos.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **TimeWindowSec:** `60` o más. (Poner menos es engañarse a uno mismo, ya que solo interpola datos).
* **AutoFilter:** `True`.

---

### ✨ Mejoras añadidas (Lab v1.3.0)

Esta versión soluciona problemas visuales críticos y mejora la precisión del filtro en modos Delta.

#### 🔧 Correcciones Lógicas
* **Filtro de Magnitud Absoluta:** Se ha corregido el cálculo del *threshold* en el modo `Delta`. Ahora se utiliza `Math.Abs(accumulatedSpeed)`, lo que permite detectar aceleraciones fuertes de venta (valores negativos) que antes alteraban el cálculo del filtro.

#### 🎨 Mejoras Visuales (De "Líneas" a "Marcadores")
* **Eliminación de Líneas Medias:** Se han eliminado las líneas horizontales que atravesaban la vela por el punto medio `(H+L)/2`, ya que generaban ruido visual y sugerían niveles de precio falsos.
* **Nuevos Marcadores Externos:** Ahora se dibujan indicadores visuales **fuera** de la vela para no obstruir la acción del precio:
    * **Aceleración Alcista:** Marcador debajo del mínimo (*Low*).
    * **Aceleración Bajista:** Marcador encima del máximo (*High*).
* **Personalización:**
    * **Formas:** Selección entre Rombo, Círculo o Triángulo (útil para diferenciar de otros indicadores como DeltaModif).
    * **Offset (Distancia):** Parámetro ajustable en píxeles para alejar el marcador de la vela y evitar solapamientos con otras señales.

---

### 🧪 Notas de desarrollo

* **El problema de la Regla de 3:** El código ejecuta: `accumulatedSpeed = accumulatedSpeed * (timeWindow / timeDiff)`.
    * *Ejemplo Real:* Ventana de 5s en Vela de 60s con 1000 de volumen.
    * *Resultado:* `1000 * (5/60) = 83.3`.
    * *Realidad:* Quizás esos 1000 ocurrieron TODOS en esos 5 segundos. El indicador reporta 83, la realidad fue 1000. **Error del 91%.**

---

### 🛠️ Propuestas de mejora

* **Descontinuar:** No tiene sentido intentar mejorar este código. La arquitectura basada en velas es un callejón sin salida para este propósito. La mejora es migrar a **SpeedOfTapeModif V2**.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es un indicador "placebo". Te da un número, te pinta un color, pero no te está contando la historia real de la cinta. En el trading profesional, una verdad a medias es una mentira completa.
Solo tiene valor como **código de reserva** por si fallan los servidores de datos de ticks y necesitamos una métrica "sucia" de velocidad.

**Propuestas de Acción:**
* Mover a carpeta `Deprecated` o `Reserva`.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Para scalping necesitamos precisión quirúrgica. Este indicador opera con machete.

**Acción:** **Conservar (Reserva / Educativo)**