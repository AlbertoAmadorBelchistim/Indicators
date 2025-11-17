## 🟦 KD - Fast (6/10)

**Nombre del archivo:** `KdFast.cs`  
**Nombre del indicador:** KD - Fast  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602411](https://help.atas.net/support/solutions/articles/72000602411)

---

### ⚙️ Parámetros configurables

- **PeriodK**: Periodo para el cálculo de la línea %K (por defecto: 10)
- **PeriodD**: Periodo para la media móvil de %K, que genera la línea %D (por defecto: 10)

---

### 🧭 Clasificación
📂 Momentum — Oscilador estocástico rápido basado en %K y su media móvil %D

---

### 🧠 Uso más frecuente

- Identificar condiciones de sobrecompra y sobreventa en movimientos rápidos  
- Generar señales de entrada cuando %K cruza %D  
- Detectar divergencias con el precio para anticipar posibles giros

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Reacciona con rapidez a cambios de dirección  
✅ Útil en rangos y consolidaciones para detectar puntos extremos  
⛔ Genera muchas señales falsas en entornos tendenciales sin filtros adicionales

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce de líneas %K y %D** como señal de entrada/salida  
- **Confirmación de giros** cuando %K rebota en zonas extremas (20/80)  
- **Divergencias rápidas** en scalping de reversión sobre niveles clave

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **PeriodK**: `8`  
- **PeriodD**: `3`  

✅ Esta configuración da señales más rápidas para scalping de corto plazo  
✅ Mejor en rangos que en fuertes tendencias  
⛔ Mayor probabilidad de señales falsas sin filtro de tendencia

---

### 🧪 Notas de desarrollo

- La línea %K se calcula como posición relativa del cierre respecto al mínimo/máximo del periodo  
- La línea %D es una media simple de la línea %K, definida mediante `SMA`  
- Se usan los indicadores auxiliares `Lowest`, `Highest` y `SMA` internos a ATAS  
- El panel está separado (`NewPanel`) y las dos series se dibujan por separado (`_kSeries`, `_dSeries`)

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay validación explícita para evitar división por cero en `(high - low)`, aunque se mitiga con un ternario (`? :`)  
- No se incluye ningún tipo de suavizado para %K, lo que puede resultar en excesivo ruido para ciertos estilos de scalping  
- No ofrece niveles visuales estándar (como líneas horizontales en 20 / 80) para facilitar interpretación  
- El usuario no puede elegir el tipo de media para %D (solo usa `SMA`), lo cual limita su personalización

---

### 🛠️ Propuestas de mejora

- Añadir opción para elegir el tipo de media en la línea %D (por ejemplo, `EMA`, `SMMA`)  
- Permitir mostrar automáticamente niveles de sobrecompra/sobreventa (por ejemplo, líneas en 20 y 80)  
- Incluir alerta visual/sonora cuando %K cruce %D o cuando ambas líneas salgan de zonas extremas  
- Añadir opción de suavizado adicional sobre %K antes de calcular %D, como ocurre en versiones slow del estocástico

