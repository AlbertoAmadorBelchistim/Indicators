---
# 1. IDENTIFICACIÓN
cs_file: CotHigh.cs
name: COT High/Low
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Delta
comparison_group: "Cumulative Delta"

# 3. VALORACIÓN (Score & Priority)
score_current: 2/10
score_potential: 2/10
file_state: Defectuoso
effort: Bajo
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Descartar

# 5. ANÁLISIS
description: ¿Acumula el delta desde un nuevo máximo o mínimo?
gemini_summary: "Intento fallido de Weis Wave. El código contiene errores lógicos graves en el reseteo de acumulados y manejo de modos. Proporciona datos falsos."
competitor_notes: "Inferior a cualquier implementación correcta de Weis Wave."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-11-21
official_code_date: 2025-04-23
---

## 💀 COT High/Low (2/10)

**Nombre del archivo:** [`CotHigh.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CotHigh.cs)  
**Nombre del indicador:** COT High/Low  
**Web oficial:** [ATAS — COT High/Low](https://help.atas.net/support/solutions/articles/72000602603)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Acumula el delta desde un nuevo máximo o mínimo?

![CotHigh](../../img/CotHigh.png)

---

### ⚙️ Parámetros configurables

* **Mode:** `High` (Reinicia en nuevos máximos) o `Low` (Reinicia en nuevos mínimos).  
* **PosColor / NegColor:** Colores del histograma.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Cumulative Delta"  

---

### 🧠 Uso más frecuente

* **(Teórico):** Intentar ver la presión de compra/venta en las expansiones de rango (Swing Delta).  

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

⛔ **ROTO:** El código del modo `Low` no existe lógicamente (nunca resetea).  
⛔ **ERROR:** El modo `High` resetea con condiciones de `Low`, lo cual es absurdo.  
⛔ **Peligroso:** Da señales falsas de divergencia por mala acumulación.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Información falsa.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar.**
 
---

### 🧪 Notas de desarrollo

* Fallo lógico grave en `OnCalculate`: La condición `if` mezcla `Mode.High` con `candle.Low` de forma incorrecta.  
* Falta el bloque lógico para `Mode.Low`.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Todo el indicador requiere reescritura.** No cumple su especificación básica.  

---

### 🛠️ Propuestas de mejora

* **Ninguna.** Existen indicadores de ondas (Weis Wave) mejores. No vale la pena arreglar este script.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 
 
---

### ✍️ La opinión de Gemini sobre el Indicador

Basura técnica. Una buena idea (Swing Delta) mal implementada y abandonada.


---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Peligroso por datos falsos.

**Acción:** **Descartar**