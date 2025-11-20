# 🔥 Master Index: Order Flow & Volumen

> **El Corazón del Sistema:** Aquí residen las herramientas que detectan la agresión, la absorción y la liquidez institucional. El objetivo es filtrar esta lista para quedarnos solo con los indicadores que ofrecen una ventaja competitiva real (Edge).

> **Estado del Sistema:**
> * **Delta:** ✅ Completado (8 indicadores auditados).
> * **Footprint:** ⏳ Pendiente (Próximo objetivo).
> * **DOM / Volumen:** ⏳ Pendiente.

---


## 1. Delta (Agresión)
> **El pulso de la batalla.** Estos indicadores miden quién está golpeando el mercado (Market Orders) con más fuerza: compradores o vendedores. Es la herramienta principal para detectar divergencias y agotamiento.

| Score | Estado | Indicador | Grupo Comparativo | Pregunta Clave |
| :---: | :--- | :--- | :--- | :--- |
| **10/10** | 🏆 Conservar (Indicador Core) | [Delta Modif](indicators/es/DeltaModif.md) | *Bar Delta* | ¿Qué barras muestran una agresión (Delta) extrema, divergencia o absorción, y cómo puedo ver esas señales directamente en el gráfico de precio? |
| **5/10** | 🔄 Fusionar Lógica | [Average Delta](indicators/es/AverageDelta.md) | *Bar Delta* | ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas? |
| **4/10** | 💀 Descartar | [Delta Turnaround](indicators/es/DeltaTurnaround.md) | *Bar Delta* | ¿Se ha producido un patrón de giro de 3 velas confirmado por el delta? |
| **3/10** | 💀 Descartar | [Delta Colored Candles](indicators/es/DeltaColoredCandles.md) | *Bar Delta* | ¿Cuál es la intensidad del momentum del delta en relación con un máximo fijo? |
| **2/10** | 💀 Descartar / Revisar | [Delta Strength](indicators/es/DeltaStrength.md) | *Bar Delta* | ¿Qué velas cierran con un delta que está *casi* en su extremo (MaxDelta/MinDelta)? |
| **10/10** | ✅ Conservar | [CVD pro(multi) / Multi Market Powers](indicators/es/MultiMarketPower.md) | *Cumulative Delta* | ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional)? |
| **8/10** | ✅ Conservar (Reserva / Donante) | [CVD - Cumulative Volume Delta](indicators/es/CumulativeDelta.md) | *Cumulative Delta* | ¿Cuál es el delta acumulado (la agresión neta) desde el inicio de la sesión? |
| **6/10** | ⚠️ Reemplazar por MultiMarketPower | [CVD pro / Market Power](indicators/es/MarketPower.md) | *Cumulative Delta* | ¿Cuál es el delta acumulado (CVD) filtrado por tamaño de trade, y cómo se compara con su SMA? |
| **6/10** | ✅ Conservar | [Cumulative Daily Volume](indicators/es/CumulativeDailyVolume.md) | *-* | ¿Cuál es el volumen total acumulado desde el inicio de la sesión? |

## 2. Footprint (Huella)
> **La radiografía de la vela.** Aquí miramos *dentro* de la barra para ver dónde se acumuló el volumen, dónde hubo absorción pasiva y dónde entraron los grandes bloques (Big Trades).

| Score | Estado | Indicador | Grupo Comparativo | Pregunta Clave |
| :---: | :--- | :--- | :--- | :--- |
| **10/10** | ✅ Conservar | [Cluster Search Modifar](indicators/es/ClusterSearchModif.md) | *-* | '¿Qué clústeres de precio específicos en este gráfico cumplen *todos*' mis criterios de filtro (por Volumen, Delta, Localización, Imbalance, etc.)? |
| **10/10** | ✅ Conservar | [Cluster Statistic Modif ](indicators/es/ClusterStatisticModif.md) | *-* | ¿Cuál es el "dashboard" estadístico completo (Volumen, Delta, Ticks, *Velocidad* e *Imbalances*) de cada vela, y cómo se compara cada vela con la más "fuerte" del gráfico? |
| **9/10** | ✅ Conservar | [Adaptive Big Trades](indicators/es/AdaptiveBigTrades.md) | *-* | ¿Dónde están las operaciones grandes relativas a la liquidez actual (sin configurar filtros fijos)? |
| **9/10** | ✅ Conservar | [Big Trades (Final Fix)](indicators/es/BigTrades.md) | *-* | ¿Dónde están entrando los agresores institucionales (bloques grandes de compra/venta)? |
| **9/10** | ✅ Conservar | [Imbalance Ratio](indicators/es/ImbalanceRatio.md) | *-* | ¿Dónde se están produciendo desequilibrios (imbalances) diagonales de Ask vs. Bid en el clúster que superan un ratio y volumen mínimos? |
| **8/10** | ✅ Conservar | [Cluster Constructor Lite](indicators/es/ClusterConstructorLite.md) | *-* | ¿Existen patrones anómalos de volumen (ej. doble núcleo) dentro de la estructura de la vela? |
| **8/10** | ✅ Conservar | [Stacked Imbalance](indicators/es/StackedImbalance.md) | *-* | ¿Dónde existen zonas de desequilibrio agresivo de compra/venta apiladas que actúan como soporte/resistencia? |

## 3. DOM (Liquidez)
> **El terreno de juego.** Miden las órdenes pendientes (Limit Orders). Nos dicen dónde están las barreras (soportes/resistencias pasivos) y si hay manipulación (Spoofing).

| Score | Estado | Indicador | Grupo Comparativo | Pregunta Clave |
| :---: | :--- | :--- | :--- | :--- |
| **9/10** | ✅ Conservar | [Depth of Market](indicators/es/DOM.md) | *-* | ¿Cuál es la liquidez (libro de órdenes) actual, dibujada en el gráfico? |
| **9/10** | ✅ Conservar | [DOM Heatmap (Manual)](indicators/es/DOMLevels.md) | *-* | ¿Cómo ha evolucionado la liquidez del libro de órdenes (Heatmap) a lo largo del tiempo en el gráfico? |
| **9/10** | ✅ Conservar | [DOM Power Modif](indicators/es/DomPower.md) | *-* | '¿Cuál es el desequilibrio neto (Bids vs Asks) en el libro de órdenes y' cuál es su rango de volatilidad? |
| **9/10** | ✅ Conservar | [DOM Strength Modif](indicators/es/DomStrengthModif.md) | *-* | '¿Cuál es la fuerza de la agresión (Trades) en relación con la liquidez' pasiva (DOM)? |
| **9/10** | ✅ Conservar | [Order Book Alerts](indicators/es/OrderBookAlerts.md) | *-* | ¿Dónde hay muros de liquidez en el DOM que superan un cierto tamaño y persisten en el tiempo? |
| **5/10** | Mejorar | [Random Walk Index](indicators/es/RWI.md) | *-* | Determina si el movimiento del precio es estadísticamente significativo o solo ruido aleatorio. |

## 4. Volumen Puro
> **La gasolina.** Indicadores basados en el volumen total operado, sin distinguir compra/venta, o herramientas de perfil de volumen (Contexto).

| Score | Estado | Indicador | Grupo Comparativo | Pregunta Clave |
| :---: | :--- | :--- | :--- | :--- |
| **10/10** | ✅ Conservar | [Volume-based Support & Resistance Zones](indicators/es/VolumeSupResZones.md) | *-* | ¿Dónde están las zonas de soporte y resistencia definidas por volumen en múltiples marcos temporales? |
| **10/10** | ✅ Conservar | [VWAP / TWAP](indicators/es/VWAP.md) | *-* | ¿Cuál es el precio medio ponderado por volumen (institucional) y sus desviaciones estándar? |
| **9/10** | ✅ Conservar | [Up/Down Volume Ratio](indicators/es/UpDownVolumeRatio.md) | *-* | ¿Quién controla el flujo de volumen (compradores o vendedores) y con qué intensidad relativa? |
| **9/10** | ✅ Conservar | [Volume](indicators/es/Volume.md) | *-* | ¿Cuál es el volumen de actividad (o ticks/bid/ask) en cada vela, y cómo se relaciona con el movimiento? |
| **9/10** | ✅ Conservar | [Volume Zone Oscillator](indicators/es/VolumeZone.md) | *-* | ¿Cuál es la presión neta de compra/venta normalizada por el volumen total (Oscilador de Zona)? |
| **9/10** | ✅ Conservar | [VSA Better Volume](indicators/es/VsaBetterVolume.md) | *-* | ¿Qué nos dice el volumen sobre la intención profesional (Clímax, Churn, Trampa)? |
| **8/10** | Mejorar | [Active Volume](indicators/es/ActiveVolume.md) | *-* | Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios? |
| **8/10** | Mejorar | [MACD - Volume Weighted](indicators/es/MacdVW.md) | *-* | ¿Cuál es la convergencia/divergencia entre dos medias ponderadas por volumen (VWMAs)? |
| **8/10** | ✅ Conservar | [Simple Percentage Volume Oscillator](indicators/es/SPVO.md) | *-* | Oscilador que muestra la diferencia porcentual entre dos medias móviles de volumen. |
| **8/10** | ✅ Conservar | [Positive/Negative Volume Index](indicators/es/VolumeIndex.md) | *-* | ¿Qué está haciendo el 'dinero inteligente' (días de bajo volumen) frente al 'público' (días de alto volumen)? |
| **8/10** | ✅ Conservar | [Volume On The Chart](indicators/es/VolumeOnChart.md) | *-* | Visualiza el volumen como un histograma de fondo superpuesto al precio para ahorrar espacio. |
| **8/10** | ✅ Conservar | [Volume Per Trade](indicators/es/VolumePerTrade.md) | *-* | ¿Cuál es el tamaño promedio de las órdenes ejecutadas en cada vela (Institucional vs Retail)? |
| **8/10** | ✅ Conservar | [Price Volume Trend](indicators/es/VolumeTrend.md) | *-* | ¿Cuál es el flujo acumulado de volumen ponderado por la magnitud del movimiento del precio? |
| **7/10** | Mejorar | [Bar's Volume Filter](indicators/es/BarVolumeFilter.md) | *-* | ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks (ej. 'Volumen > 1500' y solo 'dentro de la sesión RTH')? |
| **7/10** | Mejorar | [Bid Ask Volume Ratio](indicators/es/BidAskVR.md) | *-* | ¿Cuál es el desequilibrio normalizado (de -100% a +100%) del volumen agresivo, y cuál es el momentum (pendiente) de ese desequilibrio? |
| **7/10** | Mejorar | [Relative Volume](indicators/es/RelativeVolume.md) | *-* | ¿Es el volumen actual anómalamente alto o bajo comparado con el promedio histórico para esta misma hora? |
| **7/10** | ✅ Conservar | [Spread Volume](indicators/es/SpreadVolume.md) | *-* | Visualiza el volumen ejecutado en el Ask y el Bid por separado, dibujado como histogramas en el spread. |
| **7/10** | ✅ Conservar | [Trade Volume Index](indicators/es/TVI.md) | *-* | ¿Se está acumulando o distribuyendo el volumen basándose en la dirección del tick? |
| **7/10** | ✅ Conservar | [Volume Bar Range Ratio](indicators/es/VBRR.md) | *-* | ¿Cuánto volumen es necesario para mover el precio 1 tick (Eficiencia del movimiento)? |
| **6.5/10** | Mejorar | [Ask/Bid Volume Difference Bars](indicators/es/AskBidBars.md) | *-* | ¿Cuál fue el volumen agresivo neto (Delta) de esta vela, e igualmente importante, cuál fue el rango interno (Delta Máx/Mín) de la batalla entre compradores y vendedores dentro de esa vela? |

## 5. Open Interest
> **El compromiso.** Miden si el dinero está entrando al mercado (nuevas posiciones) o saliendo (cierre de posiciones). Vital para confirmar la fuerza de una ruptura.

| Score | Estado | Indicador | Grupo Comparativo | Pregunta Clave |
| :---: | :--- | :--- | :--- | :--- |
| **9/10** | ✅ Conservar | [OI Analyzer](indicators/es/OIAnalyzer.md) | *-* | ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle? |
| **8/10** | Mejorar | [On Balance Open Interest](indicators/es/BalanceOI.md) | *-* | ¿Está el compromiso acumulado del 'dinero inteligente' (Interés Abierto) subiendo cuando los precios suben y bajando cuando los precios bajan, o está divergiendo? |
| **8/10** | ✅ Conservar | [Open Interest](indicators/es/OpenInterest.md) | *-* | ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión? |

## 6. Otros
> **Herramientas auxiliares** o experimentales de flujo de órdenes.

| Score | Estado | Indicador | Grupo Comparativo | Pregunta Clave |
| :---: | :--- | :--- | :--- | :--- |
| **10/10** | ✅ Conservar | [Absorption](indicators/es/Absorption.md) | *-* | ¿En qué niveles de precio se frenó el mercado a pesar de una gran agresión de volumen? |
| **10/10** | ✅ Conservar | [OHLC Plus Modif](indicators/es/OHLCPlus.md) | *-* | ¿Puedo tener TODOS los niveles de contexto clave (Diario, Semanal, Mensual, Contrato) en un solo indicador, con estilos profesionales y un sistema de etiquetas que no se solapen? |
| **9/10** | ✅ Conservar | [Dynamic Levels](indicators/es/DynamicLevels.md) | *-* | '¿Dónde se están formando el POC, VAH y VAL del período actual (ej.' Día, Semana, Hora) en tiempo real? |
| **9/10** | ✅ Conservar | [Dynamic Levels Channel](indicators/es/DynamicLevelsChannel.md) | *-* | '¿Dónde se están formando el POC, VAH y VAL de las últimas N barras (un' perfil móvil)? |
| **9/10** | ✅ Conservar | [Fair Value Gap](indicators/es/FairValueGap.md) | *-* | ¿Dónde están los desequilibrios de precio (gaps) no mitigados en el marco actual y superior? |
| **9/10** | ✅ Conservar | [Maximum Levels](indicators/es/MaxLevels.md) | *-* | ¿En qué nivel de precio se produjo el máximo Volumen (o Bid, Ask, Delta) para el período seleccionado? |
| **9/10** | ✅ Conservar | [Order Flow Indicator](indicators/es/OrderFlow.md) | *-* | ¿Cómo se visualiza el flujo de órdenes (trades individuales o acumulados) en el gráfico (círculos/rectángulos)? |
| **9/10** | ✅ Conservar | [Order Flow Rhythm (Clean)](indicators/es/OrderFlowRythm.md) | *-* | ¿Cuál es la velocidad de ejecución (ritmo) del mercado visualizada como mapa de calor? |
| **9/10** | ✅ Conservar | [Ratio](indicators/es/Ratio.md) | *-* | ¿Cuál es el ratio de absorción/agresión (Bid vs Ask) en el extremo de la vela? |
| **9/10** | ✅ Conservar | [Squeeze Momentum](indicators/es/SqueezeMomentum.md) | *-* | El famoso indicador de John Carter. Detecta periodos de baja volatilidad (Squeeze) seguidos de explosiones direccionales. |
| **9/10** | ✅ Conservar | [Tape Patterns](indicators/es/TapePattern.md) | *-* | ¿Dónde están los bloques de órdenes grandes y patrones de ejecución específicos en la cinta? |
| **9/10** | ✅ Conservar | [Unfinished Auction](indicators/es/UnfinishedAuction.md) | *-* | ¿Quedaron órdenes pendientes (desequilibrio) en los extremos de la vela que el precio debe volver a visitar? |
| **8.5/10** | ✅ Conservar | [HRanges](indicators/es/HRanges.md) | *-* | ¿Dónde se están formando rangos (consolidaciones) y cuál es el POC interno de esos rangos, filtrado por volumen y duración? |
| **8/10** | Mejorar | [ATR](indicators/es/ATR.md) | *-* | ¿Cuál ha sido el tamaño verdadero promedio (incluyendo gaps) de cada barra durante los últimos X períodos? |
| **8/10** | Mejorar | [Bollinger Bands](indicators/es/BollingerBands.md) | *-* | ¿Está el precio actual estadísticamente 'demasiado alto' o 'demasiado bajo' (sobre-extendido) en comparación con su media reciente, basándose en la volatilidad? |
| **8/10** | Mejorar | [Bollinger Squeeze 2](indicators/es/BollingerSqeezeV2.md) | *-* | ¿Está el mercado en compresión (Squeeze) Y cuál es la dirección del momentum (Histograma)? |
| **8/10** | Reparar | [Exhaustion](indicators/es/Exhaustion.md) | *-* | ¿Está el precio mostrando agotamiento (volumen creciente) en los últimos N ticks del máximo o mínimo de la vela? |
| **8/10** | ✅ Conservar | [Speed of Tape](indicators/es/SpeedOfTape.md) | *-* | Mide la velocidad de ejecución (ticks, volumen o delta) en una ventana de tiempo deslizante. |
| **8/10** | ✅ Conservar | [Standard Deviation](indicators/es/StdDev.md) | *-* | ¿Cuánto se está alejando el precio de su media (volatilidad absoluta)? |
| **8/10** | ✅ Conservar | [Standard Deviation Bands](indicators/es/StdDevBands.md) | *-* | ¿Está el precio alcanzando extremos estadísticos de volatilidad basados en máximos y mínimos? |
| **8/10** | ✅ Conservar | [Standard Error Bands](indicators/es/StdErrBands.md) | *-* | ¿Cuál es el rango de error estadístico esperado alrededor de la tendencia de regresión actual? |
| **8/10** | ✅ Conservar | [Synthetic VIX](indicators/es/SyntheticVix.md) | *-* | ¿Cómo de lejos está el precio actual del máximo reciente (Proxy de miedo/pánico)? |
| **8/10** | ✅ Conservar | [True Range](indicators/es/TrueRange.md) | *-* | ¿Cuál es la volatilidad real de la vela actual incluyendo los gaps de apertura? |
| **8/10** | ✅ Conservar | [Volatility Trend](indicators/es/VolatilityTrend.md) | *-* | ¿Cuál es el canal de tendencia dinámico ajustado por la persistencia de la dirección y la volatilidad? |
| **8/10** | ✅ Conservar | [VSA – WSD Histogram](indicators/es/VsaWsd.md) | *-* | ¿Cómo se distribuye la estructura de la vela (mechas vs cuerpo) y el volumen relativo? |
| **8/10** | ✅ Conservar | [Weis Wave](indicators/es/WeissWave.md) | *-* | ¿Cuánto volumen acumulado (esfuerzo) hay en la onda de precio actual? |
| **7.5/10** | Reparar | [Keltner Channel](indicators/es/KeltnerChannel.md) | *-* | ¿Dónde se sitúan las bandas de volatilidad (SMA +/- ATR * Multiplicador) y el precio se está aproximando a ellas? |
| **7/10** | Mejorar | [ADR](indicators/es/ADR.md) | *-* | ¿Cuál es el rango de movimiento "normal" o "promedio" para este instrumento en una sesión, y dónde se proyectarían esos límites hoy? |
| **7/10** | Mejorar | [Block Moving Average](indicators/es/BlockMA.md) | *-* | ¿Cómo puedo crear un 'trailing stop' de volatilidad (basado en el ATR) que solo se mueva a favor de la tendencia (hacia arriba o hacia abajo), pero que nunca retroceda? |
| **7/10** | Mejorar | [Bollinger Squeeze](indicators/es/BollingerSqeeze.md) | *-* | ¿Se está comprimiendo la volatilidad del precio (Bollinger) dentro de la volatilidad de su rango medio (Keltner), señalando una 'compresión' (squeeze) y un potencial movimiento explosivo? |
| **7/10** | ✅ Conservar | [Force Index](indicators/es/ForceIndex.md) | *-* | ¿Cuál es la fuerza de un movimiento (Volumen * (Cierre - CierreAnterior)), con suavizado opcional? |
| **7/10** | ✅ Conservar | [Historical Volatility Ratio](indicators/es/HVR.md) | *-* | ¿Está el mercado 'comprimido' (HVR<1) o 'explotando' (HVR>1) en relación con su volatilidad histórica? |
| **7/10** | Mejorar | [Money Flow Index (MFI)](indicators/es/MFI.md) | *-* | ¿Cuál es la presión de compra/venta (RSI ponderado por volumen) basada en el precio típico? |
| **7/10** | Mejorar | [OBV](indicators/es/OBV.md) | *-* | ¿Cuál es el flujo de volumen acumulado (presión de compra/venta) basado en el cierre de velas? |
| **7/10** | ✅ Conservar | [Volatility - Chaikins](indicators/es/VolatilityChaikins.md) | *-* | ¿Se está expandiendo o contrayendo el rango de precios (volatilidad) respecto al pasado reciente? |
| **7/10** | ✅ Conservar | [Williams Accumulation / Distribution (WAD)](indicators/es/WAD.md) | *-* | ¿Se está acumulando o distribuyendo el activo (basado en la presión de cierre)? |
| **6.5/10** | Mejorar | [Bid Ask](indicators/es/BidAsk.md) | *-* | ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela? |
| **6.5/10** | Mejorar | [Bollinger Bands Bandwidth](indicators/es/BollingerBandsBandwidth.md) | *-* | ¿Cómo de 'comprimida' (squeeze) o 'expandida' está la volatilidad ahora mismo, medida como un porcentaje del precio medio? |
| **6.5/10** | ✅ Conservar | [Arms Ease of Movement](indicators/es/EMV.md) | *-* | ¿Es el movimiento del precio (cambio en el punto medio) eficiente en relación con su volumen y rango? |
| **6/10** | Mejorar | [Bollinger Bands Percentage](indicators/es/BollingerBandsPercent.md) | *-* | ¿En qué posición (como un porcentaje normalizado) se encuentra el precio actual dentro de las Bandas de Bollinger? |
| **6/10** | 💀 Descartar | [Bollinger Squeeze 3](indicators/es/BollingerSqeezeV3.md) | *-* | ¿Está la volatilidad del precio (StdDev) actualmente mayor o menor que la volatilidad del rango de las velas (ATR)? |
| **6/10** | Mejorar | [Market Facilitation Index](indicators/es/MarketFacilitation.md) | *-* | ¿Cuál es la eficiencia del mercado (MFI) para mover el precio en relación con el volumen? |
| **6/10** | ✅ Conservar | [Q Stick](indicators/es/QStick.md) | *-* | ¿Cuál es el promedio móvil de la distancia entre apertura y cierre de las velas? |
| **6/10** | Mejorar | [Starc Bands](indicators/es/StarcBands.md) | *-* | ¿Dónde están los límites de volatilidad basados en el rango medio verdadero (ATR) alrededor de la media? |
| **5/10** | 💀 Descartar | [Bar Range](indicators/es/BarRange.md) | *-* | ¿Cuál es el rango (Máximo - Mínimo) de cada vela, y cuál ha sido el rango más alto de las últimas X velas? |
| **5/10** | 💀 Descartar | [Bill Williams Moving Average](indicators/es/BWMA.md) | *-* | ¿Cuál es el precio promedio exponencial (EMA), que da más peso a las velas más recientes? |
| **4/10** | 💀 Descartar | [Average Candle Range](indicators/es/ACR.md) | *-* | ¿Cuál es el tamaño promedio de una vela en lo que va de día? |
| **4/10** | Reparar | [Volatility - Historical](indicators/es/VolatilityHist.md) | *-* | ¿Cuál es la volatilidad estadística histórica basada en los retornos logarítmicos? |
| **3/10** | 💀 Descartar | [ATR Normalized](indicators/es/ATRN.md) | *-* | ¿Cuál es la volatilidad (ATR) del instrumento como un porcentaje de su precio actual? |
| **3/10** | 💀 Descartar | [Chaikin Oscillator](indicators/es/ChaikinOscillator.md) | *-* | '¿Cuál es el *momentum* del flujo de dinero (Línea AD)? ¿Está el flujo' de dinero (acumulación/distribución) acelerando o frenando? |
| **3/10** | Reparar | [Herrick Payoff Index (HPI)](indicators/es/HerrickPayoff.md) | *-* | ¿Cuál es la fuerza del movimiento (Precio + Volumen + Open Interest)? (Implementación Rota) |
| **2/10** | 💀 Descartar | [Accumulation/Distribution (A/D)](indicators/es/AD.md) | *-* | ¿El flujo de volumen acumulado está confirmando la tendencia del precio, o está mostrando una divergencia? |
| **2/10** | 💀 Descartar | [Chaikin Money Flow](indicators/es/CMF.md) | *-* | '¿Está entrando o saliendo dinero del activo? (Mide el volumen ponderado' por la posición del cierre en el rango de la vela). |
| **2/10** | 💀 Descartar | [COT High/Low](indicators/es/CotHigh.md) | *-* | '(Teóricamente) Acumula el delta desde un nuevo máximo (High) o mínimo' (Low), pero la lógica está rota. |
| **2/10** | Reparar | [Demand Index](indicators/es/Demand.md) | *-* | ¿Cuál es la presión de compra o venta relativa basada en precio y volumen? |
| **1/10** | 💀 Descartar | [Accumulation / Distribution Flow](indicators/es/ADF.md) | *-* | ¿Cuál es la _tendencia suavizada (lenta)_ del flujo de volumen acumulado? |
| **1/10** | 💀 Descartar | [Bands / Envelope](indicators/es/BandsEnvelope.md) | *-* | ¿Cuán lejos puede moverse el precio (en Ticks Fijos, Valor Fijo o Porcentaje Fijo) antes de considerarse 'sobre-extendido'? |
| **1/10** | 💀 Descartar | [Chaikin Money Oscillator](indicators/es/CMO.md) | *-* | 'Mide la "aceleración" del flujo de dinero (AD) usando la diferencia' entre dos EMAs (pero la implementación es errónea). |
| **1/10** | 💀 Descartar | [Dispersion](indicators/es/Dispersion.md) | *-* | '¿Está el precio 'pegado' a su media (comprimido) o 'explotando' lejos' de ella (volátil)? |
