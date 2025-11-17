## 🟦 Highest High / Lowest Low Over N Bars (7/10)

**Nombre del archivo:** `HighLow.cs`  
**Nombre del indicador:** Highest High / Lowest Low Over N Bars  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602244](https://help.atas.net/support/solutions/articles/72000602244)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para calcular el máximo y mínimo (por defecto: 15)

---

### 🧭 Clasificación  
📂 Levels — Indicadores de extremos móviles (canales dinámicos)

---

### 🧠 Uso más frecuente

- Identificar el **rango dinámico** de precios de los últimos N periodos  
- Marcar niveles de **soporte y resistencia dinámicos**  
- Utilizar como base para breakout, reversión o trailing

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Fácil de interpretar visualmente  
✅ Sirve como base para múltiples estrategias técnicas  
⛔ No considera volumen ni momentum  
⛔ No discrimina entre máximos/minimos relevantes o recientes

---

### 🎯 Estrategias de scalping donde se aplica

- **Breakout por canal**: entrada si el precio supera el máximo/mínimo del periodo  
- **Trailing stop estructural**: usar el mínimo como trailing dinámico en largos  
- **Reversión técnica**: operar rechazos en zonas de alto/bajo relativo

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `15` a `20`  
- Color verde para máximos, rojo/gris para mínimos  
- Combinar con Delta o DOM Strength para validar la agresión en extremos

✅ Muy eficaz para detectar puntos de decisión clave  
✅ Compatible con estructuras de rango, acumulación o continuación

---

### 🧪 Notas de desarrollo

- Usa dos series auxiliares (`_highSeries` y `_lowSeries`) para almacenar valores High y Low de cada vela  
- Calcula:
  - **Máximo**: `_highSeries.MAX(_period, bar)`  
  - **Mínimo**: `_lowSeries.MIN(_period, bar)`  
- Almacena los resultados en `_maxSeries` y `_minSeries`, que se visualizan directamente  
- Ambos valores se recalculan por cada barra en `OnCalculate()`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El indicador **no permite elegir el tipo de precio** (por ejemplo, `Close`, `Typical`, `Median`)  
- No se controla si el gráfico tiene menos barras que el `Period` inicial  
- Los colores por defecto solo se aplican al máximo (`_maxSeries`), no al mínimo

---

### 🛠️ Propuestas de mejora

- Añadir parámetro para seleccionar el tipo de precio (High, Close, Typical, etc.)  
- Visualizar los niveles como `RangeDataSeries` para poder extenderlos hasta ser rotos  
- Añadir etiquetas opcionales con los valores actuales del máximo y mínimo  
- Incorporar una lógica de alerta cuando se rompa el máximo/mínimo actual
