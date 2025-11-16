## 🟦 Daily Lines (9/10)

**Nombre del archivo:** `DailyLines.cs`  
**Nombre del indicador:** Daily Lines  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602284](https://help.atas.net/support/solutions/articles/72000602284)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de referencia (día, semana o mes actual o anterior)  
- **CustomSession**: Activar sesiones personalizadas  
- **FilterStartTime / FilterEndTime**: Horario de inicio y fin de la sesión personalizada  
- **ShowText / ShowPrice / DrawFromBar / DrawOverChart**: Opciones de visualización  
- **OpenPen / HighPen / LowPen / ClosePen**: Configuración visual de cada nivel  
- **HalfGapPen / HalfGapText / ShowHalfGap**: Línea y texto para el Half Gap  
- **TextSize**: Tamaño de letra para los textos  
- **OpenText / HighText / LowText / CloseText**: Etiquetas personalizadas por nivel

---

### 🧭 Clasificación  
📂 Levels — Niveles estructurales por sesión, semana o mes

---

### 🧠 Uso más frecuente

- Dibujar **líneas horizontales** en los niveles de apertura, máximo, mínimo y cierre  
- Mostrar los niveles del día actual o de periodos previos  
- Incluir una línea de **Half Gap** entre el cierre previo y la apertura actual  
- Usar en conjunto con zonas de liquidez, order flow o confirmaciones para entradas

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Versátil, permite niveles diarios, semanales y mensuales  
✅ Soporta sesiones personalizadas y horarios no estándar  
⛔ Complejo de configurar correctamente si se usan sesiones cruzadas por medianoche  
⛔ Requiere datos completos para que el periodo anterior se represente bien

---

### 🎯 Estrategias de scalping donde se aplica

- **Rechazo en niveles**: operar reversión al tocar High/Low del día anterior  
- **Ruptura con intención**: confirmar breakout si rompe el High anterior con delta fuerte  
- **Uso del Half Gap**: entrada cuando el precio se acerca y rebota en el Half Gap  
- **Proyecciones de rango**: medir targets basados en rango previo (opcional vía código)

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `PreviousDay`  
- **CustomSession**: `true`  
- **FilterStartTime / EndTime**: `15:30 – 22:00 UTC` (sesión regular)  
- **ShowHalfGap**: `true`  
- **ShowText / ShowPrice**: `true`  
- **TextSize**: `11`  
- **DrawOverChart**: `true`  

✅ Altamente útil para contexto diario  
✅ Compatible con herramientas de volumen y delta

---

### 🧪 Notas de desarrollo

- Mantiene una estructura interna `SessionRange` para registrar Open, High, Low, Close y su barra correspondiente  
- Al final de cada sesión (según modo), el rango se guarda en `_sessionHistory`  
- Permite hasta **5 sesiones archivadas** para consultar `PreviousDay`, `PreviousWeek`, etc.  
- El **Half Gap** se calcula como:  
  \[
  \text{HalfGap} = \text{Cierre previo} + \frac{\text{Apertura actual} - \text{Cierre previo}}{2}
  \]  
- Soporta sesiones que cruzan la medianoche correctamente con lógica condicional  
- La representación visual se adapta a lo visible en pantalla (`DrawFromBar`, `ShowPrice`, etc.)

---

### ❗ Incoherencias o aspectos mejorables detectados

- `ShouldArchiveSession()` usa `DateTime.UtcNow`, lo cual puede desincronizarse si se reproduce el gráfico o se carga parcialmente  
- No hay soporte para **guardar o visualizar más de 5 sesiones anteriores**, lo cual limita análisis más profundos  
- El cálculo del Half Gap solo se activa para `CurrentDay` incluso cuando se selecciona `PreviousDay` con `CustomSession = true`  
- `IsNewSession()` y `InsideSession()` son complejos y podrían refactorizarse para mayor claridad

---

### 🛠️ Propuestas de mejora

- Permitir **visualizar varias sesiones previas simultáneamente**, con líneas menos intensas  
- Añadir opción para mostrar el **rango total (High - Low)** como dato adicional  
- Incluir soporte para alertas cuando el precio cruce los niveles clave  
- Posibilidad de guardar/exportar los niveles en archivos históricos  
- Refactorizar `IsNewSession`, `ShouldArchiveSession` y `InsideSession` en métodos reutilizables