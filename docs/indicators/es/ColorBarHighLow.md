## 🟦 Color Bar HH/LL (6/10)

**Nombre del archivo:** `ColorBarHighLow.cs`  
**Nombre del indicador:** Color Bar HH/LL  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000618502](https://help.atas.net/support/solutions/articles/72000618502)

---

### ⚙️ Parámetros configurables

- **AverageColor**: Color aplicado cuando hay una ruptura simultánea del máximo y mínimo anteriores  
- **HighColor**: Color aplicado cuando hay un nuevo máximo sin nuevo mínimo  
- **LowColor**: Color aplicado cuando hay un nuevo mínimo sin nuevo máximo

---

### 🧭 Clasificación  
📂 Trend — Indicador visual de detección de nuevos extremos en la vela

---

### 🧠 Uso más frecuente

- Identificar **cambios estructurales** en la acción del precio (HH/LL, HL/LH)
- Resaltar visualmente las **velas significativas** en la evolución de máximos y mínimos
- Ayudar en la lectura rápida de **momentos de transición de tendencia**

---

### 📊 Nivel de relevancia  
🔟 **6 / 10**  

✅ Útil como apoyo visual para detectar velas con rupturas significativas  
✅ Fácil de interpretar en movimientos direccionales claros  
⛔ No aporta información de volumen ni contexto adicional  
⛔ Su interpretación depende de la calidad del marco temporal

---

### 🎯 Estrategias de scalping donde se aplica

- **Swing Candle Confirmation**: confirmar entrada tras vela que marca HH/LL
- **Reversión**: detectar agotamiento tras nuevo HH sin continuación
- **Rotura tendencial**: validar entrada cuando se marca HH y HL consecutivos

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **AverageColor**: Naranja o amarillo (neutral)
- **HighColor**: Azul o verde (alcista)
- **LowColor**: Magenta o rojo (bajista)

✅ Configuración cromática clara y coherente con dirección  
✅ Aumenta la rapidez en lectura de acción del precio

---

### 🧪 Notas de desarrollo

- El indicador compara cada vela con la anterior para determinar si hay:
  - Nuevo **Higher High** (HH)
  - Nuevo **Lower Low** (LL)
  - Ambos simultáneamente (HH + LL)
- Usa un `PaintbarsDataSeries` para colorear las velas según el tipo de ruptura
- No tiene dependencia de indicadores adicionales ni del volumen

---

### 🛠️ Propuestas de mejora

- Añadir opción para destacar también **Higher Low** (HL) y **Lower High** (LH)
- Incluir lógica opcional para **comparar con más de una vela anterior** (lookback)
- Permitir que el color se aplique solo a **velas de cuerpo mayor a X ticks**
- Mostrar etiquetas con el tipo de ruptura detectada en cada vela