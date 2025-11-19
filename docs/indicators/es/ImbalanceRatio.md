---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: ImbalanceRatio.cs
name: Imbalance Ratio
category: VolumeOrderFlow
score_current: 9/10
version: ATAS Official
recommended_action: 'Conservar'
description: >-
  ¿Dónde se están produciendo desequilibrios (imbalances) diagonales de Ask vs. Bid en el clúster que superan un ratio y volumen mínimos?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: >-
  Implementación 'Core' y estable del Imbalance de clúster (diagonal); incluye filtros clave (Ratio, VolumeFilter) y un resumen de texto en la vela.
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-17
official_code_date: 2025-05-27
user_modification_date: null
---

## 🟦 Imbalance Ratio (9/10)

**Nombre del archivo:** [`ImbalanceRatio.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ImbalanceRatio.cs)  
**Nombre del indicador:** Imbalance Ratio  
**Web oficial:** [ATAS — Imbalance Ratio](https://help.atas.net/support/solutions/articles/72000602404)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 27/05/2025

> **La Pregunta Clave:** ¿Dónde se están produciendo desequilibrios (imbalances) diagonales de Ask vs. Bid en el clúster que superan un ratio y volumen mínimos?

![Imbalance Ratio](../../img/ImbalanceRatio.png)

---

### ⚙️ Parámetros configurables

* **Ratio**: Relación mínima Ask/Bid (o Bid/Ask) para considerar un desequilibrio (por defecto: 4)
* **VolumeFilter**: Volumen mínimo en el par de niveles para considerar el desequilibrio
* **Transparency**: Transparencia del clúster visualizado (0 a 100)
* **BuyColor / SellColor**: Colores para desequilibrios de compra o venta
* **TextColor**: Color del texto que muestra la cantidad de desequilibrios por vela
* **ShowTop / ShowBot**: Mostrar el resumen de texto arriba o abajo de la vela

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Detección de desequilibrios agresivos entre Bid y Ask por nivel

---

### 🧠 Uso más frecuente

* Detectar **zonas de desequilibrio agresivo** en el clúster
* Visualizar acumulaciones de compras o ventas dominantes (stacked imbalances)
* Usar como filtro o confirmación de **absorciones, agotamientos o empuje institucional**

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Herramienta "Core" de Order Flow**: Es la implementación estándar y esencial de Imbalance de clúster.  
✅ **Filtros Clave**: `Ratio` y `VolumeFilter` son cruciales para eliminar el ruido y encontrar agresión significativa.  
✅ **Resumen Visual**: El conteo (`{buyRows}x{sellRows}`) en `OnRender` es una excelente ayuda visual.  
✅ Código estable y eficiente.

---

### 🎯 Estrategias de scalping donde se aplica

* **Ruptura con desequilibrio agresivo**: Entrada si aparecen *stacked imbalances* (desequilibrios apilados) en dirección de la ruptura.
* **Absorción Inversa**: Entrada contraria si aparece un gran desequilibrio justo en una zona de S/R (ej. absorción).
* **Seguimiento de "Dinero Inteligente"**: Múltiples desequilibrios agrupados indican presión institucional.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Ratio**: `3` a `5` (4 es un buen estándar)
* **VolumeFilter**: `300` a `800` (depende de la liquidez del momento)
* **Transparency**: `50`

---

### 🧪 Notas de desarrollo

* El indicador recorre el clúster comparando ticks diagonales.
* **Imbalance de Compra**: `upperInfo.Ask / lowerInfo.Bid >= _imbalanceRatio`.
* **Imbalance de Venta**: `lowerInfo.Bid / upperInfo.Ask >= _imbalanceRatio`.
* Ambos están protegidos por un `VolumeFilter` y protección contra división por cero (ej. `lowerInfo.Bid == 0`).
* Los desequilibrios se representan con `PriceSelectionValue` y `ObjectType.OnlyCluster` para dibujar solo el resaltado del clúster.
* `OnRender` dibuja el resumen de texto (`buyRows}x{sellRows}`) encima o debajo de la vela.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Este es un indicador "Core" indispensable para cualquier scalper de Order Flow. Es la herramienta fundamental para "leer dentro" de la vela.

La implementación de ATAS es excelente: es rápida, visualmente clara y, lo más importante, incluye los dos filtros cruciales que la hacen profesional: `Ratio` y `VolumeFilter`. Sin estos filtros, un gráfico de imbalance es solo ruido. Con ellos, el scalper puede aislar con precisión dónde está entrando la agresión *significativa*.

El resumen de texto (`{buyRows}x{sellRows}`) que dibuja en `OnRender` es una característica de QoL (Calidad de Vida) fantástica que te da un resumen instantáneo de la batalla de la vela. Es una herramienta estable y de 9/10.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una herramienta principal indispensable.**

Es la base del trading de Order Flow. Se usa para confirmar rupturas, detectar absorción y seguir la agresión institucional.

**Acción:** **Conservar (Herramienta Principal).**