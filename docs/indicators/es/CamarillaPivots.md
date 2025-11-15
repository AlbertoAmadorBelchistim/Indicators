---
cs_file: CamarillaPivots.cs
name: Camarilla Pivots
category: Niveles
score: 8/10
version: Estable
verdict: Conservar
description: ¿Dónde están los niveles de soporte y resistencia intradía más relevantes, basados en la fórmula de Camarilla, para operar rupturas (en L4/H4) y reversiones (en L3/H3)?
---

## 🟦 Camarilla Pivots (8/10)

**Nombre del archivo:** [`CamarillaPivots.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CamarillaPivots.cs)  
**Nombre del indicador:** Camarilla Pivots  
**Web oficial:** [ATAS — Camarilla Pivots](https://help.atas.net/support/solutions/articles/72000602341)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Dónde están los niveles de soporte y resistencia intradía más relevantes, basados en la fórmula de Camarilla, para operar rupturas (en L4/H4) y reversiones (en L3/H3)?

![CamarillaPivots](../../img/CamarillaPivots.png) 

-----

### ⚙️ Parámetros configurables

  * **PivotColor**: Color de la línea del pivot central.
  * **BetweenColor**: Color de los niveles `H3` / `L3` (los niveles clave de reversión).
  * **HighLowColor**: Color común para todos los demás niveles (H1-H2, H4-H6, L1-L2, L4-L6).

-----

### 🧭 Clasificación

📂 Levels / Pivots — Niveles de soporte/resistencia intradía por fórmula Camarilla.

-----

### 🧠 Uso más frecuente

  * **Estrategia de Reversión:** Buscar ventas en `H3` y compras en `L3`. Esta es la estrategia de Camarilla más común.
  * **Estrategia de Ruptura (Breakout):** Comprar en la ruptura de `H4` (objetivo H5/H6) y vender en la ruptura de `L4` (objetivo L5/L6).
  * **Contexto Intradía:** Usar el `Pivot` central como "imán" o punto de equilibrio.
  * Delimitar el "campo de juego" estadístico del día.

-----

### 📊 Nivel de relevancia

🔟 **8 / 10**

✅ **Herramienta de Niveles Clásica:** Un estándar de la industria para el trading intradía de futuros.  
✅ **Estrategia Clara:** A diferencia de otros pivots, la estrategia Camarilla es muy específica (revertir en L3/H3, romper L4/H4).  
✅ Proporciona 13 niveles (6S, 6R, 1 Pivot) de contexto.  
⛔ **Estático:** Las fórmulas se basan *solo* en el HLC del día anterior y no se adaptan a la volatilidad actual (como lo haría un `ADR`).  
⛔ **No considera volumen** ni Order Flow.  

-----

### 🎯 Estrategias de scalping donde se aplica

  * **Reversión en H3/L3**: Buscar un fallo de precio en H3 (para cortos) o L3 (para largos), confirmado por absorción o fallo de delta.
  * **Ruptura de H4/L4**: Entrar a favor de la ruptura de H4 (largos) o L4 (cortos), esperando un movimiento tendencial.
  * **Objetivos de Beneficio (Take Profit):** Usar H5/L5 como objetivos finales para las operaciones de ruptura.

-----

### ⚙️ Parametrización óptima para scalping (1M, S\&P 500)

  * **PivotColor**: Gris (referencia neutral).
  * **BetweenColor**: Rojo (H3) y Verde (L3) (colores de "acción" para reversión). *Nota: El indicador base usa el mismo color para ambos, lo cual es una limitación.*
  * **HighLowColor**: Azul (niveles de ruptura H4/L4) o un color sutil.
  * *Nota: La parametrización es puramente visual.*

-----

### 🧪 Notas de desarrollo

  * El indicador calcula 13 niveles (Pivot, H1-H6, L1-L6) basados en el `High`, `Low` y `Close` de la sesión anterior.
  * Los cálculos se realizan una sola vez al día, cuando `IsNewSession(bar)` es verdadero.
  * Las líneas se dibujan (`RenderValues`) hacia adelante desde el inicio de la sesión (`_lastSession`) hasta la barra actual (`bar`), asegurando que los niveles cubran todo el día.
  * El indicador usa 13 `ValueDataSeries` para gestionar cada línea individualmente.

-----

### ❗ Incoherencias o aspectos mejorables detectados

  * **¡Riesgo de Bug (DivideByZero)\!** El cálculo de `_lastH5 = _high / _low * _close;` no comprueba si `_low` es `0`. Si `_low` fuera 0 (por un dato corrupto), el indicador fallaría con una excepción de división por cero.
  * **Colores Limitados:** El indicador agrupa `H3` y `L3` bajo el mismo color (`BetweenColor`) y todos los demás niveles (excepto el Pivot) bajo `HighLowColor`. Esto es una limitación visual, ya que un trader querría ver los soportes (L1-L6) de un color y las resistencias (H1-H6) de otro, o al menos H3/L3 de colores opuestos.

-----

### 🛠️ Propuestas de mejora

  * **¡Corregir el Bug\!** Añadir una comprobación: `_lastH5 = (_low == 0) ? 0 : (_high / _low * _close);`.
  * **Mejorar los Colores:** Separar los parámetros de color para `H3` y `L3`, y para los niveles de Soporte (L) y Resistencia (H), para una mejor legibilidad.
  * Añadir alertas (`AddAlert`) cuando el precio toque los niveles clave (L3, H3, L4, H4).
  * Añadir etiquetas de texto (`AddText`) al final de las líneas para identificar qué nivel es (H1, H2, etc.).

-----

-----

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

Este es un indicador de niveles "clásico" y muy respetado, especialmente en el trading de futuros. Su principal fortaleza sobre otros tipos de pivots (como los "Floor Pivots") es que define una **estrategia operativa clara**:

  * **Rango:** `L3` y `H3` son los límites del "rango" esperado. La estrategia es buscar reversiones (fades) en estos niveles.
  * **Tendencia (Breakout):** `L4` y `H4` son los niveles de "ruptura". Si el precio rompe y se mantiene más allá de ellos, la estrategia cambia a "seguimiento de tendencia".

Es una herramienta de "mapa" excelente para el scalper. Te dice dónde están las "fronteras" clave del día. Un scalper puede usar esta información para:

1.  **No operar** en medio de la nada (entre L2 y H2).
2.  **Buscar entradas** de alta probabilidad en los niveles L3/H3/L4/H4.
3.  **Confirmar entradas** usando Order Flow (ej. buscar una absorción masiva en L3).

-----

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, es una herramienta de contexto (mapa) muy útil.**

No te da señales de entrada por sí mismo, pero te dice *dónde* buscar esas señales. Para un scalper, operar en un nivel de Camarilla conocido es estadísticamente superior a operar en un precio aleatorio.

**Acción:** **Conservar.**

**¿Merece la pena arreglarlo?** **Sí.** El bug de `DivideByZero` debe ser corregido. Las mejoras de color y las alertas lo harían pasar de un 8/10 a un 9/10 en usabilidad.