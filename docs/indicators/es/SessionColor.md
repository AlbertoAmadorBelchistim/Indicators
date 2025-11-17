## 🟦 Session Color (6/10)  
**Nombre del archivo:** `SessionColor.cs`  
**Nombre del indicador:** Session Color  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602465](https://help.atas.net/support/solutions/articles/72000602465)

---

### ⚙️ Parámetros configurables  
- **ShowAboveChart**: Dibuja la zona coloreada por encima del gráfico  
- **ShowArea**: Rellenar toda el área vertical de la sesión  
- **AreaColor**: Color del área o de los bordes verticales de la sesión  
- **StartTime / EndTime**: Hora de inicio y fin de la sesión marcada  
- **OpenAlertFilter / CloseAlertFilter**: Alertas personalizadas para el inicio y fin de sesión  

---

### 🧭 Clasificación  
📂 Visualization — Marcado visual de sesiones horarias sobre el gráfico

---

### 🧠 Uso más frecuente  
- Marcar **zonas horarias clave** en el gráfico (apertura USA, cierre Europa, etc.)  
- Delimitar **franjas de operativa intensiva** o de eventos macro  
- Añadir **alertas automáticas** al inicio y cierre de sesiones  

---

### 📊 Nivel de relevancia  
🔟 **6 / 10**  
✅ Aporta enfoque visual inmediato sobre franjas horarias relevantes  
✅ Compatible con alertas sonoras o visuales  
⛔ Limitado a una única franja horaria por instancia  

---

### 🎯 Estrategias de scalping donde se aplica  
- **Pre-market y apertura americana**: Marcar de 13:00 a 16:30 para vigilar zonas de mayor volumen  
- **Eventos programados**: CPI, FOMC o NFP en tramos fijos como 14:30 o 16:00  
- **Sesiones solapadas**: Londres + NY de 14:00 a 17:00  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **StartTime**: `15:30`  
- **EndTime**: `16:30`  
- **AreaColor**: `rgba(65,105,225,63)`  
- **ShowArea**: `true`  
- **ShowAboveChart**: `false`  
- **OpenAlertFilter / CloseAlertFilter**: Activados con nombre personalizado

✅ Enfatiza el tramo más relevante del día con claridad visual  
✅ Las alertas permiten reaccionar con precisión al inicio y fin de tramo  
⛔ No es útil si se necesita representar múltiples sesiones simultáneamente  

---

### 🧪 Notas de desarrollo  
- Crea sesiones con rangos horarios configurables mediante una clase `Session` interna  
- Pinta área coloreada o líneas verticales usando `OnRender` con `RenderContext`  
- Se adapta a cambios de horario con `InstrumentInfo.TimeZone`, aunque de forma frágil  
- Puede lanzar alertas personalizadas al inicio o fin de la sesión si están habilitadas  
- Usa listas internas para gestionar múltiples sesiones cargadas parcialmente  

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No se valida si `StartTime == EndTime` → podría dar lugar a sesiones invisibles  
- Usa `AddHours` directamente con la zona horaria, sin contemplar DST (horario de verano)  
- El método `StartSession` solo busca hacia adelante desde `bar`, no hacia atrás → posible omisión  
- La propiedad `ShowAboveChart` se enlaza con `DrawAbovePrice` pero no afecta a `OnRender`  
- Se insertan nuevas sesiones con `Insert(0, ...)`, lo que puede desordenar cronológicamente la lista  

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **varias franjas horarias simultáneas**  
- Permitir pintar solo el **rango de precios** de la sesión, no toda el área vertical  
- Mejorar el soporte para **zonas horarias dinámicas** con detección automática de DST  
- Opción para mostrar solo si hay volumen o actividad (evitar zonas planas)  
- Posibilidad de asociar etiquetas o tooltips a cada sesión visual  
