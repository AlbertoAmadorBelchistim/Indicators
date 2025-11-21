---
cs_file: BidAskVR.cs
name: Bid Ask Volume Ratio
category: Order Flow
group: Order Flow
subgroup: Delta
score_current: 7/10
version: Estable
recommended_action: Mejorar
description: ¿Cuál es el desequilibrio normalizado del volumen agresivo?
gemini_summary: "'Delta Normalizado' (7/10). Conceptualmente superior (Ratio). Lógica de 4 colores genial para divergencias."
comparison_group: "Bar Delta Details"
competitor_notes: "Competidor de Imbalance Ratio."
reusable_code: null
file_state: Mejorable
score_potential: 8/10
effort: Bajo
action_priority: P1
analysis_date: 2025-11-17
official_code_date: 23/04/2025
---

## 🟦 Bid Ask Volume Ratio (7/10 | Potencial: 8/10)

**Nombre del archivo:** [`BidAskVR.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BidAskVR.cs)  
**Nombre del indicador:** Bid Ask Volume Ratio
**Web oficial:** [ATAS — Bid Ask Volume Ratio](https://help.atas.net/support/solutions/articles/72000602330)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Cuál es el desequilibrio normalizado (de -100% a +100%) del volumen agresivo, y cuál es el momentum (pendiente) de ese desequilibrio?

![BidAskVR](../../img/BidAskVR.png)

----------

### ⚙️ Parámetros configurables

-   **MaType** (`MovingType`): Tipo de media móvil para suavizar el ratio (`Ema`, `LinReg`, `Wma`, `Sma`, `Smma`).
    
-   **Period**: Número de barras para la media móvil (por defecto: `10`).
    
-   **CalcMode** (`Mode`): Dirección del ratio (`AskBid` o `BidAsk`).
    
-   **UpperColor**: Valor positivo con pendiente creciente (ej. Verde Brillante).
    
-   **UpColor**: Valor positivo con pendiente decreciente (ej. Verde Oscuro).
    
-   **LowColor**: Valor negativo con pendiente creciente (ej. Rojo Oscuro).
    
-   **LowerColor**: Valor negativo con pendiente decreciente (ej. Rojo Brillante).
    

----------

### 🧭 Clasificación

📂 VolumeOrderFlow — Oscilador de Delta Normalizado y Suavizado.

----------

### 🧠 Uso más frecuente

-   Medir el **desequilibrio normalizado** de la agresión (Ratio Ask/Bid) para ver el dominio comprador o vendedor en términos relativos.
    
-   Detectar **agotamiento y divergencias** usando la lógica de color de 4 vías (ej. un histograma Verde Oscuro `UpColor` muestra un desequilibrio positivo que está _perdiendo fuerza_).
    
-   Confirmar la fuerza y continuación de un impulso (ej. un histograma Verde Brillante `UpperColor`).
    

----------

### 📊 Nivel de relevancia

🔟 **7 / 10**

✅ Conceptualemente Superior: Es un "Delta Normalizado". Un ratio (A-B)/(A+B) es mucho más significativo que un Delta simple (A-B), ya que pone el desequilibrio en contexto con el volumen total.

✅ Lógica de Color de 4 Vías: Su característica más potente. Muestra el momentum del momentum, ideal para detectar divergencias y agotamiento de forma temprana.

✅ Altamente Configurable: Permite 5 tipos de medias móviles para ajustar el suavizado.

⛔ Falta de Línea Cero: Al igual que otros osciladores de ATAS, ShowZeroValue = false por defecto, lo que dificulta la lectura.

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Detección de Agotamiento (Divergencia):**
    
    -   En una tendencia alcista, si el histograma pasa de `UpperColor` (Verde Brillante) a `UpColor` (Verde Oscuro), indica que el impulso comprador se está agotando (divergencia de momentum).
        
-   **Confirmación de Impulso (Ignición):**
    
    -   Un breakout alcista acompañado de barras `UpperColor` (Verde Brillante) confirma que el desequilibrio comprador está aumentando.
        
-   **Filtro de Contexto:** Evitar comprar si el histograma está en `LowerColor` (Rojo Brillante).
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **MaType**: `Ema` (Es la más reactiva al precio reciente).
    
-   **Period**: `10`
    
-   **CalcMode**: `AskBid`
    
-   _Colores:_ Usar colores de alto contraste (ej. Verde Brillante / Verde Oscuro, Rojo Brillante / Rojo Oscuro) para diferenciar claramente las 4 fases.
    

----------

### 🧪 Notas de desarrollo

-   Paso 1: Calcula el "Delta Normalizado" (Ratio) en una serie interna _vr:
    
    _vr[bar] = 100 * (candle.Ask - candle.Bid) / (candle.Ask + candle.Bid)
    
-   Paso 2: Suaviza ese ratio usando la media móvil (MaType) y Period seleccionados:
    
    maValue = IndicatorCalculate(bar, _movingType, _vr[bar])
    
-   **Paso 3:** Aplica la lógica de color de 4 vías comparando el valor actual (`maValue`) con el anterior (`_prevValue`) para determinar la pendiente.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   **Falta de Línea Cero:** El indicador es un oscilador que cruza el cero, pero está configurado con `ShowZeroValue = false`, dificultando su lectura.
    
-   **Cálculo de Arranque:** El código tiene una lógica manual (`if (bar < _period && bar > 0)`) para "simular" una EMA antes de que el período esté completo. Esto es innecesario, ya que las clases `EMA` estándar ya manejan esto. Es un código redundante.
    

----------

### 🛠️ Propuestas de mejora

-   **¡Mejora Crítica!:** Añadir una línea de cero (`LineSeries`) o establecer `ShowZeroValue = true` por defecto.
    
-   Simplificar el código en `OnCalculate` eliminando el bloque `if (bar < _period)` y dejando que el `IndicatorCalculate()` (que llama a la EMA/SMA) maneje el período de arranque.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador

Este es un indicador de Order Flow de nivel profesional. Tu puntuación de 7/10 es muy acertada.

1. Es un "Delta Normalizado":

El cálculo base (Ask - Bid) / (Ask + Bid) es conceptualmente muy superior a un simple histograma de Delta.

-   **Delta Normal:** ¿Un Delta de +500 es "grande" o "pequeño"? No lo sabes.
    
-   **Este Indicador (Ratio):** Un Delta de +500 con un volumen total de 1000 da un ratio altísimo (+100%), señalando un dominio comprador total. Un Delta de +500 con un volumen total de 20,000 da un ratio muy bajo (+2.5%), señalando una gran batalla equilibrada. Este indicador te da ese contexto.
    

2. La Lógica de Color de 4 Vías (La Clave):

Esta es la característica estrella. El color no solo te dice si el desequilibrio es positivo o negativo (por encima/debajo de cero), sino también si el momentum de ese desequilibrio está aumentando o disminuyendo (la pendiente).

-   `UpperColor` (Verde Brillante): El desequilibrio es positivo **Y** está aumentando. (Fuerte impulso alcista).
    
-   `UpColor` (Verde Oscuro): El desequilibrio es positivo, **PERO** está disminuyendo. (Agotamiento alcista / Divergencia).
    
-   `LowerColor` (Rojo Brillante): El desequilibrio es negativo **Y** está aumentando (más negativo). (Fuerte impulso bajista).
    
-   `LowColor` (Rojo Oscuro): El desequilibrio es negativo, **PERO** está disminuyendo (moviéndose hacia cero). (Agotamiento bajista / Divergencia).
    

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una herramienta de confirmación y divergencia de nivel A (7/10).**

Su lógica de "Delta Normalizado" y su coloreado de 4 vías lo hacen conceptualmente superior a un simple histograma de Delta para detectar agotamiento y divergencias de momentum.

**Acción:** **Mejorar (Prioridad P1).**

**¿Merece la pena mejorarlo?** **SÍ.** El arreglo es trivial (`effort: Bajo`) y es una prioridad P1. Añadir una línea de cero (`ShowZeroValue = true`) y limpiar el código de arranque (`if (bar < _period)`) lo convierte en una herramienta 8/10 mucho más robusta y legible.
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTEzMTIyNDYzODAsLTkzMjMzNDk0OF19
-->