---
cs_file: Ratio.cs
name: Ratio
category: Order Flow
group: Order Flow
subgroup: Footprint
score_current: 9/10
version: Stable
recommended_action: Conservar
description: ¿Cuál es el ratio de absorción/agresión (Bid vs Ask) en el extremo de la vela?
gemini_summary: "Indicador de microestructura excelente. Calcula el ratio de volumen en extremos. Código sólido."
comparison_group: "Microstructure"
competitor_notes: "Complementario a Imbalance Ratio."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-18
official_code_date: 23/04/2025
---

## 🟦 Ratio (9/10)

**Nombre del archivo:** [`Ratio.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Ratio.cs)  
**Nombre del indicador:** Ratio  
**Web oficial:** [ATAS — Ratio](https://help.atas.net/support/solutions/articles/72000602282)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es el ratio de absorción/agresión (Bid vs Ask) en el extremo de la vela?

![Ratio](../../img/Ratio.png)

---

### ⚙️ Parámetros configurables

* **Days**: Número de sesiones hacia atrás a analizar (por defecto: 20)
* **LowRatio / NeutralRatio**: Umbrales para colorear el ratio (Bajo, Neutro, Alto)
* **FontSize**: Tamaño del texto
* **Colores**: Configuración visual para cada nivel de ratio

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Ratio de presión bid/ask en zonas clave según dirección de la vela

---

### 🧠 Uso más frecuente

* Medir **presión en bid o ask** en el extremo de la vela
* Identificar zonas con **desequilibrio o absorción significativa**
* Visualizar **etiquetas numéricas** en el gráfico con ratio de agresión

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Indica con precisión desequilibrios relevantes al final de la vela  
✅ Útil para confirmar trampas o validaciones de nivel (Reversión en extremos)  
⛔ Solo muestra una etiqueta por vela

---

### 🎯 Estrategias de scalping donde se aplica

* **Confirmación de ruptura con presión** en el lado esperado
* **Trampa con absorción** si la vela cierra bajista pero hay fuerte ratio comprador en el mínimo
* **Soporte/resistencia reforzada** si aparece un ratio bajo o neutro tras testeo

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **LowRatio**: `0.70` (Señala absorción/agotamiento)
* **NeutralRatio**: `10.0` (o superior, para filtrar extremos)

---

### 🧪 Notas de desarrollo

* Para velas alcistas: Compara el Bid en el Low con el Bid en el tick superior.
* Para velas bajistas: Compara el Ask en el High con el Ask en el tick inferior.
* Dibuja una etiqueta de texto (`AddText`) encima/debajo de la vela con el valor.
* Utiliza `GetPriceVolumeInfo` para acceder a los datos internos del Footprint.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Este es un indicador de "micro-scalping" excelente. Automatiza la lectura del Footprint chart centrándose en lo más importante: ¿qué pasó en la mecha?

El código es correcto y eficiente. Calcula el ratio solo en el cierre de la barra (o en tiempo real si se desea), y gestiona las etiquetas de texto correctamente para no saturar el gráfico (usando IDs únicos por barra). Es una herramienta muy específica pero muy potente para validar giros.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, muy útil.**

Ayuda a confirmar si un "rechazo" (mecha) fue realmente una absorción o simplemente falta de interés.

**Acción:** **Conservar (Herramienta OFA de precisión).**
