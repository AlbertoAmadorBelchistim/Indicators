---
cs_file: ACR.cs  
name: Average Candle Range  
category: Volatilidad  
score: 5/10  
version: Estable  
verdict: Descartar  
description: ¿Cuál es el tamaño promedio de una vela en lo que va de día?  
---
﻿
## 🟦 Average Candle Range (5/10)

**Nombre del archivo:** `ACR.cs`  
**Nombre del indicador:** Average Candle Range  
**Web oficial:** [ATAS — Average Candle Range](https://help.atas.net/support/solutions/articles/72000602323)  
**Compatibilidad:** ATAS versión estable y superiores.
>**La Pregunta Clave:** ¿Cuál es el tamaño promedio de una vela en lo que va de día?

![ACR](../../img/ACR.png)

----------

### ⚙️ Parámetros configurables

-   **IgnoreWicks**: Ignorar mechas; si está activado, se calcula solo la diferencia entre apertura y cierre (cuerpo de la vela).
    

----------

### 🧭 Clasificación

📂 Volatility — Indicadores de rango promedio diario

----------

### 🧠 Uso más frecuente

-   Medir la **volatilidad promedio** de las velas _desde el inicio de la sesión_.
    
-   Evaluar si el rango actual está **por encima o por debajo del promedio reciente**
    
-   Filtrar operaciones si el mercado está demasiado estrecho o extendido
    

----------

### 📊 Nivel de relevancia

🔟 **5 / 10**

✅ Útil para tener un contexto de "qué tipo de día" es (volátil o comprimido).

✅ La opción IgnoreWicks lo convierte en un "filtro de chop" (medidor de tamaño de cuerpo).

⛔ Lógica de Cálculo Engañosa: No mide la volatilidad reciente, sino el promedio de toda la sesión, lo que "diluye" la información de la tarde con la de la mañana.

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Contracción de rango**: identificar consolidaciones previas a ruptura.
    
-   **Rango excesivo**: evitar entradas cuando el rango ya excede el promedio.
    
-   **Validación post-entrada**: confirmar que el movimiento tiene fuerza si supera el rango medio.
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **IgnoreWicks**: `true` (Esto es más útil, ya que mide el "Tamaño de Cuerpo Promedio" del día, un buen filtro de mercado lateral).
    
-   _Nota: No hay más configuración, ya que el promedio se reinicia por sesión y no tiene un "período"._
    

----------

### 🧪 Notas de desarrollo

-   El indicador calcula el rango de cada vela y lo acumula en `_rangeSeries`.
    
-   Si `IgnoreWicks` está activado, el rango es `|Open - Close|`, de lo contrario es `High - Low`.
    
-   Se reinicia con cada nueva sesión (`IsNewSession(bar)`).
    
-   El resultado (`_renderSeries`) es el **promedio de todas las velas desde el inicio de la sesión** hasta la barra actual, usando `_rangeSeries.CalcAverage(bar - _lastSession + 1, bar)`.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   El nombre "Average Candle Range" es genérico. Un usuario esperaría un promedio móvil (como un ATR), no un "promedio de sesión en expansión". La lógica es contraintuitiva para un scalper.
    
-   El valor del indicador es muy sensible al inicio de la sesión y se vuelve progresivamente más lento e "insensible" a medida que avanza el día.
    

----------

### 🛠️ Propuestas de mejora

-   **¡La mejora clave!:** Añadir una opción para **promedio móvil deslizante** (ej. `Period = 20`) en lugar del promedio de sesión.
    
-   Incluir una línea de referencia horizontal con el valor promedio actual.
    
-   Permitir mostrar tanto el rango de la vela actual como el promedio superpuestos.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

Este indicador es un ejemplo perfecto de una buena idea con una implementación que la hace inútil para el scalping.

El código implementa una lógica muy particular. La línea clave es:

_renderSeries[bar] = _rangeSeries.CalcAverage(bar - _lastSession + 1, bar);

Como tus "Notas de desarrollo" han detectado perfectamente, esto calcula el **promedio de rango de todas las velas desde que empezó la sesión**.

-   A las 09:31 (vela 1 de la sesión), el promedio es solo de 1 vela.
    
-   A las 09:32 (vela 2), es el promedio de 2 velas.
    
-   A las 15:00, es el promedio de cientos de velas.
    

**El problema:** Esto significa que el valor del indicador por la tarde está "contaminado" por la volatilidad (o falta de ella) de la mañana. No te dice cuál es la volatilidad _reciente_, sino cuál es la volatilidad _promedio de todo el día hasta ahora_. Un scalper necesita saber la volatilidad de los últimos 10-20 minutos, no el promedio de las últimas 5 horas.

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**No. En su estado actual, es inútil para la toma de decisiones.**

Un scalper que vea un valor "bajo" en este indicador por la tarde no sabrá si es porque la tarde está en calma, o si es porque la mañana fue _extremadamente_ calma y está arrastrando el promedio hacia abajo.

-   **Como está (Promedio de Sesión): 4/10.** Utilidad muy limitada, solo para un vago contexto diario.
    
-   **Con `IgnoreWicks = true`: 6/10.** Mejora. Se vuelve un "Tamaño de Cuerpo Promedio del Día", un filtro de "mercado de rango" decente.
    
-   **Con la "Propuesta de Mejora" (Promedio Móvil): 9/10.** Si se implementara esa mejora (un `Period` y un `SMA`), este indicador se transformaría en un **ATR**, una herramienta _esencial_ para el scalping (gestión de stops, objetivos, filtro de chop, etc.).
    

**Acción:** **Descartar (en su estado actual).**

**¿Merece la pena arreglarlo?** **SÍ, ABSOLUTAMENTE.** Es el candidato número uno para ser "arreglado". El código base es bueno. Con solo añadir un parámetro `Period` y cambiar _una línea_ de cálculo (usar `SMA` en lugar de `CalcAverage`), este indicador pasaría de ser un "descarte" a ser una pieza central de un sistema de scalping.
<!--stackedit_data:
eyJoaXN0b3J5IjpbMzc1MDcxMTI5LDEwOTQyNzg3NzZdfQ==
-->