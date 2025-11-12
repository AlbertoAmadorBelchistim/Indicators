## 🟦 Bollinger Squeeze 3 (6.5/10)

  

**Nombre del archivo:**  `BollingerSqueezeV3.cs`

**Nombre del indicador:** Bollinger Squeeze 3

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602338](https://help.atas.net/support/solutions/articles/72000602338)

  

---

  

### ⚙️ Parámetros configurables

  

#### ATR

- **AtrPeriod**: Periodo del ATR

- **AtrMultiplier**: Multiplicador aplicado al ATR

  

#### StdDev

- **StdDevPeriod**: Periodo de la desviación estándar

- **StdMultiplier**: Multiplicador aplicado a la desviación estándar

  

#### Visualización

- **PosColor**: Color para valores mayores o iguales a 1

- **NegColor**: Color para valores menores a 1

  

---

  

### 🧭 Clasificación

📂 Volatility / Squeeze — Comparación entre desviación estándar y ATR

  

---

  

### 🧠 Uso más frecuente

  

- Medir si la **volatilidad actual (std)** está por encima o por debajo de la media del rango (ATR)

- Detectar **cambios de régimen de volatilidad**

- Confirmar si hay condiciones de expansión o contracción

- Filtrar operativas en función del tipo de movimiento esperado

  

---

  

### 📊 Nivel de relevancia

🔟 **6.5 / 10**

  

✅ Cálculo simple y directo del ratio std/ATR

✅ Útil como filtro de entorno de mercado (volátil o contenido)

⛔ No tiene contexto direccional ni momentum

⛔ No indica compresión clásica (BB vs KC)

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Filtro de entorno activo**: solo operar si el valor ≥ 1 (alta volatilidad relativa)

- **Evitar operaciones en consolidación**: valores < 1 indican rango estrecho

- **Detectar puntos de inflexión**: cambios bruscos en el ratio pueden anticipar movimiento

- **Complementar con squeeze V1 o V2** para señales más completas

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **AtrPeriod**: `14`, **AtrMultiplier**: `1.0`

- **StdDevPeriod**: `14`, **StdMultiplier**: `1.0`

- **PosColor**: verde

- **NegColor**: rojo

  

✅ Proporciona un histograma limpio y directo

✅ Permite detectar fases de baja volatilidad operativamente inútiles

⛔ Necesita combinaciones para generar señales

  

---

  

### 🧪 Notas de desarrollo

  

- Calcula: $$Ratio = (StdDev × Multiplier) / (ATR × Multiplier)$$

- Muestra el valor resultante como histograma vertical

- Color verde si el valor ≥ 1, rojo si < 1

- Solo una `ValueDataSeries` (`RenderSeries`), sin bandas ni líneas auxiliares

- No tiene alertas ni líneas de umbral visibles

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir una **línea base en valor 1** para mayor claridad visual

- Incluir umbrales adicionales con coloración dinámica

- Soporte para alertas por cruce de umbral

- Posibilidad de mostrar líneas de ATR y StdDev como referencia externa

### Comentario Gemini
<!--stackedit_data:
eyJoaXN0b3J5IjpbMjA1Mjg5Mzc2OF19
-->