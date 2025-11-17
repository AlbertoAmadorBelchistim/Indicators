## 🟦 MACD (Moving Average Convergence Divergence) (8/10)

**Nombre del archivo:** `MACD.cs`  
**Nombre del indicador:** MACD  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602418](https://help.atas.net/support/solutions/articles/72000602418)

---

### ⚙️ Parámetros configurables

- **ShortPeriod**: Periodo de la media móvil exponencial rápida (por defecto: 12)  
- **LongPeriod**: Periodo de la media móvil exponencial lenta (por defecto: 26)  
- **SignalPeriod**: Periodo de la media móvil de la línea MACD (por defecto: 9)

---

### 🧭 Clasificación
📂 Momentum — Indicador clásico de convergencia/divergencia de medias móviles

---

### 🧠 Uso más frecuente

- Detectar **momentos de cambio en la tendencia** o aceleraciones en el precio  
- Identificar **cruces entre MACD y su línea de señal** como posibles entradas/salidas  
- Analizar **divergencias** entre MACD y acción del precio

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Muy utilizado en análisis técnico clásico y sistemas algorítmicos  
✅ Reacciona tanto a momentum como a la dirección del precio  
⛔ Puede generar señales falsas en mercados laterales

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce de MACD con su línea de señal** como entrada  
- **Confirmación de dirección** cuando la diferencia entre MACD y señal es creciente  
- **Entrada por divergencia** si el precio marca un mínimo más bajo y el MACD no

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ShortPeriod**: `6`  
- **LongPeriod**: `19`  
- **SignalPeriod**: `4`

✅ Permite captar movimientos cortos y rápidos en scalping  
✅ El histograma ayuda a visualizar la aceleración  
⛔ Necesita filtro direccional o contexto para evitar entradas erróneas

---

### 🧪 Notas de desarrollo

- Implementa la fórmula clásica del MACD:  
  `MACD = EMA(Short) - EMA(Long)`  
  `Signal = EMA(MACD)`  
  `Histograma = MACD - Signal`  
- Representa tres series: línea MACD, señal (EMA de MACD) y la diferencia como histograma  
- Las medias se recalculan internamente en cada barra mediante objetos `EMA`  
- La visualización del histograma es de tipo `VisualMode.Histogram`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No permite seleccionar el tipo de media móvil (solo usa `EMA`)  
- No hay validación para evitar que `ShortPeriod > LongPeriod` dé lugar a comportamientos erráticos  
- No incluye alertas visuales o sonoras ante cruces entre MACD y la señal  
- La dirección del histograma no se codifica por color (positivo/negativo), lo que dificultaría lectura rápida  
- No expone valores del cruce ni pendiente como posibles series auxiliares

---

### 🛠️ Propuestas de mejora

- Añadir validación para asegurar que `ShortPeriod < LongPeriod`  
- Permitir seleccionar tipo de media (EMA, SMA, SMMA) para más flexibilidad  
- Colorear el histograma según dirección (positivo = verde, negativo = rojo, por ejemplo)  
- Incluir alertas opcionales ante cruces de MACD y señal  
- Añadir opción de líneas horizontales en 0 para referencia visual clara

