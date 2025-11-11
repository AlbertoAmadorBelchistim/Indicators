## 🟦 Bands / Envelope (4/10)

  

**Nombre del archivo:** `BandsEnvelope.cs`

**Nombre del indicador:** Bands / Envelope

**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602522](https://help.atas.net/support/solutions/articles/72000602522)

  

---

  

### ⚙️ Parámetros configurables

  

- **CalcMode**: Modo de cálculo del rango (Percentage / Value / Ticks)

- **RangeFilter**: Rango de desviación respecto al precio (solo para Value y Ticks)

  

---

  

### 🧭 Clasificación

📂 Volatility — Indicadores de bandas alrededor del precio

  

---

  

### 🧠 Uso más frecuente

  

- Visualizar **envolventes de precio** con distintos métodos de cálculo (porcentual, valor fijo o en ticks)

- Identificar zonas de sobreextensión o sobrecompra/sobreventa

- Estimar **canales de rango dinámico** para sistemas basados en reversión

  

---

  

### 📊 Nivel de relevancia

🔟 **4 / 10**

  

✅ Versátil y compatible con distintos enfoques (ticks, porcentaje o valor)

✅ Útil para sistemas de reversión simples

⛔ No considera volatilidad implícita ni volumen

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Reversión al centro**: entrada cuando el precio toca la banda superior/inferior y hay señales de agotamiento

- **Confirmación de breakout**: rotura sostenida de la banda con volumen creciente

- **Canal de referencia**: combinación con otros indicadores para definir zonas de entrada/salida

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **CalcMode**: `Ticks`

- **RangeFilter**: `12` (aprox. 3 puntos o 12 ticks)

  

✅ Permite detectar sobreextensiones de corto plazo

⛔ No reacciona a cambios de volatilidad si no se ajusta manualmente

  

---

  

### 🧪 Notas de desarrollo

  

- El indicador calcula bandas simétricas alrededor del precio actual según el modo seleccionado:

- **Percentage**: `(± range % del precio)`

- **Value**: `(± valor absoluto)`

- **Ticks**: `(± valor en ticks del instrumento)`

- El área entre bandas se representa mediante un `RangeDataSeries` con líneas superior e inferior

- El cálculo se actualiza en tiempo real para cada vela nueva

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir **media central** entre bandas (tipo media móvil) para análisis de reversión

- Incluir **opción de suavizado** para evitar repintado excesivo en entornos volátiles

- Posibilidad de activar **alertas cuando el precio cruza una banda**

- Soporte para **desplazamiento horizontal** de las bandas para backtesting visual
<!--stackedit_data:
eyJoaXN0b3J5IjpbMjM2MjA0NF19
-->