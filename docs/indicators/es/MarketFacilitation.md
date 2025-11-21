---
cs_file: MarketFacilitation.cs
name: Market Facilitation Index
group: Order Flow
subgroup: Volume
score_current: 6/10
version: Stable
recommended_action: Mejorar
description: ¿Cuál es la eficiencia del mercado (MFI) para mover el precio en relación con el volumen?
gemini_summary: "Implementación incompleta de la teoría de Bill Williams. Calcula el índice pero no clasifica las barras (Green, Fade, Fake, Squat), que es la parte vital de la estrategia MFI."
comparison_group: "Volume Efficiency"
competitor_notes: "Inferior al VBRR en su estado actual por falta de clasificación visual."
reusable_code: null
file_state: Mejorable (Funcionalidad Faltante)
score_potential: 8/10
effort: Medio
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🔧 Market Facilitation Index (6/10)

**Nombre del archivo:** [`MarketFacilitation.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MarketFacilitation.cs)  
**Nombre del indicador:** Market Facilitation Index  
**Web oficial:** [ATAS — Market Facilitation Index](https://help.atas.net/support/solutions/articles/72000602423)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la eficiencia del mercado (MFI) para mover el precio en relación con el volumen?

![MarketFacilitation](../../img/MarketFacilitation.png)

---

### ⚙️ Parámetros configurables

* **Multiplier:** Factor de escala.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Efficiency"  

---

### 🧠 Uso más frecuente

* **Teoría del Caos (Bill Williams):** Clasificar velas en 4 estados según MFI y Volumen suben o bajan.  

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

⛔ **Incompleto:** Solo escupe el número bruto. No te dice si es una vela "Green" (MFI+, Vol+) o "Squat" (MFI-, Vol+). Sin los colores, pierde el 90% de su valor.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Squat Bar:** Vela de rango pequeño y volumen alto (MFI baja). Señal de giro inminente.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Inútil sin colores.** ---

### 🧪 Notas de desarrollo

* Fórmula: `(High - Low) * Multiplier / Volume`.  
* Falta lógica de comparación con `bar-1`.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta Funcionalidad Core:** No implementa la lógica de clasificación de Bill Williams.  

---

### 🛠️ Propuestas de mejora

* **P3:** Añadir lógica de comparación `Current vs Prev` para colorear el histograma en 4 colores (Verde, Marrón, Azul, Rosa) según la teoría MFI.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es un coche sin ruedas. El motor (cálculo) funciona, pero no te lleva a ningún lado sin la visualización correcta.

**Propuestas de Acción:**
* **Mejorar (Baja Prioridad).**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No en su estado actual.**

Requiere trabajo mental extra innecesario.

**Acción:** **Mejorar (Incompleto).**
