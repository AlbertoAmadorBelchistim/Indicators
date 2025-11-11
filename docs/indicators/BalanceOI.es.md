## 🟦 On Balance Open Interest (7.5/10)

  

**Nombre del archivo:** `BalanceOI.cs`

**Nombre del indicador:** On Balance Open Interest

**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602438](https://help.atas.net/support/solutions/articles/72000602438)

  

---

  

### ⚙️ Parámetros configurables

  

- **Minimized Mode (Período)**:

- Activación: `Enabled` (true/false)

- Valor: número de barras para aplicar el filtro

- Propósito: suavizar el cálculo eliminando ruido mediante una ventana deslizante (por defecto desactivado; valor por defecto: 10)

  

---

  

### 🧭 Clasificación

📂 VolumeOrderFlow — Indicador basado en interés abierto

  

---

  

### 🧠 Uso más frecuente

  

- Analizar si un movimiento está acompañado por **creación o cierre de posiciones** (subidas/bajadas de OI según dirección del cierre).

- Detectar **acumulación/distribución silenciosa** en movimientos laterales.

- Confirmar si un breakout es legítimo o está vacío de participación.

- Medir la **persistencia del interés abierto** con direccionalidad (mismo espíritu que el OBV pero con Open Interest).

  

---

  

### 📊 Nivel de relevancia

🔟 **7.5 / 10**

  

✅ Muy útil en **futuros**, especialmente micro y mini índices, crudo, oro

✅ Aporta lectura "profunda" sobre el compromiso del mercado

⛔ Menos útil en activos sin Open Interest visible (acciones, spot crypto)

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Ruptura con compromiso institucional** → precio sube + OI sube

- **Falsa ruptura** → precio sube pero OI cae → cierre de posiciones

- **Cambio de sesgo direccional** tras consolidación con aumento de OI

- **Seguimiento de swing intradía** apoyado por crecimiento sostenido de OI

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `5`

- **Mode**: `Histogram`

- **PositiveColor / NegativeColor**: verde brillante / rojo oscuro

- **ZeroLineColor**: gris

- **LineDashStyle**: `Dash`

- **ShowCurrentValue**: `false`

- **Width**: `2`

- **DrawZeroLine**: `true`

- **UseAlerts**: `false` (opcional si se desea automatizar señales)

  

✅ Ayuda a detectar entradas o salidas institucionales según cambios de OI

✅ Útil para confirmar si un breakout es acompañado por posiciones nuevas

⛔ Requiere contexto adicional (volumen, delta, zonas)

  

### 🧪 Notas de desarrollo

  

- El indicador calcula un acumulado de Open Interest (OI) según la dirección del cierre:

si el cierre sube, suma el OI actual; si baja, lo resta; si es igual, mantiene el valor previo.

- El modo "Minimized Mode" aplica un filtro mediante ventana deslizante, eliminando ruido de corto plazo.

- El parámetro `MinimizedMode` combina activación (`Enabled`) y tamaño de la ventana (`Value`), siendo un objeto `FilterInt`.

- La serie `_oiSignedSeries` almacena los valores con signo para aplicar el filtro, pero no se visualiza.

- El cálculo se realiza sobre el panel propio (`IndicatorDataProvider.NewPanel`) con visualización tipo histograma.

  

### 🛠️ Propuestas de mejora

  

- **Claridad del parámetro**: Separar visualmente `Enabled` y `Value` en la ficha para mayor comprensión.

- **Robustez adicional**: Añadir control opcional al principio de Oncalculate para evitar errores si el Open Interest es nulo o cero

(```if (candle == null || candle.OI == 0) return;```)

- **Ocultación explícita**: Marcar `_oiSignedSeries.IsHidden = true` para que ATAS no intente renderizar esa serie auxiliar.

- **Ampliación opcional**: Incluir opción para mostrar una línea de cero o alertas al cruzar valores positivos/negativos.

## Comentario Gemini
Aquí tienes la "pregunta clave" de este indicador:

> The Key Question: "Is the cumulative 'smart money' commitment (Open Interest) rising when prices rise and falling when prices fall, or is it diverging?"
> 
> (¿Está el compromiso acumulado del 'dinero inteligente' (Interés Abierto) subiendo cuando los precios suben y bajando cuando los precios bajan, o está divergiendo?)

----------

Tu análisis es **excelente** y has dado en el clavo con la esencia del indicador. Tu puntuación de **7.5/10** es muy acertada.

Has identificado perfectamente el concepto: **es un OBV (On-Balance Volume), pero que usa el Interés Abierto (OI) en lugar del Volumen**. Esta es una herramienta de nivel profesional para el trading de futuros, ya que el OI te dice si hay _dinero nuevo_ (posiciones nuevas) entrando o saliendo del mercado.

Tu ficha es casi perfecta, pero tiene **dos discrepancias importantes** con el código que has proporcionado.

----------

### ✍️ Discrepancia 1: La "Parametrización óptima" es Incorrecta

Esta es la discrepancia más grave. La sección "Parametrización óptima" que has escrito **no se corresponde en absoluto con el código**.

-   Tu ficha lista parámetros como: `Period: 5`, `Mode: Histogram`, `PositiveColor`, `ZeroLineColor`, `Width`, `DrawZeroLine`, etc.
    
-   El código (`BalanceOI.cs`) **no tiene ninguno de esos parámetros**.
    

El **único parámetro configurable** en el código es el `MinimizedMode { get; set; }`, que es un objeto que combina un `bool Enabled` y un `int Value`.

Parece que has copiado y pegado la sección de parámetros de otro indicador por error.

----------

### 🛠️ Discrepancia 2: La Lógica de "Minimized Mode"

Tu "Nota de desarrollo" es mitad correcta y mitad incorrecta. Has entendido mal lo que hace el `MinimizedMode`.

Analicemos los dos modos de este indicador, porque son **conceptualmente muy diferentes**:

#### 1. Modo Normal (`MinimizedMode.Enabled = false`)

-   **Lo que hace:** `_renderSeries[bar] = _renderSeries[bar - 1] +/- candle.OI`
    
-   **Tu análisis:** Tu nota "calcula un acumulado de Open Interest (OI) según la dirección del cierre..." es **100% correcta**.
    
-   **Resultado:** Es un **ACUMULADOR** (como el OBV). El valor del indicador solo sube o baja, mostrando el OI acumulado total a lo largo del tiempo.
    

#### 2. Modo Minimizado (`MinimizedMode.Enabled = true`)

-   **Lo que hace:** `_renderSeries[bar] = _renderSeries[bar - 1] + _oiSignedSeries[bar] - _oiSignedSeries[bar - MinimizedMode.Value]`
    
-   **Tu análisis:** Tu nota "aplica un filtro mediante ventana deslizante, eliminando ruido" es **incorrecta**.
    
-   **Resultado:** Esto no es un _filtro_ ni un _suavizado_ (como un SMA). Es una **SUMA MÓVIL (Rolling Sum)**. El indicador calcula la **suma neta del OI con signo de las últimas X velas**.
    

**La diferencia es crucial:**

-   **Modo Normal:** Te dice: "¿Cuál es el OI acumulado _total_ desde que empezó el gráfico?".
    
-   **Modo Minimizado (el de tu imagen):** Te dice: "¿Cuál ha sido el _cambio neto_ de OI en las últimas 10 velas?".
    

La imagen que has subido (un histograma que oscila alrededor de cero) confirma que estás usando el **Modo Minimizado** (la Suma Móvil), no el Modo Normal (el Acumulador).

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, absolutamente.** Tu puntuación de 7.5/10 es correcta. Es una herramienta de confirmación de nivel profesional.

El **Modo Minimizado (Rolling Sum)**, que es el que muestras en la imagen, es de hecho _más útil_ para el scalping que el modo acumulativo, ya que te muestra el "momentum" del OI reciente.

Tus propuestas de mejora son excelentes, especialmente la de añadir una línea de cero (que es esencial para un histograma oscilante) y la de `_oiSignedSeries.IsHidden = true`.

**Acción:** **CONSERVAR**.

Es un indicador fundamental para el trading de futuros. Solo te recomendaría corregir tu ficha para reflejar los parámetros correctos y la lógica real del "Minimized Mode".
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTc4NzM5ODMzNF19
-->