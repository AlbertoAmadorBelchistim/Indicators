---
# 1. IDENTIFICACIÓN
cs_file:  DeltaStrength.cs  
name:  Delta Strength  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  Delta  
comparison_group:  "Bar Delta Analysis"  

# 3. VALORACIÓN (Score & Priority)
score_current:  2/10  
score_potential:  2/10  
file_state:  Estable  
effort:  Alto  
action_priority:  P4  
system_priority:  N/A  

# 4. DECISIÓN
recommended_action:  Descartar  

# 5. ANÁLISIS
description:  ¿Qué velas cierran con un delta dentro de un rango porcentual específico respecto a su extremo?  
gemini_summary:  "Conceptualmente roto. Su diseño excluye los valores del 100% (máxima agresión), ocultando paradójicamente la información más valiosa de Order Flow."  
competitor_notes:  "Delta Modif realiza análisis de fuerza y absorción correctamente sin excluir extremos."  
reusable_code:  null  

# 6. METADATOS
analysis_date:  2025-12-03  
official_code_date:  2025-04-23  
---

## ⛔ Delta Strength (2/10)

**Nombre del archivo:** [`DeltaStrength.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DeltaStrength.cs)  
**Nombre del indicador:** Delta Strength  
**Web oficial:** [ATAS - Delta Strength](https://help.atas.net/support/solutions/articles/72000602363)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Qué velas cierran con un delta dentro de un rango porcentual específico respecto a su extremo?

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
**Comparison Group:** "Bar Delta Analysis"  


---

### 🧠 Uso más frecuente

* **(Teórico)** Identificar velas que cierran "casi" en sus máximos de delta.  


---

### 📊 Nivel de relevancia
🔟 **2 / 10**

⛔ **Lógica Rota:** Si configuras 90-98%, ocultas las velas del 100% (las más fuertes). Absurdo.  


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

* **Exclusión de Outliers:** El código trata los valores extremos como ruido.  


---

### 🛠️ Propuestas de mejora

* **Ninguna.** Descartar.  


---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 

---

### ✍️ La opinión de Gemini sobre el Indicador

Un "Falso Amigo".

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No**

**Acción:** **Descartar**