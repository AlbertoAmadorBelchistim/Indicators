---
# 1. IDENTIFICACIÓN
cs_file: MarketPower.cs
name: CVD pro / Market Power
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Delta
comparison_group: "Cumulative Delta"

# 3. VALORACIÓN (Score & Priority)
score_current: 6/10
score_potential: 6/10
file_state: Estable (Redundante)
effort: N/A
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Descartar

# 5. ANÁLISIS
description: ¿Cuál es el delta acumulado filtrado por tamaño de trade y su media móvil?
gemini_summary: "Versión obsoleta o simplificada de MultiMarketPower. Solo permite un filtro a la vez. Dado que MMP hace lo mismo x5 de forma eficiente, este indicador es redundante."
competitor_notes: "Absorbido y superado por MultiMarketPower."
reusable_code: "Lógica de SMA sobre CVD."

# 6. METADATOS
analysis_date: 2025-11-21
official_code_date: 2025-04-23
---

## 💀 CVD pro / Market Power (6/10)

**Nombre del archivo:** [`MarketPower.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MarketPower.cs)  
**Nombre del indicador:** CVD pro / Market Power  
**Web oficial:** [ATAS — Market Power](https://help.atas.net/support/solutions/articles/72000602424)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es el delta acumulado filtrado por tamaño de trade y su media móvil?

![MarketPower](../../img/MarketPower.png)

---

### ⚙️ Parámetros configurables

* **SmaPeriod:** Periodo de la media móvil aplicada al CVD.  
* **CumulativeTrades:** Modo acumulado o tick.  
* **Min/Max Volume:** Un solo rango de filtro de volumen.  
* **ShowSma / ShowCumulative:** Opciones de visualización.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Cumulative Delta"  

---

### 🧠 Uso más frecuente

* **Histórico:** Filtrar delta institucional antes de que existiera MMP.  

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ **SMA Integrada:** Tiene una media móvil que MMP no trae de serie.  
⛔ **Limitado:** Solo 1 filtro. MMP tiene 5.  
⛔ **Redundante:** Ocupa espacio y recursos duplicando funciones.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna recomendable.** (Usar MMP para esta función).  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **N/A.** (Indicador descartado).  

---

### 🧪 Notas de desarrollo

* Código robusto con gestión de gaps (`ConcurrentQueue`).  
* Implementación de SMA interna que no existe en el indicador ganador.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Obsolescencia:** Ha sido superado por la evolución del software (MultiMarketPower).  

---

### 🛠️ Propuestas de mejora

* **Ninguna.** 
 
---

### 💎 Valor Reutilizable (Código Donante)

* **SMA Integrada:** Portar la lógica de la media móvil (`_smaSeries`) a `MultiMarketPower` sería una mejora táctica interesante (Cruce de Delta Institucional sobre su media).  

---

### ✍️ La opinión de Gemini sobre el Indicador

Fue un buen indicador en su momento. Ahora es un "padre" superado por su "hijo". No hay razón operativa para mantenerlo instalado.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Ineficiente comparado con el ganador.

**Acción:** **Descartar**