## 🟦 Initial Balance (9/10)

**Nombre del archivo:** `InitialBalance.cs`  
**Nombre del indicador:** Initial Balance  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602294](https://help.atas.net/support/solutions/articles/72000602294)

---

### ⚙️ Parámetros configurables

- **Days**: Número de sesiones a mostrar hacia atrás  
- **Period**: Duración del Initial Balance (en minutos o barras)  
- **PeriodMode**: Tipo de periodo (Minutes o Bars)  
- **CustomSessionStart**: Activar sesión personalizada  
- **StartDate / EndDate**: Hora de inicio y fin para la sesión personalizada  
- **X1 / X2 / X3**: Multiplicadores de expansión para IBHX1-3 e IBLX1-3  
- **ShowOpenRange**: Mostrar rectángulo del rango de apertura  
- **BorderWidth / BorderColor / FillColor**: Estilo del rectángulo  
- **DrawText**: Mostrar etiquetas con los niveles  
- **FontSize**: Tamaño del texto  
- **ExtendLastLineToRight**: Extender líneas hasta el borde derecho  
- **ShowDuringFormation**: Mostrar niveles incluso durante la formación  
- **Colores de áreas**: Para cada zona de expansión (IBHX32, IBHX21, etc.)

---

### 🧭 Clasificación  
📂 Levels — Representación estructurada del rango de apertura y expansiones

---

### 🧠 Uso más frecuente

- Identificar el **rango inicial de equilibrio** (primera hora / 60 minutos / N barras)  
- Visualizar **zonas proyectadas de expansión** (IBHX1, IBHX2, etc.)  
- Evaluar si el precio está **dentro o fuera del balance inicial**  
- Confirmar roturas reales o falsas del rango IB

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Muy útil para interpretar estructura del día y zonas clave  
✅ Soporta sesiones personalizadas, etiquetas, áreas y proyecciones  
⛔ Requiere configuración precisa del horario para ser efectivo  
⛔ No usa volumen o delta, solo niveles estructurales

---

### 🎯 Estrategias de scalping donde se aplica

- **Ruptura del IBH o IBL**: entrada si el precio sale con convicción  
- **Rechazo en IBHX1/IBLX1**: posible reversión si hay absorción en esas zonas  
- **Operar dentro del IB**: operar extremos si el precio permanece encajado  
- **Target técnico**: usar IBHX3/IBLX3 como nivel final de proyección

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `60`  
- **PeriodMode**: `Minutes`  
- **CustomSessionStart**: `true`, StartDate = `9:30`, EndDate = `16:00`  
- **X1/X2/X3**: `1 / 2 / 3`  
- **ShowOpenRange / DrawText / ExtendLastLineToRight**: `true`  
- Colores personalizados por zona para facilitar lectura  
- **ShowDuringFormation**: `true` para ver zonas desde el inicio

✅ Ideal para sesiones regulares del mercado de EE.UU.  
✅ Compatible con setups basados en ruptura, absorción o test de zona

---

### 🧪 Notas de desarrollo

- Detecta inicio de sesión usando horarios personalizados o cambio de día  
- Calcula el máximo/mínimo desde el inicio hasta el fin del periodo IB  
- A partir de ese rango, proyecta líneas `X1`, `X2`, `X3` arriba y abajo  
- Visualiza etiquetas, rectángulo de apertura y áreas coloreadas entre niveles  
- Soporta múltiples sesiones (almacena niveles por sesión en `_sessionIBValues`)  
- Lógica robusta para sesiones cruzadas (antes o después de medianoche)

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El cálculo de niveles solo se actualiza si `ShowDuringFormation = true`; de lo contrario, no se dibujan durante la primera hora  
- En sesiones personalizadas con misma hora de inicio y fin (`StartDate == EndDate`), no hay validación clara  
- No hay control visual sobre qué niveles (IB, IBHX1, etc.) se quieren mostrar u ocultar individualmente  
- Puede haber superposición de etiquetas si el espacio vertical es reducido

---

### 🛠️ Propuestas de mejora

- Añadir toggles para mostrar/ocultar individualmente niveles (IBHX1, IBLX1, etc.)  
- Incluir lógica para mostrar extensión visual (POC o VWAP) dentro del IB  
- Guardar y reutilizar niveles históricos para backtesting visual  
- Añadir alertas cuando se cruce un nivel clave (ej. IBH, IBHX2, etc.)  
- Mostrar etiquetas flotantes con el rango del IB y su altura en ticks/puntos
