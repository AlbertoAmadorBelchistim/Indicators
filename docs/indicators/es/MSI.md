## 🟦 McClellan Summation Index (MSI) (7/10)

**Nombre del archivo:** `MSI.cs`  
**Nombre del indicador:** McClellan Summation Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602427](https://help.atas.net/support/solutions/articles/72000602427)

---

### ⚙️ Parámetros configurables

Este indicador **no tiene parámetros configurables desde la interfaz**. Utiliza por defecto:
- **ShortPeriod**: `19`  
- **LongPeriod**: `39`

---

### 🧭 Clasificación
📂 Momentum — Indicador acumulativo derivado del McClellan Oscillator

---

### 🧠 Uso más frecuente

- Evaluar la **fuerza de la tendencia del mercado** en el tiempo  
- Confirmar señales de fondo/techo cuando cambia de signo o dirección  
- Medir el **impulso acumulado** derivado de la amplitud del mercado

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Amplifica y suaviza la señal del McClellan Oscillator  
✅ Relevante para análisis de amplitud y fuerza estructural  
⛔ Su acumulación depende fuertemente de la calidad del dato de entrada

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro de fondo direccional**: operar solo en la dirección del MSI acumulado  
- **Confirmación de fondo/techo**: cruce sostenido de la línea cero  
- **Divergencias acumulativas**: cuando el MSI y el precio se desacoplan

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- No aplicable directamente a velas de 1M; se recomienda en marco superior (ej. D1)  
✅ Puede servir como **filtro contextual** si se incorpora como fuente externa  
⛔ El indicador no opera sobre volumen ni precio directamente, sino sobre una serie externa

---

### 🧪 Notas de desarrollo

- Calcula dos EMAs manualmente con constantes predefinidas:  
  `EMA_short = EMA(value, 19)`, `EMA_long = EMA(value, 39)`  
- El valor del MSI se actualiza como acumulado de la diferencia entre ambas  
- Internamente utiliza tres `ValueDataSeries`: `_shortEma`, `_longEma` y `_renderSeries`  
- El resultado se representa en un nuevo panel independiente

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No permite personalizar los periodos de las EMAs (`19`, `39`) desde la UI  
- El valor del MSI permanece constante si `value == 0`, pero no se avisa ni se colorea  
- No se indica si `value` debe ser un McClellan Oscillator o cualquier entrada arbitraria  
- La lógica de cálculo de EMAs está duplicada y no usa objetos `EMA`, lo que dificulta mantenimiento  
- El cálculo ignora el valor cero pero mantiene los acumulados sin marca visual

---

### 🛠️ Propuestas de mejora

- Permitir modificar `ShortPeriod` y `LongPeriod` desde la configuración  
- Mostrar línea cero como referencia visual clara  
- Añadir opción de color dinámico según la pendiente del MSI  
- Incluir advertencia o tooltip si se alimenta con datos no esperados (no-McClellan)  
- Refactorizar el uso de EMAs con clases estándar (`EMA`) para mayor claridad

