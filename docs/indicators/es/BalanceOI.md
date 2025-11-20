---
cs_file: BalanceOI.cs
name: On Balance Open Interest
category: Order Flow
group: Order Flow
subgroup: Open Interest
score_current: 7/10
version: Estable
recommended_action: Conservar (Reserva)
description: ¿Está el compromiso acumulado del 'dinero inteligente' subiendo o bajando en relación al precio?
gemini_summary: "Interesante oscilador de momentum para el OI. Su 'Modo Minimizado' actúa como una media móvil del flujo de dinero, útil para detectar divergencias rápidas, pero es una herramienta secundaria frente al Analyzer."
comparison_group: "Open Interest Analysis"
competitor_notes: "Complementa al OI Analyzer ofreciendo una visión de oscilador rápido (momentum), pero no da el detalle direccional."
reusable_code: "Lógica de Suma Móvil (Rolling Sum) para crear osciladores a partir de datos acumulados."
file_state: Estable
score_potential: 8/10
effort: Bajo
action_priority: Bajo
analysis_date: 2025-11-20
official_code_date: 2025-04-23
---

## 🟦 On Balance Open Interest (7/10)

**Nombre del archivo:** [`BalanceOI.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BalanceOI.cs)  
**Nombre del indicador:** On Balance Open Interest  
**Web oficial:** [ATAS — On Balance Open Interest](https://help.atas.net/support/solutions/articles/72000602438)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Está el compromiso acumulado del 'dinero inteligente' subiendo o bajando en relación al precio?

![BalanceOI](../../img/BalanceOI.png)

---

### ⚙️ Parámetros configurables

* **Minimized Mode**:
    * `Enabled`: Activa el modo oscilador (suma móvil).
    * `Value`: Periodo de la suma móvil (N barras). Por defecto `10`.

---

### 🧭 Clasificación
**Grupo:** Order Flow
**Subgrupo:** Open Interest

---

### 🧠 Uso más frecuente

* **Divergencias Rápidas:** Precio hace nuevo máximo, pero el BalanceOI (momentum de dinero) hace un máximo menor.
* **Confirmación de Tendencia:** El oscilador se mantiene consistentemente positivo (flujo de entrada) o negativo (flujo de salida).

---

### 📊 Nivel de relevancia
7️⃣ **7 / 10 (TÁCTICO)**

✅ **Concepto Inteligente:** Aplica la lógica del OBV (On Balance Volume) al Open Interest.  
✅ **Modo Minimizado:** Convierte datos acumulados en un oscilador reactivo, ideal para ver "tirones" de liquidez.  
⛔ **Inferior al Analyzer:** No distingue si el movimiento es por compras o ventas, solo sigue la dirección del cierre de la vela, lo cual puede ser engañoso (falsos positivos).

---

### 🎯 Estrategias de scalping donde se aplica

* **Entrada en Retroceso:** En tendencia alcista, esperar a que el BalanceOI (modo minimizado) caiga cerca de cero y gire hacia arriba.
* **Divergencia de Flujo:** Si el precio rompe un nivel pero el BalanceOI no acompaña, sospechar de una falsa ruptura.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **MinimizedMode**: `True` (Activado).
* **Value**: `10` (Estándar) o `5` (Muy rápido).

---

### 🧪 Notas de desarrollo

* **Modo Normal:** Acumula `+OI` si el cierre de la vela es alcista y `-OI` si es bajista (Lógica OBV clásica).
* **Modo Minimizado:** Calcula la suma neta de ese valor "signado" en las últimas N barras. Es matemáticamente un *Momentum de Flujo de OI*.

---

### ❗ Incoherencias o aspectos mejorables detectados
* **Falta de Referencia Visual:** En el "Modo Minimizado", el indicador oscila alrededor de cero, pero **no dibuja una línea de cero** por defecto (`ShowZeroValue = false`). Esto hace difícil ver a simple vista si el momentum es positivo o negativo sin añadir una línea manual.
* **Naming Confuso:** El nombre del parámetro `MinimizedMode` es poco intuitivo. Debería llamarse `OscillatorMode` o `RollingSum`.

---

### 🛠️ Propuestas de mejora
* **Mejora Visual (P1):** Establecer `ShowZeroValue = true` por defecto cuando se activa el modo minimizado. Es un cambio de una línea de código que mejora la usabilidad un 100%.

---

### 💎 Valor Reutilizable (Código Donante)
* **Lógica de "Rolling Sum":** El código implementa una forma eficiente de convertir cualquier dato acumulativo en un oscilador de momentum (`_renderSeries[bar] + ... - _oiSignedSeries[bar - Period]`). Esta lógica es exportable para crear osciladores de cualquier dato de Order Flow.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta táctica interesante. No te da la "verdad profunda" del `OIAnalyzer` (el *porqué*), pero te da una señal visual muy rápida del *qué* (el impulso).

Se queda con un 7/10 porque en scalping moderno preferimos la precisión del desglose direccional, pero como oscilador rápido para tener en una esquina es muy válido.

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, como apoyo.**

Útil para confirmar la fuerza inmediata del movimiento sin saturar la vista con números.

**Acción:** **Conservar (Reserva / Táctico).**