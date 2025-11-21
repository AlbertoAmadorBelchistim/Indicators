---
cs_file: VBRR.cs
name: Volume Bar Range Ratio
group: Order Flow
subgroup: Volume
score_current: 7/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Cuánto volumen es necesario para mover el precio 1 tick (Eficiencia)?
gemini_summary: "Indicador de densidad. Muestra cuánto 'cuesta' mover el precio. Picos altos indican absorción masiva (esfuerzo sin resultado). Valles indican falta de liquidez."
comparison_group: "Volume Efficiency"
competitor_notes: "Inverso del Market Facilitation Index."
reusable_code: null
file_state: Estable
score_potential: 8/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Volume Bar Range Ratio (7/10)

**Nombre del archivo:** [`VBRR.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VBRR.cs)  
**Nombre del indicador:** Volume Bar Range Ratio  
**Web oficial:** [ATAS — VBRR](https://help.atas.net/support/solutions/articles/72000602499)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuánto volumen es necesario para mover el precio 1 tick (Eficiencia del movimiento)?

![VBRR](../../img/VBRR.png)

---

### ⚙️ Parámetros configurables

* **N/A:** Cálculo directo.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Efficiency"  

---

### 🧠 Uso más frecuente

* **Detección de Absorción:** VBRR muy alto = Mucho volumen en poco rango. Alguien está absorbiendo pasivamente (Iceberg).  
* **Detección de Vacío:** VBRR muy bajo = El precio se mueve con soplidos. Peligro de slippage o reversión rápida (V-Shape).  

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Concepto Sólido:** Cuantifica la "fricción" del mercado.  
⛔ **Ruido:** Sin suavizado, es difícil de leer en tiempo real.  
⛔ **Outliers:** En velas Doji (Rango=0), hereda el valor anterior, lo cual es un parche.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Parada en Seco:** Precio llega a nivel clave + VBRR explota. Señal de giro.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **N/A.** ---

### 🧪 Notas de desarrollo

* Fórmula: `Volume / (High - Low)`.
* Manejo de Doji: `if (High != Low) ... else prev`.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Visualización:** Un histograma tan volátil se beneficiaría de una línea media.

---

### 🛠️ Propuestas de mejora

* **P3:** Añadir opción de suavizado (SMA).

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es el detector de "Muros". Si el precio choca y el VBRR sube, hay un muro.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para detectar absorción.

**Acción:** **Conservar (Reserva).**