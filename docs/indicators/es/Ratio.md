## 🟦 Ratio (9/10)

**Nombre del archivo:** `Ratio.cs`  
**Nombre del indicador:** Ratio  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602282](https://help.atas.net/support/solutions/articles/72000602282)

---

### ⚙️ Parámetros configurables

- **Days**: Número de sesiones hacia atrás a analizar (por defecto: 20)  
- **LowRatio**: Umbral inferior para color de ratio bajo (por defecto: 0.71)  
- **NeutralRatio**: Umbral para color de ratio neutro (por defecto: 29)  
- **FontSize**: Tamaño del texto (por defecto: 10)  
- **LowColor / NeutralColor / HighColor**: Colores para los distintos rangos del ratio  
- **BackgroundColor**: Color de fondo del texto

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Ratio de presión bid/ask en zonas clave según dirección de la vela

---

### 🧠 Uso más frecuente

- Medir **presión en bid o ask** según tipo de vela (alcista/bajista)  
- Identificar zonas con **desequilibrio o absorción significativa**  
- Visualizar **etiquetas numéricas** en el gráfico con ratio de agresión

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Indica con precisión desequilibrios relevantes al final de la vela  
✅ Útil para confirmar trampas o validaciones de nivel  
⛔ Solo muestra una etiqueta por vela, sin histórico de ratios o líneas

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de ruptura con presión** en el lado esperado  
- **Trampa con absorción** si la vela cierra bajista pero hay fuerte ratio comprador  
- **Soporte/resistencia reforzada** si aparece un ratio bajo o neutro tras testeo

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Days**: `20`  
- **LowRatio**: `0.70`  
- **NeutralRatio**: `1.20`  
- **FontSize**: `10`  
- **Colors**: Verde (bajo), Gris (neutral), Azul (alto)

✅ Refuerza decisiones por acción del precio y contexto institucional  
✅ Compatible con absorciones, reversiones y tests en zonas relevantes  
⛔ Puede generar ruido si se aplica sin filtro de volumen mínimo

---

### 🧪 Notas de desarrollo

- Para velas alcistas analiza el **bid** en los dos precios más bajos  
- Para velas bajistas analiza el **ask** en los dos precios más altos  
- Calcula un **ratio entre niveles consecutivos** y lo muestra como etiqueta flotante  
- Determina el color del texto en función de los umbrales definidos por el usuario  
- Soporta control de sesiones mediante `IsNewSession()` para limitar cálculo al rango deseado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay validación si `NeutralRatio < LowRatio`, lo que puede invertir la lógica de colores  
- El texto de la etiqueta se usa directamente para parsear el valor → frágil si el formato cambia o hay error en conversión  
- El uso de `Days = 0` desactiva el cálculo sin notificación clara en la interfaz  
- El `bar == 0` solo define `_targetBar` sin iniciar la serie → puede dejar el primer valor sin calcular  
- Las etiquetas se eliminan y vuelven a crear incluso si no cambian, lo cual puede afectar el rendimiento en gráficos densos

---

### 🛠️ Propuestas de mejora

- Añadir validación visual para inconsistencias entre `LowRatio` y `NeutralRatio`  
- Mostrar tooltip o etiqueta con explicación del valor (`Buy Ratio`, `Sell Ratio`, etc.)  
- Permitir guardar ratios históricos en una serie para análisis posterior  
- Incluir opción para filtrar por volumen mínimo de la vela  
- Optimizar `ReDraw()` evitando recrear etiquetas sin necesidad

