## 🟦 Bollinger Bands (7.5/10)

  

**Nombre del archivo:**  `BollingerBands.cs`

**Nombre del indicador:** Bollinger Bands

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602339](https://help.atas.net/support/solutions/articles/72000602339)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Número de velas para el cálculo del SMA y la desviación estándar

- **Width**: Multiplicador aplicado a la desviación estándar

- **Shift**: Desplazamiento horizontal de las bandas

- **UseAlertsTop / Mid / Bot**: Activar alertas por aproximación a cada banda

- **AlertSensitivityTop / Mid / Bot**: Sensibilidad de alerta (en ticks)

- **RepeatAlertTop / Mid / Bot**: Permitir alertas repetidas o únicas por barra

- **AlertFileTop / Mid / Bot**: Archivo de sonido para cada tipo de alerta

- **FontColor / BackgroundColor**: Colores del texto y fondo de la alerta

  

---

  

### 🧭 Clasificación

📂 Volatility / Channels — Bandas dinámicas basadas en desviación estándar

  

---

  

### 🧠 Uso más frecuente

  

- Delimitar **zonas de volatilidad relativa**

- Detectar **contracciones y expansiones** del rango de precios

- Confirmar setups de reversión o breakout mediante contacto con las bandas

- Usar como filtro direccional si el precio permanece fuera de la banda media

  

---

  

### 📊 Nivel de relevancia

🔟 **7.5 / 10**

  

✅ Clásico y ampliamente usado en todos los marcos temporales

✅ Flexible por sus alertas, desplazamiento y representación visual

⛔ Sensible a valores extremos si no se ajusta correctamente el ancho (Width)

⛔ No incluye información sobre volumen ni delta

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Reversión en banda extrema**: entrada cuando el precio toca banda superior/inferior con confirmación

- **Compresión-expansión**: operar breakout tras contracción visible del canal

- **Entrada con filtro**: solo operar si el precio está por encima/por debajo de la banda media (tendencia)

- **Alertas dinámicas**: disparar eventos cuando el precio se acerca a zonas clave

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `20`

- **Width**: `2`

- **Shift**: `0`

- **UseAlertsTop / Mid / Bot**: `true`

- **AlertSensitivity**: `1` (1 tick)

- **AlertFile**: `"alert1"`

- **FontColor**: blanco

- **BackgroundColor**: gris medio

  

✅ Configuración clásica para detectar presión o extremos de corto plazo

✅ Combinable con volumen o delta para validar absorciones o rupturas

⛔ Evitar usar en entornos laterales muy volátiles

  

---

  

### 🧪 Notas de desarrollo

  

- Calcula SMA (`Period`) y desviación estándar (`StdDev`)

- Dibuja tres líneas: superior, media (SMA) e inferior

- Las bandas se desplazan con `Shift`, incluso en negativo (adelanto)

- Colorea el área entre bandas según la dirección del SMA actual

- Soporta hasta tres sistemas independientes de alerta con sensibilidad y color personalizados

- Utiliza internamente series de tipo `RangeDataSeries` y `ValueDataSeries`

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir opción para **mostrar la pendiente del canal con color dinámico**

- Incluir un modo de **relleno gradiente** según intensidad de desviación

- Posibilidad de **exportar señales de alerta a archivo externo o API**

- Incluir indicadores derivados como %B o BandWidth como opción de serie secundaria
<!--stackedit_data:
eyJoaXN0b3J5IjpbNTM1OTQ1NTcwXX0=
-->