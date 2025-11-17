## 🟦 QQE (Qualitative Quantitative Estimation) (7/10)

**Nombre del archivo:** `QQE.cs`  
**Nombre del indicador:** Qualitative Quantitative Estimation  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602629](https://help.atas.net/support/solutions/articles/72000602629)

---

### ⚙️ Parámetros configurables

- **RsiPeriod**: Periodo del RSI base (por defecto: 14)  
- **SlowFactor**: Periodo del suavizado del RSI (por defecto: 5)  
- **UseAlerts**: Activar alertas al cruce del nivel objetivo  
- **AlertFile**: Archivo de sonido de la alerta

---

### 🧭 Clasificación
📂 Momentum — Oscilador de impulso suavizado basado en RSI y ATR del RSI

---

### 🧠 Uso más frecuente

- Confirmar la **dirección y estabilidad del impulso**  
- Detectar **cambios de fase** en la tendencia mediante cruce de líneas  
- Usar como **sistema de alertas suaves** cuando se alcanza una zona objetivo

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Mejora del RSI con control de ruido y señal más estable  
✅ Indicador visual con buen comportamiento en tendencias  
⛔ Puede ser difícil de interpretar sin conocer la lógica del QQE

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro direccional** cuando la línea QQE está por encima/por debajo del nivel 50  
- **Confirmación de impulso** tras rebote en la línea de referencia  
- **Alertas por cruce de línea** como señal de continuación o agotamiento

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **RsiPeriod**: `10`  
- **SlowFactor**: `4`  
- **UseAlerts**: `true`  
- **LineSeries[0].Value**: `50` (nivel objetivo)

✅ Buen equilibrio entre sensibilidad y filtrado  
✅ Alertas visuales y sonoras para decisiones rápidas  
⛔ Ligeramente más lento que RSI puro por capas de suavizado

---

### 🧪 Notas de desarrollo

- Usa un RSI base con suavizado adicional (EMA) → `_rsiMa`  
- Calcula ATR del RSI suavizado y aplica doble EMA  
- Calcula umbral dinámico (`dar`) como `ema(atr(rsi_smooth)) × 4.236`  
- Traza línea dinámica `_trLevelSlow` que se ajusta al comportamiento del RSI suavizado  
- Lanza alertas cuando se cruza el nivel 50 en la dirección contraria a la vela previa

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El valor del cruce de alerta está fijo en `LineSeries[0].Value = 50` y no puede modificarse desde la UI  
- La lógica de inicialización en `bar == 0` no establece valores explícitos en las series  
- La alerta depende del valor en `bar - 2` y `bar - 1` sin tolerancia ni control de ticks mínimos  
- No se permite visualizar el ATR del RSI ni sus valores individuales como series separadas  
- El multiplicador `4.236` está hardcoded y no se puede ajustar por el usuario

---

### 🛠️ Propuestas de mejora

- Permitir modificar el nivel objetivo directamente desde la interfaz  
- Exponer el multiplicador `4.236` como parámetro configurable  
- Añadir opción para mostrar series auxiliares (ATR del RSI, RSI suavizado)  
- Implementar coloración dinámica del fondo o línea según la pendiente  
- Incluir validación si `bar < max(Period, SlowFactor)` para evitar accesos incorrectos

