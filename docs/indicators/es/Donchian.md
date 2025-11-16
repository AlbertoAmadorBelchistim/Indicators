## 🟦 Donchian Channel (8/10)

**Nombre del archivo:** `Donchian.cs`  
**Nombre del indicador:** Donchian Channel  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602376](https://help.atas.net/support/solutions/articles/72000602376)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para calcular el máximo y mínimo (por defecto: 20)  
- **ShowAverage**: Mostrar o no la línea media (mitad entre el máximo y el mínimo)

---

### 🧭 Clasificación  
📂 Levels — Indicadores de canal y ruptura

---

### 🧠 Uso más frecuente

- Delimitar el **rango máximo y mínimo** en una ventana móvil  
- Detectar **rupturas de rango** al superar el canal  
- Usar la media como referencia de balance o punto medio del rango

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Muy útil en estrategias de breakout o reversión  
✅ Fácil de interpretar visualmente  
⛔ No considera volumen ni momentum, sólo precio  
⛔ Puede dar señales falsas en fases de alta volatilidad lateral

---

### 🎯 Estrategias de scalping donde se aplica

- **Breakout confirmado**: entrada si se rompe el máximo con delta a favor  
- **Reversión intradía**: venta en test del techo tras falsa ruptura  
- **Mean reversion**: entrada si el precio vuelve hacia la línea media

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `20`  
- **ShowAverage**: `true`  
- Línea High: roja; Low: verde; Media: azul  
- Extender líneas horizontales desde el punto de cálculo para mayor claridad

✅ Muy eficaz para detectar zonas de decisión  
✅ Compatible con Delta, DOM o CVD como confirmación

---

### 🧪 Notas de desarrollo

- Calcula el canal Donchian como:
  - **High**: máximo de los últimos `Period` valores  
  - **Low**: mínimo de los últimos `Period` valores  
  - **Media**: opcional, promedio entre High y Low
- Si hay velas sin high/low, utiliza el máximo o mínimo de (Open, Close)  
- El cálculo se hace directamente sobre las velas mediante bucle descendente  
- El indicador utiliza tres series de datos (`_highSeries`, `_lowSeries`, `_averageSeries`)

---

### ❗ Incoherencias o aspectos mejorables detectados

- El bucle que calcula High/Low incluye un límite redundante:
  ```csharp
  bar - (bar < _period ? bar : _period)
