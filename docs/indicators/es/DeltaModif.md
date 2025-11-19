---
cs_file: DeltaModif.cs
name: Delta Modif 
category: Order FLow
score: 10/10
version: Estable
verdict: 'Conservar (herramienta principal)'
description: >-
  ¿Qué barras muestran una agresión (Delta) extrema, divergencia o absorción, y cómo puedo ver esas señales directamente en el gráfico de precio?
---

## 🟥 Delta (versión modificada) (10/10)

- **Nombre del archivo:** [DeltaModif.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/DeltaModif.cs)  
- **Nombre del indicador:** Delta Modif  
- **Web oficial base:** [ATAS — Delta](https://help.atas.net/en/support/solutions/articles/72000602362-delta)  
- **Compatibilidad:** ATAS versión beta y superiores.  
Para compatibilidad con versiones anteriores, debe usarse la compilación "stable" de los indicadores.
- **Última revisión del código base:**  [`Delta.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Delta.cs): 16/9/2025
- **Última revisión del código modificado:** 6/11/2025 (v 1.3.0) *(Versión extendida y mejorada por Alberto Amador Belchistim sobre la beta oficial de ATAS)*
- **Agradecimientos:** A **LoloTrader** y **Nick** por sus sugerencias e ideas para mejorar este indicador.

> **La Pregunta Clave:** ¿Qué barras muestran una agresión (Delta) extrema, divergencia o absorción, y cómo puedo ver esas señales directamente en el gráfico de precio?

![Delta](../../img/Delta.png)

---

### ⚙️ Parámetros configurables

#### 📊 Visualización
- **Modo**:
	- `Velas`: formato habitual (cuerpo + mechas).
	- `High-Low`: Las mechas se representan como "cuerpos" blancos.
	- `Histograma`: Cuerpos sin mechas.
	- `Barras`: solo líneas verticales (formato barra).
- **Modo minimizado**: Se representa el delta absoluto cambiando solo el color de la vela.
- **Mostrar valor actual**: muestra la cifra de delta en eje Y.
- **Show Threshold Lines**: Muestra las 4 líneas de umbral.
- **Threshold Source**: `Fixed / DynamicSigned`.  
  - `Fixed`: usa niveles fijos configurados en Fixed Threshold (Upper/Lower Major/Minor).  
  - `DynamicSigned`: calcula media y desviación por signo (Welford) anclados a la sesión actual.

![Diferentes tipos de visualización](../../img/DeltaVisualization.png)

#### 🟦 Dynamic Threshold
- **Session Window Mode**: ventana horaria para el cálculo dinámico. Puede ser:
  - `RTH`: horario de sesión regular.
  - `Full24h`: día completo.
- **RTH Start / End (HH:mm)**: límites de sesión (por defecto 09:30–16:00).
- **Std Multiplier (k)**: número de desviaciones estándar para definir los niveles Major (por defecto `1.0`).

![Diferencias Threshold](../../img/DeltaThreshold.png)

---

#### 🧰 Filtros
Oculta barras según criterios de dirección de la vela del precio, tipo de delta y valor del delta:
- **Dirección de barras (precio):** `Cualquier / Alcista / Bajista`  
- **Tipo de delta:** `Cualquier / Positivo / Negativo`  
- **Filtro**: cuantía mínima de |delta| por barra.

![Filtros](../../img/DeltaFilters.png)

NOTA: Estos filtros afectan a la visualización de divergencias, absorciones y señales visuales en el precio, ya que a efectos prácticos es como si **no existieran** las barras que no cumplen los criterios.

---
#### 🔀 Divergencias
Detecta cuándo el precio y el delta van en direcciones opuestas.
* **ShowDivergence**: Muestra los puntos de divergencia clásicos (círculos) en el **gráfico de precio**.
* **DivergenceBarsFilter**: Permite colorear las **velas/barras del propio indicador Delta** que muestran divergencia. Puedes activar/desactivar y elegir el color.

![Divergencias](../../img/DeltaDivergencias.png)

---

#### 🧲 Absorción:
Busca barras donde el delta muestra una reversión significativa desde su máximo/mínimo hasta el cierre.
* **Absorption**: Activa y define el umbral para detectar absorción. Dibuja un pequeño punto en el panel del Delta cuando detecta una "cola" significativa en la vela del Delta.

![Absorción](../../img/DeltaAbsorption.png)

---

#### 🔢 Etiqueta de volumen (panel Delta)
Muestra el valor numérico del Delta sobre cada barra en el panel del indicador. 
NOTA: Sólo aparece si el gráfico de precio está en formato Footprint para evitar problemas de visualización.
* **Mostrar**: Activa/Desactiva esta etiqueta numérica.
* **Color**: Color del texto.
* **Ubicación**: Posición de la etiqueta (`Arriba / Centro / Abajo` de la barra).
* **Tipo de letra**: Tipo y tamaño de letra.

![Etiqueta de volumen](../../img/DeltaVolumeLabel.png)

---

#### 📍 Señales visuales en panel de precio (triángulos)
Dibuja triángulos en el **gráfico de precio** para señalar barras cuyo delta supera el umbral marcado.  
IMPORTANTE: Esta lógica es independiente de las alertas sonoras.

* **Price signal Offset ticks**: Distancia (en ticks) para separar el triángulo de la vela (High/Low).
* **Price Signal Size**: Tamaño en píxeles del triángulo.
* **Price Signal Up Color / Price Signal Down Color**: Colores para los triángulos de delta positivo y negativo.
* **Show Visual Alerts**: Activa o desactiva estos triángulos.
* **Visual Up Threshold / Visual Down Threshold**: Define qué umbral (`Major` o `Minor`) debe superar el Delta para que se dibuje el triángulo.

![Señales visuales](../../img/DeltaVisualAlerts.png)

---

#### 🔔 Alarmas
Configura las alertas sonoras.
* **Archivo de alarma**: Nombre del archivo de sonido (ej. "alert1").
* **Color de texto / Fondo**: Colores del pop-up de alerta.
* **Audio Alerts**: Activa/Desactiva las alertas sonoras.
* **Audio Up Threshold / Audio Down Threshold**: Define qué umbral (`Major` o `Minor`) debe superar el Delta para disparar la alerta sonora.
* **Audio At Bar Close Only**:
	* `Activado (true)`: La alerta solo sonará cuando la vela **cierre** por encima/debajo del umbral (evita falsas alarmas intra-vela).
	* `Desactivado (false)`: Sonará en tiempo real en cuanto el delta cruce el umbral.

---

### ✨ Mejoras introducidas en la versión oficial beta (ATAS)

1.  **Coloreado de Divergencias en Velas de Delta**
    * Además de los puntos clásicos en el precio, ahora se pueden colorear las propias velas/barras del indicador Delta cuando hay divergencia.
    * El color se controla desde la UI y se adapta a cualquier modo visual (Velas, Histograma, etc.).

2.  **Mejoras de UI en Absorción**
    * Se ha creado un grupo (`Absorption`) que unifica el control (activar/desactivar) y el valor del umbral.
    * Cualquier cambio en el umbral de absorción actualiza el dibujo al instante, sin recargar el indicador.

3.  **Acabado Visual**
    * Se ha eliminado el borde (border) de las velas del Delta para un aspecto más limpio y moderno.

---

### ✨ Mejoras añadidas por Alberto Amador Belchistim

#### 1) Price Signals (Señales en el Gráfico de Precio)
* **Qué es**: Marcadores visuales (triángulos) que aparecen en el **panel de precio** (arriba/abajo de las velas) cuando el Delta de esa barra supera un umbral extremo.
* **Para qué sirve**: Te permite identificar picos de agresión (inicios de impulso o clímax) de forma inmediata, sin tener que mirar el panel inferior. Ideal para scalping rápido.
* **Lógica**: Los triángulos se disparan usando los umbrales definidos en `VisualUpLevel` y `VisualDownLevel` (puedes elegir "Major" o "Minor"). Estos, a su vez, usan la fuente de datos que hayas elegido (`Fixed` o `DynamicSigned`). Esta lógica es **independiente** de las alertas sonoras.

#### 2) Threshold Lines (Líneas Guía de Umbral)
* **Qué es**: Cuatro líneas horizontales (`UpMajor`, `UpMinor`, `DnMinor`, `DnMajor`) en el panel del Delta.
* **Para qué sirve**: Son la referencia visual de tus umbrales. Te permiten ver de un vistazo si el Delta actual es "normal", "fuerte" (minor) o "extremo" (major), y actúan como la referencia visual para los "Price Signals".
* **Lógica**: Se dibujan automáticamente según los valores de `Fixed` o `DynamicSigned`.

#### 3) Ajustes de UI y Cálculo
* **Reorganización de UI**: Los parámetros están agrupados de forma más lógica.
* **Cálculo No-Repainting**: El modo `DynamicSigned` calcula sus valores usando la barra anterior (cerrada), asegurando que las señales no desaparecen ni cambian en tiempo real ("non-repainting").

---

### 🧭 Clasificación
📂 **VolumeOrderFlow** — Indicadores de flujo y delta de agresión por barra o acumulado.

---

### 🧠 Uso más frecuente
* Detectar **cambios de impulso** o agotamiento mediante divergencias visuales (puntos en precio o barras de Delta coloreadas).
* Identificar **absorciones** (colas de agresión con reversión) con los puntos de absorción.
* Detectar **picos de delta extremo** con los **Price Signals** (triángulos) directamente en el panel de precio.
* Usar las **Threshold Lines** (fijas o dinámicas) para calibrar el nivel de delta "significativo" en cada activo y marco temporal.

---
 
### 📊 Nivel de relevancia
🔟 **10.0 / 10**

✅ **Foco en el precio:** Permite operar solo con el gráfico de precio, liberando espacio en pantalla al hacer opcional el panel inferior.  
✅ **Adaptativo:** El modo `DynamicSigned` (umbrales dinámicos) es una función profesional que adapta el indicador a la volatilidad de cada sesión, evitando el ruido de los filtros fijos.  
✅ **Alto valor para scalping:** Las señales en el precio y el modo de umbral dinámico son cambios cruciales para la operativa intradía rápida.  
⛔ **Requiere calibración:** Sigue siendo necesario calibrar los umbrales (Fijos o el multiplicador Dinámico) para cada activo y timeframe.  

---

### 🎯 Estrategias de scalping donde se aplica
* **Clímax de Delta en soportes/resistencias**: Triángulo `PriceSignal` (Major) apareciendo en una zona clave → señal de posible agotamiento o absorción.
* **Ignición de Delta en rupturas**: Triángulo `PriceSignal` (Major) apareciendo *a favor* de una ruptura → confirmación de impulso y agresión.
* **Filtro de contexto**: Usar los filtros de "Dirección de barras" para buscar, por ejemplo, deltas positivos extremos (`PriceSignalUp`) *solo* en velas de precio bajistas (Divergencia/Absorción).


---

### ⚙️ Parametrización que uso actualmente (1M, S&P 500)

| **Parámetro** | **Valor recomendado** | **Comentario** |
| :--- | :--- | :--- |
| **Show Threshold Lines** | `True` | Referencias visuales activas. |
| **Threshold Source** | `DynamicSigned` | **La función clave.** Se adapta al día. |
| **Session Window Mode** | `RTH` | Ancla el cálculo de la media y la desviación a la sesión RTH. |
| **RTH Start / End** | `09:30` / `16:00` | Horario de sesión americana. |
| **Std Multiplier (k)** | `1.0` | Nivel estándar (Mean + 1 StDev) para el umbral "Major". |
| **Filtros (Grupo)** | `(Off)` | Generalmente no filtro el delta, prefiero ver toda la información. |
| **DivergenceBarsFilter** | `True` | Activar coloración de divergencias en barras del Delta. |
| **Absorption.Enabled** | `True` | Activar detección de absorción. |
| **Absorption.Value** | `250` | (Depende del activo) Umbral para colas significativas. |
| **Show Visual Alerts** | `True` | **Activar triángulos en precio (clave).** |
| **Price Signal Offset Ticks**| `2` | Visibilidad sin tapar precio. |
| **Price Signal Size** | `10` | Tamaño equilibrado. |
| **Visual Up Threshold** | `Major` | Activar triángulos solo en picos extremos (Mean + k\*Std). |
| **Visual Down Threshold**| `Major` | Activar triángulos solo en picos extremos. |
| **Audio At Bar Close Only**| `True` | Evitar alertas falsas intra-barra. |

✅ Esta configuración se centra en usar las funciones avanzadas (Dynamic Thresholds + Price Signals) para mostrar solo picos de agresión *extremos y adaptativos* directamente en el gráfico de precio.

---

### 🧪 Notas de desarrollo

  * La modificación principal fue añadir las `ValueDataSeries` para `_priceSignalUp`, `_priceSignalDown` y las cuatro líneas de umbral (`_upMajor`, `_upMinor`, etc.).
  * Se implementó la lógica de Welford (acumulador) para el cálculo de media/std en `DynamicSigned` sin *look-ahead* (usa datos de la barra anterior cerrada).
  * Se reorganizó la UI (grupos *Visual signals in price panel*, *Fixed Threshold*, *Dynamic Threshold*) para que la configuración sea más intuitiva.
  * Se aseguró que los `Price Signals` respeten la fuente de umbral (`Fixed` o `Dynamic`) seleccionada.

-----

### 🛠️ Propuestas de mejora futura

  * Permitir selección de forma del marcador (triángulo, rombo, flecha).
  * Añadir un modo "Delta Acumulado" que resetee los umbrales dinámicos con el delta acumulado de la sesión, no solo el de la barra.

-----

-----

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

El indicador Delta es fundamental, pero la mayoría de las versiones son solo *visualizadores* pasivos que ocupan espacio en un panel inferior.

Tu modificación, `DeltaModif.cs`, soluciona el mayor problema del análisis de Delta: **la desconexión entre la señal (en el panel inferior) y la acción (en el panel de precio).**

Las dos modificaciones clave son de nivel profesional:

1.  **Umbrales Dinámicos (`DynamicSigned`):** Esta es la forma *correcta* de medir "extremo". Un delta de 500 puede ser insignificante en la apertura y un clímax a mediodía. Al anclarlo a la media y desviación estándar de la sesión (RTH), creas un filtro que se auto-ajusta a la volatilidad del día.
2.  **Señales en Precio (`Price Signals`):** Esta es la mejora que *transforma* el indicador para un scalper. Al dibujar un triángulo en el gráfico de precio cuando se supera el umbral dinámico, has convertido el Delta de un *dato* a una *señal*. Un trader ahora puede ocultar el panel inferior y seguir recibiendo la información más crítica (picos de agresión) en su gráfico principal.

Mientras que `ClusterSearchModif` te da la agresión *micro* (dentro del clúster), `DeltaModif` te da la agresión *meso* (el resultado de la batalla en esa barra). Las señales en el precio te permiten confirmar o negar setups de `BarsPattern` o `OHLCPlusModif` de un solo vistazo.

-----

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una herramienta "Core" (central) e indispensable.**

La versión estándar de Delta es solo un visualizador. Tu versión modificada es un **generador de señales de agresión**.

Para un scalper, la velocidad es clave. La capacidad de ver un "pico de delta extremo" (basado en un umbral dinámico, no fijo) directamente en el gráfico de precio, sin mover los ojos al panel inferior, es una ventaja operativa inmensa. Permite confirmar instantáneamente una ignición en una ruptura o un clímax de absorción en un nivel.

**Acción:** **Conservar (Herramienta Principal).**

**¿Merece la pena arreglarlo?** **No (está completo).** La lógica central (Umbrales Dinámicos + Señales en Precio) es robusta y está terminada. Las propuestas de mejora (como cambiar la forma del marcador) son solo "calidad de vida" (QoL), no correcciones necesarias.