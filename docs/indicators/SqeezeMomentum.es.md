Hecha la revisión y perfeccionamiento de la ficha. Este es un indicador muy conocido y potente, pero con matices importantes.

----------

## 🟦 Squeeze Momentum (8/10)

Nombre del archivo: SqueezeMomentum.cs

Nombre del indicador: Squeeze Momentum

Web oficial: https://help.atas.net/support/solutions/articles/72000602637

----------

### ⚙️ Parámetros configurables

-   **BBPeriod**: Periodo para Bandas de Bollinger (por defecto: `20`)
    
-   **BBMultFactor**: Multiplicador para Bandas de Bollinger (por defecto: `2.0`)
    
-   **KCPeriod**: Periodo para Keltner Channel (por defecto: `20`)
    
-   **KCMultFactor**: Multiplicador para Keltner Channel (por defecto: `1.5`)
    
-   **UseTrueRange**: Usar True Range para el Keltner Channel (por defecto: `false`)
    
-   **Upper/Up/Low/LowerColor**: Colores para el histograma de momentum (positivo/negativo y subiendo/bajando).
    
-   **True/False/NullColor**: Colores para los puntos en la línea cero que indican el estado del "Squeeze" (Compresión ON / Compresión OFF / Sin Squeeze).
    

----------

### 🧭 Clasificación

📂 Volatility / Momentum — Indicador híbrido que detecta compresión de volatilidad y mide el momentum direccional.

----------

### 🧠 Uso más frecuente

-   Detectar **compresión de volatilidad** (un "Squeeze") para anticipar una ruptura explosiva.
    
-   Medir el **momentum** (histograma) para confirmar la dirección y fuerza de la ruptura _después_ de que ocurra.
    
-   Filtrar operaciones: Evitar operar en rangos cuando el Squeeze está activo; buscar entradas cuando el Squeeze se "libera".
    

----------

### 📊 Nivel de relevancia

🔟 **8 / 10**

✅ Un "sistema" dos-en-uno muy popular y conceptualmente brillante (popularizado por John Carter como "TTM Squeeze").

✅ Los puntos del Squeeze (comparando BB vs KC) son una forma excelente de definir "calma antes de la tormenta".

⛔ El histograma de momentum tiene un lag considerable (es una LinReg(20) de un cálculo basado en SMA(20)). No es una señal de entrada rápida.

⛔ La ausencia de alertas nativas para la "liberación del Squeeze" es una carencia grave.

----------

### 🎯 Estrategias de scalping donde se aplica

-   **La Estrategia "Squeeze Play":**
    
    1.  **Esperar:** El scalper espera a que aparezcan los puntos de Squeeze (por defecto, `TrueColor = Black`). Esto indica compresión y "prohibido operar rangos".
        
    2.  **Apuntar:** El mercado está construyendo energía.
        
    3.  **Disparar:** La entrada se produce _cuando el Squeeze se libera_ (los puntos cambian a `FalseColor = Gray`) **Y** el histograma de momentum cruza la línea cero, confirmando la dirección de la explosión.
        
-   **Filtro de Contexto:** Usar los puntos de Squeeze como un "semáforo" de régimen de mercado.
    

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **BBPeriod**: `20`
    
-   **BBMultFactor**: `2.0`
    
-   **KCPeriod**: `20`
    
-   **KCMultFactor**: `1.5`
    
-   **Importante:** `UseTrueRange = true`. (El cálculo de Keltner es más robusto con True Range, el `false` por defecto es una mala elección).
    

✅ Los valores por defecto son el estándar de la industria y funcionan bien. La clave es activar `UseTrueRange`.

----------

### 🧪 Notas de desarrollo

Este indicador tiene dos componentes:

1.  **Squeeze Dots (Puntos en Línea Cero):**
    
    -   Compara las Bandas de Bollinger (volatilidad de StdDev) con el Keltner Channel (volatilidad de ATR).
        
    -   `sqzOn` (Squeeze Activo, `TrueColor`): Ocurre cuando las BBs se meten _dentro_ del KC. (`lowerBB > lowerKC && upperBB < upperKC`). La volatilidad de corto plazo es menor que el rango de mercado.
        
    -   `sqzOff` (Squeeze Liberado, `FalseColor`): Ocurre cuando las BBs están _fuera_ del KC. (`lowerBB < lowerKC && upperBB > upperKC`). La volatilidad explota.
        
2.  **Momentum Histogram (Histograma):**
    
    -   **NO** es un MACD.
        
    -   Calcula un oscilador: `Close - [ (Avg(High, Low) + SMA(Close, 20)) / 2 ]`
        
    -   A ese valor, le aplica una **Regresión Lineal (`LinReg(20)`)** para suavizarlo y proyectar su tendencia.
        
    -   El histograma se colorea según si el valor es positivo/negativo y si está subiendo o bajando respecto a la barra anterior (dando los 4 colores).
        

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   **¡LAG!** Es la mayor "incoherencia" funcional. El indicador es **lento** por diseño. El histograma de momentum depende de una `SMA(20)` y una `LinReg(20)`. Para un scalper de 1M, la señal de "cruce de cero" del histograma ocurrirá _varias velas después_ de que el precio haya iniciado la ruptura.
    
-   **`UseTrueRange = false` por defecto:** Es una mala decisión de diseño. El Keltner Channel moderno casi siempre usa True Range.
    
-   **Ausencia Total de Alertas:** El evento más importante ("la liberación del Squeeze") no tiene una alerta. El usuario tiene que vigilarlo visualmente, lo cual es ineficiente.
    

----------

### 🛠️ Propuestas de mejora

-   **¡Alertas!** Añadir alertas es crucial. Como mínimo:
    
    1.  Alerta para `sqzOn` (Inicio de compresión).
        
    2.  Alerta para `sqzOff` (Inicio de expansión / Ruptura).
        

----------

----------

> La Pregunta Clave: "¿Está el mercado 'cargando el muelle' (comprimido)? Y si es así, ¿hacia dónde se liberará la energía (momentum)?"
> 
> (Is the market 'loading the spring' (compressed)? And if so, where is the energy (momentum) being released?)

----------

### ✍️ Mi Opinión sobre el Indicador (El Análisis Correcto)

Este es un indicador "dos-en-uno" brillante, pero es fundamental entender sus partes y, sobre todo, su **velocidad**.

Parte 1: Los Puntos (El Semáforo de Volatilidad)

Esta es la parte más valiosa. La lógica de "BBs dentro de KCs" es una definición excelente y robusta de un "Squeeze".

-   **Puntos Negros (`TrueColor`):** "Modo Squeeze". El mercado está en una compresión de baja volatilidad. Te está diciendo: "Paciencia. No operes rangos, esto va a romper. Prepárate."
    
-   **Puntos Grises (`FalseColor`):** "Modo Expansión". El Squeeze se ha liberado. El muelle se ha soltado. Te está diciendo: "¡Ahora! El movimiento está en marcha."
    

Parte 2: El Histograma (El GPS Lento)

Esta es la parte que genera confusión. No es una señal de entrada rápida. Es una LinReg(20) aplicada a un cálculo basado en SMA(20). Es lento.

Para un scalper de 1M, cuando este histograma finalmente cruce el cero, el precio ya llevará 3 o 4 velas de movimiento. Usarlo como señal de _entrada_ es entrar tarde.

**El Error Común vs. El Uso Correcto:**

-   **Error Común:** Ver puntos negros (Squeeze) y entrar inmediatamente cuando el histograma se gira. (Entrada prematura, el Squeeze no se ha liberado).
    
-   **Error Común 2:** Esperar a que los puntos se vuelvan grises (Liberación) _y_ el histograma cruce el cero. (Entrada muy tardía, te has perdido la ignición).
    
-   **Uso Correcto (Scalping):** Usar los **Puntos** como el "Contexto" y tu sistema de Price Action/Order Flow como la "Entrada".
    
    1.  Ves **Puntos Negros** (Squeeze): Dejas de buscar reversiones. Pones el gráfico en modo "Breakout".
        
    2.  Ves que se forma un micro-rango, una acumulación.
        
    3.  El precio rompe ese rango con volumen/delta.
        
    4.  **Entras por Price Action/Volumen.**
        
    5.  Miras al SqueezeMomentum y ves que _justo ahora_ los puntos se vuelven **Grises**. Eso _confirma_ tu entrada. El histograma (lento) te servirá para _gestionar la salida_ (ej. salir cuando el histograma se gire).
        

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, es una herramienta de CONTEXTO de 10/10, pero una herramienta de ENTRADA de 3/10.**

Es radicalmente superior a `Dispersion` o `HVR` porque te da un estado binario (Squeeze ON/OFF) que tiene implicaciones tácticas directas (Esperar vs. Actuar).

No lo uses para encontrar _la_ entrada. Úsalo para decidir _qué tipo de entrada_ deberías estar buscando (reversión vs. ruptura). Para un scalper, saber cuándo "el muelle está cargado" es oro puro.

**Acción:** **Conservar.** (Y ajustar `UseTrueRange = true`).
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTExMzQwMTY1ODEsNzMwOTk4MTE2XX0=
-->