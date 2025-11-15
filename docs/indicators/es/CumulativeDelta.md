## 🟦 CVD - Cumulative Volume Delta (9/10)

**Nombre del archivo:** `CumulativeDelta.cs`  
**Nombre del indicador:** CVD - Cumulative Volume Delta  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602360-cumulative-volume-delta](https://help.atas.net/support/solutions/articles/72000602360-cumulative-volume-delta)

---

### ⚙️ Parámetros configurables

- **Mode**: Tipo de visualización (Candles, Bars, Line)  
- **SessionCumDeltaMode**: Tipo de sesión para reiniciar acumulación (None / Default / Custom)  
- **CustomSessionStart**: Hora de inicio para sesión personalizada  
- **UseScale**: Usar escala  
- **ShowValue**: Mostrar valor actual  
- **IsVisible**: Visibilidad del indicador  
- **PosColor / NegColor**: Colores para delta positivo o negativo  
- **AlertFile**: Archivo de sonido para alerta  
- **ChangeSize**: Delta mínimo para lanzar alerta  
- **AlertBGColor / AlertForeColor**: Colores para fondo y texto de alerta  
- **BorderColorFilter / CandleModeFilter / WidthFilter / LineStyleFilter**: Opciones visuales y de estilo para velas o línea

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Delta acumulado por sesión o total

---

### 🧠 Uso más frecuente

- Acumular el **delta por sesión o de forma continua** para detectar presión agresiva  
- Visualizar el comportamiento del flujo de órdenes comprador vs vendedor  
- Identificar **divergencias entre precio y delta acumulado**  
- Generar alertas ante movimientos de delta superiores a cierto umbral

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Uno de los indicadores más utilizados en order flow  
✅ Altamente configurable para análisis en tiempo real o sesiones personalizadas  
⛔ Puede dar señales confusas en rangos laterales o días de bajo volumen  
⛔ Requiere buena configuración del modo de sesión para evitar falsos resets

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión con divergencia de delta**  
- **Confirmación de breakout con delta creciente**  
- **Absorción**: si el precio sube pero el CVD cae  
- **Entrada con alerta de delta fuerte en soporte/resistencia**

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Mode**: `Candles` o `Line`  
- **SessionCumDeltaMode**: `CustomSession`, con hora personalizada a 15:30 UTC  
- **ChangeSize**: `4000`  
- **PosColor / NegColor**: Verde / Rojo intensos  
- **AlertBGColor**: gris oscuro  
- **WidthFilter**: `2`  
- **UseScale**: `true`

✅ Facilita la lectura estructural del delta intradía  
✅ Las alertas permiten reacción rápida a spikes de agresión

---

### 🧪 Notas de desarrollo

- Acumula `candle.Delta` para construir una serie de delta acumulado  
- Soporta tres modos de visualización:  
  - `Candles`: como velas tradicionales de delta  
  - `Bars`: histograma  
  - `Line`: curva continua  
- Permite reiniciar el cálculo:
  - Nunca (`None`)  
  - Por sesión (`DefaultSession`)  
  - Por hora personalizada (`CustomSession`)  
- El acumulado se reinicia en función de `CheckStartBar()` que adapta la lógica a la sesión seleccionada  
- Admite alertas configurables al superar cierto tamaño de delta  
- El valor acumulado se guarda en `_cumDelta` y se actualiza dinámicamente

---

### ❗ Incoherencias o aspectos mejorables detectados

- El cálculo de **`CheckStartBar()` usa `AddHours(InstrumentInfo.TimeZone)`**, lo cual puede fallar si el timezone es negativo o si el horario de verano está mal definido.  
- En modo `Candles`, el valor de `_currentCandle` se asigna sin protección si ya existía en una llamada previa.  
- El valor de referencia del eje cero (`ZeroLine`) puede no actualizarse correctamente si `LineSeries[0]` no se inicializa en orden.

---

### 🛠️ Propuestas de mejora

- Añadir opción para **acumular delta por bloques de tiempo (ej. cada 5 minutos)**  
- Añadir soporte para **varios acumuladores simultáneos** (por sesión, diario, semana)  
- Optimizar gestión de memoria con reuso de objetos `Candle`  
- Mostrar **etiquetas de divergencia automáticas**  
- Control más preciso sobre cuándo y cómo se resetea el delta acumulado