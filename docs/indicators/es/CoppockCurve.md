## 🟦 Coppock Curve (3/10)

**Nombre del archivo:** `CoppockCurve.cs`  
**Nombre del indicador:** Coppock Curve  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602602-coppock-curve](https://help.atas.net/support/solutions/articles/72000602602-coppock-curve)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de la media móvil ponderada (WMA) que suaviza la suma de los ROCs (por defecto: 10)

---

### 🧭 Clasificación  
📂 Momentum — Oscilador basado en tasas de cambio suavizadas

---

### 🧠 Uso más frecuente

- Identificar **cambios de ciclo** en tendencias de largo plazo  
- Detectar **zonas de compra óptimas** tras correcciones profundas  
- Utilizado como **herramienta de timing en mercados amplios** como índices bursátiles

---

### 📊 Nivel de relevancia  
🔟 **3 / 10**

✅ Suaviza la volatilidad y evita señales falsas  
✅ Ideal para detección de suelos en marcos temporales amplios  
⛔ Su señalización es lenta y poco útil para scalping  
⛔ No responde bien a fases laterales prolongadas

---

### 🎯 Estrategias de scalping donde se aplica

**No recomendado para scalping en 1M**  
Este indicador está diseñado para marcos **semanales o diarios**, por lo que **no se recomienda en gráficos intradía rápidos** como S&P 500 en 1 minuto.

---

### ⚙️ Parametrización óptima (uso contextual en D1 o W1)

- **Period**: `10`  
- **ROC largo**: 14  
- **ROC corto**: 11

✅ Esta configuración reproduce el método original de Coppock  
✅ Útil para confirmar cambios de fase en estrategias de swing trading

---

### 🧪 Notas de desarrollo

- El indicador **suma dos tasas de cambio** (ROC 11 y ROC 14, en modo porcentaje) y aplica una **media móvil ponderada** (WMA) sobre la suma resultante  
- El valor final se representa en forma de **histograma**, mostrando la inclinación o reversión de tendencia  
- La propiedad `Period` controla exclusivamente el suavizado final (WMA), pero **los ROCs tienen valores fijos en el código (11 y 14)**  
- No hay lógica de señal ni filtros adicionales: se deja a interpretación visual

---

### 🛠️ Propuestas de mejora

- Permitir que los **valores de los ROCs** (actualmente 11 y 14) sean configurables desde la UI  
- Añadir una **línea cero** como referencia visual en el histograma  
- Implementar una opción para **mostrar señales de entrada** cuando cruza de negativo a positivo  
- Incluir una versión adaptada para **marcos intradía**, con parámetros dinámicos o multitemporales