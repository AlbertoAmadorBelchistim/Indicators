## 🟦 Adaptive RSI Moving Average (6/10)

  

**Nombre del archivo:**  `AdaptiveRsiAverage.cs`

**Nombre del indicador:** Adaptive RSI Moving Average

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602311](https://help.atas.net/support/solutions/articles/72000602311)

  

---

  

### ⚙️ Parámetros configurables

  

- **RsiPeriod**: Periodo del RSI base (por defecto: no especificado, depende del valor inicial del RSI interno)

- **RsiSmooth**: Suavizado del RSI, opcional y configurable (por defecto: activado, valor 10)

- **PriceSmooth**: Suavizado del precio de entrada, opcional y configurable (por defecto: activado, valor 10)

- **ScaleFactor**: Factor de adaptación de la media (por defecto: 0.5)

  

---

  

### 🧭 Clasificación

📂 Momentum — Medias móviles adaptativas según la fuerza del RSI

  

---

  

### 🧠 Uso más frecuente

  

- Crear una **media móvil que se adapta a la fuerza del RSI**, haciéndose más rápida cuando hay fuerte impulso y más lenta cuando el mercado está en rango

- Identificar zonas de **aceleración o desaceleración del precio**

- Servir como guía dinámica para entradas/salidas basadas en momentum

  

---

  

### 📊 Nivel de relevancia

🔟 **6 / 10**

  

✅ Originalidad en el uso del RSI para modular la suavidad de una media

✅ Buena para sistemas de seguimiento de tendencia con adaptación al contexto

⛔ Menos útil en operativa puramente basada en order flow o volumen

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Tendencias rápidas**: la media acelera su respuesta cuando el RSI muestra momentum

- **Filtros de entrada**: solo entrar en dirección de la pendiente adaptativa

- **Soportes dinámicos**: usarla como trailing stop en estructuras intradía

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **RsiPeriod**: `14`

- **RsiSmooth**: ✅ activado, valor `10`

- **PriceSmooth**: ✅ activado, valor `8`

- **ScaleFactor**: `0.6`

  

✅ Esta configuración permite que la media reaccione con rapidez en impulsos intradía

✅ Reduce ruido gracias al suavizado doble (precio y RSI)

⛔ Puede retrasarse si se usan valores demasiado altos en periodos de suavizado

  

---

  

### 🧪 Notas de desarrollo

  

- Se usa un RSI suavizado como regulador de la sensibilidad de una media móvil adaptativa.

- El cálculo se basa en la fórmula:

$$

MA_t = MA_{t-1} + (P_t - MA_{t-1}) \cdot \left[2 \cdot \text{ScaleFactor} \cdot \left|\frac{RSI}{100} - 0.5\right|\right]

$$

- La lógica principal se ejecuta en `OnCalculate`, mientras que `PriceSmooth` y `RsiSmooth` se aplican mediante filtros EMA internos.

- El valor de salida es igual al precio suavizado si no hay suficientes barras (menor o igual a `RsiPeriod`).

  

---

  

### 🛠️ Propuestas de mejora

  

- Permitir elegir entre otros osciladores adaptativos (ej. CCI, Stochastic) para modular la media

- Añadir opción de visibilidad de la línea base, RSI o la señal adaptativa por separado

- Incluir coloración dinámica de la línea según el ángulo de inclinación o aceleración

- Incorporar alertas visuales cuando el ángulo de la media supere cierto umbral

- Ofrecer un modo de histogramas comparativos entre la media base y la adaptativa
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTIxMjE0Mjc2NDVdfQ==
-->