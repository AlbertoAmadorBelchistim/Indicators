## 🟨 ATR Normalized (3/10)

  

**Nombre del archivo:**  `ATRN.cs`

**Nombre del indicador:** ATR Normalized

**Web oficial:**  [ATAS -   ATR Normalized](https://help.atas.net/support/solutions/articles/72000602633)

**Compatibilidad**: ATAS versión estable y superiores.

**La Pregunta Clave:** ¿Cuál es la volatilidad (ATR) del instrumento como un porcentaje de su precio actual?

----------
### ⚙️ Parámetros configurables

-   **Period**: Periodo del ATR clásico utilizado para el cálculo base (por defecto: `10`)
    

----------

### 🧭 Clasificación

📂 Volatility — Indicadores de volatilidad y rango normalizado

----------

### 🧠 Uso más frecuente

-   Evaluar la **volatilidad relativa** de cada vela en relación con su precio de cierre.
    
-   (Teóricamente) Normalizar la volatilidad para comparar diferentes activos (ej. S&P 500 vs. Petróleo).
    

----------

### 📊 Nivel de relevancia

🔟 **3 / 10**

⛔ Inútil para Scalping: Resuelve un problema (comparar activos de diferentes precios) que un scalper (enfocado en un solo activo) no tiene.

⛔ Redundante: En un gráfico intradía, el precio (el divisor) cambia muy poco, por lo que la línea del ATRN es visualmente idéntica a la del ATR estándar. No añade información.

⛔ Hereda Defectos: Utiliza internamente el indicador ATR.cs de ATAS, que (como ya vimos) usa una SMA (lenta) en lugar de la EMA/RMA (canónica y más rápida).

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Ninguna.**
    
-   Es una versión redundante y no mejorada del `ATR` estándar.
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **No se recomienda su uso para scalping.**
    

----------

### 🧪 Notas de desarrollo

-   Es un indicador "envoltorio" (wrapper). Contiene una instancia del indicador `ATR.cs` (`_atr`).
    
-   La fórmula aplicada es la normalización estándar:
    
    $$ \\ \text{ATRN} = 100 \times \left( \frac{\text{ATR}(\text{Period})}{\text{Close}} \right)$$
    
    $$$$
    
-   Hereda la implementación subóptima del `ATR` base (cálculo basado en SMA).
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   El indicador es conceptualmente redundante para el trading intradía en un solo instrumento.
    

----------

### 🛠️ Propuestas de mejora

-   No hay mejoras que "arreglen" el concepto. La solución es usar el `ATR` estándar.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

1.  **Resuelve un Problema que No Tienes:** El _único_ propósito de este indicador es comparar la volatilidad de un activo de 5000$ (como el S&P 500) con uno de 80$ (como el Petróleo). Para un scalper que está mirando **un solo instrumento** (el S&P 500), esta normalización es un paso matemático innecesario.
    
2.  **No Añade Información (Intradía):** Durante una sesión de scalping, el precio del S&P 500 se moverá, digamos, de 4550 a 4570. Dividir el ATR por 4550 o por 4570 apenas cambiará el resultado. La línea del **ATR Normalizado** se verá (y te dirá) _exactamente lo mismo_ que la línea del **ATR Estándar**.
    
3.  **Hereda el Mismo Defecto:** Este indicador usa el `ATR.cs` que analizamos antes. Eso significa que está calculando la volatilidad usando una **Media Móvil Simple (SMA)**, lo cual lo hace **menos reactivo** que el ATR canónico (que usa EMA/RMA).
    

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**No.** No te aporta absolutamente ninguna información nueva que el **ATR estándar** no te dé ya, y además hereda su implementación subóptima (basada en SMA).

**Acción:** **Descartar.**

**¿Merece la pena arreglarlo?** **No.** El indicador es conceptualmente inútil para el scalping y redundante.

<!--stackedit_data:
eyJoaXN0b3J5IjpbLTIwMzcxMTA5MzYsLTExMjMwODYwNzksLT
E2NDU0MjU3OTNdfQ==
-->