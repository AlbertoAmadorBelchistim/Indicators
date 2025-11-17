## 🟦 Volatility – Historical (8 / 10)  
**Nombre del archivo:** `VolatilityHist.cs`  
**Nombre del indicador:** Volatility - Historical  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602266](https://help.atas.net/support/solutions/articles/72000602266)

---

### ⚙️ Parámetros configurables  
- **Period**: Número de barras para calcular la desviación estándar logarítmica (por defecto: `10`)

---

### 🧭 Clasificación  
📂 Volatility — Volatilidad histórica basada en desviación estándar logarítmica

---

### 🧠 Uso más frecuente  
- Medir la **volatilidad histórica real** del activo a través del **log-retorno**  
- Comparar periodos de **alta o baja variación relativa**  
- Usar como filtro para activar o desactivar estrategias en función de la **volatilidad implícita reciente**

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Basado en una medida **estandarizada y estadísticamente robusta**  
✅ Muy útil como **indicador de contexto** para evaluar riesgo  
⛔ Puede ser complejo de interpretar sin conocimiento de estadística financiera

---

### 🎯 Estrategias de scalping donde se aplica  
- **Activación por volatilidad**: Ejecutar estrategias solo si la volatilidad supera cierto nivel  
- **Tamaño de stop variable**: Ajustar el riesgo según el valor del indicador  
- **Filtrado de entorno**: Evitar operar en zonas con compresión extrema

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Period**: `10`

✅ Ofrece una métrica de **volatilidad confiable** y comparable entre activos  
✅ Útil para **gestión de riesgo dinámica**  
⛔ No es un indicador de señal, sino **de contexto**

---

### 🧪 Notas de desarrollo  
- Calcula el **logaritmo natural del cociente** entre el cierre actual y el anterior  
- Usa una **SMA** para suavizar los log-retornos y calcular la **varianza**  
- Escala el resultado por 100 y por la raíz cuadrada del número de barras (como en estadística de muestra)

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- Usa `CurrentBar` en lugar de `Period` en el cálculo final del escalado (puede provocar errores al inicio)  
- No hay protección ante división por cero si `SourceDataSeries[bar - 1] == 0`  
- No incluye líneas guía ni umbrales para facilitar su lectura

---

### 🛠️ Propuestas de mejora  
- Corregir uso de `CurrentBar` por `Period` para coherencia estadística  
- Añadir **alertas automáticas** o líneas guía para niveles clave de volatilidad  
- Incluir opciones visuales para representar volatilidad media, máxima o mínima
