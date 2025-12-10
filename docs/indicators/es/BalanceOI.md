---
# 1. IDENTIFICACIÓN
cs_file: BalanceOI.cs
name: On Balance Open Interest
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Open Interest
comparison_group: "Open Interest Analysis"

# 3. VALORACIÓN (Score & Priority)
score_current: 1/10
score_potential: 8/10
file_state: Estable
effort: Bajo
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Conservar (Reserva)

# 5. ANÁLISIS
description: ¿Está el compromiso acumulado del 'dinero inteligente' subiendo o bajando en relación al precio?
gemini_summary: "Oscilador tipo OBV aplicado al OI. Totalmente dependiente de la variación intradía del OI. En S&P 500 con Rithmic es inservible."
competitor_notes: "Alternativa de momentum al OI Analyzer."
reusable_code: "Lógica de Suma Móvil (Rolling Sum)."

# 6. METADATOS
analysis_date: 2025-11-21
official_code_date: 2025-04-23
---

## 🛡️ On Balance Open Interest (1/10)

**Nombre del archivo:** [`BalanceOI.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BalanceOI.cs)  
**Nombre del indicador:** On Balance Open Interest  
**Web oficial:** [ATAS — On Balance Open Interest](https://help.atas.net/support/solutions/articles/72000602438)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Está el compromiso acumulado del 'dinero inteligente' subiendo o bajando en relación al precio?

![BalanceOI](../../img/BalanceOI.png)

---

### ⚙️ Parámetros configurables

* **Minimized Mode:** Activa modo oscilador.
* **Value:** Ventana de la suma móvil.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Open Interest  
**Comparison Group:** "Open Interest Analysis"  

---

### 🧠 Uso más frecuente

* **Divergencias:** Solo útil en mercados con OI dinámico (Crypto).

---

### 📊 Nivel de relevancia
🔟 **1 / 10**

⛔ **Datos:** No funciona en CME/Rithmic.

---

### 🎯 Estrategias de scalping donde se aplica

* Ninguna en este contexto.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar.**

---

### 🧪 Notas de desarrollo

* Implementa `Rolling Sum`.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Falta línea cero por defecto.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica Rolling Sum:** Útil snippet para otros osciladores.

---

### ✍️ La opinión de Gemini sobre el Indicador

Si no hay cambios en el OI, la matemática de este indicador produce basura o nada. Al cajón.

**Propuestas de Acción:**
* **Archivar.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

**Acción:** **Conservar (Reserva)**