## 🟦 Margin Zones (9/10)

**Nombre del archivo:** `MarginZones.cs`  
**Nombre del indicador:** Margin Zones  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602421](https://help.atas.net/support/solutions/articles/72000602421)

---

### ⚙️ Parámetros configurables

- **Margin**: Valor de margen utilizado como base para el cálculo (por defecto: 3200)  
- **TickCost**: Valor monetario de un tick (por defecto: 6.25)  
- **ZoneWidth**: Número de días de anchura de las zonas (por defecto: 3)  
- **CustomPriceFilter**: Permite fijar un precio base manual o dejarlo en automático  
- **Direction**: Dirección de la zona (Up / Down)  
- **Visualización por zona**: Colores y visibilidad individual para las zonas 25%, 50%, 75%, 100%, 150%, 200% y línea base

---

### 🧭 Clasificación
📂 Level — Niveles de relevancia institucional o técnica en base a zonas de margen

---

### 🧠 Uso más frecuente

- Visualizar zonas de **interés institucional** en base al precio y margen  
- Detectar **niveles clave de defensa o presión** en el gráfico  
- Medir posibles **objetivos o reversiones** en función de zonas alcanzadas

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Altamente útil para análisis profesional de contexto institucional  
✅ Visualización muy rica y configurable  
⛔ Necesita comprensión del concepto de margen y valor por tick

---

### 🎯 Estrategias de scalping donde se aplica

- **Operar reversiones** al llegar a zona 100%, 150% o 200%  
- **Confirmar presión compradora/vendedora** si se defiende o rompe una zona  
- **Proyectar targets o stops** usando múltiplos del margen (150%, 200%)

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Margin**: `3200`  
- **TickCost**: `6.25`  
- **ZoneWidth**: `3`  
- **Direction**: `Down` (para detectar presión bajista)  
- **CustomPriceFilter**: desactivado (modo automático)

✅ Muestra zonas relevantes en gráficos de 1 minuto con claridad  
✅ Compatible con estructuras tipo Wyckoff o VSA  
⛔ Necesita contexto y análisis adicional para operar con confianza

---

### 🧪 Notas de desarrollo

- Calcula una serie de niveles (`25%`, `50%`, `75%`, `100%`, `150%`, `200%`) sobre un precio base  
- El precio base puede ser manual (`CustomPrice`) o automático (mínimo/máximo semanal)  
- Cada zona se representa como un rectángulo coloreado y línea auxiliar  
- La dirección de cálculo (`Up` o `Down`) cambia si se busca un mínimo o un máximo  
- Se actualiza al final de cada semana y se recalcula si el precio rompe la zona base  
- Visualmente se dibujan los rectángulos solo dentro del rango visible

---

### ❗ Incoherencias o aspectos mejorables detectadas

- Si `TickCost` es muy bajo o erróneo, el cálculo de zonas se dispara y puede romper el gráfico  
- No hay validación visual si `Margin` o `ZoneWidth` son incoherentes → podría no mostrarse nada  
- El valor `zoneSize` se calcula con división entera (`Margin / TickCost`), que puede inducir a error si el valor no es múltiplo del tick  
- La línea base no se dibuja si `BaseLineFilter` está desactivado, pero no hay advertencia visible  
- Se utilizan varios `DataSeries` invisibles únicamente para mantener valores, lo que podría optimizarse

---

### 🛠️ Propuestas de mejora

- Validar que `TickCost > 0` y mostrar advertencia si es demasiado bajo  
- Mostrar tooltip o etiqueta con el precio base y nivel de cada zona  
- Añadir opción para alinear las zonas a cierres o máximos/minimos de sesión  
- Permitir cambiar el tipo de cálculo base (por ejemplo, mínimo del mes, no solo semana actual)  
- Añadir alertas visuales si se cruza o se mantiene dentro de una zona clave

