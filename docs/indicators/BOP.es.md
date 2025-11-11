## 🟦 Balance of Power (BOP) (5.5/10)

  

**Nombre del archivo:** `BOP.cs`

**Nombre del indicador:** Balance of Power

**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602623](https://help.atas.net/support/solutions/articles/72000602623)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Periodo de la media móvil simple aplicada al cálculo del BOP

  

---

  

### 🧭 Clasificación

📂 Momentum / PriceAction — Indicador de fuerza relativa entre apertura y cierre

  

---

  

### 🧠 Uso más frecuente

  

- Medir el **desequilibrio entre compradores y vendedores** dentro de una vela

- Confirmar si una barra fue dominada por la presión compradora o vendedora

- Evaluar la **intensidad relativa del cierre** dentro del rango de la vela

- Usado como oscilador para detectar **divergencias o confirmaciones**

  

---

  

### 📊 Nivel de relevancia

🔟 **5.5 / 10**

  

✅ Sencillo de interpretar en contextos claros de tendencia

✅ Útil como complemento en osciladores multi-fuente

⛔ Menos eficaz en velas de rango estrecho

⛔ No incluye volumen ni análisis de order flow

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Confirmar impulso real**: BOP creciente junto a rupturas o breakout

- **Detectar reversión débil**: divergencias entre precio y BOP

- **Filtrar señales**: evitar entradas si el BOP no confirma el movimiento

- **Validar absorciones**: vela verde con BOP negativo → posible engaño

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `14`

  

✅ Suaviza las señales sin perder agilidad en marcos rápidos

✅ Compatible con setups que buscan validación estructural

⛔ Evitar usarlo como única señal de entrada

  

---

  

### 🧪 Notas de desarrollo

  

- Cálculo base por vela:

`(Close - Open) / (High - Low)`

si `(High - Low) != 0`, de lo contrario BOP = 0

- El valor se suaviza mediante una media móvil simple (`SMA`)

- El resultado se representa como una sola serie (`RenderSeries`)

- No utiliza volumen ni delta en el cálculo

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir opción de suavizado por otros tipos de medias (EMA, WMA)

- Incluir alertas por cruce de cero o por divergencia

- Integrar filtros por rango mínimo para evitar barras insignificantes

- Posibilidad de combinar con delta o volumen para mayor precisión
<!--stackedit_data:
eyJoaXN0b3J5IjpbOTYyODE2MDE2XX0=
-->