## 🟦 Greatest Swing Value (7/10)

**Nombre del archivo:** `GreatestSwing.cs`  
**Nombre del indicador:** Greatest Swing Value  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602635](https://help.atas.net/support/solutions/articles/72000602635)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras usadas para el promedio de swings (por defecto: 10)  
- **Multiplier**: Factor de ampliación aplicado al swing promedio (por defecto: 5)

---

### 🧭 Clasificación  
📂 Levels — Indicadores de canales dinámicos calculados a partir de swings

---

### 🧠 Uso más frecuente

- Calcular **niveles dinámicos de soporte y resistencia** basados en swings  
- Determinar zonas donde el mercado tiende a revertir según **amplitud histórica del movimiento**  
- Proyectar rangos extremos para entrada o salida según impulso reciente

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Lectura estructural basada en movimientos reales (swings)  
✅ Se adapta dinámicamente al comportamiento reciente del mercado  
⛔ No incluye confirmación por volumen o delta  
⛔ Requiere calibración cuidadosa del multiplicador

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada contraria en zonas de proyección**: cuando el precio alcanza los niveles proyectados y aparece señal de absorción  
- **Breakout validado**: si el precio atraviesa el nivel sin retroceso  
- **Trailing stop táctico**: usar el canal como guía para trailing de posiciones abiertas

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`  
- **Multiplier**: `5`  
- Color verde para el canal superior, rojo para el inferior  
- Usar junto con delta o CVD para confirmar agresión en extremos

✅ Útil para estimar targets y stops dinámicos  
✅ Complementa estructuras visuales de absorción o reversión

---

### 🧪 Notas de desarrollo

- Calcula la diferencia entre:
  - **High y Open** para velas bajistas (`Close < Open`)  
  - **Open y Low** para velas alcistas (`Close > Open`)  
- Acumula estos swings en series `_buy` y `_sell`, respectivamente  
- Luego calcula un promedio móvil **ignorando ceros** (`SkipZeroMa`)  
- Aplica el multiplicador sobre ese promedio y lo proyecta desde el Open:  
  $$
  \text{Nivel superior} = \text{Open} + \text{mediaBuy} \cdot \text{Multiplier}  
  \\
  \text{Nivel inferior} = \text{Open} - \text{mediaSell} \cdot \text{Multiplier}
  $$

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El cálculo de swings depende estrictamente de si `Close < Open` o `Close > Open`, lo cual **excluye doji o velas neutras**, aunque puedan contener swing útil  
- El uso de `bar - 1` en la media móvil puede provocar desfase visual o discrepancia en valores iniciales  
- No permite visualizar los valores crudos de swing ni el promedio, solo el canal final proyectado

---

### 🛠️ Propuestas de mejora

- Incluir opción para mostrar también los valores de swing bruto y media móvil  
- Añadir soporte para incluir velas doji (Close ≈ Open) si el rango cumple condiciones mínimas  
- Exponer los niveles proyectados como `RangeDataSeries` para facilitar visualización adicional  
- Incluir lógica de alerta cuando el precio toque o atraviese el canal proyectado
