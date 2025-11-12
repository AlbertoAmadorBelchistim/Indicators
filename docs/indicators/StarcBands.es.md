## 🟦 Starc Bands (7/10)

Nombre del archivo: StarcBands.cs

Nombre del indicador: Starc Bands

Web oficial: https://help.atas.net/support/solutions/articles/72000602475

----------

### ⚙️ Parámetros configurables

-   **Period**: Periodo de la media móvil simple (SMA) (por defecto: `10`)
    
-   **SmaPeriod**: Periodo del ATR (Average True Range) (por defecto: `10`)
    
-   **TopBand**: Multiplicador del ATR para ambas bandas (por defecto: `1`)
    
-   **BotBand**: (Obsoleto) Parámetro inactivo y oculto.
    

----------

### 🧭 Clasificación

📂 Volatility / Channel — Bandas de volatilidad basadas en una media móvil y el ATR.

----------

### 🧠 Uso más frecuente

-   Establecer bandas de volatilidad en torno a una media móvil simple.
    
-   Detectar condiciones de sobreextensión (sobrecompra/sobreventa) cuando el precio toca las bandas.
    
-   Identificar puntos de reversión a la media desde los extremos del canal.
    

----------

### 📊 Nivel de relevancia

🔟 **7 / 10**

✅ Un canal de volatilidad clásico basado en ATR, útil para "fading" (operar contra la sobreextensión).

✅ Permite configurar por separado el período de la media (tendencia) y el del ATR (volatilidad).

⛔ Implementación "lite": sin alertas, sin sombreado de canal.

⛔ Totalmente redundante si ya se usa el indicador Keltner Channel.

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Reversión en Extremos (Fading):** Vender en la banda superior, comprar en la banda inferior (requiere `TopBand` > 1.5, por ej. `2.0`).
    
-   **Filtro de Rango:** Usar la SMA como eje central y las bandas como los límites esperados del "ruido".
    
-   **Confirmación de Tendencia:** Si el precio "camina" por una de las bandas, confirma una tendencia fuerte (aunque es una estrategia más tendencial que de scalping).
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **Period**: `20` (Media base)
    
-   **SmaPeriod**: `20` (Período de ATR)
    
-   **TopBand**: `2.0` (Un multiplicador estándar para capturar ~95% del precio)
    

✅ Simple y visualmente limpio (solo 3 líneas).

----------

### 🧪 Notas de desarrollo

-   Este indicador es una implementación de los "Stoller Average Range Channels" (STARC).
    
-   Es, en esencia, un **Keltner Channel**.
    
-   Línea Media: `SMA(Close, Period)`
    
-   Línea Superior: `SMA + (ATR(SmaPeriod) * TopBand)`
    
-   Línea Inferior: `SMA - (ATR(SmaPeriod) * TopBand)`
    
-   Usa el parámetro `TopBand` para calcular _ambas_ bandas (superior e inferior). El parámetro `BotBand` está marcado como `[Obsolete]` y no se usa.
    
-   Las bandas están configuradas para ser ignoradas por el sistema de alertas (`IgnoredByAlerts = true`).
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   **¡Error de Nomenclatura Grave!** El parámetro llamado `SmaPeriod` controla el período del **ATR**. El parámetro `Period` controla el período de la **SMA**. Esto es terriblemente confuso y propenso a errores de configuración por parte del usuario.
    
-   **Alertas Desactivadas:** El código deshabilita explícitamente las alertas en las bandas (`IgnoredByAlerts = true`). Esto elimina una funcionalidad clave para un scalper, que querría ser alertado al tocar un extremo.
    
-   **Parámetro Obsoleto:** El código aún contiene el parámetro `BotBand` como obsoleto, lo cual es solo ruido.
    
-   **Implementación "Lite":** A diferencia del indicador `KeltnerChannel` de ATAS, este no incluye un sombreado del canal (`RangeDataSeries`), siendo visualmente más pobre.
    

----------

### 🛠️ Propuestas de mejora

-   **Renombrar `SmaPeriod` a `AtrPeriod`** para que coincida con lo que realmente hace.
    
-   Eliminar la línea `IgnoredByAlerts = true` para permitir que los usuarios configuren alertas en las bandas.
    
-   Eliminar completamente el parámetro obsoleto `BotBand`.
    

----------

----------

> La Pregunta Clave: "Basado en la volatilidad promedio (ATR) de las últimas X velas, ¿dónde están los límites estadísticos 'normales' del precio alrededor de su media (SMA) de las últimas Y velas?"
> 
> (Based on the average volatility (ATR) of the last X bars, where are the 'normal' statistical boundaries around its mean (SMA) of the last Y bars?)

----------

### ✍️ Mi Opinión sobre el Indicador (El Análisis Correcto)

Este indicador es un **clon redundante** del `Keltner Channel`.

Acabamos de analizar el KeltnerChannel.cs (un 7/10) que calculaba SMA(Period) ± (ATR(Period) * Koef).

Este StarcBands.cs calcula SMA(Period) ± (ATR(SmaPeriod) * TopBand).

Es la **misma fórmula exacta**. La única diferencia es que `KeltnerChannel` usaba un solo parámetro (`Period`) para la SMA y el ATR, mientras que `StarcBands` te permite (correctamente) tener períodos separados.

Entonces, ¿por qué es peor?

1.  **Redundancia:** Es la definición de "código hinchado" (bloat) tener dos indicadores que hacen lo mismo.
    
2.  **Implementación Pobre:** Es una versión "lite" y perezosa. El `KeltnerChannel` tenía un canal sombreado (`RangeDataSeries`) y un sistema de alertas (aunque tuviera un bug). Este `StarcBands` no tiene sombreado y, peor aún, el programador _desactivó deliberadamente_ las alertas (`IgnoredByAlerts = true`).
    
3.  **Confusión Garantizada:** El error de nombrar `SmaPeriod` al parámetro que controla el ATR es imperdonable. Un usuario novato _nunca_ sabrá qué está configurando. Pensará que está ajustando dos SMAs.
    

En resumen: ATAS tiene dos Keltner Channel. Uno (el `KeltnerChannel`) está casi bien, pero tiene un bug en las alertas y acopla los períodos. El otro (este `StarcBands`) arregla el acoplamiento de períodos (lo cual es bueno), pero lo hace con un nombre confuso, deshabilita las alertas y elimina el sombreado visual.

### 📈 Veredicto: ¿Es útil para Scalping?

**No. Es redundante y funcionalmente inferior.**

Si un scalper quiere un canal de ATR, debe usar el indicador `KeltnerChannel` que ya "Conservamos (con Ajustes)". Ese indicador es superior porque _intenta_ tener alertas (aunque necesite una corrección de bug) y proporciona un sombreado visual más claro.

Este `StarcBands` es un clon perezoso. La única ventaja teórica que tiene (períodos desacoplados) está totalmente saboteada por un nombre de parámetro que garantiza la confusión del usuario. No aporta valor.

**Acción:** **Descartar.**
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE3NDU5NDQ1NzhdfQ==
-->