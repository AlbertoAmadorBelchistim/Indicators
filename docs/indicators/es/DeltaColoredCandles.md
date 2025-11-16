## 🟦 Delta Colored Candles (7/10)

**Nombre del archivo:** `DeltaColoredCandles.cs`  
**Nombre del indicador:** Delta Colored Candles  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000618743](https://help.atas.net/support/solutions/articles/72000618743)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para acumular el delta (por defecto: 14)  
- **MaxDelta**: Delta máximo esperado para escalar el color (por defecto: 600)  
- **ColorScheme**: Esquema de color del heatmap (`RedToDarkToGreen`, `GreenToRed`, etc.)

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Velas coloreadas según delta acumulado

---

### 🧠 Uso más frecuente

- Visualizar la **intensidad de la agresión neta Bid vs Ask** en forma de color  
- Detectar acumulaciones o momentos de delta extremo de forma visual  
- Confirmar rupturas o rechazos con validación cromática  
- Suavizar la percepción del delta vela a vela con un enfoque acumulativo

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Muy útil para detección visual rápida de presión acumulada  
✅ Compatible con otras herramientas basadas en agresión y order flow  
⛔ No muestra delta en valores, solo color  
⛔ Requiere calibración correcta de MaxDelta para no saturar colores

---

### 🎯 Estrategias de scalping donde se aplica

- **Ruptura con confirmación de agresión acumulada** (vela en verde fuerte)  
- **Absorción o rechazo** si tras gran volumen aparece una vela débil (color neutro)  
- **Análisis de continuidad**: varios colores fuertes consecutivos indican momentum sostenido  
- **Filtro de entrada**: solo operar si el color confirma la dirección de la señal

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `8` o `14`  
- **MaxDelta**: `5000` (ajustable según liquidez del activo)  
- **ColorScheme**: `RedToDarkToGreen`

✅ Aporta lectura rápida sin sobrecargar el gráfico  
✅ Útil para operadores visuales o tácticos

---

### 🧪 Notas de desarrollo

- El indicador guarda el delta por vela en `_delta[bar]`  
- Calcula la suma de delta en una ventana de longitud `Period`  
- Escala el resultado con respecto a `MaxDelta`, obteniendo un porcentaje  
- Ajusta el **valor visual (rate)** como:  
  $$
  \text{rate} = 50 + \left(\frac{\text{sumDelta} \times 100}{\text{MaxDelta}}\right) / 2
  $$
- El color resultante se obtiene mediante `HeatmapExtensions.GetColor()`  
- El color se asigna a la barra mediante `_colorBars[bar]`

---

### ❗ Incoherencias o aspectos mejorables detectados

- El valor de `rate` puede **superar fácilmente 100**, lo que puede provocar errores si el método `GetColor()` no lo controla internamente  
- No hay **lógica de normalización por volatilidad o tamaño de vela**, lo que puede sesgar la interpretación en rangos estrechos  
- No muestra la escala de colores en el gráfico, lo que puede dificultar la lectura precisa  
- El `Period` y `MaxDelta` deben ser calibrados manualmente por el usuario

---

### 🛠️ Propuestas de mejora

- Limitar `rate` a un máximo de 100 antes de pasarlo a la función de color  
- Añadir opción para mostrar **leyenda de colores** o escala de intensidad  
- Permitir coloración basada en delta relativo o dividido por rango o volumen  
- Incluir una opción para **dibujar también etiquetas de delta** además del color  
- Posibilidad de activar/deactivar dinámicamente el heatmap sin borrar los datos

