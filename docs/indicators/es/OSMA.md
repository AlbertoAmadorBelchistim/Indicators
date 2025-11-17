## 🟦 OSMA (Moving Average of Oscillator) (7/10)

**Nombre del archivo:** `OSMA.cs`  
**Nombre del indicador:** OSMA (Moving Average of Oscillator)  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602432](https://help.atas.net/support/solutions/articles/72000602432)

---

### ⚙️ Parámetros configurables

- **ShortPeriod**: Periodo de la media exponencial rápida (por defecto: 9)  
- **LongPeriod**: Periodo de la media exponencial lenta (por defecto: 12)  
- **SignalPeriod**: Periodo de la media de la línea MACD (por defecto: 26)

---

### 🧭 Clasificación
📂 Momentum — Oscilador derivado del MACD con suavizado adicional

---

### 🧠 Uso más frecuente

- Detectar **cambios de momentum** en el precio  
- Confirmar **tendencias** o anticipar giros mediante el cruce con la línea cero  
- Evaluar la diferencia entre impulso y su media suavizada

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Suaviza el MACD y reduce ruido para confirmar señales  
✅ Relevante para identificar la fuerza de movimientos continuados  
⛔ Puede retrasar las señales respecto a indicadores más reactivos

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de dirección**: operar a favor del histograma creciente/decreciente  
- **Cruce con cero** como señal de entrada/salida  
- **Divergencia entre precio y OSMA** para anticipar reversión

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ShortPeriod**: `6`  
- **LongPeriod**: `19`  
- **SignalPeriod**: `5`

✅ Alta sensibilidad manteniendo estructura del MACD  
✅ Buena representación de cambios de impulso en 1M  
⛔ Puede requerir filtro adicional en entornos laterales

---

### 🧪 Notas de desarrollo

- Calcula `MACD = EMA(Short) - EMA(Long)`  
- Luego aplica una `SMA` al MACD para suavizarlo  
- El valor de OSMA es la diferencia entre MACD y su SMA:  
  `OSMA = MACD - Signal`  
- Representado como histograma (`VisualMode.Histogram`) en un nuevo panel  
- Compatible con modo minimizado para optimizar visualización

---

### ❗ Incoherencias o aspectos mejorables detectadas

- Utiliza `SMA` como línea de señal, mientras que el MACD clásico suele usar `EMA`, lo cual puede cambiar la dinámica  
- No permite al usuario elegir el tipo de media para el suavizado (solo `SMA`)  
- El histograma no tiene coloración dinámica según signo o pendiente  
- No se valida si los periodos están muy próximos (por ejemplo, `Short = 9`, `Long = 10`) → puede generar señales poco útiles  
- No incluye alertas ni cruces ni ofrece niveles de referencia

---

### 🛠️ Propuestas de mejora

- Añadir opción para seleccionar el tipo de media para `SignalPeriod` (`SMA`, `EMA`, etc.)  
- Incluir coloración del histograma según si el valor es positivo o negativo  
- Agregar alertas visuales/sonoras al cruce con cero o cambio de signo  
- Permitir visualización de las líneas de MACD y Signal para comparación directa  
- Validar que la diferencia entre `LongPeriod` y `ShortPeriod` sea suficiente para obtener señales significativas

