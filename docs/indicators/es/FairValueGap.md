## 🟦 Fair Value Gap (9/10)

**Nombre del archivo:** `FairValueGap.cs`  
**Nombre del indicador:** Fair Value Gap  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000618795](https://help.atas.net/support/solutions/articles/72000618795)

---

### ⚙️ Parámetros configurables

- **HigherTimeframe**: Marco temporal superior desde el cual detectar gaps  
- **ShowCurrentTF / ShowHigherTF**: Mostrar gaps del marco actual o superior  
- **MidpointTouch**: Usar o no la línea media como condición de cierre  
- **HideOlds**: Ocultar gaps ya cerrados  
- **Transparency**: Transparencia de las zonas (0 a 10)  
- **Colores**: Colores de los gaps para marcos actual y superior (bullish/bearish)  
- **MidPointColor / MidPointWidth**: Color y grosor de la línea del punto medio  
- **ShowLabel / LabelSize / LabelColor / LabelOffsetX / LabelOffsetY**: Opciones visuales de etiquetas

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Detección de zonas de desequilibrio institucional por estructura de velas

---

### 🧠 Uso más frecuente

- Identificar **zonas de desequilibrio entre oferta y demanda**  
- Detectar huecos de valor justo (Fair Value Gaps) en marcos múltiples  
- Visualizar zonas de posible **absorción, reversión o continuidad institucional**

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Compatible con múltiples marcos y estructuras simultáneas  
✅ Dibujo limpio, con lógica clara y etiquetas visuales  
⛔ Complejo de entender si no se conoce la teoría de FVG  
⛔ Requiere buena gestión de recursos si se usa con gráficos extensos o en múltiples instancias

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión en FVG no cerrada**: entrada al volver a testear el gap desde el lado opuesto  
- **Confirmación institucional**: gap que no se cierra tras varios intentos → sesgo direccional  
- **Spring o Upthrust**: si la mecha crea un FVG y luego lo respeta  
- **Entrada agresiva si hay confluencia con Delta, DOM o CVD en el borde del FVG**

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **HigherTimeframe**: `M15` o `H1`  
- **ShowCurrentTF**: `true`  
- **MidpointTouch**: `true`  
- **Transparency**: `5`  
- **ShowLabel**: `true`  
- **LabelColor**: `Gray`, `LabelSize`: `10`

✅ Muy útil para validar entrada con contexto institucional  
✅ Compatible con zonas clave como POC, VAL/VAH o Imbalances

---

### 🧪 Notas de desarrollo

- Evalúa gaps creados por un salto entre la vela actual y la de dos velas atrás (`bar - 2`)  
- Crea rectángulos entre el `High` de la vela 2 y el `Low` de la actual (o viceversa)  
- Si el precio vuelve a tocar la zona (o su punto medio), se considera **cerrado**  
- Soporta cálculo de FVG en marcos personalizados (Daily, H4, H1, etc.)  
- Usa listas internas de `Signal` para almacenar cada gap abierto  
- El renderizado se realiza en `OnRender` y respeta filtros de visibilidad y color

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El valor de `_secondsPerCandle` depende del `TimeFrame` por texto, lo que puede fallar si se cambian convenciones de nombre  
- La condición `if (signal.EndBar > 0)` en `DrawGaps()` puede hacer que **no se vean gaps cerrados recientes si HideOlds está activo**  
- En `CreateNewGap()`, puede haber repetición si varias velas consecutivas cumplen condiciones similares  
- El uso de `RoundToFraction()` para calcular `midPrice` puede provocar desajustes visuales si el gap tiene tamaño impar (en ticks)

---

### 🛠️ Propuestas de mejora

- Exponer como `RangeDataSeries` los gaps activos para trazado cruzado u otras herramientas  
- Añadir **alertas visuales/sonoras** cuando un FVG se toca o se cierra  
- Incluir una opción para **marcar los gaps que nunca se cerraron**  
- Añadir una estadística de porcentaje de cierre vs permanencia de FVG  
- Mostrar datos de volumen dentro del gap si hay clúster disponible
