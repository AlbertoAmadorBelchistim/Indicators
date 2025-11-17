## 🟦 Moving Median (6/10)

**Nombre del archivo:** `MMed.cs`  
**Nombre del indicador:** Moving Median  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602433](https://help.atas.net/support/solutions/articles/72000602433)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras utilizadas para calcular la mediana móvil (por defecto: 10)

---

### 🧭 Clasificación
📂 Statistical — Mediana móvil de precios en una ventana temporal

---

### 🧠 Uso más frecuente

- Suavizar los precios **sin verse afectado por valores extremos**  
- Sustituir medias móviles convencionales en entornos con picos o gaps  
- Comparar con SMA o EMA para detectar posibles distorsiones por outliers

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Robusta frente a datos atípicos o extremos  
✅ Puede mejorar señales en entornos volátiles o erráticos  
⛔ Menos reactiva que otras medias; puede parecer lenta

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro direccional** robusto para detectar tendencia limpia  
- **Comparación con EMA/SMA** para detectar anomalías  
- **Soporte para estrategias cuantiles** o de bandas no paramétricas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `9`  

✅ Mantiene consistencia sin verse afectada por spikes o noticias  
✅ Mejora la estabilidad en comparación con medias tradicionales  
⛔ Puede tener retraso mayor que una EMA, especialmente en giros

---

### 🧪 Notas de desarrollo

- Calcula la mediana de los valores en una ventana deslizante de tamaño `Period`  
- Ordena los valores y selecciona el central (o promedio de los dos centrales si el número es par)  
- El resultado se guarda en la serie `RenderSeries`  
- No utiliza clases auxiliares (como SMA o EMA), todo el cálculo es manual por lista ordenada

---

### ❗ Incoherencias o aspectos mejorables detectadas

- En la lógica del `bar < Period`, se usa una fórmula innecesariamente compleja para encontrar el índice de la mediana  
- Se ordenan todos los valores incluso si ya estaban parcialmente ordenados → **ineficiente en tiempo real**  
- No hay validación explícita si el periodo es mayor al número de barras cargadas (aunque se mitiga parcialmente)  
- La mediana no se reinicia con nuevos valores si `RecalculateValues()` es interrumpido por error

---

### 🛠️ Propuestas de mejora

- Optimizar la búsqueda de la mediana usando estructuras como heaps o QuickSelect en lugar de `OrderBy`  
- Simplificar la lógica de índice para casos impares/pares  
- Añadir opción de visualización tipo área o bandas usando cuartiles  
- Incorporar validación si el número de barras cargadas es inferior al periodo  
- Permitir seleccionar tipo de fuente (Close, HL2, etc.) desde la interfaz

