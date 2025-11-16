## 🟦 Directional Movement Oscillator (7/10)

**Nombre del archivo:** `DmOscillator.cs`  
**Nombre del indicador:** Directional Movement Oscillator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602371](https://help.atas.net/support/solutions/articles/72000602371)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras usadas para suavizar DI+ y DI- (por defecto: 14)

---

### 🧭 Clasificación  
📂 Trend — Osciladores de dirección de movimiento

---

### 🧠 Uso más frecuente

- Medir la **diferencia neta entre DI+ y DI-**  
- Confirmar si la dirección dominante es compradora o vendedora  
- Detectar posibles **cambios de dirección** mediante cruces por el eje cero

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Muy visual e intuitivo para evaluar tendencia dominante  
✅ Útil como oscilador confirmatorio o filtro  
⛔ No tiene lógica de señal por sí solo (requiere interpretación)  
⛔ No incluye información sobre la fuerza (ADX)

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce por encima de cero**: posible inicio de tendencia alcista  
- **Cruce por debajo de cero**: posible inicio de tendencia bajista  
- **Confirmación de fuerza direccional**: operar solo si el valor absoluto crece

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10` a `14`  
- Visualización con línea azul o histograma  
- Añadir línea cero como referencia clara

✅ Ideal para detectar sesgo direccional  
✅ Rápido de interpretar en decisiones tácticas

---

### 🧪 Notas de desarrollo

- El indicador encapsula internamente un `DmIndex`, que calcula DI+ y DI-  
- Obtiene el valor del oscilador como:
  \[
  \text{DM Oscillator}_t = DI^+_t - DI^-_t
  \]
- Usa dos `ValueDataSeries` internas del `DmIndex`:
  - `DataSeries[0]`: DI+  
  - `DataSeries[1]`: DI-
- El valor final se guarda en `_renderSeries`

---

### ❗ Incoherencias o aspectos mejorables detectados

- El nombre `RenderSeries` no especifica claramente que representa la diferencia DI+ - DI-, lo que puede dificultar la interpretación técnica si no se consulta la documentación  
- El indicador depende de un `DmIndex` como subindicador, lo cual podría afectar el rendimiento si se instancia repetidamente  
- No hay control si alguno de los valores de `DataSeries` está vacío o fuera de índice

---

### 🛠️ Propuestas de mejora

- Renombrar `_renderSeries` como `DmOscSeries` o similar para mayor claridad  
- Añadir líneas auxiliares en cero y umbrales opcionales  
- Incluir visualización tipo histograma para mayor contraste visual  
- Permitir mostrar simultáneamente DI+, DI- y el oscilador en el mismo panel