---
cs_file: TVI.cs
name: Trade Volume Index
group: Order Flow
subgroup: Volume
score_current: 7/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Se está acumulando o distribuyendo el volumen basándose en la dirección del tick?
gemini_summary: "Un acumulador tick a tick. Es una versión de 'alta definición' del OBV. Útil para ver presión de fondo, pero sufre de deriva (drift) al no estar normalizado."
comparison_group: "Volume Oscillators"
competitor_notes: "Similar a OBV pero intra-vela."
reusable_code: null
file_state: Estable
score_potential: 7/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Trade Volume Index (7/10)

**Nombre del archivo:** [`TVI.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/TVI.cs)  
**Nombre del indicador:** Trade Volume Index  
**Web oficial:** [ATAS — Trade Volume Index](https://help.atas.net/support/solutions/articles/72000602296)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Se está acumulando o distribuyendo el volumen basándose en la dirección del tick?

![TVI](../../img/TVI.png)

---

### ⚙️ Parámetros configurables

* **N/A:** Sin parámetros.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Oscillators"  

---

### 🧠 Uso más frecuente

* **Divergencia de Flujo:** Precio sube, TVI no acompaña -> Falta de presión compradora real.  
* **Confirmación:** TVI rompiendo máximos antes que el precio.  

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Precisión:** Mira dentro de la vela (tick a tick), no solo el cierre (OBV).  
⛔ **Deriva:** No tiene límites. Difícil de leer en rangos largos.  
⛔ **Reset:** Falta opción de reinicio de sesión.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Wyckoff Spring:** Precio rompe soporte, TVI se mantiene plano -> Absorción.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **N/A.** ---

### 🧪 Notas de desarrollo

* Lógica de acumulador simple.  
* Asume que si `Price > Prev + TickSize` es compra.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** ---

### 🛠️ Propuestas de mejora

* **P3:** Añadir reinicio de sesión diario.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta de fondo. Útil para tener una segunda opinión sobre la tendencia general.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Limitado.**

Mejor usar Cumulative Delta.

**Acción:** **Conservar (Reserva).**