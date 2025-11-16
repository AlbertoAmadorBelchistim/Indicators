## 🟦 DOM Strength (9/10)

**Nombre del archivo:** `DomStrength.cs`  
**Nombre del indicador:** DOM Strength  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602375](https://help.atas.net/support/solutions/articles/72000602375)

---

### ⚙️ Parámetros configurables

- **Period**: Ventana de tiempo para acumular volumen de trades (por defecto: 5)  
- **Percent**: Valor base para normalizar los ratios (por defecto: 50)  
- **LevelDepth**: Número de niveles del DOM a considerar (por defecto: 10)  
- **ShowDelta**: Mostrar o no las velas de delta superpuestas  
- **Color20 / 50 / 80 / -20 / -50 / -80**: Colores para codificar visualmente los distintos rangos de fuerza

---

### 🧭 Clasificación  
📂 OrderBook — Indicadores de desequilibrio entre profundidad y ejecución

---

### 🧠 Uso más frecuente

- Medir la **fuerza relativa de compras y ventas activas respecto al DOM pasivo**  
- Detectar **momentos de presión agresiva real** sobre niveles visibles  
- Visualizar de forma compacta si los compradores o vendedores están absorbiendo liquidez

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Visualmente intuitivo y útil para lectura rápida  
✅ Combina datos del DOM y del flujo real de órdenes  
⛔ Requiere interpretación contextual; los valores por sí solos no son señales  
⛔ Puede consumir recursos si se usan periodos largos y muchos trades acumulados

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión con presión visible**: entrada si hay más agresión que liquidez  
- **Breakout confirmado**: si el ratio de compra supera umbral positivo justo antes de romper  
- **Absorción**: si hay volumen comprador pero se mantiene el muro de asks

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `3` a `5`  
- **Percent**: `50`  
- **LevelDepth**: `10`  
- **Colores**: mantener verde para fuerza compradora y rojo para vendedora  
- **ShowDelta**: `false` si ya hay otros indicadores de delta activos

✅ Aporta contexto invisible en el gráfico tradicional  
✅ Compatible con DOM Power, Cluster y Delta para confluencia

---

### 🧪 Notas de desarrollo

- El indicador compara volumen ejecutado (`_trades`) con volumen visible del DOM (DOM snapshot + profundidad acumulada)  
- Calcula dos razones:
  - **BuyRatio = BuyVolume / CumulativeAsks**  
  - **SellRatio = SellVolume / CumulativeBids**  
- Resta al resultado el valor de referencia (`Percent`) para centrar en 0  
- Muestra barras horizontales codificadas por color en la parte superior (compras) e inferior (ventas)  
- Si `ShowDelta = true`, muestra velas delta en el mismo panel usando `CumulativeDelta`

---

### ❗ Incoherencias o aspectos mejorables detectados

- En `CalcCumulativeDepth()`, el cálculo de los valores acumulados **usa bucles hasta `i <= LevelDepth.Value`**, lo cual incluye un nivel adicional (off-by-one)  
- No se valida si `_mDepthBid.Values.Count > LevelDepth.Value` antes de acceder al índice `lastIdx - i`, lo que podría lanzar excepción si hay pocos niveles  
- El panel depende de `OnCumulativeTradesResponse`, lo que implica que puede **no inicializarse correctamente en tiempo real si no hay ticks acumulados previos**

---

### 🛠️ Propuestas de mejora

- Corregir el uso de `i <= LevelDepth.Value` por `i < LevelDepth.Value` para evitar incluir un nivel adicional no deseado  
- Añadir protección contra desbordamientos de índices en `_mDepthBid` y `_mDepthAsk`  
- Permitir activar una línea base de referencia (eje cero) visible  
- Añadir una opción para activar alertas si los valores de fuerza superan ciertos umbrales (ej. +80 o -80)
