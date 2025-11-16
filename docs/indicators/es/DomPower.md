## 🟦 DOM Power (8/10)

**Nombre del archivo:** `DomPower.cs`  
**Nombre del indicador:** DOM Power  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602374](https://help.atas.net/support/solutions/articles/72000602374)

---

### ⚙️ Parámetros configurables

- **LevelDepth**: Número de niveles de profundidad del DOM a considerar (por defecto: 5, desactivado por defecto)

---

### 🧭 Clasificación  
📂 OrderBook — Indicadores basados en profundidad de mercado

---

### 🧠 Uso más frecuente

- Medir la **presión relativa de órdenes límite en el libro**  
- Evaluar si hay más intención de compra o venta en los primeros niveles del DOM  
- Detectar desequilibrios latentes antes de que se ejecuten agresivamente  
- Visualizar máximos y mínimos de delta de profundidad en tiempo real

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Accede directamente a los datos de profundidad en tiempo real  
✅ Permite filtrar por número de niveles visibles en el DOM  
⛔ No tiene señal explícita, es un indicador de interpretación visual  
⛔ Puede requerir ajuste fino según instrumento o momento del mercado

---

### 🎯 Estrategias de scalping donde se aplica

- **Spoofing detectado**: presión aparente desaparece al acercarse el precio  
- **Desequilibrio de liquidez**: el delta de bids y asks anticipa movimientos  
- **Confirmación pasiva**: entrada sólo si el DOM respalda la dirección del flujo agresivo

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **LevelDepth**: `5` o `10` según profundidad media del instrumento  
- Usar junto a indicadores de agresión (delta, CVD, imbalances)  
- Activar visualización en panel propio con colores diferenciados para bids y asks

✅ Detecta presión real de liquidez antes de que ocurra la ejecución  
✅ Compatible con estrategias de absorción, empuje o agotamiento

---

### 🧪 Notas de desarrollo

- El indicador accede directamente al **MarketDepthInfo** para obtener snapshots del libro  
- Si el filtro está activado (`LevelDepth.Enabled`), usa `SortedList<decimal, decimal>` para mantener los `n` primeros niveles  
- Calcula:
  - `_asks[bar] = -cumAsks` (valor negativo)  
  - `_bids[bar] = cumBids`  
  - `delta = cumBids - cumAsks`  
  - `_maxDelta` y `_minDelta` se actualizan si el nuevo delta supera valores anteriores  
- Controla visualmente el valor acumulado de cada barra, actualizándose en cada tick mediante `MarketDepthChanged`

---

### ❗ Incoherencias o aspectos mejorables detectados

- En `MarketDepthChanged`, si `LevelDepth.Enabled` y hay menos niveles de los requeridos, **se ignora el filtro** y se usa `CumulativeDomAsks` / `CumulativeDomBids`, lo que puede generar valores inconsistentes entre barras  
- En `OnCalculate`, el código vuelve a asignar valores de barras anteriores aunque no hayan cambiado, **duplicando procesamiento innecesariamente**  
- El uso de `_isLastDeltaCalc` y `lastCandle` permite solo **una actualización por barra**, lo cual puede ocultar movimientos intermedios si hay múltiples actualizaciones de DOM entre dos velas

---

### 🛠️ Propuestas de mejora

- Añadir opción para limitar la frecuencia de actualización (por ejemplo, una vez por tick o por intervalo de tiempo)  
- Mostrar delta relativo acumulado en porcentaje (`(bids - asks)/(bids + asks)`)  
- Incluir **alertas visuales o etiquetas** cuando el delta supere cierto umbral  
- Permitir visualizar `maxDelta` y `minDelta` como líneas adicionales o bandas
