---
cs_file: Exhaustion.cs
name: Exhaustion
group: Order Flow
subgroup: Footprint
score_current: 8/10
version: Stable
recommended_action: Reparar
description: ¿Está el precio mostrando agotamiento en los últimos N ticks de la vela?
gemini_summary: "Concepto brillante (detectar agotamiento de volumen en extremos), pero ejecución deficiente. Tiene un error de diseño ('todo o nada') que oculta información valiosa y un bug tipográfico en los parámetros."
comparison_group: "Microstructure"
competitor_notes: "Potencialmente complementario al Ratio, pero actualmente inferior por usabilidad."
reusable_code: null
file_state: Buggy
score_potential: 9/10
effort: Medio
action_priority: P2
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛠️ Exhaustion (8/10)

**Nombre del archivo:** [`Exhaustion.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Exhaustion.cs)  
**Nombre del indicador:** Exhaustion  
**Web oficial:** [ATAS — Exhaustion](https://help.atas.net/support/solutions/articles/72000641184-exhaustion)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Está el precio mostrando agotamiento en los últimos N ticks de la vela?

![Exhaustion](../../img/Exhaustion.png)

---

### ⚙️ Parámetros configurables

* **AmountOfPrices:** (Mal escrito como `AmoutOfPrices`) Número de niveles a verificar.
* **CalcMode:** Fuente (`Bid`, `Ask`, `Volume`).
* **Visuals:** Colores y formas de la señal.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Microstructure"  

---

### 🧠 Uso más frecuente

* **Detección de Secado:** Ver cómo el volumen se reduce drásticamente a medida que el precio alcanza un extremo (agotamiento de la subasta).  

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Concepto Visual:** Dibuja una caja alrededor de los ticks de agotamiento, lo cual es muy visual.  
⛔ **Bug Lógico:** Si pides 5 niveles y hay 4 de agotamiento, no dibuja nada. Debería dibujar los 4.  
⛔ **Typo:** `AmoutOfPrices`.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Reversión en V:** Precio estira, se agota (caja de exhaustion) y gira.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Amount:** `3` (Menos restrictivo para que funcione).  

---

### 🧪 Notas de desarrollo

* Recorre el clúster desde el extremo hacia dentro.  
* Compara `SourceValue > PrevSourceValue`.  
* Condición fatal: `if (pvInfos.Count != _amoutOfPrices) return;`.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Error de Diseño:** La condición de igualdad estricta mata la utilidad del indicador.  
* **Ortografía:** `Amout` -> `Amount`.  

---

### 🛠️ Propuestas de mejora

* **P2 (Reparar):** Cambiar la lógica para que dibuje *hasta* N niveles, o al menos los que encuentre, en lugar de exigir el número exacto. Corregir el nombre del parámetro.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es un indicador frustrante. Podría ser genial, pero es demasiado rígido. Necesita una mano de pintura en el código.

**Propuestas de Acción:**
* **Reparar y mover a Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**A veces.**

Cuando funciona es bueno, pero falla demasiado por su rigidez.

**Acción:** **Reparar.**