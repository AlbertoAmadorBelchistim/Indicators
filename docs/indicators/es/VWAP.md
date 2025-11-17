## 🟦 VWAP / TWAP (10 / 10)  
**Nombre del archivo:** `VWAP.cs`  
**Nombre del indicador:** VWAP / TWAP  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602503](https://help.atas.net/support/solutions/articles/72000602503)

---

### ⚙️ Parámetros configurables  
- **Type**: Tipo de periodo (`M15`, `M30`, `Hourly`, `H4`, `Daily`, `Weekly`, `Monthly`, `All`, `Custom`)  
- **TWAPMode**: Modo de cálculo: `VWAP` o `TWAP`  
- **VolumeMode**: Tipo de volumen: `Total`, `Bid`, `Ask`  
- **StDev / StDev1 / StDev2**: Desviaciones estándar para bandas  
- **ColoredDirection**: Colorear línea principal según dirección  
- **BullishColor / BearishColor**: Colores para dirección alcista/bajista  
- **VWAPOnly**: Ocultar bandas y mostrar solo línea central  
- **Days**: Días hacia atrás para cálculo  
- **CustomSessionStart / CustomSessionEnd**: Horario personalizado para periodo `Custom`  
- **AllowCustomStartPoint**: Activar punto de inicio manual (con teclas F/G y opciones de guardado)  
- **ShowFirstPeriod**: Mostrar el primer periodo parcial  
- **ResetOnSession**: Resetear cálculo al inicio de sesión

---

### 🧭 Clasificación  
📂 Volume — Media ponderada por volumen y tiempo con desviaciones y sesiones

---

### 🧠 Uso más frecuente  
- Calcular la **media ponderada por volumen o tiempo** para evaluar valor justo  
- Confirmar contextos de **valor aceptado, rechazo o desviación extrema**  
- Dibujar bandas dinámicas para **zonas de reversión o continuación**

---

### 📊 Nivel de relevancia  
🔟 **10 / 10**  
✅ Clave para definir **zonas de equilibrio institucional**  
✅ Soporta múltiples sesiones, bandas de desviación y alertas visuales  
⛔ Requiere configuración cuidadosa de horarios y filtros si se usa en varios timeframes

---

### 🎯 Estrategias de scalping donde se aplica  
- **Reversión en bandas**: Entrada cuando el precio toca desviación ±2 o ±3 y rebota  
- **Reentrada tras test al VWAP**: Confirmar rechazo o aceptación del valor justo  
- **Ruptura institucional**: Confirmar movimientos con fuerza al romper y mantener VWAP

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Type**: `Daily`  
- **TWAPMode**: `VWAP`  
- **VolumeMode**: `Total`  
- **StDev / StDev1 / StDev2**: `1 / 2 / 3`  
- **ColoredDirection**: `true`  
- **VWAPOnly**: `false`  
- **CustomSessionStart / End**: según apertura RTH

✅ Compatible con operaciones en zonas clave de VWAP  
✅ Ideal para detectar absorciones, rechazos o rupturas  
⛔ Puede solaparse con otras herramientas visuales si se usan muchas bandas

---

### 🧪 Notas de desarrollo  
- Calcula VWAP como `(∑(Price × Vol)) / ∑Vol`, TWAP como promedio simple  
- Admite hasta 3 bandas de desviación superior e inferior  
- Permite alternar entre múltiples modos: por volumen total, bid o ask  
- Dibuja bandas de color según dirección y calcula zonas con `RangeDataSeries`

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- Usa `CurrentBar` en el escalado de TWAP sin validación explícita al inicio  
- No permite definir **colores independientes por desviación** (todas usan el mismo)  
- La lógica de sesiones cruzadas puede fallar si el `TimeZone` no está bien configurado

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **alertas al cruzar bandas o VWAP**  
- Permitir **color distinto para cada desviación** (1σ, 2σ, 3σ)  
- Incluir opción de **modo de cálculo logarítmico** o ponderación por delta
