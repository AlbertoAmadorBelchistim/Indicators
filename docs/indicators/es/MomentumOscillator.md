## 🟦 Price Momentum Oscillator (7/10)

**Nombre del archivo:** `MomentumOscillator.cs`  
**Nombre del indicador:** Price Momentum Oscillator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602449](https://help.atas.net/support/solutions/articles/72000602449)

---

### ⚙️ Parámetros configurables

- **Period1**: Periodo para el cálculo de la tasa de cambio suavizada (por defecto: 10)  
- **Period2**: Periodo para el suavizado adicional sobre la señal (por defecto: 10)  
- **SignalPeriod**: Periodo del suavizado final con EMA (`_smoothSeries`, por defecto: 10)

---

### 🧭 Clasificación
📂 Momentum — Oscilador compuesto de tasas de cambio suavizadas

---

### 🧠 Uso más frecuente

- Detectar **cambios de impulso** mediante la pendiente de la línea  
- Confirmar entradas por **cruce de línea de señal**  
- Evaluar **fuerza relativa** de movimientos recientes

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Más suave que un Momentum clásico, mejor filtrado  
✅ Buen indicador de impulso con capacidad de personalización  
⛔ Su lógica compuesta puede ser difícil de interpretar sin conocer su estructura

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por cruce** de `_signalSeries` y `_smoothSeries`  
- **Confirmación de aceleración** cuando ambas líneas se alinean al alza o baja  
- **Filtro de tendencia suave** para evitar señales falsas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period1**: `6`  
- **Period2**: `3`  
- **SignalPeriod**: `5`

✅ Alta sensibilidad con suficiente filtrado  
✅ Compatible con estrategias de impulso y ruptura  
⛔ El cálculo acumulado puede introducir retrasos si se configuran periodos largos

---

### 🧪 Notas de desarrollo

- Calcula la **tasa de cambio relativa**:  
  `rate = 100 * (Close[t] - Close[t-1]) / Close[t-1]`  
- Aplica dos capas de suavizado (tipo EMA manual):  
  - Primero sobre `rate`  
  - Luego sobre `signalSeries`  
- Finalmente aplica un EMA adicional (`_smoothSeries`) con multiplicador ×10  
- Las dos series principales se visualizan como líneas

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay validación para evitar división por cero en `SourceDataSeries[bar - 1]`  
- El suavizado se hace manualmente con fórmula EMA pero no se documenta su naturaleza claramente  
- La serie `_rateSeries` no se muestra en pantalla ni puede activarse desde la UI  
- La multiplicación por 10 en `_smoothSeries` es arbitraria y no está documentada en la interfaz  
- No existe opción para mostrar cruce visual ni alertas por intersección de líneas

---

### 🛠️ Propuestas de mejora

- Añadir validación contra división por cero en barras con precio anterior igual a 0  
- Permitir visualizar también la serie `rate` como referencia  
- Documentar y permitir modificar el factor multiplicador (`×10`) en `_smoothSeries`  
- Añadir opción de alertas visuales o sonoras al cruce entre `_signalSeries` y `_smoothSeries`  
- Ofrecer un modo de visualización tipo histograma para el cruce de impulso

