## 🟦 Linear Regression (6/10)

**Nombre del archivo:** `LinearReg.cs`  
**Nombre del indicador:** Linear Regression  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602415](https://help.atas.net/support/solutions/articles/72000602415)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras utilizadas para calcular la recta de regresión lineal (por defecto: 10)

---

### 🧭 Clasificación
📂 Trend — Indicador de tendencia basado en regresión lineal

---

### 🧠 Uso más frecuente

- Representar visualmente la dirección y pendiente del precio en un periodo dado  
- Identificar tendencias mediante la inclinación de la línea regresiva  
- Usar la línea como soporte/resistencia dinámica en sistemas de entrada/salida

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Útil para observar inclinaciones de corto o medio plazo  
✅ Refleja bien el sesgo direccional del precio  
⛔ No reacciona bien ante giros bruscos o consolidaciones erráticas

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de sesgo direccional** antes de operar rupturas  
- **Operaciones de reversión** cuando el precio se aleja excesivamente de la línea  
- **Scalping tendencial** en fases de pendiente clara

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`

✅ Muestra claramente el sesgo de las últimas 10 velas  
✅ Reacciona con suficiente agilidad a los cambios en M1  
⛔ Puede repintar si se recalcula en gráficos en tiempo real

---

### 🧪 Notas de desarrollo

- Calcula la recta de regresión lineal clásica sobre el periodo definido  
- Utiliza sumatorias de `x`, `y`, `x²`, `x*y` para derivar la pendiente y la intersección  
- El valor resultante para cada `bar` se obtiene como `y = k * bar + b`  
- El resultado se guarda en la serie principal del indicador

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La variable `bar` se usa como `x` directamente en la predicción final (`k * bar + b`), lo cual puede introducir inconsistencias si el eje X no representa tiempo lineal  
- No hay validación frente a `k == 0` más allá de forzar el valor a 0, lo cual puede ocultar divisiones mal condicionadas  
- No se ofrece opción para visualizar los residuos o la calidad del ajuste (R² o similar)  
- No se permite cambiar el tipo de fuente (solo `SourceDataSeries`), lo que limita su flexibilidad

---

### 🛠️ Propuestas de mejora

- Reescalar la serie `x` para que empiece en cero (`x = 0, 1, 2, ..., N-1`) y evitar ambigüedades con índices reales  
- Añadir serie de residuos para evaluar desviaciones  
- Incluir opción para mostrar la pendiente (`k`) como valor en tooltip o etiqueta  
- Permitir aplicar regresión sobre otras series (High, Low, Median)  
- Añadir suavizado opcional para evitar saltos entre líneas si se usa en tiempo real

