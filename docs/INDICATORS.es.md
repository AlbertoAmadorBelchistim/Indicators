# Índice de Indicadores

(Breve introducción sobre los indicadores de trading.)

---

## 🧭 Navegación Rápida
**[A](#a) | [B](#b) | [C](#c) | [D](#d) | [E](#e) | [F](#f) | [G](#g) | [H](#h) | [I](#i) | [J](#j) | [K](#k) | [L](#l) | [M](#m) | [N](#n) | [O](#o) | [P](#p) | [Q](#q) | [R](#r) | [S](#s) | [T](#t) | [U](#u) | [V](#v) | [W](#w) | [X](#x) | [Y](#y) | [Z](#z)**

---


## A

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `ACDC.cs` | [AC DC Histogram](indicators/es/ACDC.md) | Momentum | 2/10 | Descartar | ¿Cuál es la dirección _suavizada y con retardo_ de la aceleración del mercado? |
| `AccountInfoDisplay.cs` | [Account Info Display](indicators/es/AccountInfoDisplay.md) | Utilidad | 7/10 | Conservar | ¿Cuál es el estado de mi cuenta (Balance, PnL, Margen) en tiempo real, sin tener que apartar la vista del gráfico? |
| `ADF.cs` | [Accumulation / Distribution Flow](indicators/es/ADF.md) | Tendencia | 3/10 | Descartar | ¿Cuál es la _tendencia suavizada (lenta)_ del flujo de volumen acumulado? |
| `AD .cs` | [Accumulation/Distribution (A/D)](indicators/es/AD.md) | Volumen clásico | 2/10 | Descartar | ¿El flujo de volumen acumulado está confirmando la tendencia del precio, o está mostrando una divergencia? |
| `ActiveVolume.cs` | [Active Volume](indicators/es/ActiveVolume.md) | Order Flow | 8/10 | Conservar | Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios? |
| `AdaptiveBinaryWaveMA.cs` | [Adaptive Binary Wave](indicators/es/AdaptativeBinaryWaveMA.md) | Tendencia | 7/10 | Conservar | ¿Ha roto la media móvil adaptativa (AMA) su 'canal' reciente por una cantidad estadísticamente significativa? |
| `AMA.cs` | [Adaptive Moving Average](indicators/es/AMA.md) | Tendencia | 7/10 | Conservar | ¿Cómo puedo obtener una media móvil suave que _no_ tenga retardo (lag) durante una ruptura fuerte, pero _sí_ filtre el 'ruido' en un mercado lateral? |
| `AdaptiveRsiAverage.cs` | [Adaptive RSI Moving Average](indicators/es/AdaptativeRSIAverage.md) | Tendencia | 4/10 | Descartar | ¿Cómo puedo obtener una media móvil que automáticamente se ralentice cuando el mercado está indeciso (RSI cerca de 50) y se acelere para capturar tendencias cuando el momentum es fuerte (RSI cerca de 0 o 100)? |
| `ADX.cs` | [ADX](indicators/es/ADX.md) | Tendencia | 6/10 | Conservar | ¿Está el mercado en una fuerte tendencia (alcista o bajista), o simplemente está 'oscilando' lateralmente? |
| `ADXR.cs` | [ADXR](indicators/es/ADXR.md) | Tendencia | 3/10 | Descartar | ¿Cuál es la fuerza _estable y suavizada_ de la tendencia, ignorando el ruido a corto plazo del propio ADX? |
| `Alligator.cs` | [Alligator](indicators/es/Alligator.md) | Tendencia | 6/10 | Descartar | ¿Está el mercado 'durmiendo' (en rango, con las medias entrelazadas) o está 'despierto y comiendo' (en tendencia, con las medias abiertas)? |
| `AroonIndicator.cs` | [Aroon Indicator](indicators/es/AroonIndicator.md) | Momentum | 3/10 | Descartar | ¿La fortaleza del mercado proviene de haber hecho recientemente nuevos máximos, o de haber hecho recientemente nuevos mínimos? |
| `AskBidBars.cs` | [Ask/Bid Volume Difference Bars](indicators/es/AskBidBars.md) | Order Flow | 6.5/10 | Conservar (con reservas) | ¿Cuál fue el volumen agresivo neto (Delta) de esta vela, e igualmente importante, cuál fue el rango interno (Delta Máx/Mín) de la batalla entre compradores y vendedores dentro de esa vela? |
| `ATR.cs` | [ATR](indicators/es/ATR.md) | Volatilidad | 8/10 | Conservar y Mejorar | ¿Cuál ha sido el tamaño verdadero promedio (incluyendo gaps) de cada barra durante los últimos X períodos? |
| `ATRN.cs` | [ATR Normalized](indicators/es/ATRN.md) | Volatilidad | 3/10 | Descartar | ¿Cuál es la volatilidad (ATR) del instrumento como un porcentaje de su precio actual? |
| `ACR.cs` | [Average Candle Range](indicators/es/ACR.md) | Volatilidad | 5/10 | Descartar o arreglar | ¿Cuál es el tamaño promedio de una vela en lo que va de día? |
| `ADR.cs` | [Average Daily Range](indicators/es/ADR.md) | Volatilidad | 7/10 | Conservar | ¿Cuál es el rango de movimiento "normal" o "promedio" para este instrumento en una sesión, y dónde se proyectarían esos límites hoy? |
| `AverageDelta.cs` | [Average Delta](indicators/es/AverageDelta.md) | Order Flow | 6.5/10 | Conservar (con reservas) | ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas, suavizando el ruido de vela a vela? |
| `AveragePriceBar.cs` | [Average Price for Bar](indicators/es/AveragePriceBar.md) | Precio | 2/10 | Descartar | ¿En lugar de solo el 'Cierre', cuál es el precio promedio interno (ej. Mediana, Típico) de cada vela individual? |
| `AO.cs` | [Awesome Oscillator](indicators/es/AO.md) | Momentum | 2/10 | Descartar | ¿Está el momentum reciente a corto plazo (5 barras) ganando la batalla contra el momentum de la tendencia a largo plazo (34 barras)? |
---

## B

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `ACBW.cs` | [Bill Williams AC](indicators/es/ACBW.md) | Momentum | 3/10 | Descartar | ¿El momentum (AO) está acelerando o frenando? |
---

## S

| Archivo (`.cs`) | Nombre del Indicador | Categoría | Puntuación Scalping | Veredicto | Descripción |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `Angle.cs` | [Study Angle](indicators/es/Angle.md) | Momentum | 2/10 | Descartar | ¿Cuál es el ángulo geométrico literal (en grados) de la tendencia del precio durante las últimas X barras? |
<!--stackedit_data:
eyJoaXN0b3J5IjpbNTk2NDk5NTUwXX0=
-->