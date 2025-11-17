## 🟦 Fisher Transform (6.5/10)

**Nombre del archivo:** `FisherTransform.cs`  
**Nombre del indicador:** Fisher Transform  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602385](https://help.atas.net/support/solutions/articles/72000602385)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para calcular el máximo y mínimo sobre los que se basa la transformación (por defecto: 10)

---

### 🧭 Clasificación  
📂 Momentum — Osciladores con normalización estadística para detectar giros

---

### 🧠 Uso más frecuente

- Detectar **zonas de sobrecompra o sobreventa** con base en una transformación estadística  
- Anticipar **giros extremos del mercado** con mayor sensibilidad que RSI o estocástico  
- Generar señales cuando la línea Fisher cruza su línea de activación (trigger)

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Responde bien a cambios extremos de comportamiento del precio  
✅ Su transformación estadística lo hace más reactivo en los bordes  
⛔ Puede generar muchas señales falsas en rangos estrechos  
⛔ Sensible a datos extremos (outliers) en el cálculo del rango

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce alcista**: cuando Fisher cruza por encima de la línea de activación  
- **Cruce bajista**: cuando Fisher cruza por debajo de la línea trigger  
- **Entrada tras giro**: en zonas donde Fisher alcanza extremos positivos o negativos y revierte

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `8` a `10`  
- Usar con líneas guía en ±1.5 y 0  
- Complementar con Delta o CVD para validar cambios reales de agresión

✅ Preciso en entornos de alta direccionalidad  
✅ Compatible con setups de reversión agresiva o seguimiento temprano

---

### 🧪 Notas de desarrollo

- Se basa en la transformación estadística de Fisher aplicada a una oscilación tipo WPR normalizada  
- Fórmulas clave:
  - WPR normalizado:
$$
    wpr = \frac{\text{Close} - \text{Min}}{\text{Max} - \text{Min}}
$$
  - Valor transformado:
    $$
    x = 0.66 \cdot (wpr - 0.5) + 0.67 \cdot x_{t-1}
    $$
  - Transformación Fisher:
    $$
    Fisher_t = 0.5 \cdot \ln\left(\frac{1 + x}{1 - x}\right) + 0.5 \cdot Fisher_{t-1}
    $$
- Se aplican límites (±0.999) al valor de entrada para evitar overflow en el logaritmo  
- `_triggers[bar]` se rellena con el valor anterior de Fisher como línea de señal

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El valor `_lastBar` se define pero **nunca se actualiza**, por lo que `bar != _lastBar` siempre es `true`, haciendo que se copie `_lastFisher` en cada barra sin optimización  
- Se usa `GetCandle(bar).High` para el máximo pero `Close` para el mínimo, lo que puede producir distorsión si hay velas con mechas largas  
- No hay validación si el rango `(sMax - sMin)` es demasiado estrecho, lo que puede hacer que el oscilador oscile bruscamente

---

### 🛠️ Propuestas de mejora

- Añadir lógica para actualizar `_lastBar = bar;` y evitar asignaciones redundantes  
- Usar el mismo tipo de dato (ej. High/Low o Close) para `sMax` y `sMin` por coherencia  
- Añadir visualización de líneas guía (±2, ±1, 0) como referencias de zona extrema  
- Permitir cambiar el color de la línea trigger o visualizarla como histograma
