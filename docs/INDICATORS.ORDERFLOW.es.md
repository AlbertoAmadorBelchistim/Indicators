# 🔥 Master Index: Order Flow & Volumen

### 📊 Status Dashboard
**Progreso de Auditoría:** `▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░░░` **71%** (Fase 2 Completada)

| Total Indicadores | 🟢 Core (Ganadores) | 🟡 Reserva (Backup) | 💀 Descartados |
| :---: | :---: | :---: | :---: |
| **67** | **25** | **17** | 16 |

[🔙 Volver al Índice General](INDICATORS.es.md)

---
## 📑 Navegación Rápida
| [🧱 DOM](#dom) | [📉 Delta](#delta) | [👣 Footprint](#footprint) | [💎 Open Interest](#open-interest) | [📊 Volume](#volume) | [📂 Volume Profile](#volume-profile) | 

---

<br>

<a id='dom'></a>
## 🧱 DOM

### ⚔️ DOM Liquidity Flow &nbsp;✅ <small>FASE 2</small>
_Dinámica de liquidez: Stacking, Pulling y cambios en el LOB._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **9/10** | ✅ | 🟢 **CORE** | [DOM Dynamics](indicators/es/DomDynamics.md) | ¿Se está añadiendo (Stacking) o retirando (Pulling) liquidez neta del mercado en este instante? |
| 9/10 | ✅ | 🟡 Reserva | [Order Book Alerts](indicators/es/OrderBookAlerts.md) | ¿Dónde hay muros de liquidez en el DOM que superan un cierto tamaño y persisten en el tiempo? |
| 8/10 | ✅ | 🟡 Reserva | [Pulling & Stacking Bars Lite](indicators/es/PullingStackingBars.md) | ¿Qué lado (Bid/Ask) está añadiendo (Stacking) o quitando (Pulling) órdenes y en qué magnitud exacta? |

### ⚔️ DOM Visuals &nbsp;✅ <small>FASE 2</small>
_Visualización gráfica del libro de órdenes y profundidad (Depth of Market)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **10/10** | ✅ | 🟢 **CORE** | [MBO DOM](indicators/es/MBODOM.md) | ¿La muralla de liquidez es real (una institución) o son 500 traders retail? ¿Hay bloques grandes esperando (Icebergs)? |
| **9/10** | ✅ | 🟢 **CORE** | [DOM Levels/Heatmap](indicators/es/DOMLevels.md) | ¿Dónde ha estado la liquidez históricamente? ¿Es este nivel de soporte nuevo o lleva ahí todo el día? |
| 7/10 | ✅ | 🟡 Reserva | [Depth Of Market](indicators/es/DOM.md) | ¿Cuánta liquidez hay AHORA MISMO en cada nivel (agregado)? |

### ⚔️ Liquidez vs Agresión &nbsp;✅ <small>FASE 2</small>
_Comparativa de fuerza: Órdenes Limitadas (Muros) vs Órdenes a Mercado (Ataques)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **10/10** | ✅ | 🟢 **CORE** | [DOM Pressure](indicators/es/DomPressure.md) | ¿Está el mercado absorbiendo la agresión o dejándola pasar? Indicador híbrido que superpone la intención pasiva (DOM Power) con la agresión real (Trade Strength). |
| 9/10 | ✅ | 💀 Descartar | [DOM Power Modif](indicators/es/DomPowerModif.md) | <sub>¿Cuál es el desequilibrio neto (Bids vs Asks) en el libro de órdenes y su rango de volatilidad?</sub> |
| 9/10 | ✅ | 💀 Descartar | [DOM Strength Modif](indicators/es/DomStrengthModif.md) | <sub>¿Cuál es la fuerza de la agresión (Trades) en relación con la liquidez pasiva (DOM)?</sub> |

<br>

<a id='delta'></a>
## 📉 Delta

### ⚔️ Bar Delta Analysis &nbsp;✅ <small>FASE 2</small>
_Delta neto por vela. ¿Quién ganó la batalla en esta barra?_

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **10/10** | ✅ | 🟢 **CORE** | [Delta Modif](indicators/es/DeltaModif.md) | ¿Qué barras muestran una agresión (Delta) extrema, divergencia o absorción, y cómo se comporta el flujo respecto a su tendencia media? |
| 9/10 | ✅ | 🟡 Reserva | [Delta Patterns](indicators/es/DeltaPatterns.md) | ¿Qué patrones de micro-estructura ocurren dentro de una ventana de volumen constante? |
| 4/10 | ✅ | 💀 Descartar | [Ask/Bid Volume Difference Bars](indicators/es/AskBidBars.md) | <sub>¿Cuál fue el rango total (Max/Min) y el cierre neto del Delta en la vela?</sub> |
| 4/10 | ✅ | 💀 Descartar | [Delta Turnaround](indicators/es/DeltaTurnaround.md) | <sub>¿Se ha producido un patrón de giro de 3 velas confirmado por el delta?</sub> |
| 3/10 | ✅ | 💀 Descartar | [Average Delta](indicators/es/AverageDelta.md) | <sub>¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas?</sub> |
| 3/10 | ✅ | 💀 Descartar | [Delta Colored Candles](indicators/es/DeltaColoredCandles.md) | <sub>¿Cuál es la intensidad del momentum del delta en relación con un máximo fijo, visualizada en el color de las velas?</sub> |
| 2/10 | ✅ | 💀 Descartar | [Delta Strength](indicators/es/DeltaStrength.md) | <sub>¿Qué velas cierran con un delta dentro de un rango porcentual específico respecto a su extremo?</sub> |

### ⚔️ Bar Delta Details &nbsp;✅ <small>FASE 2</small>
_Desglose intra-vela del Delta (Bid vs Ask) y ratios de fuerza._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **8.5/10** | ✅ | 🟢 **CORE** | [Bid Ask Volume Ratio](indicators/es/BidAskVR.md) | ¿Cuál es el desequilibrio normalizado (de -100% a +100%) del volumen agresivo y su momentum? |
| 6.5/10 | ✅ | 🟡 Reserva | [Bid Ask](indicators/es/BidAsk.md) | ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela? |

### ⚔️ Cumulative Delta &nbsp;✅ <small>FASE 2</small>
_Tendencia de fondo. Acumulación de agresiones durante toda la sesión._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **10/10** | ✅ | 🟢 **CORE** | [CVD pro(multi) / Multi Market Powers (Modif)](indicators/es/MultiMarketPower.md) | ¿Está el Smart Money empujando ahora mismo o estamos ante una corrección pasiva dentro de una tendencia acumulada masiva? |
| 8/10 | ✅ | 🟡 Reserva | [CVD - Cumulative Volume Delta](indicators/es/CumulativeDelta.md) | ¿Cuál es el delta acumulado desde el inicio de la sesión (o desde una hora personalizada)? |
| 6/10 | ✅ | 💀 Descartar | [CVD pro / Market Power](indicators/es/MarketPower.md) | <sub>¿Cuál es el delta acumulado filtrado por tamaño de trade y su media móvil?</sub> |
| 2/10 | ✅ | 💀 Descartar | [COT High/Low](indicators/es/CotHigh.md) | <sub>¿Acumula el delta desde un nuevo máximo o mínimo?</sub> |

<br>

<a id='footprint'></a>
## 👣 Footprint

### ⚔️ Big Trades Analysis &nbsp;⚠️ <small>PENDIENTE</small>
_Detector de Ballenas. Filtros de grandes bloques y órdenes institucionales._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| 9/10 | ⚠️ | Conservar | [Adaptive Big Trades](indicators/es/AdaptiveBigTrades.md) | ¿Dónde están las operaciones grandes relativas a la liquidez actual (sin configurar filtros fijos)? |
| 9/10 | ⚠️ | Conservar | [Big Trades (Final Fix)](indicators/es/BigTrades.md) | ¿Dónde están entrando los agresores institucionales (bloques grandes)? |

### ⚔️ Cluster Analysis &nbsp;⚠️ <small>PENDIENTE</small>
_Estructura interna. Dónde se negoció el volumen dentro de la vela._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| 10/10 | ⚠️ | Conservar | [Absorption](indicators/es/Absorption.md) | ¿En qué niveles de precio se frenó el mercado a pesar de una gran agresión de volumen? |
| 10/10 | ⚠️ | Conservar | [Cluster Search Modif](indicators/es/ClusterSearchModif.md) | ¿Qué clústeres de precio específicos cumplen TODOS mis criterios de filtro? |
| 10/10 | ⚠️ | Conservar | [Cluster Statistic Modif](indicators/es/ClusterStatisticModif.md) | ¿Cuál es el 'dashboard' estadístico completo de cada vela? |

### ⚔️ Cluster Structure &nbsp;⚠️ <small>PENDIENTE</small>
_Patrones de forma dentro del Footprint (P, b, D profiles)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| 10/10 | ⚠️ | Conservar | [ZigZag Pro](indicators/es/Zigzag.md) | ¿Qué dicen las métricas acumuladas (Delta/Volumen) de cada onda de precio? |
| 8/10 | ⚠️ | Conservar | [Cluster Constructor Lite](indicators/es/ClusterConstructorLite.md) | ¿Existen patrones anómalos de volumen dentro de la estructura de la vela? |

### ⚔️ Imbalance Analysis &nbsp;⚠️ <small>PENDIENTE</small>
_Desequilibrios diagonales agresivos (Aggressive Buying/Selling Imbalances)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **9/10** | ⚠️ | 🟢 **CORE** | [Imbalance Ratio](indicators/es/ImbalanceRatio.md) | ¿Dónde se están produciendo desequilibrios diagonales de Ask vs. Bid en el clúster? |
| **8/10** | ⚠️ | 🟢 **CORE** | [Stacked Imbalance](indicators/es/StackedImbalance.md) | ¿Dónde existen zonas de desequilibrio agresivo apiladas que actúan como soporte? |

### ⚔️ Microstructure &nbsp;⚠️ <small>PENDIENTE</small>
_Eventos finos: Agotamiento (Exhaustion) y Absorción en extremos._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **9/10** | ⚠️ | 🟢 **CORE** | [Ratio](indicators/es/Ratio.md) | ¿Cuál es el ratio de absorción/agresión (Bid vs Ask) en el extremo de la vela? |
| 8/10 | ⚠️ | 🛠️ Reparar | [Exhaustion](indicators/es/Exhaustion.md) | ¿Está el precio mostrando agotamiento en los últimos N ticks de la vela? |

<br>

<a id='open-interest'></a>
## 💎 Open Interest

### ⚔️ Open Interest Analysis &nbsp;✅ <small>FASE 2</small>
_Salud de la tendencia basada en la entrada/salida de contratos reales._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| 4/10 | ✅ | 🟡 Reserva | [Open Interest](indicators/es/OpenInterest.md) | ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión? |
| 2/10 | ✅ | 🟡 Reserva | [OI Analyzer](indicators/es/OIAnalyzer.md) | ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle? |
| 1/10 | ✅ | 🟡 Reserva | [On Balance Open Interest](indicators/es/BalanceOI.md) | ¿Está el compromiso acumulado del 'dinero inteligente' subiendo o bajando en relación al precio? |
| 0/10 | ✅ | 💀 Descartar | [Herrick Payoff Index (HPI)](indicators/es/HerrickPayoff.md) | <sub>¿Cuál es la fuerza del movimiento combinando Precio, Volumen y OI?</sub> |

<br>

<a id='volume'></a>
## 📊 Volume

### ⚔️ Classic Volume &nbsp;✅ <small>FASE 2</small>
_Indicadores clásicos de acumulación/distribución (A/D)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| 2/10 | ✅ | 💀 Descartar | [Accumulation/Distribution (A/D)](indicators/es/AD.md) | <sub>¿El flujo de volumen acumulado (estimado por la forma de la vela) confirma la tendencia?</sub> |
| 1/10 | ✅ | 💀 Descartar | [Accumulation / Distribution Flow](indicators/es/ADF.md) | <sub>¿Cuál es la tendencia suavizada del flujo de volumen acumulado?</sub> |

### ⚔️ Execution Quality &nbsp;✅ <small>FASE 2</small>
_Estructura de ejecución: fragmentación vs bloques (tamaño medio por trade y calidad del tape)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **8.5/10** | ✅ | 🟢 **CORE** | [Volume Per Trade](indicators/es/VolumePerTrade.md) | ¿Cuál es el tamaño medio de la ejecución por trade en cada vela (proxy de participación institucional vs flujo fragmentado)? |

### ⚔️ Standard Volume &nbsp;✅ <small>FASE 2</small>
_Volumen total por barra y sus variantes visuales._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **9.5/10** | ✅ | 🟢 **CORE** | [Volume](indicators/es/VolumeModif.md) | ¿Cuál es el volumen real y estadísticamente relevante en cada vela según la métrica seleccionada? |
| 8/10 | ✅ | 🟡 Reserva | [Volume On The Chart](indicators/es/VolumeOnChart.md) | Visualiza el volumen como un histograma de fondo superpuesto al precio para ahorrar espacio. |
| 6/10 | ✅ | 🟡 Reserva | [Cumulative Daily Volume](indicators/es/CumulativeDailyVolume.md) | ¿Cuál es el volumen total acumulado desde el inicio de la sesión? |

### ⚔️ Tape Analysis &nbsp;✅ <small>FASE 2</small>
_Lectura de Cinta (Time & Sales). Reconstrucción de órdenes._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **9/10** | ✅ | 🟢 **CORE** | [Tape Patterns](indicators/es/TapePattern.md) | ¿Dónde están los bloques de órdenes grandes y patrones de ejecución específicos en la cinta? |
| 7.5/10 | ✅ | 🟡 Reserva | [Order Flow Indicator](indicators/es/OrderFlow.md) | ¿Cómo se visualiza el flujo de órdenes (trades individuales) en el gráfico? |

### ⚔️ Tape Speed &nbsp;✅ <small>FASE 2</small>
_Ritmo del mercado. Detección de aceleraciones y frenazos en la cinta._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **10/10** | ✅ | 🟢 **CORE** | [Speed of Tape Modif V2](indicators/es/SpeedOfTapeModifV2.md) | ¿Cuál es la velocidad REAL de ejecución (HFT) independientemente de la duración de la vela? |
| 6/10 | ✅ | 🟡 Reserva | [Speed of Tape (Lab)](indicators/es/SpeedOfTapeLab.md) | ¿Cuál es la velocidad de ejecución del mercado calculada por interpolación? |
| 7/10 | ✅ | Conservar (Refactorizar Urgente) | [Order Flow Rhythm (Lab)](indicators/es/OrderFlowRythmLab.md) | ¿Cuál es la intensidad/ritmo del mercado visualizada como mapa de calor (Heatmap)? |

### ⚔️ VSA & Anomalies &nbsp;✅ <small>FASE 2</small>
_Volume Spread Analysis. Velas climáticas, volumen de parada y anomalías._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **9/10** | ✅ | 🟢 **CORE** | [VSA Better Volume](indicators/es/VsaBetterVolume.md) | ¿Qué nos dice el volumen sobre la intención profesional (Clímax, Churn, Trampa)? |
| 5/10 | ✅ | 🟡 Reserva | [Relative Volume](indicators/es/RelativeVolume.md) | ¿Es el volumen actual anómalamente alto comparado con el promedio histórico para esta misma hora? |
| 4/10 | ✅ | 💀 Descartar | [Spread Volume](indicators/es/SpreadVolume.md) | <sub>¿Quién está agrediendo más dentro del spread actual?</sub> |
| 3/10 | ✅ | 💀 Descartar | [Bar's Volume Filter](indicators/es/BarVolumeFilter.md) | <sub>¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks?</sub> |

### ⚔️ Volume Efficiency &nbsp;✅ <small>FASE 2</small>
_Esfuerzo vs Resultado. ¿Cuánto mueve el precio cada unidad de volumen?_

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **7.5/10** | ✅ | 🟢 **CORE** | [Volume Bar Range Ratio](indicators/es/VBRR.md) | ¿Cuánto volumen “cuesta” mover el precio (volumen por unidad de rango) en cada vela? |
| 6/10 | ✅ | 🟡 Reserva | [Arms Ease of Movement](indicators/es/EMV.md) | ¿Qué “facilidad” tiene el precio para desplazarse al ajustar el movimiento del punto medio por la fricción volumen/rango? |
| 5.5/10 | ✅ | 💀 Descartar | [Market Facilitation Index](indicators/es/MarketFacilitation.md) | <sub>¿Cuánta “facilidad” tiene el mercado para mover el precio por unidad de volumen (rango por volumen) en cada vela?</sub> |

### ⚔️ Volume Oscillators &nbsp;✅ <small>FASE 2</small>
_Olas y flujos de volumen oscilante (Weiss Wave, TVI)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **9/10** | ✅ | 🟢 **CORE** | [Up/Down Volume Ratio](indicators/es/UpDownVolumeRatio.md) | ¿Quién controla el flujo de volumen (compradores o vendedores) y con qué intensidad relativa? |
| 7.5/10 | ✅ | 🟡 Reserva | [MACD - Volume Weighted](indicators/es/MacdVW.md) | ¿Cuál es la convergencia entre dos medias ponderadas por volumen y su señal suavizada para filtrar tendencia con participación? |
| 7.5/10 | ✅ | 🟡 Reserva | [Weis Wave](indicators/es/WeissWave.md) | ¿Cuánto volumen acumulado (esfuerzo) hay en la onda de precio actual? |
| 4.5/10 | ✅ | 💀 Descartar | [Trade Volume Index](indicators/es/TVI.md) | <sub>¿Se está acumulando o distribuyendo el volumen asignándolo tick a tick según la dirección del precio respecto al tick size?</sub> |

<br>

<a id='volume-profile'></a>
## 📂 Volume Profile

### ⚔️ Dynamic Profiles &nbsp;⚠️ <small>PENDIENTE</small>
_Perfiles móviles que se adaptan al precio (VWAP, Canales dinámicos)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **10/10** | ⚠️ | 🟢 **CORE** | [VWAP/TWAP](indicators/es/VWAP.md) | ¿Cuál es el precio medio ponderado por volumen (institucional) y sus desviaciones estándar? |
| **9/10** | ⚠️ | 🟢 **CORE** | [Dynamic Levels Channel](indicators/es/DynamicLevelsChannel.md) | ¿Dónde se están formando el POC, VAH y VAL de las últimas N barras (un perfil móvil)? |

### ⚔️ Session Profile &nbsp;⚠️ <small>PENDIENTE</small>
_Perfil de Volumen fijo por sesión (TPO, Market Profile, VPOC)._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **9/10** | ⚠️ | 🟢 **CORE** | [Dynamic Levels](indicators/es/DynamicLevels.md) | ¿Dónde se están formando el POC, VAH y VAL del período actual en tiempo real? |
| **8.5/10** | ⚠️ | 🟢 **CORE** | [Maximum Levels](indicators/es/MaxLevels.md) | ¿En qué nivel de precio se produjo el máximo Volumen (o Bid, Ask, Delta) para el período seleccionado? |
| **8.5/10** | ⚠️ | 🟢 **CORE** | [Unfinished Auction](indicators/es/UnfinishedAuction.md) | ¿Quedaron órdenes pendientes en los extremos de la vela que el precio debe volver a visitar? |

### ⚔️ Volume Profile &nbsp;⚠️ <small>PENDIENTE</small>
_Perfiles de volumen estáticos y zonas de alto valor._

| Nota | Fase | Estado | Indicador | Pregunta Clave / Descripción |
| :---: | :---: | :--- | :--- | :--- |
| **10/10** | ⚠️ | 🟢 **CORE** | [Volume-based Support & Resistance Zones](indicators/es/VolumeSupResZones.md) | ¿Dónde están las zonas de soporte y resistencia definidas por volumen en múltiples marcos temporales? |
| **9/10** | ⚠️ | 🟢 **CORE** | [Active Volume](indicators/es/ActiveVolume.md) | Filtrando ruido, ¿dónde está el volumen significativo y agresivo? |
| **8/10** | ⚠️ | 🟢 **CORE** | [HRanges](indicators/es/HRanges.md) | ¿Dónde se están formando rangos (consolidaciones) y cuál es el POC interno? |
