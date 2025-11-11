## 🟦 Adaptive RSI Moving Average (6/10)

  

**Nombre del archivo:**  `AdaptiveRsiAverage.cs`

**Nombre del indicador:** Adaptive RSI Moving Average

**Web oficial:**  [ATAS - Adaptive RSI Moving Average](https://help.atas.net/support/solutions/articles/72000602311)

  

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

## Comentario Gemini
Aquí tienes la "pregunta clave" de este indicador:
**¿Cómo puedo obtener una media móvil que automáticamente se ralentice cuando el mercado está indeciso (RSI cerca de 50) y se acelere para capturar tendencias cuando el momentum es fuerte (RSI cerca de 0 o 100)?**

----------
### ✍️ Mi Opinión sobre el Indicador

Vamos a analizar su proceso:

1.  **Lag 1:** Coge el precio y lo suaviza con una `EMA(10)` (el `PriceSmooth`).
    
2.  **Lag 2:** Calcula el `RSI(14)` sobre ese precio ya suavizado.
    
3.  **Lag 3:** Coge ese RSI y lo vuelve a suavizar con otra `EMA(10)` (el `RsiSmooth`).
    
4.  **Lag 4:** Usa ese valor (suavizado 3 veces) como el factor de suavizado para la media móvil final.
  

Es un indicador de **"lag sobre lag sobre lag"**.

El resultado es el que se ve en tu captura de pantalla: una media móvil **increíblemente suave**.

-   **Lo Bueno:** Filtra el "chop" de manera espectacular (mira cómo se aplana de 03:50 a 08:10).
    
-   **Lo Malo:** Es _muy_ lenta para reaccionar a los giros bruscos. Para cuando te da la señal, el movimiento ya ha comenzado (mira la caída de las 18:40).
    

### 📈 Veredicto: ¿Es útil para Scalping?

Es el "primo lento" del AMA (Kaufman).

**Acción:** **Descartar**.
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTM5MTA5NzA1MiwtMTg0MjEzMjUwMF19
-->