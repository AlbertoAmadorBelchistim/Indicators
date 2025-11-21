---
cs_file: RoundNr.cs
name: Round Numbers
category: Structure
group: Structure
subgroup: Static Levels
score_current: 7/10
version: Stable
recommended_action: Conservar
description: ¿Dónde están los niveles de precio psicológicos (números redondos)?
gemini_summary: "Visualizador eficiente de niveles psicológicos. Código estable y limpio."
comparison_group: "Grid Levels"
competitor_notes: "Sin competencia."
reusable_code: null
file_state: Estable
score_potential: 8/10
effort: Bajo
action_priority: N/A
analysis_date: 2025-11-18
official_code_date: 23/04/2025
---

## 🟦 Round Numbers (7/10)

**Nombre del archivo:** [`RoundNr.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/RoundNr.cs)  
**Nombre del indicador:** Round Numbers  
**Web oficial:** [ATAS — Round Numbers](https://help.atas.net/support/solutions/articles/72000602459)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Dónde están los niveles de precio psicológicos (números redondos) en el gráfico?

![RoundNr](../../img/RoundNr.png)

---

### ⚙️ Parámetros configurables

* **Step**: Distancia en ticks entre líneas (ej. 100 ticks).
* **Pen**: Configuración de la línea (Color, grosor).

---

### 🧭 Clasificación
📂 Level — Indicador de niveles estáticos horizontales basados en precio.

---

### 🧠 Uso más frecuente

* **Soportes/Resistencias Psicológicos:** Los humanos y algoritmos tienden a poner órdenes en números terminados en 00 o 50.
* **Grid Visual:** Ayuda a medir distancias visualmente sin usar la herramienta de regla.

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Muy ligero (renderizado puro, sin cálculos históricos).  
✅ Útil para limpiar el gráfico de grids predeterminados y usar uno personalizado.  
⛔ **Opciones limitadas:** No permite desactivar las etiquetas de texto independientemente de las líneas.  
⛔ **Cálculo de Módulo:** `lowLines % 1 == 0` es técnicamente arriesgado con decimales, aunque suele funcionar.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Magnet Trading:** El precio suele ser atraído a los números redondos. Scalping hacia el nivel 00.
* **Rebote en Nivel:** Esperar absorción en el Order Flow justo en el número redondo.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Step**: `40` (En ES, 1 punto = 4 ticks. 40 ticks = 10 puntos). Niveles cada 10 puntos son clave en el S&P 500.
* **Color**: Gris muy claro, para que sea fondo y no señal.

---

### 🧪 Notas de desarrollo

* **Render-Only:** Sobrescribe `OnRender` y deja `OnCalculate` vacío. Este es el patrón de diseño correcto para indicadores puramente visuales.
* **Lógica de Dibujo:** Calcula dinámicamente qué líneas son visibles en la ventana (`ChartInfo.Region`) para no dibujar líneas fuera de pantalla. Eficiente.
* **Detección de Espacio:** Verifica si hay espacio vertical (`isFreeSpace`) antes de dibujar el texto para evitar solapamientos. Buen detalle.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Cumple su función con eficiencia. Es el tipo de indicador "invisible" que mejora la calidad de vida del trader sin molestar. La lógica de renderizado es inteligente al dibujar solo lo visible.

**Propuestas de Mejora:**
* **Toggle de Texto:** Añadir `bool ShowText` para permitir líneas limpias sin números.
* **Estilos de Línea:** Permitir definir diferentes estilos para niveles "Mayores" (ej. cada 1000 ticks) y "Menores" (ej. cada 100 ticks).

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

El scalping es precisión. Saber dónde está el "doble cero" (00) de un vistazo ayuda a colocar Take Profits racionales.

**Acción:** **Mejorar (Añadir opciones de visibilidad de texto).**

