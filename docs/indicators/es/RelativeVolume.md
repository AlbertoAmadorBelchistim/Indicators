---
# 1. IDENTIFICACIÓN
cs_file: RelativeVolume.cs
name: Relative Volume
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: "VSA & Anomalies"

# 3. VALORACIÓN (Score & Priority)
score_current: 5/10
score_potential: 6/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: P3

# 4. DECISIÓN
recommended_action: Conservar (Reserva)

# 5. ANÁLISIS
description: ¿Es el volumen actual anómalamente alto comparado con el promedio histórico para esta misma hora?
gemini_summary: "Indicador de contexto estacional intradía. Compara el volumen actual con el promedio de esa misma hora en los últimos N días. Útil para filtrar la volatilidad natural de la apertura/cierre en gráficos de tiempo, pero INÚTIL en gráficos no temporales (Ticks, Volumen, Renko) o para scalping de flujo puro."
competitor_notes: "Único en su enfoque temporal, pero limitado por el tipo de gráfico."
reusable_code: "Lógica de diccionario temporal."

# 6. METADATOS
analysis_date: 2025-12-11
official_code_date: 2025-04-23
---

## 🛡️ Relative Volume (5/10)

**Nombre del archivo:** [`RelativeVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/RelativeVolume.cs)  
**Nombre del indicador:** Relative Volume  
**Web oficial:** [ATAS — Relative Volume](https://help.atas.net/support/solutions/articles/72000602457)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Es el volumen actual anómalamente alto comparado con el promedio histórico para esta misma hora?

![RelativeVolume](../../img/RelativeVolume.png)

---

### ⚙️ Parámetros configurables

* **LookBack:** Días atrás para calcular el promedio de la "hora exacta".

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "VSA & Anomalies"  

---

### 🧠 Uso más frecuente

* **Swing Intradía:** Saber si el volumen de las 11:30 es relevante o es el silencio habitual.

---

### 📊 Nivel de relevancia
🔟 **5 / 10**

✅ **Contexto Horario:** Normaliza la curva de actividad diaria.  
⛔ **Limitación Crítica:** No funciona correctamente en gráficos que no sean de tiempo (Ticks, Range, Renko), ya que la lógica de "comparar horas" se rompe.  
⛔ **Lento:** No sirve para gatillo de entrada.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna directa.** Solo contexto.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar** si operas con gráficos de Ticks o Volumen constante.

---

### 🧪 Notas de desarrollo

* Código dependiente de `TimeOfDay`.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Debería desactivarse o avisar en gráficos no temporales.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es útil si operas M5 o M15 y quieres filtrar el ruido de la apertura. Para scalping puro, es prescindible.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Poco.**

**Acción:** **Conservar (Reserva)**
