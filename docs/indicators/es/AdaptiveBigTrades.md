---
cs_file: AdaptativeBigTrades.cs
name: Adaptive Big Trades
category: Order Flow
group: Order Flow
subgroup: Footprint
score_current: 9/10
version: Stable
recommended_action: Conservar
description: ¿Dónde están las operaciones grandes relativas a la liquidez actual (sin configurar filtros fijos)?
gemini_summary: "Calcula dinámicamente el umbral de 'Big Trade' usando percentiles estadísticos. Excelente UX."
comparison_group: "Big Trades Analysis"
competitor_notes: "Compite directamente con Big Trades (Fijo). Su ventaja es la adaptabilidad."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: Bajo
action_priority: N/A
analysis_date: 2025-11-19
user_modification_date: 2025-11-19
---

## 🟦 Adaptive Big Trades (9/10)

**Nombre del indicador:** Adaptive Big Trades  
**Web oficial:** [ATAS — Adaptive Big Trades](https://help.atas.net/support/solutions/articles/72000606745)  
**Compatibilidad:** ATAS versión estable y superiores.

> **La Pregunta Clave:** ¿Dónde están las operaciones grandes relativas a la liquidez actual (sin configurar filtros fijos)?

![AdaptiveBigTrades](../../img/AdaptiveBigTrades.png)

---

### ⚙️ Parámetros configurables

* **Percentile Filter**: Porcentaje de corte (ej. `0.8` muestra el Top 20% de trades más grandes).
* **Object Size**: Tamaño base de las burbujas visuales.
* **Colors**: Color para compras y ventas agresivas.

---

### 🧭 Clasificación
📂 OrderFlow — Tape Reading automatizado y adaptativo.

---

### 🧠 Uso más frecuente

* **Mercados Cambiantes:** En la apertura 100 lotes es ruido, pero a mediodía 100 lotes es una ballena. Este indicador se ajusta solo.
* **Zonas de Interés:** Ver dónde "aprietan" los institucionales sin tener que recalibrar el indicador cada hora.

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Adaptabilidad:** Es su gran ventaja. Elimina el error humano de configurar un filtro demasiado alto (no ves nada) o bajo (ves ruido).  
✅ **Eficiencia:** Solicita el histórico una sola vez (`OnCumulativeTradesResponse`) y calcula el umbral estadístico.  
⛔ **Retardo Inicial:** Necesita cargar datos históricos para calcular el percentil correctamente.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Breakout con Iniciativa:** Si el precio rompe un nivel y aparece una burbuja grande del color de la ruptura, es confirmación de alta calidad.
* **Rechazo en V:** Burbuja grande en un mínimo que no logra bajar más el precio -> Absorción pasiva -> Entrada en largo.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Percentile**: `0.90` (Top 10% para ver solo lo relevante).
* **Size**: `30` px.

---

### 🧪 Notas de desarrollo

* **Estadística:** Usa un `SortedDictionary` para contar frecuencias de volumen y determinar el corte.
* **Visualización:** Dibuja círculos (`FillEllipse`) directamente sobre el gráfico. Incluye un *tooltip* personalizado al pasar el ratón (`DrawHoverTooltip`).

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una mejora inteligente sobre el indicador de "Big Trades" estándar. La lógica de percentiles es como deberían funcionar todos los filtros de volumen por defecto en las plataformas modernas.

**Propuestas de Mejora:**
* **Auto-Recálculo:** Opción para recalcular el umbral cada X minutos (ej. "Rolling Percentile") para adaptarse si la volatilidad cambia durante la sesión.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.** Te ahorra tiempo de configuración y te mantiene enfocado en lo importante.

**Acción:** **Conservar.**