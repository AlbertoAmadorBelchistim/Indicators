---
cs_file: EMV.cs
name: Arms Ease of Movement
group: Order Flow
subgroup: Volume
score_current: 6.5/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Es el movimiento del precio eficiente en relación con su volumen y rango?
gemini_summary: "Un clásico del análisis técnico. Suaviza la relación volumen/precio para mostrar la tendencia de la 'facilidad' del movimiento. Útil para contexto, lento para scalping."
comparison_group: "Volume Efficiency"
competitor_notes: "Versión suavizada y más compleja del VBRR."
reusable_code: null
file_state: Estable
score_potential: 6.5/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Arms Ease of Movement (6.5/10)

**Nombre del archivo:** [`EMV.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/EMV.cs)  
**Nombre del indicador:** Arms Ease of Movement  
**Web oficial:** [ATAS — Arms Ease of Movement](https://help.atas.net/support/solutions/articles/72000602315)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Es el movimiento del precio eficiente en relación con su volumen y rango?

![EMV](../../img/EMV.png)

---

### ⚙️ Parámetros configurables

* **Period:** Suavizado (Default 9).  
* **MaType:** Tipo de media móvil.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Efficiency"  

---

### 🧠 Uso más frecuente

* **Confirmación de Tendencia:** Si el EMV sube, el precio sube con "facilidad" (poco volumen resistiendo).  
* **Divergencia:** Precio sube pero EMV baja -> El avance se está volviendo costoso (difícil).  

---

### 📊 Nivel de relevancia
🔟 **6.5 / 10**

✅ **Estabilidad:** Al estar suavizado, da menos señales falsas que el VBRR.  
⛔ **Lag:** La media móvil introduce retraso.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Swing Trading Intradía:** Más útil en M5/M15 que en M1.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Period:** `5` (Para reducir el lag).  

---

### 🧪 Notas de desarrollo

* Fórmula compleja que involucra el "MidPoint Move" y el "Box Ratio".  
* Código sólido.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** ---

### 🛠️ Propuestas de mejora

* **Ninguna.** ---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta de "vuelo por instrumentos". Te dice si el mercado planea suave o si el motor está sufriendo.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Limitado.**

Mejor para ver el fondo.

**Acción:** **Conservar (Reserva).**