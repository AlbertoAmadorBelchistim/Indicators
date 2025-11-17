## 🟦 Gaps (8.5/10)

**Nombre del archivo:** `Gaps.cs`  
**Nombre del indicador:** Gaps  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000618858](https://help.atas.net/support/solutions/articles/72000618858)

---

### ⚙️ Parámetros configurables

- **CloseGapsPartially**: Permitir cierre parcial del gap  
- **MinDeviation**: Mínima desviación (en % de rango promedio) para considerar un gap  
- **LimitMaxGapBodyLength**: Limitar la longitud máxima del gap en barras  
- **MaxGapBodyLengthFilter**: Número máximo de barras que puede durar un gap  
- **Transparency**: Transparencia del área de gap (0 a 10)  
- **BullishColor / BearlishColor**: Colores de los gaps alcistas y bajistas  
- **HideGaps / HideBorder**: Ocultar los gaps o sus bordes  
- **BorderWidth**: Grosor de las líneas de borde  
- **ShowLabel / LabelSize / LabelColor / LabelOffsetX / LabelOffsetY**: Opciones visuales para etiquetas  
- **UseAlerts / AlertFile / AlertForeColor / AlertBGColor**: Alertas al abrir o cerrar gaps

---

### 🧭 Clasificación  
📂 Levels — Identificación de huecos técnicos relevantes como niveles estructurales

---

### 🧠 Uso más frecuente

- Detectar **huecos de apertura** con impacto técnico o institucional  
- Marcar zonas donde el mercado dejó vacío de negociación  
- Generar alertas visuales y sonoras al **aparecer o cerrarse un gap**  
- Distinguir entre **gaps alcistas y bajistas**, con trazado y cierre parcial opcional

---

### 📊 Nivel de relevancia  
🔟 **8.5 / 10**

✅ Altamente visual e informativo en contextos de ruptura o reversión  
✅ Permite análisis táctico y visual sin sobrecargar el gráfico  
⛔ Sensible a configuración del rango o marco temporal  
⛔ No discrimina entre gaps normales y gaps con volumen institucional

---

### 🎯 Estrategias de scalping donde se aplica

- **Test del gap**: operar rebotes o rechazos en la zona del hueco  
- **Cierre parcial como señal de absorción**: detectar si un gap empieza a cerrarse sin completarse  
- **Entrada tras ruptura con gap abierto**: confirmar momentum si el gap permanece sin cerrar tras breakout  
- **Alerta táctica**: actuar cuando el precio se aproxima o cierra un gap reciente

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **MinDeviation**: `30`  
- **LimitMaxGapBodyLength**: `true`, con filtro en `100`  
- **CloseGapsPartially**: `true`  
- **Transparency**: `5`  
- **ShowLabel**: `true`, tamaño `10`  
- **UseAlerts**: `true`, archivo `"alert1"`

✅ Compatible con rupturas en apertura, zonas de absorción y huecos de estructura  
✅ Aporta claridad visual sin sobrecargar el análisis microestructural

---

### 🧪 Notas de desarrollo

- Evalúa gaps entre la vela actual y la anterior cuando:
  $$
  \text{Gap} = \text{Low}_t > \text{High}_{t-1} \quad \text{o} \quad \text{High}_t < \text{Low}_{t-1}
  $$
- El gap se valida si supera una desviación mínima sobre el rango promedio (SMA de 14)  
- Se construyen objetos `Box` para representar el área, y se agrupan en estructuras `Gap`  
- Los gaps se cierran automáticamente al tocarse o exceder su duración máxima  
- Soporta visualización por color, grosor, etiquetas y alertas configurables  
- Dibuja etiquetas con el marco temporal del gap si está activo `ShowLabel`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La variable `MaxGapBodyLength` sigue presente aunque marcada como `[Obsolete]`, lo cual puede inducir a errores si se modifica en entornos no actualizados  
- No hay opción para mostrar historial de gaps cerrados o gaps anteriores  
- No discrimina entre gaps con o sin volumen asociado (gap vacío vs. gap con ejecución)  
- Los colores de transparencia se recalculan con fórmulas internas fijas, sin opción visual avanzada

---

### 🛠️ Propuestas de mejora

- Eliminar o reemplazar completamente el parámetro `MaxGapBodyLength` obsoleto  
- Añadir opción para **mostrar gaps anteriores ya cerrados** con otro estilo  
- Integrar lógica para detectar **gaps con volumen** (gap válido institucional vs. técnico simple)  
- Añadir visualización de estadísticas: número de gaps abiertos, promedio de duración, etc.  
- Mostrar fecha/hora del inicio del gap en la etiqueta
