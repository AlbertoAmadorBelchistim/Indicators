# Índice de Indicadores Seleccionados (Conservar)

(Este índice ha sido generado automáticamente)

---

## 🧭 Navegación Rápida
**[A](#a) | [B](#b) | [C](#c) | [D](#d) | [E](#e) | [F](#f) | [G](#g) | [H](#h) | [I](#i) | [J](#j) | [K](#k) | [L](#l) | [M](#m) | [N](#n) | [O](#o) | [P](#p) | [Q](#q) | [R](#r) | [S](#s) | [T](#t) | [U](#u) | [V](#v) | [W](#w) | [X](#x) | [Y](#y) | [Z](#z)**

---

## A

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `AccountInfoDisplay.cs` | [Account Info Display](indicators/es/AccountInfoDisplay.md) | Utilidad | 7/10 | Conservar | ¿Cuál es el estado de mi cuenta (Balance, PnL, Margen) en tiempo real, sin tener que apartar la vista del gráfico? |
| `ActiveVolume.cs` | [Active Volume](indicators/es/ActiveVolume.md) | Order Flow | 8/10 | Conservar | Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios? |
| `AdaptiveBinaryWaveMA.cs` | [Adaptive Binary Wave](indicators/es/AdaptativeBinaryWaveMA.md) | Tendencia | 7/10 | Conservar | ¿Ha roto la media móvil adaptativa (AMA) su 'canal' reciente por una cantidad estadísticamente significativa? |
| `AMA.cs` | [Adaptive Moving Average](indicators/es/AMA.md) | Tendencia | 7/10 | Conservar | ¿Cómo puedo obtener una media móvil suave que _no_ tenga retardo (lag) durante una ruptura fuerte, pero _sí_ filtre el 'ruido' en un mercado lateral? |
| `ADR.cs` | [ADR](indicators/es/ADR.md) | Volatilidad | 7/10 | Conservar | ¿Cuál es el rango de movimiento "normal" o "promedio" para este instrumento en una sesión, y dónde se proyectarían esos límites hoy? |
| `ADX.cs` | [ADX](indicators/es/ADX.md) | Tendencia | 6/10 | Conservar | ¿Está el mercado en una fuerte tendencia (alcista o bajista), o simplemente está 'oscilando' lateralmente? |
| `AskBidBars.cs` | [Ask/Bid Volume Difference Bars](indicators/es/AskBidBars.md) | Order Flow | 6.5/10 | Conservar (con reservas) | ¿Cuál fue el volumen agresivo neto (Delta) de esta vela, e igualmente importante, cuál fue el rango interno (Delta Máx/Mín) de la batalla entre compradores y vendedores dentro de esa vela? |
| `ATR.cs` | [ATR](indicators/es/ATR.md) | Volatilidad | 8/10 | Conservar y Mejorar | ¿Cuál ha sido el tamaño verdadero promedio (incluyendo gaps) de cada barra durante los últimos X períodos? |
| `AverageDelta.cs` | [Average Delta](indicators/es/AverageDelta.md) | Order Flow | 6.5/10 | Conservar (con reservas) | ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas, suavizando el ruido de vela a vela? |

---

## B

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `BarTimer.cs` | [Bar Timer](indicators/es/BarTimer.md) | Utilidad / Visualización | 8/10 | Conservar | ¿Cuánto tiempo (o ticks/volumen) le queda a esta vela, y puedes avisarme 3 segundos antes de que cierre? |
| `BarVolumeFilter.cs` | [Bar's Volume Filter](indicators/es/BarVolumeFilter.md) | Order Flow | 7/10 | Conservar | ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks (ej. 'Volumen \> 1500' y solo 'dentro de la sesión RTH')? |
| `BarsPattern.cs` | [Bars Pattern](indicators/es/BarsPattern.md) | Order Flow / Utilidad | 9/10 | Conservar (Herramienta Principal) | ¿Qué velas de este gráfico cumplen _todos_ mis criterios específicos y multicapa para un setup de alta calidad (Volumen, Delta, forma de vela, etc.)? |
| `BidAsk.cs` | [Bid Ask](indicators/es/BidAsk.md) | Order Flow | 6.5/10 | Conservar (con reservas) | ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela? |
| `BidAskVR.cs` | [Bid Ask Volume Ratio](indicators/es/BidAskVR.md) | Order Flow | 7/10 | Conservar (Herramienta principal) | ¿Cuál es el desequilibrio normalizado (de -100% a +100%) del volumen agresivo, y cuál es el momentum (pendiente) de ese desequilibrio? |
| `BlockMA.cs` | [Block Moving Average](indicators/es/BlockMA.md) | Tendencia / Volatilidad | 7/10 | Conservar (con reservas) | ¿Cómo puedo crear un 'trailing stop' de volatilidad (basado en el ATR) que solo se mueva a favor de la tendencia (hacia arriba o hacia abajo), pero que nunca retroceda? |
| `BollingerBands.cs` | [Bollinger Bands](indicators/es/BollingerBands.md) | Volatilidad / Canal | 8/10 | Conservar (esencial) | ¿Está el precio actual estadísticamente 'demasiado alto' o 'demasiado bajo' (sobre-extendido) en comparación con su media reciente, basándose en la volatilidad? |
| `BollingerBandsPercent.cs` | [Bollinger Bands Percentage](indicators/es/BollingerBandsPercent.md) | Volatilidad | 6/10 | Conservar (con reservas) | ¿En qué posición (como un porcentaje normalizado) se encuentra el precio actual dentro de las Bandas de Bollinger? |
| `BollingerSqueezeV2.cs` | [Bollinger Squeeze 2](indicators/es/BollingerSqeezeV2.md) | Volatilidad / Momentum | 8/10 | Conservar (Herramienta principal) | ¿Cuál es el momentum (y la pendiente de ese momentum) del precio? Y, al mismo tiempo, ¿está el mercado en una 'compresión' (squeeze) de baja volatilidad (punto rojo) o en una 'expansión' de alta volatilidad (punto verde)? |

---

## C

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `CamarillaPivots.cs` | [Camarilla Pivots](indicators/es/CamarillaPivots.md) | Niveles | 8/10 | Conservar | ¿Dónde están los niveles de soporte y resistencia intradía más relevantes, basados en la fórmula de Camarilla, para operar rupturas (en L4/H4) y reversiones (en L3/H3)? |
| `CandleStatistics.cs` | [Candle Statistics](indicators/es/CandleStatistics.md) | Utilidad / Visualización | 8/10 | Conservar | ¿Cuál es la "radiografía" de esta vela? ¿Cuál fue su Volumen total, su Delta neto, su número de Ticks (trades) y su Duración? |
| `CCI.cs` | [CCI](indicators/es/CCI.md) | Momentum | 7/10 | Conservar | ¿Qué tan lejos se ha desviado el precio "típico" de hoy de su precio "promedio", medido en unidades de desviación estadística? |
| `CMS.cs` | [Clear Method Swing Line](indicators/es/CMS.md) | Tendencia | 8/10 | Conservar (herramienta de contexto) | ¿Cuál es la estructura de mercado (swing highs/lows) objetiva y actual, sin subjetividad? |
| `ClusterSearchModif.cs` | [Cluster Search Modif ](indicators/es/ClusterSearchModif.md) | Order Flow | 9/10 | Conservar (Herramienta Principal) | ¿Qué clústeres de precio específicos en este gráfico cumplen *todos* mis criterios de filtro (por Volumen, Delta, Localización, Imbalance, etc.)? |
| `ClusterStatisticModif.cs` | [Cluster Statistic Modif ](indicators/es/ClusterStatisticModif.md) | Order FLow | 10/10 | Conservar (herramienta principal) | ¿Cuál es el "dashboard" estadístico completo (Volumen, Delta, Ticks, *Velocidad* e *Imbalances*) de cada vela, y cómo se compara cada vela con la más "fuerte" del gráfico? |
| `CumulativeDailyVolume.cs` | [Cumulative Daily Volume](indicators/es/CumulativeDailyVolume.md) | Volume | 6/10 | Conservar (contexto esencial) | ¿Cuál es el volumen total acumulado desde el inicio de la sesión? |
| `CumulativeDelta.cs` | [CVD - Cumulative Volume Delta](indicators/es/CumulativeDelta.md) | VolumeOrderFlow | 9/10 | Conservar (herramienta principal) | ¿Cuál es el delta acumulado (la agresión neta) desde el inicio de la sesión? |

---

## D

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `DeltaModif.cs` | [Delta Modif ](indicators/es/DeltaModif.md) | Order FLow | 10/10 | Conservar (herramienta principal) | ¿Qué barras muestran una agresión (Delta) extrema, divergencia o absorción, y cómo puedo ver esas señales directamente en el gráfico de precio? |

---

## L

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `LevelsLolo.cs` | [LevelsLolo](indicators/es/LevelsLolo.md) | Levels | 9/10 | Conservar (herramienta de contexto clave) | ¿Dónde están los niveles clave de SpotGamma (CO, LG, VT, PW/CW) y cómo puedo visualizarlos en mi gráfico con una jerarquía clara de importancia? |

---

## O

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `BalanceOI.cs` | [On Balance Open Interest](indicators/es/BalanceOI.md) | Order Flow | 8/10 | Conservar | ¿Está el compromiso acumulado del 'dinero inteligente' (Interés Abierto) subiendo cuando los precios suben y bajando cuando los precios bajan, o está divergiendo? |