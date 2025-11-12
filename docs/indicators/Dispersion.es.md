## 🟦 Dispersion (6/10)

Nombre del archivo: Dispersion.cs

Nombre del indicador: Dispersion

Web oficial: https://help.atas.net/support/solutions/articles/72000602626

----------

### ⚙️ Parámetros configurables

-   **Period**: Número de barras para calcular la desviación respecto a la SMA (por defecto: 10)
    

----------

### 🧭 Clasificación

📂 Volatility — Indicadores de variabilidad del precio

----------

### 🧠 Uso más frecuente

-   Medir la **dispersión o variabilidad del precio** en torno a su media
    
-   Evaluar contextos de **alta o baja volatilidad**
    
-   Filtrar estrategias según el entorno de mercado (rango vs tendencia)
    

----------

### 📊 Nivel de relevancia

🔟 **6 / 10**

✅ Simple y efectivo para medir volatilidad local

✅ Útil para complementar estrategias de breakout o reversión

⛔ No distingue dirección del movimiento

⛔ Puede ser redundante si se usa con otros indicadores como ATR o desviación estándar

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Filtro de rango**: evitar entradas en zonas de dispersión baja
    
-   **Confirmación de breakout**: entrada solo si la dispersión se incrementa rápidamente
    
-   **Detección de compresión**: buscar rupturas tras caída del indicador
    

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **Period**: `10`
    
-   Línea base: usar promedio móvil simple del indicador para contextualizar
    
-   Usar en conjunto con ATR, volumen o delta para validar zonas de breakout
    

✅ Rápido de interpretar

✅ Mejora la discriminación entre mercado activo o plano

----------

### 🧪 Notas de desarrollo

-   Calcula la dispersión como el promedio de los cuadrados de la diferencia entre el precio y su SMA:
    
    $$ \text{Dispersion}_t = \frac{1}{n} \sum_{i=0}^{n-1} (x_{t-i} - \text{SMA}_{t-i})^2$$
    
-   Usa internamente dos `ValueDataSeries`:
    
    -   `_diffSeries`: diferencias cuadradas
        
    -   `_renderSeries`: valor final mostrado
        
-   El cálculo es similar a una **varianza suavizada**, aunque sin raíz cuadrada (por tanto no es desviación estándar)
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   La fórmula implementada **no aplica raíz cuadrada**, por lo que el valor resultante **no es comparable directamente con desviación estándar**
    
-   Usa `CalcSum(_sma.Period - 1, bar)`, lo cual omite una barra en la media (debería ser `Period` o documentarse la intención)
    
-   No ofrece referencia visual como línea base o umbrales de volatilidad alta/baja
    

----------

### 🛠️ Propuestas de mejora

-   Añadir opción para calcular también la **desviación estándar (sqrt)**
    
-   Permitir representar una **línea base media móvil** del indicador
    
-   Añadir alertas visuales si se supera cierto umbral de dispersión
    
-   Mostrar etiquetas o zonas coloreadas cuando el valor está por encima o por debajo de niveles clave

### Comentario Gemini
> La Pregunta Clave: "¿Está el precio 'pegado' a su media (comprimido) o está 'explotando' lejos de ella (volátil)?"
> 
> (Is the price 'stuck' to its average (compressed) or 'exploding' away from it (volatile)?)

----------

### ✍️ Mi Opinión sobre el Indicador (El Análisis Correcto)

Este indicador intenta medir la volatilidad, pero lo hace de la forma menos intuitiva y técnicamente más cuestionable posible.

-   **La Métrica Equivocada (El Pecado Original):** El indicador calcula la **Varianza** (o algo parecido), no la Desviación Estándar. Muestra el valor en _unidades cuadradas_ ($ticks^2$). Un scalper no piensa en "ticks al cuadrado". Si la volatilidad real (distancia del precio a la media) se duplica de 1 tick a 2 ticks, el indicador se cuadruplica (de 1 a 4). Esto lo hace visualmente engañoso e imposible de usar para establecer umbrales fiables.
    
-   **El Error de Cálculo:** Como se detectó en la ficha, el código suma los cuadrados de `Period - 1` barras, pero luego divide entre `Period`. Es un error técnico en la implementación de la (ya de por sí pobre) fórmula de la varianza. No es ni una cosa ni la otra.
    
-   **Es Lento y Redundante:** Es un indicador de volatilidad con doble lag. Primero, calcula la diferencia contra una `SMA` (que ya tiene lag). Segundo, calcula un _promedio_ de esa diferencia (más lag).
    

### 📈 Veredicto: ¿Es útil para Scalping?

Categóricamente, **no**. Es una herramienta inferior en todos los sentidos a las que ya tenemos y a los estándares de la industria.

1.  **Si quieres medir la volatilidad (el rango):** Usa **ATR (Average True Range)**. Es el estándar global, se mide en ticks/puntos (lo cual es intuitivo) y responde más rápido.
    
2.  **Si quieres medir la desviación de la media:** Usa **Bollinger Bands** o el indicador **Standard Deviation** (que sí aplica la raíz cuadrada). Estas herramientas usan la métrica correcta y son mucho más útiles, especialmente las BB, que se trazan sobre el precio.
    

Este indicador es un intento fallido de crear una "Desviación Estándar" que olvida la parte más importante: la raíz cuadrada. No aporta valor.

**Acción:** **Descartar.**
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE4MTc0ODUyMzEsLTIwNTEwNjU2ODcsND
MzMTE4MzI4XX0=
-->