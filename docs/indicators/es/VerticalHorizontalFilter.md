## 🟦 Vertical Horizontal Filter (8/10)  
**Nombre del archivo:** `VerticalHorizontalFilter.cs`  
**Nombre del indicador:** Vertical Horizontal Filter  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000619282](https://help.atas.net/support/solutions/articles/72000619282)

---

### ⚙️ Parámetros configurables  
- **Period**: Número de barras para el cálculo del filtro (por defecto: `10`)  
- **Type**: Tipo de dato de entrada a utilizar (`Volume`, `Ticks`, `Asks`, `Bids`, `Open`, `High`, `Low`, `Close`, `OHLCAverage`, `HLCAverage`, `HLAverage`)  
- **HistogramColor**: Color del histograma

---

### 🧭 Clasificación  
📂 Statistical — Indicador de tendencia vs lateralidad basado en relación vertical/horizontal

---

### 🧠 Uso más frecuente  
- Medir si el mercado se encuentra en una **fase tendencial (direccional)** o **lateral (consolidación)**  
- Confirmar si existe un **movimiento significativo o solo fluctuación aleatoria**  
- Ayudar a seleccionar **estrategias de breakout vs rango** dependiendo del valor del filtro

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Útil para filtrar **mercados en rango vs direccionales**  
✅ Compatible con múltiples fuentes (volumen, precio, bid/ask…)  
⛔ Requiere interpretación y calibración según el activo y timeframe

---

### 🎯 Estrategias de scalping donde se aplica  
- **Activación de estrategias de ruptura** solo si el filtro supera cierto umbral  
- **Evitación de falsas señales** cuando el mercado no presenta movimiento real  
- **Confirmación de fases impulsivas** antes de operar en la dirección del flujo

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Period**: `10`  
- **Type**: `Volume` o `Close`  
- **HistogramColor**: `Blue`

✅ Permite distinguir si un movimiento tiene verdadera intención  
✅ Excelente filtro previo a operar setups en consolidación  
⛔ No emite señales directas de entrada ni salida

---

### 🧪 Notas de desarrollo  
- Calcula la relación entre el **rango vertical** (máximo - mínimo) y la **suma de movimientos absolutos** durante el periodo  
- Cuanto mayor es el valor, más direccional es el movimiento  
- Puede aplicarse a **volumen, precio, o cualquier fuente** configurable

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No incluye líneas de umbral que ayuden a interpretar cuándo el valor es "alto" o "bajo"  
- No ofrece **alertas automáticas** ni interpretación visual integrada  
- La interpretación cambia según el tipo de entrada (volumen vs precio)

---

### 🛠️ Propuestas de mejora  
- Añadir **líneas horizontales de referencia** para facilitar lectura rápida (ej: nivel 0.5 como neutralidad)  
- Permitir definir **alertas visuales o sonoras** al cruzar cierto umbral  
- Incluir una **leyenda contextual** que indique si el mercado está en fase lateral o direccional
