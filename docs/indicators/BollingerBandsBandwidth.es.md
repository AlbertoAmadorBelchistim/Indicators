## 🟦 Bollinger Bands: Bandwidth (6.5/10)

  

**Nombre del archivo:**  `BollingerBandsBandwidth.cs`

**Nombre del indicador:** Bollinger Bands: Bandwidth

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602340](https://help.atas.net/support/solutions/articles/72000602340)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Periodo de cálculo para la SMA y la desviación estándar

- **Width**: Multiplicador aplicado a la desviación estándar (define amplitud de las bandas)

  

---

  

### 🧭 Clasificación

📂 Volatility — Medición relativa de la amplitud del canal de Bollinger

  

---

  

### 🧠 Uso más frecuente

  

- Medir la **ancho relativo del canal de Bollinger**

- Detectar **compresiones previas a movimientos explosivos**

- Confirmar cambios de régimen de volatilidad

- Usar como filtro para estrategias de breakout

  

---

  

### 📊 Nivel de relevancia

🔟 **6.5 / 10**

  

✅ Lectura directa y cuantitativa de la volatilidad relativa

✅ Ideal como disparador para estrategias de expansión

⛔ Requiere contexto adicional (precio o volumen) para interpretar correctamente

⛔ No indica dirección ni momentum

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Breakout tras compresión**: entrar cuando el Bandwidth sube bruscamente tras mínimos locales

- **Evitar entradas en alta compresión**: si el Bandwidth está en mínimos extremos

- **Confirmación de volatilidad**: combinar con ATR para validar entorno activo

- **Filtrar señales**: evitar setups si el ancho no supera cierto umbral

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `20`

- **Width**: `2`

  

✅ Detecta eficazmente estrechamientos extremos del canal

✅ Compatible con estrategias de breakout en velas de 1 minuto

⛔ No genera señales por sí mismo, solo contexto

  

---

  

### 🧪 Notas de desarrollo

  

- Internamente usa un indicador `BollingerBands` para obtener los valores superior, inferior y SMA

- Calcula: `(Upper - Lower) / SMA * 100` → valor porcentual del ancho relativo

- Usa una única serie (`RenderSeries`) representada como línea o histograma

- Requiere que el SMA no sea cero para evitar división indefinida

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir **alerta por cruce de umbrales configurables**

- Visualización condicional por colores según expansión/contracción

- Opcional: línea de referencia con la media móvil del ancho

- Soporte para exportar señales de compresión extrema
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE3MTY2OTI3NTldfQ==
-->