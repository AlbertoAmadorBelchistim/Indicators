## 🟦 Polarized Fractal Efficiency (7/10)

**Nombre del archivo:** `PolarizedFractal.cs`  
**Nombre del indicador:** Polarized Fractal Efficiency  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602281](https://help.atas.net/support/solutions/articles/72000602281)

---

### ⚙️ Parámetros configurables

- **ShortPeriod**: Periodo para calcular la eficiencia fractal (por defecto: 10)  
- **Smooth**: Periodo de suavizado EMA sobre el valor final (por defecto: 10)

---

### 🧭 Clasificación
📂 Momentum — Indicador de eficiencia direccional basado en distancia fractal suavizada

---

### 🧠 Uso más frecuente

- Medir la **eficiencia del movimiento direccional** del precio  
- Identificar fases de **movimiento tendencial vs. ruido lateral**  
- Confirmar la calidad de una ruptura o impulso

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Útil para filtrar entornos de baja direccionalidad  
✅ Suavizado con EMA mejora la señal sin perder sensibilidad  
⛔ Menos conocido, requiere interpretación contextual

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro de contexto**: evitar operar en fases ineficientes (< ±30)  
- **Confirmación de tendencia**: operar solo si el valor crece sostenidamente  
- **Señal de reversión**: si el valor cae bruscamente tras pico de eficiencia

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ShortPeriod**: `10`  
- **Smooth**: `6`

✅ Buena sensibilidad al cambio direccional  
✅ Evita operar en lateralizaciones ineficientes  
⛔ Puede reaccionar con retraso tras movimientos explosivos

---

### 🧪 Notas de desarrollo

- Calcula el cociente entre la distancia entre extremos y la suma de desviaciones cuadráticas intermedias  
- Convierte el resultado en valor porcentual (`× 100`) tras aplicar suavizado con EMA  
- Si el movimiento es bajista, el valor se invierte (`-pfe`)  
- Usa internamente dos series auxiliares (`_renderSeries`, `_sqrtSeries`)  
- Se dibuja como línea en panel separado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El nombre del parámetro `ShortPeriod` es confuso, ya que no hay un `LongPeriod`; sería más claro como `Period`  
- En `bar == 0`, se limpian las series pero no se inicializan con valor nulo explícito  
- No hay validación si `Smooth` es mayor que el número de barras disponibles  
- La fórmula de eficiencia incluye `+1` en el numerador y `+N²` en el denominador, pero no está documentado en UI ni en comentarios  
- No incluye alertas ni coloración dinámica según la dirección o intensidad

---

### 🛠️ Propuestas de mejora

- Renombrar `ShortPeriod` a `Period` para mayor claridad  
- Añadir validación si el periodo de suavizado es mayor que las barras disponibles  
- Documentar en la interfaz la interpretación del valor: qué es eficiencia, rango útil, etc.  
- Incluir codificación de color según signo o magnitud (verde/rojo o intensidad)  
- Añadir alertas visuales o etiquetas si se supera cierto umbral (ej: ±50)

