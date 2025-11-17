## 🟦 Squeeze Momentum (8/10)  
**Nombre del archivo:** `SqueezeMomentum.cs`  
**Nombre del indicador:** Squeeze Momentum  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602637](https://help.atas.net/support/solutions/articles/72000602637)

---

### ⚙️ Parámetros configurables  
- **BBPeriod**: Periodo de la media móvil para las Bandas de Bollinger (por defecto: `20`)  
- **BBMultFactor**: Factor de multiplicación para las Bandas de Bollinger (por defecto: `2.0`)  
- **KCPeriod**: Periodo de la media móvil para el Keltner Channel (por defecto: `20`)  
- **KCMultFactor**: Factor de multiplicación para el Keltner Channel (por defecto: `1.5`)  
- **UseTrueRange**: Usar el True Range en el cálculo del Keltner Channel (por defecto: `false`)  
- **UpperColor**: Color para el valor positivo del oscilador (por defecto: `Lime`)  
- **UpColor**: Color para el valor creciente del oscilador (por defecto: `Green`)  
- **LowColor**: Color para el valor negativo del oscilador (por defecto: `DarkRed`)  
- **LowerColor**: Color para el valor decreciente del oscilador (por defecto: `Red`)  
- **NullColor**: Color para los valores nulos (por defecto: `Blue`)  
- **FalseColor**: Color para los valores falsos (por defecto: `Gray`)  
- **TrueColor**: Color para los valores verdaderos (por defecto: `Black`)

---

### 🧭 Clasificación  
📂 Volatility — Indicador de momentum basado en el "squeeze" del mercado

---

### 🧠 Uso más frecuente  
- Detectar **fases de compresión y expansión** en el mercado (Squeeze y No Squeeze)  
- Identificar posibles **rupturas de volatilidad** con señales claras de compra/venta  
- Utilizar como filtro de **confirmación** en estrategias de breakout

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Ideal para **mercados laterales** y detectar rupturas de volatilidad  
✅ Útil para **confirmar movimientos fuertes** tras un periodo de consolidación  
⛔ Menos efectivo en **mercados con baja volatilidad constante** sin períodos de compresión

---

### 🎯 Estrategias de scalping donde se aplica  
- **Squeeze de volatilidad**: Confirmar **rupturas de precio** cuando las bandas de Bollinger y Keltner Channel se contraen  
- **Breakout trades**: Usar las señales de **expansión de volatilidad** para entrar tras la ruptura  
- **Filtrado de entradas falsas**: El indicador ayuda a evitar señales falsas durante la compresión del mercado

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **BBPeriod**: `20`  
- **BBMultFactor**: `2.0`  
- **KCPeriod**: `20`  
- **KCMultFactor**: `1.5`  
- **UpperColor**: `Lime`  
- **UpColor**: `Green`  
- **FalseColor**: `Gray`  

✅ Detecta y **confirma rupturas** con alta precisión  
✅ Muy útil para **operaciones basadas en volatilidad**  
⛔ Menos preciso en **mercados sin consolidación previa**

---

### 🧪 Notas de desarrollo  
- Utiliza **Bandas de Bollinger** y **Keltner Channel** para medir el "squeeze" de volatilidad  
- Calcula la **diferencia** entre los rangos de los canales y las bandas para determinar la fase de mercado  
- Las señales se colorean según la tendencia y la fuerza del movimiento (expansión o compresión de volatilidad)  
- Los valores de la fase de squeeze se representan con **puntos coloreados** sobre el gráfico

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No se validan los valores extremos para **prevenir cálculos erróneos** en mercados de baja liquidez  
- El indicador puede **demorar** en reaccionar si el mercado cambia de fase demasiado rápido  
- El sistema de **colores de las señales** podría mejorarse con más opciones de personalización  
- **Alertas** no disponibles directamente para el cambio de fase squeeze/no squeeze

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **alertas automáticas** cuando el mercado pase de squeeze a no squeeze  
- Permitir **personalización avanzada** de los colores y el estilo visual de las señales  
- Mejorar la **respuesta del indicador** en mercados muy rápidos o con saltos de volatilidad
