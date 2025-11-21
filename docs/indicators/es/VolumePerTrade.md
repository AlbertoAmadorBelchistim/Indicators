---
cs_file: VolumePerTrade.cs
name: Volume Per Trade
group: Order Flow
subgroup: Volume
score_current: 9/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Cuál es el tamaño promedio de las órdenes ejecutadas en cada vela?
gemini_summary: "Métrica de calidad institucional. Al dividir el Volumen total por el número de Ticks (trades), revela si el mercado lo mueven muchos pececillos (Retail) o pocos tiburones (Institucional)."
comparison_group: "Volume Efficiency"
competitor_notes: "Único en su enfoque de 'Calidad de Orden' vs 'Cantidad de Volumen'."
reusable_code: null
file_state: Estable
score_potential: 9.5/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 Volume Per Trade (9/10)

**Nombre del archivo:** [`VolumePerTrade.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VolumePerTrade.cs)  
**Nombre del indicador:** Volume Per Trade  
**Web oficial:** [ATAS — Volume Per Trade](https://help.atas.net/support/solutions/articles/72000619357)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es el tamaño promedio de las órdenes ejecutadas en cada vela (Institucional vs Retail)?

![VolumePerTrade](../../img/VolumePerTrade.png)

---

### ⚙️ Parámetros configurables

* **N/A:** Este indicador es una métrica directa sin parámetros de ajuste.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Efficiency"  

---

### 🧠 Uso más frecuente

* **Detector de Ballenas:** Un VPT alto indica que están entrando órdenes de gran tamaño (bloques), característico de actividad institucional.  
* **Detector de HFT/Retail:** Un volumen muy alto con un VPT bajo (cercano a 1) indica una "lluvia" de órdenes pequeñas (algoritmos de alta frecuencia o pánico minorista).  

---

### 📊 Nivel de relevancia
🔟 **8 / 10 (IMPRESCINDIBLE)**

✅ **Visión de Rayos X:** Diferencia 1000 contratos hechos por 1 persona de 1000 contratos hechos por 1000 personas.  
✅ **Simplicidad:** No requiere configuración. Es un dato puro.  
⛔ **Sin Suavizado:** La serie puede ser muy volátil (ruidosa) en gráficos rápidos.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Institutional Breakout:** Si el precio rompe un nivel y el VPT se dispara, es una ruptura respaldada por "dinero inteligente".  
* **Fakeout:** Ruptura con volumen alto pero VPT bajo = Trampa de minoristas/HFT.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **N/A** (Sin parámetros).  

---

### 🧪 Notas de desarrollo

* Fórmula: `candle.Volume / candle.Ticks`.
* Código extremadamente eficiente y limpio.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **División por Cero:** Teóricamente, si `candle.Ticks` es 0, podría dar error, aunque en un mercado real es imposible tener Volumen > 0 con Ticks = 0.

---

### 🛠️ Propuestas de mejora

* **SMA (P2):** Añadir una media móvil simple al gráfico para ver si el tamaño medio de la orden está creciendo o decreciendo a lo largo de la sesión.

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** Cálculo trivial.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una de las métricas más subestimadas del Order Flow. En un mundo de algoritmos que trocean las órdenes, ver el tamaño medio de ejecución es una de las pocas formas de intuir la presencia de grandes jugadores que no logran ocultar del todo su huella.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para filtrar la calidad de las rupturas.

**Acción:** **Conservar (Core).**