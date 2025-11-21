---
cs_file: AskBidBars.cs
name: Ask/Bid Volume Difference Bars
group: Order Flow
subgroup: Delta
score_current: 6.5/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Cuál fue el volumen agresivo neto (Delta) de esta vela y cuál fue el rango total de la lucha entre compradores y vendedores?
gemini_summary: "Conceptualmente brillante: cuenta la 'historia completa' del Delta (Máximo, Mínimo y Neto) en una sola barra. Sin embargo, falla en la ejecución visual actual al carecer de una línea cero de referencia, lo que la hace inferior al ganador del grupo."
comparison_group: "Bar Delta"
competitor_notes: "Ofrece una visualización estructural (High/Low) que Delta Modif puede replicar, pero Ask/Bid Bars es más ligero. Se mantiene como alternativa 'Lite'."
reusable_code: "Lógica de mapeo High=MaxDelta, Low=MinDelta para crear velas sintéticas."
file_state: Estable (Con defecto de UI)
score_potential: 8/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Ask/Bid Volume Difference Bars (6.5/10)

**Nombre del archivo:** [`AskBidBars.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/AskBidBars.cs)  
**Nombre del indicador:** Ask/Bid Volume Difference Bars  
**Web oficial:** [ATAS — Ask/Bid Volume Difference Bars](https://help.atas.net/support/solutions/articles/72000602527)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál fue el volumen agresivo neto (Delta) de esta vela y cuál fue el rango total de la lucha entre compradores y vendedores?

![AskBidBars](../../img/AskBidBars.png)

---

### ⚙️ Parámetros configurables

* **N/A:** El indicador no expone parámetros de configuración lógica en la interfaz (UI).  
* *Nota: Los colores se gestionan directamente desde la configuración de la serie de datos (Candles).*

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Bar Delta"  

---

### 🧠 Uso más frecuente

* **Narrativa de la Vela (Storytelling):** Muestra el recorrido completo de la agresividad. Puedes ver hasta dónde empujaron los compradores (`High/MaxDelta`) y los vendedores (`Low/MinDelta`) antes de cerrar (`Close/Delta`).  
* **Detección de Absorción:** Una vela con una mecha inferior muy larga (`MinDelta` muy negativo) que cierra positiva o cerca de cero indica que toda la venta agresiva fue absorbida.  
* **Identificación de Agotamiento:** Velas con mechas largas por ambos lados pero con un cuerpo (Delta neto) muy pequeño.  

---

### 📊 Nivel de relevancia
🔟 **6.5 / 10**

✅ **Riqueza de Información:** Condensa tres datos críticos (Delta Máximo, Mínimo y Neto) en un solo glifo visual.  
✅ **Visualización de Volatilidad:** Permite ver si el Delta se movió de forma direccional o errática dentro de la vela.  
⛔ **Fallo de UI (Crítico):** No dibuja una línea cero (`ShowZeroValue` no está explícito). Esto hace que las velas "floten" en el panel, dificultando la lectura rápida.  
⛔ **Redundancia:** `Delta Modif` (Ganador) replica esta funcionalidad en modo "Candles" añadiendo alertas dinámicas.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Reversión por Absorción:** En soporte, buscar vela con mecha inferior larga (Delta negativo) que cierra verde.  
* **Confirmación de Breakout:** En rotura, buscar vela con cuerpo grande y sin mechas en contra.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Configuración Manual Requerida:** Dado que no tiene parámetros, es vital añadir manualmente una línea horizontal en el nivel `0` para tener referencia visual.  

---

### 🧪 Notas de desarrollo

* Utiliza una `CandleDataSeries` personalizada llamada `_renderSeries`.  
* Mapeo: `High` = `MaxDelta`, `Low` = `MinDelta`, `Close` = `Delta`.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta de Referencia Cero:** El código no incluye `ShowZeroValue = true`.  
* **Gestión de Colores:** Hereda colores del gráfico principal, lo que puede confundir.  

---

### 🛠️ Propuestas de mejora

* **Mejora UI (P3):** Añadir línea cero fija en el código. Es sencillo, pero al ser un indicador de "Reserva", la prioridad es baja.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Mapeo:** El concepto `High=Max` y `Low=Min` debe estar presente en el indicador Ganador.  

---

### ✍️ La opinión de Gemini sobre el Indicador

Idea excelente con ejecución incompleta. Es superior a un histograma simple porque revela la **lucha** (rango), pero inferior al ganador `Delta Modif` porque carece de contexto estadístico y tiene un fallo visual (falta línea cero). Se queda como un buen suplente ligero.

**Propuestas de Acción:**
* **Conservar como Reserva.** Si alguna vez se necesita una versión "Lite", se le debe aplicar el parche de la línea cero.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Excelente detector de absorción visual.

**Acción:** **Conservar (Reserva).**