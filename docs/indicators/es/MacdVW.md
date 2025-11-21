---
cs_file: MacdVW.cs
name: MACD - Volume Weighted
group: Order Flow
subgroup: Volume
score_current: 8/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Cuál es la convergencia entre dos medias ponderadas por volumen (VWMAs)?
gemini_summary: "La evolución lógica del MACD. Al ponderar el precio por el volumen, filtra los movimientos con poca participación y da más peso a los movimientos institucionales. Si usas MACD, usa este."
comparison_group: "Volume Oscillators"
competitor_notes: "La versión 'Pro' del MACD estándar."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ MACD - Volume Weighted (8/10)

**Nombre del archivo:** [`MacdVW.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MacdVW.cs)  
**Nombre del indicador:** MACD - Volume Weighted  
**Web oficial:** [ATAS — MACD - Volume Weighted](https://help.atas.net/support/solutions/articles/72000602231)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la convergencia entre dos medias ponderadas por volumen (VWMAs)?

![MacdVW](../../img/MacdVW.png)

---

### ⚙️ Parámetros configurables

* **Period:** Signal EMA (Default 9).  
* **Short/Long Period:** Periodos de las VWMAs (Default 12/26).  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Oscillators"  

---

### 🧠 Uso más frecuente

* **Cruce de Línea Cero:** Cambio de tendencia confirmado por volumen.  
* **Divergencia MACD:** Señal de reversión más fiable que en el MACD estándar porque incluye el factor volumen.  

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Filtrado:** Ignora movimientos de precio manipulados con poco volumen.  
✅ **Robustez:** Código con protecciones contra división por cero.  
⛔ **Visualización:** El histograma es monocromático.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Trend Following:** Mantener posición mientras MACD-VW > 0.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Short:** `8`.  
* **Long:** `21`.  

---

### 🧪 Notas de desarrollo

* Calcula `Sum(Price * Volume) / Sum(Volume)` (VWMA).  
* Resta VWMA Corta - VWMA Larga.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Estética:** Le falta color dinámico al histograma.  

---

### 🛠️ Propuestas de mejora

* **P3:** Colorear histograma (Verde si sube, Rojo si baja).  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es una mejora técnica sólida. No reinventa la rueda, pero la hace rodar mejor.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Mejor que el MACD normal.

**Acción:** **Conservar (Reserva).**
