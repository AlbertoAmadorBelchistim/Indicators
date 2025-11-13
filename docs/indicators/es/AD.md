---
cs_file: AD .cs
name: Accumulation/Distribution (A/D)   
category: Volumen clásico
score: 2/10
version: Estable
verdict: Descartar
description: ¿El flujo de volumen acumulado está confirmando la tendencia del precio, o está mostrando una divergencia?
---

## 🟦 AD (2/10)

**Nombre del archivo:** `AD.cs`  
**Nombre del indicador:** Accumulation/Distribution (A/D)  
**Web oficial:** [ATAS - Accumulation/Distribution (A/D)](https://help.atas.net/support/solutions/articles/72000606733)  
**Compatibilidad**: ATAS versión estable y superiores.  
>**La Pregunta Clave:** ¿El flujo de volumen acumulado está confirmando la tendencia del precio, o está mostrando una divergencia?

![AD](../../img/AD.png)


----------

### ⚙️ Parámetros configurables

Este indicador **no tiene parámetros configurables**.

----------

### 🧭 Clasificación

📂 VolumeClassic — Indicadores clásicos basados en volumen acumulado y relación precio-volumen

----------

### 🧠 Uso más frecuente

-   Medir el **flujo acumulado de dinero** (inferido) basado en el precio de cierre relativo al rango de la vela.
    
-   Detectar posibles **divergencias** entre el precio y la línea AD (propósito principal).
    
-   Confirmar la dirección de una tendencia.
    

----------

### 📊 Nivel de relevancia

🔟 **2 / 10**

✅ Concepto clásico para análisis de divergencias.

⛔ ¡ERROR DE VISUALIZACIÓN CRÍTICO! Se dibuja como un histograma (VisualMode.Histogram), lo que hace imposible su uso principal (comparar picos/valles para divergencias).

⛔ Es un indicador de "Price Action Ponderado por Volumen", no tiene en cuenta el Order Flow (agresión Bid/Ask).

⛔ Es un indicador obsoleto, superado por herramientas como el Delta Acumulado.

----------

### 🎯 Estrategias de scalping donde se aplica

-   **En su estado actual (histograma), ninguna. No es usable.**
    
-   (Teóricamente, si fuera una línea): Confirmación de tendencia o divergencias en timeframes altos (H1, H4) para dar contexto al scalping, no para entradas.
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   N/A. El indicador no tiene parámetros y su visualización por defecto lo hace inutilizable.
    

----------

### 🧪 Notas de desarrollo

-   Calcula la línea de acumulación/distribución según la fórmula clásica de Chaikin:
    
    $$ \\ AD\_t = AD\_{t-1} + \left( \frac{(C - L) - (H - C)}{H - L} \right) \times V$$
    
    $$$$
    
-   Si el rango de la vela es cero (High = Low), se mantiene el valor anterior (`prev`).
    
-   **EL FALLO CLAVE:** El resultado se almacena en un `ValueDataSeries` con `VisualType = VisualMode.Histogram`, en lugar de `VisualMode.Line`.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   **Fallo de Visualización:** El indicador es una línea _acumulativa_ diseñada para el análisis de divergencias. Al dibujarse como un histograma (basado en cero), se pierde toda la información acumulada, haciendo imposible comparar picos y valles. Es un error de implementación básico.
    

----------

### 🛠️ Propuestas de mejora

-   Cambiar el `VisualType` de `Histogram` a `Line`. Este es el único arreglo necesario para que el indicador sea funcional.
    
    C#
    
    ```
    DataSeries[0] = new ValueDataSeries("Ad", "AD")
    {
        VisualType = VisualMode.Line, // <-- ARREGLO
        UseMinimizedModeIfEnabled = true
    };
    
    ```
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

El análisis de la ficha es 100% correcto. Este indicador es un ejemplo perfecto de cómo un error de una sola palabra (`Histogram` en lugar de `Line`) puede hacer que una herramienta sea **completamente inútil**.

El A/D es una **línea acumulativa**. Su _único_ propósito es trazar **divergencias** (ej. el precio hace un nuevo máximo, pero la línea A/D no).

Como se puede ver en la imagen, el histograma hace imposible comparar un pico con el anterior. No puedes trazar una línea de tendencia sobre él. Este indicador, visualizado como un histograma, es inútil.

Además, conceptualmente, es un indicador de "Order Flow Falso". Intenta _adivinar_ la acumulación/distribución basándose en dónde cierra el precio dentro del rango. En la era moderna, tenemos herramientas como el **Delta Acumulado** que nos dan esta información de forma _real_ y precisa, no adivinada.

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**No. Categóricamente, no.**

1.  **Está Roto:** En su estado actual como histograma, es inutilizable.
    
2.  **Es Obsoleto:** Incluso si estuviera arreglado, es una herramienta de "Price Action Ponderado" de los años 80. Un scalper moderno debe usar **Delta** o **ActiveVolume** (que ya conservamos) para ver la agresión real, no la agresión "estimada" por la A/D.
    

**Acción:** **Descartar.**

**¿Merece la pena arreglarlo?** **No.** Aunque el arreglo es trivial (cambiar `Histogram` por `Line`), el indicador en sí es obsoleto y conceptualmente inferior a las herramientas de Order Flow que ya hemos decidido conservar.
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE2NDk1MTg1MTIsLTYyMjYyMjk2OCw0NT
UwNDQ4MDgsLTU4NzgxNjU3NV19
-->