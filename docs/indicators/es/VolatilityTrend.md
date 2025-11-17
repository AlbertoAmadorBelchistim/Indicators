## 🟦 Volatility Trend (9 / 10)  
**Nombre del archivo:** `VolatilityTrend.cs`  
**Nombre del indicador:** Volatility Trend  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602267](https://help.atas.net/support/solutions/articles/72000602267)

---

### ⚙️ Parámetros configurables  
- **Period**: Periodo para el cálculo del ATR (por defecto: `10`)  
- **MaxDynamicPeriod**: Periodo máximo dinámico para determinar el rango de máximos/mínimos (por defecto: `15`)

---

### 🧭 Clasificación  
📂 Volatility — Indicador de volatilidad direccional basada en dinámica de precio + ATR

---

### 🧠 Uso más frecuente  
- Confirmar la **dirección de la volatilidad** en función del movimiento continuo del precio  
- Detectar si el mercado está en **modo tendencial con expansión de rango**  
- Generar un canal dinámico que actúe como **zona de soporte o resistencia volátil**

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  
✅ Combina **dirección del precio + amplitud del rango**  
✅ Útil para detectar **expansiones direccionales** sostenidas  
⛔ Puede comportarse de forma errática en mercados laterales con cambios constantes de dirección

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de dirección**: Validar que una ruptura tiene continuidad con dirección y expansión volátil  
- **Canal dinámico**: Usar como guía de soporte/resistencia adaptativa basada en dirección + ATR  
- **Evitar operaciones en congestión**: Si el rango dinámico es bajo o cambia dirección constantemente

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Period**: `10`  
- **MaxDynamicPeriod**: `15`

✅ Excelente para detectar **rupturas con volatilidad creciente**  
✅ Útil para filtrar señales en entornos de rango vs dirección  
⛔ Puede cambiar de dirección con demasiada frecuencia en días sin tendencia clara

---

### 🧪 Notas de desarrollo  
- Usa un **ATR clásico** como base para medir amplitud  
- Calcula una **dirección binaria (+1 / -1)** según si el precio actual sube o baja respecto al anterior  
- Suma dinámicamente los periodos consecutivos con misma dirección y ajusta el cálculo de máximo o mínimo  
- Resta el ATR del extremo correspondiente para generar el valor de salida

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- Utiliza `this[bar - 1]` como referencia en la dirección sin validación inicial robusta  
- La lógica de expansión puede dar valores erráticos si el **cambio de dirección** es frecuente  
- No permite personalizar la lógica de dirección (por ejemplo, usar `Close` vs `High/Low`)

---

### 🛠️ Propuestas de mejora  
- Añadir opción de **alertas visuales** cuando cambie la dirección del canal  
- Permitir seleccionar **tipo de fuente** para el cálculo direccional (`Close`, `Median`, `Typical`)  
- Incluir soporte para dibujar una **banda o canal completo** con colores para dirección
