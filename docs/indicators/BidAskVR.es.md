## 🟦 Bid Ask Volume Ratio (7/10)

  

**Nombre del archivo:**  `BidAskVR.cs`

**Nombre del indicador:** Bid Ask Volume Ratio

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602330](https://help.atas.net/support/solutions/articles/72000602330)

  

---

  

### ⚙️ Parámetros configurables

  

#### Cálculo

- **CalcMode** (`AskBid` o `BidAsk`): define la dirección del desequilibrio:

- `AskBid` = (Ask - Bid)

- `BidAsk` = (Bid - Ask)

- **Period**: número de velas usadas en la media móvil

  

#### Tipo de media móvil (`MaType`)

- `Sma`: media simple

- `Ema`: media exponencial

- `Wma`: media ponderada

- `LinReg`: regresión lineal

- `Smma`: media suavizada

  

#### Colores del histograma

- **UpperColor**: valor positivo con pendiente creciente

- **UpColor**: valor positivo con pendiente decreciente

- **LowerColor**: valor negativo con pendiente decreciente

- **LowColor**: valor negativo con pendiente creciente

  

---

  

### 🧭 Clasificación

📂 VolumeOrderFlow — Ratio de volumen Bid/Ask suavizado

  

---

  

### 🧠 Uso más frecuente

  

- Detectar **desequilibrios de agresión** entre compradores y vendedores

- Medir **la fuerza relativa** del flujo comprador vs. vendedor

- Confirmar si el **impulso está perdiendo fuerza o ganando inercia**

  

---

  

### 📊 Nivel de relevancia

🔟 **7 / 10**

  

✅ Muy útil para detectar **cambios sutiles en la presión agresiva**

✅ Compatible con otras herramientas como footprint o delta acumulado

⛔ Puede parecer contradictorio en barras de poco rango o mixtas

⛔ Sensible a movimientos espasmódicos si se usa sin suavizado

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Confirmación de ruptura**: ratio positivo creciente + precio saliendo de rango

- **Detección de agotamiento**: ratio cae o se vuelve negativo tras impulso

- **Filtrar rupturas falsas** con divergencia entre precio y ratio

- **Validar absorciones**: ratio opuesto a la dirección aparente del precio

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **CalcMode**: `AskBid`

- **Period**: `10`

- **MaType**: `Ema`

- **UpperColor**: verde brillante

- **UpColor**: verde oscuro

- **LowerColor**: rojo brillante

- **LowColor**: rojo oscuro

  

✅ Buen equilibrio entre sensibilidad y estabilidad

✅ Muestra visualmente si hay pérdida de fuerza compradora o vendedora

⛔ Evitar en zonas de consolidación donde el volumen se equilibra

  

---

  

### 🧪 Notas de desarrollo

  

- Calcula el ratio: `(Ask - Bid) / (Ask + Bid) * 100` o su inverso

- El resultado se suaviza según el tipo de media móvil seleccionada

- El color del histograma depende del valor y su pendiente respecto al valor anterior

- Usa internamente instancias de indicadores como `EMA`, `SMA`, `WMA`, `SMMA`, etc.

- No usa `ValueDataSeries` adicionales ni lógica de acumulación, solo ratio puntual

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir una **línea base cero** para referencias más visuales

- Incluir opción de **alerta cuando el ratio cruce un umbral**

- Soporte para **coloración de fondo o etiquetas de valor**

- Opción de visualizar también **Bid y Ask absolutos** para análisis más completo
<!--stackedit_data:
eyJoaXN0b3J5IjpbNDY1NDEwOTE0XX0=
-->