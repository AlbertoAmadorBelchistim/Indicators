## 🟥 Delta (versión modificada)

- **Nombre del archivo:** [DeltaModif.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/DeltaModif.cs)  
- **Nombre del indicador:** Delta Modif  
- **Web oficial:** [ATAS — Delta](https://help.atas.net/en/support/solutions/articles/72000602362-delta)  
- **Compatibilidad:** ATAS versión beta y superiores.  
Para compatibilidad con versiones anteriores, debe usarse la compilación "stable" de los indicadores. 
- **Versión actual:** 1.3.0 (6/11/2025) *(Versión extendida y mejorada por Alberto Amador Belchistim sobre la beta oficial de ATAS)*
- **Agradecimientos:** A **LoloTrader** y **Nick** por sus sugerencias e ideas para mejorar este indicador.

![Delta](../img/Delta.png)

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

![Diferentes tipos de visualización](../img/DeltaVisualization.png)

#### 🟦 Dynamic Threshold
- **Session Window Mode**: ventana horaria para el cálculo dinámico. Puede ser:
  - `RTH`: horario de sesión regular.
  - `Full24h`: día completo.
- **RTH Start / End (HH:mm)**: límites de sesión (por defecto 09:30–16:00).
- **Std Multiplier (k)**: número de desviaciones estándar para definir los niveles Major (por defecto `1.0`).

![Diferencias Threshold](../img/DeltaThreshold.png)

---

#### 🧰 Filtros
Oculta barras según criterios de dirección de la vela del precio, tipo de delta y valor del delta:
- **Dirección de barras (precio):** `Cualquier / Alcista / Bajista`  
- **Tipo de delta:** `Cualquier / Positivo / Negativo`  
- **Filtro**: cuantía mínima de |delta| por barra.

![Filtros](../img/DeltaFilters.png)

NOTA: Estos filtros afectan a la visualización de divergencias, absorciones y señales visuales en el precio, ya que a efectos prácticos es como si **no existieran** las barras que no cumplen los criterios.

---
#### 🔀 Divergencias
Detecta cuándo el precio y el delta van en direcciones opuestas.
* **ShowDivergence**: Muestra los puntos de divergencia clásicos (círculos) en el **gráfico de precio**.
* **DivergenceBarsFilter**: Permite colorear las **velas/barras del propio indicador Delta** que muestran divergencia. Puedes activar/desactivar y elegir el color.

![Divergencias](../img/DeltaDivergencias.png)

---

#### 🧲 Absorción:
Busca barras donde el delta muestra una reversión significativa desde su máximo/mínimo hasta el cierre.
* **Absorption**: Activa y define el umbral para detectar absorción. Dibuja un pequeño punto en el panel del Delta cuando detecta una "cola" significativa en la vela del Delta.

![Absorción](../img/DeltaAbsorption.png)

---

#### 🔢 Etiqueta de volumen (panel Delta)
Muestra el valor numérico del Delta sobre cada barra en el panel del indicador. 
NOTA: Sólo aparece si el gráfico de precio está en formato Footprint para evitar problemas de visualización.
* **Mostrar**: Activa/Desactiva esta etiqueta numérica.
* **Color**: Color del texto.
* **Ubicación**: Posición de la etiqueta (`Arriba / Centro / Abajo` de la barra).
* **Tipo de letra**: Tipo y tamaño de letra.

![Etiqueta de volumen](../img/DeltaVolumeLabel.png)

---

#### 📍 Señales visuales en panel de precio (triángulos)
Dibuja triángulos en el **gráfico de precio** para señalar barras cuyo delta supera el umbral marcado.  
IMPORTANTE: Esta lógica es independiente de las alertas sonoras.

* **Price signal Offset ticks**: Distancia (en ticks) para separar el triángulo de la vela (High/Low).
* **Price Signal Size**: Tamaño en píxeles del triángulo.
* **Price Signal Up Color / Price Signal Down Color**: Colores para los triángulos de delta positivo y negativo.
* **Show Visual Alerts**: Activa o desactiva estos triángulos.
* **Visual Up Threshold / Visual Down Threshold**: Define qué umbral (`Major` o `Minor`) debe superar el Delta para que se dibuje el triángulo.

![Señales visuales](../img/DeltaVisualAlerts.png)

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
🔟 **9.0 / 10**

✅ **Foco en el precio:** Permite operar solo con el gráfico de precio, liberando espacio en pantalla al hacer opcional el panel inferior.  
✅ **Alto valor para scalping:** Las señales en el precio y el modo de umbral dinámico son cambios cruciales para la operativa intradía rápida.  
⛔ **Requiere calibración:** Sigue siendo necesario calibrar los umbrales (Fijos o el multiplicador Dinámico) para cada activo y timeframe.  

---

### 🎯 Estrategias de scalping donde se aplica
* **Delta extremo en ruptura**: Triángulo `PriceSignalUp` + Delta superando `UpMajorLevel` → entrada con confirmación de impulso.
* **Absorción en resistencia**: Triángulo `PriceSignalDown` en zona superior con divergencia bajista → posible rechazo.
* **Ruptura fallida**: Divergencia alcista + delta débil → aviso de agotamiento.
* **Cambio de flujo progresivo**: Secuencia de barras con absorción y menor magnitud de delta → reversión probable.

---

### ⚙️ Parametrización que uso actualmente (1M, S&P 500)

| **Parámetro** | **Valor recomendado** | **Comentario** |
| :--- | :--- | :--- |
| **Show Threshold Lines** | ✅ | Referencias visuales activas |
| **Threshold Source** | `Dynamic` | Permite adaptarse al día si es muy volátil o atípico. |
| **Upper major / Lower major** | `500 / -500` | Captura solo picos de clímax/ignición. Ajustar si hay mucho ruido. |
| **Upper minor / Lower minor** | `250 / -250` | Nivel de "interés" o agresión media. |
| **Session Window Mode** | `RTH` | Ancla el cálculo de la media y la desviación a la sesión. |
| **RTH Start** | `09:30:00` | Inicio sesión americana. |
| **RTH End** | `16:00:00` | Fin sesión americana. |
| **Std Multiplier (k)** | `1.0` | Nivel estándar para definir Major/Minor (suma 1 desviación) |
| **Barras de divergencia** | ✅ | Añadir coloración de divergencias en barras|
| **Valor absorción** | `250` | Busco detectar cambios bruscos desde el máximo/mínimo. |
| **Price Signal Offset Ticks** | `2` | Visibilidad sin tapar precio |
| **Price Signal Size** | `10` | Tamaño equilibrado |
| **Show visual alerts** | ✅ | **Activar triángulos en precio** |
| **Visual Up Threshold** | `Major` | **Activar triángulos al superar el umbral superior major** |
| **Visual Down Threshold** | `Major` | **Activar triángulos al superar el umbral inferior major** |


✅ Esta configuración muestra de forma integrada los picos de delta, divergencias y absorciones, con coherencia entre el panel Delta y el gráfico de precio.

---

### 🧪 Notas de desarrollo
* Se añadieron nuevas `ValueDataSeries` para Price Signals y Threshold Lines.
* Se reorganizó la UI (grupos *Visualization*, *Drawing*, *Absorption*).
* Se implementó lógica condicional entre alertas, señales y líneas guía.
* Comentarios normalizados al inglés y limpieza de código redundante.

### 🛠️ Propuestas de mejora futura
* Permitir selección de forma del marcador (triángulo, rombo, flecha).


<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE5NzkyMjEyOTRdfQ==
-->