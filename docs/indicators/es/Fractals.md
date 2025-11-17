## 🟦 Fractals (7/10)

**Nombre del archivo:** `Fractals.cs`  
**Nombre del indicador:** Fractals  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602388](https://help.atas.net/support/solutions/articles/72000602388)

---

### ⚙️ Parámetros configurables

- **Mode**: Mostrar fractales altos, bajos, ambos o ninguno  
- **ShowLine**: Dibujar línea horizontal desde el fractal hasta ser tocado  
- **HighPen / LowPen**: Estilo y color para las líneas de fractal alto y bajo  
- **VisualMode**: Configuración del modo de visualización (puntos)

---

### 🧭 Clasificación  
📂 Levels — Indicadores de detección de niveles fractales de soporte/resistencia

---

### 🧠 Uso más frecuente

- Detectar **niveles potenciales de soporte o resistencia**  
- Marcar máximos y mínimos locales con condiciones estrictas de comparación  
- Confirmar rupturas o rechazos en niveles fractales recientes

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Muy útil para análisis de microestructura o zonas técnicas limpias  
✅ Compatible con visualización de líneas persistentes y puntos  
⛔ No utiliza volumen ni delta para validar los niveles  
⛔ Puede generar señales redundantes en mercados de baja volatilidad

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada en rechazo de fractal reciente**  
- **Ruptura confirmada**: operar si el precio atraviesa un fractal previo y no vuelve  
- **Zona clave estructural**: usar los fractales como referencia para proyecciones u objetivos

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Mode**: `All`  
- **ShowLine**: `true`  
- **HighPen / LowPen**: líneas finas, verdes y rojas respectivamente  
- Visualizar solo los últimos 3-5 fractales recientes para claridad

✅ Preciso como herramienta de contexto técnico  
✅ Se puede alinear con herramientas de absorción o volumen para mayor validación

---

### 🧪 Notas de desarrollo

- El fractal se define como vela (bar - 2) que es:
  - Mayor que las 2 anteriores y las 2 posteriores en su **High** → fractal alto  
  - Menor que las 2 anteriores y posteriores en su **Low** → fractal bajo  
- Se dibujan puntos visuales en `_fractalUp` y `_fractalDown`  
- Si `ShowLine = true`, se crea una línea horizontal con `LineTillTouch` desde el fractal hasta que el precio la toque  
- El sistema gestiona visualmente líneas activas mediante una lista `HorizontalLinesTillTouch`  
- Permite ocultar todas las señales seleccionando `Mode = None`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El parámetro `VisualMode` aparece pero no está implementado dinámicamente (los puntos siempre son `Dots`)  
- Si `ShowLine = true`, y el fractal cambia en la misma vela (reentrada), la línea previa puede no eliminarse correctamente  
- Las líneas se actualizan usando `_lastBar` sin validación cruzada con `CurrentBar`, lo que puede dejar residuos si hay reinicio parcial del gráfico

---

### 🛠️ Propuestas de mejora

- Hacer que `VisualMode` permita elegir entre puntos, triángulos o etiquetas  
- Permitir limitar el número de fractales visibles o históricos  
- Añadir opción para mostrar el **valor de precio exacto** en el fractal como texto flotante  
- Incluir alertas si el precio toca un nivel fractal relevante
