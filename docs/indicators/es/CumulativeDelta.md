---
# 1. IDENTIFICACIÓN
cs_file: CumulativeDelta.cs
name: CVD - Cumulative Volume Delta
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Delta
comparison_group: "Cumulative Delta"

# 3. VALORACIÓN (Score & Priority)
score_current: 8/10
score_potential: 9/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: P2

# 4. DECISIÓN
recommended_action: Conservar (Reserva)

# 5. ANÁLISIS
description: ¿Cuál es el delta acumulado desde el inicio de la sesión (o desde una hora personalizada)?
gemini_summary: "El estándar fiable. Aunque es tácticamente inferior al ganador (MMP) por no tener filtros de volumen, posee una característica crítica que le falta al ganador: la gestión de 'CustomSession' (reinicio horario)."
competitor_notes: "Inferior a MultiMarketPower en análisis, pero superior en gestión de sesión (RTH vs ETH)."
reusable_code: "El método 'CheckStartBar' y la lógica de 'CustomSessionStart' son vitales para portar a otros indicadores acumulativos."

# 6. METADATOS
analysis_date: 2025-11-21
official_code_date: 2025-11-13
---

## 🛡️ CVD - Cumulative Volume Delta (8/10)

**Nombre del archivo:** [`CumulativeDelta.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CumulativeDelta.cs)  
**Nombre del indicador:** CVD - Cumulative Volume Delta  
**Web oficial:** [ATAS — Cumulative Volume Delta](https://help.atas.net/support/solutions/articles/72000602360)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 2025-11-13  

> **La Pregunta Clave:** ¿Cuál es el delta acumulado desde el inicio de la sesión (o desde una hora personalizada)?

![CumulativeDelta](../../img/CumulativeDelta.png)

---

### ⚙️ Parámetros configurables

* **Mode:** Visualización (`Candles`, `Bars`, `Line`). Para scalping, `Line` es lo más limpio.
* **SessionCumDeltaMode:** `DefaultSession` (todo el día) o `CustomSession` (horario específico).
* **CustomSessionStart:** Hora de reinicio (ej. 15:30:00). Característica estrella.
* **UseAlerts:** Alertas sonoras si el delta cambia bruscamente.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Cumulative Delta"  

---

### 🧠 Uso más frecuente

* **Contexto RTH:** Reiniciar el delta a las 15:30 para ver quién está ganando *hoy* en la sesión americana, ignorando la sesión nocturna (Overnight).
* **Divergencias Simples:** Precio haciendo nuevos máximos, CVD fallando (sin entrar en detalles de filtros).

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Gestión de Sesión:** Único indicador del grupo con reinicio horario nativo.  
✅ **Ligereza:** Carga computacional mínima.  
⛔ **Ceguera:** Trata igual una orden de 1 contrato que una de 1000.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Apertura Americana:** Evaluar el sesgo direccional puro desde las 15:30.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Visual:** `Line`.
* **Session Mode:** `CustomSession` -> `09:30` (Hora del Exchange) o `15:30` (Hora Local).

---

### 🧪 Notas de desarrollo

* Código estable y bien mantenido por ATAS.
* La lógica de `CheckStartBar` es robusta y maneja correctamente los cambios de día.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguna. Es un indicador base sólido.

---

### 🛠️ Propuestas de mejora

* Ninguna. Su rol es ser el backup fiable.

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica `CustomSession`:** Debe ser extraída e injertada en `MultiMarketPower` para crear el "Super Indicador" definitivo.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el "Toyota Corolla" de los indicadores. No tiene lujos, pero arranca siempre. Lo mantenemos en la recámara por su capacidad de reiniciar la sesión, algo vital para separar el análisis del *Overnight* del *RTH*.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Específicamente para medir el sesgo de la sesión actual sin contaminación previa.

**Acción:** **Conservar (Reserva)**