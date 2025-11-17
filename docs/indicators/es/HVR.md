## 🟦 Historical Volatility Ratio (7/10)

Nombre del archivo: HVR.cs

Nombre del indicador: Historical Volatility Ratio

Web oficial: https://help.atas.net/support/solutions/articles/72000602393

----------

### ⚙️ Parámetros configurables

-   **ShortPeriod**: Periodo de la desviación estándar de corto plazo (por defecto: 6)
    
-   **LongPeriod**: Periodo de la desviación estándar de largo plazo (por defecto: 100)
    

----------

### 🧭 Clasificación

📂 Volatility — Indicadores de régimen de volatilidad (Relativa)

----------

### 🧠 Uso más frecuente

-   Evaluar si la **volatilidad reciente (corta)** es anómalamente alta o baja en comparación con la **volatilidad histórica (larga)**.
    
-   Detectar transiciones de régimen: de compresión (HVR < 1) a expansión (HVR > 1).
    
-   Filtrar estrategias: activar setups de breakout en expansión, activar setups de reversión en compresión.
    

----------

### 📊 Nivel de relevancia

🔟 **7 / 10**

✅ Implementa un cálculo de volatilidad robusto (basado en log-returns), mucho más "Quant" que la media.

✅ Mide la volatilidad de forma relativa (un ratio), lo que lo hace un excelente filtro de contexto.

⛔ No aporta ninguna información sobre la dirección del precio.

⛔ Es un indicador de "estado" (contexto), no de "timing" (entrada).

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Filtro de Breakout (Expansión):** Buscar rupturas solo cuando HVR es > 1 (y preferiblemente > 1.2 o 1.5), indicando que la volatilidad reciente está superando a la histórica.
    
-   **Filtro de Rango (Compresión):** Buscar reversiones a la media (scalps contra extremos de rango) solo cuando HVR es < 1 (y preferiblemente < 0.8), indicando un mercado tranquilo o comprimido.
    
-   **Alerta de "Quiet Squeeze":** Un valor de HVR extremadamente bajo (ej. < 0.5) puede preceder a un movimiento explosivo (compresión que anticipa la ruptura).
    

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **ShortPeriod**: `6`
    
-   **LongPeriod**: `50` (El 100 por defecto es demasiado lento para un gráfico de 1M, 50-60 barras es más ágil).
    
-   **Visualización**: Trazar una línea horizontal clave en el nivel `1.0`.
    

✅ Es la herramienta perfecta para decidir _qué tipo de setup_ operar (tendencial o de rango).

----------

### 🧪 Notas de desarrollo

-   El indicador NO calcula la desviación estándar del precio, sino de los **retornos logarítmicos** (log-returns).
    
-   **Paso 1:** Calcula el log-return:
    
    $$ lr_t = \log\left(\frac{\text{Close}_t}{\text{Close}_{t-1}}\right)$$
    
-   **Paso 2:** Calcula dos desviaciones estándar (StdDev) sobre esos log-returns:
    
    -   $\sigma_{\text{short}} = \text{StdDev}(lr, \text{ShortPeriod})$
        
    -   $\sigma_{\text{long}} = \text{StdDev}(lr, \text{LongPeriod})$
        
-   **Paso 3:** El valor final es el ratio de ambas:
    
    $$ \text{HVR}_t = \frac{\sigma_{\text{short}}}{\sigma_{\text{long}}}$$
    
-   Esta es una metodología de análisis de volatilidad muy robusta y estándar en finanzas cuantitativas.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   El código no maneja la (rara pero posible) excepción de una división por cero si $\sigma_{\text{long}}$ fuera 0 (lo cual solo pasaría si todos los log-returns en el periodo largo fuesen idénticos, algo imposible en la práctica).
    
-   El valor por defecto de `LongPeriod = 100` es adecuado para gráficos diarios (visión de 5 meses), pero excesivamente lento para scalping en 1M (casi 2 horas de datos).
    

----------

### 🛠️ Propuestas de mejora

-   Añadir líneas de referencia horizontales (ej. en 0.5, 1.0, 1.5) para definir zonas de "compresión", "normal" y "expansión".
    
-   Permitir un suavizado (SMA/EMA) opcional del HVR resultante para reducir el ruido.
    

----------

----------

> La Pregunta Clave: "¿Está el mercado 'comprimido' o 'explotando' en relación con su comportamiento normal de las últimas horas?"
> 
> (Is the market 'compressed' or 'exploding' relative to its normal behavior over the last few hours?)

----------

### ✍️ Mi Opinión sobre el Indicador (El Análisis Correcto)

Este indicador es **inteligente**. A diferencia del desastroso `Dispersion` (que calculaba la varianza del precio), este HVR hace lo correcto: mide la volatilidad usando **log-returns**.

Esta es la forma estándar en que los analistas "Quant" miden la volatilidad, ya que normaliza los cambios (un movimiento de 10 a 11 puntos, +10%, es más volátil que uno de 100 a 101 puntos, +1%).

El indicador, por tanto, no mide la volatilidad en "ticks" (como el ATR), sino que crea un **ratio normalizado**.

-   Si **HVR = 1.0**, la volatilidad de corto plazo es _exactamente igual_ a la de largo plazo. Es el equilibrio.
    
-   Si **HVR > 1.0**, el mercado se está "calentando" (expansión de volatilidad).
    
-   Si **HVR < 1.0**, el mercado se está "enfriando" (compresión de volatilidad).
    

Es un indicador puramente de **contexto** o **régimen**. No te dice _dónde_ entrar, sino _cómo_ deberías estar operando (buscando breakouts o buscando reversiones).

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, es muy útil como filtro de régimen.**

No es un indicador de entrada, y un scalper no debería estar mirándolo en cada vela. Es una herramienta que se consulta para definir el plan de trading de la sesión:

1.  **¿Buscamos breakouts?** Mira el HVR. Si está por encima de 1.2 y subiendo, "luz verde" para estrategias de momentum y ruptura.
    
2.  **¿Buscamos rangos?** Mira el HVR. Si está por debajo de 0.8 y estable, "luz verde" para desvanecer (fade) los extremos y buscar reversiones a la media.
    

Es el "Termostato de Volatilidad" del mercado. Establece la temperatura y te dice qué ropa ponerte (qué estrategia usar). Complementa perfectamente a otros indicadores de entrada.

**Acción:** **Conservar.** (Ajustar `LongPeriod` a `50`-`60` para 1M).
<!--stackedit_data:
eyJoaXN0b3J5IjpbNjQyODc2NDg1XX0=
-->