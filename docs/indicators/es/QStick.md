## 🟦 Q Stick (6/10)

**Nombre del archivo:** `QStick.cs`  
**Nombre del indicador:** Q Stick  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602451](https://help.atas.net/support/solutions/articles/72000602451)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para calcular la media de la diferencia Open–Close (por defecto: 10)

---

### 🧭 Clasificación
📂 Momentum — Media móvil de la diferencia entre cierre y apertura

---

### 🧠 Uso más frecuente

- Medir el **impulso direccional promedio** de las velas  
- Detectar **sesgos alcistas o bajistas acumulados**  
- Confirmar rupturas o giros mediante persistencia del signo del QStick

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Indicador simple, útil como filtro o confirmador direccional  
✅ Puede combinarse con otros osciladores para filtrar señales falsas  
⛔ Poco conocido y limitado si se usa de forma aislada

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de dirección**: operar solo si el QStick es positivo/negativo  
- **Cruce con la línea cero** como gatillo de entrada o salida  
- **Filtro de contexto**: evitar operar en rangos si QStick permanece cercano a cero

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `9`

✅ Detecta con rapidez cambios sostenidos en la dirección de las velas  
✅ Sencillo de leer y configurar  
⛔ Puede necesitar combinación con volumen o delta para robustecer la señal

---

### 🧪 Notas de desarrollo

- Calcula la diferencia entre cierre y apertura en cada vela y guarda en `_openCloseSeries`  
- La serie principal (`_renderSeries`) es la media de esas diferencias en el periodo seleccionado  
- Añade una línea horizontal fija en cero (`ZeroVal`) como referencia visual  
- Soporta `MinimizedMode` para visualización simplificada  
- Usa `CalcSum(period, bar)` para optimizar cálculo acumulado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se valida si el número de barras cargadas es menor que `Period`  
- No permite aplicar suavizado adicional (ej: EMA del QStick)  
- El nombre del indicador puede confundirse con indicadores de tipo "stick" de patrones de velas  
- No ofrece alertas ni visualización dinámica de color según dirección  
- No expone la serie de diferencia cruda (`Close - Open`) como serie auxiliar visible

---

### 🛠️ Propuestas de mejora

- Añadir opción para suavizar el resultado con EMA o SMA adicional  
- Incluir alertas visuales o sonoras al cruce con cero  
- Permitir mostrar la serie `Close - Open` como histograma adicional  
- Añadir codificación de color en la línea principal según signo o pendiente  
- Documentar en la interfaz cómo se interpreta el QStick y sus rangos típicos

