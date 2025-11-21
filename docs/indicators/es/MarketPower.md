---
cs_file: MarketPower.cs
name: CVD pro / Market Power
group: Order Flow
subgroup: Delta
score_current: 6/10
version: Estable
recommended_action: Descartar
description: ¿Cuál es el delta acumulado filtrado por tamaño de trade y su media móvil?
gemini_summary: "Versión obsoleta de MultiMarketPower. Solo permite un filtro a la vez. MultiMarketPower hace lo mismo x5 y más eficiente. No tiene sentido mantenerlo."
comparison_group: "Cumulative Delta"
competitor_notes: "Absorbido y superado por MultiMarketPower."
reusable_code: "Lógica de SMA sobre CVD."
file_state: Estable (Redundante)
score_potential: 6/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 💀 CVD pro / Market Power (6/10)

**Nombre del archivo:** [`MarketPower.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MarketPower.cs)  
**Nombre del indicador:** CVD pro / Market Power  
**Web oficial:** [ATAS — CVD pro / Market Power](https://help.atas.net/support/solutions/articles/72000602424)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es el delta acumulado filtrado por tamaño de trade y su media móvil?

![MarketPower](../../img/MarketPower.png)

---

### ⚙️ Parámetros configurables

* **SmaPeriod:** [Parameter] Suavizado del CVD.  
* **CumulativeTrades:** [Display] Modo acumulado o tick.  
* **Min/Max Volume:** [Display] Un solo rango de filtro.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta
**Comparison Group:** "Cumulative Delta"  

---

### 🧠 Uso más frecuente

* **Histórico:** Filtrar delta institucional (antes de que existiera MMP).  

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ **SMA Integrada:** Tiene una media móvil que MMP no trae de serie.  
⛔ **Limitado:** Solo 1 filtro. MMP tiene 5.  
⛔ **Redundante:** Ocupa espacio y recursos duplicando funciones.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Cruce de SMA:** Delta cruzando su media.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No Recomendado.** ---

### 🧪 Notas de desarrollo

* Código robusto con gestión de gaps (`ConcurrentQueue`).  
* Implementación de SMA interna.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Obsolescencia:** Ha sido superado por la evolución del software (MMP).  

---

### 🛠️ Propuestas de mejora

* **Ninguna.** 

---

### 💎 Valor Reutilizable (Código Donante)

* **SMA Integrada:** Portar la lógica de la media móvil (`_smaSeries`) a `MultiMarketPower` sería una mejora táctica interesante (Cruce de Delta Institucional sobre su media).  

---

### ✍️ La opinión de Gemini sobre el Indicador

Fue un buen indicador. Ahora es un "padre" superado por su "hijo". No hay razón para usarlo teniendo MMP.

**Propuestas de Acción:**
* **Descartar.**
* Anotar la idea de la SMA para futura mejora de MMP.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Ineficiente comparado con el ganador.

**Acción:** **Descartar.**