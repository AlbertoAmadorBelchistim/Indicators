## 🟦 Cumulative Daily Volume (6/10)

**Nombre del archivo:** `CumulativeDailyVolume.cs`  
**Nombre del indicador:** Cumulative Daily Volume  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000618670](https://help.atas.net/support/solutions/articles/72000618670)

---

### ⚙️ Parámetros configurables

- **HistogramColor**: Color del histograma de volumen acumulado (por defecto: azul)

---

### 🧭 Clasificación  
📂 Volume — Indicadores de volumen tradicional por vela o sesión

---

### 🧠 Uso más frecuente

- Medir el **volumen total acumulado** desde el inicio de cada sesión  
- Evaluar si la sesión actual está mostrando **volumen inusualmente alto o bajo**  
- Comparar el ritmo de acumulación de volumen respecto a sesiones previas  
- Confirmar o filtrar señales basadas en la presencia de volumen institucional

---

### 📊 Nivel de relevancia  
🔟 **6 / 10**

✅ Muy útil para análisis comparativo entre sesiones  
✅ Ayuda a detectar días de alta actividad (news, eventos)  
⛔ No incluye descomposición del volumen (Bid/Ask, Delta)  
⛔ No tiene umbrales ni herramientas estadísticas incorporadas

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro de días activos**: entrar solo si el volumen acumulado supera la media  
- **Confirmación de ruptura**: validar breakout si el volumen del día es alto  
- **Anticipación de reversión**: rechazar setups si el volumen está por debajo del habitual

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **HistogramColor**: azul o púrpura para buena visibilidad sin distracción  
- Complementar con indicadores de volumen relativo o comparación intersesión

✅ Aporta contexto sin sobrecargar el gráfico  
✅ Mejora la toma de decisiones en escenarios de alta o baja liquidez

---

### 🧪 Notas de desarrollo

- El indicador acumula el volumen de cada vela desde el inicio de la sesión  
- Si la sesión es nueva (`IsNewSession(bar)`), se reinicia el contador a cero  
- Usa un `ValueDataSeries` con `VisualMode.Histogram` y color configurable  
- El volumen acumulado se guarda en `_data[bar]` y se actualiza en tiempo real  
- El color del histograma se gestiona a través de la propiedad `HistogramColor`

---

### ❗ Incoherencias o aspectos mejorables detectados

- La variable `_sum` se reinicia correctamente solo en sesiones nuevas, pero **su valor previo se actualiza incluso si `bar` no ha cambiado**, lo cual puede causar inconsistencias si `OnCalculate` se llama múltiples veces para el mismo `bar` sin pasar por `bar != _lastBar`.  
- **No se acumula si el bar actual se recalcula** sin cambiar su índice: esto podría afectar gráficos con datos cargados parcialmente o en replays
- ⚠️ Aunque está categorizado como `VolumeOrderFlow` en el código, **este indicador no utiliza order flow**. Solo acumula el volumen total por vela, por lo que su clasificación lógica es `Volume` tradicional.

---

### 🛠️ Propuestas de mejora

- Forzar acumulación en cada llamada a `OnCalculate` o añadir protección contra múltiples llamadas al mismo `bar` sin pérdida de precisión  
- Añadir opción para mostrar líneas de referencia (media de volumen, desviación típica)  
- Incluir una función de **comparación contra sesiones anteriores** (por ejemplo, mostrar % acumulado respecto al día anterior a la misma hora)  
- Opción de alertas visuales si se supera un umbral de volumen acumulado