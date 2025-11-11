## 🟦 Active Volume (8/10)

  

**Nombre del archivo:**  `ActiveVolume.cs`

**Nombre del indicador:** Active Volume

**Web oficial:**  [ATAS - Active Volume](https://help.atas.net/ru-RU/support/solutions/articles/72000608343-active-volume)

  

---

  

### ⚙️ Parámetros configurables

  

- **Filter**: Volumen mínimo para acumular (por defecto: 50)

- **RowWidth**: Ancho de cada columna en la tabla (px)

- **ShowBid / ShowAsk / ShowVolume**: Mostrar columnas de Bid, Ask y Suma en tabla

- **Offset**: Desplazamiento de la tabla respecto al gráfico

- **DateFrom**: Fecha desde la cual comenzar a acumular datos

- **DigitsAfterComma**: Decimales mostrados en los valores

- **Mode (CalcMode)**: Modo de visualización (BidAsk / Bid / Ask)

- **ProfileWidth / ProfileOffset**: Ancho y desplazamiento del perfil dibujado

- **ProfileFillColor / BidProfileValueColor / AskProfileValueColor**: Colores de fondo y valores del perfil

  

---

  

### 🧭 Clasificación

📂 VolumeOrderFlow — Indicadores de volumen activo acumulado en nivel de precio

  

---

  

### 🧠 Uso más frecuente

  

- Visualizar la **acumulación de agresión bid y ask** por nivel de precio

- Identificar zonas donde hubo **actividad agresiva desequilibrada**

- Detectar absorciones o presencia institucional en zonas clave

- Confirmar volumen dominante tras ruptura o rechazo

  

---

  

### 📊 Nivel de relevancia

🔟 **8 / 10**

  

✅ Ofrece lectura visual precisa de acumulación de agresión por nivel de precio

✅ Ideal para validar rupturas, absorciones o rechazos

⛔ Requiere buena configuración y espacio visual en el gráfico

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Absorciones visuales**: Ask elevado + no rompe resistencia = venta

- **Rupturas reales**: fuerte bid agresivo apareciendo en ruptura

- **Rechazo de zona**: volumen concentrado pero sin continuación

- **Actividad institucional**: perfiles desequilibrados en niveles técnicos

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Filter**: `100`

- **RowWidth**: `14`

- **Mode**: `BidAsk`

- **ProfileWidth**: `30`

- **ProfileOffset**: `25`

- **DigitsAfterComma**: `2`

- **ShowBid / ShowAsk / ShowVolume**: ✅ activados

- **DateFrom**: sesión actual

  

✅ Esta configuración resalta zonas de absorción o agresión intensa

✅ Ideal para confirmar rompimientos y test de soportes/resistencias

⛔ Requiere espacio horizontal, mejor en pantalla secundaria o lateral

  

### 🧪 Notas de desarrollo

  

- El indicador acumula agresión Bid y Ask desde `DateFrom` usando objetos `CumulativeTrade`.

- Los volúmenes se almacenan por precio en diccionarios separados (`_bidValues`, `_askValues`).

- Se actualiza en tiempo real usando `OnCumulativeTrade` y `OnUpdateCumulativeTrade`.

- La representación visual incluye un perfil horizontal escalado y una tabla de valores por fila de precio.

- El filtro por volumen (parámetro `Filter`) evita acumular microtrades irrelevantes.

  

### 🛠️ Propuestas de mejora

  

- Añadir opción para **resetear acumulación cada sesión** sin tener que cambiar `DateFrom` manualmente.

- Incluir una **línea de POC (nivel de mayor volumen acumulado)**.

- Permitir **configurar escala logarítmica** en la representación del perfil para evitar saturación por valores extremos.

- Ofrecer una **vista resumida o colapsable** si el gráfico tiene muchas filas de precio visibles.

- Incluir **alertas visuales o sonoras** si un nivel supera cierto umbral de agresión.

## Comentario de Gemini
Aquí tienes la "pregunta clave" de este indicador:
Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios?

----------
### 🛠️ Tus Propuestas de Mejora son Geniales

1.  **"Añadir opción para resetear acumulación cada sesión"**: Esta es la **mejora más importante** que necesita. Para el scalping intradía, necesitas un perfil por sesión. Sin esto, los datos de la mañana "contaminan" el perfil de la tarde. Esta es una mejora de 10/10.
    
2.  **"Incluir una línea de POC"**: Estándar y necesario. Totalmente de acuerdo.
    
3.  **"Permitir escala logarítmica"**: Esta es una idea **brillante** y muy avanzada. Resuelve el mayor problema de los perfiles de volumen: un único _trade_ masivo (ej. 5000 lotes en la apertura) puede "achatar" visualmente el resto del perfil, haciendo que los trades de 200 lotes (que también son importantes) parezcan invisibles. Una escala logarítmica arreglaría esto.
    

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una de las herramientas de Order Flow más útiles que existen.**

Mientras que indicadores como el AMA o el ATR te dicen _qué_ pasó con el precio, este indicador te dice _por qué_ pasó. Te muestra la "batalla" de la oferta y la demanda en cada nivel de precio, filtrando el ruido.

**Acción:** **CONSERVAR Y MEJORAR.**

Este es un indicador para tu arsenal principal. Si implementas las mejoras que tú mismo sugeriste (especialmente el reseteo por sesión y la escala logarítimca), se convertiría en una herramienta de 10/10.
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTM2MTYxNDE5OV19
-->