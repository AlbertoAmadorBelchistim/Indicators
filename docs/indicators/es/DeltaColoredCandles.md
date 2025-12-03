---
# 1. IDENTIFICACIÓN
cs_file:  DeltaColoredCandles.cs  
name:  Delta Colored Candles  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  Delta  
comparison_group:  "Bar Delta Analysis"  

# 3. VALORACIÓN (Score & Priority)
score_current:  3/10  
score_potential:  3/10  
file_state:  Estable  
effort:  Medio  
action_priority:  P4  
system_priority:  N/A  

# 4. DECISIÓN
recommended_action:  Descartar  

# 5. ANÁLISIS
description:  ¿Cuál es la intensidad del momentum del delta en relación con un máximo fijo, visualizada en el color de las velas?  
gemini_summary:  "Intento primitivo de visualización. Falla gravemente por depender de un parámetro 'MaxDelta' fijo que no se adapta a los cambios de volatilidad intradía, saturándose o quedándose 'ciego' según la hora."  
competitor_notes:  "Delta Modif ofrece visualización en precio muy superior y dinámica."  
reusable_code:  null  

# 6. METADATOS
analysis_date:  2025-11-21  
official_code_date:  2025-04-23  
---

## 💀 Delta Colored Candles (3/10)

**Nombre del archivo:** [`DeltaColoredCandles.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DeltaColoredCandles.cs)  
**Nombre del indicador:** Delta Colored Candles  
**Web oficial:** [ATAS — Delta Colored Candles](https://help.atas.net/support/solutions/articles/72000618743)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la intensidad del *momentum* del delta en relación con un máximo fijo, visualizada en el color de las velas?

![DeltaColoredCandles](../../img/DeltaColoredCandles.png)


---

### ⚙️ Parámetros configurables

* **Period:** [Parameter] Ventana de acumulación del delta (por defecto 14).  
* **MaxDelta:** [Parameter] Valor de referencia fijo para escalar el color (por defecto 600).  
* **ColorScheme:** [Display] Esquema de degradado de color.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Bar Delta Analysis"  


---

### 🧠 Uso más frecuente

* **Heatmap en Precio:** Ver la temperatura del delta en las velas.  


---

### 📊 Nivel de relevancia
🔟 **3 / 10**

⛔ **Estático:** Depende de un valor fijo manual. Inviable para sesiones con volatilidad variable.  
⛔ **Confuso:** Altera el color de la vela, dificultando la lectura de la acción del precio.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna fiable.** Requiere reajuste manual constante del parámetro `MaxDelta`.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **No Recomendado.** 

---

### 🧪 Notas de desarrollo

* Usa `PaintbarsDataSeries`.  
* Calcula suma de delta en N periodos y normaliza linealmente contra `MaxDelta`.  
* Usa `HeatmapExtensions.GetColor()` para generar el degradado.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta de Normalización Dinámica:** No usa ATR ni Desviación Estándar. Es una fórmula lineal simple contra un número arbitrario.  


---

### 🛠️ Propuestas de mejora

* **Ninguna.** Descartar.  


---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.**
---

### ✍️ La opinión de Gemini sobre el Indicador

Es un indicador obsoleto. En el trading algorítmico moderno, exigimos adaptabilidad. Una herramienta que me pide adivinar el "Delta Máximo" del día por adelantado no sirve.



---

### 📈 Veredicto: ¿Es útil para Scalping?

**No**

**Acción:** **Descartar**