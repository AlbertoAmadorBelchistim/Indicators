## 🟦 Inside Bar (InsideEqualsBar) (7.5/10)

**Nombre del archivo:** `InsideEqualsBar.cs`  
**Nombre del indicador:** Inside Bar  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602245](https://help.atas.net/support/solutions/articles/72000602245)

---

### ⚙️ Parámetros configurables

- **ToleranceType**: Modo de tolerancia para considerar que una vela está dentro de otra:
  - `Ticks`: número de ticks de diferencia permitida
  - `Price`: diferencia absoluta en precio
  - `Percent`: porcentaje respecto a la vela base  
- **Tolerance**: Valor numérico asociado a `ToleranceType`  
- **CandleArea**: Zona considerada para evaluar el inside bar:
  - `HighLow`: cuerpo completo (máximo y mínimo)
  - `Body`: solo cuerpo real (Open vs Close)  
- **AreaColor**: Color del área visual que cubre el rango del patrón detectado

---

### 🧭 Clasificación  
📂 Levels — Detección visual de estructuras de consolidación o compresión

---

### 🧠 Uso más frecuente

- Detectar **patrones de inside bar** (consolidación dentro de una vela anterior)  
- Visualizar **zonas de equilibrio o pausa** antes de ruptura  
- Identificar series consecutivas de velas contenidas dentro del mismo rango

---

### 📊 Nivel de relevancia  
🔟 **7.5 / 10**

✅ Muy útil como herramienta visual para patrones de breakout  
✅ Flexible gracias al sistema de tolerancia y personalización  
⛔ No realiza cálculo predictivo ni alertas  
⛔ No considera el volumen, solo rango visual

---

### 🎯 Estrategias de scalping donde se aplica

- **Ruptura de inside bar**: operar en la dirección de la ruptura tras confirmación de agresión  
- **Consolidación en zona clave**: detectar si el precio está comprimiendo antes de un desequilibrio  
- **Rechazo dentro del rango**: operar contraria si el precio no logra romper el patrón

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ToleranceType**: `Ticks`  
- **Tolerance**: `2`  
- **CandleArea**: `HighLow`  
- **AreaColor**: amarillo semitransparente (`ARGB(100, 200, 200, 0)`)  
- Usar con `DOM`, `Delta` o `Volume Profile` para confirmar contexto

✅ Compatible con entornos de compresión previa a impulso  
✅ Útil para entradas tácticas en rupturas o reversiones

---

### 🧪 Notas de desarrollo

- Evalúa si la vela `bar - 1` está contenida dentro de una anterior (`startCandle`) según tolerancia  
- Guarda el rango como par (`_currentStart`, `bar - 1`) si se detecta contención válida  
- Representa el área entre la vela base y la última contigua como rectángulo en `OnRender`  
- Admite acumulación de inside bars sucesivos hasta que se rompa la condición  
- Compatible con `ChartVisualModes.Clusters`, ajusta altura por `PriceRowHeight` si es necesario

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se valida si el precio de la vela `startCandle` es cero → podría provocar división por cero con `Percent`  
- La lógica depende de `bar - 1` y `bar - 2`, lo cual puede omitir la vela actual como parte del rango  
- No hay soporte para alertas visuales ni condiciones de finalización específicas (solo ruptura)

---

### 🛠️ Propuestas de mejora

- Añadir opción para lanzar alertas (sonoras o visuales) cuando se rompe el rango del inside bar  
- Permitir seleccionar número mínimo de inside bars consecutivos antes de validación  
- Exponer visualmente los límites superior e inferior con líneas horizontales  
- Añadir etiquetas de texto indicando la duración del patrón (`n` velas dentro de la misma)
