## 🟦 Volume-based Support & Resistance Zones (10 / 10)  
**Nombre del archivo:** `VolumeSupResZones.cs`  
**Nombre del indicador:** Volume-based Support & Resistance Zones  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000619397](https://help.atas.net/support/solutions/articles/72000619397)

---

### ⚙️ Parámetros configurables  
- **TimeFrameType1–4**: Timeframes independientes para generar zonas (M1 a Monthly)  
- **SmaPeriod1–4**: Periodo para la media del volumen en cada timeframe  
- **DisplayMode1–4**: Mostrar como zona, línea o desactivado  
- **SupColor / ResColor**: Color de soporte y resistencia (con transparencia configurable)  
- **ExtendPrevious / ExtendLast**: Extender zonas pasadas o la actual  
- **ShowTimeFrameLabel / LabelLocation / LabelTextSize**: Personalización de etiquetas  
- **ShowHLLines / ShowOCLines**: Mostrar líneas de máximo/mínimo y open/close de la zona  
- **LineWidth / LineStyle**: Estilo y grosor de las líneas  
- **UseAlert / AlertFile**: Activar alertas visuales/sonoras al crearse una nueva zona

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Zonas de soporte y resistencia dinámicas basadas en volumen por timeframe

---

### 🧠 Uso más frecuente  
- Detectar zonas de soporte o resistencia que surgen por **acumulación de volumen** significativo  
- Confirmar puntos clave para **rebote, ruptura o rechazo** según diferentes escalas temporales  
- Visualizar zonas relevantes con extensión y personalización avanzada

---

### 📊 Nivel de relevancia  
🔟 **10 / 10**  
✅ Combina **volumen, estructura y multitemporalidad**  
✅ Ideal para detectar zonas institucionales o de congestión  
⛔ Requiere calibración de timeframes y colores para evitar saturación

---

### 🎯 Estrategias de scalping donde se aplica  
- **Reversión desde zona relevante**: Entrada en rebote con volumen defendido  
- **Ruptura de zona**: Confirmación si se supera la resistencia con intención  
- **Validación multitemporal**: Entrar solo si la zona coincide en varios marcos temporales

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **TimeFrameType1**: `M15`  
- **TimeFrameType2**: `H1`  
- **SmaPeriod1–2**: `6`  
- **DisplayMode1–2**: `Zone`  
- **ExtendLast / ExtendPrevious**: `true`  
- **UseAlert**: `true`  
- **ShowHLLines / ShowOCLines**: `true`  
- **ZoneTransparency**: `3`

✅ Permite detectar zonas clave que no se ven en el timeframe actual  
✅ Compatible con estrategias de reacción o rompimiento sobre clúster  
⛔ Alto número de zonas puede saturar visualmente en velas rápidas

---

### 🧪 Notas de desarrollo  
- Cada timeframe genera zonas a partir de patrones de reversión con volumen creciente  
- Las zonas se dibujan como **rectángulos o líneas** según configuración, y pueden extenderse  
- El cálculo distingue entre zonas de soporte y resistencia, y permite hasta 4 timeframes  
- Se emite alerta si aparece una nueva zona al cerrarse el patrón

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- Usa `CurrentBar` sin protección al inicio en zonas de reversión → riesgo de error en primeros cálculos  
- No permite limitar cuántas zonas se mantienen en pantalla  
- No admite alternar entre detección automática o manual de la zona (solo lógica fija)

---

### 🛠️ Propuestas de mejora  
- Incluir opción para **mostrar solo zonas activas** o más recientes  
- Añadir posibilidad de **activar/desactivar timeframes individualmente**  
- Permitir **guardar zonas históricas** o generar archivo externo para análisis estadístico
