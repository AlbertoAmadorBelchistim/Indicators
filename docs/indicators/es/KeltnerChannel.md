## 🟦 Keltner Channel (8/10)

**Nombre del archivo:** `KeltnerChannel.cs`  
**Nombre del indicador:** Keltner Channel  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602574](https://help.atas.net/support/solutions/articles/72000602574)

---

### ⚙️ Parámetros configurables

- **Days**: Días hacia atrás para cortar la visualización de las líneas (por defecto: 20)  
- **Period**: Periodo para el cálculo del ATR y la media base (por defecto: 34)  
- **Koef**: Multiplicador aplicado al ATR para definir el canal (por defecto: 4.0)  
- **Alertas (Top / Mid / Bot)**: Opciones de activación, repetición, sensibilidad, color y sonido para cada banda del canal

---

### 🧭 Clasificación
📂 Volatility — Canal de volatilidad basado en ATR y media móvil

---

### 🧠 Uso más frecuente

- Delimitar rangos dinámicos de precio según la volatilidad  
- Identificar zonas de sobreextensión (ruptura de banda superior/inferior)  
- Activar alertas cuando el precio se acerca a los bordes del canal

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Canal adaptativo eficaz para analizar sobreextensiones  
✅ Las alertas permiten automatizar la vigilancia de eventos clave  
⛔ Requiere buena calibración del `Koef` para evitar bandas inútiles o excesivas

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión desde extremos del canal**: venta cerca de la banda superior, compra en la inferior  
- **Confirmación de ruptura** si el precio supera y se mantiene fuera del canal  
- **Entrada con alerta sonora** cuando el precio toca el límite del canal tras acumulación

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Days**: `0` (mostrar continuamente)  
- **Period**: `21`  
- **Koef**: `1.5`  
- **AlertSensitivityTop / Mid / Bot**: `1`  
- **RepeatAlert...**: según preferencia

✅ Canal sensible y dinámico en entorno volátil  
✅ Alertas útiles para entrada rápida tras test de banda  
⛔ Las rupturas en alta volatilidad pueden generar señales falsas

---

### 🧪 Notas de desarrollo

- Usa un canal basado en la fórmula: `SMA ± Koef × ATR`, ambos con mismo `Period`  
- Los valores se representan mediante líneas superior, media e inferior, y un `RangeDataSeries` sombreado  
- Se corta visualización según el número de sesiones (`Days`) configurado  
- Incluye alertas independientes para cada banda del canal, con parámetros personalizables

---

### ❗ Incoherencias o aspectos mejorables detectadas

- Se reutiliza `AlertFileTop` también para la alerta de la banda inferior (`Bot`) → puede lanzar el sonido incorrecto  
- No hay validación explícita si `Koef` es negativo, lo cual puede provocar inversión del canal  
- El `RangeDataSeries` se añade como índice 2, pero no está documentado visualmente en UI  
- No existe protección si `CurrentBar == 0` y `IsNewSession(0)` lanza error  
- Falta opción para usar `EMA` en lugar de `SMA` como media base, lo cual limitaría adaptabilidad

---

### 🛠️ Propuestas de mejora

- Separar el archivo de alerta para cada banda (actualmente `AlertFileTop` se usa en `Bot`)  
- Validar que `Koef` sea siempre positivo  
- Añadir tipo de media configurable (`SMA`, `EMA`, etc.)  
- Permitir mostrar etiquetas de precio en las bandas del canal  
- Mostrar de forma clara el canal visual en UI (leyenda, color diferenciado)

