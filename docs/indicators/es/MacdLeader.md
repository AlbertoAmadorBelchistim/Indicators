## 🟦 MACD Leader (6.5/10)

**Nombre del archivo:** `MacdLeader.cs`  
**Nombre del indicador:** MACD Leader  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602419](https://help.atas.net/support/solutions/articles/72000602419)

---

### ⚙️ Parámetros configurables

- **MacdPeriod**: Periodo de señal del MACD (por defecto: 9)  
- **MacdShortPeriod**: Periodo corto del MACD y de las EMAs rápidas auxiliares (por defecto: 12)  
- **MacdLongPeriod**: Periodo largo del MACD y de las EMAs lentas auxiliares (por defecto: 26)

---

### 🧭 Clasificación
📂 Momentum — Variante del MACD con alisamiento doble de EMAs

---

### 🧠 Uso más frecuente

- Identificar señales adelantadas respecto al MACD clásico  
- Detectar aceleraciones y giros de momentum antes que el histograma convencional  
- Medir la diferencia entre impulso de corto y largo plazo con ajuste suave

---

### 📊 Nivel de relevancia
🔟 **6.5 / 10**

✅ Más reactivo que el MACD clásico gracias al alisamiento diferencial  
✅ Puede anticipar cambios de tendencia leves  
⛔ Su interpretación requiere mayor comprensión del funcionamiento interno

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación anticipada**: entrada cuando el valor de `MACD Leader` gira antes que el MACD convencional  
- **Divergencia temprana**: cuando el `MACD Leader` se desacopla del histograma MACD clásico  
- **Detección de aceleraciones**: si el valor se separa rápidamente de cero

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **MacdPeriod**: `5`  
- **MacdShortPeriod**: `8`  
- **MacdLongPeriod**: `21`

✅ Rápida respuesta al impulso intradía  
✅ Combina suavizado con sensibilidad  
⛔ Puede generar señales falsas sin confirmación adicional (delta, volumen, etc.)

---

### 🧪 Notas de desarrollo

- Combina la lógica del MACD con dos EMAs adicionales que calculan la diferencia entre valor y su propia EMA  
- La fórmula base es:  
  `MACD Leader = MACD + (EMA rápida residual - EMA lenta residual)`  
- Se visualiza en un nuevo panel con dos series: la principal (`RenderSeries`) y el histograma MACD convencional  
- Se reutiliza internamente el objeto `MACD` con sus propios parámetros de corto, largo y señal

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay validación cruzada para evitar que `MacdShortPeriod > MacdLongPeriod`  
- El valor mostrado depende de cuatro EMAs distintas, pero solo se representa una línea, lo que dificulta depuración  
- No se puede visualizar ni acceder directamente a las EMAs residuales, lo que impide verificar su contribución  
- No incluye sistema de alertas ni codificación de colores por zonas o dirección  
- El histograma mostrado (`DataSeries[1]`) hereda directamente del MACD y puede inducir a confusión sin personalización

---

### 🛠️ Propuestas de mejora

- Permitir visualizar individualmente las EMAs auxiliares o su diferencia  
- Añadir alertas visuales/auditivas cuando el valor cruce cero o cambie de dirección  
- Implementar lógica de coloración según la pendiente o el cruce del valor con cero  
- Validar coherencia entre periodos de corto y largo plazo  
- Documentar en la UI qué representa el valor de la línea principal (`RenderSeries`) para facilitar su uso

