## 🟦 Exponential Moving Average (EMA) (8/10)

**Nombre del archivo:** `EMA.cs`  
**Nombre del indicador:** Exponential Moving Average  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602641](https://help.atas.net/support/solutions/articles/72000602641)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para el cálculo de la media exponencial (por defecto: 10)  
- **ColoredDirection**: Activar coloración según dirección de la EMA  
- **BullishColor / BearishColor**: Colores para pendiente ascendente o descendente  
- **UseAlerts**: Activar alertas de proximidad del precio a la EMA  
- **RepeatAlert**: Permitir alertas múltiples en una misma barra  
- **AlertSensitivity**: Número de ticks máximo entre el precio y la EMA para disparar alerta  
- **AlertFile**: Archivo de sonido para la alerta  
- **FontColor / BackgroundColor**: Colores del texto y fondo de la alerta

---

### 🧭 Clasificación  
📂 Trend — Medias móviles con lógica adaptativa y alertas

---

### 🧠 Uso más frecuente

- Suavizar el precio para identificar la **tendencia predominante**  
- Activar alertas cuando el **precio se aproxima a la EMA**  
- Usar coloración para **confirmar pendiente** o zona favorable

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Muy útil como referencia dinámica para entrada y salida  
✅ Alta visibilidad gracias a la coloración y alertas configurables  
⛔ Solo considera el cierre, no admite otros tipos de precio  
⛔ No expone valores intermedios o pendientes como series auxiliares

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por rebote en EMA**: comprar cuando el precio toca EMA ascendente  
- **Reversión o retesteo**: operar rechazo tras testear la EMA  
- **Filtro de sesgo direccional**: operar solo en la dirección del color de la EMA  
- **Alerta reactiva**: recibir notificación cuando el precio se aproxima a la EMA con baja distancia

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `9` a `13`  
- **ColoredDirection**: `true`  
- **UseAlerts**: `true`  
- **AlertSensitivity**: `1` (1 tick)  
- **BullishColor / BearishColor**: verde / rojo  
- **AlertFile**: `"alert1"`

✅ Aporta alta precisión visual y auditiva  
✅ Compatible con otras herramientas de confirmación (Delta, DOM)

---

### 🧪 Notas de desarrollo

- El valor se calcula como:
  $$
  EMA_t = \alpha \cdot \text{Precio}_t + (1 - \alpha) \cdot EMA_{t-1}, \quad \alpha = \frac{2}{1 + \text{Period}}
  $$
- El valor inicial (bar = 0) se iguala al primer precio  
- Si `ColoredDirection` está activo, la línea se colorea según la pendiente  
- Las alertas se activan si la diferencia entre `Close` y la EMA es menor o igual al número de ticks definido por `AlertSensitivity`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El cálculo usa el `Close` como entrada fija, sin posibilidad de configurar otro tipo de precio  
- La condición `bar != CurrentBar - 1` para alertas impide dispararlas en barras pasadas o durante ciertos replays  
- No hay control sobre repintado si `ColoredDirection` se modifica con el gráfico ya cargado

---

### 🛠️ Propuestas de mejora

- Permitir elegir el tipo de precio para calcular la EMA (Typical, Weighted, Median, etc.)  
- Exponer la **pendiente de la EMA** como serie auxiliar para usarla como filtro cuantitativo  
- Añadir líneas auxiliares de alerta visible sobre el gráfico  
- Permitir representar también la diferencia entre el precio y la EMA como histograma
