---
cs_file: WeissWave.cs
name: Weis Wave
group: Order Flow
subgroup: Volume
score_current: 8/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Quanto volumen acumulado (esfuerzo) hay en la onda de precio actual?
gemini_summary: "El indicador estructural por excelencia. Acumula el volumen de velas consecutivas en la misma dirección, creando 'ondas' de presión. Esencial para el análisis Wyckoff."
comparison_group: "Volume Oscillators"
competitor_notes: "Único. No es un oscilador matemático, es un acumulador estructural."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Weis Wave (8/10)

**Nombre del archivo:** [`WeissWave.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/WeissWave.cs)  
**Nombre del indicador:** Weis Wave  
**Web oficial:** [ATAS — Weis Wave](https://help.atas.net/support/solutions/articles/72000602507)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuánto volumen acumulado (esfuerzo) hay en la onda de precio actual?

![WeissWave](../../img/WeissWave.png)

---

### ⚙️ Parámetros configurables

* **Filter:** Umbral para resaltar ondas gigantes.  
* **Colors:** Up/Down.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Oscillators"  

---

### 🧠 Uso más frecuente

* **Ley de Esfuerzo vs Resultado:** Onda de volumen masiva (Esfuerzo) con poco avance de precio (Resultado) = Absorción/Giro.  
* **Secado (No Supply):** Retroceso bajista con onda de volumen minúscula = Falta de vendedores.  

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Visión Macro:** Ayuda a ver el mercado en movimientos, no en velas sueltas.  
⛔ **Sensible:** Esta versión cambia de onda con una sola vela de color contrario. Sería mejor si usara un ZigZag de precio.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Wave Failure:** Precio hace nuevo mínimo, pero la onda de volumen bajista es mucho menor que la anterior.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Filter:** `5000` (Para destacar clímax).  

---

### 🧪 Notas de desarrollo

* Lógica simple: `Sign(Close - Open)`.  
* Acumula volumen mientras el signo se mantenga.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ruido:** Una vela Doji o de color contrario rompe la onda.  

---

### 🛠️ Propuestas de mejora

* **P3 (ZigZag):** Integrar lógica de ZigZag (cambio mínimo de ticks) para definir las ondas de forma más robusta.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es fundamental para entender la estructura. No te da la entrada exacta, te da el contexto del ciclo.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para ver divergencias estructurales.

**Acción:** **Conservar (Reserva).**
