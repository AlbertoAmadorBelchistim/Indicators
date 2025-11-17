## 🟦 ZigZag Pro (10 / 10)  
**Nombre del archivo:** `Zigzag.cs`  
**Nombre del indicador:** ZigZag Pro  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602632](https://help.atas.net/support/solutions/articles/72000602632)

---

### ⚙️ Parámetros configurables  
- **CalcMode**: Modo de cálculo (`Relative`, `Absolute`, `Ticks`)  
- **Percentage**: Umbral de cambio requerido (en %/ticks/precio según modo)  
- **Days**: Días hacia atrás desde los que comenzar el cálculo  
- **IgnoreWicks**: Ignorar mechas en el cálculo de máximos y mínimos  
- **TextSize / TextColor**: Tamaño y color del texto  
- **VerticalOffset**: Desplazamiento vertical del texto respecto al precio  
- **ShowDelta / ShowVolume / ShowTicks / ShowBars / ShowTime**: Mostrar datos acumulativos en cada tramo (delta, volumen, número de ticks, número de barras, duración)

---

### 🧭 Clasificación  
📂 PriceAction — Indicador visual de ondas basado en estructura de zigzag con métricas acumuladas

---

### 🧠 Uso más frecuente  
- Visualizar ondas de precio con etiquetas de **delta, volumen, ticks y duración**  
- Confirmar **intensidad de un tramo** o debilidad mediante las métricas acumuladas  
- Identificar zonas de reversión, clímax o absorción basándose en la estructura

---

### 📊 Nivel de relevancia  
🔟 **10 / 10**  
✅ Altamente configurable y visualmente potente para lectura estructural  
✅ Compatible con múltiples estilos de análisis (Wyckoff, VSA, patrones armónicos)  
⛔ No genera señales automáticas; depende de la interpretación del operador

---

### 🎯 Estrategias de scalping donde se aplica  
- **Lectura de estructura**: Confirmar si un tramo con muchos ticks y poco delta es absorción  
- **Comparación de ondas**: Validar si una onda de continuación pierde fuerza (menos volumen/delta)  
- **Reversión en contexto**: Entrada cuando se rompe una onda y el tramo nuevo muestra delta agresivo

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **CalcMode**: `Ticks`  
- **Percentage**: `30`  
- **Days**: `2`  
- **IgnoreWicks**: `true`  
- **ShowDelta / Volume / Ticks / Bars / Time**: `true`  
- **VerticalOffset**: `1`  
- **TextSize**: `15`

✅ Ideal para desglosar estructura intradía en tiempo real  
✅ Complemento clave para análisis de clúster y confirmación contextual  
⛔ Requiere interpretación activa por parte del operador

---

### 🧪 Notas de desarrollo  
- Calcula ondas según cambio porcentual, absoluto o en ticks  
- Etiqueta cada swing con datos acumulados: delta, volumen, ticks, barras y duración  
- Permite visualización adaptativa con desplazamiento vertical y tamaño de texto  
- Usa lógica robusta para evitar repintado en swings recientes  
- Alterna automáticamente entre _uptrend_ y _downtrend_ con lógica de validación

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- Uso de `bar - 1` y `GetCandle(0)` para comparación puede dar lecturas sesgadas al inicio  
- El identificador de texto `AddText(id)` puede generar conflictos si hay varias instancias  
- No muestra líneas de conexión entre extremos previos en tiempo real (solo visualización actual)

---

### 🛠️ Propuestas de mejora  
- Añadir modo de **líneas conectadas entre swings** con valores históricos  
- Incluir opción de **resaltado visual de máximos acumulativos** (volumen o delta)  
- Permitir activar **alertas sonoras o visuales** al cerrar una nueva onda
