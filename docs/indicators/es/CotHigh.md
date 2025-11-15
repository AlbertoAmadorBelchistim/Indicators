## 🟦 COT High/Low (7/10)

**Nombre del archivo:** `CotHigh.cs`  
**Nombre del indicador:** COT High/Low  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602603](https://help.atas.net/support/solutions/articles/72000602603)

---

### ⚙️ Parámetros configurables

- **Mode** (`High` / `Low`): Define si se buscan nuevos máximos o nuevos mínimos  
- **PosColor**: Color para barras con delta acumulado positivo (por defecto: verde)  
- **NegColor**: Color para barras con delta acumulado negativo (por defecto: rojo)

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Indicadores de acumulación de delta en extremos

---

### 🧠 Uso más frecuente

- Acumular el **delta neto** desde un nuevo **máximo o mínimo local**  
- Evaluar la **intensidad de la agresión compradora o vendedora** tras una expansión del rango  
- Detectar zonas donde hubo un nuevo high/low seguido de acumulación o absorción

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Ideal para validar si los nuevos extremos están respaldados por flujo agresivo  
✅ Compatible con lectura contextual de order flow y estructuras de precio  
⛔ Puede confundir si no se entiende la lógica de acumulación continua  
⛔ No reinicia automáticamente si el mercado se estanca

---

### 🎯 Estrategias de scalping donde se aplica

- **Ruptura válida**: buscar acumulación positiva en nuevos máximos o negativa en nuevos mínimos  
- **Falsos breakouts**: si el delta acumulado tras el nuevo high/low es bajo o contrario  
- **Absorción en extremos**: acumulación opuesta tras marcar un nuevo extremo

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Mode**: `High` si se buscan breakout alcistas, `Low` para entradas bajistas  
- **Colores**:  
  - `PosColor`: verde intenso  
  - `NegColor`: rojo oscuro  

✅ Muy útil en confluencia con footprint, POC, o zonas de reversión

---

### 🧪 Notas de desarrollo

- Usa un `ValueDataSeries` con visualización en histograma para mostrar acumulación del delta  
- Solo **resetea** el valor acumulado si se detecta un nuevo máximo o mínimo (según el `Mode`)  
- ❗ **Incoherencia lógica detectada**:
  - En `Mode = High`, también reinicia si el **mínimo** es mayor que el máximo anterior (`candle.Low >= _extValue`), lo cual **no es coherente**
  - En `Mode = Low`, se reinicia si `Low >= _extValue`, cuando **debería ser `Low <= _extValue`**
  - Esto puede provocar reinicios indebidos o acumulaciones incorrectas en tendencia

---

### 🛠️ Propuestas de mejora

- Corregir la lógica de comparación:
  - Para `Mode.High`: reiniciar solo si `candle.High > _extValue`
  - Para `Mode.Low`: reiniciar solo si `candle.Low < _extValue`
- Añadir parámetro para definir si se reinicia con **breakout limpio o con cierre confirmado**  
- Implementar validación visual del extremo alcanzado con etiquetas  
- Incluir documentación en la UI que explique **cuándo se reinicia** el contador de delta  