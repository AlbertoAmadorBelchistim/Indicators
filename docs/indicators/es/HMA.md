## 🟦 Hull Moving Average (HMA) (8/10)

**Nombre del archivo:** `HMA.cs`  
**Nombre del indicador:** Hull Moving Average  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602550](https://help.atas.net/support/solutions/articles/72000602550)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para el cálculo del HMA (por defecto: 16)  
- **ColoredDirection**: Activar coloración según pendiente del indicador  
- **BullishColor / BearishColor**: Colores para pendiente alcista o bajista

---

### 🧭 Clasificación  
📂 Trend — Medias móviles optimizadas para suavizado sin retraso

---

### 🧠 Uso más frecuente

- Suavizar la acción del precio con **mínimo retraso**  
- Identificar **cambios de dirección** con mayor rapidez que SMA o EMA  
- Confirmar fases de impulso o transición en estrategias de seguimiento de tendencia

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Suavizado sin retraso aparente con buena reacción  
✅ Visualización clara con colores según dirección  
⛔ Sensible a sobreoptimización del periodo  
⛔ No permite elegir el tipo de precio base (siempre Close)

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce del precio con HMA**: como entrada táctica  
- **Cambio de color**: operar en la dirección de la pendiente del indicador  
- **Filtro direccional**: usar HMA para validar setups basados en agresión, DOM o estructuras

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `16`  
- **ColoredDirection**: `true`  
- **BullishColor / BearishColor**: verde/rojo  
- Dibujarlo sobre el gráfico principal (`DenyToChangePanel = true`)

✅ Buen filtro de tendencia rápida  
✅ Compatible con setups visuales de microestructura

---

### 🧪 Notas de desarrollo

- Se basa en la fórmula original de Alan Hull:  
  $$
  HMA = WMA\left(2 \cdot WMA(\text{Close}, n/2) - WMA(\text{Close}, n), \sqrt{n}\right)
  $$
- Usa tres WMA internas:
  - `WMA_1`: con `n` periodos  
  - `WMA_2`: con `n/2` periodos  
  - `WMA_final`: con `sqrt(n)` periodos  
- El resultado se almacena en `_renderSeries[bar]`  
- Si `ColoredDirection = true`, se colorea la línea según su pendiente respecto a la vela anterior

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se controla el caso donde `Period < 2`, lo que podría llevar a WMA de periodo 0 (aunque se fuerza mínimo de 1 en `[Range]`)  
- El tipo de precio usado es siempre `candle.Close`, sin opción de personalizar (Typical, Median, etc.)  
- Los WMA se instancian como objetos separados pero sin validación cruzada de entrada (valor NaN o vacío)

---

### 🛠️ Propuestas de mejora

- Permitir elegir el tipo de precio base (Close, Typical, Weighted)  
- Añadir visualización opcional de los tres componentes intermedios de la fórmula  
- Incluir una alerta visual o sonora cuando cambie de pendiente  
- Implementar una versión que muestre histograma de pendiente o cruce con el precio
