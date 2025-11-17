## 🟦 Trades On Chart (9/10)  
**Nombre del archivo:** `TradesOnChart.cs`  
**Nombre del indicador:** Trades On Chart  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000633119](https://help.atas.net/support/solutions/articles/72000633119)

---

### ⚙️ Parámetros configurables  
- **ShowLine**: Mostrar líneas que conectan las **entradas** y **salidas** de las transacciones (por defecto: `true`)  
- **ShowTooltip**: Mostrar las **descripciones** de las transacciones (por defecto: `true`)  
- **BuyColor**: Color para las barras de **compra** (por defecto: `Green`)  
- **SellColor**: Color para las barras de **venta** (por defecto: `Red`)  
- **LineWidth**: Ancho de las líneas de conexión entre las transacciones (por defecto: `2`)  
- **LineStyle**: Estilo de línea para las conexiones (por defecto: `Dash`)  
- **MarkerSize**: Tamaño de los marcadores de las transacciones (por defecto: `2`)

---

### 🧭 Clasificación  
📂 Visualization — Indicador para mostrar las **transacciones en el gráfico**

---

### 🧠 Uso más frecuente  
- Visualizar las **transacciones de compra y venta** directamente en el gráfico  
- Mostrar los **resultados de las transacciones** y calcular el **PNL** (ganancias o pérdidas)  
- **Marcar entradas y salidas** de cada transacción con líneas y colores personalizados

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  
✅ Ideal para visualizar las **transacciones pasadas** y sus resultados directamente en el gráfico  
✅ Muy útil para **analizar el rendimiento histórico de las transacciones**  
⛔ No es útil para **predecir movimientos futuros** o para **estrategias de trading en tiempo real**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de entrada/salida**: Usar las **líneas de transacción** para confirmar puntos de **entrada y salida**  
- **Gestión de riesgos**: Analizar las transacciones anteriores para **ajustar tamaños de posición** y **gestionar el riesgo**  
- **PNL de las transacciones**: Ver el **rendimiento histórico** de las transacciones para mejorar la toma de decisiones

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **ShowLine**: `true`  
- **ShowTooltip**: `true`  
- **BuyColor**: `Green`  
- **SellColor**: `Red`  
- **LineWidth**: `2`  
- **MarkerSize**: `2`

✅ Funciona bien para **mostrar transacciones pasadas** y verificar su rendimiento  
✅ Las **descripciones y líneas de transacciones** ayudan a evaluar el **rendimiento histórico**  
⛔ No es útil para **visualizar transacciones en tiempo real**

---

### 🧪 Notas de desarrollo  
- Muestra las **transacciones de compra y venta** utilizando **líneas de conexión** y **marcadores**  
- Calcula el **PNL** y muestra el rendimiento de cada transacción en función del precio de apertura y cierre  
- Permite personalizar los colores y el estilo de las líneas y marcadores para las transacciones

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No permite personalizar **el estilo de las descripciones** o la **información mostrada en los tooltips**  
- **Alertas no configurables** basadas en las transacciones de compra/venta  
- La **visualización de las líneas de transacción** podría ser más flexible en cuanto a colores y estilos

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **alertas automáticas** cuando se produzcan transacciones de compra/venta  
- Mejorar la **personalización visual** de las transacciones (colores, tamaños de líneas y marcadores)  
- Permitir ajustes de **transparencia** para las líneas de transacciones y los marcadores para mejorar la visibilidad
