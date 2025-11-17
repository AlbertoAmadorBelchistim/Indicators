## 🟦 Up/Down Volume Ratio (9/10)  
**Nombre del archivo:** `UpDownVolumeRatio.cs`  
**Nombre del indicador:** Up/Down Volume Ratio  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000619242](https://help.atas.net/support/solutions/articles/72000619242)

---

### ⚙️ Parámetros configurables  
- **CalcMode**: Modo de cálculo: `UpDownVolume` (por defecto) o `AskBidVolume`  
- **Period**: Periodo de suavizado (por defecto: `10`)  
- **MovType**: Tipo de media móvil a aplicar (SMA, EMA, LinReg, WMA, WWMA, SZMA, SMMA)  
- **HistogramColor**: Color del histograma

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Comparación entre volumen de compra y de venta con suavizado

---

### 🧠 Uso más frecuente  
- Medir la **intensidad relativa de la presión compradora vs vendedora**  
- Observar si el **volumen acompaña la dirección del precio** o hay divergencias  
- Detectar **cambios de dominancia** entre compradores y vendedores con suavizado configurable

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  
✅ Excelente para confirmar si el **movimiento del precio está respaldado por el volumen**  
✅ Muy útil para detectar **desequilibrios progresivos** con media móvil personalizada  
⛔ Puede ser difícil de interpretar si se elige un **modo de cálculo no coherente con el activo**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de ruptura**: Validar rupturas si el ratio se desplaza de forma sostenida hacia un lado  
- **Divergencias**: Si el precio sube pero el volumen ratio baja, posible agotamiento  
- **Filtrado de señales**: Evitar entradas cuando el ratio indica **equilibrio o contracción**

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **CalcMode**: `AskBidVolume`  
- **Period**: `10`  
- **MovType**: `EMA`  
- **HistogramColor**: `Blue`

✅ Ideal para ver **presión agresiva** acumulada en clústeres  
✅ Permite ajustar el suavizado al contexto de mercado  
⛔ Exige seleccionar correctamente entre `UpDownVolume` y `AskBidVolume` según tipo de activo

---

### 🧪 Notas de desarrollo  
- Cálculo del ratio:
  - `UpDownVolume`: compara volúmenes según si la vela fue alcista o bajista  
  - `AskBidVolume`: compara directamente volumen agresivo Ask vs Bid  
- Se suaviza con diferentes tipos de medias móviles según el usuario configure  
- Se muestra como un histograma en porcentaje (de -100 a 100)

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- Puede **devolver cero** en velas sin volumen (riesgo de distorsión visual)  
- No permite configurar umbrales de alerta ni líneas de referencia  
- No muestra valores brutos subyacentes (solo el ratio final suavizado)

---

### 🛠️ Propuestas de mejora  
- Añadir líneas de referencia fijas (por ejemplo, ±30, ±60) para facilitar interpretación  
- Implementar **alertas visuales/sonoras** cuando el ratio supere cierto umbral  
- Añadir opción para mostrar **volumen neto Ask - Bid** junto al ratio
