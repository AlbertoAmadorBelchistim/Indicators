## 🟦 Volume On The Chart (8 / 10)  
**Nombre del archivo:** `VolumeOnChart.cs`  
**Nombre del indicador:** Volume On The Chart  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000619334](https://help.atas.net/support/solutions/articles/72000619334)

---

### ⚙️ Parámetros configurables  
- **Height**: Altura del área de visualización del volumen como porcentaje del panel de velas (por defecto: `15`)  
- **ShowVolume**: Mostrar etiquetas de volumen en modo clúster (controlado internamente)  
- **VolLocation**: Posición vertical del texto del volumen (`Up`, `Center`, `Down`)  
- **Font / FontColor**: Fuente y color del texto del volumen (usados internamente)  
- **Input**: Fuente de datos (`Volume` o `Ticks`)  

---

### 🧭 Clasificación  
📂 Volume — Visualización directa del volumen sobre el gráfico de velas

---

### 🧠 Uso más frecuente  
- Mostrar el **volumen o número de transacciones** de cada vela sobre el gráfico  
- Comparar fácilmente la **intensidad de participación por vela** sin panel adicional  
- Visualizar el volumen directamente sin distraerse del contexto de precio

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Muy visual y directo para confirmar si el movimiento está respaldado por volumen  
✅ Compatible con análisis de clúster y volumen sin saturar el gráfico  
⛔ Requiere ajustar altura y color si se superpone a elementos importantes

---

### 🎯 Estrategias de scalping donde se aplica  
- **Validación de ruptura**: Ver si el movimiento está acompañado por volumen creciente  
- **Rechazo/absorción**: Detectar velas de gran volumen con cierre contrario  
- **Segmentación intrabar**: Visualizar diferencias entre volumen y ticks para evaluar agresividad

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Height**: `15`  
- **Input**: `Volume`  
- **VolLocation**: `Center`  
- **FontColor**: `White`

✅ Ideal para seguimiento de volumen por vela sin ocupar otro panel  
✅ Compatible con visualización de delta y clúster simultáneamente  
⛔ Puede dificultar lectura si hay demasiadas etiquetas o texto pequeño

---

### 🧪 Notas de desarrollo  
- Dibuja un **rectángulo de volumen proporcional** en cada vela usando `RenderContext`  
- Calcula el volumen máximo visible para escalar proporcionalmente  
- El texto de volumen se muestra opcionalmente, y solo en modo de clúster  
- No usa panel propio: el gráfico se dibuja **sobre el panel de velas**

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No permite cambiar el color de los rectángulos desde la UI  
- Las etiquetas de texto solo se muestran si está activado el modo clúster  
- No se puede alternar entre `Volume` y `Ticks` desde la interfaz (requiere código)

---

### 🛠️ Propuestas de mejora  
- Permitir **cambiar colores de volumen directamente** desde la interfaz  
- Incluir opción para **mostrar volumen como histograma transparente bajo la vela**  
- Añadir compatibilidad con **delta u otras métricas personalizadas**
