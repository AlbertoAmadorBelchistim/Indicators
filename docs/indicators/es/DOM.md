## 🟦 Depth of Market (DOM) (9/10)

**Nombre del archivo:** `DOM.cs`  
**Nombre del indicador:** Depth of Market  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602367](https://help.atas.net/support/solutions/articles/72000602367)

---

### ⚙️ Parámetros configurables

- **VisualMode**: Modo de visualización (`Common`, `Cumulative`, `Combined`)  
- **UseAutoSize / Width / RightToLeft / ProportionVolume**: Ajustes de escala y orientación  
- **BidRows / AskRows / TextColor / Backgrounds**: Colores personalizables por tipo de nivel  
- **FilterColors**: Filtros por volumen con color asignado  
- **ShowCumulativeValues**: Mostrar volumen total acumulado  
- **PriceLevelsHeight / Scale / UseScale / CustomScale**: Control visual de la escala y resolución de niveles

---

### 🧭 Clasificación  
📂 OrderBook — Indicadores de profundidad de mercado (nivel 2)

---

### 🧠 Uso más frecuente

- Visualizar el **libro de órdenes (bid/ask)** en el gráfico  
- Detectar **niveles con gran liquidez** (muros) o desequilibrio de órdenes  
- Evaluar volumen acumulado por nivel de precio (modo `Cumulative`)  
- Identificar zonas de absorción o spoofing mediante colores o filtros

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Ofrece lectura directa del DOM en el gráfico  
✅ Altamente personalizable con múltiples modos y escalas  
⛔ Complejo de configurar sin conocimiento previo  
⛔ Puede consumir recursos si el DOM es muy profundo y hay muchos filtros activos

---

### 🎯 Estrategias de scalping donde se aplica

- **Detección de muros**: operar reversiones si hay volúmenes extremos en niveles visibles  
- **Spoofing**: detectar desaparición de niveles con volumen ficticio  
- **Confirmación**: validar rupturas si desaparecen órdenes limitadas o si se concentra volumen en favor del precio  
- **Reversión o breakout con confluencia**: entrada cuando el DOM y el order flow coinciden en dirección

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **VisualMode**: `Combined` o `Cumulative`  
- **Width**: `150`  
- **FilterColors**: asignar colores a volúmenes ≥ 300, 1000, 2500, etc.  
- **ShowCumulativeValues**: `true`  
- **UseAutoSize**: `true`  
- **Scale**: `20`

✅ Lectura rápida de presión visible por nivel de precio  
✅ Complemento visual clave para decisiones tácticas

---

### 🧪 Notas de desarrollo

- Obtiene snapshot del DOM usando `MarketDepthInfo.GetMarketDepthSnapshot()`  
- Los niveles se representan como **barras horizontales de color y texto** según volumen  
- Si el modo es `Cumulative`, se calcula el volumen acumulado hacia arriba (asks) y abajo (bids)  
- Las dimensiones de cada bloque se escalan con el volumen y el máximo visible  
- Se pueden aplicar filtros por volumen (`FilterColors`) para colorear niveles según su tamaño  
- Incluye visualización de volumen total acumulado (`DrawCumulativeValues`)  
- El dibujo se realiza directamente en `OnRender()` sobre el gráfico, no en paneles

---

### ❗ Incoherencias o aspectos mejorables detectados

- No hay opción de limitar la profundidad visualizada (ej. 10 niveles) para aligerar gráficos  
- El filtro por volumen solo colorea, pero **no ordena ni prioriza visualmente** los niveles más significativos  
- El valor de escala (`Scale`) no siempre se adapta bien a movimientos rápidos si `UseAutoSize = false`  
- Al desactivarse `UseScale`, la lógica de `_upScale` y `_downScale` se desactiva sin fallback visual

---

### 🛠️ Propuestas de mejora

- Añadir opción para **mostrar solo los N primeros niveles** por lado  
- Implementar alertas visuales o sonoras si aparece volumen en un nivel por encima de cierto umbral  
- Añadir etiquetas flotantes con el volumen exacto acumulado por bloque  
- Permitir un modo “resumen” que agrupe niveles por bloques (ej. cada 4 ticks)