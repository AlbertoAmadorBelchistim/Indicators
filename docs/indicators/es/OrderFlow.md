---
cs_file: OrderFlow.cs
name: Order Flow Indicator
category: Order Flow
group: Order Flow
subgroup: Volume
score_current: 9/10
version: Stable
recommended_action: Conservar
description: ¿Cómo se visualiza el flujo de órdenes (trades individuales) en el gráfico?
gemini_summary: "Visualizador de Order Flow sólido (Círculos/Rectángulos). Pequeño riesgo en OnRender pero funcional."
comparison_group: "Tape Analysis"
competitor_notes: "Estándar visual."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-18
official_code_date: 23/04/2025
---

## 🟦 Order Flow Indicator (9/10)

**Nombre del archivo:** [`OrderFlow.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OrderFlow.cs)  
**Nombre del indicador:** Order Flow Indicator  
**Web oficial:** [ATAS — Order Flow Indicator](https://help.atas.net/support/solutions/articles/72000602441)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cómo se visualiza el flujo de órdenes (trades individuales o acumulados) en el gráfico (círculos/rectángulos)?

![OrderFlow](../../img/OrderFlow.png)

---

### ⚙️ Parámetros configurables

* **VisMode**: Forma de visualización (`Circles`, `Rectangles`)
* **TradesMode**: Tipo de trade (`Cumulative`, `Separated`)
* **Filter**: Volumen mínimo para mostrar
* **ShowSmallTrades**: Mostrar trades con volumen inferior al filtro
* **CombineSmallTrades**: Agrupar pequeños trades del mismo precio
* **Size / Spacing**: Tamaño y espaciado de los objetos
* **SpeedInterval**: Frecuencia de actualización (ms)
* **Offset**: Separación horizontal respecto al precio actual
* **UseAlerts / AlertFilter**: Sistema de alertas por volumen

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Visualización granular del flujo de órdenes con volumen y dirección

---

### 🧠 Uso más frecuente

* Visualizar **trades individuales o acumulados** con color y volumen
* Confirmar momentos de **agresión direccional relevante**
* Identificar **intensidad de entrada institucional o desequilibrio**

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Muy visual e intuitivo para seguir el flujo de órdenes en tiempo real  
✅ Configurable para trabajar con trades individuales o bloques acumulados  
⛔ Requiere buena configuración para evitar saturación visual

---

### 🎯 Estrategias de scalping donde se aplica

* **Entrada por agresión clara** (conglomerado de puntos verdes/rojos grandes)
* **Confirmación de ruptura** si aparecen grandes trades sobre nivel clave
* **Filtro direccional** si hay asimetría clara en la agresión de compra o venta

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **TradesMode**: `Cumulative`
* **Filter**: `15`
* **CombineSmallTrades**: `true`
* **Spacing**: `8`
* **Size**: `12`
* **UseAlerts**: `true`, **AlertFilter**: `30`

---

### 🧪 Notas de desarrollo

* Usa `OnNewTrade` o `OnCumulativeTrade` para capturar datos en tiempo real
* Almacena los trades en listas (`_trades`, `_singleTrades`) y las limpia periódicamente (`CleanUpTrades`) para no saturar la memoria
* Dibuja visualmente objetos (`Ellipse` o `Rect`) con color, tamaño y volumen usando `OnRender`
* Se actualiza por temporizador interno (`OnTimerCall`) para controlar la carga de renderizado

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta de visualización de Order Flow muy útil y bien implementada. El uso de un temporizador (`SpeedInterval`) para controlar el redibujado es una decisión inteligente para evitar lag en mercados rápidos.

El código tiene un pequeño riesgo de concurrencia en `OnRender`: aunque usa `lock`, accede a las propiedades de los trades (`_trades[i].Volume`) fuera del bloque de bloqueo principal en algunas partes, lo que *podría* causar problemas si la lista se modifica simultáneamente, aunque la estructura del código intenta minimizar esto copiando datos a listas locales (`ellipses`). En la práctica, es estable.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Ver la agresión en tiempo real (bolas grandes entrando) es una de las señales más directas para el scalping.

**Acción:** **Conservar (Herramienta visual sólida).**
