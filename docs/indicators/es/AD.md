---
cs_file: AD.cs
name: Accumulation/Distribution (A/D)
group: Order Flow
subgroup: Volume
score_current: 2/10
version: Estable
recommended_action: Descartar
description: ¿El flujo de volumen acumulado está confirmando la tendencia del precio?
gemini_summary: "Concepto obsoleto de los años 80 (Chaikin) que intenta estimar la acumulación basándose en el precio de cierre. Superado totalmente por el Delta real. Además, tiene un error de visualización (Histograma) que lo hace inutilizable."
comparison_group: "Classic Volume"
competitor_notes: "Inferior a cualquier indicador de Delta moderno."
reusable_code: null
file_state: Defectuoso (Visualización)
score_potential: 3/10
effort: Bajo
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 💀 AD (2/10)

**Nombre del archivo:** [`AD.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/AD.cs)  
**Nombre del indicador:** Accumulation/Distribution (A/D)  
**Web oficial:** [ATAS - Accumulation/Distribution (A/D)](https://help.atas.net/support/solutions/articles/72000606733)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿El flujo de volumen acumulado está confirmando la tendencia del precio?

![AD](../../img/AD.png)

---

### ⚙️ Parámetros configurables

* **N/A:** Sin parámetros.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume 
**Comparison Group:** "Classic Volume"  

---

### 🧠 Uso más frecuente

* **(Teórico):** Divergencias Precio vs Flujo.  

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

⛔ **Obsoleto:** Estima el flujo. El Delta lo *mide*.  
⛔ **Roto:** Se dibuja como histograma, impidiendo ver la acumulación.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** 
 
---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar.** 

---

### 🧪 Notas de desarrollo

* Fórmula clásica: `((C-L) - (H-C)) / (H-L) * V`.  
* Error: `VisualType = VisualMode.Histogram`. Debería ser `Line`.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Visualización:** El histograma reinicia visualmente la percepción en cada barra, ocultando la tendencia acumulada.  

---

### 🛠️ Propuestas de mejora

* **Ninguna.** Usar `CumulativeDelta`.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 
 
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una reliquia. Antes de que existiera el Order Flow (datos de Bid/Ask en tiempo real), esto era lo mejor que había. Hoy es como usar un mapa de papel teniendo GPS.

**Propuestas de Acción:**
* **Descartar.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Obsoleto y mal implementado.

**Acción:** **Descartar.**