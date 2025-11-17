## 🟦 Starc Bands (7/10)  
**Nombre del archivo:** `StarcBands.cs`  
**Nombre del indicador:** Starc Bands  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602475](https://help.atas.net/support/solutions/articles/72000602475)

---

### ⚙️ Parámetros configurables  
- **Period**: Periodo de la media móvil simple (SMA) (por defecto: `10`)  
- **SmaPeriod**: Periodo del ATR (Average True Range) (por defecto: `10`)  
- **TopBand**: Multiplicador para el cálculo de la banda superior (por defecto: `1`)  
- **BotBand**: (Obsoleto) Multiplicador para el cálculo de la banda inferior (por defecto: `1`)

---

### 🧭 Clasificación  
📂 Volatility — Bandas de volatilidad basadas en el ATR

---

### 🧠 Uso más frecuente  
- Establecer **bandas de volatilidad** en torno a una media móvil simple  
- Detectar **condiciones de sobrecompra o sobreventa** cuando el precio toca las bandas  
- Confirmar **rupturas** o **expansiones de volatilidad** en el mercado

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**  
✅ Ideal para **identificar cambios de volatilidad**  
✅ Útil para **filtrar señales en condiciones de alta volatilidad**  
⛔ No tan eficaz en **mercados con baja volatilidad**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Rupturas de bandas**: Usar la **ruptura de la banda superior** como señal de compra y la **banda inferior** para venta  
- **Sobrecompra/sobreventa**: Identificar zonas donde el precio toca las bandas para posibles **reversiones**  
- **Confirmación de tendencias**: Usar la **expansión de las bandas** para confirmar el inicio de una tendencia fuerte

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Period**: `10`  
- **SmaPeriod**: `10`  
- **TopBand**: `2`  

✅ Ideal para capturar **rupturas y expansiones de volatilidad**  
✅ Las **bandas de volatilidad** son útiles para determinar **niveles de entrada y salida**  
⛔ Menos eficaz si el mercado no muestra grandes **movimientos de volatilidad**

---

### 🧪 Notas de desarrollo  
- Calcula las **bandas superior e inferior** basadas en el ATR multiplicado por un factor de ajuste  
- La **media móvil simple (SMA)** se utiliza como base para las bandas  
- La visualización muestra **tres líneas**: la SMA, la banda superior y la banda inferior

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- El parámetro **BotBand** está obsoleto y no se utiliza de manera efectiva en los cálculos  
- No se permite personalizar el **color o grosor de las bandas**  
- Las alertas no se pueden personalizar directamente, lo que limita su uso en estrategias automatizadas  
- La **configuración de los períodos** puede no adaptarse a mercados con alta volatilidad

---

### 🛠️ Propuestas de mejora  
- Eliminar el parámetro **BotBand** o incluirlo como un parámetro activo si es necesario  
- Permitir la **personalización avanzada** de la visualización (colores y grosor de las bandas)  
- Añadir **alertas automáticas** cuando el precio toque o rompa las bandas de volatilidad  
- Implementar un **filtro dinámico de volatilidad** que ajuste las bandas en función de la **volatilidad del mercado**
