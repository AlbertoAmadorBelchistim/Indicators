## 🟦 Bollinger Squeeze (7/10)

  

**Nombre del archivo:**  `BollingerSqueeze.cs`

**Nombre del indicador:** Bollinger Squeeze

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602337](https://help.atas.net/support/solutions/articles/72000602337)

  

---

  

### ⚙️ Parámetros configurables

  

#### Bollinger Bands

- **BbPeriod**: Periodo del SMA y desviación estándar

- **BbWidth**: Multiplicador aplicado a la desviación estándar

  

#### Keltner Channel

- **KbPeriod**: Periodo del ATR

- **KbMultiplier**: Multiplicador aplicado al ATR

  

---

  

### 🧭 Clasificación

📂 Volatility / Squeeze — Indicador de contracción del rango entre dos canales

  

---

  

### 🧠 Uso más frecuente

  

- Detectar **zonas de baja volatilidad extrema** (squeeze)

- Anticipar **movimientos explosivos** tras una contracción prolongada

- Filtrar contextos de alta o baja probabilidad de breakout

- Estimar cuándo el rango está demasiado comprimido para operar

  

---

  

### 📊 Nivel de relevancia

🔟 **7 / 10**

  

✅ Muy útil para detectar fases de acumulación o pausa

✅ Compatible con estrategias de breakout o momentum

⛔ No muestra dirección, solo intensidad relativa

⛔ No tiene alertas ni niveles de referencia predefinidos

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Entrada tras ruptura del squeeze**: esperar expansión tras contracción visible

- **Evitar operar en compresión**: cuando el histograma se estabiliza en valores bajos

- **Validación contextual**: combinar con flujo o volumen para detectar ruptura real

- **Backtesting de zonas squeeze**: analizar qué rompimientos funcionaron

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **BbPeriod**: `20`

- **BbWidth**: `2`

- **KbPeriod**: `20`

- **KbMultiplier**: `1.5`

  

✅ Detecta compresiones con alto potencial de ruptura

✅ Compatible con volumen o delta como disparador adicional

⛔ Puede necesitar ajuste en días de alta volatilidad

  

---

  

### 🧪 Notas de desarrollo

  

- Calcula dos canales:

- **Bollinger Bands** (`SMA ± Width × StdDev`)

- **Keltner Channel** (`EMA ± Multiplier × ATR`)

- Mide el ratio entre ambos: `(Keltner width / Bollinger width) - 1`

- Si el resultado es positivo, se dibuja en verde; si es negativo, en rojo

- Utiliza dos `ValueDataSeries` (`UpRatio`, `DownRatio`) en formato histograma

- No se dibujan áreas ni líneas, solo barras verticales

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir línea base en 0 para referencia clara

- Incluir umbrales visuales para detectar squeeze extremos

- Soporte para alertas al entrar/salir del squeeze

- Posibilidad de combinarlo con momentum o volumen directamente

### Comentario Gemini
<!--stackedit_data:
eyJoaXN0b3J5IjpbNTUyODYyNjNdfQ==
-->