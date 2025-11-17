## 🟦 Lowest (6/10)

**Nombre del archivo:** `Lowest.cs`  
**Nombre del indicador:** Lowest  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602417](https://help.atas.net/support/solutions/articles/72000602417)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras hacia atrás para buscar el mínimo valor en la serie de entrada (por defecto: 10)

---

### 🧭 Clasificación
📂 Level — Indicadores que marcan niveles de precio relevantes (mínimos, máximos, pivotes)

---

### 🧠 Uso más frecuente

- Determinar el **mínimo local** de un rango para detectar soportes  
- Generar señales de compra cuando el precio rompe por encima del mínimo anterior  
- Servir como base para otros indicadores (e.g. %K, trailing stops, breakout)

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Simple y eficiente para detección de extremos  
✅ Útil como componente en estrategias más complejas  
⛔ Por sí solo, no ofrece contexto ni validación adicional

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de giro**: si el precio deja de hacer mínimos decrecientes  
- **Entrada por breakout**: tras superación del último mínimo importante  
- **Trailing stop dinámico**: ajustar stop al mínimo de las últimas `n` velas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`

✅ Captura zonas de soporte recientes  
✅ Buena respuesta sin ser excesivamente volátil  
⛔ Puede dar señales atrasadas si el mercado gira rápido

---

### 🧪 Notas de desarrollo

- Calcula el mínimo en una ventana móvil de longitud `Period`  
- Usa `SourceDataSeries` como entrada de datos  
- Solo utiliza un bucle sencillo para comparar valores históricos  
- El valor se actualiza en `this[bar]` con el mínimo encontrado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se permite elegir qué serie (Close, Low, etc.) se evalúa como entrada → depende exclusivamente de `SourceDataSeries`  
- No se controla el caso donde `Period > CurrentBar`, lo que puede causar resultados inconsistentes en barras iniciales  
- No ofrece opción de visualizar el valor mínimo mediante líneas auxiliares o etiquetas  
- No se expone ninguna alerta si el mínimo cambia de valor

---

### 🛠️ Propuestas de mejora

- Permitir selección de la fuente de datos (`Low`, `Close`, etc.)  
- Añadir visualización opcional del mínimo actual con una línea en el gráfico  
- Incluir alertas visuales o sonoras cuando el valor mínimo cambia  
- Añadir una opción para mostrar también el bar en el que ocurrió el mínimo  
- Ofrecer posibilidad de suavizar el mínimo mediante una media móvil si se desea

