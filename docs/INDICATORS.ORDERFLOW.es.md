# 🔥 Master Index: Order Flow & Volumen

> **Leyenda de Fases:**
> * **✅ Fase 2:** Auditado con Gold Standard.
> * **⚠️ Fase 1:** Importación original.

[🔙 Volver al Índice General](INDICATORS.es.md)

---

## 🧱 DOM
> *El terreno de juego. Órdenes limitadas (pendientes) y muros de liquidez.*


### ⚔️ DOM Visuals <small>⚠️ (Pendiente)</small>
_Visualización gráfica del libro de órdenes y profundidad (Depth of Market)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Depth of Market](indicators/es/DOM.md) | ¿Cuál es la liquidez (libro de órdenes) actual, dibujada en el gráfico? |
| 9/10 | ✅ Conservar | [DOM Heatmap (Manual)](indicators/es/DOMLevels.md) | ¿Cómo ha evolucionado la liquidez del libro de órdenes (Heatmap) a lo largo del tiempo? |

### ⚔️ Liquidez vs Agresión <small>⚠️ (Pendiente)</small>
_Comparativa de fuerza: Órdenes Limitadas (Muros) vs Órdenes a Mercado (Ataques)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [DOM Power Modif](indicators/es/DomPower.md) | ¿Cuál es el desequilibrio neto (Bids vs Asks) en el libro de órdenes y su rango? |
| 9/10 | ✅ Conservar | [DOM Strength Modif](indicators/es/DomStrengthModif.md) | ¿Cuál es la fuerza de la agresión (Trades) en relación con la liquidez pasiva (DOM)? |

### ⚔️ Liquidity Events <small>⚠️ (Pendiente)</small>
_Detección de manipulación: Spoofing, Pulling (retirada) y Stacking (apilamiento)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 10/10 | ✅ Conservar | [Pulling & Stacking Bars (Clean)](indicators/es/PullingStackingBars.md) | ¿Se está añadiendo (Stacking) o retirando (Pulling) liquidez del libro de órdenes? |
| 9/10 | ✅ Conservar | [Order Book Alerts](indicators/es/OrderBookAlerts.md) | ¿Dónde hay muros de liquidez en el DOM que superan un tamaño y persisten? |

## 📉 Delta
> *El pulso de la batalla. ¿Quién golpea el mercado a mercado (Market Orders)?*


### ⚔️ Bar Delta <small>✅ (Fase 2)</small>
_Delta neto por vela. ¿Quién ganó la batalla en esta barra?_

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| **10/10** | ✅ Conservar (Core) | [Delta Modif](indicators/es/DeltaModif.md) | ¿Qué barras muestran una agresión (Delta) extrema, divergencia o absorción, y cómo puedo ver esas señales directamente en el gráfico de precio? |
| **6.5/10** | ✅ Conservar (Reserva) | [Ask/Bid Volume Difference Bars](indicators/es/AskBidBars.md) | ¿Cuál fue el volumen agresivo neto (Delta) de esta vela y cuál fue el rango total de la lucha entre compradores y vendedores? |
| **4/10** | 💀 Descartar | [Delta Turnaround](indicators/es/DeltaTurnaround.md) | ¿Se ha producido un patrón de giro de 3 velas confirmado por el delta? |
| **3/10** | 💀 Descartar | [Average Delta](indicators/es/AverageDelta.md) | ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas? |
| **3/10** | 💀 Descartar | [Delta Colored Candles](indicators/es/DeltaColoredCandles.md) | ¿Cuál es la intensidad del momentum del delta en relación con un máximo fijo, visualizada en el color de las velas? |
| **2/10** | 💀 Descartar | [Delta Strength](indicators/es/DeltaStrength.md) | ¿Qué velas cierran con un delta dentro de un rango porcentual específico respecto a su extremo? |

### ⚔️ Bar Delta Details <small>✅ (Fase 2)</small>
_Desglose intra-vela del Delta (Bid vs Ask) y ratios de fuerza._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| **7/10** | ✅ Conservar (Core) | [Bid Ask Volume Ratio](indicators/es/BidAskVR.md) | ¿Cuál es el desequilibrio normalizado del volumen agresivo y su momentum? |
| **6.5/10** | ✅ Conservar (Reserva) | [Bid Ask](indicators/es/BidAsk.md) | ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela? |

### ⚔️ Cumulative Delta <small>✅ (Fase 2)</small>
_Tendencia de fondo. Acumulación de agresiones durante toda la sesión._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| **10/10** | ✅ Conservar (Core) | [CVD pro(multi) / Multi Market Powers](indicators/es/MultiMarketPower.md) | ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional)? |
| **8/10** | ✅ Conservar (Reserva) | [CVD - Cumulative Volume Delta](indicators/es/CumulativeDelta.md) | ¿Cuál es el delta acumulado desde el inicio de la sesión? |
| **6/10** | 💀 Descartar | [CVD pro / Market Power](indicators/es/MarketPower.md) | ¿Cuál es el delta acumulado filtrado por tamaño de trade y su media móvil? |
| **2/10** | 💀 Descartar | [COT High/Low](indicators/es/CotHigh.md) | ¿Acumula el delta desde un nuevo máximo o mínimo? |

## 👣 Footprint
> *La radiografía. Análisis del volumen dentro de la vela (Clusters).*


### ⚔️ Big Trades Analysis <small>⚠️ (Pendiente)</small>
_Detector de Ballenas. Filtros de grandes bloques y órdenes institucionales._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Adaptive Big Trades](indicators/es/AdaptiveBigTrades.md) | ¿Dónde están las operaciones grandes relativas a la liquidez actual (sin configurar filtros fijos)? |
| 9/10 | ✅ Conservar | [Big Trades (Final Fix)](indicators/es/BigTrades.md) | ¿Dónde están entrando los agresores institucionales (bloques grandes)? |

### ⚔️ Cluster Analysis <small>⚠️ (Pendiente)</small>
_Estructura interna. Dónde se negoció el volumen dentro de la vela._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 10/10 | ✅ Conservar | [Absorption](indicators/es/Absorption.md) | ¿En qué niveles de precio se frenó el mercado a pesar de una gran agresión de volumen? |
| 10/10 | ✅ Conservar | [Cluster Search Modif](indicators/es/ClusterSearchModif.md) | ¿Qué clústeres de precio específicos cumplen TODOS mis criterios de filtro? |
| 10/10 | ✅ Conservar | [Cluster Statistic Modif](indicators/es/ClusterStatisticModif.md) | ¿Cuál es el 'dashboard' estadístico completo de cada vela? |

### ⚔️ Cluster Structure <small>⚠️ (Pendiente)</small>
_Patrones de forma dentro del Footprint (P, b, D profiles)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 10/10 | ✅ Conservar | [ZigZag Pro](indicators/es/Zigzag.md) | ¿Qué dicen las métricas acumuladas (Delta/Volumen) de cada onda de precio? |
| 8/10 | ✅ Conservar | [Cluster Constructor Lite](indicators/es/ClusterConstructorLite.md) | ¿Existen patrones anómalos de volumen dentro de la estructura de la vela? |

### ⚔️ Imbalance Analysis <small>⚠️ (Pendiente)</small>
_Desequilibrios diagonales agresivos (Aggressive Buying/Selling Imbalances)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Imbalance Ratio](indicators/es/ImbalanceRatio.md) | ¿Dónde se están produciendo desequilibrios diagonales de Ask vs. Bid en el clúster? |
| 8/10 | ✅ Conservar | [Stacked Imbalance](indicators/es/StackedImbalance.md) | ¿Dónde existen zonas de desequilibrio agresivo apiladas que actúan como soporte? |

### ⚔️ Microstructure <small>⚠️ (Pendiente)</small>
_Eventos finos: Agotamiento (Exhaustion) y Absorción en extremos._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Ratio](indicators/es/Ratio.md) | ¿Cuál es el ratio de absorción/agresión (Bid vs Ask) en el extremo de la vela? |
| 8/10 | 🛠️ Reparar | [Exhaustion](indicators/es/Exhaustion.md) | ¿Está el precio mostrando agotamiento en los últimos N ticks de la vela? |

## 💎 Open Interest
> *El compromiso. Dinero nuevo entrando vs cierre de posiciones.*


### ⚔️ Open Interest Analysis <small>⚠️ (Pendiente)</small>
_Salud de la tendencia basada en la entrada/salida de contratos reales._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 10/10 | ✅ Conservar (Indicador Core) | [OI Analyzer](indicators/es/OIAnalyzer.md) | ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle? |
| 7/10 | ✅ Conservar (Reserva) | [On Balance Open Interest](indicators/es/BalanceOI.md) | ¿Está el compromiso acumulado del 'dinero inteligente' subiendo o bajando en relación al precio? |
| 6/10 | ✅ Conservar (Reserva / Donante) | [Open Interest](indicators/es/OpenInterest.md) | ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión? |
| 3/10 | 🛠️ Reparar | [Herrick Payoff Index (HPI)](indicators/es/HerrickPayoff.md) | ¿Cuál es la fuerza del movimiento (Precio + Volumen + OI)? |

## 📊 Volume
> *La gasolina. Cantidad total operada, perfiles y velocidad de cinta.*


### ⚔️ Classic Volume <small>✅ (Fase 2)</small>
_Indicadores clásicos de acumulación/distribución (A/D)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| **2/10** | 💀 Descartar | [Accumulation/Distribution (A/D)](indicators/es/AD.md) | ¿El flujo de volumen acumulado está confirmando la tendencia del precio? |
| **1/10** | 💀 Descartar | [Accumulation / Distribution Flow](indicators/es/ADF.md) | ¿Cuál es la tendencia suavizada del flujo de volumen acumulado? |

### ⚔️ Standard Volume <small>⚠️ (Pendiente)</small>
_Volumen total por barra y sus variantes visuales._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Volume](indicators/es/Volume.md) | ¿Cuál es el volumen de actividad en cada vela? |
| 8/10 | ✅ Conservar | [Volume On The Chart](indicators/es/VolumeOnChart.md) | Visualiza el volumen como un histograma de fondo superpuesto al precio. |
| 6/10 | ✅ Conservar | [Cumulative Daily Volume](indicators/es/CumulativeDailyVolume.md) | ¿Cuál es el volumen total acumulado desde el inicio de la sesión? |

### ⚔️ Tape Analysis <small>⚠️ (Pendiente)</small>
_Lectura de Cinta (Time & Sales). Reconstrucción de órdenes._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Order Flow Indicator](indicators/es/OrderFlow.md) | ¿Cómo se visualiza el flujo de órdenes (trades individuales) en el gráfico? |
| 9/10 | ✅ Conservar | [Tape Patterns](indicators/es/TapePattern.md) | ¿Dónde están los bloques de órdenes grandes en la cinta? |

### ⚔️ Tape Speed <small>⚠️ (Pendiente)</small>
_Ritmo del mercado. Detección de aceleraciones y frenazos en la cinta._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Order Flow Rhythm (Clean)](indicators/es/OrderFlowRythm.md) | ¿Cuál es la velocidad de ejecución (ritmo) del mercado visualizada como mapa de calor? |
| 8/10 | ✅ Conservar | [Speed of Tape](indicators/es/SpeedOfTape.md) | ¿Cuál es la velocidad de ejecución (ritmo) del mercado? |

### ⚔️ VSA & Anomalies <small>⚠️ (Pendiente)</small>
_Volume Spread Analysis. Velas climáticas, volumen de parada y anomalías._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [VSA Better Volume](indicators/es/VsaBetterVolume.md) | ¿Qué nos dice el volumen sobre la intención profesional (Clímax, Churn, Trampa)? |
| 8/10 | ✅ Conservar | [VSA – WSD Histogram](indicators/es/VsaWsd.md) | ¿Cómo se distribuye la estructura de la vela (mechas vs cuerpo) y el volumen relativo? |
| 7/10 | 🔧 Mejorar | [Bar's Volume Filter](indicators/es/BarVolumeFilter.md) | ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks? |
| 7/10 | 🔧 Mejorar | [Relative Volume](indicators/es/RelativeVolume.md) | ¿Es el volumen actual anómalamente alto o bajo comparado con el promedio histórico? |
| 7/10 | ✅ Conservar | [Spread Volume](indicators/es/SpreadVolume.md) | ¿Quién está agrediendo más dentro del spread actual? |

### ⚔️ Volume Efficiency <small>⚠️ (Pendiente)</small>
_Esfuerzo vs Resultado. ¿Cuánto mueve el precio cada unidad de volumen?_

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 8/10 | ✅ Conservar | [Volume Per Trade](indicators/es/VolumePerTrade.md) | ¿Cuál es el tamaño promedio de las órdenes ejecutadas en cada vela? |
| 7/10 | ✅ Conservar | [Volume Bar Range Ratio](indicators/es/VBRR.md) | ¿Quanto volumen es necesario para mover el precio 1 tick (Eficiencia)? |
| 6.5/10 | ✅ Conservar | [Arms Ease of Movement](indicators/es/EMV.md) | ¿Es el movimiento del precio eficiente en relación con su volumen y rango? |
| 6/10 | 🔧 Mejorar | [Market Facilitation Index](indicators/es/MarketFacilitation.md) | ¿Cuál es la eficiencia del mercado (MFI) para mover el precio en relación con el volumen? |

### ⚔️ Volume Oscillators <small>⚠️ (Pendiente)</small>
_Olas y flujos de volumen oscilante (Weiss Wave, TVI)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Up/Down Volume Ratio](indicators/es/UpDownVolumeRatio.md) | ¿Quién controla el flujo de volumen (compradores o vendedores) y con qué intensidad? |
| 8/10 | 🔧 Mejorar | [MACD - Volume Weighted](indicators/es/MacdVW.md) | ¿Cuál es la convergencia entre dos medias ponderadas por volumen (VWMAs)? |
| 8/10 | ✅ Conservar | [Weis Wave](indicators/es/WeissWave.md) | ¿Cuánto volumen acumulado (esfuerzo) hay en la onda de precio actual? |
| 7/10 | ✅ Conservar | [Trade Volume Index](indicators/es/TVI.md) | ¿Se está acumulando o distribuyendo el volumen basándose en la dirección del tick? |

### ⚔️ Volume Profile <small>⚠️ (Pendiente)</small>
_Perfiles de volumen estáticos y zonas de alto valor (High Volume Nodes)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 10/10 | ✅ Conservar | [Volume-based Support & Resistance Zones](indicators/es/VolumeSupResZones.md) | ¿Dónde están las zonas de soporte y resistencia definidas por volumen en múltiples marcos temporales? |
| 8.5/10 | ✅ Conservar | [HRanges](indicators/es/HRanges.md) | ¿Dónde se están formando rangos (consolidaciones) y cuál es el POC interno? |
| 8/10 | 🔧 Mejorar | [Active Volume](indicators/es/ActiveVolume.md) | Filtrando ruido, ¿dónde está el volumen significativo y agresivo? |

## 📂 Volume Profile

### ⚔️ Dynamic Profiles <small>⚠️ (Pendiente)</small>
_Perfiles móviles que se adaptan al precio (VWAP, Canales dinámicos)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 10/10 | ✅ Conservar | [VWAP / TWAP](indicators/es/VWAP.md) | ¿Cuál es el precio medio ponderado por volumen (institucional) y sus desviaciones? |
| 9/10 | ✅ Conservar | [Dynamic Levels Channel](indicators/es/DynamicLevelsChannel.md) | ¿Dónde se están formando el POC, VAH y VAL de las últimas N barras (perfil móvil)? |

### ⚔️ Session Profile <small>⚠️ (Pendiente)</small>
_Perfil de Volumen fijo por sesión (TPO, Market Profile, VPOC)._

| Score | Estado | Indicador | Descripción / Notas |
| :---: | :--- | :--- | :--- |
| 9/10 | ✅ Conservar | [Dynamic Levels](indicators/es/DynamicLevels.md) | ¿Dónde se están formando el POC, VAH y VAL del período actual en tiempo real? |
| 9/10 | ✅ Conservar | [Maximum Levels](indicators/es/MaxLevels.md) | ¿En qué nivel de precio se produjo el máximo Volumen (o Bid, Ask, Delta)? |
| 9/10 | ✅ Conservar | [Unfinished Auction](indicators/es/UnfinishedAuction.md) | ¿Quedaron órdenes pendientes en los extremos de la vela que el precio debe volver a visitar? |
