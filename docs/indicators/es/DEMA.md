## 🟦 Double Exponential Moving Average (DEMA) (6.5/10)

**Nombre del archivo:** `DEMA.cs`  
**Nombre del indicador:** Double Exponential Moving Average  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602543](https://help.atas.net/support/solutions/articles/72000602543)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para el cálculo de la media doblemente suavizada (por defecto: 10)

---

### 🧭 Clasificación  
📂 Trend — Medias móviles adaptativas para detección de tendencia

---

### 🧠 Uso más frecuente

- Suavizar el precio con menor retraso que una EMA o SMA convencional  
- Identificar cambios de tendencia más rápidos  
- Usar como base para sistemas de cruce de medias (ej. DEMA + EMA o DEMA + SMA)

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Menor lag que una EMA o SMA tradicional  
✅ Útil para entradas más tempranas o sistemas de reacción rápida  
⛔ Puede ser sensible al ruido en marcos de tiempo bajos  
⛔ No aporta información adicional más allá de la curva suavizada

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce de medias**: entrada cuando DEMA cruza por encima/abajo de otra media  
- **Reversión rápida**: usar cambio de pendiente o giro para anticipar reversión  
- **Filtrado de entradas**: operar solo si el precio está por encima o por debajo de la DEMA según el sesgo

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `8` a `13` (según sensibilidad deseada)  
- Combinar con EMA(21) o VWAP para filtro de contexto  
- Usar junto con delta o absorción para confirmar entrada

✅ Reduce retraso respecto a otras medias  
✅ Compatible con sistemas reactivos o tipo momentum

---

### 🧪 Notas de desarrollo

- Calcula internamente:
  - Primera EMA (`_emaFirst`) con el valor de entrada  
  - Segunda EMA (`_emaSecond`) sobre el resultado de la primera  
- El valor final se calcula como:
  \[
  \text{DEMA} = 2 \times \text{EMA}_1 - \text{EMA}_2
  \]
- Se guarda en `_renderSeries[bar]`  
- La serie permite minimizar modo visual (`UseMinimizedModeIfEnabled = true`)

---

### ❗ Incoherencias o aspectos mejorables detectados

- No hay verificación si `bar == 0` antes de acceder a valores previos, aunque en práctica es tolerado por el sistema de EMA de ATAS  
- El cálculo es correcto, pero **podría exponerse `EMA1` y `EMA2` como series secundarias** si se desea inspección o debug

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar las dos EMAs internas junto a la DEMA  
- Incluir un modo para usar precios distintos (ej. cierre, típico, weighted, etc.)  
- Mostrar alerta si se produce un cruce con otra media configurada manualmente  
- Añadir etiquetas de texto opcionales con el valor actual
