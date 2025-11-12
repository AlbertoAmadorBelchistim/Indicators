## 🟦 Bollinger Squeeze 2 (8/10)

**Nombre del archivo:**  `BollingerSqueezeV2.cs`  
**Nombre del indicador:**  Bollinger Squeeze 2  
**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602634](https://www.google.com/url?q=https%3A%2F%2Fhelp.atas.net%2Fsupport%2Fsolutions%2Farticles%2F72000602634)

----------

### ⚙️ Parámetros configurables

#### Bollinger Bands

-   **BbPeriod**: Periodo de cálculo del canal
-   **BbWidth**: Ancho del canal (multiplicador de desviación estándar)

#### Keltner Channel

-   **KbPeriod**: Periodo del ATR usado en el canal
-   **KbMultiplier**: Multiplicador aplicado al ATR

#### Momentum

-   **MomentumPeriod**: Periodo del oscilador Momentum
-   **EmaMomentum**: Periodo del EMA aplicado al Momentum

#### Visualización

-   **UpperColor / UpColor**: Colores para momentum positivo creciente o decreciente
-   **LowerColor / LowColor**: Colores para momentum negativo decreciente o creciente

----------

### 🧭 Clasificación

📂 Volatility / Squeeze — Indicador combinado de contracción con dirección de momentum

----------

### 🧠 Uso más frecuente

-   Detectar  **squeeze de volatilidad**  (BB dentro de KC) con  **dirección de momentum**
-   Confirmar si la presión está a favor o en contra tras compresión
-   Identificar  **rupturas filtradas**  por impulso real
-   Colorear visualmente zonas con mayor probabilidad de expansión

----------

### 📊 Nivel de relevancia

🔟  **8 / 10**

✅ Integra squeeze + momentum para señales más sólidas  
✅ Muy útil para confirmar dirección tras compresión  
⛔ Puede volverse sensible en entornos de alta volatilidad  
⛔ No es intuitivo si no se conoce bien la lógica

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Ruptura validada**: squeeze + EMA momentum creciente en la misma dirección
-   **Filtro de ruido**: evitar squeeze sin dirección clara (colores alternos)
-   **Entrada temprana**: aprovechar puntos donde momentum cambia mientras se rompe el squeeze
-   **Confirmación de impulso**: zona verde o roja intensa posterior a compresión

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **BbPeriod**:  `20`,  **BbWidth**:  `2`
-   **KbPeriod**:  `20`,  **KbMultiplier**:  `1.5`
-   **MomentumPeriod**:  `10`,  **EmaMomentum**:  `8`
-   **UpperColor**: verde claro
-   **LowerColor**: rojo claro

✅ Detecta fases de compresión con dirección válida  
✅ Proporciona señales visuales claras y filtradas  
⛔ Puede dar señales contradictorias si el momentum oscila lateralmente

----------

### 🧪 Notas de desarrollo

-   Detecta squeeze clásico: Bollinger dentro de Keltner
-   Calcula un oscilador Momentum suavizado por un EMA
-   El valor del EMA determina el color de fondo (positivo/negativo y su pendiente)
-   Si hay squeeze, se dibujan puntos (`ValueDataSeries`) en verde o rojo
-   El histograma (`RenderSeries`) toma el valor del EMA del momentum

----------

### 🛠️ Propuestas de mejora

-   Añadir  **alertas sonoras o visuales**  al iniciar/romper un squeeze
-   Permitir mostrar línea base cero y líneas de referencia del momentum
-   Exportar señales de entrada/salida para backtesting
-   Opción de  **cambiar el suavizado por otras medias móviles**  (WMA, SMMA, etc.)
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE3Njg1NjU1MjddfQ==
-->