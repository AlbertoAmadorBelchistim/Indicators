---
cs_file: VsaWsd.cs
name: VSA – WSD Histogram
group: Order Flow
subgroup: Volume
score_current: 8/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Cómo se distribuye la estructura de la vela (mechas vs cuerpo) y el volumen relativo?
gemini_summary: "Deconstruye la vela en histogramas de mechas y cuerpo. Es una herramienta analítica única para cuantificar la estructura de la vela ('Price Action Cuantitativo')."
comparison_group: "VSA & Anomalies"
competitor_notes: "Único. Complementa al Better Volume dando detalle de la forma de la vela."
reusable_code: null
file_state: Estable
score_potential: 8/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 08/05/2025
---

## 🛡️ VSA – WSD Histogram (8/10)

**Nombre del archivo:** [`VsaWsd.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VsaWsd.cs)  
**Nombre del indicador:** VSA – WSD Histogram  
**Web oficial:** [ATAS — VSA – WSD Histogram](https://help.atas.net/support/solutions/articles/72000602501)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 08/05/2025  

> **La Pregunta Clave:** ¿Cómo se distribuye la estructura de la vela (mechas vs cuerpo) y el volumen relativo?

![VsaWsd](../../img/VsaWsd.png)

---

### ⚙️ Parámetros configurables

* **Period:** Media móvil de volumen.  
* **Visuals:** Colores para mechas (Upper/Lower Wick) y cuerpo (HighLow).  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "VSA & Anomalies"  

---

### 🧠 Uso más frecuente

* **Análisis de Rechazo:** Cuantificar visualmente el tamaño de la mecha de rechazo en un histograma.  
* **Contracción (WSD):** Detectar velas de rango estrecho (puntos de señal) que indican "No Supply" o "No Demand".  

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Desglose:** Separa visualmente la "intención" (cuerpo) del "rechazo" (mecha).  
✅ **Señales:** Marca puntos cuando hay contracción de rango (`HighLow < Prev`), clave en VSA.  
⛔ **Nombre:** "WSD" (Weakness/Strength Detection) es poco intuitivo.  

---

### 🎯 Estrategias de scalping donde se aplica

* **No Demand:** Vela alcista, rango estrecho (punto generado), volumen bajo + mecha superior grande -> Señal de venta.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Period:** `100` (Media estable de fondo).  

---

### 🧪 Notas de desarrollo

* Calcula el tamaño de mechas y cuerpo en ticks.  
* Genera señales (`_dotsBuy`, `_dotsSell`) basadas en la contracción del rango.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** ---

### 🛠️ Propuestas de mejora

* **Ninguna.** ---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta para el purista del Price Action. Si te importa el tamaño exacto de la mecha, este indicador te lo pone en un gráfico fácil de leer.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Ayuda a calificar la calidad de la vela.

**Acción:** **Conservar (Reserva).**