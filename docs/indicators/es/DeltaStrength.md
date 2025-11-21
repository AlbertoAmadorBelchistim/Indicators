---
cs_file: DeltaStrength.cs
name: Delta Strength
group: Order Flow
subgroup: Delta
score_current: 2/10
version: Estable
recommended_action: Descartar
description: ¿Qué velas cierran con un delta dentro de un rango porcentual específico respecto a su extremo?
gemini_summary: "Conceptualmente roto. Su diseño de filtro 'pasa-banda' (ej. 90%-98%) excluye los valores del 100% (máxima agresión), ocultando paradójicamente la información más valiosa de Order Flow."
comparison_group: "Bar Delta"
competitor_notes: "Delta Modif realiza análisis de fuerza y absorción correctamente sin excluir extremos."
reusable_code: null
file_state: Estable (Lógica Defectuosa)
score_potential: 2/10
effort: Alto
action_priority: P4
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## ⛔ Delta Strength (2/10)

**Nombre del archivo:** [`DeltaStrength.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DeltaStrength.cs)  
**Nombre del indicador:** Delta Strength  
**Web oficial:** [ATAS - Delta Strength](https://help.atas.net/support/solutions/articles/72000602363)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Qué velas cierran con un delta dentro de un rango porcentual específico respecto a su extremo (MaxDelta/MinDelta)?

![DeltaStrength](../../img/DeltaStrength.png)

---

### ⚙️ Parámetros configurables

* **MaxFilter:** [Display] Límite superior del porcentaje (ej. 98).  
* **MinFilter:** [Display] Límite inferior del porcentaje (ej. 90).  
* **PosFilter / NegFilter:** [Display] Filtro por dirección de vela.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Bar Delta"  

---

### 🧠 Uso más frecuente

* **(Teórico)** Identificar velas que cierran "casi" en sus máximos de delta.  

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

⛔ **Lógica Rota:** Excluye el 100%. Si una vela cierra con Delta = MaxDelta (convicción total), este indicador la oculta si MaxFilter < 100.  
⛔ **Peligroso:** En Order Flow, los valores extremos son la señal, no el ruido a filtrar.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Induce a error grave por omisión de datos.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No Recomendado.** 

---

### 🧪 Notas de desarrollo

* Filtro restrictivo: `if (Delta >= Min && Delta <= Max)`.  
* Diseño conceptual erróneo para análisis de momentum.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Exclusión de Outliers:** El código trata los valores extremos como algo a evitar, cuando en Delta son lo que buscamos.  

---

### 🛠️ Propuestas de mejora

* **Ninguna.**

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 

---

### ✍️ La opinión de Gemini sobre el Indicador

Un "Falso Amigo". Su nombre sugiere medir fuerza, pero su matemática la oculta.

**Propuestas de Acción:**
* **Descartar.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

**Acción:** **Descartar.**