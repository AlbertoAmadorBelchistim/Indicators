## 🟦 Standard Deviation (StdDev) (8/10)  
**Nombre del archivo:** `StdDev.cs`  
**Nombre del indicador:** Standard Deviation  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602477](https://help.atas.net/support/solutions/articles/72000602477)

---

### ⚙️ Parámetros configurables  
- **Period**: Periodo de la media móvil simple (SMA) para el cálculo de la desviación estándar (por defecto: `10`)

---

### 🧭 Clasificación  
📂 Volatility — Indicador de volatilidad basado en la desviación estándar

---

### 🧠 Uso más frecuente  
- Medir la **volatilidad del mercado** mediante la variación de precios respecto a la media  
- Identificar **movimientos extremos** y cambios de tendencia mediante un aumento en la volatilidad  
- Utilizar como **filtro de confirmación** en estrategias de ruptura o reversión

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Ideal para medir **variabilidad de precios** y detectar **picos de volatilidad**  
✅ Funciona bien en **mercados con altos movimientos de precios**  
⛔ No es tan útil en mercados con **volatilidad constante o baja**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Breakouts**: Confirmar si un movimiento de precios es válido tras una **ruptura de la desviación estándar**  
- **Identificación de volatilidad**: Usar el aumento de la desviación estándar como indicador de **movimientos fuertes**  
- **Filtro de ruido**: Filtrar **movimientos erráticos** en mercados con baja volatilidad

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Period**: `10`

✅ Ideal para **confirmar aumentos de volatilidad** y medir la estabilidad del mercado  
✅ Funciona bien en **mercados que muestran cambios rápidos de tendencia**  
⛔ Menos efectivo en mercados con **tendencias estables y predecibles**

---

### 🧪 Notas de desarrollo  
- Calcula la **desviación estándar** de los precios utilizando una **media móvil simple** como base  
- La fórmula utilizada es:  
  `StdDev = sqrt(sum((price - sma)^2) / period)`
- El valor calculado se utiliza para representar la **volatilidad** del mercado en forma de una serie de valores

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No valida los **precios nulos** o erróneos en el cálculo de la desviación estándar  
- La **personalización visual** del indicador es limitada (no permite cambiar el estilo o color de la línea de desviación estándar)  
- La **respuesta en mercados muy volátiles** puede no ser tan precisa debido a los altos cambios rápidos de precios

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **alertas automáticas** cuando la desviación estándar supere un umbral configurado  
- Mejorar la **personalización visual** (grosor de línea, colores) del indicador  
- Permitir **ajustes dinámicos** en la fórmula para adaptarse a distintos tipos de mercados
