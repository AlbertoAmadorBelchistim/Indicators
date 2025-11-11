## 🟦 ATR (6/10)

  

**Nombre del archivo:**  `ATR.cs`

**Nombre del indicador:** ATR

**Web oficial:**  [ATAS - ATR](https://help.atas.net/support/solutions/articles/72000602536)
**Compatibilidad:** ATAS versión stable y superiores.

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Periodo de cálculo del ATR (por defecto: 10)

- **Multiplier**: Multiplicador aplicado al valor del ATR (por defecto: 1)

  

---

  

### 🧭 Clasificación

📂 Volatility — Indicadores de volatilidad basados en rango verdadero medio

  

---

  

### 🧠 Uso más frecuente

  

- Medir la **volatilidad media reciente** de un instrumento

- Ajustar **stops dinámicos** en función del rango medio

- Determinar si un mercado está en **expansión o contracción de rango**

- Filtrar entradas en función de la volatilidad

  

---

  

### 📊 Nivel de relevancia

🔟 **6 / 10**

  

✅ Muy útil como **indicador base** en sistemas de gestión de riesgo

✅ Fácil de interpretar y parametrizar

⛔ No discrimina dirección ni contexto: **solo mide magnitud del rango**

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Stops adaptativos**: SL de 1.5xATR para evitar barridas en alta volatilidad

- **Filtro de entrada**: solo operar si ATR > umbral mínimo de rango

- **Gestión de riesgo**: ajustar tamaño de posición inversamente proporcional al ATR

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `14`

- **Multiplier**: `1.0`

  

✅ Refleja con precisión la volatilidad reciente

✅ Evita entradas en momentos de compresión

⛔ Sensible a spikes; considerar suavizar con media exponencial si se desea mayor estabilidad

  

---

  

### 🧪 Notas de desarrollo

  

- El cálculo del rango verdadero (`True Range`) usa el máximo entre:

- Diferencia entre el máximo y el mínimo de la vela actual

- Distancia entre el cierre anterior y el mínimo actual

- Distancia entre el cierre anterior y el máximo actual

- El valor del ATR se calcula como una **media móvil simple** sobre el rango verdadero

- El resultado final se multiplica por el factor `Multiplier`

- El valor se almacena en `_values[bar]` y luego se aplica el multiplicador antes de mostrarse

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir opción para **EMA en lugar de SMA** en el cálculo del ATR

- Incluir **color dinámico** si ATR sube o baja respecto a velas anteriores

- Permitir **mostrar valor como etiqueta en el gráfico**

- Incluir **nivel medio de ATR histórico** para comparar con el valor actual

##Comentario de Gemin
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTYyMTA5MzUzMl19
-->