## 🟦 Relative Vigor Index (RVI) (6/10)

**Nombre del archivo:** `RelativeVigorIndex.cs`  
**Nombre del indicador:** Relative Vigor Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000619101](https://help.atas.net/support/solutions/articles/72000619101)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo para suavizar la línea de señal (por defecto: 10)  
- **SmaPeriod**: Periodo del suavizado principal del RVI (por defecto: 4)

---

### 🧭 Clasificación
📂 Momentum — Indicador de impulso basado en la relación entre cierre y rango de la vela

---

### 🧠 Uso más frecuente

- Confirmar la **dirección del impulso actual**  
- Detectar **cruces de señal** como puntos de entrada o salida  
- Evaluar la **intensidad del movimiento** en función de la pendiente

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Suavizado y visualmente estable en comparación con RSI o Momentum  
✅ Proporciona señales claras con su línea de señal  
⛔ Puede retrasarse en fases de alta volatilidad

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce con línea de señal** como disparador de entrada  
- **Confirmación direccional** si el RVI se mantiene por encima de la señal  
- **Reversión anticipada** si el RVI cambia de pendiente y cruza a la baja

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `6`  
- **SmaPeriod**: `3`

✅ Mayor sensibilidad sin exceso de ruido  
✅ Compatible con confirmaciones visuales rápidas  
⛔ No funciona bien solo, requiere confluencia con estructura o volumen

---

### 🧪 Notas de desarrollo

- Calcula:  
  `RVI = (Close − Open) / (High − Low)`  
- El valor se suaviza con una **SMA** definida por `SmaPeriod`  
- La línea de señal aplica una **SMA adicional** sobre el mismo valor base  
- Se usan dos `ValueDataSeries`: `_rviSeries` (verde) y `_signalSeries` (azul)  
- Los valores se actualizan en tiempo real con `OnCalculate()`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La línea de señal (`_signalSeries`) se calcula directamente sobre el mismo `rvi` en lugar del valor suavizado (`_rviSeries`) → puede perder lógica interpretativa  
- El valor inicial de `rvi` no está validado si `High == Low` en velas planas (aunque evita división por cero, no lo indica visualmente)  
- No hay coloración dinámica ni codificación por cruce o pendiente  
- No hay línea cero ni alertas visuales configurables  
- Ambas líneas se asignan a `DataSeries[0]` y `Add()`, lo que puede causar confusión en paneles

---

### 🛠️ Propuestas de mejora

- Aplicar la señal (`_smaSig`) sobre el resultado suavizado (`_rviSeries`) y no directamente sobre el `rvi`  
- Incluir línea base en cero como referencia visual  
- Añadir codificación de color según cruce o dirección  
- Permitir configuración visual de ambas líneas desde la UI  
- Incorporar alertas al cruce entre RVI y señal o cambios de dirección

