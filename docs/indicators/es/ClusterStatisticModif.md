---
cs_file: ClusterStatisticModif.cs
name: Cluster Statistic Modif
category: Order Flow
group: Order Flow
subgroup: Footprint
score_current: 10/10
version: Estable
recommended_action: Conservar
description: ¿Cuál es el 'dashboard' estadístico completo de cada vela?
gemini_summary: "Obra maestra. Dashboard profesional que combina estadísticas base con métricas avanzadas (Speed of Tape, Imbalances). Herramienta central."
comparison_group: "Cluster Analysis"
competitor_notes: "Sin competencia."
reusable_code: null
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-17
official_code_date: 23/04/2025
user_modification_date: 01/10/2025
---

## 🟦 Cluster Statistic (versión modificada)

- **Nombre del archivo:** [ClusterStatisticModif.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/ClusterStatisticModif.cs) 
- **Nombre del indicador:** Cluster Statistic Modif 
- **Web oficial:** [ATAS — Cluster Statistic](https://help.atas.net/en/support/solutions/articles/72000602624-cluster-statistics)  
- **Compatibilidad:** ATAS versión estable y superiores.
- **Última revisión del código base:**  [`ClusterStatistic.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ClusterStatistic.cs): 23/4/2025  
- **Última revisión del código modificado:** 1/10/2025 (v 1.1.0)
*(Versión extendida y optimizada por Alberto Amador Belchistim)*

![Tabla completa de estadísticas](../../img/ClusterStatistics.png)

> **La Pregunta Clave:** ¿Cuál es el "dashboard" estadístico completo (Volumen, Delta, Ticks, *Velocidad* e *Imbalances*) de cada vela, y cómo se compara cada vela con la más "fuerte" del gráfico?

---

### ⚙️ Parámetros configurables

![Configuración de las filas](../../img/ClusterStatisticRawsConfig.png)

#### 🧱 Filas
Selección de las filas de datos que se muestran en la tabla de estadísticas del clúster.

- **Cantidad de operaciones** – número total de trades por vela.  
- **Altura** – altura vertical de la vela (en ticks).  
- **Tiempo** – instante inicial de la vela.  
- **Duración de la vela (seg.)** – duración total en segundos.  
- **Volumen** – volumen total negociado en la vela.  
- **Volumen/segundo** – velocidad de ejecución media (volumen por segundo).  
- **Mostrar asks / bids** – muestra los volúmenes de ask y bid por separado.  
- **Delta** – diferencia entre volumen agresivo de compra y venta.  
- **Delta/seg.** – delta por segundo medio (velocidad del flujo).  
- **Mostrar Delta/Volumen** – ratio de delta normalizado respecto al volumen total.  
- **Delta máximo / mínimo** – valores extremos de delta entre los clusters de la vela.  
- **Cambio de Delta** – variación de delta respecto a la vela anterior.  
- **Max Vol/seg (peak)** – pico máximo de volumen por segundo dentro de la vela.  
- **Delta at peak** – delta en el momento del pico de volumen.  
- **Delta/Vol at peak** – relación delta/volumen en el pico de actividad.  
- **Buy / Sell / Net Imbalances** – desequilibrios por lado (compra, venta y diferencia). 
- **Stacked Imbalances** – Desequilibrios en clústeres consecutivos. 
- **Volumen / Delta / Delta/volumen de sesión** – valores acumulados desde el inicio de la sesión.

---

#### ⚡ Max vol/seg
Parámetros del cálculo de velocidad pico (volumen por segundo).

![Configuración de la velocidad pico](../../img/ClusterStatisticPeakConfig.png)

- **Ventana temporal (seg.)** – tamaño de la ventana móvil usada para el cálculo. <br>
En el ejemplo compararía el volumen acumulado en cada intervalo de 5 segundos dentro de la vela.
- **Volumen mínimo por ventana** – volumen mínimo durante la ventana para tenerla en cuenta en el cálculo.  
- **Usar filtro automático** – los colores de la tabla se ajustan dinámicamente según un filtro basado en la media de los últimos valores. 
- **Periodo del filtro automático** – número de velas usadas para el filtro.  
- **Filtro automático = EMA (off = SMA)** – tipo de media móvil empleada (EMA o SMA).

---

#### ⚖️ Imbalance
Control de los umbrales de desequilibrio y filtrado.

![Configuración de los imbalances](../../img/ClusterStatisticImbalanceConfig.png)

- **Umbral de desequilibrio (%)** – porcentaje de diferencia mínima entre el ask de un nivel y el bid del inferior para resaltar un desequilibrio.  
- **Filtro de volumen de desequilibrio** – volumen mínimo necesario para tener en cuenta el desequilibrio.

---

#### 🎨 Visualización
Ajustes generales de apariencia del panel.

![Configuración de la visualización](../../img/ClusterStatisticVisualizationConfig.png)

- **Fondo** – color de fondo del panel.  
- **Transparencia** – opacidad del fondo (0–255).  
- **1. Cuadrícula** – color de las líneas de cuadrícula.  
- **Mostrar por degradado** – activa el sombreado por degradado de color en las distintas celdas.  
- **Color de volumen / Ask / Bid** – colores base para cada tipo de dato.

---

#### ✏️ Texto

![Configuración del texto](../../img/ClusterStatisticVisualizationConfig.png)
- **Color** – color del texto.  
- **Tipo de letra** – fuente y tamaño (por ejemplo, Arial 9 px).  
- **Alineación centrada** – centra el texto dentro de cada celda.  
- **Ratios como porcentaje** – muestra los ratios en formato de porcentaje.

---

#### 🧩 Encabezados
![Configuración de los encabezados](../../img/ClusterStatisticHeadersConfig.png)
- **Color** – color de fondo del encabezado.  
- **Ocultar encabezamientos** – oculta la columna de títulos de la tabla.

---

#### 🔔 Alertas de volumen / Delta / Net Imbalance
![Configuración de las alertas](../../img/ClusterStatisticAlertsConfig.png)
- **Habilitado** – activa la alerta.  
- **Filtro** – valor de umbral para activar la alerta.  
- **Archivo de alarma** – nombre del archivo de sonido (por ejemplo, `alert1`).
- **Usar vela cerrada** (sólo en Net Imbalance)– evalúa la condición únicamente al cierre de la vela.  

---

#### ⚙️ Misc.
- **Mostrar descripción** – muestra un texto descriptivo interno debajo del panel.


---

### 🧭 Clasificación  
📂 VolumeOrderFlow / Dashboard — Dashboard de estadísticas avanzadas de vela (Volumen, Delta, Velocidad e Imbalance).

---

### 🧠 Uso más frecuente

  * Mostrar un **resumen por vela** de variables clave (Volumen, Delta, Ticks).
  * Detectar **picos de velocidad** (`Max Vol/sec`) para identificar ignición, pánico o clímax.
  * Cuantificar la **agresión** (`Delta/seg`, `Delta/Vol at peak`).
  * Contar **desequilibrios** (`Net Imbalances`, `Stacked Imbalances`) para confirmar la presión institucional.

---

### ✨ Mejoras introducidas en la versión por Alberto Amador Belchistim

Esta versión modificada conserva la base del indicador original pero añade métricas avanzadas orientadas al scalping profesional.

Se han incorporado las siguientes mejoras respecto a la versión oficial:

**Visuales / UI**

- Reordenación de campos de la interfaz para mejorar legibilidad de métricas y facilitar interpretación rápida.  
- Mejora del contraste y diferenciación de color para métricas clave para facilitar identificación de señales relevantes.

**Funcionales / métricas adicionales**

- Añadido cálculo de **Delta por segundo (Delta/sec)** para medir agresión neta en función del tiempo de vela. 
- Introducción de series “PeakVolPerSec” y “PeakDeltaPerSec” para identificar picos de velocidad máxima dentro de la vela.
- Añadido “PeakDeltaPerVol” (Delta/Vol en el momento del máximo volumen/segundo) para evaluar eficiencia de impulso frente a volumen. 
- Integración de filtros por umbral basado en media (media móvil/exponencial) para métricas de pico de velocidad, permitiendo destacar eventos excepcionales en función del contexto histórico reciente.  
- Implementación de imbalances de huella (buy/sell/net) con filtro de volumen y opción de alerta cuando el desequilibrio supera umbral configurado.
- Corrección de cálculo de acumulación máxima de Bid (`maxBid`) y de `_maxDeltaPerVol` en actualizaciones de vela para mejorar precisión del resumen.

**Valor añadido práctico**

- Permite al trader ver **no solo qué se acumuló**, sino **cómo de rápido** (volumen/delta por segundo), lo cual mejora la detección de impulsos reales frente a ruido.  
- Las métricas pico (Vol/sec, Delta/sec) permiten **anticipar rupturas** o confirmarlas antes que el volumen total sea evidente.  
- Los imbalances ayudan a detectar **actividad institucional o desequilibrio significativo** en tiempo real.  
- La mejora visual hace que el panel sea **más legible** en marcos rápidos (scalping) y en sesiones donde el espacio gráfico es limitado.

---

### 📊 Nivel de relevancia  
🔟 **10 / 10**

✅ **Dashboard Definitivo:** Combina las estadísticas de `CandleStatistics` (8/10) con métricas avanzadas de *velocidad* (SoT) e *imbalance* (Footprint), todo en una tabla interactiva.  
✅ **Métricas de Velocidad:** Añadir `Max Vol/sec` y `Delta at peak` es una mejora de nivel profesional que aporta un contexto que el volumen por sí solo no puede.  
⛔ **Sobrecarga de Información:** Es el indicador más denso en datos. Requiere que el trader sepa exactamente qué filas activar y cuáles ocultar.

---

### 🎯 Estrategias de scalping donde se aplica

  * **Ruptura con Convicción (Ignición):** Buscar una vela de ruptura que muestre: `Volume` alto + `Delta` alto + `Net Imbalances` alto + `Max Vol/sec` alto + `Delta at peak` positivo.
  * **Absorción en S/R:** Buscar una vela en un nivel clave que muestre: `Volume` alto + `Delta` cercano a cero (o contrario) + `Net Imbalances` opuestos (ej. muchos Buy Imbalances en una resistencia que no rompe).
  * **Clímax de Agotamiento:** Una vela con `Volume` extremo, `Height` extrema, y un `Max Vol/sec` muy alto, pero un `Delta` que no acompaña (divergencia).

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| **Parámetro** | **Valor recomendado** | **Comentario** |
| :--- | :--- | :--- |
| **Filas Activas (Rows)** | `Delta`, `Delta/seg`, `Delta/Vol`, `Delta at peak`, `Net Imb`, `Stacked Imb` | Foco en Delta, Velocidad y Desequilibrio. |
| **Time Window (sec)** | `5` | Ventana de 5 segundos para medir la velocidad. |
| **Min Volume per Window** | `150` | Filtro de ruido para la velocidad. |
| **Use Auto Filter** | `true` | Heatmap adaptativo para picos de velocidad. |
| **Auto Filter Period** | `3` (EMA) | Filtro muy rápido. |
| **Imbalance Threshold** | `300` (300%) | Ratio estándar 3:1. |
| **Imbalance Volume Filter**| `30` | Filtro de ruido para imbalances. |
| **VisibleProportion** | `true` | **¡Clave\!** Normaliza el heatmap a la sesión visible. |
| **Ratios as percent** | `true` | Más fácil de leer ("30%" vs "0.30"). |

✅ Esta configuración ofrece una lectura equilibrada entre **intensidad del flujo** y **desequilibrio neto**, permitiendo detectar fácilmente:
- Cambios bruscos de delta intrabarra (fatiga o absorción).  
- Zonas con *stacked imbalance* o *net imbalance* dominante.  
- Momentos de aceleración del flujo (Delta/sec o Delta/Vol at peak).  

⛔ Evita sobrecargar la tabla con métricas redundantes (por ejemplo, Volume o Max/Min Delta) para mantener claridad visual en escalas de 1M.  


---

### 🧪 Notas de desarrollo

  * Es un indicador de **dibujo 100% personalizado** (`OnRender`) que dibuja una tabla interactiva (filas arrastrables).
  * **Cálculo Histórico (SoT):** `OnCumulativeTradesResponse` se usa para solicitar *todos* los trades históricos (`RequestForCumulativeTrades`). `RebuildHistoricalSoT` luego itera por esta enorme lista para calcular los picos de velocidad (`_peakVolPerSec`, `_peakDeltaPerSec`) para cada barra cerrada.
  * **Cálculo en Tiempo Real (SoT):** `OnCalculate` gestiona el "live bar". Usa una `Queue<Sample>` (`_win`) como ventana deslizante. Cada tick (o actualización de `OnCalculate`) añade el `incVol` y `incDelta`, purga los datos antiguos de la cola, y recalcula el `_rtPeakVolPerSec` de la barra actual.
  * **Imbalances:** La lógica de imbalances (`for (decimal price = candle.High...`) se ejecuta al final de `OnCalculate` para escanear el footprint de la vela actual.

---

### 🛠️ Propuestas de mejora futura

- Permitir **alertas automáticas** cuando Delta/sec o PeakDeltaPerVol superen umbral dinámico calculado por sesión.  

-----

-----

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

Este es un indicador "Data-Dump" o "Dashboard" de nivel profesional. Es el hermano mayor de `CandleStatistics` (8/10). Mientras `CandleStatistics` te da un pequeño resumen *encima* de la vela, `ClusterStatistic` te da un *panel completo y dedicado* con 16 métricas, un heatmap y la capacidad de reordenar las filas.

**Tus Modificaciones ("Alfa"):**
Tus añadidos elevan este indicador a un 10/10. La versión estable es un 9/10, pero añadir:

1.  **Velocidad (Speed of Tape):** `Max Vol/sec` y `Delta at peak`.
2.  **Imbalances:** `Net Imbalance` y `Stacked Imbalance`.
    ...transforma esto de un "dashboard estadístico" a una "herramienta de ejecución" completa.

La *Velocidad* es crucial: un `Volume` de 2000 lotes en 1 segundo (ignición) es una señal completamente diferente a un `Volume` de 2000 lotes en 60 segundos (absorción/distribución). Tu indicador es el único que hemos visto que mide esto.

-----

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, es una herramienta de análisis de contexto de primera categoría.**

No te da señales de entrada, pero te da toda la información que necesitas para *validar* una señal. Te permite ver la "anatomía" completa de cada vela. Su único "defecto" es que puede ser una sobrecarga de información si no se configura para mostrar solo lo esencial (lo cual has resuelto con tu parametrización óptima).

**Acción:** **Conservar (Herramienta Principal).**

**¿Merece la pena arreglarlo?** El indicador (tu versión modificada) es una obra maestra. Está completo.