## 🟦 MACD Bollinger Bands - Standard (6.5/10)

**Nombre del archivo:** `MacdBbStandart.cs`  
**Nombre del indicador:** MACD Bollinger Bands - Standard  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602295](https://help.atas.net/support/solutions/articles/72000602295)

---

### ⚙️ Parámetros configurables

- **MacdPeriod**: Periodo de la señal del MACD y del cálculo de desviación estándar (por defecto: 9)  
- **MacdShortPeriod**: Periodo corto del MACD (por defecto: 12)  
- **MacdLongPeriod**: Periodo largo del MACD (por defecto: 26)  
- **StdDev**: Multiplicador de la desviación estándar aplicada sobre la línea de señal del MACD (por defecto: 2)

---

### 🧭 Clasificación
📂 Momentum — MACD con envolventes de volatilidad estándar (tipo Bollinger)

---

### 🧠 Uso más frecuente

- Determinar si el MACD se mueve dentro o fuera de un rango normal de desviación  
- Medir la **expansión de momentum** mediante la separación de las bandas  
- Identificar condiciones de **sobrecompra/sobreventa relativa al MACD**

---

### 📊 Nivel de relevancia
🔟 **6.5 / 10**

✅ Más simple y limpio que su versión “Improved”  
✅ Útil como envolvente para validar condiciones extremas de impulso  
⛔ No considera la aceleración del MACD como en la versión “Improved”

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión en banda extrema**: entrada si el MACD toca o cruza las bandas  
- **Confirmación de ruptura**: si el MACD se expande y rompe el canal  
- **Entrada segura**: si el MACD vuelve dentro de las bandas tras salida extrema

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **MacdPeriod**: `6`  
- **MacdShortPeriod**: `8`  
- **MacdLongPeriod**: `21`  
- **StdDev**: `2`

✅ Detección clara de condiciones extremas sin ruido excesivo  
✅ Compatible con setups de reversión o expansión  
⛔ Requiere contexto o filtro direccional para evitar señales engañosas

---

### 🧪 Notas de desarrollo

- Utiliza el indicador `MACD` clásico interno para obtener la línea de señal  
- Aplica `StdDev` sobre la línea de señal y genera dos bandas: superior e inferior  
- Las bandas se representan mediante `ValueDataSeries` púrpuras  
- La lógica de bandas es más sencilla que en la versión “Improved” (no considera el histograma)

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La desviación se aplica directamente sobre la señal del MACD sin tener en cuenta la variabilidad de la diferencia (histograma), lo que puede subestimar movimientos extremos  
- La propiedad `MacdPeriod` afecta simultáneamente a la señal y a la desviación estándar, reduciendo control independiente  
- No se valida si `ShortPeriod > LongPeriod`, lo que puede romper la lógica del MACD  
- Falta codificación de color o alerta cuando se cruzan las bandas  
- Las series del MACD no se renombran ni distinguen en esta versión respecto a la base

---

### 🛠️ Propuestas de mejora

- Separar el control de periodos para `MACD` y `StdDev`  
- Añadir opción para mostrar histograma o diferencia entre MACD y señal  
- Colorear la zona entre bandas o aplicar relleno dinámico  
- Incluir alertas visuales/auditivas al cruce de las bandas  
- Validar coherencia entre parámetros (e.g., `Short < Long`)

