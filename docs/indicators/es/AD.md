---
# 1. IDENTIFICACIÓN
cs_file: AD.cs
name: Accumulation/Distribution (A/D)
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: Classic Volume

# 3. VALORACIÓN (Score & Priority)
score_current: 2/10
score_potential: 2/10
file_state: Estable (con defecto visual)
effort: N/A
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Descartar

# 5. ANÁLISIS
description: ¿El flujo de volumen acumulado (estimado por la forma de la vela) confirma la tendencia?
gemini_summary: "Indicador obsoleto para el trading de Order Flow. Estima la agresividad basándose en la posición del cierre relativo al rango (High-Low), algo innecesario cuando ATAS proporciona el Delta real. Además, su visualización por defecto como histograma dificulta la lectura de la tendencia acumulada."
competitor_notes: "Inferior a Cumulative Delta en precisión y utilidad."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-12-10
official_code_date: 2025-04-23
---

## 💀 Accumulation/Distribution (A/D) (2/10)

**Nombre del archivo:** [`AD.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/AD.cs)  
**Nombre del indicador:** Accumulation/Distribution (A/D)  
**Web oficial:** [ATAS - Accumulation/Distribution (A/D)](https://help.atas.net/support/solutions/articles/72000606733)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿El flujo de volumen acumulado (estimado por la forma de la vela) confirma la tendencia?

![AD](../../img/AD.png)

---

### ⚙️ Parámetros configurables

* **N/A:** No tiene parámetros configurables, es una fórmula matemática fija.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume 
**Comparison Group:** "Classic Volume"  

---

### 🧠 Uso más frecuente

* **Análisis Técnico Clásico:** Búsqueda de divergencias entre precio y flujo teórico en gráficos diarios (D1).

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

⛔ **Tecnología Superada:** Es una estimación matemática diseñada para cuando no existía el dato de Delta real.  
⛔ **Defecto Visual:** Se renderiza como Histograma (`VisualMode.Histogram`) cuando, al ser un dato acumulativo, debería ser una Línea para leerse correctamente.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Usar Cumulative Delta en su lugar.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No usar.**

---

### 🧪 Notas de desarrollo

* Fórmula clásica de Chaikin: `((Close - Low) - (High - Close)) / (High - Low) * Volume`.
* El código es robusto pero la elección de visualización es incorrecta para el tipo de dato.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Visualización:** Cambiar `VisualType` a `VisualMode.Line` haría el indicador legible, aunque seguiría siendo inútil frente al Delta.

---

### 🛠️ Propuestas de mejora

* Ninguna. El indicador cumple su función histórica, simplemente no es apto para trading moderno de flujo de órdenes.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una reliquia de los años 80. En un entorno profesional con datos de Nivel 2 (Market Depth) y Tick Data, estimar la compra/venta por la forma de la vela es ineficiente y propenso a errores (ej. velas de absorción donde el precio no se mueve pero hay mucho volumen).

**Propuestas de Acción:**
* **Descartar** del arsenal de Scalping.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

**Acción:** **Descartar**