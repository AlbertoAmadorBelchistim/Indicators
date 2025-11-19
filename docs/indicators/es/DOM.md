---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: DOM.cs
name: Depth of Market
category: OrderBook
score_current: 9/10
version: Latest
recommended_action: 'Conservar'
description: >-
  ¿Cuál es la liquidez (libro de órdenes) actual, dibujada en el gráfico?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: >-
  '"Herramienta 'Core' de Order Flow que visualiza el libro de órdenes' (DOM) directamente en el gráfico, esencial para scalping de liquidez."
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-17
official_code_date: 2025-11-10
user_modification_date: null
---

## 🟦 Depth of Market (DOM) (9/10)

**Nombre del archivo:** [`DOM.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DOM.cs)  
**Nombre del indicador:** Depth of Market  
**Web oficial:** [ATAS — Depth of Market](https://help.atas.net/support/solutions/articles/72000602367)  
**Compatibilidad:** ATAS versión latest y superiores.  
**Última revisión del código oficial:** 10/11/2025

> **La Pregunta Clave:** ¿Cuál es la liquidez (libro de órdenes) actual, dibujada en el gráfico?

![DOM](../../img/DOM.png)

---

### ⚙️ Parámetros configurables

* **VisualMode**: Modo de visualización (`Common`, `Cumulative`, `Combined`).
* **UseAutoSize / Width / RightToLeft / ProportionVolume**: Ajustes de escala y orientación.
* **BidRows / AskRows / TextColor / Backgrounds**: Colores personalizables por tipo de nivel.
* **FilterColors**: Filtros por volumen con color asignado.
* **ShowCumulativeValues**: Mostrar volumen total acumulado.
* **PriceLevelsHeight / Scale / UseScale / CustomScale**: Control visual de la escala y resolución de niveles.

---

### 🧭 Clasificación
📂 OrderBook — Indicadores de profundidad de mercado (nivel 2).

---

### 🧠 Uso más frecuente

* Visualizar el **libro de órdenes (bid/ask)** en el gráfico.
* Detectar **niveles con gran liquidez** (muros) o desequilibrio de órdenes.
* Evaluar volumen acumulado por nivel de precio (modo `Cumulative`).
* Identificar zonas de absorción o *spoofing* mediante colores o filtros.

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Herramienta "Core":** Ofrece lectura directa del DOM en el gráfico, indispensable para scalping de liquidez.  
✅ Altamente personalizable con múltiples modos y escalas.  
⛔ Complejo de configurar sin conocimiento previo.  
⛔ Consume recursos si el DOM es muy profundo.

---

### 🎯 Estrategias de scalping donde se aplica

* **Detección de Muros (Liquidez):** Operar reversiones o absorciones en niveles con gran volumen en el DOM.
* **Spoofing**: Detectar "muros" falsos que desaparecen justo antes de que el precio los toque.
* **Confirmación de Ruptura**: Validar rupturas si la liquidez opuesta ("muro" de venta en una ruptura alcista) es retirada.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **VisualMode**: `Combined` o `Cumulative`.
* **Width**: `150` (o al gusto).
* **FilterColors**: Asignar colores a volúmenes significativos (ej. ≥ 300, 1000 lotes en ES).
* **ShowCumulativeValues**: `true`.
* **UseAutoSize**: `true`.
* **Scale**: `20`.

---

### 🧪 Notas de desarrollo

* Es un indicador de **renderizado puro** que lee el `MarketDepthInfo.GetMarketDepthSnapshot()` y lo dibuja en el panel de precios.
* No *calcula* análisis, solo *visualiza* los datos del Nivel 2.
* La lógica de `OnRender` maneja el dibujo de las barras, texto y colores según los filtros.
* El modo `Cumulative` suma la liquidez desde el mejor Bid/Ask hacia afuera.

---

### 🛠️ Propuestas de mejora

* Añadir opción para **limitar la profundidad visualizada** (ej. "mostrar solo 10 niveles") para reducir el ruido visual y de CPU.
* Añadir alertas si un nivel de X lotes aparece a Y ticks del precio.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Esta no es una herramienta de "análisis", es la **visualización de los datos en bruto**. Para un scalper de Order Flow, es una de las tres herramientas fundamentales, junto con el *Footprint* (lo que se ejecutó) y el *Time & Sales* (cuándo se ejecutó).

`DOM` te muestra la **intención pasiva** (órdenes límite) en el gráfico. Te permite ver dónde están los "muros" de liquidez que pueden frenar el precio (absorción) o dónde está el "vacío" de liquidez que puede causar un movimiento rápido.

Tu puntuación de 9/10 es perfecta. Es una herramienta indispensable.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una herramienta principal indispensable.**

Permite al scalper leer la liquidez pasiva, identificar *icebergs* (órdenes grandes ocultas), y detectar *spoofing*.

**Acción:** **Conservar (Herramienta Principal).**