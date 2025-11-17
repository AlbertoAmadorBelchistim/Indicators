## 🟦 MACD Bollinger Bands - Improved (6.5/10)

**Nombre del archivo:** `MacdBbImproved.cs`  
**Nombre del indicador:** MACD Bollinger Bands - Improved  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602628](https://help.atas.net/support/solutions/articles/72000602628)

---

### ⚙️ Parámetros configurables

- **MacdPeriod**: Periodo del cálculo del MACD y de la señal (por defecto: 9)  
- **MacdShortPeriod**: Periodo corto del MACD (por defecto: 12)  
- **MacdLongPeriod**: Periodo largo del MACD (por defecto: 26)  
- **StdDev**: Multiplicador de la desviación estándar para calcular las bandas (por defecto: 2)

---

### 🧭 Clasificación
📂 Momentum — Extensión del MACD con bandas dinámicas basadas en volatilidad

---

### 🧠 Uso más frecuente

- Detectar condiciones de **expansión o contracción del impulso** mediante bandas alrededor del MACD  
- Identificar momentos de **divergencia o extremos** en la diferencia entre MACD y su señal  
- Confirmar entradas en rupturas con validación del rango del histograma

---

### 📊 Nivel de relevancia
🔟 **6.5 / 10**

✅ Añade contexto de volatilidad al análisis del MACD  
✅ Mejora la lectura visual de extremos y consolidaciones  
⛔ Puede ser redundante si ya se usa Bollinger Bands sobre el precio directamente

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión en banda extrema**: entrada cuando el histograma alcanza banda superior o inferior  
- **Confirmación de ruptura**: si el histograma cruza banda tras consolidación  
- **Filtro de entrada**: evitar operar cuando el histograma está dentro de las bandas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **MacdPeriod**: `6`  
- **MacdShortPeriod**: `8`  
- **MacdLongPeriod**: `21`  
- **StdDev**: `2`

✅ Resalta condiciones de impulso excesivo o pérdida de fuerza  
✅ Compatible con operaciones en entornos volátiles  
⛔ La configuración muy sensible puede generar señales ruidosas

---

### 🧪 Notas de desarrollo

- Combina un cálculo estándar de MACD con bandas basadas en:  
  `Banda Superior = MACD señal + SMA(|MACD - Señal|) + StdDev * σ`  
  `Banda Inferior = MACD señal - SMA(|MACD - Señal|) - StdDev * σ`  
- Usa un objeto `MACD` interno y calcula diferencia de su línea con la señal  
- Las bandas se dibujan como dos `ValueDataSeries` púrpuras  
- El panel es independiente (`NewPanel`) y las series internas del MACD se heredan directamente

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El parámetro `MacdPeriod` afecta simultáneamente al `SMA`, `StdDev` y señal del MACD, lo que puede limitar flexibilidad  
- El valor de `deltaMacd` puede ser negativo, pero solo se usa su valor absoluto para calcular el suavizado, ocultando dirección  
- No se proporciona ninguna alerta visual o auditiva si el histograma cruza las bandas  
- No hay validación cruzada entre los parámetros (por ejemplo, `ShortPeriod` no debería ser mayor que `LongPeriod`)  
- Se heredan las series del MACD original, lo que puede confundir si no se diferencian claramente en la UI

---

### 🛠️ Propuestas de mejora

- Permitir configurar los periodos del `SMA` y `StdDev` de forma independiente del MACD  
- Incluir opción para colorear las bandas según la dirección del histograma  
- Añadir alertas configurables al cruce de bandas por el histograma  
- Validar coherencia entre periodos `Short`, `Long` y `Signal` del MACD  
- Mostrar de forma explícita el histograma como serie separada para facilitar interpretación

