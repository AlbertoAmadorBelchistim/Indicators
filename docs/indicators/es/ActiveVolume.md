--- 
cs_file: ActiveVolume.cs
name: Active Volume
category: Order Flow
score: 8/10
version: Estable
verdict: Conservar
description: Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios?
---﻿
## 🟦 Active Volume (8/10)
  
**Nombre del archivo:**  `ActiveVolume.cs`  
**Nombre del indicador:** Active Volume  
**Web oficial:**  [ATAS - Active Volume](https://help.atas.net/ru-RU/support/solutions/articles/72000608343-active-volume)  
**Compatibilidad:** ATAS versión estable y superiores.
**La Pregunta Clave:** Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios?

![Active Volume](../../img/ActiveVolume.png)

----------

### ⚙️ Parámetros configurables

-   **Filter**: Volumen mínimo para acumular (por defecto: 50)
    
-   **RowWidth**: Ancho de cada columna en la tabla (por defecto: 70)
    
-   **ShowBid / ShowAsk / ShowSum**: Mostrar columnas de Bid, Ask y Suma en tabla (por defecto: `true` para todas)
    
-   **Offset**: Desplazamiento de la tabla respecto al gráfico (por defecto: 0)
    
-   **DateFrom**: Fecha desde la cual comenzar a acumular datos (por defecto: Hoy)
    
-   **DigitsAfterComma**: Decimales mostrados en los valores (por defecto: 0)
    
-   **Mode (CalcMode)**: Modo de visualización del perfil (por defecto: `BidAsk`)
    
-   **ProfileWidth / ProfileOffset**: Ancho y desplazamiento del perfil (por defecto: 70 / 0)
    
-   **ProfileFillColor / BidProfileValueColor / AskProfileValueColor**: Colores de fondo y valores del perfil.
    

----------

### 🧭 Clasificación

📂 VolumeOrderFlow — Indicadores de volumen activo acumulado en nivel de precio

----------

### 🧠 Uso más frecuente

-   Visualizar la **acumulación de agresión bid y ask** por nivel de precio
    
-   Identificar zonas donde hubo **actividad agresiva desequilibrada**
    
-   Detectar absorciones o presencia institucional en zonas clave
    
-   Confirmar volumen dominante tras ruptura o rechazo
    

----------

### 📊 Nivel de relevancia

🔟 **8 / 10**

✅ Ofrece lectura visual precisa de acumulación de agresión por nivel de precio.

✅ Ideal para validar rupturas, absorciones o rechazos.

✅ Herramienta de Order Flow pura que filtra el "ruido".

⛔ Requiere buena configuración y espacio visual en el gráfico.

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Absorciones visuales**: Ask elevado + no rompe resistencia = venta.
    
-   **Rupturas reales**: Fuerte agresión Bid apareciendo en la ruptura de un soporte.
    
-   **Rechazo de zona**: Volumen concentrado pero sin continuación (indica una batalla que el otro lado está ganando).
    
-   **Actividad institucional**: Perfiles muy desequilibrados en niveles técnicos clave.
    

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **Filter**: `100` (Filtra todo por debajo de 100 lotes, buscando solo manos fuertes).
    
-   **Mode**: `BidAsk`
    
-   **ProfileWidth**: `30`
    
-   **ProfileOffset**: `25`
    
-   **DigitsAfterComma**: `2`
    
-   **ShowBid / ShowAsk / ShowSum**: `true` (todas activadas).
    
-   **DateFrom**: Sesión actual.
    

✅ Esta configuración resalta zonas de absorción o agresión intensa.

✅ Ideal para confirmar rompimientos y test de soportes/resistencias.

----------

### 🧪 Notas de desarrollo

-   El indicador acumula agresión Bid y Ask desde `DateFrom` usando objetos `CumulativeTrade`.
    
-   Los volúmenes se almacenan por precio en diccionarios separados (`_bidValues`, `_askValues`).
    
-   Se actualiza en tiempo real usando `OnCumulativeTrade` y `OnUpdateCumulativeTrade`.
    
-   La representación visual incluye un perfil horizontal escalado y una tabla de valores por fila de precio.
    
-   El filtro por volumen (parámetro `Filter`) evita acumular microtrades irrelevantes.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   El código es robusto. La principal carencia es de usabilidad:
    
-   No hay una opción de **reseteo automático por sesión**, obligando al usuario a cambiar el `DateFrom` manualmente cada día.
    
-   Carece de una línea de **POC** (Point of Control) para el volumen acumulado.
    

----------

### 🛠️ Propuestas de mejora

-   Añadir opción para **resetear acumulación cada sesión** (un booleano `ResetOnSession`) sin tener que cambiar `DateFrom` manualmente.
    
-   Incluir una **línea de POC (nivel de mayor volumen acumulado)**.
    
-   Permitir **configurar escala logarítmica** en la representación del perfil para evitar saturación por valores extremos.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

**Sí. Es una de las herramientas de Order Flow más útiles que existen.**

Mientras que indicadores como el AMA o el ATR te dicen _qué_ pasó con el precio, este indicador te dice _por qué_ pasó. Te muestra la "batalla" de la oferta y la demanda en cada nivel de precio, filtrando el ruido.

Es un "Volume Profile" pero para la _agresión_ (trades ejecutados al Bid o al Ask), no para el volumen total. Te permite ver exactamente a qué precio los compradores agresivos (que pagan el Ask) se encontraron con los vendedores agresivos (que pagan el Bid).

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**Absolutamente SÍ.** Es una herramienta de arsenal principal para un scalper de Order Flow.

No es un indicador de "entrada/salida" por sí mismo, sino una herramienta de **confirmación y contexto** de altísimo valor. Te permite validar si un breakout tiene "combustible" (agresión real) o si un soporte se está "defendiendo" (absorción).

**Acción:** **Conservar.**

**¿Merece la pena arreglarlo?** Sí. El indicador funciona, pero las "Propuestas de mejora" (especialmente el reseteo por sesión y un POC) lo elevarían de un 8/10 a un 10/10 en usabilidad.
<!--stackedit_data:
eyJoaXN0b3J5IjpbNDQ3MDc1ODIyXX0=
-->