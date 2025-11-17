## 🟦 Watermark (2 / 10)  
**Nombre del archivo:** `Watermark.cs`  
**Nombre del indicador:** Watermark  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602668](https://help.atas.net/support/solutions/articles/72000602668)

---

### ⚙️ Parámetros configurables  
- **TextColor**: Color del texto  
- **TextLocation**: Ubicación del texto en el gráfico (centro, esquina, etc.)  
- **HorizontalOffset / VerticalOffset**: Desplazamiento de la línea principal  
- **ShowInstrument / ShowPeriod**: Mostrar nombre del instrumento y periodo  
- **Font**: Fuente de la línea principal  
- **AdditionalText / AdditionalFont / AdditionalTextYOffset**: Texto adicional, fuente y desplazamiento vertical

---

### 🧭 Clasificación  
📂 Visualization — Indicador decorativo con información del gráfico

---

### 🧠 Uso más frecuente  
- Mostrar en pantalla el **nombre del instrumento y el timeframe** actual  
- Añadir un **texto personalizado** como firma, recordatorio, etiqueta o marca  
- Personalizar el diseño visual de la zona de trading con elementos estáticos

---

### 📊 Nivel de relevancia  
🔟 **2 / 10**  
✅ Útil como complemento visual en entornos multi-instrumento  
✅ Facilita capturas de pantalla informativas o sesiones de formación  
⛔ No aporta información técnica ni señales operativas

---

### 🎯 Estrategias de scalping donde se aplica  
⛔ No se aplica directamente a estrategias de scalping  
✅ Puede ser útil en **pantallas de control múltiples** para identificar activos rápidamente

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **ShowInstrument / ShowPeriod**: `true`  
- **TextLocation**: `TopLeft`  
- **Font.Size**: `36`  
- **TextColor**: `White`  
- **AdditionalText**: `"RTH Only"` (si aplica)  

✅ Aporta claridad organizativa en pantallas complejas  
⛔ No modifica ni influye en cálculos técnicos

---

### 🧪 Notas de desarrollo  
- No realiza cálculos en `OnCalculate`  
- Solo dibuja texto en `OnRender`, con control completo de posición y formato  
- Usa `RenderContext.DrawString` con medidas adaptativas  
- Puede mostrar **una o dos líneas** de texto, centradas o alineadas

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No ofrece preview del texto completo hasta que se aplica  
- No se adapta automáticamente al tamaño del gráfico o resolución  
- No permite vincular contenido dinámico (precio, fecha, delta…)

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **variables dinámicas** (ej: `{LastPrice}`, `{Delta}`)  
- Incluir **modo de visibilidad condicional** (por sesión, por timeframe, etc.)  
- Permitir **rotación del texto** o fondo difuminado estilo watermark real
