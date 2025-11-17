## 🟦 Mean Deviation (5/10)

**Nombre del archivo:** `MeanDeviation.cs`  
**Nombre del indicador:** Mean Deviation  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602428](https://help.atas.net/support/solutions/articles/72000602428)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para calcular la media y la desviación respecto a ella (por defecto: 10)

---

### 🧭 Clasificación
📂 Statistical — Indicador de dispersión basado en desviación media absoluta

---

### 🧠 Uso más frecuente

- Medir la **variabilidad media** del precio respecto a su promedio  
- Estimar si el precio está en una fase de **alta o baja dispersión**  
- Complementar a medias móviles como filtro de contexto

---

### 📊 Nivel de relevancia
🔟 **5 / 10**

✅ Útil para estimar el "ruido" del mercado  
✅ Más robusto ante valores extremos que la desviación estándar  
⛔ Menos comúnmente utilizado y menos intuitivo que otros indicadores de volatilidad

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro de contexto**: evitar operar en fases de baja variabilidad  
- **Confirmación de explosión de volatilidad**: si la desviación se incrementa rápidamente  
- **Soporte para bandas dinámicas**: usar como componente para crear canales adaptativos

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `14`

✅ Capta bien el rango medio de dispersión en tramos de 1 minuto  
✅ Ayuda a identificar compresión antes de ruptura  
⛔ No proporciona señales de entrada/salida por sí solo

---

### 🧪 Notas de desarrollo

- Calcula la **desviación media absoluta**:  
  `MeanDev = avg(|precio - SMA|)`  
- Se usa una `SMA` interna como base  
- La lógica recorre el número de barras definido en `Period` y acumula la distancia absoluta  
- El resultado se guarda en la serie principal (`this[bar]`)  
- No tiene visualización personalizada ni configuración de color

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se permite elegir el tipo de media base (solo `SMA`), lo cual limita flexibilidad  
- El nombre del indicador puede inducir a confusión con la desviación estándar (que es diferente)  
- No incluye opciones visuales ni panel de configuración estética  
- No hay validación explícita si el periodo supera el número de barras disponibles (aunque se mitiga con `Math.Min`)

---

### 🛠️ Propuestas de mejora

- Permitir elegir entre `SMA`, `EMA` u otras como base del cálculo  
- Incluir una línea cero como referencia visual  
- Añadir opción de coloreado dinámico si la desviación aumenta o disminuye  
- Ofrecer un modo de visualización alternativo (barra, área o relleno)  
- Añadir línea de señal opcional (por ejemplo, media móvil de la Mean Dev)

