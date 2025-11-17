## 🟦 Inertia V2 (7/10)

**Nombre del archivo:** `Inertia2.cs`  
**Nombre del indicador:** Inertia V2  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602405](https://help.atas.net/support/solutions/articles/72000602405)

---

### ⚙️ Parámetros configurables

- **RviPeriod**: Periodo del cálculo suavizado entre subidas y bajadas (por defecto: 10)  
- **LinearRegPeriod**: Periodo de regresión lineal aplicada al valor resultante (por defecto: 14)  
- **StdDevPeriod**: Periodo de desviación estándar aplicada al cierre (por defecto: 10)

---

### 🧭 Clasificación  
📂 Momentum — Osciladores de impulso estructurado mediante regresión y volatilidad

---

### 🧠 Uso más frecuente

- Medir el **momentum suavizado con base estadística**  
- Confirmar la persistencia direccional utilizando la desviación estándar y regresión  
- Filtrar señales erráticas de impulso combinando volatilidad y dirección

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Más robusto frente a ruido que la versión clásica  
✅ Integra regresión y desviación estándar en un único oscilador  
⛔ No es fácilmente interpretable sin conocer su construcción  
⛔ El valor absoluto no tiene un rango definido (no es como un RSI o MACD)

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada con pendiente creciente** del indicador tras lateralidad  
- **Confirmación de impulso** si el valor supera un máximo reciente con aceleración  
- **Filtro de rango**: evitar operar si el valor se estabiliza o disminuye

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **RviPeriod**: `10`  
- **StdDevPeriod**: `10`  
- **LinearRegPeriod**: `14`  
- Dibujar en panel separado  
- Líneas guía opcionales para detectar aceleración

✅ Ayuda a discernir fases activas vs pasivas  
✅ Compatible con confirmación de agresión o ruptura

---

### 🧪 Notas de desarrollo

- Se basa en la **desviación estándar del cierre** como medida de volatilidad  
- Usa lógica tipo RVI para distinguir entre cierres al alza y a la baja:  
  - Si el cierre actual > anterior → `stdUp += σ`  
  - Si cierre actual < anterior → `stdDown += σ`
- Aplica una media móvil exponencial a `stdUp` y `stdDown`  
- Calcula un índice tipo RVI normalizado:
  \[
  RVIX = \frac{100 \cdot \text{stdUp}}{\text{stdUp} + \text{stdDown}}
  \]
- El resultado final es el valor suavizado mediante regresión lineal sobre `RVIX`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El valor de `RVIX` está limitado entre 0 y 100 pero no se representa como tal (no hay guía visual)  
- La regresión lineal sobre `RVIX` puede generar confusión al usuario que espera un rango fijo  
- No hay opción de visualizar `RVIX` ni las series `stdUp` / `stdDown` por separado

---

### 🛠️ Propuestas de mejora

- Exponer `RVIX` como serie secundaria para interpretación directa  
- Añadir líneas guía fijas (40, 50, 60) para representar fases neutras y extremas  
- Permitir elegir entre `EMA`, `WMA` o `Regresión` como método de suavizado final  
- Incluir alertas visuales o sonoras si la pendiente cambia de signo o se supera un umbral
