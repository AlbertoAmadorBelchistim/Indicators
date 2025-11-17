## 🟦 Volume Per Trade (8 / 10)  
**Nombre del archivo:** `VolumePerTrade.cs`  
**Nombre del indicador:** Volume Per Trade  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000619357](https://help.atas.net/support/solutions/articles/72000619357)

---

### ⚙️ Parámetros configurables  
- Este indicador **no tiene parámetros configurables desde la UI**

---

### 🧭 Clasificación  
📂 Volume — Relación entre volumen total y número de transacciones por vela

---

### 🧠 Uso más frecuente  
- Medir el **tamaño medio de las transacciones** en cada vela  
- Detectar presencia de **ordenes grandes (institucionales)** frente a microoperaciones  
- Usar como filtro para diferenciar entre **actividad minorista** y profesional

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Útil para evaluar si el volumen está formado por muchas pequeñas operaciones o pocas grandes  
✅ Indicador directo y fácil de interpretar visualmente  
⛔ No muestra detalles intrabar ni distingue Bid/Ask

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de absorción**: Alto volumen con bajo número de trades = posible presencia institucional  
- **Filtro de entrada**: Evitar operar cuando el ratio indica dispersión (muchos microtrades sin dirección clara)  
- **Detección de agresión real**: Aumentos sostenidos del ratio = concentración de lotes

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- No tiene parámetros configurables  
- Se recomienda usarlo en combinación con delta, clúster o volumen agresivo

✅ Complemento ideal para leer **calidad del volumen**  
✅ Puede alertar sobre **cambios de comportamiento del tape**  
⛔ Requiere interpretación contextual, no ofrece señales por sí mismo

---

### 🧪 Notas de desarrollo  
- Calcula `Volume / Ticks` en cada vela y lo representa como **histograma**  
- Usa `VisualMode.Histogram` con `ResetAlertsOnNewBar` activo  
- Funciona en el **panel principal**, sin permitir cambio de ubicación

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No valida si `Ticks == 0` → **riesgo de división por cero** en velas huecas  
- No ofrece posibilidad de establecer alertas o niveles de umbral  
- Carece de opciones visuales: **color, grosor, suavizado**

---

### 🛠️ Propuestas de mejora  
- Añadir protección contra división por cero (Ticks = 0)  
- Incluir media móvil o suavizado del valor para facilitar lectura  
- Permitir configuración visual y establecer alertas al superar ciertos valores
