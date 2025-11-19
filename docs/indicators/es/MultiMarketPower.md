---
cs_file: MultiMarketPower.cs
name: CVD pro(multi) / Multi Market Powers
category: Order Flow
group: Order Flow
subgroup: Delta
score_current: 10/10
version: Latest
recommended_action: Conservar
description: ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional)?
gemini_summary: "El Santo Grial del Delta Acumulado. Supera a todos los demás CVDs porque permite desglosar la agresión en 5 capas simultáneas (ej. <1 contrato vs >50 contratos) en un solo panel. Código robusto y optimizado."
comparison_group: "Cumulative Delta"
competitor_notes: "Vence a 'Market Power' y 'Cumulative Delta' por su capacidad de filtrado múltiple sin perder rendimiento."
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-19
official_code_date: 2025-08-14
---

## 🏆 CVD pro(multi) / Multi Market Powers (10/10)

**Nombre del archivo:** [`MultiMarketPower.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MultiMarketPower.cs)  
**Nombre del indicador:** CVD pro(multi) / Multi Market Powers  
**Web oficial:** [ATAS — CVD pro(multi) / Multi Market Powers](https://help.atas.net/support/solutions/articles/72000602434)  
**Compatibilidad:** ATAS versión latest y superiores.  
**Última revisión del código oficial:** 14/08/2025  

> **La Pregunta Clave:** ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional)?

![MultiMarketPower](../../img/MultiMarketPower.png)

---

### ⚙️ Parámetros configurables

El indicador calcula **simultáneamente 5 líneas de Delta** independientes. No hay un interruptor "On/Off" por filtro en la lógica; se activan o desactivan visualmente desde la pestaña de Colores/Visualización o estableciendo volúmenes inalcanzables.

* **CumulativeTrades**:
    * `True`: Acumula el delta trade a trade (recomendado para ver la tendencia de la sesión).
    * `False`: Muestra el delta neto reseteado en cada vela (modo tick a tick).
* **Filters Settings (1 a 5):** Cada uno de los 5 filtros tiene su propia configuración independiente:
    * **MinimumVolume**: Volumen mínimo del trade para entrar en este filtro.
    * **MaximumVolume**: Volumen máximo. **Nota:** Si se establece en `0`, el indicador lo interpreta como "Sin Límite" (infinito).
* **Visuals (Por serie):**
    * Cada filtro corresponde a una serie (`Filter1Series`...`Filter5Series`) donde se puede personalizar **Color**, **Grosor** y **Visibilidad**.

---

### 🧭 Clasificación
**Grupo:** Order Flow
**Subgrupo:** Delta (Acumulado)

---

### 🧠 Uso más frecuente

* **Desglose de Participantes:** Diferenciar visualmente si la subida del precio la están provocando los pequeños traders (retail, filtro 1) o los grandes bloques (institucional, filtro 5).
* **Divergencias Internas:** Identificar situaciones donde el delta retail compra (FOMO) mientras el delta institucional vende o se aplana (distribución).
* **Limpieza de Ruido:** Ocultar visualmente el Filtro 1 (<1 contrato) para ver la "verdadera" tendencia de la subasta sin el ruido de los algoritmos HFT de bajo volumen.

---

### 📊 Nivel de relevancia
🔟 **10 / 10 (IMPRESCINDIBLE)**

✅ **Superioridad Técnica:** Consolida la función de 5 indicadores en uno solo sin penalizar el rendimiento de ATAS.  
✅ **Claridad Táctica:** Permite ver la "anatomía" de la vela desglosada por participantes en un solo vistazo.  
✅ **Versatilidad:** Funciona tanto para scalping de micro-estructura (M1) como para análisis de sesión intradía.

---

### 🎯 Estrategias de scalping donde se aplica

* **Detección de Absorción Institucional:** Cuando el Filtro 5 (Institucional) muestra delta plano o divergente mientras el precio hace nuevos máximos (absorción pasiva de las compras retail).
* **Validación de Rupturas (Breakouts):** Una ruptura de nivel solo es válida si viene acompañada de un impulso fuerte en los Filtros 4 y 5. Si solo sube el Filtro 1 y 2, es una trampa (Fakeout).
* **Giro por Agotamiento:** Cuando todos los filtros se alinean en una dirección extrema y de repente el Filtro 1 (Retail) sigue empujando pero los Filtros 4-5 giran, anticipando una reversión inminente.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **CumulativeTrades**: `true` (Visión de sesión)
* **Filtro 1 (Retail / HFT):** Min `0` - Max `5` (Ruido y pequeños especuladores)
* **Filtro 2 (Small Traders):** Min `6` - Max `20` (Traders activos)
* **Filtro 3 (Medium):** Min `21` - Max `50` (Locales y pequeños fondos)
* **Filtro 4 (Large):** Min `51` - Max `100` (Institucional táctico)
* **Filtro 5 (Whales):** Min `101` - Max `0` (Institucional pesado / Bloques)

---

### 🧪 Notas de desarrollo

* **Arquitectura Paralela:** El código mantiene 5 variables de estado independientes (`_delta1` a `_delta5`) que se actualizan en cada tick o trade acumulado.
* **Lógica de Filtrado:** Utiliza una estructura condicional robusta en el bucle de procesamiento de trades (`GetCumulativeTrades`). La condición `_maxVolume == 0` se maneja explícitamente como infinito (`|| _maxVolume == 0`), evitando errores de desbordamiento.
* **Gestión de Datos:** Al igual que su predecesor, solicita datos históricos de trades para construir el CVD desde el inicio de la sesión sin necesidad de tener el gráfico abierto previamente.
* **Eficiencia:** A pesar de procesar 5 flujos, solo itera sobre la lista de trades una vez por actualización, asignando cada trade a su "cubo" correspondiente, lo que lo hace muy eficiente en CPU.

---

### ✍️ La opinión de Gemini sobre el Indicador

Este indicador es el **Santo Grial** del análisis de Delta en ATAS.

Mientras que otros indicadores te dicen *cuánto* se ha comprado, este te dice *quién* lo ha hecho. En el ecosistema actual del S&P 500, donde el volumen retail y el institucional a menudo van en direcciones opuestas (o con tiempos distintos), esta información es la ventaja estadística (Edge) que buscamos.

Su implementación técnica es impecable: limpia, eficiente y sin tonterías. Hace obsoleto el uso de múltiples instancias de "Cumulative Delta" o "Market Power".

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, es la herramienta principal (Core) de Delta.**

Sustituye a cualquier otro CVD. Debe estar presente en cualquier plantilla de Order Flow seria.

**Acción:** **Conservar y Priorizar.**