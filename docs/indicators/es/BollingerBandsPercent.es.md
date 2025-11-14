


## 🟦 Bollinger Bands: Percentage (6/10)

Nombre del archivo: BollingerBandsPercent.cs

Nombre del indicador: Bollinger Bands: Percentage

Web oficial: ATAS — Bollinger Bands: Percentage

Compatibilidad: ATAS versión estable y superiores.

> **La Pregunta Clave:** ¿En qué posición (como un porcentaje normalizado) se encuentra el precio actual dentro de las Bandas de Bollinger?

----------

### ⚙️ Parámetros configurables

-   **CalcMode**: (por defecto: `Bottom`)
    
    -   `Bottom`: Calcula la posición relativa a todo el canal (0% a 100%).
        
    -   `Middle`: Calcula la posición relativa a la banda media (SMA).
        
-   **Period**: Periodo del SMA y desviación estándar (por defecto: `10`).
    
-   **Width**: Multiplicador de la desviación estándar (por defecto: `1`).
    

----------

### 🧭 Clasificación

📂 Volatility — Indicador de posición relativa dentro del canal de Bollinger (Conocido como "%B").

----------

### 🧠 Uso más frecuente

-   **Normalizar la posición del precio** dentro del canal (0% = Banda Inf, 50% = SMA, 100% = Banda Sup).
    
-   **Detectar Divergencias:** Identificar agotamiento (ej. precio hace nuevo máximo, pero %B hace un máximo más bajo).
    
-   Confirmar **rupturas reales** (cuando %B se mantiene > 100% o < 0%).
    
-   Identificar condiciones de sobrecompra (>100) o sobreventa (<0).
    

----------

### 📊 Nivel de relevancia

🔟 **6 / 10**

✅ Excelente para Divergencias: Es su uso principal. Cuantifica la "sobreextensión" de forma normalizada.

✅ Permite identificar rápidamente si el precio está en extremos.

⛔ Tiene Lag: Se basa en las Bandas de Bollinger, que ya son indicadores con lag (basados en SMA).

⛔ Ruidoso: Al ser un oscilador, puede fluctuar erráticamente en mercados laterales.

⛔ Valores por Defecto Débiles: Hereda los defaults (10, 1.0) que no son el estándar (20, 2.0).

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Reversión desde Extremos (con Divergencia):** Buscar ventas si el precio hace un nuevo máximo pero el %B hace un máximo más bajo (divergencia bajista).
    
-   **Confirmación de Ruptura (Breakout):** Si el precio rompe la banda superior, el %B subirá por encima de 100. Si se _mantiene_ por encima de 100, confirma una tendencia fuerte ("caminar por la banda").
    
-   **Filtro de "Chop":** Si el %B oscila erráticamente entre, digamos, 40% y 60%, confirma un mercado lateral sin dirección.
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **CalcMode**: `Bottom` (Es la fórmula canónica de %B).
    
-   **Period**: `20`
    
-   **Width**: `2.0`
    
-   _Nota: Es crucial cambiar los valores por defecto a los estándares (`20, 2.0`) para que el indicador sea fiable._
    

----------

### 🧪 Notas de desarrollo

-   Es un indicador "envoltorio" (wrapper). Contiene una instancia del indicador `BollingerBands` (`_bb`).
    
-   Llama a `_bb.Calculate(bar, value)` para obtener los valores de las bandas.
    
-   **Fórmula (Modo `Bottom`):** Esta es la fórmula canónica de **%B**:
    
    $$ \\ %B = 100 \times \left( \frac{\text{Precio} - \text{Banda Inferior}}{\text{Banda Superior} - \text{Banda Inferior}} \right)$$
    
    $$$$
    
-   **Fórmula (Modo `Middle`):** Una variante menos común.
    
    $$ \\ \text{Valor} = 100 \times \left( \frac{\text{Precio} - \text{SMA}}{\text{Banda Superior} - \text{SMA}} \right)$$
    
    $$$$
    
-   El indicador protege contra la división por cero si las bandas tienen ancho cero.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   **Valores por Defecto Débiles:** El indicador usa `Period=10` y `Width=1` por defecto, lo que lo hace demasiado ruidoso y no coincide con el estándar de `20, 2.0`.
    
-   **Falta de Líneas de Referencia:** Un oscilador normalizado como este es casi inútil sin líneas de referencia horizontales.
    

----------

### 🛠️ Propuestas de mejora

-   **¡Mejora Crítica!:** Añadir líneas horizontales fijas (no configurables) en `0`, `50` y `100`.
    
-   Cambiar los valores por defecto a `Period = 20` y `Width = 2.0`.
    
-   Añadir alertas cuando el indicador cruce los niveles 0, 50 o 100.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

Este indicador es un "derivado" clásico de las Bandas de Bollinger, a menudo conocido como **%B** (Percent B). Tu puntuación de 6/10 es la nota exacta que se merece.

1. Es un Derivado Útil:

A diferencia del BollingerBandsBandwidth (que medía el ancho y descartamos como "redundante"), este indicador (%B) nos da información nueva y valiosa: la posición normalizada del precio.

2. La Lógica (%B):

Tus "Notas de desarrollo" son 100% correctas. El CalcMode.Bottom es la fórmula canónica de %B: (Precio - Banda Inferior) / (Banda Superior - Banda Inferior).

-   Un valor de **100%** (o > 100) significa que el precio ha tocado (o roto) la banda superior.
    
-   Un valor de **50%** significa que el precio está en la media móvil (SMA).
    
-   Un valor de **0%** (o < 0) significa que el precio ha tocado (o roto) la banda inferior.
    

3. Tus Propuestas de Mejora son Esenciales:

Tu primera propuesta ("Añadir líneas horizontales fijas para zonas 0%, 50% y 100%") es fundamental. Un oscilador normalizado sin sus niveles de referencia es muy difícil de leer.

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**Es una herramienta de confirmación útil, pero no esencial.** Tu 6/10 es perfecto.

-   **Lo Bueno (El 6):** Es excelente para detectar **divergencias**. (Ej. El precio hace un nuevo máximo, pero el `%B` no llega al 100% -> señal de agotamiento). También es genial para confirmar una "ruptura real" (cuando el `%B` se dispara a > 100% y se _mantiene_ ahí).
    
-   **Lo Malo (El -4):** Es un indicador con lag (se basa en el `BollingerBands`) y es ruidoso. No es algo que usarías para tomar una decisión de entrada por sí solo.
    

**Acción:** **Conservar (con reservas).**

Es un indicador clásico y válido. No es "esencial" (como `BarsPattern` o `ActiveVolume`), pero es una herramienta de confirmación sólida para un arsenal si te gusta cazar divergencias.
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTQ3ODI0NTU1NywtNTA1NTYzNzJdfQ==
-->