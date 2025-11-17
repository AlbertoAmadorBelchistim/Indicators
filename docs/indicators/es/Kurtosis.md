## 🟦 Kurtosis (5/10)

**Nombre del archivo:** `Kurtosis.cs`  
**Nombre del indicador:** Kurtosis  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602556](https://help.atas.net/support/solutions/articles/72000602556)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras usadas para calcular la curtosis (mínimo: 4; por defecto: 20)

---

### 🧭 Clasificación
📂 Statistical — Medida de curtosis sobre precios (forma de la distribución)

---

### 🧠 Uso más frecuente

- Medir si la distribución de precios presenta colas pesadas (leptocúrtica) o ligeras (platicúrtica)  
- Detectar momentos de alta concentración o dispersión extrema en los precios  
- Complementar análisis de volatilidad o eventos estadísticamente atípicos

---

### 📊 Nivel de relevancia
🔟 **5 / 10**

✅ Útil en análisis cuantitativo o estudios de comportamiento estadístico del precio  
✅ Distingue fases de concentración vs dispersión de precios  
⛔ Poco intuitivo para uso visual directo sin formación estadística

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro de entorno**: evitar entradas cuando la curtosis indica alta concentración de precios  
- **Confirmación de agotamiento** tras un spike de curtosis (colas pesadas = evento extremo)  
- **Detección de anomalías** para scalping estadístico en gráficos cuantitativos

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `20`  

✅ Captura bien las fases de agrupación o expansión anómalas  
✅ Útil como filtro estadístico en sistemas avanzados  
⛔ Necesita interpretación contextual: no es señal directa de entrada/salida

---

### 🧪 Notas de desarrollo

- Calcula tanto **curtosis poblacional** como **muestral (estimador)**  
- La SMA se usa como media base para restar y calcular desviaciones  
- Usa dos `ValueDataSeries` auxiliares (`Square`, `Quad`) para almacenar potencias 2 y 4 de la desviación  
- Representa dos líneas: `PopulationSeries` (línea base) y `SampleSeries` (en azul)

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se valida si `squareSum` es cero → división por cero puede ocurrir si no hay volatilidad (precio plano)  
- El cálculo para `SampleSeries` incluye constantes que pueden ser inestables para valores de `Period` muy cercanos a 4  
- No se indica en UI cuál serie corresponde a curtosis poblacional o muestral, lo que puede confundir al usuario  
- La fórmula del estimador muestral no está comentada ni referenciada, lo que dificulta su comprensión y validación

---

### 🛠️ Propuestas de mejora

- Añadir validación y control si `squareSum == 0` para evitar división por cero  
- Incluir tooltip o descripción clara de qué representa cada línea (Poblacional vs Muestral)  
- Ofrecer opción de mostrar solo una de las dos líneas  
- Añadir referencias estadísticas o bibliográficas en comentarios para futuras validaciones  
- Posibilidad de exportar valores para análisis externo en sistemas cuantitativos

