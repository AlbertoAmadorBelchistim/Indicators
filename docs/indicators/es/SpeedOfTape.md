## 🟦 Speed of Tape (8/10)  
**Nombre del archivo:** `SpeedOfTape.cs`  
**Nombre del indicador:** Speed of Tape  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602472](https://help.atas.net/support/solutions/articles/72000602472)

---

### ⚙️ Parámetros configurables  
- **AutoFilter**: Activar/desactivar el filtro automático (por defecto: `true`)  
- **AutoFilterPeriod**: Período del filtro SMA (por defecto: `14`)  
- **Sec**: Número de segundos para calcular la "velocidad" del tape (por defecto: `15`)  
- **Trades**: Número de operaciones para filtrar (por defecto: `100`)  
- **Type**: Tipo de cálculo (Volume, Ticks, Buys, Sells, Delta)  
- **UseAlerts**: Activar alertas (por defecto: `false`)  
- **AlertFile**: Nombre del archivo de alerta  
- **AlertForeColor**: Color del texto de la alerta  
- **AlertBgColor**: Color de fondo de la alerta  
- **DrawLines**: Dibujar líneas para señales (por defecto: `true`)  
- **BarsLength**: Longitud de las líneas de señales (por defecto: `10`)  
- **MaxSpeedColor**: Color para la velocidad máxima  

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Indicador de velocidad del tape

---

### 🧠 Uso más frecuente  
- Medir la **velocidad del tape** observando el número de transacciones en un periodo de tiempo determinado  
- Analizar el **sentimiento del mercado** mediante el delta de compra/venta  
- Identificar **movimientos rápidos** en el mercado basados en el volumen o los ticks

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Excelente para identificar **movimientos rápidos** y cambios de ritmo  
✅ Compatible con **alertas automáticas** para señales en tiempo real  
⛔ No útil para mercados sin movimientos significativos o con bajo volumen

---

### 🎯 Estrategias de scalping donde se aplica  
- **Captura de movimientos rápidos**: Detectar aceleraciones de precio con alta actividad  
- **Confirmación de cambios de tendencia**: Observar la **aceleración en ticks o delta** para confirmar rupturas  
- **Filtrado de operaciones**: Evitar entrar en momentos de baja velocidad o volumen

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Sec**: `15`  
- **Trades**: `100`  
- **Type**: `Ticks`  
- **MaxSpeedColor**: `Yellow`  
- **DrawLines**: `true`  
- **BarsLength**: `10`

✅ Detecta rápidamente cambios de velocidad de tape  
✅ Las alertas permiten actuar con agilidad ante variaciones en el tape  
⛔ Menos efectivo en mercados sin volatilidad significativa

---

### 🧪 Notas de desarrollo  
- Calcula la **velocidad del tape** mediante volumen, ticks, compras, ventas o delta, según el tipo seleccionado  
- Utiliza un filtro SMA para suavizar los valores de velocidad  
- Los valores de velocidad máxima se destacan en color personalizado  
- Se pueden **dibujar líneas** en el gráfico para mostrar las señales de velocidad máxima  

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- El cálculo de la velocidad puede ser **menos preciso** en mercados con alta volatilidad o ruido  
- No permite personalizar **la fórmula de cálculo** de la velocidad  
- Las **alertas de velocidad** no son personalizables más allá de la activación por eventos de velocidad  
- La visualización de las **líneas de señal** podría mejorarse con más opciones de personalización

---

### 🛠️ Propuestas de mejora  
- Añadir **más tipos de filtro** para personalizar la detección de señales de tape  
- Permitir configurar **la longitud de las señales** y su visualización más detallada  
- Incluir **más tipos de alertas** y condiciones para personalizar los disparadores de alertas  
- Mejorar la **precisión del cálculo** en mercados altamente volátiles
