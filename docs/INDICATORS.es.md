# Índice de Indicadores

Bienvenidos a la guía de indicadores del repositorio. El objetivo principal de esta documentación es **aclarar el propósito y uso de cada indicador** que se encuentra aquí, ya sea en su versión original del repositorio oficial de ATAS o como una modificación personal.

El enfoque principal de análisis y las puntuaciones están **orientados al scalping en el S&P 500 (ES/MES).**

**Nota Importante**: Las valoraciones, puntuaciones ("Score") y veredictos ("Veredicto") son **opiniones subjetivas** basadas en mi experiencia y el apoyo de herramientas de IA (como Gemini y ChatGPT).

Para compilar esta información, se ha utilizado la documentación oficial de ATAS, el código fuente de los repositorios.

---

## 🧭 Navegación Rápida
**[A](#a) | [B](#b) | [C](#c) | [D](#d) | [E](#e) | [F](#f) | [G](#g) | [H](#h) | [I](#i) | [J](#j) | [K](#k) | [L](#l) | [M](#m) | [N](#n) | [O](#o) | [P](#p) | [Q](#q) | [R](#r) | [S](#s) | [T](#t) | [U](#u) | [V](#v) | [W](#w) | [X](#x) | [Y](#y) | [Z](#z)**

---

| `DividedByPrice.cs` | [1 Divided by Price](indicators/es/DividedByPrice.md) | Price | 1/10 | Descartar | ¿Cuál es el gráfico de precios invertido (1 / Precio)? |
## A

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Absorption.cs` | [Absorption](indicators/es/Absorption.md) | OrderFlow | 10/10 | Conservar | ¿En qué niveles de precio se frenó el mercado a pesar de una gran agresión de volumen? |
| `ACDC.cs` | [AC DC Histogram](indicators/es/ACDC.md) | Momentum | 2/10 | Descartar | ¿Cuál es la dirección _suavizada y con retardo_ de la aceleración del mercado? |
| `AccountInfoDisplay.cs` | [Account Info Display](indicators/es/AccountInfoDisplay.md) | Utilidad | 7/10 | Mejorar | ¿Cuál es el estado de mi cuenta (Balance, PnL, Margen) en tiempo real, sin tener que apartar la vista del gráfico? |
| `ADF.cs` | [Accumulation / Distribution Flow](indicators/es/ADF.md) | Volumen clásico | 1/10 | Descartar | ¿Cuál es la _tendencia suavizada (lenta)_ del flujo de volumen acumulado? |
| `AD.cs` | [Accumulation/Distribution (A/D)](indicators/es/AD.md) | Volumen clásico | 2/10 | Descartar | ¿El flujo de volumen acumulado está confirmando la tendencia del precio, o está mostrando una divergencia? |
| `ActiveVolume.cs` | [Active Volume](indicators/es/ActiveVolume.md) | Order Flow | 8/10 | Mejorar | Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios? |
| `AdaptativeBigTrades.cs` | [Adaptive Big Trades](indicators/es/AdaptiveBigTrades.md) | OrderFlow | 9/10 | Conservar | ¿Dónde están las operaciones grandes relativas a la liquidez actual (sin configurar filtros fijos)? |
| `AdaptiveBinaryWaveMA.cs` | [Adaptive Binary Wave](indicators/es/AdaptativeBinaryWaveMA.md) | Tendencia | 7/10 | Mejorar | ¿Ha roto la media móvil adaptativa (AMA) su 'canal' reciente por una cantidad estadísticamente significativa? |
| `AMA.cs` | [Adaptive Moving Average](indicators/es/AMA.md) | Tendencia | 7/10 | Mejorar | ¿Cómo puedo obtener una media móvil suave que _no_ tenga retardo (lag) durante una ruptura fuerte, pero _sí_ filtre el 'ruido' en un mercado lateral? |
| `AdaptiveRsiAverage.cs` | [Adaptive RSI Moving Average](indicators/es/AdaptativeRSIAverage.md) | Tendencia | 4/10 | Descartar | ¿Cómo puedo obtener una media móvil que automáticamente se ralentice cuando el mercado está indeciso (RSI cerca de 50) y se acelere para capturar tendencias cuando el momentum es fuerte (RSI cerca de 0 o 100)? |
| `ADR.cs` | [ADR](indicators/es/ADR.md) | Volatilidad | 7/10 | Mejorar | ¿Cuál es el rango de movimiento "normal" o "promedio" para este instrumento en una sesión, y dónde se proyectarían esos límites hoy? |
| `ADX.cs` | [ADX](indicators/es/ADX.md) | Tendencia | 6/10 | Mejorar | ¿Está el mercado en una fuerte tendencia (alcista o bajista), o simplemente está 'oscilando' lateralmente? |
| `ADXR.cs` | [ADXR](indicators/es/ADXR.md) | Tendencia | 3/10 | Descartar | ¿Cuál es la fuerza _estable y suavizada_ de la tendencia, ignorando el ruido a corto plazo del propio ADX? |
| `Alligator.cs` | [Alligator](indicators/es/Alligator.md) | Tendencia | 6/10 | Conservar | ¿Está el mercado 'durmiendo' (en rango, con las medias entrelazadas) o está 'despierto y comiendo' (en tendencia, con las medias abiertas)? |
| `EMV.cs` | [Arms Ease of Movement](indicators/es/EMV.md) | Volume | 6.5/10 | Conservar | ¿Es el movimiento del precio (cambio en el punto medio) eficiente en relación con su volumen y rango? |
| `AroonIndicator.cs` | [Aroon Indicator](indicators/es/AroonIndicator.md) | Momentum | 3/10 | Descartar | ¿La fortaleza del mercado proviene de haber hecho recientemente nuevos máximos, o de haber hecho recientemente nuevos mínimos? |
| `AroonOscillator.cs` | [Aroon Oscillator](indicators/es/AroonOscillator.md) | Momentum | 3/10 | Descartar | ¿Qué fuerza es más fuerte y reciente, la que está creando nuevos máximos (AroonUp) o la que está creando nuevos mínimos (AroonDown)? |
| `AskBidBars.cs` | [Ask/Bid Volume Difference Bars](indicators/es/AskBidBars.md) | Order Flow | 6.5/10 | Mejorar | ¿Cuál fue el volumen agresivo neto (Delta) de esta vela, e igualmente importante, cuál fue el rango interno (Delta Máx/Mín) de la batalla entre compradores y vendedores dentro de esa vela? |
| `ATR.cs` | [ATR](indicators/es/ATR.md) | Volatilidad | 8/10 | Mejorar | ¿Cuál ha sido el tamaño verdadero promedio (incluyendo gaps) de cada barra durante los últimos X períodos? |
| `ATRN.cs` | [ATR Normalized](indicators/es/ATRN.md) | Volatilidad | 3/10 | Descartar | ¿Cuál es la volatilidad (ATR) del instrumento como un porcentaje de su precio actual? |
| `ACR.cs` | [Average Candle Range](indicators/es/ACR.md) | Volatilidad | 4/10 | Descartar | ¿Cuál es el tamaño promedio de una vela en lo que va de día? |
| `AverageDelta.cs` | [Average Delta](indicators/es/AverageDelta.md) | Order Flow | 6.5/10 | Mejorar | ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas, suavizando el ruido de vela a vela? |
| `AveragePriceBar.cs` | [Average Price for Bar](indicators/es/AveragePriceBar.md) | Precio | 2/10 | Descartar | ¿En lugar de solo el 'Cierre', cuál es el precio promedio interno (ej. Mediana, Típico) de cada vela individual? |
| `AO.cs` | [Awesome Oscillator](indicators/es/AO.md) | Momentum | 2/10 | Descartar | ¿Está el momentum reciente a corto plazo (5 barras) ganando la batalla contra el momentum de la tendencia a largo plazo (34 barras)? |

---

## B

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Logo.cs` | [Background Picture](indicators/es/Logo.md) | Visualization | 2/10 | Conservar | ¿Cómo puedo superponer una imagen o logo personalizado en el gráfico? |
| `BOP.cs` | [Balance of Power](indicators/es/BOP.md) | Momentum / Price action | 5/10 | Descartar | ¿Cuál es, en promedio, la fuerza del cuerpo de la vela (Cierre vs. Apertura) en relación con su rango total (Máximo vs. Mínimo)? |
| `BandsEnvelope.cs` | [Bands / Envelope](indicators/es/BandsEnvelope.md) | Volatilidad | 1/10 | Descartar | ¿Cuán lejos puede moverse el precio (en Ticks Fijos, Valor Fijo o Porcentaje Fijo) antes de considerarse 'sobre-extendido'? |
| `BarDifference.cs` | [Bar Difference](indicators/es/BarDifference.md) | Momentum | 3/10 | Descartar | ¿Cuántos ticks ha subido o bajado el precio (Cierre) en comparación con hace X velas? |
| `BarNumbering.cs` | [Bar Numbering](indicators/es/BarNumbering.md) | Utilidad / Visualización | 4/10 | Descartar | ¿Cuántas velas han pasado desde el inicio de la sesión, y puedes etiquetarlas cada X velas, por favor? |
| `BarRange.cs` | [Bar Range](indicators/es/BarRange.md) | Volatilidad | 5/10 | Descartar | ¿Cuál es el rango (Máximo - Mínimo) de cada vela, y cuál ha sido el rango más alto de las últimas X velas? |
| `BarTimer.cs` | [Bar Timer](indicators/es/BarTimer.md) | Utilidad / Visualización | 8/10 | Mejorar | ¿Cuánto tiempo (o ticks/volumen) le queda a esta vela, y puedes avisarme 3 segundos antes de que cierre? |
| `BarVolumeFilter.cs` | [Bar's Volume Filter](indicators/es/BarVolumeFilter.md) | Order Flow | 7/10 | Mejorar | ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks (ej. 'Volumen > 1500' y solo 'dentro de la sesión RTH')? |
| `BarsPattern.cs` | [Bars Pattern](indicators/es/BarsPattern.md) | Order Flow / Utilidad | 9/10 | Mejorar | ¿Qué velas de este gráfico cumplen _todos_ mis criterios específicos y multicapa para un setup de alta calidad (Volumen, Delta, forma de vela, etc.)? |
| `BidAsk.cs` | [Bid Ask](indicators/es/BidAsk.md) | Order Flow | 6.5/10 | Mejorar | ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela? |
| `BidAskVR.cs` | [Bid Ask Volume Ratio](indicators/es/BidAskVR.md) | Order Flow | 7/10 | Mejorar | ¿Cuál es el desequilibrio normalizado (de -100% a +100%) del volumen agresivo, y cuál es el momentum (pendiente) de ese desequilibrio? |
| `BigTrades.cs` | [Big Trades (Final Fix)](indicators/es/BigTrades.md) | OrderFlow | 9/10 | Conservar | ¿Dónde están entrando los agresores institucionales (bloques grandes de compra/venta)? |
| `ACBW.cs` | [Bill Williams AC](indicators/es/ACBW.md) | Momentum | 1/10 | Descartar | ¿El momentum (AO) está acelerando o frenando? |
| `BWMA.cs` | [Bill Williams Moving Average](indicators/es/BWMA.md) | Tendencia / Volatilidad | 5/10 | Descartar | ¿Cuál es el precio promedio exponencial (EMA), que da más peso a las velas más recientes? |
| `BionicCandle.cs` | [Bionic Candle (Clean)](indicators/es/BionicCandle.md) | Visualization | 8/10 | Conservar | ¿Cómo visualizar la estructura interna de fuerza y rechazo de la vela eliminando el ruido visual? |
| `BlockMA.cs` | [Block Moving Average](indicators/es/BlockMA.md) | Tendencia / Volatilidad | 7/10 | Mejorar | ¿Cómo puedo crear un 'trailing stop' de volatilidad (basado en el ATR) que solo se mueva a favor de la tendencia (hacia arriba o hacia abajo), pero que nunca retroceda? |
| `BollingerBands.cs` | [Bollinger Bands](indicators/es/BollingerBands.md) | Volatilidad / Canal | 8/10 | Mejorar | ¿Está el precio actual estadísticamente 'demasiado alto' o 'demasiado bajo' (sobre-extendido) en comparación con su media reciente, basándose en la volatilidad? |
| `BollingerBandsBandwidth.cs` | [Bollinger Bands Bandwidth](indicators/es/BollingerBandsBandwidth.md) | Volatilidad | 6.5/10 | Mejorar | ¿Cómo de 'comprimida' (squeeze) o 'expandida' está la volatilidad ahora mismo, medida como un porcentaje del precio medio? |
| `BollingerBandsPercent.cs` | [Bollinger Bands Percentage](indicators/es/BollingerBandsPercent.md) | Volatilidad | 6/10 | Mejorar | ¿En qué posición (como un porcentaje normalizado) se encuentra el precio actual dentro de las Bandas de Bollinger? |
| `BollingerSqueeze.cs` | [Bollinger Squeeze](indicators/es/BollingerSqeeze.md) | Volatilidad | 7/10 | Mejorar | ¿Se está comprimiendo la volatilidad del precio (Bollinger) dentro de la volatilidad de su rango medio (Keltner), señalando una 'compresión' (squeeze) y un potencial movimiento explosivo? |
| `BollingerSqueezeV2.cs` | [Bollinger Squeeze 2](indicators/es/BollingerSqeezeV2.md) | Volatilidad / Momentum | 8/10 | Mejorar | ¿Está el mercado en compresión (Squeeze) Y cuál es la dirección del momentum (Histograma)? |
| `BollingerSqueezeV3.cs` | [Bollinger Squeeze 3](indicators/es/BollingerSqeezeV3.md) | Volatilidad / Momentum | 6/10 | Descartar | ¿Está la volatilidad del precio (StdDev) actualmente mayor o menor que la volatilidad del rango de las velas (ATR)? |

---

## C

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `CamarillaPivots.cs` | [Camarilla Pivots](indicators/es/CamarillaPivots.md) | Niveles | 8/10 | Mejorar | ¿Dónde están los niveles de soporte y resistencia intradía más relevantes, basados en la fórmula de Camarilla, para operar rupturas (en L4/H4) y reversiones (en L3/H3)? |
| `CandleStatistics.cs` | [Candle Statistics](indicators/es/CandleStatistics.md) | Utilidad / Visualización | 8/10 | Mejorar | ¿Cuál es la "radiografía" de esta vela? ¿Cuál fue su Volumen total, su Delta neto, su número de Ticks (trades) y su Duración? |
| `CCI.cs` | [CCI](indicators/es/CCI.md) | Momentum | 7/10 | Mejorar | ¿Qué tan lejos se ha desviado el precio "típico" de hoy de su precio "promedio", medido en unidades de desviación estadística? |
| `CMF.cs` | [Chaikin Money Flow](indicators/es/CMF.md) | Volumen | 2/10 | Descartar | '¿Está entrando o saliendo dinero del activo? (Mide el volumen ponderado' por la posición del cierre en el rango de la vela). |
| `CMO.cs` | [Chaikin Money Oscillator](indicators/es/CMO.md) | Volume | 1/10 | Descartar | 'Mide la "aceleración" del flujo de dinero (AD) usando la diferencia' entre dos EMAs (pero la implementación es errónea). |
| `ChaikinOscillator.cs` | [Chaikin Oscillator](indicators/es/ChaikinOscillator.md) | Momentum / Volumen clásico | 3/10 | Descartar | '¿Cuál es el *momentum* del flujo de dinero (Línea AD)? ¿Está el flujo' de dinero (acumulación/distribución) acelerando o frenando? |
| `CFO.cs` | [Chande Forecast Oscillator](indicators/es/CFO.md) | Momentum | 4/10 | Descartar | ¿Cuán lejos, en porcentaje, se ha desviado el precio de su propia línea de tendencia (Regresión Lineal)? |
| `ChandeMomentum.cs` | [Chande Momentum Oscillators](indicators/es/ChandeMomentum.md) | Momentum | 5/10 | Mejorar | '¿Cuál es la fuerza neta del impulso (Suma de Subidas vs. Suma de' Bajadas), expresada como un oscilador centrado en cero? |
| `CMS.cs` | [Clear Method Swing Line](indicators/es/CMS.md) | Tendencia | 8/10 | Conservar | '¿Cuál es la estructura de mercado (swing highs/lows) objetiva y actual,' sin subjetividad? |
| `ClusterConstructorLite.cs` | [Cluster Constructor Lite](indicators/es/ClusterConstructorLite.md) | OrderFlow | 8/10 | Conservar | ¿Existen patrones anómalos de volumen (ej. doble núcleo) dentro de la estructura de la vela? |
| `ClusterSearchModif.cs` | [Cluster Search Modifar](indicators/es/ClusterSearchModif.md) | Order Flow | 10/10 | Conservar | '¿Qué clústeres de precio específicos en este gráfico cumplen *todos*' mis criterios de filtro (por Volumen, Delta, Localización, Imbalance, etc.)? |
| `ClusterStatisticModif.cs` | [Cluster Statistic Modif](indicators/es/ClusterStatisticModif.md) | Order FLow | 10/10 | Conservar | ¿Cuál es el "dashboard" estadístico completo (Volumen, Delta, Ticks, *Velocidad* e *Imbalances*) de cada vela, y cómo se compara cada vela con la más "fuerte" del gráfico? |
| `ColorBarHighLow.cs` | [Color Bar HH/LL](indicators/es/ColorBarHighLow.md) | Tendencia | 3/10 | Descartar | 'Colorea las velas si hacen un nuevo máximo (HH) o mínimo (LL) en comparación' con la vela inmediatamente anterior. |
| `ColorBarOpenClose.cs` | [Color Bar Open/Close](indicators/es/ColorBarOpenClose.md) | Trend | 1/10 | Descartar | 'Colorea las velas según si el cierre es mayor (alcista) o menor (bajista)' que la apertura. |
| `CBI.cs` | [Connie Brown Composite Index](indicators/es/CBI.md) | Momentum | 5/10 | Descartar | ¿Cuál es el momentum "compuesto" (RSI + Momentum) del precio, y cómo se compara con sus propias medias móviles (lenta y rápida)? |
| `CoppockCurve.cs` | [Coppock Curve](indicators/es/CoppockCurve.md) | Momentum | 3/10 | Descartar | '¿Cuál es el momentum de largo plazo del mercado? (Diseñado para gráficos' semanales/mensuales). |
| `CotHigh.cs` | [COT High/Low](indicators/es/CotHigh.md) | VolumeOrderFlow | 2/10 | Descartar | '(Teóricamente) Acumula el delta desde un nuevo máximo (High) o mínimo' (Low), pero la lógica está rota. |
| `CAV.cs` | [Cumulative Adjusted Value](indicators/es/CAV.md) | Momentum | 5/10 | Descartar | ¿Está el precio *consistente y acumulativamente* cotizando por encima de su media (momentum alcista), o por debajo de ella (momentum bajista)? |
| `CumulativeDailyVolume.cs` | [Cumulative Daily Volume](indicators/es/CumulativeDailyVolume.md) | Volume | 6/10 | Conservar | ¿Cuál es el volumen total acumulado desde el inicio de la sesión? |
| `CurrentPrice.cs` | [Current Price](indicators/es/CurrentPrice.md) | Visualization | 3/10 | Descartar | '¿Cuál es el último precio y la hora actual, mostrados directamente en' el gráfico? |
| `CumulativeDelta.cs` | [CVD - Cumulative Volume Delta](indicators/es/CumulativeDelta.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Cuál es el delta acumulado (la agresión neta) desde el inicio de la sesión? |
| `MarketPower.cs` | [CVD pro / Market Power](indicators/es/MarketPower.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Cuál es el delta acumulado (CVD) filtrado por tamaño de trade, y cómo se compara con su SMA? |
| `MultiMarketPower.cs` | [CVD pro(multi) / Multi Market Powers](indicators/es/MultiMarketPower.md) | VolumeOrderFlow | 10/10 | Conservar | ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional)? |

---

## D

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `DailyChange.cs` | [Daily Change](indicators/es/DailyChange.md) | Visualization | 6/10 | Conservar | ¿Cuál es la variación neta del precio en el día (en %, ticks o $)? |
| `DailyHighLow.cs` | [Daily HighLow](indicators/es/DailyHighLow.md) | Levels | 6/10 | Reparar | '¿Dónde están el Máximo, Mínimo y Mediana del día actual, y la Mediana' del día anterior? |
| `DailyLinesModif.cs` | [Daily Lines Modif](indicators/es/DailyLinesModif.md) | Levels | 9/10 | Conservar | '¿Dónde están los niveles estructurales (OHLC) del día/semana/mes' anterior, y dónde está el "Half Gap" (mitad del hueco) de la apertura de hoy? |
| `DeltaColoredCandles.cs` | [Delta Colored Candles](indicators/es/DeltaColoredCandles.md) | VolumeOrderFlow | 4/10 | Mejorar | '¿Cuál es la intensidad del *momentum* del delta (delta acumulado en N' barras) en relación con un máximo fijo? |
| `DeltaModif.cs` | [Delta Modif](indicators/es/DeltaModif.md) | Order FLow | N/A | N/A | ¿Qué barras muestran una agresión (Delta) extrema, divergencia o absorción, y cómo puedo ver esas señales directamente en el gráfico de precio? |
| `DeltaStrength.cs` | [Delta Strength](indicators/es/DeltaStrength.md) | VolumeOrderFlow | 5/10 | Descartar | '¿Qué velas cierran con un delta que está *casi* en su extremo' (MaxDelta/MinDelta), pero no *exactamente* en él? |
| `DeltaTurnaround.cs` | [Delta Turnaround](indicators/es/DeltaTurnaround.md) | VolumeOrderFlow | 6/10 | Descartar | '¿Se ha producido un patrón de giro de 3 velas (dos en una dirección,' una en la opuesta) confirmado por el delta? |
| `Demand.cs` | [Demand Index](indicators/es/Demand.md) | Volume | 2/10 | Reparar | ¿Cuál es la presión de compra o venta relativa basada en precio y volumen? |
| `DeMarker.cs` | [DeMarker](indicators/es/DeMarker.md) | Momentum | 2/10 | Reparar | '¿Cuáles son las zonas de sobrecompra o sobreventa basadas en la' comparación de máximos y mínimos? (Implementación ROTA) |
| `DOM.cs` | [Depth of Market](indicators/es/DOM.md) | OrderBook | 9/10 | Conservar | ¿Cuál es la liquidez (libro de órdenes) actual, dibujada en el gráfico? |
| `DeTrendedDi.cs` | [Detrended Oscillator - DiNapoli](indicators/es/DeTrendedDi.md) | Momentum | 6/10 | Conservar | ¿Cuál es el ciclo del precio, eliminando la tendencia de una SMA? |
| `DeTrended.cs` | [DeTrended Price Oscillator](indicators/es/DeTrended.md) | Momentum | 3/10 | Descartar | '¿Cuáles son los ciclos de corto plazo eliminando la tendencia general?' (Implementación no estándar) |
| `DIPos.cs` | [DI+ (Directional Indicator Positivo)](indicators/es/DIPos.md) | Tendencia | 3/10 | Descartar | ¿Cuál es la presión compradora relativa? (Componente del sistema ADX/DMI) |
| `DINeg.cs` | [DI- (Directional Indicator Negativo)](indicators/es/DINeg.md) | Tendencia | 3/10 | Descartar | ¿Cuál es la presión vendedora relativa? (Componente del sistema ADX/DMI) |
| `DmIndex.cs` | [Directional Movement Index](indicators/es/DmIndex.md) | Tendencia | 3/10 | Descartar | ¿Cuál es la fuerza direccional (DI+ vs DI-)? (Implementación NO estándar) |
| `DmOscillator.cs` | [Directional Movement Oscillator](indicators/es/DmOscillator.md) | Tendencia | 3/10 | Descartar | ¿Cuál es la diferencia neta (DI+ menos DI-)? (Basado en un DMI no estándar) |
| `Dispersion.cs` | [Dispersion](indicators/es/Dispersion.md) | Volatility | 1/10 | Descartar | '¿Está el precio 'pegado' a su media (comprimido) o 'explotando' lejos' de ella (volátil)? |
| `DOMLevels.cs` | [DOM Heatmap (Manual)](indicators/es/DOMLevels.md) | OrderFlow | 9/10 | Conservar | ¿Cómo ha evolucionado la liquidez del libro de órdenes (Heatmap) a lo largo del tiempo en el gráfico? |
| `DomPowerModif.cs` | [DOM Power Modif](indicators/es/DomPower.md) | OrderBook | 9/10 | Conservar | '¿Cuál es el desequilibrio neto (Bids vs Asks) en el libro de órdenes y' cuál es su rango de volatilidad? |
| `DomStrengthModif.cs` | [DOM Strength Modif](indicators/es/DomStrengthModif.md) | OrderBook | 9/10 | Conservar | '¿Cuál es la fuerza de la agresión (Trades) en relación con la liquidez' pasiva (DOM)? |
| `Donchian.cs` | [Donchian Channel](indicators/es/Donchian.md) | Levels | 8/10 | Conservar | ¿Cuál es el rango de precio (máximo y mínimo) de las últimas N barras? |
| `DEMA.cs` | [Double Exponential Moving Average](indicators/es/DEMA.md) | Tendencia | 6/10 | Conservar | '¿Cuál es el precio suavizado, pero con menos retraso (lag) que una EMA' estándar? |
| `DoubleStochastic.cs` | [Double Stochastic](indicators/es/DoubleStochastic.md) | Momentum | 7/10 | Conservar | '¿Cuál es el indicador Estocástico, pero aplicado por segunda vez sobre' sí mismo para suavizar el ruido? |
| `DoubleStochasticBressert.cs` | [Double Stochastic - Bressert](indicators/es/DoubleStochasticBressert.md) | Momentum | 5/10 | Descartar | '¿Cuál es el indicador Double Stochastic, pero con una capa extra de' suavizado EMA? |
| `DtOscillator.cs` | [DT Oscillator](indicators/es/DtOscillator.md) | Momentum | 7/10 | Conservar | ¿Cuál es el momentum, basado en un StochasticRSI suavizado con dos SMAs? |
| `DX.cs` | [DX (Directional Index)](indicators/es/DX.md) | Tendencia | 3/10 | Descartar | '¿Cuál es la fuerza de la tendencia? (Componente del ADX, implementación' NO estándar) |
| `DynamicLevels.cs` | [Dynamic Levels](indicators/es/DynamicLevels.md) | VolumeOrderFlow | 9/10 | Conservar | '¿Dónde se están formando el POC, VAH y VAL del período actual (ej.' Día, Semana, Hora) en tiempo real? |
| `DynamicLevelsChannel.cs` | [Dynamic Levels Channel](indicators/es/DynamicLevelsChannel.md) | VolumeOrderFlow | 9/10 | Conservar | '¿Dónde se están formando el POC, VAH y VAL de las últimas N barras (un' perfil móvil)? |
| `DMI.cs` | [Dynamic Momentum Index](indicators/es/DMI.md) | Momentum | 5/10 | Reparar | '¿Cuál es el RSI, pero con un periodo que se ajusta automáticamente a la' volatilidad del mercado? |

---

## E

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `ElderRay.cs` | [Elder Ray](indicators/es/ElderRay.md) | Momentum | 6.5/10 | Mejorar | ¿Cuál es la fuerza de los compradores (High - EMA) y vendedores (Low - EMA) en relación con el consenso (EMA)? |
| `EMA.cs` | [EMA](indicators/es/EMA.md) | Trend | 9/10 | Conservar | ¿Cuál es el valor de la media móvil exponencial, coloreada por pendiente, con alertas de proximidad al precio? |
| `Ergodic.cs` | [Ergodic](indicators/es/Ergodic.md) | Momentum | 4/10 | Reparar | ¿Cuál es la diferencia (histograma) entre el True Strength Index (TSI) y su línea de señal? |
| `Exhaustion.cs` | [Exhaustion](indicators/es/Exhaustion.md) | VolumeOrderFlow | 8/10 | Reparar | ¿Está el precio mostrando agotamiento (volumen creciente) en los últimos N ticks del máximo o mínimo de la vela? |
| `ExternalCharts.cs` | [External Chart](indicators/es/ExternalCharts.md) | Visualization | 7.5/10 | Conservar | ¿Cómo se ven las velas de un timeframe superior (ej. H1, H4) superpuestas en mi gráfico actual? |

---

## F

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `FairValueGap.cs` | [Fair Value Gap](indicators/es/FairValueGap.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Dónde están los desequilibrios de precio (gaps) no mitigados en el marco actual y superior? |
| `FisherTransform.cs` | [Fisher Transform](indicators/es/FisherTransform.md) | Momentum | 6.5/10 | Conservar | ¿Cuál es el momentum del precio, normalizado por una transformación estadística de Fisher? |
| `ForceIndex.cs` | [Force Index](indicators/es/ForceIndex.md) | Volume | 7/10 | Conservar | ¿Cuál es la fuerza de un movimiento (Volumen * (Cierre - CierreAnterior)), con suavizado opcional? |
| `Fractals.cs` | [Fractals](indicators/es/Fractals.md) | Levels | 7.5/10 | Conservar | ¿Dónde están los máximos y mínimos fractales (swing) de 5 barras, con líneas opcionales de S/R? |
| `FCV.cs` | [Full Contract Value](indicators/es/FCV.md) | Price | 2/10 | Descartar | (Teórico) ¿Cuál es el valor del precio escalado por un multiplicador personalizado? |

---

## G

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Gaps.cs` | [Gaps](indicators/es/Gaps.md) | Levels | 8.5/10 | Conservar | ¿Dónde están los huecos de precio (Gaps) que superan una desviación mínima relativa al rango promedio? |
| `GreatestSwing.cs` | [Greatest Swing Value](indicators/es/GreatestSwing.md) | Levels | 7/10 | Conservar | ¿Dónde están los niveles S/R dinámicos, proyectados desde el Open actual usando la media de los 'swings de rechazo' anteriores? |
| `GMMA.cs` | [Guppy Multiple Moving Average](indicators/es/GMMA.md) | Trend | 8.5/10 | Conservar | ¿Cuál es la relación (compresión/expansión) entre las 6 EMAs de traders (cortas) y las 6 EMAs de inversores (largas)? |

---

## H

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `HeikenAshi.cs` | [Heiken Ashi](indicators/es/HeikenAshi.md) | Visualization | 6.5/10 | Conservar | ¿Cómo se vería el precio usando velas Heiken Ashi estándar para suavizar la tendencia? |
| `HeikenAshiSmoothed.cs` | [Heiken Ashi Smoothed](indicators/es/HeikenAshiSmoothed.md) | Visualization | 5/10 | Mejorar | ¿Cómo se vería el precio usando velas Heiken Ashi "doblemente suavizadas" (SMMA + HA + WMA)? |
| `HerrickPayoff.cs` | [Herrick Payoff Index (HPI)](indicators/es/HerrickPayoff.md) | Volume | 3/10 | Reparar | ¿Cuál es la fuerza del movimiento (Precio + Volumen + Open Interest)? (Implementación Rota) |
| `Highest.cs` | [Highest](indicators/es/Highest.md) | Levels | 6/10 | Descartar | ¿Cuál es el valor más alto del "Source" (por defecto, el Cierre) de las últimas N barras? |
| `HighLow.cs` | [Highest High / Lowest Low Over N Bars](indicators/es/HighLow.md) | Levels | 7/10 | Conservar | ¿Cuál es el rango de precio (máximo más alto y mínimo más bajo) de las últimas N barras? |
| `HVR.cs` | [Historical Volatility Ratio](indicators/es/HVR.md) | Volatility | 7/10 | Conservar | ¿Está el mercado 'comprimido' (HVR<1) o 'explotando' (HVR>1) en relación con su volatilidad histórica? |
| `HRanges.cs` | [HRanges](indicators/es/HRanges.md) | Volume | 8.5/10 | Conservar | ¿Dónde se están formando rangos (consolidaciones) y cuál es el POC interno de esos rangos, filtrado por volumen y duración? |
| `HMA.cs` | [Hull Moving Average](indicators/es/HMA.md) | Trend | 8/10 | Conservar | ¿Cuál es el valor de la media móvil de Hull (HMA), una media rápida y de bajo lag coloreada por pendiente? |
| `HurstExponent.cs` | [Hurst Exponent](indicators/es/HurstExponent.md) | Statistical | 8/10 | Conservar | ¿El comportamiento del mercado es tendencial (persistente, H>0.5), de reversión (antipersistente, H<0.5) o aleatorio (H=0.5)? |

---

## I

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Ichimoku.cs` | [Ichimoku Kinko Hyo](indicators/es/Ichimoku.md) | Trend | 9/10 | Conservar | ¿Cuál es el estado de la tendencia y el equilibrio del mercado según el sistema Ichimoku (Nube, Tenkan, Kijun, Chikou)? |
| `ImbalanceRatio.cs` | [Imbalance Ratio](indicators/es/ImbalanceRatio.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Dónde se están produciendo desequilibrios (imbalances) diagonales de Ask vs. Bid en el clúster que superan un ratio y volumen mínimos? |
| `Inertia.cs` | [Inertia](indicators/es/Inertia.md) | Momentum | 6.5/10 | Descartar | ¿Cuál es el momentum del RVI, suavizado por una Regresión Lineal? |
| `Inertia2.cs` | [Inertia V2](indicators/es/Inertia2.md) | Momentum | 7/10 | Conservar | ¿Cuál es el momentum, basado en un RVI (calculado sobre StdDev) y suavizado por una Regresión Lineal? |
| `InitialBalanceModif.cs` | [Initial Balance Modif](indicators/es/InitialBalanceModif.md) | Levels | 9/10 | Conservar | ¿Cuáles son el rango de apertura (IB) y sus expansiones proyectadas (IBHX/IBLX) para la sesión actual? |
| `InsideEqualsBar.cs` | [Inside Bar](indicators/es/InsideEqualsBar.md) | Levels | 7.5/10 | Conservar | ¿Dónde se están formando patrones de "Inside Bar" (compresión), con tolerancia y definición de área (Cuerpo o Mechas) personalizables? |
| `FisherTransformInverse.cs` | [Inverse Fisher Transform](indicators/es/FisherTransformInverse.md) | Momentum | 6.5/10 | Conservar | ¿Cuál es el momentum del precio, suavizado y normalizado por una transformación inversa de Fisher? |
| `FisherTransformInverseRsi.cs` | [Inverse Fisher Transform with RSI](indicators/es/FisherTransformInverseRsi.md) | Momentum | 6.5/10 | Conservar | ¿Cuál es el momentum, basado en un RSI suavizado y normalizado por una transformación inversa de Fisher? |

---

## K

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `KAMA.cs` | [Kaufman Adaptive Moving Average](indicators/es/KAMA.md) | Trend | 7/10 | Conservar | ¿Cuál es el valor de la media móvil adaptativa (KAMA), que se acelera en tendencias y se frena en rangos? |
| `KdFast.cs` | [KD - Fast](indicators/es/KdFast.md) | Momentum | 6/10 | Conservar | ¿Cuál es el valor del oscilador Estocástico Rápido (%K) y su media móvil (%D)? |
| `KdSlow.cs` | [KD - Slow](indicators/es/KdSlow.md) | Momentum | 6/10 | Reparar | ¿Cuál es el valor del oscilador Estocástico Lento (%K Lento = SMA(%K Rápido), %D Lento = SMA(%D Rápido))? |
| `KDJ.cs` | [KDJ](indicators/es/KDJ.md) | Momentum | 5/10 | Reparar | ¿Cuál es el valor del Estocástico Lento (%K, %D) más la línea de señal %J (3*%K - 2*%D)? |
| `KeltnerChannel.cs` | [Keltner Channel](indicators/es/KeltnerChannel.md) | Volatility | 7.5/10 | Reparar | ¿Dónde se sitúan las bandas de volatilidad (SMA +/- ATR * Multiplicador) y el precio se está aproximando a ellas? |
| `Kurtosis.cs` | [Kurtosis](indicators/es/Kurtosis.md) | Statistical | 5/10 | Conservar | ¿Cuál es la "pesadez de las colas" (Kurtosis) de la distribución de precios, para medir la frecuencia de eventos extremos? |

---

## L

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `LevelsLolo.cs` | [LevelsLolo](indicators/es/LevelsLolo.md) | Levels | 9/10 | Conservar | ¿Dónde están los niveles clave externos (ej. SpotGamma) y cuál es su jerarquía de importancia (Rank, Tipo, 0DTE)? |
| `LinearReg.cs` | [Linear Regression](indicators/es/LinearReg.md) | Trend | 5/10 | Mejorar | ¿Cuál es la línea de tendencia de regresión lineal (mínimos cuadrados) de las últimas N barras? |
| `LinRegChannel.cs` | [Linear Regression Channel](indicators/es/LinRegChannel.md) | Trend | 3/10 | Reparar | ¿Cuál es el canal de tendencia dominante (regresión lineal) y dónde están sus límites de desviación estándar? |
| `LinRegSlope.cs` | [Linear Regression Slope](indicators/es/LinRegSlope.md) | Trend | 1/10 | Reparar | ¿Cuál es la pendiente (dirección e intensidad) de la tendencia en el período reciente? |
| `Lowest.cs` | [Lowest](indicators/es/Lowest.md) | Level | 6/10 | Conservar | ¿Cuál es el valor más bajo (mínimo) en las últimas N barras? |

---

## M

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `MACD.cs` | [MACD](indicators/es/MACD.md) | Momentum | 8/10 | Mejorar | ¿Cuál es la convergencia o divergencia entre las medias móviles de corto y largo plazo? |
| `MacdVW.cs` | [MACD - Volume Weighted](indicators/es/MacdVW.md) | Momentum | 8/10 | Mejorar | ¿Cuál es la convergencia/divergencia entre dos medias ponderadas por volumen (VWMAs)? |
| `MacdBbImproved.cs` | [MACD Bollinger Bands - Improved](indicators/es/MacdBbImproved.md) | Momentum | 6/10 | Mejorar | ¿Cuál es el rango de volatilidad "mejorado" (BB + SMA del histograma) alrededor de la línea de señal del MACD? |
| `MacdBbStandart.cs` | [MACD Bollinger Bands - Standard](indicators/es/MacdBbStandart.md) | Momentum | 6/10 | Conservar | ¿Cuál es el rango de volatilidad (Bollinger Bands) alrededor de la línea de señal del MACD? |
| `MacdCloud.cs` | [MACD Cloud](indicators/es/MacdCloud.md) | Trend | 7/10 | Conservar | ¿Cuál es la tendencia visual definida por el cruce de dos medias ponderadas (WMA)? |
| `MacdLeader.cs` | [MACD Leader](indicators/es/MacdLeader.md) | Momentum | 7/10 | Conservar | ¿Cuál es una versión "adelantada" del MACD que incorpora la diferencia entre el precio y sus EMAs? |
| `MarginZones.cs` | [Margin zones](indicators/es/MarginZones.md) | Level | 3/10 | Reparar | ¿Dónde están los niveles de margen clave (25%, 50%, 100%...) calculados desde el extremo semanal o un precio fijo? |
| `MarketFacilitation.cs` | [Market Facilitation Index](indicators/es/MarketFacilitation.md) | Volume | 6/10 | Mejorar | ¿Cuál es la eficiencia del mercado (MFI) para mover el precio en relación con el volumen? |
| `MaxLevels.cs` | [Maximum Levels](indicators/es/MaxLevels.md) | VolumeOrderFlow | 9/10 | Conservar | ¿En qué nivel de precio se produjo el máximo Volumen (o Bid, Ask, Delta) para el período seleccionado? |
| `McClellanOscillator.cs` | [McClellan Oscillator](indicators/es/McClellanOscillator.md) | Momentum | 6/10 | Conservar | ¿Cuál es la diferencia entre la EMA rápida y la EMA lenta (impulso de mercado)? |
| `MSI.cs` | [McClellan Summation Index](indicators/es/MSI.md) | Momentum | 7/10 | Mejorar | ¿Cuál es la suma acumulada del Oscilador McClellan (amplitud de mercado a largo plazo)? |
| `MeanDeviation.cs` | [Mean Deviation](indicators/es/MeanDeviation.md) | Statistical | 5/10 | Mejorar | ¿Cuál es la desviación media absoluta (volatilidad) del precio respecto a su media simple? |
| `Momentum.cs` | [Momentum](indicators/es/Momentum.md) | Momentum | 7/10 | Conservar | ¿Cuál es la diferencia de precio (velocidad) entre la barra actual y la de hace N periodos? |
| `MomentumTrend.cs` | [Momentum Trend](indicators/es/MomentumTrend.md) | Momentum | 3/10 | Mejorar | ¿Está el momentum aumentando o disminuyendo vela a vela (visualizado como puntos)? |
| `MFI.cs` | [Money Flow Index (MFI)](indicators/es/MFI.md) | Volume | 7/10 | Mejorar | ¿Cuál es la presión de compra/venta (RSI ponderado por volumen) basada en el precio típico? |
| `MovingAverage.cs` | [Moving Average](indicators/es/MovingAverage.md) | Trend | 9/10 | Conservar | ¿Cuál es la tendencia suavizada del precio usando uno de los 11 tipos de medias disponibles? |
| `MaDifference.cs` | [Moving Average Difference](indicators/es/MaDifference.md) | Momentum | 6/10 | Mejorar | ¿Cuál es la diferencia (momentum) entre dos medias móviles y está acelerando o desacelerando? |
| `MaEnvelope.cs` | [Moving Average Envelope](indicators/es/MaEnvelope.md) | Level | 5/10 | Conservar | ¿Cuál es el canal de precios (fijo o porcentual) alrededor de una media móvil simple? |
| `MMed.cs` | [Moving Median](indicators/es/MMed.md) | Statistical | 6/10 | Mejorar | ¿Cuál es la mediana (valor central) de los precios en el período reciente? |
| `MurrayMath.cs` | [Murrey Math](indicators/es/MurrayMath.md) | Level | 8/10 | Conservar | ¿Cuáles son los niveles de soporte y resistencia armónicos basados en la teoría de Murrey Math? |
| `MutualFundBars.cs` | [Mutual Fund Bars](indicators/es/MutualFundBars.md) | Visualization | 4/10 | Conservar | ¿Cómo se vería el gráfico si cada vela abriera exactamente al cierre de la anterior (estilo fondo mutuo)? |

---

## O

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `OBV.cs` | [OBV](indicators/es/OBV.md) | Volume | 7/10 | Mejorar | ¿Cuál es el flujo de volumen acumulado (presión de compra/venta) basado en el cierre de velas? |
| `OHLCPlusModif.cs` | [OHLC Plus Modif](indicators/es/OHLCPlus.md) | VolumeOrderFlow | 10/10 | Conservar | ¿Puedo tener TODOS los niveles de contexto clave (Diario, Semanal, Mensual, Contrato) en un solo indicador, con estilos profesionales y un sistema de etiquetas que no se solapen? |
| `OIAnalyzer.cs` | [OI Analyzer](indicators/es/OIAnalyzer.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle? |
| `BalanceOI.cs` | [On Balance Open Interest](indicators/es/BalanceOI.md) | Order Flow | 8/10 | Mejorar | ¿Está el compromiso acumulado del 'dinero inteligente' (Interés Abierto) subiendo cuando los precios suben y bajando cuando los precios bajan, o está divergiendo? |
| `OpenInterest.cs` | [Open Interest](indicators/es/OpenInterest.md) | Volume | 8/10 | Conservar | ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión? |
| `OpenLine.cs` | [Open Line](indicators/es/OpenLine.md) | Level | 8/10 | Conservar | ¿Dónde está el precio de apertura de la sesión actual (o personalizada) y ha sido tocado ya? |
| `OrderBlock.cs` | [Order Block](indicators/es/OrderBlock.md) | Level | 9/10 | Conservar | ¿Dónde están los bloques de órdenes institucionales (zonas de oferta/demanda no mitigadas) basados en la estructura de swings? |
| `OrderBookAlerts.cs` | [Order Book Alerts](indicators/es/OrderBookAlerts.md) | OrderBook | 9/10 | Conservar | ¿Dónde hay muros de liquidez en el DOM que superan un cierto tamaño y persisten en el tiempo? |
| `OrderFlow.cs` | [Order Flow Indicator](indicators/es/OrderFlow.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Cómo se visualiza el flujo de órdenes (trades individuales o acumulados) en el gráfico (círculos/rectángulos)? |
| `OrderFlowRythm.cs` | [Order Flow Rhythm (Clean)](indicators/es/OrderFlowRythm.md) | OrderFlow | 9/10 | Conservar | ¿Cuál es la velocidad de ejecución (ritmo) del mercado visualizada como mapa de calor? |
| `OSMA.cs` | [OSMA (Moving Average of Oscillator)](indicators/es/OSMA.md) | Momentum | 7/10 | Mejorar | ¿Cuál es la diferencia entre el MACD y su línea de señal (el histograma MACD)? |
| `OutsideBar.cs` | [Outside Bar](indicators/es/OutsideBar.md) | PriceAction | 6/10 | Mejorar | ¿Es la barra actual una "Outside Bar" (engloba completamente a la anterior)? |

---

## P

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `ParabolicSAR.cs` | [Parabolic SAR](indicators/es/ParabolicSAR.md) | Trend | 8/10 | Mejorar | ¿Cuál es el nivel de stop dinámico (trailing stop) basado en precio y tiempo (aceleración)? |
| `PercentagePrice.cs` | [Percentage Price Oscillator](indicators/es/PercentagePrice.md) | Momentum | 7/10 | Mejorar | ¿Cuál es la diferencia porcentual entre dos medias móviles (MACD normalizado)? |
| `PinBarPro.cs` | [Pin Bar Pro (Final Fix)](indicators/es/PinBarPro.md) | OrderFlow | 9/10 | Conservar | ¿Es este Pin Bar (rechazo) genuino, confirmado por absorción de volumen en la mecha? |
| `PivotsModif.cs` | [Pivots modif](indicators/es/Pivots.md) | Level | 8/10 | Conservar | ¿Cuáles son los niveles de soporte y resistencia (Pivots, R1-3, S1-3) calculados sobre sesiones estándar o personalizadas? |
| `PolarizedFractal.cs` | [Polarized Fractal Efficiency](indicators/es/PolarizedFractal.md) | Momentum | 7/10 | Conservar | ¿Cuál es la eficiencia (tendencia vs ruido) del movimiento del precio? |
| `VolumeIndex.cs` | [Positive/Negative Volume Index](indicators/es/VolumeIndex.md) | Volume | 8/10 | Conservar | ¿Qué está haciendo el 'dinero inteligente' (días de bajo volumen) frente al 'público' (días de alto volumen)? |
| `StochasticDiNapoli.cs` | [Preferred Stochastic - DiNapoli](indicators/es/StochasticDiNapoli.md) | Momentum | 7/10 | Conservar | ¿Cómo filtrar el ruido del estocástico usando el método de suavizado de DiNapoli? |
| `MomentumOscillator.cs` | [Price Momentum Oscillator](indicators/es/MomentumOscillator.md) | Momentum | 7/10 | Mejorar | ¿Cuál es la tasa de cambio del precio, suavizada doblemente y amplificada x10? |
| `VolumeTrend.cs` | [Price Volume Trend](indicators/es/VolumeTrend.md) | Volume | 8/10 | Conservar | ¿Cuál es el flujo acumulado de volumen ponderado por la magnitud del movimiento del precio? |
| `PullingStackingBars.cs` | [Pulling & Stacking Bars (Clean)](indicators/es/PullingStackingBars.md) | OrderFlow | 10/10 | Conservar | ¿Se está añadiendo (Stacking) o retirando (Pulling) liquidez del libro de órdenes en tiempo real? |

---

## Q

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `QStick.cs` | [Q Stick](indicators/es/QStick.md) | Momentum | 6/10 | Conservar | ¿Cuál es el promedio móvil de la distancia entre apertura y cierre de las velas? |
| `QQE.cs` | [Qualitative Quantitative Estimation](indicators/es/QQE.md) | Momentum | 7/10 | Mejorar | ¿Cuál es el RSI suavizado y filtrado por volatilidad (QQE)? |

---

## R

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `RMO.cs` | [Rahul Mohindar Oscillator](indicators/es/RMO.md) | Momentum | 7/10 | Conservar | ¿Cuál es la tendencia primaria y el impulso de corto plazo (basado en múltiples medias)? |
| `RWI.cs` | [Random Walk Index](indicators/es/RWI.md) | Momentum | 5/10 | Mejorar | Determina si el movimiento del precio es estadísticamente significativo o solo ruido aleatorio. |
| `ROC.cs` | [Rate of Change (ROC)](indicators/es/ROC.md) | Momentum | 6/10 | Mejorar | ¿Cuál es la velocidad del cambio de precio (en % o ticks) comparado con 'n' barras atrás? |
| `Ratio.cs` | [Ratio](indicators/es/Ratio.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Cuál es el ratio de absorción/agresión (Bid vs Ask) en el extremo de la vela? |
| `RMI.cs` | [Relative Momentum Index](indicators/es/RMI.md) | Momentum | 7/10 | Mejorar | ¿Cuál es la fuerza relativa del impulso (RSI suavizado con SMMA) en una ventana temporal? |
| `RelativeVigorIndex.cs` | [Relative Vigor Index](indicators/es/RelativeVigorIndex.md) | Momentum | 6/10 | Mejorar | ¿Cuál es la convicción del cierre (Close vs Open) relativa al rango (High vs Low)? |
| `RelativeVolume.cs` | [Relative Volume](indicators/es/RelativeVolume.md) | Volume | 7/10 | Mejorar | ¿Es el volumen actual anómalamente alto o bajo comparado con el promedio histórico para esta misma hora? |
| `Repulse.cs` | [Repulse](indicators/es/Repulse.md) | Momentum | 6/10 | Mejorar | ¿Cuál es la "presión de repulsión" (fuerza de compra/venta interna) suavizada de las velas? |
| `RolloverDates.cs` | [Rollover Dates](indicators/es/RollOverDates.md) | Utility | 8/10 | Conservar | ¿Cuándo vence el contrato de futuros actual y debo cambiar al siguiente? |
| `RoundNr.cs` | [Round Numbers](indicators/es/RoundNr.md) | Level | 7/10 | Mejorar | ¿Dónde están los niveles de precio psicológicos (números redondos) en el gráfico? |
| `RSI.cs` | [RSI (Relative Strength Index)](indicators/es/RSI.md) | Momentum | 8/10 | Conservar | ¿Está el precio sobreextendido (sobrecompra/sobreventa) en relación a su historial reciente? |
| `RTIndicator.cs` | [RT Indicator (Clean)](indicators/es/RTIndicator.md) | Trend | 8/10 | Conservar | ¿Está el precio acelerando su pendiente (Slope) con confirmación de momentum interno? |
| `RVI.cs` | [RVI V1 (Relative Vigor Index)](indicators/es/RVI.md) | Momentum | 4/10 | Mejorar | ¿Cierran las velas consistentemente en la parte alta o baja de su rango? |
| `RVI2.cs` | [RVI V2 (Relative Vigor Index)](indicators/es/RVI2.md) | Momentum | 7/10 | Conservar | Versión mejorada y configurable del RVI que mide la convicción de los cierres respecto al rango. |

---

## S

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `SchaffTrendCycle.cs` | [Schaff Trend Cycle](indicators/es/SchaffTrendCycle.md) | Momentum | 7/10 | Conservar | Oscilador cíclico que combina MACD y Estocástico para detectar giros de mercado más rápido que el MACD tradicional. |
| `SessionColor.cs` | [Session Color](indicators/es/SessionColor.md) | Visualization | 6/10 | Mejorar | Dibuja un fondo de color o líneas verticales para marcar una franja horaria específica (Sesión). |
| `SZMA.cs` | [Simple Moving Average - Skip Zeros](indicators/es/SZMA.md) | Trend | 7/10 | Conservar | ¿Cuál es el promedio real de una serie de datos ignorando los valores vacíos o ceros? |
| `SPVO.cs` | [Simple Percentage Volume Oscillator](indicators/es/SPVO.md) | Volume | 8/10 | Conservar | Oscilador que muestra la diferencia porcentual entre dos medias móviles de volumen. |
| `SWWMA.cs` | [Sine-Wave Weighted Moving Average](indicators/es/SWWMA.md) | Trend | 5/10 | Mejorar | ¿Cuál es la tendencia suavizada usando una ponderación sinusoidal fija de 5 periodos? |
| `SMA.cs` | [SMA (Simple Moving Average)](indicators/es/SMA.md) | Trend | 9/10 | Conservar | Media Móvil Simple optimizada con alertas de precio y cambio de color por tendencia. |
| `SMMA.cs` | [SMMA (Smoothed Moving Average)](indicators/es/SMMA.md) | Trend | 7/10 | Conservar | Media móvil suavizada que reduce el ruido del mercado dando menos peso a los precios recientes que una EMA. |
| `None` | [Speed of Tape](indicators/es/SpeedOfTape.md) | VolumeOrderFlow | 8/10 | Conservar | Mide la velocidad de ejecución (ticks, volumen o delta) en una ventana de tiempo deslizante. |
| `SpreadVolume.cs` | [Spread Volume](indicators/es/SpreadVolume.md) | VolumeOrderFlow | 7/10 | Conservar | Visualiza el volumen ejecutado en el Ask y el Bid por separado, dibujado como histogramas en el spread. |
| `SqueezeMomentum.cs` | [Squeeze Momentum](indicators/es/SqueezeMomentum.md) | Volatility | 9/10 | Conservar | El famoso indicador de John Carter. Detecta periodos de baja volatilidad (Squeeze) seguidos de explosiones direccionales. |
| `StackedImbalance.cs` | [Stacked Imbalance](indicators/es/StackedImbalance.md) | VolumeOrderFlow | 8/10 | Conservar | ¿Dónde existen zonas de desequilibrio agresivo de compra/venta apiladas que actúan como soporte/resistencia? |
| `StdDev.cs` | [Standard Deviation](indicators/es/StdDev.md) | Volatility | 8/10 | Conservar | ¿Cuánto se está alejando el precio de su media (volatilidad absoluta)? |
| `StdDevBands.cs` | [Standard Deviation Bands](indicators/es/StdDevBands.md) | Volatility | 8/10 | Conservar | ¿Está el precio alcanzando extremos estadísticos de volatilidad basados en máximos y mínimos? |
| `StdErrBands.cs` | [Standard Error Bands](indicators/es/StdErrBands.md) | Volatility | 8/10 | Conservar | ¿Cuál es el rango de error estadístico esperado alrededor de la tendencia de regresión actual? |
| `StarcBands.cs` | [Starc Bands](indicators/es/StarcBands.md) | Volatility | 6/10 | Mejorar | ¿Dónde están los límites de volatilidad basados en el rango medio verdadero (ATR) alrededor de la media? |
| `Stochastic.cs` | [Stochastic](indicators/es/Stochastic.md) | Momentum | 8/10 | Conservar | ¿Dónde cerró el precio en relación con su rango High-Low reciente? |
| `StohasticPercentile.cs` | [Stochastic - Percentile](indicators/es/StohasticPercentile.md) | Momentum | 5/10 | Mejorar | ¿Qué percentil estadístico ocupa el precio actual respecto a los últimos N periodos? |
| `StochasticMomentum.cs` | [Stochastic Momentum](indicators/es/StochasticMomentum.md) | Momentum | 7/10 | Conservar | ¿Cuál es el impulso del precio relativo al centro de su rango (SMI) en lugar de al mínimo? |
| `StochasticRsi.cs` | [Stochastic RSI](indicators/es/StochasticRsi.md) | Momentum | 6/10 | Mejorar | ¿En qué parte de su rango reciente se encuentra el RSI actual (Sensibilidad extrema)? |
| `Angle.cs` | [Study Angle](indicators/es/Angle.md) | Momentum | 2/10 | Descartar | ¿Cuál es el ángulo geométrico literal (en grados) de la tendencia del precio durante las últimas X barras? |
| `SuperTrend.cs` | [Super Trend](indicators/es/SuperTrend.md) | Trend | 9/10 | Conservar | ¿Cuál es la dirección actual de la tendencia y dónde está el nivel de Stop Loss dinámico (Trailing Stop)? |
| `SwingHighLow.cs` | [Swing High and Low](indicators/es/SwingHighLow.md) | Momentum | 8/10 | Conservar | ¿Cuáles son los puntos de giro (máximos/mínimos locales) confirmados en la estructura del mercado? |
| `SyntheticVix.cs` | [Synthetic VIX](indicators/es/SyntheticVix.md) | Volatility | 8/10 | Conservar | ¿Cómo de lejos está el precio actual del máximo reciente (Proxy de miedo/pánico)? |

---

## T

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `T3.cs` | [T3](indicators/es/T3.md) | Trend | 8/10 | Conservar | ¿Cuál es la tendencia suavizada eliminando el ruido con la fórmula T3 de Tillson? |
| `TapePattern.cs` | [Tape Patterns](indicators/es/TapePattern.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Dónde están los bloques de órdenes grandes y patrones de ejecución específicos en la cinta? |
| `TDSequential.cs` | [TD Sequential](indicators/es/TDSequential.md) | Momentum | 8/10 | Conservar | ¿En qué punto del ciclo de agotamiento (Setup/Countdown) se encuentra la tendencia actual? |
| `TVI.cs` | [Trade Volume Index](indicators/es/TVI.md) | Volume | 7/10 | Conservar | ¿Se está acumulando o distribuyendo el volumen basándose en la dirección del tick? |
| `TradesOnChart.cs` | [Trades On Chart](indicators/es/TradesOnChart.md) | Visualization | 9/10 | Conservar | ¿Dónde ejecuté mis operaciones pasadas y cuál fue el resultado (PnL) visualmente? |
| `TMA.cs` | [Triangular Moving Average](indicators/es/TMA.md) | Trend | 7/10 | Conservar | ¿Cuál es la tendencia central "verdadera" con un suavizado extremo (doble promedio)? |
| `TEMA.cs` | [Triple Exponential Moving Average](indicators/es/TEMA.md) | Trend | 8/10 | Conservar | ¿Cuál es la tendencia inmediata con el mínimo retraso posible (Lag casi cero)? |
| `TRIX.cs` | [TRIX](indicators/es/TRIX.md) | Momentum | 7/10 | Conservar | ¿Cuál es la tasa de cambio (ROC) de una media móvil triplemente suavizada? |
| `TrueRange.cs` | [True Range](indicators/es/TrueRange.md) | Volatility | 8/10 | Conservar | ¿Cuál es la volatilidad real de la vela actual incluyendo los gaps de apertura? |
| `TSI.cs` | [True Strength Index](indicators/es/TSI.md) | Momentum | 8/10 | Conservar | ¿Cuál es el momentum del precio libre de ruido gracias a un doble suavizado exponencial? |

---

## U

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `UltimateOscillator.cs` | [Ultimate Oscillator](indicators/es/UltimateOscillator.md) | Momentum | 7/10 | Conservar | ¿Cuál es el momentum real del mercado combinando tres marcos temporales para evitar señales falsas? |
| `UnfinishedAuction.cs` | [Unfinished Auction](indicators/es/UnfinishedAuction.md) | VolumeOrderFlow | 9/10 | Conservar | ¿Quedaron órdenes pendientes (desequilibrio) en los extremos de la vela que el precio debe volver a visitar? |
| `UpDownVolumeRatio.cs` | [Up/Down Volume Ratio](indicators/es/UpDownVolumeRatio.md) | Volume | 9/10 | Conservar | ¿Quién controla el flujo de volumen (compradores o vendedores) y con qué intensidad relativa? |

---

## V

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `VerticalGrid.cs` | [Vertical Grid](indicators/es/VerticalGrid.md) | Visualization | 4/10 | Mejorar | Dibuja líneas verticales y etiquetas de tiempo a intervalos regulares personalizados. |
| `VerticalHorizontalFilter.cs` | [Vertical Horizontal Filter](indicators/es/VerticalHorizontalFilter.md) | Statistical | 8/10 | Conservar | ¿Está el mercado en tendencia (movimiento vertical) o en rango (movimiento horizontal)? |
| `VolatilityChaikins.cs` | [Volatility - Chaikins](indicators/es/VolatilityChaikins.md) | Volatility | 7/10 | Conservar | ¿Se está expandiendo o contrayendo el rango de precios (volatilidad) respecto al pasado reciente? |
| `VolatilityHist.cs` | [Volatility - Historical](indicators/es/VolatilityHist.md) | Volatility | 4/10 | Reparar | ¿Cuál es la volatilidad estadística histórica basada en los retornos logarítmicos? |
| `VolatilityTrend.cs` | [Volatility Trend](indicators/es/VolatilityTrend.md) | Volatility | 8/10 | Conservar | ¿Cuál es el canal de tendencia dinámico ajustado por la persistencia de la dirección y la volatilidad? |
| `Volume.cs` | [Volume](indicators/es/Volume.md) | Volume | 9/10 | Conservar | ¿Cuál es el volumen de actividad (o ticks/bid/ask) en cada vela, y cómo se relaciona con el movimiento? |
| `VBRR.cs` | [Volume Bar Range Ratio](indicators/es/VBRR.md) | Volume | 7/10 | Conservar | ¿Cuánto volumen es necesario para mover el precio 1 tick (Eficiencia del movimiento)? |
| `VolumeOnChart.cs` | [Volume On The Chart](indicators/es/VolumeOnChart.md) | Volume | 8/10 | Conservar | Visualiza el volumen como un histograma de fondo superpuesto al precio para ahorrar espacio. |
| `VolumePerTrade.cs` | [Volume Per Trade](indicators/es/VolumePerTrade.md) | Volume | 8/10 | Conservar | ¿Cuál es el tamaño promedio de las órdenes ejecutadas en cada vela (Institucional vs Retail)? |
| `VolumeZone.cs` | [Volume Zone Oscillator](indicators/es/VolumeZone.md) | Volume | 9/10 | Conservar | ¿Cuál es la presión neta de compra/venta normalizada por el volumen total (Oscilador de Zona)? |
| `VolumeSupResZones.cs` | [Volume-based Support & Resistance Zones](indicators/es/VolumeSupResZones.md) | VolumeOrderFlow | 10/10 | Conservar | ¿Dónde están las zonas de soporte y resistencia definidas por volumen en múltiples marcos temporales? |
| `Vortex.cs` | [Vortex](indicators/es/Vortex.md) | Momentum | 7/10 | Conservar | ¿Cuál es la fuerza direccional del mercado basada en el flujo de vórtices (High-Low)? |
| `VPF.cs` | [Voss Predictive Filter](indicators/es/VPF.md) | Trend | 8/10 | Conservar | ¿Cuál es la proyección cíclica del precio eliminando el ruido espectral? |
| `VsaBetterVolume.cs` | [VSA Better Volume](indicators/es/VsaBetterVolume.md) | Volume | 9/10 | Conservar | ¿Qué nos dice el volumen sobre la intención profesional (Clímax, Churn, Trampa)? |
| `VsaWsd.cs` | [VSA – WSD Histogram](indicators/es/VsaWsd.md) | Volume | 8/10 | Conservar | ¿Cómo se distribuye la estructura de la vela (mechas vs cuerpo) y el volumen relativo? |
| `VWAP.cs` | [VWAP / TWAP](indicators/es/VWAP.md) | VolumeOrderFlow | 10/10 | Conservar | ¿Cuál es el precio medio ponderado por volumen (institucional) y sus desviaciones estándar? |

---

## W

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Watermark.cs` | [Watermark](indicators/es/Watermark.md) | Visualization | 5/10 | Conservar | Muestra información del instrumento o texto personalizado en el fondo del gráfico. |
| `Wavetrend.cs` | [Wavetrend](indicators/es/Wavetrend.md) | Momentum | 8/10 | Conservar | ¿Cuál es el ciclo de oscilación del precio basado en la volatilidad (famoso indicador de TV)? |
| `WAO.cs` | [Weighted Average Oscillator](indicators/es/WAO.md) | Momentum | 7/10 | Conservar | ¿Cuál es la diferencia entre dos medias móviles ponderadas (WMA)? (MACD rápido). |
| `WMA.cs` | [Weighted Moving Average](indicators/es/WMA.md) | Trend | 8/10 | Conservar | ¿Cuál es la media móvil ponderada linealmente (más peso a lo reciente)? |
| `WeissWave.cs` | [Weis Wave](indicators/es/WeissWave.md) | Volume | 8/10 | Conservar | ¿Cuánto volumen acumulado (esfuerzo) hay en la onda de precio actual? |
| `WWMA.cs` | [Welles Wilder's Moving Average](indicators/es/WWMA.md) | Trend | 7/10 | Conservar | ¿Cuál es la tendencia suavizada según el método original de Wilder (base de RSI/ATR)? |
| `WAD.cs` | [Williams Accumulation / Distribution (WAD)](indicators/es/WAD.md) | Volume | 7/10 | Conservar | ¿Se está acumulando o distribuyendo el activo (basado en la presión de cierre)? |
| `WilliamsR.cs` | [Williams' %R](indicators/es/WilliamsR.md) | Momentum | 7/10 | Conservar | ¿Dónde cerró el precio relativo al rango High-Low (versión invertida)? |
| `WoodiesCCI.cs` | [Woodies CCI](indicators/es/WoodiesCCI.md) | Momentum | 9/10 | Conservar | Sistema completo de trading de Woodie basado en patrones de CCI. |
| `WPR.cs` | [WPR (Williams %R)](indicators/es/WPR.md) | Momentum | 6/10 | Descartar | Versión duplicada de Williams %R con visualización ligeramente diferente. |

---

## Z

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Actual | Acción Recomendada | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `ZScore.cs` | [Z-Score](indicators/es/ZScore.md) | Statistical | 8/10 | Conservar | ¿A cuántas desviaciones estándar se encuentra el precio actual de su media histórica? |
| `ZLEMA.cs` | [Zero Lag Exponential Moving Average](indicators/es/ZLEMA.md) | Trend | 8/10 | Conservar | ¿Cuál es la tendencia suavizada eliminando el retraso matemático inherente a las medias móviles? |
| `Zigzag.cs` | [ZigZag Pro](indicators/es/Zigzag.md) | PriceAction | 10/10 | Conservar | ¿Qué dicen las métricas acumuladas (Delta/Volumen) de cada onda de precio sobre la estructura del mercado? |