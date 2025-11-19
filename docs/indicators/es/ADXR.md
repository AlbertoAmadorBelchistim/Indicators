---
cs_file: ADXR.cs
name: ADXR
category: Trend
group: Trend
subgroup: Trend Filter
score_current: 3/10
version: Estable
recommended_action: Descartar
description: ¿Cuál es la fuerza _estable y suavizada_ de la tendencia, ignorando el
  ruido a corto plazo del propio ADX?
gemini_summary: '"Lag sobre lag" (3/10). Es un promedio del ADX (que ya es lento).
  Inútil para scalping.'
file_state: Estable (Conceptualm. Roto)
score_potential: 3/10
effort: N/A
action_priority: P4 (Descartar)
analysis_date: 2025-11-17
official_code_date: 23/04/2025
---

## 🟦 ADXR (3/10)

**Nombre del archivo:** [`ADXR.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ADXR.cs)  
**Nombre del indicador:** ADXR  
**Web oficial:** [ATAS - ADXR](https://help.atas.net/support/solutions/articles/72000602314)  
**Compatibilidad**: ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la fuerza _estable y suavizada_ de la tendencia, ignorando el ruido a corto plazo del propio ADX?

![ADXR](../../img/ADXR.png)

----------

### ⚙️ Parámetros configurables

-   **Period**: Número de barras de retraso para el promedio (por defecto: `2`)
    
-   **AdxPeriod**: Periodo utilizado por el indicador ADX interno (por defecto: `14`)
    

----------

### 🧭 Clasificación

📂 Trend — Indicadores de fuerza de tendencia (derivado de ADX)

----------

### 🧠 Uso más frecuente

-   Confirmar fuerza de tendencia suavizada y reducir ruido de ADX puro.
    
-   Evitar entradas en consolidaciones.
    
-   Estimar estabilidad de una tendencia en curso (para swing o position trading).
    

----------

### 📊 Nivel de relevancia

🔟 **3 / 10**

✅ Suaviza el ADX (útil para traders de gráficos diarios que no quieren reaccionar al ruido).

⛔ LAG EXTREMO: Es un promedio de un indicador (ADX) que ya es un promedio de un promedio. Es uno de los indicadores más lentos que existen.

⛔ CONCEPTUALMENTE INÚTIL (PARA SCALPING): Añade lag al lag, haciéndolo lo opuesto a lo que un scalper (que necesita velocidad) requiere.

⛔ Redundante.

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Ninguna.**
    
-   Este indicador es demasiado lento para CUALQUIER estrategia de scalping. Sus señales llegarán entre 30 y 60 minutos tarde en un gráfico de 1M.
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **No se recomienda su uso para scalping.**
    

----------

### 🧪 Notas de desarrollo

-   El indicador usa un ADX interno (`_adx`) como sub-indicador.
    
-   El ADXR es un promedio simple del valor actual del ADX y el valor del ADX de `Period` barras atrás.
    
-   Fórmula: `ADXR[bar] = (_adx[bar] + _adx[bar - _period]) / 2`
    
-   El `Period` (defecto: 2) no es un "período de SMA", sino el _offset_ de la segunda muestra.
    
-   Se fuerza un valor mínimo de `0.01` al resultado final.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   El indicador es funcional y fiel a su fórmula. La incoherencia es puramente conceptual: es un indicador de _fuerza de tendencia_ diseñado para ser tan lento que solo es aplicable a timeframes muy altos (Diario, Semanal), pero se ofrece junto a herramientas de scalping.
    

----------

### 🛠️ Propuestas de mejora

-   No hay mejoras que "arreglen" este indicador para el scalping. Su diseño fundamental es ser lento.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

Ya habíamos establecido que el **ADX (6/10)** era un "filtro de régimen" con un **lag extremo**. Era una herramienta lenta que nos decía qué _había pasado_ en los últimos 30-60 minutos.

El **ADXR** coge ese indicador lento y con lag... y **le añade más lag** al aplicarle una media móvil encima.

-   El ADX te dice (tarde) que hay una tendencia.
    
-   El ADXR te lo dice _aún más tarde_.
    

Para un scalper que opera en gráficos de 1M o 5M, el lag es el enemigo. Necesitas información _ahora_. El ADXR es conceptualmente lo **opuesto** a lo que un scalper necesita.

Mira la captura de pantalla: la línea del ADXR es una curva lenta y suave que ignora por completo la acción del precio. A las 09:25, cuando el precio se desploma, el ADXR sigue subiendo perezosamente. Es una herramienta diseñada para traders de gráficos diarios o semanales, para que no se asusten por las correcciones de 2-3 días.

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**No. Es completamente inútil para scalping.**

Es un indicador que _aumenta_ el lag, el enemigo número uno del scalper.

**Acción:** **Descartar.**

**¿Merece la pena arreglarlo?** **No.** El indicador no está "roto"; está "diseñado para ser lento". No hay nada que arreglar. Simplemente no pertenece a la caja de herramientas de un scalper.
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTk0MjU4ODM5OV19
-->