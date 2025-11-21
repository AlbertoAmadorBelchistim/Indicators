---
cs_file: BalanceOI.cs
name: On Balance Open Interest
group: Order Flow
subgroup: Open Interest
score_current: 7/10
version: Estable
recommended_action: Conservar (Reserva)
description: ¿Está el compromiso acumulado del 'dinero inteligente' subiendo o bajando en relación al precio?
gemini_summary: "Interesante oscilador de momentum para el OI. Su 'Modo Minimizado' actúa como una media móvil del flujo de dinero, útil para detectar divergencias rápidas de forma visual."
comparison_group: "Open Interest Analysis"
competitor_notes: "Complementa al OI Analyzer ofreciendo una visión de oscilador rápido (momentum), pero carece del detalle direccional profundo."
reusable_code: "Lógica de Suma Móvil (Rolling Sum)"
file_state: Estable
score_potential: 8/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ On Balance Open Interest (7/10)

**Nombre del archivo:** [`BalanceOI.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BalanceOI.cs)  
**Nombre del indicador:** On Balance Open Interest  
**Web oficial:** [ATAS — On Balance Open Interest](https://help.atas.net/support/solutions/articles/72000602438)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Está el compromiso acumulado del 'dinero inteligente' subiendo o bajando en relación al precio?

![BalanceOI](../../img/BalanceOI.png)

---

### ⚙️ Parámetros configurables

* **Minimized Mode:**
    * `Enabled`: Activa el modo oscilador (Rolling Sum).
    * `Value`: Periodo de la ventana de suma (ej. 10 barras).

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Open Interest  
**Comparison Group:** "Open Interest Analysis"  

---

### 🧠 Uso más frecuente

* **Divergencias Rápidas:** Precio haciendo nuevo máximo vs BalanceOI haciendo máximo menor (Agotamiento de flujo).  
* **Confirmación de Impulso:** Ver "tirones" fuertes de entrada de dinero en el modo oscilador.  

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Agilidad:** Convierte datos acumulados pesados en un oscilador reactivo.  
✅ **Concepto OBV:** Aplica la lógica clásica de On Balance Volume al OI.  
⛔ **Inferior al Analyzer:** No distingue la dirección real de la agresión, solo usa el cierre de la vela (como el OBV tradicional), lo que puede dar falsos positivos.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Entrada en Retroceso:** En tendencia, esperar a que el oscilador caiga a cero y gire.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **MinimizedMode:** `True` (Activado).  
* **Value:** `10`.  

---

### 🧪 Notas de desarrollo

* Implementa una "Suma Móvil" eficiente: `Current = Current + New - Old`.  
* Defecto UI: No muestra línea cero por defecto en modo oscilador.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta Línea Cero:** Dificulta la lectura rápida del cruce de momentum.  

---

### 🛠️ Propuestas de mejora

* **UI (P3):** Activar `ShowZeroValue = true` por defecto.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica Rolling Sum:** Útil para convertir cualquier acumulativo en oscilador.  

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta táctica válida. No te da la verdad profunda del `OIAnalyzer`, pero te da una señal visual rápida del impulso inmediato.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Como oscilador de apoyo.

**Acción:** **Conservar (Reserva).**