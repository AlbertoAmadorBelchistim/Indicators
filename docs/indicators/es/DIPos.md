## 🟦 DI+ (Directional Indicator Positivo) (7/10)

**Nombre del archivo:** `DIPos.cs`  
**Nombre del indicador:** DI+  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000621049](https://help.atas.net/support/solutions/articles/72000621049)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para el cálculo del ATR y WMA (por defecto: 10)

---

### 🧭 Clasificación  
📂 Trend — Indicadores de fuerza direccional

---

### 🧠 Uso más frecuente

- Medir la **presión compradora relativa**  
- Identificar **tendencias alcistas** con fortaleza creciente  
- Usar junto a DI- y ADX en sistemas de **Directional Movement Index (DMI)**

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Mide fuerza alcista basada en máximos crecientes  
✅ Compatible con DI-, ADX y estrategias de cruce direccional  
⛔ Aislado, no sirve para análisis completo de tendencia  
⛔ No representa ni confirma la dirección dominante sin compararlo con DI-

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de tendencia alcista** si DI+ crece de forma sostenida  
- **Cruce DI+ sobre DI-**: posible señal de compra  
- **Evitar cortos** cuando DI+ está muy elevado y estable

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `7` o `10`  
- Color azul, línea continua  
- Combinar con DI- y ADX para visión completa de tendencia y fuerza

✅ Detecta impulsos alcistas con claridad  
✅ Compatible con estructuras de continuación en tendencia

---

### 🧪 Notas de desarrollo

- Detecta presión compradora cuando:
  - **El máximo actual supera al anterior**  
  - **La diferencia en máximos es mayor que la caída en mínimos**
- Si se cumple, se mide esa distancia como “val” y se suaviza con WMA  
- Se normaliza por ATR del mismo periodo:
  $$
  DI^+ = 100 \times \frac{\text{WMA}_{\text{positivo}}}{ATR}
  $$
- El valor se guarda en `this[bar]`  
- El color por defecto es azul (línea sólida)

---

### ❗ Incoherencias o aspectos mejorables detectados

- Igual que en **DI-**, la señal puede ser inconsistente en velas con bajo rango o gaps  
- No se expone valor conjunto con DI- ni se incluye ADX, por lo que se debe complementar manualmente  
- No hay alertas ni líneas auxiliares (ej. umbral de 20 o 40)

---

### 🛠️ Propuestas de mejora

- Integrar DI+ y DI- en un solo indicador opcional con colores diferenciados  
- Añadir visualización del **ADX** para analizar fuerza direccional  
- Incluir **alertas configurables** cuando se cruce cierto nivel o haya cruce DI+ / DI-  
- Mostrar etiquetas con valor actual o máximo reciente
