## 🟦 Daily HighLow (7.5/10)

**Nombre del archivo:** `DailyHighLow.cs`  
**Nombre del indicador:** Daily HighLow  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602609](https://help.atas.net/support/solutions/articles/72000602609)

---

### ⚙️ Parámetros configurables

- **Days**: Número de días hacia atrás desde los cuales empezar a mostrar los niveles (por defecto: 20)

---

### 🧭 Clasificación  
📂 Levels — Indicadores que muestran niveles de referencia horizontales

---

### 🧠 Uso más frecuente

- Mostrar el **máximo, mínimo y media** del día en curso  
- Comparar el **nivel medio actual con el del día anterior**  
- Evaluar zonas relevantes para breakout, absorción o reversión  
- Identificar posibles soportes o resistencias dinámicas

---

### 📊 Nivel de relevancia  
🔟 **7.5 / 10**

✅ Útil para contextualizar el movimiento del precio intradía  
✅ Facilita análisis estructural (HH/LL y midpoint)  
⛔ No incorpora información de volumen o delta  
⛔ El parámetro `Days` no filtra datos, solo define el punto desde el que se muestran

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada en reversión**: esperar rebote en el High o Low diario  
- **Breakout validado**: operar si se rompe el máximo/mínimo con intención  
- **Comparación de medianas**: si la mediana del día actual supera la del anterior → sesgo alcista

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Days**: `1` o `0` (para mostrar solo el día actual y el anterior)  
- **VisualType**: cuadrado discreto, colores neutros que no interfieran  
- Combinar con Delta, CVD o absorciones para validar movimientos

✅ Muy visual para contextos de microestructura  
✅ Compatible con setup de reversión o breakout

---

### 🧪 Notas de desarrollo

- Calcula dinámicamente:
  - **High del día actual**
  - **Low del día actual**
  - **Mediana = (High + Low) / 2**
  - **Mediana del día anterior**
- Usa `ValueDataSeries` con `VisualMode.Square` para representación puntual  
- Se reinician valores al detectar una nueva sesión (`IsNewSession(bar)`)  
- El parámetro `Days` solo limita el **inicio de cálculo**, no filtra visualización posterior

---

### ❗ Incoherencias o aspectos mejorables detectados

- El valor de `_targetBar` depende del número de sesiones pasadas, pero **no se actualiza dinámicamente si cambia el parámetro `Days` tras el inicio**  
- No se protege el acceso a `GetCandle(_lastSession - 1)` si el día anterior no existe (riesgo si `Days = 0`)  
- Las series de datos comienzan a mostrar valores solo a partir del día seleccionado, pero **no hay opción para ocultar días anteriores o marcar la frontera**

---

### 🛠️ Propuestas de mejora

- Permitir mostrar líneas extendidas horizontales en lugar de puntos  
- Añadir opción para **mostrar el rango diario numérico** (High - Low)  
- Incluir soporte para **niveles históricos adicionales** (3 días atrás, apertura, cierre)  
- Mejorar control del parámetro `Days` y su efecto real en el gráfico  
- Incluir etiquetas opcionales con los valores (por ejemplo: “High 5320.25”)
