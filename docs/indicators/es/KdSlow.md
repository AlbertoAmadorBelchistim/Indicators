## 🟦 KD - Slow (7/10)

**Nombre del archivo:** `KdSlow.cs`  
**Nombre del indicador:** KD - Slow  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602412](https://help.atas.net/support/solutions/articles/72000602412)

---

### ⚙️ Parámetros configurables

- **PeriodK**: Periodo para el cálculo de la línea %K (por defecto: 10)  
- **PeriodD**: Periodo para la media móvil de %K en `KdFast` (por defecto: 10)  
- **SlowPeriodD**: Periodo adicional de suavizado aplicado a la línea %D (por defecto: 10)

---

### 🧭 Clasificación
📂 Momentum — Oscilador estocástico suavizado con doble media móvil

---

### 🧠 Uso más frecuente

- Filtrar señales del estocástico rápido con suavizados adicionales  
- Confirmar giros con menor ruido y mayor estabilidad  
- Detectar zonas de sobrecompra/sobreventa más consistentes

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Reduce señales falsas al suavizar %K y %D  
✅ Mejor para operativa en marcos más amplios o sistemas conservadores  
⛔ Puede retrasar las señales respecto a KD-Fast o KDJ

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por cruce retardado** de %K y %D suavizados  
- **Confirmación de señales previas** generadas por otros indicadores rápidos  
- **Detección de giros sostenidos** en movimientos prolongados

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **PeriodK**: `8`  
- **PeriodD**: `3`  
- **SlowPeriodD**: `3`

✅ Ofrece mayor estabilidad para entradas en zonas clave  
✅ Reduce sobre-reacción ante velas individuales  
⛔ Puede llegar tarde si el mercado gira bruscamente

---

### 🧪 Notas de desarrollo

- El indicador encapsula internamente un `KdFast` y aplica una media a sus salidas  
- %K se suaviza mediante `_kSma` y %D mediante `_dSma`  
- Los valores se calculan sobre las `ValueDataSeries` generadas por `KdFast`  
- Se dibujan dos líneas: `_kSeries` (roja) y `_dSeries` (verde), ambas en un nuevo panel

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El nombre de la propiedad `PeriodD` está duplicado para dos parámetros distintos (`PeriodD` y `SlowPeriodD`), lo que puede generar confusión en la UI  
- El usuario no tiene control sobre el tipo de media móvil usada para suavizar (siempre es `SMA`)  
- No hay representación visual de zonas de sobrecompra/sobreventa, lo que dificulta su uso sin referencias adicionales  
- El acceso a las `ValueDataSeries` internas de `KdFast` es directo y poco encapsulado, lo cual podría romperse si cambia la estructura interna de `KdFast`

---

### 🛠️ Propuestas de mejora

- Renombrar propiedades para evitar ambigüedad entre `PeriodD` y `SlowPeriodD`  
- Permitir elegir el tipo de media móvil (por ejemplo, `EMA`, `SMMA`)  
- Añadir líneas horizontales en 20 / 80 para sobrecompra/sobreventa  
- Incluir alertas visuales o sonoras al cruce de líneas o entrada/salida de zonas extremas  
- Encapsular mejor el acceso a los valores de `KdFast` para aumentar la robustez del diseño

