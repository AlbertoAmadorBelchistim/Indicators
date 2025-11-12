-

## 🟦 Standard Deviation Bands (4/10)

Nombre del archivo: StdDevBands.cs

Nombre del indicador: Standard Deviation Bands

Web oficial: https://help.atas.net/support/solutions/articles/72000602614

----------

### ⚙️ Parámetros configurables

-   **Period** (por defecto: `10`): Periodo usado para _todos_ los cálculos (SMA, StdDev, Highest, Lowest).
    
-   **SmaPeriod** (por defecto: `2`): **¡PARÁMETRO CRÍTICAMENTE ENGAÑOSO!** A pesar de su nombre, esta variable (`_width`) controla el **Multiplicador de Ancho** de las bandas. En la UI de ATAS, se etiqueta como "BBandsWidth".
    

----------

### 🧭 Clasificación

📂 Volatility / Channel (No EstándAR)

----------

### 🧠 Uso más frecuente

-   (Intento) de crear bandas de volatilidad similares a las Bandas de Bollinger.
    
-   Usar las 4 líneas (dos medias, dos bandas) y sus alertas para trading de rejilla (grid).
    

----------

### 📊 Nivel de relevancia

🔟 **4 / 10**

✅ Implementa un sistema de alertas muy completo para 4 líneas (2 medias, 2 bandas).

⛔ ¡CÁLCULO EXÓTICO Y ENGAÑOSO! El nombre "Standard Deviation Bands" sugiere Bandas de Bollinger (SMA(Close) ± StdDev(Close)). Este indicador NO ES ESO.

⛔ El cálculo se basa en la media y desviación de los máximos más altos (Highest) y los mínimos más bajos (Lowest), no del precio de cierre.

⛔ Error de Nomenclatura Fatal: El parámetro SmaPeriod controla el multiplicador de ancho (_width), lo cual garantiza la confusión del usuario.

----------

### 🎯 Estrategias de scalping donde se aplica

-   Dado el cálculo no estándar, es imposible aplicar estrategias estándar.
    
-   (Teóricamente) Reversión a la media cuando el precio toca las bandas exteriores, pero los niveles de las bandas son arbitrarios y no tienen la base estadística de las Bandas de Bollinger.
    

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **Period**: `20` (Estándar para bandas)
    
-   **SmaPeriod**: `2` (Recordando que este parámetro es el _ancho_, no un periodo).
    

✅ Las alertas integradas son rápidas.

⛔ La lógica de las bandas no es fiable.

----------

### 🧪 Notas de desarrollo

-   Este indicador traza **4 líneas**, no 3.
    
-   **Línea Media Superior:** `_smaTopSeries = SMA(Highest(High, Period))`
    
-   **Línea Media Inferior:** `_smaBotSeries = SMA(Lowest(Low, Period))`
    
-   **Banda Exterior Superior:** `_topSeries = SMA(Highest) + (StdDev(Highest) * Width)`
    
-   **Banda Exterior Inferior:** `_botSeries = SMA(Lowest) - (StdDev(Lowest) * Width)`
    
-   El parámetro `Width` se llama `SmaPeriod` en el código.
    
-   El indicador NO se parece en nada a las Bandas de Bollinger.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

1.  **¡Cálculo Exótico / Engañoso!** El problema principal. El indicador se llama "Standard Deviation Bands" pero no implementa la fórmula estándar. Calcula la desviación estándar de los valores más extremos (Highest/Lowest), no del precio, lo que invalida su uso estadístico.
    
2.  **¡Error de Nomenclatura de Parámetros!** El parámetro `SmaPeriod` (un `int`) controla la variable `_width` (el multiplicador). Esto es un error de programación básico que hace imposible que un usuario configure el indicador correctamente sin leer el código.
    
3.  **¡Bug de Alerta!** Mismo error de copiar y pegar que en `KeltnerChannel`. La alerta de la banda inferior (`BotAlert`) está configurada para usar el archivo de sonido de la banda superior (`AlertFileTop`).
    
4.  **Colores Fijos:** Las bandas están fijadas en `DodgerBlue` y no se pueden configurar por el usuario.
    
5.  **Alertas Redundantes:** El código deshabilita las alertas nativas (`IgnoredByAlerts = true`) para implementar su propio sistema de alertas `CheckAlerts()`, lo cual es un diseño redundante.
    

----------

### 🛠️ Propuestas de mejora

-   **Descartar este indicador.**
    
-   Si se quisiera arreglar:
    
    1.  Renombrar `SmaPeriod` a `Width` y cambiar su tipo a `decimal`.
        
    2.  Corregir el bug de la alerta `AlertFileTop`.
        
    3.  **Lo más importante:** Tirar el cálculo a la basura y reemplazarlo por el de las Bandas de Bollinger reales (`SMA(Close) ± StdDev(Close) * Width`).
        

----------

----------

> La Pregunta Clave: "Si ignoramos el precio, y solo miramos la media de los máximos más altos y la media de los mínimos más bajos... ¿cuál es la desviación estándar _de esos dos valores_?"
> 
> (If we ignore the price, and only look at the average of the highest highs and the average of the lowest lows... what is the standard deviation _of those two values_?)

----------

### ✍️ Mi Opinión sobre el Indicador (El Análisis Correcto)

Este es el peor indicador que hemos analizado hasta ahora, y por un margen amplio. Es peor que un indicador inútil, es un indicador **engañoso**. Es un "Caballo de Troya".

Cuando descartamos `StdDev` (el "ladrillo"), dijimos que buscábamos la "casa" (el "plato cocinado"). Este indicador se presenta como esa casa, se llama "Standard Deviation Bands", pero los cimientos están podridos y la arquitectura es de un loco.

1.  **El Engaño del Nombre:** Se llama "Standard Deviation Bands", lo que el 99.9% de los traders asociará con **Bollinger Bands**. No lo es. Cualquier estrategia basada en Bollinger Bands (BB Squeeze, reversión en 2-StdDev) fallará catastróficamente con esto.
    
2.  **El Cálculo Absurdo:** No tiene sentido estadístico. Calcula la media de los máximos (`SMA(Highest)`) y luego añade una desviación estándar _de esos mismos máximos_ (`StdDev(Highest)`). Es como decir "la media de los más altos, más un poco más". Es una tautología, no un análisis.
    
3.  **El Error de Programación:** Que el parámetro `SmaPeriod` controle el `_width` es una vergüenza. Es un error de programación de nivel 101. Un usuario pondrá `Period=20` y `SmaPeriod=20` (pensando que son dos medias) y acabará con un multiplicador de ancho de 20, viendo unas bandas a 500 puntos del precio.
    

Este indicador es un ejemplo perfecto de "código basura" (garbage code) que ha llegado a producción. Es confuso, incorrecto, tiene bugs (el de la alerta) y no cumple la función que su propio nombre promete.

### 📈 Veredicto: ¿Es útil para Scalping?

**No. Es activamente peligroso.**

Usar este indicador es peor que no usar nada, porque te dará señales (toques de banda) que no tienen ninguna base estadística o lógica. Un scalper que intente "comprar en la banda inferior" estará comprando en un nivel completamente arbitrario que solo depende de `SMA(Lowest(Low)) - StdDev(Lowest(Low))`.

Es un indicador que _debe_ ser eliminado del arsenal de un trader.

**Acción:** **Descartar (¡Peligroso!).**
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTYxMjg1MTcxMyw3MzA5OTgxMTZdfQ==
-->