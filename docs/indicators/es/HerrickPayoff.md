---
# 1. IDENTIFICACIÓN
cs_file: HerrickPayoff.cs
name: Herrick Payoff Index (HPI)
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Open Interest
comparison_group: "Open Interest Analysis"

# 3. VALORACIÓN (Score & Priority)
score_current: 0/10
score_potential: 2/10
file_state: Defectuoso
effort: N/A
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Descartar

# 5. ANÁLISIS
description: ¿Cuál es la fuerza del movimiento combinando Precio, Volumen y OI?
gemini_summary: "Indicador inservible. Matemáticamente incorrecto en su implementación de suavizado y dependiente de un dato (OI intradía) que no existe en el feed de S&P 500."
competitor_notes: "Inferior a todo."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-11-21
official_code_date: 2025-04-23
---

## 💀 Herrick Payoff Index (HPI) (0/10)

**Nombre del archivo:** [`HerrickPayoff.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/HerrickPayoff.cs)  
**Nombre del indicador:** Herrick Payoff Index  
**Web oficial:** [ATAS — Herrick Payoff Index](https://help.atas.net/support/solutions/articles/72000602286)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es la fuerza del movimiento combinando Precio, Volumen y OI? (Implementación Rota)

![Herrick Payoff Index](../../img/HerrickPayoff.png)

---

### ⚙️ Parámetros configurables

* Divisor, Smooth.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Open Interest  
**Comparison Group:** "Open Interest Analysis"  

---

### 🧠 Uso más frecuente

* Ninguno confiable.

---

### 📊 Nivel de relevancia
🔟 **0 / 10**

⛔ **Doble Fallo:** Código roto + Falta de datos.

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.**

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Eliminar.**

---

### 🧪 Notas de desarrollo

* Error en la fórmula de suavizado acumulativo.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Todo el cálculo es sospechoso.

---

### 🛠️ Propuestas de mejora

* Borrar.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

No gastes ni un segundo más en esto.

**Propuestas de Acción:**
* **Eliminar del proyecto.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

**Acción:** **Descartar**