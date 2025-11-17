## 🟦 Moving Average Difference (6/10)

**Nombre del archivo:** `MaDifference.cs`  
**Nombre del indicador:** Moving Average Difference  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602289](https://help.atas.net/support/solutions/articles/72000602289)

---

### ⚙️ Parámetros configurables

- **Period1**: Periodo de la primera media móvil simple (por defecto: 10)  
- **Period2**: Periodo de la segunda media móvil simple (por defecto: 20)  
- **PosColor / NegColor**: Colores para valores crecientes o decrecientes del histograma

---

### 🧭 Clasificación
📂 Momentum — Diferencia entre dos medias móviles simples

---

### 🧠 Uso más frecuente

- Identificar el **impulso relativo** entre dos medias móviles  
- Evaluar la **aceleración o desaceleración** del precio según el histograma  
- Confirmar cruces de medias móviles con visualización continua

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Muy útil como filtro visual de momentum  
✅ Simple de interpretar y personalizable visualmente  
⛔ No tiene señal explícita de entrada/salida, solo contexto

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de entrada**: entrada solo si el histograma es creciente  
- **Detección de giros**: cuando el color del histograma cambia  
- **Filtro de tendencia**: operar solo en la dirección de la diferencia dominante

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period1**: `8`  
- **Period2**: `21`  
- **Colores**: Verde (positivo), Rojo (negativo)

✅ Proporciona indicación clara de la presión direccional  
✅ Compatible con otros indicadores de entrada como MACD o RSI  
⛔ Requiere confirmación adicional (estructura, volumen)

---

### 🧪 Notas de desarrollo

- Calcula la diferencia entre dos `SMA` y la representa como histograma  
- Colorea cada barra según si es mayor o menor que la anterior  
- Usa una única `ValueDataSeries` con `VisualMode.Histogram`  
- El color del histograma se actualiza dinámicamente en tiempo real

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El color se define según si el valor actual es mayor al anterior, **no según el signo de la diferencia** → puede ser confuso si la tendencia es negativa pero creciente  
- No se valida si `Period1 > Period2` o viceversa; esto puede alterar la interpretación si se invierten sin querer  
- No se permite seleccionar el tipo de media móvil (solo SMA)  
- El histograma se borra en `bar == 0`, lo cual puede provocar pérdidas de datos si se recalcula parcialmente  
- No se incluyen alertas o condiciones lógicas basadas en el cruce cero

---

### 🛠️ Propuestas de mejora

- Añadir opción de colorear según signo (`> 0` verde, `< 0` rojo) o cambio de pendiente (actual)  
- Validar coherencia entre periodos (`Period1 < Period2` recomendado por convención)  
- Permitir elegir el tipo de media (SMA, EMA, SMMA...)  
- Añadir alertas visuales al cruce de cero o al cambio de color  
- Evitar limpieza completa del histograma al recalcular en `bar == 0`

