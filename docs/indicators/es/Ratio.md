---
cs_file: Ratio.cs
name: Ratio
group: Order Flow
subgroup: Footprint
score_current: 9/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Cuál es el ratio de absorción/agresión (Bid vs Ask) en el extremo de la vela?
gemini_summary: "Herramienta de precisión quirúrgica. Calcula matemáticamente la relación de fuerzas en la mecha de la vela. Un ratio bajo (<0.7) indica absorción/defensa; un ratio alto (>10) indica ruptura/iniciativa. Esencial para validar giros."
comparison_group: "Microstructure"
competitor_notes: "Más fiable y estándar que 'Exhaustion'."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 Ratio (9/10)

**Nombre del archivo:** [`Ratio.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Ratio.cs)  
**Nombre del indicador:** Ratio  
**Web oficial:** [ATAS — Ratio](https://help.atas.net/support/solutions/articles/72000602282)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es el ratio de absorción/agresión (Bid vs Ask) en el extremo de la vela?

![Ratio](../../img/Ratio.png)

---

### ⚙️ Parámetros configurables

Este indicador clasifica el extremo de la vela en tres estados:

#### 📊 Umbrales de Ratio
* **Low Ratio:** Valor por debajo del cual se considera "Absorción/Defensa" (Default: 0.71).
* **Neutral Ratio:** Zona gris (Default: 29.0).
* **High Ratio:** Valor por encima del cual se considera "Ruptura/Iniciativa".

#### 🎨 Visualización
* **Colors:** Colores distintos para Low, Neutral y High.
* **Font Size:** Tamaño de la etiqueta numérica.
* **Background:** Color de fondo de la etiqueta.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Microstructure"  

---

### 🧠 Uso más frecuente

* **Validación de Giros:** Si el precio llega a un soporte y aparece una etiqueta con ratio bajo (ej. 0.15) en color verde, significa que hubo una defensa masiva (Absorción). Señal de compra.  
* **Trampas de Ruptura:** Si el precio rompe un nivel pero cierra con mecha y ratio bajo, es una trampa (Fakeout).  
* **Confirmación de Ruptura:** Si el precio rompe y el ratio es alto (ej. 50.0) en color azul, indica que no hubo resistencia. Ruptura válida.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (IMPRESCINDIBLE)**

✅ **Lectura Objetiva:** Elimina la subjetividad de mirar el Footprint. Te da un número exacto.  
✅ **Micro-Estructura:** Analiza lo que pasa en el tick exacto del giro.  
✅ **Eficiente:** Solo pone una etiqueta por vela, no ensucia el gráfico.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ratio Defense:** Entrar a la contra en cuanto aparece un Ratio < 0.5 en un extremo de vela.  
* **Pullback Validation:** En tendencia alcista, esperar un retroceso. Si la vela de retroceso acaba con Ratio de absorción, entrar largo.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Low Ratio** | `0.70` | Menos de 1 indica absorción. 0.7 es un buen filtro. |
| **Neutral Ratio** | `10.0` | Filtrar valores intermedios. |
| **Days** | `2` | Solo interesa el presente. |

---

### 🧪 Notas de desarrollo

* **Lógica Alcista:** `LowBid2 / LowBid`. Compara el Bid del mínimo con el Bid del tick superior.
* **Lógica Bajista:** `HighAsk2 / HighAsk`. Compara el Ask del máximo con el Ask del tick inferior.
* Dibuja etiquetas usando `AddText` con IDs únicos por barra (`BAR_x`).

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Es la implementación estándar del Ratio de Order Flow.

---

### 🛠️ Propuestas de mejora

* **Ninguna.** ---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Ratio:** El método `CaclulateRatio` es exportable a estrategias automáticas.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una de esas herramientas pequeñas que marcan la diferencia. Saber si un mínimo fue "vendido con fuerza pero absorbido" (Ratio bajo) o "simplemente rechazado por falta de interés" es vital.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Te dice si el freno es real.

**Acción:** **Conservar (Core).**
