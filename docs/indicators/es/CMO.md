## 🟦 Chaikin Money Oscillator (CMO) (7/10)

**Nombre del archivo:** `CMO.cs`  
**Nombre del indicador:** Chaikin Money Oscillator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602299](https://help.atas.net/support/solutions/articles/72000602299)

---

### ⚙️ Parámetros configurables

- **PeriodLong**: Periodo largo para la EMA del flujo de dinero (por defecto: 10)  
- **PeriodShort**: Periodo corto para la EMA del flujo de dinero (por defecto: 3)

---

### 🧭 Clasificación  
📂 Volume — Osciladores basados en acumulación/distribución de volumen

---

### 🧠 Uso más frecuente

- Medir la **aceleración o desaceleración del flujo monetario**  
- Detectar **momentos de impulso creciente** con volumen confirmado  
- Confirmar o filtrar señales de entrada según la pendiente del oscilador  
- Identificar divergencias entre precio y volumen acumulado

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Más sensible que el CMF al usar diferencias de medias exponenciales  
✅ Útil para confirmar fuerza en rupturas o retrocesos  
⛔ Puede generar ruido en marcos de tiempo muy cortos  
⛔ Sensible a spikes de volumen anómalos

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada con momentum confirmado**: operar solo si el CMO es creciente  
- **Filtro de rupturas falsas**: evitar trades si el CMO cae mientras el precio sube  
- **Divergencias con volumen**: aprovechar momentos en que el oscilador no acompaña al precio

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **PeriodLong**: `8`  
- **PeriodShort**: `3`

✅ Aumenta la reactividad sin perder estabilidad  
✅ Compatible con análisis de impulso sobre microestructuras

---

### 🧪 Notas de desarrollo

- Calcula un **flujo monetario diario (AD diario)** en base al rango y al volumen de cada vela:

$$
AD = \left( \frac{( \text{Close} - \text{Low} ) - ( \text{High} - \text{Close} )}{\text{High} - \text{Low}} \right) \times \text{Volume}
$$

- A diferencia del Chaikin Oscillator clásico, que aplica EMAs sobre una ADL acumulada histórica, este indicador **reinicia el AD cada sesión y suaviza su valor diario**
- Aplica dos **EMAs** sobre el AD: una larga y una corta  
- La diferencia entre ambas constituye el valor del oscilador (`emaLong - emaShort`)  

---

### 🛠️ Propuestas de mejora

- Permitir seleccionar el tipo de media (EMA, SMA, WMA) para mayor flexibilidad  
- Incluir una **línea base en cero** como referencia visual clara  
- Añadir opción para pintar zonas positivas/negativas con color diferenciado  
- Implementar alertas visuales al cruzar el nivel cero o cambiar de pendiente