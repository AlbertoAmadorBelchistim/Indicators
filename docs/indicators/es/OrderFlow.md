## 🟦 Order Flow Indicator (9/10)

**Nombre del archivo:** `OrderFlow.cs`  
**Nombre del indicador:** Order Flow Indicator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602441](https://help.atas.net/support/solutions/articles/72000602441)

---

### ⚙️ Parámetros configurables

- **VisMode**: Forma de visualización (`Circles`, `Rectangles`)  
- **TradesMode**: Tipo de trade (`Cumulative`, `Separated`)  
- **Filter**: Volumen mínimo para mostrar  
- **ShowSmallTrades**: Mostrar trades con volumen inferior al filtro  
- **CombineSmallTrades**: Agrupar pequeños trades del mismo precio  
- **Size / Spacing**: Tamaño y espaciado de los objetos  
- **SpeedInterval**: Frecuencia de actualización (ms)  
- **Offset**: Separación horizontal respecto al precio actual  
- **LinkingToBar / DoNotShowAboveChart**: Ubicación en relación al gráfico  
- **Buys / Sells / BorderColor / LineColor**: Colores visuales  
- **Font / TextColor**: Fuente y color del texto  
- **UseAlerts / AlertFilter / AlertFile / AlertColor**: Sistema de alertas por volumen

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Visualización granular del flujo de órdenes con volumen y dirección

---

### 🧠 Uso más frecuente

- Visualizar **trades individuales o acumulados** con color y volumen  
- Confirmar momentos de **agresión direccional relevante**  
- Identificar **intensidad de entrada institucional o desequilibrio**

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Muy visual e intuitivo para seguir el flujo de órdenes en tiempo real  
✅ Configurable para trabajar con trades individuales o bloques acumulados  
⛔ Requiere buena configuración para evitar saturación visual

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por agresión clara** (conglomerado de puntos verdes/rojos grandes)  
- **Confirmación de ruptura** si aparecen grandes trades sobre nivel clave  
- **Filtro direccional** si hay asimetría clara en la agresión de compra o venta

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **TradesMode**: `Cumulative`  
- **Filter**: `15`  
- **CombineSmallTrades**: `true`  
- **Spacing**: `8`  
- **Size**: `12`  
- **SpeedInterval**: `300`  
- **UseAlerts**: `true`, **AlertFilter**: `30`

✅ Representación dinámica y precisa del volumen agresivo  
✅ Compatible con interpretación de absorciones o agresiones por nivel  
⛔ Puede sobrecargar la pantalla si no se filtran correctamente los trades

---

### 🧪 Notas de desarrollo

- Usa `OnNewTrade` o `OnCumulativeTrade` según configuración  
- Almacena los trades en listas (`_trades`, `_singleTrades`) con limpieza periódica  
- Dibuja visualmente objetos (`Ellipse` o `Rect`) con color, tamaño y volumen  
- Aplica alertas cuando el volumen supera el umbral definido  
- Se actualiza por temporizador interno cada `SpeedInterval`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La lógica para combinar trades pequeños en `CombineSmallTrades` depende del tipo de visualización, lo que puede ser confuso  
- En `bar == 0`, no hay validación explícita en `OnCumulativeTrade` si el objeto anterior es `null`  
- No se permite configurar el tipo de volumen mostrado (`Bid`, `Ask`, `Delta`, etc.)  
- No existe opción para mostrar líneas o niveles si se detectan clusters  
- La alerta puede activarse aunque el punto correspondiente quede fuera del área visible (`DoNotShowAboveChart` activado)

---

### 🛠️ Propuestas de mejora

- Permitir elegir tipo de volumen mostrado: `Delta`, `Bid`, `Ask`, `Total`  
- Añadir opción para resaltar visualmente zonas de cluster (acumulación en rango estrecho)  
- Incluir alertas visuales (etiquetas o flechas) junto con la sonora  
- Optimizar la lógica de combinación de pequeños trades para que funcione de forma unificada  
- Añadir suavizado visual o agrupación por precio para mejorar legibilidad en entornos densos
