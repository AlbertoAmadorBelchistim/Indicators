---
cs_file: ActiveVolume.cs
name: Active Volume
category: Order Flow
score_current: 8/10
version: Estable
recommended_action: Mejorar
description: Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios?

# --- Análisis y Triaje de Gemini ---
gemini_summary: Herramienta "Core" (8/10). Su potencial transformacional (Delta por Precio) es una mejora P1 de alto impacto.
file_state: Mejorable
score_potential: 10/10 (Transformacional)
effort: Medio
action_priority: P1 (Mejora Estratégica)
analysis_date: 2025-11-17
official_code_date: 3/12/2024
user_modification_date: 13/11/2025
# ------------------------------------
---

## 🟦 Active Volume (8/10 | Potencial: 10/10)
  
**Nombre del archivo:**  [`ActiveVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ActiveVolume.cs)  
**Versión modificada:** [`ActiveVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/ActiveVolume.cs)  
**Nombre del indicador:** Active Volume  
**Web oficial:**  [ATAS - Active Volume](https://help.atas.net/ru-RU/support/solutions/articles/72000608343-active-volume)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 3/12/2024  
**Última revisión del código modificado:** 13/11/2025

>**La Pregunta Clave:** Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios?

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

### 🛠️ Propuestas de mejora (Prioridad P1/P2)

El indicador es una herramienta de 8/10, pero tiene dos caminos de mejora:

**Mejoras de Usabilidad (Prioridad P2 - Esfuerzo Medio):**
* **Reset por Sesión:** Añadir una opción booleana `ResetOnSession` para evitar cambiar la fecha (`DateFrom`) manualmente.
* **POC (Point of Control):** Añadir una línea visual para el nivel de precio con la *mayor agresión sumada* (Bid+Ask).

**Mejora Transformacional (Prioridad P1 - Esfuerzo Medio):**
* **Modo "Delta Profile":** Añadir un nuevo `CalcMode` que muestre el **Delta Neto por Precio** (`Ask - Bid`). El código ya almacena `_askValues` y `_bidValues` por separado, haciendo esto factible.
* **Visualización de Delta:** En el modo "Delta Profile", el histograma debería ser de un solo color (ej. azul) y crecer a la derecha (Delta positivo) o a la izquierda (Delta negativo) desde un eje central.
* **Coloreado de Fondo:** Permitir colorear el fondo de la celda de la tabla (ej. rojo/verde) basado en el desequilibrio (`Ask / Bid`) por precio.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (Análisis "Modo Ingeniero")

Es una herramienta "Core" de Order Flow. Su función (filtrar ruido) es esencial. Sin embargo, en su estado actual (8/10), solo presenta los *datos en bruto* (Agresión Bid, Agresión Ask).

La verdadera oportunidad es **transformarlo en un "Perfil de Agresión Neta"**. Un scalper no solo quiere ver `Bid: 100` y `Ask: 50`. Quiere ver el resultado: `Delta: +50`.

El código `.cs` ya almacena los valores de Bid y Ask por separado, por lo que añadir un modo "Delta Profile" es una mejora P1 (Alto impacto, Esfuerzo Medio). Esto lo convertiría en la herramienta de confirmación definitiva para:
* Identificar **absorción** (Delta de venta alto pero el precio no baja).
* Identificar **agotamiento** (Delta de compra alto en un máximo que falla).
* Validar **rupturas** (Delta positivo fuerte rompiendo una resistencia).

### 📈 Veredicto: ¿Es útil para Scalping?

**Absolutamente SÍ.** Es una herramienta de arsenal principal para un scalper de Order Flow.

Las mejoras de usabilidad (P2) lo harían más cómodo. Las mejoras transformacionales (P1, añadir Delta por Precio) lo convertirían en un indicador **10/10** y, posiblemente, en uno de los 5 indicadores más potentes de toda la librería para el scalping.

**Acción:** **Mejorar (Prioridad P1/P2).**
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTM2MzQzMzUzNl19
-->