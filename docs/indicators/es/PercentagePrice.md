## 🟦 Percentage Price Oscillator (PPO) (7/10)

**Nombre del archivo:** `PercentagePrice.cs`  
**Nombre del indicador:** Percentage Price Oscillator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602445](https://help.atas.net/support/solutions/articles/72000602445)

---

### ⚙️ Parámetros configurables

- **ShortPeriod**: Periodo para la EMA corta (por defecto: 5)  
- **LongPeriod**: Periodo para la EMA larga (por defecto: 20)

---

### 🧭 Clasificación
📂 Momentum — Oscilador de impulso basado en diferencia porcentual de dos EMAs

---

### 🧠 Uso más frecuente

- Medir la **fuerza del impulso** relativo en porcentaje  
- Confirmar rupturas o cambios de dirección en función del sesgo porcentual  
- Detectar condiciones de **divergencia o aceleración** frente al precio

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Proporciona información de impulso normalizada (porcentaje)  
✅ Similar al MACD pero comparando en términos relativos  
⛔ Requiere calibración para evitar señales erróneas en activos muy volátiles

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de dirección** cuando el PPO cruza cero con pendiente clara  
- **Entrada por divergencia** si el PPO muestra desaceleración mientras el precio sigue  
- **Filtro direccional** en sistemas que requieren confirmación por impulso porcentual

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ShortPeriod**: `6`  
- **LongPeriod**: `21`

✅ Sensible al impulso sin exceso de ruido  
✅ Detecta bien aceleraciones en ruptura o test de nivel  
⛔ No se recomienda con periodos muy bajos o muy cercanos entre sí

---

### 🧪 Notas de desarrollo

- Calcula dos EMAs (corta y larga) y obtiene la diferencia relativa:  
  `PPO = 100 * (EMA corta − EMA larga) / EMA larga`  
- Si el valor de la EMA larga es cero, reutiliza el valor anterior  
- El resultado se guarda en una única `ValueDataSeries` (`_renderSeries`)  
- Se representa como línea en un panel separado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se valida si `ShortPeriod ≥ LongPeriod`, lo que puede llevar a lecturas erróneas  
- Si `EMA larga == 0`, el valor se mantiene con `bar - 1`, sin advertencia → puede ocultar error persistente  
- No incluye línea de señal ni histograma, lo que lo limita frente a MACD clásico  
- No ofrece alertas visuales o sonoras al cruce con cero o cambios de dirección  
- No se puede elegir el tipo de media (solo EMA)

---

### 🛠️ Propuestas de mejora

- Validar que `LongPeriod > ShortPeriod` o al menos advertir si no se cumple  
- Añadir línea de señal (EMA del PPO) y opción de histograma  
- Implementar alertas por cruce con cero o por cambio de signo  
- Permitir seleccionar tipo de media (`SMA`, `EMA`, `WMA`)  
- Colorear la línea según la dirección o pendiente para facilitar lectura visual

