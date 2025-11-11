## 🟦 Bill Williams Moving Average (BWMA) (5/10)

  

**Nombre del archivo:** `BWMA.cs`

**Nombre del indicador:** Bill Williams Moving Average

**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602334](https://help.atas.net/support/solutions/articles/72000602334)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Número de velas utilizadas en el cálculo suavizado

  

---

  

### 🧭 Clasificación

📂 Trend / Averages — Media móvil suavizada no lineal

  

---

  

### 🧠 Uso más frecuente

  

- Suavizar el precio para obtener una **media más sensible** al cambio sin ser muy reactiva

- Utilizar como línea de referencia para **seguimiento de tendencia**

- Confirmar la **fuerza o debilidad de un movimiento**

- Filtrar señales en sistemas con confirmación direccional

  

---

  

### 📊 Nivel de relevancia

🔟 **5 / 10**

  

✅ Más suave que la media exponencial, útil para ver contexto sin ruido

✅ Ideal para complementar estructuras como fractales o Alligator

⛔ No es ampliamente conocida fuera del enfoque Bill Williams

⛔ No se adapta a la volatilidad ni al volumen

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Filtro de tendencia**: solo operar a favor si el precio está por encima/debajo de la BWMA

- **Salida por cambio de dirección**: cruzar la media puede implicar debilidad

- **Confirmación visual**: el ángulo de inclinación ayuda a validar setups

- **Entrada retrasada**: usar como segunda confirmación tras patrón de vela

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `13`

  

✅ Proporciona una curva limpia, útil para filtrar contexto

✅ Funciona bien combinada con fractales, volumen o delta

⛔ No es recomendada como señal principal en marcos rápidos

  

---

  

### 🧪 Notas de desarrollo

  

- Cálculo específico:

`BWMA[bar] = (1 - 1 / Period) * BWMA[bar-1] + value / Period`

- Usa solo una `ValueDataSeries` llamada `RenderSeries`

- Color por defecto: azul

- No tiene lógica de alertas ni valores auxiliares

- Es un suavizado que combina características de la EMA con menor ruido

  

---

  

### 🛠️ Propuestas de mejora

  

- Incluir opción de **cambio de color por pendiente**

- Soporte para alertas por cruce con precio u otras medias

- Añadir múltiples líneas BWMA para crear canal visual

- Visualización opcional como banda (tipo Ribbon)

## Opinión Gemini
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTI5NDY3MjI3Nl19
-->