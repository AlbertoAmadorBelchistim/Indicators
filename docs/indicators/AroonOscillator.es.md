Aquí tienes la ficha que habríamos creado, que confirma por qué es un descarte.

> The Key Question: "Which force is stronger and more recent: the one making new highs (bulls) or the one making new lows (bears)?"
> 
> (¿Qué fuerza es más fuerte y reciente: la que está creando nuevos máximos (alcistas) o la que está creando nuevos mínimos (bajistas)?)

----------

## 🟥 Aroon Oscillator (3/10)

Nombre del archivo: AroonOscillator.cs

Nombre del indicador: Aroon Oscillator

Web oficial: https://help.atas.net/support/solutions/articles/72000602317

----------

### ⚙️ Parámetros configurables

-   **Period**: Periodo del indicador Aroon interno (por defecto: 10)
    

----------

### 🧭 Clasificación

📂 Momentum — Indicadores de fuerza relativa de tendencia y reversión

----------

### 🧠 Uso más frecuente

-   Identificar la **dirección de la tendencia** basándose en un único oscilador.
    -   **Valores > 0** indican que el AroonUp es más fuerte que el AroonDown (tendencia alcista).
    -   **Valores < 0** indican que el AroonDown es más fuerte (tendencia bajista).
    
-   El cruce por la línea cero se usa (en teoría) como señal de cambio de tendencia.
    

----------

### 📊 Nivel de relevancia

🔟 **3 / 10**

✅ Simplifica las dos líneas del Aroon en una sola.
⛔ Hereda todos los problemas del Aroon Indicator: es ruidoso, lento y errático.
⛔ Como se ve en la captura de pantalla, el resultado es un zig-zag "digital" (no suave) que es inutilizable para el scalping.

----------

### 🎯 Estrategias de scalping donde se aplica

-   (Teóricamente) Comprar al cruzar el nivel 0 hacia arriba; vender al cruzar hacia abajo.
-   **En la práctica, no es aplicable**. La señal es demasiado ruidosa y llega demasiado tarde.
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **Ninguna**. El indicador es conceptualmente inadecuado para el scalping en alta frecuencia.
    

----------

### 🧪 Notas de desarrollo

-   Este es un indicador "envoltorio" (wrapper). No calcula nada por sí mismo.
-   Crea una instancia interna del `AroonIndicator` (el que vimos en el paso anterior). 
-   Su única lógica es `_renderSeries[bar] = AroonUp[bar] - AroonDown[bar]`.
-   Es simplemente una resta de los dos componentes del indicador anterior.
    

----------

### 📈 Veredicto

**Descartar, con más motivo que el anterior.**

Este indicador coge los datos ruidosos y lentos del `AroonIndicator` y los presenta de una forma diferente, pero no mejor. Es un indicador de libro de texto diseñado para gráficos diarios de acciones, y es fundamentalmente inútil para el scalping del S&P 500.
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTExNzU3MDMyNzksMjIzNzA0NjIzXX0=
-->