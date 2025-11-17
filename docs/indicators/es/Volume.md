## 🟦 Volume (9 / 10)  
**Nombre del archivo:** `Volume.cs`  
**Nombre del indicador:** Volume  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602498](https://help.atas.net/support/solutions/articles/72000602498)

---

### ⚙️ Parámetros configurables  
- **Input**: Fuente de datos (`Volume`, `Ticks`, `Asks`, `Bids`)  
- **UseFilter**: Activar filtro de volumen mínimo  
- **FilterValue**: Valor mínimo de volumen para pintar la barra  
- **FilterColor**: Color de la barra que supera el filtro  
- **UseVolumeAlerts**: Activar alertas por volumen  
- **AlertVolumeFile**: Archivo de sonido para alerta por volumen  
- **UseReverseAlerts**: Activar alertas por reversión (volumen vs vela)  
- **AlertReverseFile**: Archivo de sonido para alerta de reversión  
- **ShowMaxVolume**: Mostrar línea de volumen máximo  
- **HiVolPeriod**: Periodo de cálculo del volumen máximo  
- **LineColor**: Color de la línea de volumen máximo  
- **ShowVolume**: Mostrar etiquetas de volumen en clúster  
- **FontColor / Font / VolLocation**: Personalización de texto  
- **DeltaColored**: Colorear barras por delta  
- **PosColor / NegColor / NeutralColor**: Colores según delta

---

### 🧭 Clasificación  
📂 Volume — Volumen clásico por vela y tipo de transacción

---

### 🧠 Uso más frecuente  
- Visualizar volumen **por vela** según tipo de transacción (bid, ask, ticks, total)  
- Aplicar **filtros de volumen** para detectar velas significativas  
- Mostrar **alertas sonoras** por picos de volumen o condiciones de reversión

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  
✅ Altamente configurable y flexible según estilo operativo  
✅ Compatible con visualización en modo clúster y análisis de contexto  
⛔ Puede requerir ajustes precisos para no saturar el gráfico con datos

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de intención**: Validar si un movimiento está respaldado por volumen alto  
- **Absorción/reversión**: Detectar si el cuerpo de vela contradice al delta  
- **Clúster + volumen extremo**: Marcar zonas donde el volumen excede umbral y confirmar reacción

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Input**: `Volume`  
- **UseFilter**: `true`  
- **FilterValue**: `1500`  
- **FilterColor**: `LightBlue`  
- **UseReverseAlerts**: `true`  
- **HiVolPeriod**: `20`  
- **ShowVolume**: `true`  
- **DeltaColored**: `true`  
- **PosColor / NegColor**: `Green / Red`

✅ Proporciona contexto visual potente para zonas de volumen elevado  
✅ Útil en conjunto con clústeres y herramientas de absorción  
⛔ Requiere calibración manual en activos de baja liquidez

---

### 🧪 Notas de desarrollo  
- Usa datos de vela (`GetCandle(bar)`) para extraer volumen, bid, ask o ticks  
- Permite alertas por reversión delta vs cuerpo de vela y por volumen absoluto  
- Dibuja etiquetas directamente en el gráfico en modo clúster  
- Mide el volumen máximo dentro de un rango configurable (`HiVolPeriod`)

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- El cálculo de alerta de reversión no considera **cuerpo insignificante con gran delta**  
- No incluye opción de **suavizado del volumen** (ej. media móvil)  
- Las etiquetas pueden solaparse en modos clúster densos

---

### 🛠️ Propuestas de mejora  
- Añadir opción de **media móvil del volumen**  
- Permitir mostrar **delta junto al volumen** en la etiqueta  
- Implementar **condiciones compuestas de alerta** (volumen + delta, volumen + cuerpo)
