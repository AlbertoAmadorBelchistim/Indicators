---
# 1. IDENTIFICACIÓN
cs_file: ADF.cs
name: Accumulation / Distribution Flow
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: Classic Volume

# 3. VALORACIÓN (Score & Priority)
score_current: 1/10
score_potential: 1/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Descartar

# 5. ANÁLISIS
description: ¿Cuál es la tendencia suavizada del flujo de volumen acumulado?
gemini_summary: "Toma un concepto ya obsoleto (AD) y le añade retraso aplicando una Media Móvil Simple (SMA). Es el concepto de 'lag sobre estimación'. Totalmente inútil para la toma de decisiones reactivas en Scalping."
competitor_notes: "El indicador menos reactivo de todo el grupo de Volumen."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-12-10
official_code_date: 2025-04-23
---

## 💀 Accumulation / Distribution Flow (1/10)

**Nombre del archivo:** [`ADF.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ADF.cs)  
**Nombre del indicador:** Accumulation / Distribution Flow  
**Web oficial:** [ATAS — Accumulation / Distribution Flow](https://help.atas.net/support/solutions/articles/72000602569)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es la tendencia suavizada del flujo de volumen acumulado?

![ADF](../../img/ADF.png)

---

### ⚙️ Parámetros configurables

* **Period:** Periodo de la SMA para suavizar el dato (Default: 14).
* **UsePrev:** * `True`: Usa el cierre anterior como referencia (estilo Price Volume Trend).
    * `False`: Usa la apertura actual como referencia (estilo Intraday AD).

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Classic Volume"  

---

### 🧠 Uso más frecuente

* **Tendencias de muy largo plazo:** Intentar ver hacia dónde fluye el dinero institucional eliminando todo el ruido de corto plazo.

---

### 📊 Nivel de relevancia
🔟 **1 / 10**

⛔ **Lag Extremo:** Combina una fórmula estimativa con un promedio móvil. La señal de giro llega demasiado tarde para operar.  
⛔ **Redundancia:** No aporta información que no esté ya visible en la acción del precio o en una media móvil normal.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.**

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar.**

---

### 🧪 Notas de desarrollo

* Calcula una variación del AD y luego aplica `_sma.Calculate`.
* Comparte el mismo problema de visualización que el AD (Histograma en lugar de Línea), lo que hace difícil ver la continuidad de la tendencia suavizada.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Concepto:** Suavizar un indicador de volumen acumulado suele ser contraproducente porque elimina los picos de volumen, que son precisamente la señal de intervención profesional.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

Si el AD es un mapa viejo, el ADF es mirar ese mapa viejo a través de un cristal empañado (SMA). Añade retraso a una métrica que ya era imprecisa.

**Propuestas de Acción:**
* **Descartar.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

**Acción:** **Descartar**