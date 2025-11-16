## 🟦 DI- (Directional Indicator Negativo) (7/10)

**Nombre del archivo:** `DINeg.cs`  
**Nombre del indicador:** DI-  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000621048](https://help.atas.net/support/solutions/articles/72000621048)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para el cálculo del ATR y WMA (por defecto: 10)

---

### 🧭 Clasificación  
📂 Trend — Indicadores de fuerza direccional

---

### 🧠 Uso más frecuente

- Medir la **presión bajista relativa** en el precio  
- Identificar **momentos de fortaleza de la tendencia bajista**  
- Usar junto a DI+ y ADX como parte del sistema de **Directional Movement Index (DMI)**

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Útil en sistemas de seguimiento de tendencia  
✅ Compatible con ADX y otros componentes de DMI  
⛔ No aporta señales por sí solo  
⛔ Requiere combinarlo con DI+ para evaluar dirección dominante

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de tendencia bajista** si DI- crece mientras el precio cae  
- **Cruce DI+ / DI-**: entrada en favor del cruce dominante  
- **Filtro de contexto**: evitar largos si DI- está creciendo fuertemente

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `7` o `10`  
- Color rojo, línea discontinua (`Dash`)  
- Combinar con DI+ y ADX para análisis direccional completo

✅ Compatible con estructuras de tendencia rápidas  
✅ Mejora la interpretación de presión vendedora real

---

### 🧪 Notas de desarrollo

- Calcula un valor de movimiento negativo (val) si:
  - **El mínimo actual es menor que el mínimo anterior**  
  - **El rango del máximo actual respecto al anterior es más pequeño que la bajada en mínimos**  
- Este valor se suaviza mediante una **WMA**  
- Se normaliza usando el **ATR** del mismo periodo:
  \[
  DI^- = 100 \times \frac{\text{WMA}_{\text{negativo}}}{ATR}
  \]
- El valor se guarda en `this[bar]`  
- Se colorea en rojo con línea discontinua

---

### ❗ Incoherencias o aspectos mejorables detectados

- No expone los valores de **DI+ ni ADX**, por lo que **no puede usarse como sistema completo de DMI** sin combinarlo con otros indicadores  
- La condición para detectar presión bajista depende de relaciones de rango que pueden ser sensibles a **gaps o velas de rango bajo**  
- No ofrece umbrales ni filtros visuales para interpretar fuerza o cambio de tendencia

---

### 🛠️ Propuestas de mejora

- Incluir una versión combinada con DI+ y ADX (como en el sistema DMI completo)  
- Añadir soporte para alertas cuando supere umbrales (ej. > 20 o > 40)  
- Permitir mostrar **ambos DI+ y DI- en el mismo panel** con colores diferenciados  
- Mostrar etiquetas numéricas para facilitar interpretación directa