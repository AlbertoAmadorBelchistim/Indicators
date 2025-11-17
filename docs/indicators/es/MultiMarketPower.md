## 🟦 CVD pro(multi) / Multi Market Powers (10/10)

**Nombre del archivo:** `MultiMarketPower.cs`  
**Nombre del indicador:** CVD pro(multi) / Multi Market Powers  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602434](https://help.atas.net/support/solutions/articles/72000602434)

---

### ⚙️ Parámetros configurables

- **CumulativeTrades**: Acumulación por trade (`true`) o tick a tick (`false`)  
- **UseFilter1–5**: Activar o desactivar cada filtro de volumen  
- **MinVolume / MaxVolume** (para cada filtro): Rango de volumen por trade a incluir  
- **Color / LineWidth** (por filtro): Personalización visual de las líneas  
- Visualización simultánea de hasta 5 series distintas, cada una con su lógica de volumen

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Delta acumulado segmentado por filtros de volumen (Big Trades)

---

### 🧠 Uso más frecuente

- Analizar la **acumulación de delta por bloques de tamaño**  
- Detectar **presencia institucional** mediante volumen clasificado  
- Confirmar trampas, absorciones o desequilibrios por rango de volumen

---

### 📊 Nivel de relevancia
🔟 **10 / 10**

✅ Herramienta de análisis avanzada con control fino por tamaño de trade  
✅ Visualización separada de cada segmento de volumen  
⛔ Requiere configuración adecuada de filtros y comprensión del order flow

---

### 🎯 Estrategias de scalping donde se aplica

- **Detección de absorciones** por gran volumen (filtro 4–5)  
- **Validación de agresión retail** en rangos bajos (filtro 1–2)  
- **Acumulación silenciosa** si el delta crece en filtros medios sin movimiento en precio

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **CumulativeTrades**: `true`  
- **Filtro1**: `1–5` → micro trades  
- **Filtro2**: `6–10` → retail  
- **Filtro3**: `11–20` → operadores medianos  
- **Filtro4**: `21–40` → institucional ligero  
- **Filtro5**: `41+` → institucional pesado

✅ Permite distinguir la participación de distintos actores  
✅ Proporciona visión clara y directa del delta segmentado  
⛔ Complejo de interpretar si se activan todos sin filtro visual adecuado

---

### 🧪 Notas de desarrollo

- Se basa en `CumulativeTrade` o `MarketDataArg` según el modo  
- Cada trade se evalúa por su volumen y se asigna al filtro correspondiente  
- Se mantiene un delta separado por filtro, que se acumula y se traza como línea  
- Incluye soporte completo para carga histórica y actualización en tiempo real  
- Soporta hasta 5 líneas activas simultáneamente con sus propios colores y grosores

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La variable `_lastTrade` puede provocar errores si se intenta clonar en `null`  
- No hay control si `MinVolume > MaxVolume` → produce filtros vacíos silenciosamente  
- Las series son visibles aunque no se hayan activado explícitamente desde la UI  
- No incluye lógica de alertas ni visualización de etiquetas o tooltips  
- El código tiene múltiples rutas duplicadas en `CalculateTrade` y `CalculateBarTrades` que podrían unificarse

---

### 🛠️ Propuestas de mejora

- Añadir validación cruzada entre `MinVolume` y `MaxVolume` en cada filtro  
- Implementar tooltips o etiquetas flotantes con el valor de delta en cada punto  
- Añadir alertas visuales o sonoras cuando un filtro supera un umbral configurado  
- Refactorizar y consolidar lógica común entre métodos de cálculo  
- Permitir alternar entre modo línea y modo histograma para cada filtro

