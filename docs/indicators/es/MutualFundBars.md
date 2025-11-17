## 🟦 Mutual Fund Bars (4/10)

**Nombre del archivo:** `MutualFundBars.cs`  
**Nombre del indicador:** Mutual Fund Bars  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000619006](https://help.atas.net/support/solutions/articles/72000619006)

---

### ⚙️ Parámetros configurables

Este indicador **no tiene parámetros configurables desde la UI**.

---

### 🧭 Clasificación
📂 Visualization — Reconstrucción visual tipo barras para replicar comportamiento de fondos mutuos

---

### 🧠 Uso más frecuente

- Mostrar **barras suavizadas** en las que el precio abre donde cerró la vela anterior  
- Reproducir comportamiento de activos o fondos que solo muestran el valor al cierre  
- Visualizar movimientos eliminando el ruido intradía

---

### 📊 Nivel de relevancia
🔟 **4 / 10**

✅ Claridad visual mejorada para ciertos activos o estilos analíticos  
✅ Ideal para replicar fondos o activos sin oscilaciones intradía  
⛔ No aporta señal de entrada ni contexto operativo

---

### 🎯 Estrategias de scalping donde se aplica

⛔ No aplicable directamente a scalping.  
✅ Puede usarse como herramienta visual de comparación o en backtesting pasivo

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

No aplicable como herramienta principal en M1.  
✅ Posible uso como **referencia visual secundaria**

---

### 🧪 Notas de desarrollo

- Genera una **nueva serie de velas artificiales** (`_renderSeries`) donde:  
  - `Open = Close` de la vela anterior  
  - `High/Low = max/min` entre `Close` actual y anterior  
  - `Close = Close` actual  
- Usa `CandleDataSeries` y sobreescribe los colores por defecto en `OnApplyDefaultColors`  
- La serie original de velas (`_bars`) se oculta mediante `_transparent`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay control de errores si el `bar == 0`, más allá de limpiar la serie; podría añadirse protección más robusta  
- No permite personalizar los colores, grosores o bordes desde la UI  
- La lógica presupone que `GetCandle(bar - 1)` siempre está disponible (no válido en cargas parciales)  
- No existe opción para usar el `Open` real si se desea  
- No permite aplicar lógica condicional sobre el tipo de vela generada (ej. pintar solo si cambio relevante)

---

### 🛠️ Propuestas de mejora

- Añadir opción para seleccionar entre `Open real` y `Close previo` como apertura  
- Incluir parámetros para personalizar colores y estilo de las velas  
- Añadir validación explícita si `bar < 1` o si falta histórico  
- Mostrar tooltip o valores sobre la vela reconstruida  
- Permitir aplicar lógicas de filtrado (ej: pintar solo si el movimiento supera X ticks)

