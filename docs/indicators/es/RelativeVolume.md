## 🟦 Relative Volume (7/10)

**Nombre del archivo:** `RelativeVolume.cs`  
**Nombre del indicador:** Relative Volume  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602457](https://help.atas.net/support/solutions/articles/72000602457)

---

### ⚙️ Parámetros configurables

- **LookBack**: Número de sesiones para calcular el promedio horario (por defecto: 20)  
- **DeltaColored**: Activar coloreado según el delta en lugar del cuerpo de la vela  
- **PosColor / NegColor / NeutralColor**: Colores para barras con delta positivo, negativo o neutro

---

### 🧭 Clasificación
📂 Volume — Comparativa entre el volumen actual y su media en ese mismo horario

---

### 🧠 Uso más frecuente

- Detectar si el **volumen de una vela es significativamente mayor o menor al promedio histórico**  
- Confirmar entradas si el volumen relativo es alto en una ruptura o zona relevante  
- Medir **momentos de alta o baja participación relativa** a nivel horario

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Útil para filtrar señales según contexto de participación relativa  
✅ Resalta automáticamente barras anómalas en comparación al histórico  
⛔ Solo funciona correctamente en gráficos basados en tiempo

---

### 🎯 Estrategias de scalping donde se aplica

- **Validación de ruptura**: volumen relativo alto confirma movimiento  
- **Filtro direccional**: evitar operar en condiciones de volumen débil  
- **Detección de absorción** si hay delta bajo pero volumen alto relativo

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **LookBack**: `20`  
- **DeltaColored**: `true`  
- **PosColor**: verde  
- **NegColor**: rojo  
- **NeutralColor**: gris

✅ Permite identificar velas con anomalías de participación  
✅ Se adapta al contexto específico del horario actual  
⛔ No aplicable en gráficos no temporales (ej. volumen, ticks)

---

### 🧪 Notas de desarrollo

- Compara el volumen actual con la **media histórica de velas que ocurrieron a la misma hora**  
- Usa un diccionario `Dictionary<TimeSpan, AvgBar>` para acumular medias por horario  
- Dibuja un punto (serie `_averagePoints`) con el valor medio para referencia visual  
- Soporta coloreado por **delta** o por dirección del cuerpo de la vela  
- El histograma de volumen se representa con `VisualMode.Histogram`, puntos medios con `VisualMode.Dots`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La variable `_isSupportedTimeFrame` solo permite gráficos de tiempo o segundos, pero no lanza advertencia si se usa en otros tipos  
- No hay protección explícita si `GetCandle(bar - 1)` da error al principio del gráfico  
- El valor de `_averagePoints` se establece antes de haber agregado el volumen de la vela anterior → **puede haber un desfase de una barra**  
- El cálculo depende del `TimeOfDay`, lo que puede dar lugar a sesgos si hay cambios de horario o gaps  
- No se incluye ningún valor visual de comparación (porcentaje o ratio sobre el promedio)

---

### 🛠️ Propuestas de mejora

- Incluir validación visual o mensaje si el gráfico no es de tipo soportado  
- Ajustar el orden: primero añadir volumen de la vela anterior y luego usar la media  
- Añadir etiqueta con porcentaje de volumen relativo (`actual / promedio`)  
- Permitir mostrar líneas horizontales de referencia en lugar de puntos  
- Ofrecer opción de suavizado en los puntos promedio para evitar saltos por valores atípicos

