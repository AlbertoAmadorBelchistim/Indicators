---
cs_file: MarketPower.cs
name: CVD pro / Market Power
category: Order Flow
group: Order Flow
subgroup: Delta
score_current: 6/10
version: Estable
recommended_action: Reemplazar por MultiMarketPower
description: ¿Cuál es el delta acumulado (CVD) filtrado por tamaño de trade, y cómo se compara con su SMA?
gemini_summary: "Indicador redundante. Aunque es técnicamente sólido (9/10 en código), funcionalmente está obsoleto frente al 'MultiMarketPower', que hace exactamente lo mismo pero permitiendo múltiples filtros a la vez."
comparison_group: "Cumulative Delta"
competitor_notes: "Absorbido por MultiMarketPower."
reusable_code: "Lógica de SMA sobre CVD, Gestión de Gaps (RequestForCumulativeTrades)"
file_state: Estable (Redundante)
score_potential: 6/10
effort: N/A
action_priority: Bajo
analysis_date: 2025-11-19
official_code_date: 2025-04-23
---

## ⚠️ CVD pro / Market Power (6/10)

**Nombre del archivo:** [`MarketPower.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MarketPower.cs)  
**Nombre del indicador:** CVD pro / Market Power  
**Web oficial:** [ATAS — CVD pro / Market Power](https://help.atas.net/support/solutions/articles/72000602424)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es el delta acumulado (CVD) filtrado por tamaño de trade, y cómo se compara con su SMA?

![MarketPower](../../img/MarketPower.png)

---

### ⚙️ Parámetros configurables

* **SmaPeriod**: Periodo para suavizado de la línea CVD (por defecto: 14).
* **CumulativeTrades**: Activar modo de acumulación (CVD real) o tick a tick.
* **MinimumVolume / MaximumVolume**: Filtro por volumen mínimo y máximo de cada trade.
* **ShowSMA / ShowHighLow / ShowCumulative**: Opciones de visualización de SMA, extremos y acumulación.

---

### 🧭 Clasificación
**Grupo:** Order Flow
**Subgrupo:** Delta (Acumulado)

---

### 🧠 Uso más frecuente

* **(Histórico)** Visualizar el delta acumulado filtrando el ruido (ej. trades < 5 lotes).
* **(Histórico)** Detectar la tendencia de la subasta con la ayuda de la SMA.

---

### 📊 Nivel de relevancia
6️⃣ **6 / 10 (REDUNDANTE)**

✅ **Código Robusto:** Maneja correctamente la inicialización de históricos y gaps de datos.  
✅ **SMA Integrada:** A diferencia de otros, trae una media móvil del delta "de serie".  
⛔ **Limitación Crítica:** Solo permite UN filtro de volumen a la vez. Para ver "Pequeños vs Grandes", tendrías que cargar este indicador dos veces, duplicando el consumo de recursos.

---

### 🎯 Estrategias de scalping donde se aplica

* **Validación de Tendencia:** Usar la SMA de 14 periodos sobre el CVD para confirmar si la presión de compra es sostenible.
* *Nota:* Esta estrategia se ejecuta mejor en `MultiMarketPower`.

---

### ⚙️ Parametrización óptima (Si se decide usar)

* **SmaPeriod**: `14`.
* **CumulativeTrades**: `true`.
* **MinimumVolume**: `10` (Para ver solo institucional).

---

### 🧪 Notas de desarrollo

* Soporta dos modos de entrada: `OnCumulativeTrade` (agregado) y `OnNewTrade` (tick a tick).
* **Gestión de Datos:** Usa `ConcurrentQueue` (`_gapTrades`) para gestionar trades que llegan durante la inicialización histórica, evitando huecos.
* **Filtrado:** La función `IsTradeValid` maneja `_maxVolume is 0` como "sin límite".

---

### 💎 Valor Reutilizable (Código Donante)

Este indicador tiene dos joyas que le faltan al ganador (`MultiMarketPower`) y que deberíamos portar:

1.  **SMA Integrada (`_smaSeries`):**
    * *Código:* Cálculo de `_sma.Calculate(bar, _cumulativeDelta[bar])`.
    * *Por qué:* `MultiMarketPower` pinta 5 líneas pero no sus medias. Tener una SMA sobre la línea institucional (Filtro 5) sería una señal de entrada brutal (Cruce de Delta sobre su media).
2.  **Gestión de Gaps (`ConcurrentQueue`):**
    * *Código:* La lógica de `_gapTrades` y `_gapTicks`.
    * *Por qué:* Es una forma muy segura de manejar la carga de datos en tiempo real sin perder ni un tick. Vale la pena revisar si MMP la implementa igual de bien.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es un indicador "padre". Fue excelente en su día, pero ha sido superado por su "hijo", el `MultiMarketPower`.

Técnicamente es impecable (9/10), pero tácticamente es ineficiente (6/10). En el trading moderno, ver un solo flujo de delta es estar tuerto. Necesitas ver la contrapartida (Retail comprando vs Institucional vendiendo) en el mismo gráfico. `MarketPower` te obliga a elegir uno u otro.

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, pero ineficiente.**

No aporta nada que su "hermano mayor" (`MultiMarketPower`) no tenga. Mantenerlo en el gráfico solo consume recursos y espacio innecesariamente.

**Acción:** **NO USAR. Reemplazar por `MultiMarketPower`. (Portar su SMA antes de archivar).**