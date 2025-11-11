## 🟦 Aroon Indicator (6/10)

  

**Nombre del archivo:**  `AroonIndicator.cs`

**Nombre del indicador:** Aroon Indicator

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602316](https://help.atas.net/support/solutions/articles/72000602316)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Número de barras para evaluar el máximo y mínimo recientes (por defecto: 10)

  

---

  

### 🧭 Clasificación

📂 Momentum — Indicadores de fuerza relativa de tendencia y reversión

  

---

  

### 🧠 Uso más frecuente

  

- Detectar **inicio o final de tendencias** basadas en la aparición reciente de máximos o mínimos

- Confirmar la **fuerza de la tendencia actual**: valores cercanos a 100 indican fortaleza

- Identificar zonas de **consolidación o cambio de tendencia** cuando ambas líneas convergen

  

---

  

### 📊 Nivel de relevancia

🔟 **6 / 10**

  

✅ Útil como herramienta secundaria para confirmar contexto de tendencia

✅ Fácil de interpretar y parametrizar

⛔ No considera volumen ni desequilibrios de order flow

⛔ Puede retrasarse en marcos muy cortos

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Confirmación de impulso**: entrada cuando la línea AroonUp supera 70 y AroonDown cae bajo 30

- **Detección de agotamiento**: cuando ambas líneas convergen hacia 50, posible reversión

- **Apoyo a ruptura**: si AroonUp se mantiene alto tras romper resistencia, mayor probabilidad de continuación

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `14`

  

✅ Esta configuración permite detectar tendencias recientes sin atraso

✅ Compatible con otros indicadores de momentum como CCI o RSI

⛔ Puede requerir ajuste en sesiones de alta volatilidad

  

---

  

### 🧪 Notas de desarrollo

  

- El indicador almacena los últimos `Period` valores de máximos y mínimos mediante una lista (`_extValues`).

- Calcula el número de barras transcurridas desde el máximo más alto y el mínimo más bajo en ese rango.

- Aplica la fórmula clásica de Aroon:

- `AroonUp = 100 * (Period - (bar - barDelMaximo)) / Period`

- `AroonDown = 100 * (Period - (bar - barDelMinimo)) / Period`

- Los valores se almacenan en dos `ValueDataSeries`: `_upSeries` (azul) y `_downSeries` (rojo).

- Incluye lógica para evitar duplicados si se recalcula el mismo `bar`.

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir opción para **alertas visuales o sonoras** al cruzarse AroonUp y AroonDown

- Incluir una tercera línea o color para **zona neutra** (ambos valores entre 40 y 60)

- Permitir **mostrar histograma de diferencia** entre ambas líneas como filtro direccional

- Soporte para **visualización condicional por color de fondo** (tendencia alcista / bajista / lateral)
<!--stackedit_data:
eyJoaXN0b3J5IjpbNjY0MjM3MzQyXX0=
-->