## 🟦 1 Divided by Price (5.5/10)

**Nombre del archivo:** `DividedByPrice.cs`  
**Nombre del indicador:** 1 Divided by Price  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602309](https://help.atas.net/support/solutions/articles/72000602309)

---

### ⚙️ Parámetros configurables

Este indicador **no tiene parámetros configurables** desde la interfaz.

---

### 🧭 Clasificación  
📂 Price — Indicadores que transforman el precio en nuevas unidades

---

### 🧠 Uso más frecuente

- Visualizar el **inverso del precio**: `1 / Open`, `1 / Close`, etc.  
- Utilizarlo como parte de análisis algorítmico o comparativo entre activos  
- Detectar **relaciones armónicas o proporcionales** mediante simetría de precios

---

### 📊 Nivel de relevancia  
🔟 **5.5 / 10**

✅ Permite análisis transformado poco común  
✅ Puede servir en estudios comparativos o matemáticos específicos  
⛔ Poco intuitivo visualmente, no útil para la mayoría de traders discrecionales  
⛔ No representa un indicador técnico clásico

---

### 🎯 Estrategias de scalping donde se aplica

**No recomendado directamente para scalping.**  
Puede utilizarse en backtests de modelos matemáticos o indicadores personalizados que empleen precios invertidos.

---

### ⚙️ Funcionamiento interno

- Para cada vela, calcula:
  - `Open = 1 / candle.Open`  
  - `Close = 1 / candle.Close`  
  - `High = 1 / candle.Low`  
  - `Low = 1 / candle.High`  
- El máximo y mínimo se invierten para **preservar la forma de vela correcta**
- El resultado se guarda en una `CandleDataSeries` (`_reversedCandles`)  
- Hereda los colores del gráfico base mediante `OnApplyDefaultColors()`

---

### 🧪 Notas de desarrollo

- El objetivo es **mostrar una versión invertida del gráfico**, útil en estudios armónicos, transformaciones matemáticas o backtests de simetrías  
- El orden de `High` y `Low` se intercambia al invertir los valores para evitar que el `High` sea menor que el `Low`  
- Puede mostrar formas visuales útiles para traders que trabajen con teoría armónica o ciclos invertidos

---

### ❗ Incoherencias o aspectos mejorables detectados

- **No hay control sobre división por cero**, lo que puede generar errores si el precio llega a cero en algún activo sintético  
- El gráfico puede resultar extremadamente distorsionado en activos con precios bajos (como cent stocks o microfuturos)  
- **No tiene valores visibles** ni etiquetas para interpretar los niveles generados

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar también el **valor transformado como etiqueta**  
- Incluir línea cero o referencia para interpretar valores invertidos  
- Añadir parámetro para aplicar la transformación sólo al `Close` u otro campo específico  
- Mostrar advertencia si los valores se acercan a cero o provocan divisiones extremas