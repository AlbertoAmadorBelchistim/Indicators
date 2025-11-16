## 🟦 Demand Index (6.5/10)

**Nombre del archivo:** `Demand.cs`  
**Nombre del indicador:** Demand Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602288](https://help.atas.net/support/solutions/articles/72000602288)

---

### ⚙️ Parámetros configurables

- **BuySellPower**: Periodo para suavizado del volumen y rango (por defecto: 10)  
- **BuySellSmooth**: Periodo para suavizado de buy/sell power (por defecto: 10)  
- **IndicatorSmooth**: Periodo de la SMA aplicada al valor final (por defecto: 10)

---

### 🧭 Clasificación  
📂 Volume — Indicadores derivados del análisis de volumen y rango

---

### 🧠 Uso más frecuente

- Medir la **presión de compra o venta** relativa en base a cambios de precio y volumen  
- Detectar acumulación o distribución oculta con herramientas no convencionales  
- Evaluar la **fuerza direccional** combinando volumen y dinámica de precio

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Basado en una fórmula clásica (James Sibbet) con valor teórico  
✅ Representación clara con línea principal y SMA suavizada  
⛔ Fórmula compleja, poco conocida por la mayoría de operadores  
⛔ Sensible a errores de datos si hay huecos o volumen cero

---

### 🎯 Estrategias de scalping donde se aplica

- **Detección de acumulación**: índice positivo creciente sin subida de precio  
- **Confirmación de ruptura**: spike de demanda index con precio saliendo de rango  
- **Reversión por agotamiento**: divergencia entre precio y línea de demanda index

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **BuySellPower**: `10`  
- **BuySellSmooth**: `7`  
- **IndicatorSmooth**: `5`  
- Combinar con Delta, CVD o zonas de perfil para detectar entrada institucional

✅ Suaviza bien el ruido sin perder señales importantes  
✅ Compatible con setups de absorción y presión delta sostenida

---

### 🧪 Notas de desarrollo

- Usa una fórmula basada en **price sum**, volumen y diferencia relativa para calcular dos valores:
  - **bp** (Buy Power)
  - **sp** (Sell Power)
- Aplica medias exponenciales sobre bp y sp, luego compara ambas:
  - Si `bp > sp`:  
$$
    DI = 100 \times \left(1 - \frac{sp}{bp}\right)
$$
  - Si `sp > bp`:  
    $$
    DI = 100 \times (\frac{bp}{sp} - 1)
    $$
- Aplica una **SMA adicional** sobre el resultado final  
- Representa la serie principal y la suavizada  
- Incluye una línea cero como referencia neutral

---

### ❗ Incoherencias o aspectos mejorables detectados

- Usa el `High` y `Low` de la primera vela como base de comparación para todos los cálculos, lo que puede provocar **sesgos si esa vela es atípica o errónea**  
- Si el volumen de la vela actual es cero o menor que el del EMA, el índice puede volverse extremadamente sensible  
- El uso de `Math.Exp` con multiplicadores de rango puede generar **overflow o resultados extremos**, ya controlados en parte por try/catch

---

### 🛠️ Propuestas de mejora

- Permitir usar el rango promedio en vez del rango de la primera vela para mayor robustez  
- Añadir etiquetas visuales o colores para zona de fuerte presión de compra/venta  
- Mostrar delta o volumen como línea secundaria para validación rápida  
- Permitir activar alertas cuando la línea cruce umbrales (ej. +30 o -30)