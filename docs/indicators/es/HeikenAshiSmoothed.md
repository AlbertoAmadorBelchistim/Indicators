## 🟦 Heiken Ashi Smoothed (6/10)

**Nombre del archivo:** `HeikenAshiSmoothed.cs`  
**Nombre del indicador:** Heiken Ashi Smoothed  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602392](https://help.atas.net/support/solutions/articles/72000602392)

---

### ⚙️ Parámetros configurables

- **SmmaPeriod**: Periodo de suavizado tipo SMMA (por defecto: 10)  
- **WmaPeriod**: Periodo de suavizado tipo WMA para la salida visual (por defecto: 10)  
- **ShowBars**: Mostrar o no las barras suavizadas en el gráfico

---

### 🧭 Clasificación  
📂 Visualization — Representación suavizada de velas para análisis de tendencia

---

### 🧠 Uso más frecuente

- Eliminar ruido del gráfico mediante **velas Heiken Ashi doblemente suavizadas**  
- Identificar **cambios de fase** más limpios y consistentes  
- Confirmar impulso o retroceso con mayor estabilidad visual

---

### 📊 Nivel de relevancia  
🔟 **6 / 10**

✅ Muy útil como contexto visual para filtrar señales de ruido  
✅ Compatible con indicadores de impulso o clúster  
⛔ No proporciona señales por sí mismo  
⛔ La suavización doble puede provocar cierto retraso en los giros

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada cuando las velas cambian de color sostenidamente**  
- **Filtro de tendencia visual**: operar solo en dirección del bloque actual  
- **Confirmación de ruptura** si las velas smoothed mantienen color tras breakout

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **SmmaPeriod**: `10`  
- **WmaPeriod**: `5`  
- **ShowBars**: `true`  
- Usar junto con Delta o DOM Strength para validación

✅ Ayuda a interpretar contexto direccional sin saturar el gráfico  
✅ Compatible con estructuras como Spring, Upthrust o absorciones

---

### 🧪 Notas de desarrollo

- Aplica primero un **suavizado SMMA** independiente a Open, High, Low y Close  
- Luego, calcula una vela tipo Heiken Ashi basada en estos valores suavizados  
- Finalmente, aplica un segundo **suavizado WMA** a la vela generada para mostrar la versión final (`_smoothedCandles`)  
- Las velas se dibujan usando `CandleDataSeries` con colores adaptados desde el gráfico principal  
- Puede ocultarse o mostrarse en el gráfico con `ShowBars`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- En `OnCalculate`, la lógica de `bar == 0` salta el suavizado WMA, lo que genera una vela inicial no coherente con el resto  
- El cálculo de High y Low utiliza `Math.Max` y `Math.Min` pero **no asegura coherencia con las velas originales si hay gaps extremos**  
- No hay visualización del cuerpo real de Heiken Ashi intermedio, solo la versión suavizada

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar tanto la vela Heiken Ashi intermedia como la suavizada final  
- Permitir elegir tipo de suavizado alternativo (EMA, SMMA, etc.)  
- Añadir etiquetas o alertas cuando cambia de color tras X barras  
- Incluir líneas auxiliares con el cierre smoothed para usarlo como trailing o referencia
