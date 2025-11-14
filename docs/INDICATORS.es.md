# Índice de Indicadores

Bienvenidos a la guía de indicadores del repositorio. El objetivo principal de esta documentación es **aclarar el propósito y uso de cada indicador** que se encuentra aquí, ya sea en su versión original del repositorio oficial de ATAS o como una modificación personal. 

El enfoque principal de análisis y las puntuaciones están **orientados al scalping en el S&P 500 (ES/MES)**. 

**Nota Importante:** Las valoraciones, puntuaciones ("Score") y veredictos ("Veredicto") son **opiniones subjetivas** basadas en mi experiencia y el apoyo de herramientas de IA (como Gemini y ChatGPT). 

Para compilar esta información, se ha utilizado la documentación oficial de ATAS, el código fuente de los repositorios.

---

## 🧭 Navegación Rápida
**[A](#a) | [B](#b) | [C](#c) | [D](#d) | [E](#e) | [F](#f) | [G](#g) | [H](#h) | [I](#i) | [J](#j) | [K](#k) | [L](#l) | [M](#m) | [N](#n) | [O](#o) | [P](#p) | [Q](#q) | [R](#r) | [S](#s) | [T](#t) | [U](#u) | [V](#v) | [W](#w) | [X](#x) | [Y](#y) | [Z](#z)**

---


## A

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `ACDC.cs` | [AC DC Histogram](indicators/es/ACDC.md) | Momentum | 2/10 | Descartar | ¿Cuál es la dirección _suavizada y con retardo_ de la aceleración del mercado? |
| `AccountInfoDisplay.cs` | [Account Info Display](indicators/es/AccountInfoDisplay.md) | Utilidad | 7/10 | Conservar | ¿Cuál es el estado de mi cuenta (Balance, PnL, Margen) en tiempo real, sin tener que apartar la vista del gráfico? |
| `AD .cs` | [Accumulation/Distribution (A/D)](indicators/es/AD.md) | Volumen clásico | 2/10 | Descartar | ¿El flujo de volumen acumulado está confirmando la tendencia del precio, o está mostrando una divergencia? |
| `ActiveVolume.cs` | [Active Volume](indicators/es/ActiveVolume.md) | Order Flow | 8/10 | Conservar | Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios? |
| `AdaptiveBinaryWaveMA.cs` | [Adaptive Binary Wave](indicators/es/AdaptativeBinaryWaveMA.md) | Tendencia | 7/10 | Conservar | ¿Ha roto la media móvil adaptativa (AMA) su 'canal' reciente por una cantidad estadísticamente significativa? |
| `AMA.cs` | [Adaptive Moving Average](indicators/es/AMA.md) | Tendencia | 7/10 | Conservar | ¿Cómo puedo obtener una media móvil suave que _no_ tenga retardo (lag) durante una ruptura fuerte, pero _sí_ filtre el 'ruido' en un mercado lateral? |
| `AdaptiveRsiAverage.cs` | [Adaptive RSI Moving Average](indicators/es/AdaptativeRSIAverage.md) | Tendencia | 4/10 | Descartar | ¿Cómo puedo obtener una media móvil que automáticamente se ralentice cuando el mercado está indeciso (RSI cerca de 50) y se acelere para capturar tendencias cuando el momentum es fuerte (RSI cerca de 0 o 100)? |
| `ADF.cs` | [ADF](indicators/es/ADF.md) | Volumen clásico | 3/10 | Descartar | ¿Cuál es la _tendencia suavizada (lenta)_ del flujo de volumen acumulado? |
| `ADR.cs` | [ADR](indicators/es/ADR.md) | Volatilidad | 7/10 | Conservar | ¿Cuál es el rango de movimiento "normal" o "promedio" para este instrumento en una sesión, y dónde se proyectarían esos límites hoy? |
| `ADX.cs` | [ADX](indicators/es/ADX.md) | Tendencia | 6/10 | Conservar | ¿Está el mercado en una fuerte tendencia (alcista o bajista), o simplemente está 'oscilando' lateralmente? |
| `ADXR.cs` | [ADXR](indicators/es/ADXR.md) | Tendencia | 3/10 | Descartar | ¿Cuál es la fuerza _estable y suavizada_ de la tendencia, ignorando el ruido a corto plazo del propio ADX? |
| `Alligator.cs` | [Alligator](indicators/es/Alligator.md) | Tendencia | 6/10 | Descartar | ¿Está el mercado 'durmiendo' (en rango, con las medias entrelazadas) o está 'despierto y comiendo' (en tendencia, con las medias abiertas)? |
| `AroonIndicator.cs` | [Aroon Indicator](indicators/es/AroonIndicator.md) | Momentum | 3/10 | Descartar | ¿La fortaleza del mercado proviene de haber hecho recientemente nuevos máximos, o de haber hecho recientemente nuevos mínimos? |
| `AroonOscillator.cs` | [Aroon Oscillator](indicators/es/AroonOscillator.md) | Momentum | 3/10 | Descartar | ¿Qué fuerza es más fuerte y reciente, la que está creando nuevos máximos (AroonUp) o la que está creando nuevos mínimos (AroonDown)? |
| `AskBidBars.cs` | [Ask/Bid Volume Difference Bars](indicators/es/AskBidBars.md) | Order Flow | 6.5/10 | Conservar (con reservas) | ¿Cuál fue el volumen agresivo neto (Delta) de esta vela, e igualmente importante, cuál fue el rango interno (Delta Máx/Mín) de la batalla entre compradores y vendedores dentro de esa vela? |
| `ATR.cs` | [ATR](indicators/es/ATR.md) | Volatilidad | 8/10 | Conservar y Mejorar | ¿Cuál ha sido el tamaño verdadero promedio (incluyendo gaps) de cada barra durante los últimos X períodos? |
| `ATRN.cs` | [ATR Normalized](indicators/es/ATRN.md) | Volatilidad | 3/10 | Descartar | ¿Cuál es la volatilidad (ATR) del instrumento como un porcentaje de su precio actual? |
| `ACR.cs` | [Average Candle Range](indicators/es/ACR.md) | Volatilidad | 5/10 | Descartar | ¿Cuál es el tamaño promedio de una vela en lo que va de día? |
| `AverageDelta.cs` | [Average Delta](indicators/es/AverageDelta.md) | Order Flow | 6.5/10 | Conservar (con reservas) | ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas, suavizando el ruido de vela a vela? |
| `AveragePriceBar.cs` | [Average Price for Bar](indicators/es/AveragePriceBar.md) | Precio | 2/10 | Descartar | ¿En lugar de solo el 'Cierre', cuál es el precio promedio interno (ej. Mediana, Típico) de cada vela individual? |
| `AO.cs` | [Awesome Oscillator](indicators/es/AO.md) | Momentum | 2/10 | Descartar | ¿Está el momentum reciente a corto plazo (5 barras) ganando la batalla contra el momentum de la tendencia a largo plazo (34 barras)? |

---

## B

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `BOP.cs` | [Balance of Power](indicators/es/BOP.md) | Momentum / Price action | 5/10 | Descartar | ¿Cuál es, en promedio, la fuerza del cuerpo de la vela (Cierre vs. Apertura) en relación con su rango total (Máximo vs. Mínimo)? |
| `BandsEnvelope.cs` | [Bands / Envelope](indicators/es/BandsEnvelope.md) | Volatilidad | 1/10 | Descartar | ¿Cuán lejos puede moverse el precio (en Ticks Fijos, Valor Fijo o Porcentaje Fijo) antes de considerarse 'sobre-extendido'? |
| `BarDifference.cs` | [Bar Difference](indicators/es/BarDifference.md) | Momentum | 3/10 | Descartar | ¿Cuántos ticks ha subido o bajado el precio (Cierre) en comparación con hace X velas? |
| `BarNumbering.cs` | [Bar Numbering](indicators/es/BarNumbering.md) | Utilidad / Visualización | 4/10 | Descartar | ¿Cuántas velas han pasado desde el inicio de la sesión, y puedes etiquetarlas cada X velas, por favor? |
| `BarRange.cs` | [Bar Range](indicators/es/BarRange.md) | Volatilidad | 5/10 | Descartar | ¿Cuál es el rango (Máximo - Mínimo) de cada vela, y cuál ha sido el rango más alto de las últimas X velas? |
| `BarTimer.cs` | [Bar Timer](indicators/es/BarTimer.md) | Utilidad / Visualización | 8/10 | Conservar | ¿Cuánto tiempo (o ticks/volumen) le queda a esta vela, y puedes avisarme 3 segundos antes de que cierre? |
| `BarVolumeFilter.cs` | [Bar's Volume Filter](indicators/es/BarVolumeFilter.md) | Order Flow | 7/10 | Conservar | ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks (ej. 'Volumen \> 1500' y solo 'dentro de la sesión RTH')? |
| `BarsPattern.cs` | [Bars Pattern](indicators/es/BarsPattern.md) | Order Flow / Utilidad | 9/10 | Conservar (Herramienta Principal) | ¿Qué velas de este gráfico cumplen _todos_ mis criterios específicos y multicapa para un setup de alta calidad (Volumen, Delta, forma de vela, etc.)? |
| `BidAsk.cs` | [Bid Ask](indicators/es/BidAsk.md) | Order Flow | 6.5/10 | Conservar (con reservas) | ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela? |
| `BidAskVR.cs` | [Bid Ask Volume Ratio](indicators/es/BidAskVR.md) | Order Flow | 7/10 | Conservar (Herramienta principal) | ¿Cuál es el desequilibrio normalizado (de -100% a +100%) del volumen agresivo, y cuál es el momentum (pendiente) de ese desequilibrio? |
| `ACBW.cs` | [Bill Williams AC](indicators/es/ACBW.md) | Momentum | 3/10 | Descartar | ¿El momentum (AO) está acelerando o frenando? |
| `BWMA.cs` | [Bill Williams Moving Average](indicators/es/BWMA.md) | Tendencia | 5/10 | Descartar | ¿Cuál es el precio promedio exponencial (EMA), que da más peso a las velas más recientes? |
| `BlockMA.cs` | [Block Moving Average](indicators/es/BlockMA.md) | Tendencia / Volatilidad | 7/10 | Conservar (con reservas) | ¿Cómo puedo crear un 'trailing stop' de volatilidad (basado en el ATR) que solo se mueva a favor de la tendencia (hacia arriba o hacia abajo), pero que nunca retroceda? |
| `BollingerBands.cs` | [Bollinger Bands](indicators/es/BollingerBands.md) | Volatilidad / Canal | 8/10 | Conservar (esencial) | ¿Está el precio actual estadísticamente 'demasiado alto' o 'demasiado bajo' (sobre-extendido) en comparación con su media reciente, basándose en la volatilidad? |
| `BollingerBandsBandwidth.cs` | [Bollinger Bands Bandwidth](indicators/es/BollingerBandsBandwidth.md) | Volatilidad | 6.5/10 | Descartar | ¿Cómo de 'comprimida' (squeeze) o 'expandida' está la volatilidad ahora mismo, medida como un porcentaje del precio medio? |
| `BollingerBandsPercent.cs` | [Bollinger Bands Percentage](indicators/es/BollingerBandsPercent.md) | Volatilidad | 6/10 | Conservar (con reservas) | ¿En qué posición (como un porcentaje normalizado) se encuentra el precio actual dentro de las Bandas de Bollinger? |
| `BollingerSqueeze.cs` | [Bollinger Squeeze](indicators/es/BollingerSqeeze.md) | Volatilidad | 7/10 | Descartar | ¿Se está comprimiendo la volatilidad del precio (Bollinger) dentro de la volatilidad de su rango medio (Keltner), señalando una 'compresión' (squeeze) y un potencial movimiento explosivo? |
| `BollingerSqueezeV2.cs` | [Bollinger Squeeze 2](indicators/es/BollingerSqeezeV2.md) | Volatilidad / Momentum | 8/10 | Conservar (Herramienta principal) | ¿Cuál es el momentum (y la pendiente de ese momentum) del precio? Y, al mismo tiempo, ¿está el mercado en una 'compresión' (squeeze) de baja volatilidad (punto rojo) o en una 'expansión' de alta volatilidad (punto verde)? |
| `BollingerSqueezeV3.cs` | [Bollinger Squeeze 3](indicators/es/BollingerSqeezeV3.md) | Volatilidad / Momentum | 6/10 | Descartar | ¿Está la volatilidad del precio (StdDev) actualmente mayor o menor que la volatilidad del rango de las velas (ATR)? |

---

## O

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `BalanceOI.cs` | [On Balance Open Interest](indicators/es/BalanceOI.md) | Order Flow | 8/10 | Conservar | ¿Está el compromiso acumulado del 'dinero inteligente' (Interés Abierto) subiendo cuando los precios suben y bajando cuando los precios bajan, o está divergiendo? |

---

## S

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Angle.cs` | [Study Angle](indicators/es/Angle.md) | Momentum | 2/10 | Descartar | ¿Cuál es el ángulo geométrico literal (en grados) de la tendencia del precio durante las últimas X barras? |