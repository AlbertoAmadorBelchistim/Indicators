---
cs_file: BidAsk.cs
name: Bid Ask
category: Order Flow
score_current: 6.5/10
version: Estable
recommended_action: Mejorar
description: ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela?

# --- Análisis y Triaje de Gemini ---
gemini_summary: El histograma de OF más puro (6.5/10). Muestra la "batalla" (Bid vs Ask), no solo el resultado (Delta).
file_state: Mejorable
score_potential: 7/10
effort: Bajo
action_priority: P3 (Mejora Opcional)
analysis_date: 2025-11-17
official_code_date: 23/04/2025
user_modification_date: null
# ------------------------------------
---

## 🟦 Bid Ask (6.5/10 | Potencial: 7/10)

**Nombre del archivo:** [`BidAsk.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BidAsk.cs)  
**Nombre del indicador:** Bid Ask  
**Web oficial:** [ATAS — Bid Ask](https://help.atas.net/support/solutions/articles/72000602329)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela?

![BidAsk](../../img/BidAsk.png)


-----

### ⚙️ Parámetros configurables

Este indicador **no tiene parámetros configurables**.

  * Los colores del histograma se heredan automáticamente del esquema de Footprint del gráfico (`FootprintBidColor`, `FootprintAskColor`).

-----

### 🧭 Clasificación

📂 VolumeOrderFlow — Histograma de agresión Bid y Ask por vela.

-----

### 🧠 Uso más frecuente

  * Visualizar en cada vela la **agresión de compra (Ask)** y **venta (Bid)** por separado.
  * Evaluar rápidamente la presión de compradores o vendedores barra a barra.
  * Confirmar si el **movimiento del precio está acompañado por desequilibrio agresivo**.

-----

### 📊 Nivel de relevancia

🔟 **6.5 / 10**

✅ **Visualización Clara:** Más informativo que un simple histograma de Delta, ya que muestra la "batalla completa" (ej. un Delta de +100 puede ser `Ask 1000 vs Bid 900` (alta lucha) o `Ask 110 vs Bid 10` (dominio total)).  
✅ Ideal para confirmar zonas de rechazo o absorción.  
⛔ **No Configurable:** No permite filtros de ningún tipo.  
⛔ **Puede ser Ruidoso:** Ver ambos lados de la agresión puede ser más "ruidoso" y menos directo que ver solo el Delta neto.

-----

### 🎯 Estrategias de scalping donde se aplica

  * **Confirmar Rupturas Reales**: Fuerte `Ask` (histograma verde) acompañando un breakout alcista.
  * **Detección de Absorciones**: Una vela alcista (verde) que tiene un `Bid` (histograma rojo) *mayor* que el `Ask`, indicando absorción de ventas.
  * **Evaluar Control de la Vela**: Visualización directa de quién dominó la agresión.

-----

### ⚙️ Parametrización óptima para scalping (1M, S\&P 500)

  * N/A. El indicador no tiene parámetros.

-----

### 🧪 Notas de desarrollo

  * Usa dos `ValueDataSeries` (`_bids` y `_asks`) y las muestra como un histograma dual.
  * **`_bids[bar] = -candle.Bid;`**: El volumen Bid se multiplica por -1 para que se dibuje hacia abajo, separado del Ask.
  * **`_asks[bar] = candle.Ask;`**: El volumen Ask se dibuja hacia arriba.
  * Los colores se ajustan automáticamente en `OnApplyDefaultColors()` según los colores de Footprint activos en la plataforma.

-----

### ❗ Incoherencias o aspectos mejorables detectados

  * El indicador es simple y funcional. Su principal "defecto" es la falta de configurabilidad (filtros, colores, etc.), lo cual es una decisión de diseño.

-----

### 🛠️ Propuestas de mejora

  * Añadir una opción para **mostrar el Delta neto (`Ask - Bid`)** como una tercera barra o línea superpuesta.
  * Incluir filtros por volumen mínimo para ignorar velas de bajo interés.
  * Añadir una línea de cero (`ShowZeroValue = true`).

-----

-----

### ✍️ La opinión de Gemini sobre el Indicador

Este es uno de los indicadores de Order Flow más "puros" y fundamentales. Tu puntuación de 6.5/10 es la correcta, ya que captura perfectamente el "trade-off" de esta herramienta:

  * **Lo Bueno (El 6.5):** Es *más* información que un simple histograma de Delta. El Delta solo te dice el *resultado neto* (`Ask - Bid`). Este indicador te muestra la *batalla completa*. Por ejemplo:

      * Un Delta de **+100** puede ser `Ask 1000` vs `Bid 900` (una gran batalla).
      * Un Delta de **+100** puede ser `Ask 110` vs `Bid 10` (dominio comprador total).
        Este indicador te permite ver esa diferencia.

  * **Lo Malo (El -3.5):** Es información más "ruidosa" y, a menudo, **redundante**. Para tomar decisiones rápidas de scalping, el **Delta neto** (`Ask - Bid`) suele ser una métrica más limpia y directa.

-----

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, pero con reservas (6.5/10).**

Es una herramienta de "análisis profundo" más que una herramienta de "señales rápidas". Es útil para entender *por qué* el Delta es el que es. Si ya usas un histograma de Delta, este puede ser redundante.

**Acción:** **Mejorar (Prioridad P3).**

**¿Merece la pena mejorarlo?** **Sí.** El indicador funciona perfectamente (6.5/10). El arreglo es trivial (`effort: Bajo`) y consiste en añadir una línea de cero (`ShowZeroValue = true`), lo cual mejora la legibilidad y lo convierte en una herramienta 7/10.