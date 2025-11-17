## 🟦 Price Volume Trend (PVT) (8 / 10)  
**Nombre del archivo:** `VolumeTrend.cs`  
**Nombre del indicador:** Price Volume Trend  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602450](https://help.atas.net/support/solutions/articles/72000602450)

---

### ⚙️ Parámetros configurables  
- Este indicador **no tiene parámetros configurables desde la UI**

---

### 🧭 Clasificación  
📂 Volume — Indicador acumulativo de volumen ponderado por variación de precio

---

### 🧠 Uso más frecuente  
- Medir la **tendencia del volumen en función del movimiento del precio**  
- Confirmar si los movimientos de precio están respaldados por volumen  
- Comparar acumulación de volumen durante tendencias alcistas o bajistas

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Útil para confirmar **tendencias fuertes acompañadas de volumen**  
✅ Muy similar al OBV pero incorpora **proporcionalidad de cambio de precio**  
⛔ Menos sensible a microvariaciones → más lento en girarse ante reversiones

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de dirección**: Validar tendencias si el PVT sube con el precio  
- **Divergencias**: Si el precio sube y el PVT baja, puede anticipar agotamiento  
- **Filtro de entradas**: Evitar operar contra la dirección del PVT acumulado

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- Sin parámetros configurables; se recomienda usarlo junto a delta o clúster para validación

✅ Buena representación de volumen validando dirección del precio  
✅ Complementa muy bien a indicadores de tendencia o rupturas  
⛔ Requiere interpretación visual y contexto (no genera señales directas)

---

### 🧪 Notas de desarrollo  
- El cálculo es:  
  `PVT[bar] = ((Close[bar] - Close[bar-1]) / Close[bar]) * Volume + PVT[bar-1]`  
- Usa el **volumen total** y el **cambio proporcional de precio**  
- Se acumula barra a barra en una serie continua  
- Dibuja una línea base en `0` para ayudar en la visualización

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No protege explícitamente contra `Close == 0`, aunque aplica fallback en tal caso  
- No permite seleccionar otros inputs como volumen de compra/venta o delta  
- No tiene alertas ni umbrales visuales configurables

---

### 🛠️ Propuestas de mejora  
- Permitir seleccionar **fuentes alternativas de volumen** (Bid, Ask, Delta)  
- Incluir **alertas por cambio de pendiente** o cruce de umbrales  
- Añadir **visualización de divergencias** automáticas con el precio
