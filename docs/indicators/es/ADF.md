---
cs_file: ADF.cs
name: Accumulation / Distribution Flow
group: Order Flow
subgroup: Volume
score_current: 1/10
version: Estable
recommended_action: Descartar
description: ¿Cuál es la tendencia suavizada del flujo de volumen acumulado?
gemini_summary: "Toma un concepto obsoleto (AD) y lo hace más lento aplicándole una SMA. Es 'lag sobre lag'. Totalmente inútil para la toma de decisiones en tiempo real."
comparison_group: "Classic Volume"
competitor_notes: "El peor de su clase."
reusable_code: null
file_state: Estable
score_potential: 1/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 💀 Accumulation / Distribution Flow (1/10)

**Nombre del archivo:** [`ADF.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ADF.cs)  
**Nombre del indicador:** Accumulation / Distribution Flow  
**Web oficial:** [ATAS — Accumulation / Distribution Flow](https://help.atas.net/support/solutions/articles/72000602569)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la tendencia suavizada del flujo de volumen acumulado?

![ADF](../../img/ADF.png)

---

### ⚙️ Parámetros configurables

* **Period:** Suavizado SMA (Default 14).  
* **UsePrev:** Usar cierre anterior.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume (Classic)  
**Comparison Group:** "Classic Volume"  

---

### 🧠 Uso más frecuente

* **(Teórico):** Tendencia de flujo de largo plazo.  

---

### 📊 Nivel de relevancia
🔟 **1 / 10**

⛔ **Lag Extremo:** Aplica SMA a un acumulativo. La señal llega tardísimo.  
⛔ **Redundante:** No aporta nada que el precio no diga ya.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** 

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar.** 

---

### 🧪 Notas de desarrollo

* Calcula AD y luego aplica `_sma.Calculate`.  
* También usa `VisualMode.Histogram` erróneamente.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Diseño:** Suavizar una señal de acumulación es contraproducente para el timing.  

---

### 🛠️ Propuestas de mejora

* **Ninguna.** 

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 

---

### ✍️ La opinión de Gemini sobre el Indicador

Es ruido matemático. Añade retraso a una señal que ya era de baja calidad.

**Propuestas de Acción:**
* **Descartar.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Demasiado lento.

**Acción:** **Descartar.**