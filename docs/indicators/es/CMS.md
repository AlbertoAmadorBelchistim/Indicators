## 🟦 Clear Method Swing Line (8/10)

**Nombre del archivo:** `CMS.cs`  
**Nombre del indicador:** Clear Method Swing Line  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602257](https://help.atas.net/support/solutions/articles/72000602257)

---

### ⚙️ Parámetros configurables

Este indicador **no tiene parámetros configurables** desde la UI.

---

### 🧭 Clasificación  
📂 Trend — Indicadores de estructura de mercado y cambios de tendencia

---

### 🧠 Uso más frecuente

- Identificar y seguir los **puntos de swing (mínimos y máximos relevantes)**  
- Confirmar cambios de dirección con lógica **objetiva de estructura de mercado**  
- Visualizar zonas de **transición entre tendencia alcista y bajista**

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Excelente para lectura estructural clara sin necesidad de subjetividad  
✅ Útil para estrategias basadas en swing highs / lows y patrones tipo Wyckoff  
⛔ Código complejo y no personalizable desde la interfaz  
⛔ No tiene integración directa con volumen ni momentum

---

### 🎯 Estrategias de scalping donde se aplica

- **Swing Reversal**: entrada en la primera vela tras cambio de estructura CMS  
- **Trend Continuation**: seguir movimiento mientras la línea CMS mantiene dirección  
- **Breakout Confirmed**: operar rompimientos solo cuando CMS cambia de nivel

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- No requiere configuración adicional  
- Recomendable usar junto con otros indicadores de validación (volumen, delta, etc.)

✅ Ideal para detectar microcambios de estructura en intradía  
✅ Se adapta automáticamente a la dinámica del precio

---

### 🧪 Notas de desarrollo

- Calcula internamente una estructura jerárquica de máximos/mínimos (`hh`, `hl`, `ll`, `lh`) para detectar swings válidos  
- Usa series auxiliares intermedias (`_hh1`, `_hh2`, `_hh3`, etc.) para validar condiciones multibarra  
- La lógica de cambio se basa en el estado de `_us` (up swing: 1 o 0)  
- Dibuja dos líneas:
  - `_upSeries` (color cyan): mantiene swing alcista  
  - `_downSeries` (color magenta): mantiene swing bajista  
- La función `SplitLines()` gestiona las transiciones limpias entre fases

---

### 🛠️ Propuestas de mejora

- Añadir parámetros de configuración para controlar la **sensibilidad o profundidad** de swing  
- Posibilidad de integrar **filtros de volumen o delta** para validar rupturas  
- Incluir etiquetas en los puntos de cambio (ej. “Break HH”, “Break LL”)  
- Mostrar áreas de consolidación si hay varios swings planos consecutivos
