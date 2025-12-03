---
# 1. IDENTIFICACIÓN
cs_file:  AskBidBars.cs  
name:  Ask/Bid Volume Difference Bars  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  Delta  
comparison_group:  "Bar Delta Analysis"  

# 3. VALORACIÓN (Score & Priority)
score_current:  4/10  
score_potential:  4/10  
file_state:  Estable  
effort:  N/A  
action_priority:  P4  
system_priority:  N/A  

# 4. DECISIÓN
recommended_action:  Descartar  

# 5. ANÁLISIS
description:  ¿Cuál fue el rango total (Max/Min) y el cierre neto del Delta en la vela?  
gemini_summary:  "Aunque ofrece más información que un histograma simple (al mostrar MaxDelta y MinDelta), esta funcionalidad ya está totalmente absorbida por el indicador principal 'Delta Modif' en su modo 'Velas/High-Low'. Además, este indicador tiene un defecto visual (falta de línea cero) y no posee alertas. Su mantenimiento es redundante."  
competitor_notes:  "Totalmente redundante frente a Delta Modif, que ofrece la misma visualización enriquecida con estadística y señales."  
reusable_code:  null  

# 6. METADATOS
analysis_date:  2025-12-03  
official_code_date:  2025-04-23  
---

## 💀 Ask/Bid Volume Difference Bars (4/10)

**Nombre del archivo:** [`AskBidBars.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/AskBidBars.cs)  
**Nombre del indicador:** Ask/Bid Volume Difference Bars  
**Web oficial:** [ATAS — Ask/Bid Volume Difference Bars](https://help.atas.net/support/solutions/articles/72000602527)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál fue el rango total (Max/Min) y el cierre neto del Delta en la vela?

![AskBidBars](../../img/AskBidBars.png)


---

### ⚙️ Parámetros configurables

* **Colors:** Hereda los colores de las velas del gráfico principal o se configuran en la serie.  
* *Nota:* No tiene parámetros de configuración lógica expuestos.  


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Bar Delta Analysis"  


---

### 🧠 Uso más frecuente

* **Estructura del Delta:** Ver la volatilidad interna del flujo de órdenes (mechas del delta).  


---

### 📊 Nivel de relevancia
🔟 **4 / 10**

⛔ **Redundante:** El indicador Core (`Delta Modif`) ya replica esta visualización (Modo Candles) con mayor calidad.  
⛔ **Defecto Visual:** No pinta línea cero por defecto, dificultando la lectura rápida.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Usar Delta Modif en su lugar.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No Recomendado.** 

---

### 🧪 Notas de desarrollo

* Utiliza `CandleDataSeries` mapeando `High=MaxDelta` y `Low=MinDelta`.  


---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta de Línea Cero:** Debería forzar `ShowZeroValue = true` en la serie de datos para ser usable "out of the box".  


---

### 🛠️ Propuestas de mejora

* **Ninguna.** Eliminación directa.  


---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Mapeo:** El concepto `High=Max` y `Low=Min` ya se usa en Delta Modif.  


---

### ✍️ La opinión de Gemini sobre el Indicador

Idea excelente con ejecución incompleta y ahora redundante.



---

### 📈 Veredicto: ¿Es útil para Scalping?

**No (Por redundancia)**

Su utilidad queda anulada al tener el `Delta Modif` configurado correctamente.

**Acción:** **Descartar**