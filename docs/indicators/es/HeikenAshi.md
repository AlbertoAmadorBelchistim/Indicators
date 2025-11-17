## 🟦 Heiken Ashi (6.5/10)

**Nombre del archivo:** `HeikenAshi.cs`  
**Nombre del indicador:** Heiken Ashi  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602391](https://help.atas.net/support/solutions/articles/72000602391)

---

### ⚙️ Parámetros configurables

- **Days**: Número de días atrás desde los cuales comenzar a calcular las velas Heiken Ashi (por defecto: 20)

---

### 🧭 Clasificación  
📂 Visualization — Representación alternativa de velas para suavizar estructura

---

### 🧠 Uso más frecuente

- Suavizar el comportamiento del precio para eliminar ruido  
- Detectar **cambios de tendencia** visualmente más claros  
- Evaluar fases de **impulso o retroceso** mediante la dirección de las velas

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Ideal para detectar visualmente la dirección y estabilidad de la tendencia  
✅ Compatible con otros indicadores sin interferir en cálculos  
⛔ No aporta señales por sí mismo  
⛔ Puede retrasar las señales reales al suavizar los datos

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada tras dos velas consecutivas del mismo color**  
- **Confirmación visual de tendencia** antes de ejecutar setups más agresivos  
- **Filtro contextual**: operar solo en dirección de Heiken Ashi si se mantiene firme

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Days**: `20`  
- Dibujar sobre el gráfico principal (`DenyToChangePanel = true`)  
- Usar junto con Delta o CVD para validar contexto

✅ Ayuda a filtrar lateralidad y ruido  
✅ Compatible con setups visuales estructurales

---

### 🧪 Notas de desarrollo

- En `bar == 0` se calcula el **_targetBar** desde el cual empezar a mostrar las velas, contando sesiones hacia atrás  
- La vela Heiken Ashi se calcula como:

  - **Close**: media de (Open, High, Low, Close)  
  - **Open**: media de la vela Heiken Ashi anterior (Open + Close)  
  - **High**: máximo entre High real, Open Heiken Ashi y Close Heiken Ashi  
  - **Low**: mínimo entre Low real, Open Heiken Ashi y Close Heiken Ashi  

- Se representa usando un `CandleDataSeries` llamado `_candles`  
- Los colores se adaptan automáticamente desde la configuración del gráfico (`ChartInfo.ColorsStore`)

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El cálculo de `_targetBar` depende de `IsNewSession(i)` sin validación de que haya suficientes sesiones disponibles  
- El valor de `bar == 0` puede ejecutar un bucle innecesariamente si `Days = 0`  
- No hay opción para personalizar el método de suavizado o incluir líneas de tendencia sobre las velas

---

### 🛠️ Propuestas de mejora

- Añadir opción para incluir el cálculo desde fecha específica o número de barras  
- Exponer la vela Heiken Ashi como serie secundaria para combinarla con otros cálculos  
- Permitir representar también la dirección mediante histograma u oscilador  
- Incluir alerta visual al detectar cambio de color entre velas
