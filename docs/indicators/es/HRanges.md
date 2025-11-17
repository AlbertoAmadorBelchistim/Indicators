## 🟦 HRanges (8.5/10)

**Nombre del archivo:** `HRanges.cs`  
**Nombre del indicador:** HRanges  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602573](https://help.atas.net/support/solutions/articles/72000602573)

---

### ⚙️ Parámetros configurables

- **Days**: Número de sesiones hacia atrás desde las que comenzar a calcular rangos (por defecto: 20)  
- **VolumeFilter**: Volumen mínimo para mostrar el nivel de máximo volumen  
- **BarsRange**: Número mínimo de barras para validar la zona como rango significativo  
- **HideAllVolume / HideAllBarsFilter**: Ocultar zonas si no se cumplen los filtros  
- **SwingUpColor / SwingDnColor / NeutralColor / VolumeColor**: Colores para rangos alcistas, bajistas, neutros y POC  
- **Width**: Grosor de las líneas visuales del rango

---

### 🧭 Clasificación  
📂 Volume — Rangos validados por volumen por nivel, no por agresión

---

### 🧠 Uso más frecuente

- Detectar **zonas de consolidación** o balance del mercado  
- Confirmar rompimientos desde rangos con acumulación previa  
- Identificar **POC del rango** y clasificarlo como alcista, bajista o neutro  
- Visualizar rangos históricos con codificación visual y filtros dinámicos

---

### 📊 Nivel de relevancia  
🔟 **8.5 / 10**

✅ Lógica robusta para análisis de rangos con filtro por volumen y barras  
✅ Detección estructurada de zonas clave sin necesidad de intervención manual  
⛔ Visualización compleja si hay muchos rangos consecutivos  
⛔ Requiere ajuste cuidadoso de filtros para evitar ruido o sobrepoblación de datos

---

### 🎯 Estrategias de scalping donde se aplica

- **Rompimiento confirmado**: entrada si el precio sale del rango con agresión y volumen creciente  
- **Reversión en borde de rango**: si aparece absorción en la parte superior/inferior  
- **Confirmación estructural**: operar solo si el contexto muestra rango claro y reciente  
- **POC como test táctico**: validación de rechazo o aceptación en el volumen dominante

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Days**: `3`  
- **BarsRange**: `10`  
- **VolumeFilter**: `500`  
- **HideAllBarsFilter / HideAllVolume**: `true`  
- Colores: verde (alcista), rojo (bajista), gris (neutro), azul (POC)

✅ Ideal para definir zonas de trabajo  
✅ Compatible con Delta, DOM y zonas de absorción

---

### 🧪 Notas de desarrollo

- Detecta rango cuando:
  - El precio no continúa tras una ruptura aparente (por Close > High o < Low anteriores)  
  - Se acumulan al menos `BarsRange` dentro del mismo rango H/L  
- Cada rango se clasifica como:
  - **Up**: ruptura hacia arriba tras rango  
  - **Down**: ruptura hacia abajo  
  - **Flat**: rango aún activo o sin ruptura
- Calcula el nivel de **mayor volumen (POC)** dentro del rango y lo pinta si supera `VolumeFilter`  
- Usa un sistema interno de caché por barra para evitar múltiples llamadas a `GetAllPriceLevels()`  
- Visualiza todos los niveles con `VisualMode.Hash`, ocultos si se aplican filtros

---

### ❗ Incoherencias o aspectos mejorables detectadas

- En `RenderLevel()`, la limpieza por filtro puede sobrescribir datos previos sin conservar históricos  
- Los rangos neutros se actualizan incluso si no hay volumen suficiente, lo que puede generar ruido visual  
- No hay opción para extender los rangos más allá de su duración activa ni para guardar rangos anteriores  
- Los colores se asignan globalmente, sin opción de codificación por volumen o duración relativa

---

### 🛠️ Propuestas de mejora

- Añadir opción para extender el rango visual hasta que se rompa explícitamente  
- Guardar rangos históricos cerrados en estructuras separadas  
- Permitir mostrar el nombre o ID del rango en etiquetas flotantes  
- Incluir estadísticas por rango: duración, altura, volumen total, etc.  
- Añadir alertas al romper el rango o al tocar el POC interno
