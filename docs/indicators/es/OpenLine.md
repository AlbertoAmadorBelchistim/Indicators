## 🟦 Open Line (8/10)

**Nombre del archivo:** `OpenLine.cs`  
**Nombre del indicador:** Open Line  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602440](https://help.atas.net/support/solutions/articles/72000602440)

---

### ⚙️ Parámetros configurables

- **Days**: Número de sesiones a visualizar hacia atrás (por defecto: 5)  
- **CustomSessionStartFilter**: Hora personalizada de inicio de sesión  
- **ExtendLastLineToRight**: Extender línea de apertura de la última sesión hasta el borde del gráfico  
- **TillTouch**: Terminar la línea si es tocada por el precio  
- **OpenCandleText**: Texto a mostrar junto a la línea  
- **FontSize / Offset**: Tamaño y posición del texto  
- **LinePen**: Color y grosor de la línea de apertura

---

### 🧭 Clasificación
📂 Level — Niveles estructurales de apertura por sesión o segmento personalizado

---

### 🧠 Uso más frecuente

- Visualizar la **línea de apertura de cada sesión o bloque horario**  
- Detectar **rejeciones o absorciones** en el nivel de apertura  
- Confirmar estructuras o trampas en apertura (ej: falso breakout, recuperación)

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Muy útil para contexto estructural y análisis de sesión  
✅ Compatible con sesiones personalizadas y scalping institucional  
⛔ Requiere configuración precisa si se trabaja con horarios no estándar

---

### 🎯 Estrategias de scalping donde se aplica

- **Venta en rechazo de la apertura** si hay absorción  
- **Compra si el precio recupera apertura tras trampa bajista**  
- **Filtro direccional**: operar solo si el precio se mantiene por encima/debajo de la apertura

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Days**: `5`  
- **CustomSessionStartFilter**: `09:30` (inicio sesión cash USA)  
- **TillTouch**: `true`  
- **ExtendLastLineToRight**: `true`  
- **LinePen.Width**: `2`, color visible (ej: SkyBlue)

✅ Trazado robusto de línea de apertura incluso si se forma en vela no visible  
✅ Compatible con escenarios de apertura, trampa y continuación  
⛔ No representa más de una línea por sesión (solo apertura)

---

### 🧪 Notas de desarrollo

- Detecta inicio de sesión según sistema o filtro personalizado (`FilterTimeSpan`)  
- Crea un objeto `Session` por día, con propiedades de precio y control de si fue tocada  
- Dibuja líneas horizontales con texto adjunto usando `RenderContext.DrawLine` y `DrawString`  
- La última línea puede extenderse incluso si ya fue tocada, según configuración  
- Gestión clara de sesiones pasadas y visualización limpia sobre el gráfico de precios

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se permite representar **líneas múltiples** por sesión (solo apertura)  
- Si el primer bar coincide con la hora de filtro exacta, no se crea la sesión hasta la siguiente vela  
- No hay validación si `FontSize` es muy bajo o `OpenCandleText` es muy largo  
- La detección de cruce con el nivel (`TillTouch`) no permite tolerancia (ej: dentro de 1 tick)  
- No hay alertas ni visualización numérica del precio de apertura

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar **valor numérico** junto a la etiqueta  
- Permitir **dibujar líneas adicionales** como cierre previo o midpoint  
- Incluir opción de **alerta al cruce o toque** del nivel de apertura  
- Añadir tolerancia (en ticks) a la lógica de "touch"  
- Mejorar detección de inicio de sesión cuando el filtro coincide exactamente con `bar == 0`

