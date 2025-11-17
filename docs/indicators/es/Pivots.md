## 🟦 Pivots (8/10)

**Nombre del archivo:** `Pivots.cs`  
**Nombre del indicador:** Pivots  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602446](https://help.atas.net/support/solutions/articles/72000602446)

---

### ⚙️ Parámetros configurables

- **PivotRange**: Periodicidad del cálculo (M1, M5, H1, D1, Semanal, Mensual, etc.)  
- **ThirdFormula**: Fórmula para calcular R3/S3 (estándar o alternativa)  
- **UseCustomSession / SessionBegin / SessionEnd**: Activar sesión personalizada  
- **RenderPeriodsFilter**: Número de sesiones a mantener visibles  
- **ShowText / FontSize / TextLocation**: Mostrar etiquetas de niveles y su estilo

---

### 🧭 Clasificación
📂 Level — Cálculo de niveles clásicos de pivote (PP, R1–R3, S1–S3, intermedios)

---

### 🧠 Uso más frecuente

- Trazar **niveles horizontales clásicos** (PP, soporte, resistencia)  
- Confirmar zonas clave de giro, ruptura o test estructural  
- Complementar análisis técnico con niveles objetivos históricos

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Indicador robusto y clásico para estructurar el gráfico  
✅ Soporta múltiples marcos y sesiones personalizadas  
⛔ Requiere validación visual: no todos los niveles son operables

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por rebote** en R1/S1 si hay rechazo y volumen  
- **Confirmación de ruptura** si se supera PP con intención  
- **StopLoss técnico** si se invalida un soporte/resistencia estructural

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **PivotRange**: `Daily`  
- **UseCustomSession**: `true`  
- **SessionBegin**: `09:30`  
- **SessionEnd**: `16:00`  
- **ShowText**: `true`  
- **RenderPeriodsFilter**: `3`

✅ Compatible con estructuras institucionales, trampas y test  
✅ Permite filtrar niveles válidos recientes  
⛔ Demasiadas líneas pueden saturar el gráfico si no se filtran

---

### 🧪 Notas de desarrollo

- Calcula PP, R1–R3, S1–S3 y niveles intermedios M1–M4  
- Admite dos fórmulas para el cálculo de R3/S3 (`HighLow`, `PpHighLow`)  
- Utiliza `ValueDataSeries` para cada línea con control individual de color  
- Soporta sesiones personalizadas y filtros para mantener solo X sesiones  
- Dibuja etiquetas con nombre del nivel y color sincronizado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El control de sesiones personalizadas (`IsNewCustomSession`) es complejo y puede fallar si la vela de inicio no cubre toda la franja  
- No valida si `SessionBegin > SessionEnd` en sesiones cruzadas → puede producir comportamiento inesperado  
- El valor inicial de `_prevDayClose` es 0, lo que puede sesgar el cálculo de PP en la primera barra  
- La lógica de etiquetas no comprueba superposición ni repetición de valores  
- Se borran datos antiguos en bloque sin asegurar que haya suficientes velas intermedias (puede crear "huecos")

---

### 🛠️ Propuestas de mejora

- Validar y simplificar la lógica de sesiones personalizadas  
- Usar cierre real si `prevDayClose == 0` en la primera barra de forma explícita  
- Añadir opción de mostrar solo ciertos niveles (ej: PP, R1 y S1)  
- Incluir alertas visuales o sonoras si el precio cruza un nivel clave  
- Permitir colorear los niveles según si fueron ya tocados o no

