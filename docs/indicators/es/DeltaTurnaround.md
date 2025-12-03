---
# 1. IDENTIFICACIÓN
cs_file:  DeltaTurnaround.cs  
name:  Delta Turnaround  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  Delta  
comparison_group:  "Bar Delta Analysis"  

# 3. VALORACIÓN (Score & Priority)
score_current:  4/10  
score_potential:  4/10  
file_state:  Estable  
effort:  Bajo  
action_priority:  P4  
system_priority:  N/A  

# 4. DECISIÓN
recommended_action:  Descartar  

# 5. ANÁLISIS
description:  ¿Se ha producido un patrón de giro de 3 velas confirmado por el delta?  
gemini_summary:  "Demasiado rígido. Busca un patrón 'hard-coded' (A-A-B) que rara vez se da perfecto en mercados modernos. La detección de giros requiere análisis de contexto, no secuencias fijas."  
competitor_notes:  "Inferior a la lectura discrecional con Delta Modif."  
reusable_code:  null  

# 6. METADATOS
analysis_date:  2025-12-03  
official_code_date:  2025-07-31  
---

## 💀 Delta Turnaround (4/10)

**Nombre del archivo:** [`DeltaTurnaround.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DeltaTurnaround.cs)  
**Nombre del indicador:** Delta Turnaround  
**Web oficial:** [ATAS — Delta Turnaround](https://help.atas.net/support/solutions/articles/72000602364)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 31/07/2025  

> **La Pregunta Clave:** ¿Se ha producido un patrón de giro de 3 velas confirmado por el delta?

![DeltaTurnaround](../../img/DeltaTurnaround.png)


---

### ⚙️ Parámetros configurables

* **UseAlerts:** [Display] Activar alertas sonoras.  
* **AlertOnNewCandle:** [Display] Confirmación al cierre de vela.  
* **AlertFile / Colors:** [Display] Configuración de avisos.  
* *Nota: La lógica del patrón NO es configurable.*

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Bar Delta Analysis"  


---

### 🧠 Uso más frecuente

* **Automatización de Patrones:** Intentar detectar giros en V exactos.  


---

### 📊 Nivel de relevancia
🔟 **4 / 10**

⛔ **Rígido:** Busca una secuencia exacta de 3 velas. Cualquier variación rompe la señal.  


---

### 🎯 Estrategias de scalping donde se aplica

* **No Recomendado.**

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No Recomendado.**

---

### 🧪 Notas de desarrollo

* Lógica condicional simple (`if` anidados) comparando `candle`, `prevCandle` y `prev2Candle`.  
* Busca: `High >= Prev.High` AND `Delta < 0` (para cortos).  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Rigidez:** La lógica está "escrita en piedra" en el código.  


---

### 🛠️ Propuestas de mejora

* **Ninguna.** Para patrones, usar `BarsPattern`. Para Delta, usar `Delta Modif`.  


---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 

---

### ✍️ La opinión de Gemini sobre el Indicador

Script básico y obsoleto.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No**

**Acción:** **Descartar**