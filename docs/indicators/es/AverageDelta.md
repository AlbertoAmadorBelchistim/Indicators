---
# 1. IDENTIFICACIÓN
cs_file:  AverageDelta.cs  
name:  Average Delta  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  Delta  
comparison_group:  "Bar Delta Analysis"  

# 3. VALORACIÓN (Score & Priority)
score_current:  3/10  
score_potential:  5/10  
file_state:  Estable  
effort:  Bajo  
action_priority:  P4  
system_priority:  N/A  

# 4. DECISIÓN
recommended_action:  Descartar  

# 5. ANÁLISIS
description:  ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas?  
gemini_summary:  "Indicador básico que aplica una media móvil (SMA/EMA) al delta. Es ineficiente ocupar un panel entero para esto. Su utilidad real sería como una línea superpuesta en el indicador Delta principal."  
competitor_notes:  "Inferior a Delta Modif. La media móvil debería ser una feature de Delta Modif, no un indicador aparte."  
reusable_code:  "Lógica de SMA/EMA sobre valores de Delta."  

# 6. METADATOS
analysis_date:  2025-12-03  
official_code_date:  2025-04-23  
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
**Comparison Group:** "Bar Delta Analysis"  


---

### 🧠 Uso más frecuente

* **Filtro de Tendencia:** Ver si el flujo general es comprador o vendedor, ignorando el ruido de una vela individual.  


---

### 📊 Nivel de relevancia
🔟 **3 / 10**

⛔ **Ineficiente:** Ocupa demasiado espacio en pantalla para la poca información que da.  
⛔ **Lag:** Las medias móviles en Delta suelen ir retrasadas.  


---

### 🎯 Estrategias de scalping donde se aplica

* Ninguna recomendada como indicador standalone.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No Recomendado.** 
 
---

### 🧪 Notas de desarrollo

* Utiliza las clases `SMA` y `EMA` nativas de ATAS sobre `candle.Delta`.  


---

### ❗ Incoherencias o aspectos mejorables detectados

* **Diseño:** Debería ser una línea superpuesta (Overlay) sobre el histograma de Delta principal, no un indicador separado.  


---

### 🛠️ Propuestas de mejora

* **Fusión:** Integrar una opción "Show Average Line" dentro de `Delta Modif` en el futuro.  


---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Suavizado:** Útil para ver desviaciones rápidas respecto a la media.  


---

### ✍️ La opinión de Gemini sobre el Indicador

No merece un archivo propio en un sistema moderno.


---

### 📈 Veredicto: ¿Es útil para Scalping?

**No**

**Acción:** **Descartar**