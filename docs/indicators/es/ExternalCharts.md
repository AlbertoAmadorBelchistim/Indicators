## 🟦 External Chart (6.5/10)

**Nombre del archivo:** `ExternalCharts.cs`  
**Nombre del indicador:** External Chart  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602383](https://help.atas.net/support/solutions/articles/72000602383)

---

### ⚙️ Parámetros configurables

- **TFrame**: Marco temporal externo a simular (M1, H1, H4, Daily, Weekly, etc.)  
- **Days**: Número de días hacia atrás desde los que comenzar a dibujar (por defecto: 20)  
- **ExtCandleMode**: Mostrar como vela (OHLC) o como bloque completo  
- **UpCandleColor / DownCandleColor**: Colores para velas alcistas y bajistas  
- **UpBackground / DownBackground**: Colores de relleno para velas alcistas y bajistas  
- **Width / Style**: Ancho y estilo del trazo de contorno de las velas  
- **FillCandles**: Rellenar los bloques o dejarlos huecos  
- **ShowGrid / GridColor**: Mostrar cuadrícula interior y su color  
- **Above**: Dibujar por encima del gráfico principal (DrawAbovePrice)

---

### 🧭 Clasificación  
📂 Visualization — Representación gráfica de velas externas en marcos personalizados

---

### 🧠 Uso más frecuente

- Simular un **marco temporal superior** dentro del gráfico actual  
- Visualizar **velas agregadas** sobre el gráfico de minuto/tick sin cambiar timeframe  
- Añadir contexto visual de velas H1, H4, Daily sin necesidad de cambiar de gráfico  
- Comparar microestructura contra comportamiento de velas largas

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Muy útil para análisis contextual y multitimeframe visual  
✅ Representación flexible, configurable y clara  
⛔ No realiza cálculos técnicos, solo representación visual  
⛔ Requiere carga suficiente de datos si se usa con Weekly o Monthly

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada microestructural validada por vela superior**: entrar en M1 si la vela H1 es envolvente o impulsiva  
- **Evitar entradas contra velas Daily dominantes**  
- **Buscar absorciones en extremos de vela mayor (H4 o D1)**

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **TFrame**: `H1` o `H4`  
- **Days**: `5`  
- **ExtCandleMode**: `true`  
- **FillCandles**: `true`  
- **Above**: `true`  
- **ShowGrid**: `false`  
- Colores suaves que contrasten con el gráfico principal (azul claro y rojo suave)

✅ Aporta visión contextual sin salir del marco de ejecución  
✅ Compatible con otras herramientas de clúster y delta

---

### 🧪 Notas de desarrollo

- Agrupa velas del timeframe actual en bloques que representan una vela mayor  
- Cada “vela externa” se construye en tiempo real con Open, High, Low, Close, y se dibuja como rectángulo o vela  
- Soporta marcos horarios (M1 a H6), y también Daily, Weekly y Monthly  
- Permite cuadrícula interna por tick y por barra si está en modo clúster  
- Los rectángulos se almacenan en `_rectangles` y se dibujan en `OnRender()`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El parámetro `AreaColor` está obsoleto pero aún aparece como propiedad pública (debería eliminarse o marcarse como `[Browsable(false)]`)  
- Si el timeframe es `Monthly`, se codifica como `0`, lo que puede provocar errores si se usa `TimeSpan` directamente  
- En ciertos modos, el cálculo de `AddRect` puede agregar rectángulos sin control si el gráfico está incompleto o mal cargado  
- Las líneas de cuadrícula se dibujan incluso si hay solapamiento con otras herramientas, sin control de opacidad adicional

---

### 🛠️ Propuestas de mejora

- Ocultar o eliminar la propiedad `AreaColor` si ya no tiene función  
- Añadir validación para evitar `TFrame.Monthly = 0` en diccionario de `TimeSpan` (puede causar excepción)  
- Exponer las velas como `CandleDataSeries` adicionales para usarlas con otros indicadores  
- Permitir dibujar solo cuerpo o mechas por separado  
- Añadir etiquetas opcionales con el valor de cierre o tamaño de vela para visualización táctica
