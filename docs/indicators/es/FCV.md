## 🟦 Full Contract Value (FCV) (6/10)

**Nombre del archivo:** `FCV.cs`  
**Nombre del indicador:** Full Contract Value  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602389](https://help.atas.net/support/solutions/articles/72000602389)

---

### ⚙️ Parámetros configurables

- **CustomTickSize (CustomScale)**: Valor personalizado para escalar el indicador  
- **CurrentTickSize**: Tamaño de tick del instrumento (solo lectura)

---

### 🧭 Clasificación  
📂 Price — Escalado proporcional de precios según contrato

---

### 🧠 Uso más frecuente

- Visualizar el precio multiplicado por el valor del tick, simulando el **valor monetario del contrato**  
- Facilitar análisis económico del movimiento del precio (valor en USD o divisa base)  
- Integrar en análisis donde se requiera transformar precios en base a capital o contrato

---

### 📊 Nivel de relevancia  
🔟 **6 / 10**

✅ Útil para convertir precios en magnitudes monetarias reales  
✅ Facilita análisis financiero directo sin cálculo externo  
⛔ No aporta señal técnica  
⛔ Requiere conocer bien el valor del tick o multiplicador del instrumento

---

### 🎯 Estrategias de scalping donde se aplica

- **Análisis de riesgo por tick**: calcular fácilmente la pérdida/ganancia por movimiento mínimo  
- **Confirmación de escalas monetarias**: ver si una zona representa $100, $500, etc.  
- **Filtro de impacto**: descartar operaciones con movimiento bajo valorado

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **CustomTickSize**: activado, valor `12.5` para ES (1 tick = $12.5)  
- Representar el valor de las velas en escala monetaria  
- Combinar con indicadores de volumen para evaluar el impacto financiero real

✅ Traduce precio a magnitudes económicas  
✅ Ideal para calcular riesgo/recompensa directamente en gráfico

---

### 🧪 Notas de desarrollo

- El valor mostrado es:
  $$
  FCV = \text{Precio} \times \frac{\max(\text{TickSize}, \text{Multiplier})}{\text{TickSize}}
  $$
- Usa un campo `Filter` para definir el multiplicador (`CustomTickFilter`)  
- Si está desactivado, usa `InstrumentInfo.TickSize` como valor base  
- El `Multiplier` se recalcula automáticamente si se modifica el filtro  
- El valor se guarda en `_renderSeries` y se actualiza dinámicamente

---

### ❗ Incoherencias o aspectos mejorables detectadas

- En `OnCalculate`, se usa `Math.Max(TickSize, Multiplier)`, lo que puede provocar **valores inconsistentes si el usuario no entiende la relación entre ambos**  
- El nombre `FCV` sugiere un valor absoluto de contrato, pero solo aplica una transformación proporcional (no considera tamaño de contrato ni leverage)  
- No se valida si el `Multiplier` tiene sentido económico (por ejemplo, 0.0001 para contratos grandes)

---

### 🛠️ Propuestas de mejora

- Añadir una opción para mostrar directamente el valor en **USD por movimiento**  
- Cambiar nombre visible a `Full Contract Value (Tick × Price)` para mayor claridad  
- Incluir etiqueta flotante con el valor por tick o por punto completo  
- Permitir multiplicar por tamaño del contrato (ej. `price × tick × lotSize`)
