## 🟦 CVD pro / Market Power (9/10)

**Nombre del archivo:** `MarketPower.cs`  
**Nombre del indicador:** CVD pro / Market Power  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602424](https://help.atas.net/support/solutions/articles/72000602424)

---

### ⚙️ Parámetros configurables

- **SmaPeriod**: Periodo para suavizado de la línea CVD (por defecto: 14)  
- **CumulativeTrades**: Activar modo de acumulación (CVD real) o tick a tick  
- **MinimumVolume / MaximumVolume**: Filtro por volumen mínimo y máximo de cada trade  
- **ShowSMA / ShowHighLow / ShowCumulative**: Opciones de visualización de SMA, extremos y acumulación  
- **LineColor / HighLowColor / SmaColor / Width**: Configuración de colores y grosor

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Indicador delta acumulado (CVD) con filtros y visualización mejorada

---

### 🧠 Uso más frecuente

- Visualizar el **delta acumulado** o por barra con control avanzado  
- Detectar zonas de presión agresiva de compra o venta  
- Identificar divergencias delta/precio o aceleraciones de volumen agresivo

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Altamente configurable y potente para análisis de flujo de órdenes  
✅ Permite trabajar tanto con trades acumulados como tick a tick  
⛔ Exige interpretación precisa y buen filtrado para evitar ruido

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por divergencia delta/precio** detectando acumulación silenciosa  
- **Confirmación de intención** con rupturas acompañadas de CVD creciente  
- **Detección de agotamiento** si el precio sube pero el delta no acompaña

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **SmaPeriod**: `14`  
- **CumulativeTrades**: `true`  
- **MinimumVolume**: `10`  
- **MaximumVolume**: `0` (sin límite)  
- **ShowCumulative**: `true`  
- **ShowSMA / ShowHighLow**: activados

✅ Compatible con análisis de absorciones, desequilibrios y trampas de liquidez  
✅ Aporta contexto dinámico sobre la agresividad real  
⛔ Su visualización puede ser densa si no se configura correctamente

---

### 🧪 Notas de desarrollo

- Soporta dos modos de entrada: `OnCumulativeTrade` (agregado) y `OnNewTrade` (tick a tick)  
- Se puede cambiar entre delta acumulado o delta por barra con histogramas  
- Aplica filtros por volumen y traza líneas de máximos/mínimos del delta  
- Calcula SMA sobre CVD acumulado para análisis de tendencia del delta  
- Usa colas (`ConcurrentQueue`) para gestionar trades mientras se inicializa el indicador

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La lógica de validación de volumen (`IsTradeValid`) no contempla `MaximumVolume = 0` como “sin límite” explícitamente documentado  
- La representación del delta por barra (`_barDelta`) y el CVD acumulado pueden solaparse sin indicación clara al usuario  
- Las líneas `_higher` y `_lower` actúan como histogramas o líneas según el modo, pero esto no se indica en la UI  
- No hay alertas ni coloración dinámica según la pendiente del delta o su cruce con la SMA  
- La lógica de cambio de sesión depende de `IsNewSession` pero no está claramente desacoplada de `OnFinishRecalculate`, lo que podría generar errores en replay o carga parcial

---

### 🛠️ Propuestas de mejora

- Añadir codificación de color para histogramas y líneas según dirección o fuerza del delta  
- Ofrecer alertas visuales o sonoras al cruce del CVD con su media o niveles clave  
- Mostrar tooltips o etiquetas con los valores del CVD, HighLow y SMA  
- Separar visualmente las líneas `_barDelta` y `_cumulativeDelta` en la UI  
- Documentar en la interfaz los distintos modos (acumulativo vs tick a tick) con ayuda contextual

