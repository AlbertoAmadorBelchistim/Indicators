## 🟦 Z-Score (8 / 10)  
**Nombre del archivo:** `ZScore.cs`  
**Nombre del indicador:** Z-Score  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602269](https://help.atas.net/support/solutions/articles/72000602269)

---

### ⚙️ Parámetros configurables  
- **SmaPeriod**: Periodo de la media simple (por defecto: `10`)  
- **StdPeriod**: Periodo para la desviación estándar (por defecto: `10`)

---

### 🧭 Clasificación  
📂 Statistical — Indicador estadístico de desviación estándar respecto a la media

---

### 🧠 Uso más frecuente  
- Medir cuán **extremo** es el valor actual respecto a su promedio histórico  
- Detectar condiciones de **sobrecompra/sobreventa relativa**  
- Confirmar **anomalías o rupturas** basadas en la desviación típica del comportamiento reciente

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Útil como filtro para detectar **eventos estadísticamente significativos**  
✅ Ideal para sistemas basados en **reversión a la media** o detección de outliers  
⛔ Puede perder eficacia si la serie tiene cambios estructurales abruptos

---

### 🎯 Estrategias de scalping donde se aplica  
- **Reversión estadística**: Operar en contra del movimiento si el Z-Score supera ±2  
- **Confirmación de ruptura**: Validar que el precio se ha alejado significativamente de su media  
- **Filtro de contexto**: Solo operar cuando el valor no está dentro de ±1 (zona neutral)

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **SmaPeriod / StdPeriod**: `10`  
- Límites de relevancia: ±1 (zona neutra), ±2 (desviación significativa)

✅ Rápido y efectivo para detectar extremos  
✅ Compatible con estrategias de reversión, breakout o filtrado estadístico  
⛔ Requiere combinación con volumen, estructura o contexto para evitar señales erróneas

---

### 🧪 Notas de desarrollo  
- Calcula internamente una **SMA** y una **desviación estándar**  
- Fórmula: `Z = (ValorActual - Media) / DesviaciónTípica`  
- Si la desviación es 0 (riesgo de división), devuelve `0` para evitar errores  
- Se representa en un panel separado (`NewPanel`) como línea continua

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No hay control para evitar `SmaPeriod != StdPeriod` si se busca coherencia estadística  
- No incluye líneas horizontales guía en ±1, ±2, ±3  
- No se muestran etiquetas ni valores actuales directamente en el gráfico

---

### 🛠️ Propuestas de mejora  
- Añadir líneas guía opcionales (±1σ, ±2σ, ±3σ)  
- Incluir alertas visuales/sonoras si se supera un umbral  
- Permitir representar el Z-Score como histograma o área coloreada para facilitar la lectura
