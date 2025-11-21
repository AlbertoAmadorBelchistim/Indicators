---
cs_file: HerrickPayoff.cs
name: Herrick Payoff Index (HPI)
group: Order Flow
subgroup: Open Interest
score_current: 3/10
version: Estable
recommended_action: Descartar
description: ¿Cuál es la fuerza del movimiento combinando Precio, Volumen y OI?
gemini_summary: "Indicador roto. El concepto es bueno (fuerza combinada), pero la implementación matemática de la fórmula de 'suavizado' es errónea y produce datos no fiables."
comparison_group: "Open Interest Analysis"
competitor_notes: "Inferior e inestable comparado con cualquier otro analista de OI."
reusable_code: null
file_state: Roto
score_potential: 8/10
effort: Medio
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 💀 Herrick Payoff Index (HPI) (3/10)

**Nombre del archivo:** [`HerrickPayoff.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/HerrickPayoff.cs)  
**Nombre del indicador:** Herrick Payoff Index  
**Web oficial:** [ATAS — Herrick Payoff Index](https://help.atas.net/support/solutions/articles/72000602286)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la fuerza del movimiento combinando Precio, Volumen y OI? (Implementación Rota)

![Herrick Payoff Index](../../img/HerrickPayoff.png)

---

### ⚙️ Parámetros configurables

* **Divisor:** Escala.  
* **Smooth:** (Nombre incorrecto) Multiplicador de diferencias.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Open Interest  
**Comparison Group:** "Open Interest Analysis"  

---

### 🧠 Uso más frecuente

* **(Teórico):** Fuerza real del movimiento.  

---

### 📊 Nivel de relevancia
🔟 **3 / 10**

⛔ **ROTO:** La fórmula de suavizado no es una media móvil, es una acumulación de diferencias que genera ruido.  
⛔ **Inestable:** Manejo deficiente de `OI=0`.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Datos falsos.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar.** 

---

### 🧪 Notas de desarrollo

* Error crítico en fórmula: `_renderSeries[bar] = lastValue + _smooth * (diff)`. Esto no es suavizado.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Matemática Incorrecta.** 

---

### 🛠️ Propuestas de mejora

* **Ninguna.** No vale la pena reescribirlo.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 

---

### ✍️ La opinión de Gemini sobre el Indicador

Un intento fallido de implementar un clásico.

**Propuestas de Acción:**
* **Descartar.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

**Acción:** **Descartar.**
