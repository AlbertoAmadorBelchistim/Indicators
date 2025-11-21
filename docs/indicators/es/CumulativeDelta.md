---
cs_file: CumulativeDelta.cs
name: CVD - Cumulative Volume Delta
group: Order Flow
subgroup: Delta
score_current: 8/10
version: Estable
recommended_action: Conservar (Reserva)
description: ¿Cuál es el delta acumulado desde el inicio de la sesión?
gemini_summary: "El estándar fiable. Aunque es tácticamente inferior al ganador (MMP) por no tener filtros, posee una característica crítica que le falta al ganador: la gestión de 'CustomSession' (reinicio horario)."
comparison_group: "Cumulative Delta"
competitor_notes: "Reserva de MultiMarketPower. Se mantiene por su lógica de sesión personalizada."
reusable_code: "Lógica CustomSession (CheckStartBar)"
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 13/11/2025
---

## 🛡️ CVD - Cumulative Volume Delta (8/10)

**Nombre del archivo:** [`CumulativeDelta.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CumulativeDelta.cs)  
**Nombre del indicador:** CVD - Cumulative Volume Delta  
**Web oficial:** [ATAS — CVD - Cumulative Volume Delta](https://help.atas.net/support/solutions/articles/72000602360-cumulative-volume-delta)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 13/11/2025  

> **La Pregunta Clave:** ¿Cuál es el delta acumulado (la agresión neta) desde el inicio de la sesión?

![Cumulative Volume Delta](../../img/CumulativeDelta.png)

---

### ⚙️ Parámetros configurables

* **Mode:** Tipo de visualización (`Candles`, `Bars`, `Line`).  
* **SessionCumDeltaMode:** Lógica de reinicio (`DefaultSession` o `CustomSession`).  
* **CustomSessionStart:** Hora de reinicio personalizada (Vital para RTH).  
* **Alerts:** Alertas por cambio de delta.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Cumulative Delta"  

---

### 🧠 Uso más frecuente

* **Contexto General:** Tendencia de fondo del flujo de órdenes.  
* **Segregación RTH:** Ver el delta acumulado solo desde la apertura americana (usando `CustomSession`).  

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Fiabilidad:** Código base oficial y estable.  
✅ **Gestión de Sesión:** Único indicador del grupo con reinicio horario personalizado.  
⛔ **Simple:** No permite filtrar por tamaño de orden ("Peces Gordos" vs "Peces Chicos").  

---

### 🎯 Estrategias de scalping donde se aplica

* **Divergencia General:** Precio HH vs CVD LH.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Mode:** `Line` (Limpieza).  
* **SessionCumDeltaMode:** `CustomSession`.  
* **CustomSessionStart:** `09:30` (Hora NY).  

---

### 🧪 Notas de desarrollo

* Acumula `candle.Delta`.  
* Función clave: `CheckStartBar(bar)`. Gestiona el reinicio basándose en la hora del instrumento.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Cumple su función básica perfectamente.  

---

### 🛠️ Propuestas de mejora

* **Ninguna.** Su destino es donar su código de sesión al ganador.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica `CustomSession`:**
    * Código: Método `CheckStartBar` y propiedad `CustomSessionStart`.
    * Acción: **PORTAR A `MultiMarketPower` (Prioridad Alta).**

---

### ✍️ La opinión de Gemini sobre el Indicador

Es la "Línea Base". Si `MultiMarketPower` falla, este es el respaldo. Pero su verdadero valor hoy es su código de gestión de sesión, que es la pieza que le falta al ganador para ser perfecto.

**Propuestas de Acción:**
* **Conservar como Reserva y Donante de Código.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Como respaldo y referencia de sesión RTH.

**Acción:** **Conservar (Reserva).**