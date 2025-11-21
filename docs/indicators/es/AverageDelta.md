---
cs_file: AverageDelta.cs
name: Average Delta
group: Order Flow
subgroup: Delta
score_current: 3/10
version: Estable
recommended_action: Descartar
description: ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas?
gemini_summary: "Funcionalidad básica (SMA/EMA del Delta) que no justifica un indicador independiente ocupando un panel propio. Delta Modif ya ofrece análisis contextual superior mediante desviación estándar."
comparison_group: "Bar Delta"
competitor_notes: "Inferior a Delta Modif. Su única utilidad (ver la tendencia del delta) debería ser una línea superpuesta en el indicador principal, no un panel aparte."
reusable_code: "Lógica de cálculo de SMA/EMA sobre valores puros de Delta (para futura integración en Delta Modif)."
file_state: Estable
score_potential: 5/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 💀 Average Delta (3/10)

**Nombre del archivo:** [`AverageDelta.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/AverageDelta.cs)  
**Nombre del indicador:** Average Delta  
**Web oficial:** [ATAS - Average Delta](https://help.atas.net/support/solutions/articles/72000618456)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas?

![AverageDelta](../../img/AverageDelta.png)

---

### ⚙️ Parámetros configurables

* **Period:** [Parameter] Número de barras para el cálculo de la media (por defecto 10).  
* **CalcType:** [Display] Tipo de cálculo: `Sma` (Simple) o `Ema` (Exponencial).  
* **PosColor / NegColor:** [Display] Colores del histograma.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Bar Delta"  

---

### 🧠 Uso más frecuente

* **Filtro de Tendencia:** Suavizar el ruido vela a vela para ver si el flujo de órdenes dominante es comprador o vendedor.  
* **Régimen de Mercado:** Identificar cambios en el comportamiento del delta a medio plazo.  

---

### 📊 Nivel de relevancia
🔟 **3 / 10**

✅ **Claridad:** Elimina el ruido de los picos de delta individuales.  
⛔ **Ineficiente:** Ocupa un sub-panel completo para mostrar una sola línea/histograma.  
⛔ **Lag:** Las medias móviles en datos de flujo de órdenes suelen ir muy retrasadas respecto a la acción del precio.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Divergencia de Flujo:** Precio haciendo nuevos máximos mientras el Average Delta decrece (Agotamiento).  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No Recomendado.** Mejor usar el CVD (Cumulative Volume Delta) para ver tendencias de flujo acumulado.  

---

### 🧪 Notas de desarrollo

* Utiliza las clases `SMA` y `EMA` nativas de ATAS sobre `candle.Delta`.  
* Visualización tipo Histograma.  
* No tiene opción de línea cero explícita (`ShowZeroValue` está a `false` por defecto en la serie).  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Diseño:** Debería ser una línea superpuesta (Overlay) sobre el histograma de Delta principal, no un indicador separado.  

---

### 🛠️ Propuestas de mejora

* **Fusión (P2):** Añadir una opción "Show Average Line" (SMA/EMA) dentro del indicador ganador `Delta Modif`.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Suavizado:** La idea de tener una línea de media móvil sobre el histograma de Delta es útil para ver desviaciones rápidas respecto a la media. Esta lógica es candidata a ser absorbida por `Delta Modif`.  

---

### ✍️ La opinión de Gemini sobre el Indicador

Como archivo independiente es prescindible. Ocupa espacio valioso en pantalla. Sin embargo, la **información** que provee (la media del delta) es útil si se presenta correctamente (superpuesta).

**Propuestas de Acción:**
* **Descartar** el archivo independiente.
* **Anotar** la funcionalidad para futura fusión en `Delta Modif`.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Demasiado espacio para poca información.

**Acción:** **Descartar.**