---
cs_file: CotHigh.cs
name: COT High/Low
group: Order Flow
subgroup: Delta
score_current: 2/10
version: Estable
recommended_action: Descartar
description: ¿Acumula el delta desde un nuevo máximo o mínimo?
gemini_summary: "Concepto valioso ('Weis Wave' o 'Swing Delta') pero la implementación está rota. El modo 'Low' no funciona y el 'High' tiene lógica errónea. No usar."
comparison_group: "Cumulative Delta"
competitor_notes: "Inferior a cualquier implementación correcta de Weis Wave."
reusable_code: null
file_state: Roto
score_potential: 6/10
effort: Bajo
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 💀 COT High/Low (2/10)

**Nombre del archivo:** [`CotHigh.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CotHigh.cs)  
**Nombre del indicador:** COT High/Low  
**Web oficial:** [ATAS — COT High/Low](https://help.atas.net/support/solutions/articles/72000602603)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es el delta acumulado desde el último máximo/mínimo?

![COT High/Low](../../img/CotHigh.png)

---

### ⚙️ Parámetros configurables

* **Mode:** High o Low.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta 
**Comparison Group:** "Cumulative Delta"  

---

### 🧠 Uso más frecuente

* **(Teórico):** Ver la presión de compra/venta en las expansiones de rango.  

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

⛔ **ROTO:** El código del modo `Low` no existe lógicamente (nunca resetea).  
⛔ **ERROR:** El modo `High` resetea con condiciones de `Low`, lo cual es absurdo.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Información falsa.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar.** 
---

### 🧪 Notas de desarrollo

* Fallo lógico grave en `OnCalculate`: `if ((candle.High >= _extValue && Mode is CotMode.High) || (candle.Low >= _extValue && Mode is CotMode.High))`.  
* Falta el bloque `else if (Mode is CotMode.Low)`.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Todo el indicador requiere reescritura.** 

---

### 🛠️ Propuestas de mejora

* **Ninguna.** Existen indicadores de ondas (Weis Wave) que hacen esto bien. No vale la pena arreglar este script básico.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 

---

### ✍️ La opinión de Gemini sobre el Indicador

Basura técnica. Una buena idea mal ejecutada y abandonada.

**Propuestas de Acción:**
* **Descartar.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Peligroso por datos falsos.

**Acción:** **Descartar.**