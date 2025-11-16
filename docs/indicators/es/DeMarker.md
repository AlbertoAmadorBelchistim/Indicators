## 🟦 DeMarker (6/10)

**Nombre del archivo:** `DeMarker.cs`  
**Nombre del indicador:** DeMarker  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602365](https://help.atas.net/support/solutions/articles/72000602365)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras utilizadas en el cálculo de las medias móviles (por defecto: 10)

---

### 🧭 Clasificación  
📂 Momentum — Osciladores basados en comparación intrabarra

---

### 🧠 Uso más frecuente

- Detectar **zonas de sobrecompra y sobreventa**  
- Medir la **intensidad del impulso** comparando máximos y mínimos recientes  
- Generar señales de reversión cuando el valor se aproxima a extremos (ej. 0.7 o 0.3)

---

### 📊 Nivel de relevancia  
🔟 **6 / 10**

✅ Ligero y fácil de interpretar  
✅ Utilizado como complemento de RSI o Stochastics  
⛔ Menos reactivo que otros osciladores en movimientos abruptos  
⛔ No incluye volumen ni delta, solo precios

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada en reversión**: cuando el DeMarker cae por debajo de 0.3 y luego sube  
- **Salida parcial**: cuando el DeMarker supera 0.7 en una posición ganadora  
- **Confirmación**: usar junto a otros indicadores como filtro de sobrecompra/sobreventa

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `7` o `10`  
- Líneas auxiliares en `0.3` (sobreventa) y `0.7` (sobrecompra)  
- Combinar con volumen o delta para validar giros

✅ Funciona bien como filtro visual  
✅ Complementa setups basados en impulso

---

### 🧪 Notas de desarrollo

- Calcula dos series internas:
  - **deMax** = `High actual - High anterior` si positivo, sino 0  
  - **deMin** = `Low anterior - Low actual` si positivo, sino 0  
- Se aplican medias móviles simples (`SMA`) sobre cada serie  
- Fórmula final:
  $$
  \text{DeMarker} = \frac{\text{SMA}_{\text{deMax}}}{\text{SMA}_{\text{deMax}} + \text{SMA}_{\text{deMin}}}
  $$
- Si el denominador es cero, se conserva el valor anterior (`bar - 1`)

---

### ❗ Incoherencias o aspectos mejorables detectados

- En la línea:
  `var deMin = Math.Min(0, prevCandle.Low - candle.Low);`  
  El uso de `Math.Min` da siempre un número **negativo o cero**, pero en la fórmula debería acumularse como **positivo**. Esto **invalida la lógica esperada**, ya que el denominador puede volverse 0 o erróneamente negativo.

- Al conservar el valor anterior en caso de división por cero, no se informa visualmente de que el cálculo no es válido

---

### 🛠️ Propuestas de mejora

- Corregir la línea de `deMin` por:
  `var deMin = Math.Max(0, prevCandle.Low - candle.Low);`  
- Añadir líneas horizontales auxiliares (`0.3`, `0.5`, `0.7`) para facilitar interpretación  
- Incluir alerta o mensaje cuando el cálculo se salta por falta de datos válidos  
- Ofrecer opción de media exponencial (EMA) en lugar de SMA para mayor reactividad