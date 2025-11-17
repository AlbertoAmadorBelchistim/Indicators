## 🟦 Momentum Trend (3/10)

**Nombre del archivo:** `MomentumTrend.cs`  
**Nombre del indicador:** Momentum Trend  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602636](https://help.atas.net/support/solutions/articles/72000602636)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para calcular el impulso base (`Momentum`) (por defecto: 10)

---

### 🧭 Clasificación
📂 Momentum — Visualización del cambio en impulso con puntos sobre máximos y mínimos

---

### 🧠 Uso más frecuente

- Detectar **aceleración o desaceleración** del impulso  
- Confirmar movimientos direccionales cuando el impulso se incrementa  
- Señal visual clara sin necesidad de panel adicional

---

### 📊 Nivel de relevancia
🔟 **3 / 10**

✅ Visualmente limpio y útil como complemento de contexto  
✅ Permite identificar momentos de aceleración con alta claridad  
⛔ No ofrece valor cuantitativo ni dirección del movimiento (solo crecimiento/disminución)

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación visual de tendencia**: puntos verdes en máximos indican impulso creciente  
- **Evitar entradas contrarias** si hay aceleración en contra  
- **Soporte para overlays**: superposición sobre otros indicadores o velas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `9`

✅ Detecta impulsos crecientes sobre velas de 1 minuto  
✅ Compatible con análisis estructural y de flujo  
⛔ Puede no distinguir entre impulso por ruptura o por absorción

---

### 🧪 Notas de desarrollo

- Utiliza internamente el indicador `Momentum` para medir el impulso  
- Dibuja un punto verde en el **High** si el impulso sube respecto a la vela anterior  
- Dibuja un punto rojo en el **Low** si el impulso disminuye  
- No se muestra en panel separado (`DenyToChangePanel = true`)  
- Usa `VisualMode.Dots` con tamaño (`Width = 3`) para los puntos

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se controla el caso donde `Momentum[bar] == Momentum[bar - 1]` → no se dibuja nada, lo que puede confundir  
- La lógica de colores y niveles es fija: no se puede personalizar desde la UI  
- No hay validación ni tolerancia al ruido (ej: movimientos pequeños de 1 tick también cuentan)  
- El objeto `Momentum` interno recalcula en cada `OnCalculate` sin usar `ValueDataSeries`, lo que impide ver su valor  
- No permite mostrar líneas o bandas, solo puntos

---

### 🛠️ Propuestas de mejora

- Permitir configurar colores y estilo de los puntos desde la UI  
- Añadir umbral mínimo de cambio en el impulso para evitar señales con ruido  
- Incluir una opción para mostrar el valor numérico del impulso si se desea  
- Ofrecer una visualización alternativa con línea base o histograma  
- Mostrar etiqueta o tooltip al pasar el cursor sobre los puntos

