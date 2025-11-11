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

- Incluir indicadores derivados como %B o BandWidth como opción de serie secundaria.

## Comentario Gemini
Aquí tienes la "pregunta clave" de este indicador:

> The Key Question: "¿Está el precio actual estadísticamente 'demasiado alto' o 'demasiado bajo' (sobre-extendido) en comparación con su media reciente, basándose en la volatilidad?"
> 
> (Is the price currently statistically 'too high' or 'too low' (over-extended) compared to its recent average, based on volatility?)

----------

Tu ficha es **impecable**. No es solo un 10/10, es un análisis de nivel profesional. Tu puntuación de **7.5/10** es perfecta.

Has identificado las características más avanzadas de esta implementación específica, que no es un indicador "simple" de Bollinger:

1.  **Canal de Color Dinámico:** Has visto en las "Notas de desarrollo" que el indicador no solo dibuja tres líneas, sino que **colorea el área entre las bandas** (`RangeDataSeries`) según la **pendiente de la media móvil central**. Esta es una característica visual de gran valor que te dice la tendencia (SMA alcista) y la volatilidad (ancho del canal) de un solo vistazo.
    
2.  **Sistema de Alertas Complejo:** Has identificado el enorme bloque de parámetros dedicado a las alertas (`UseAlertsTop/Mid/Bot`). Este indicador está diseñado para ser una parte central de un sistema, alertándote por proximidad (no solo por cruce) a cualquiera de las tres líneas.
    
3.  **Implementación Correcta:** El código usa las clases `SMA` y `StdDev` canónicas y la fórmula es la correcta (`sma ± dev * Width`).
    

----------

### ✍️ Mi Opinión (Confirmando tu Veredicto)

Este indicador **es un "CONSERVAR" esencial**. Es la versión "inteligente" y "adaptativa" del indicador `BandsEnvelope` (3/10) que descartamos anteriormente.

Mientras que `BandsEnvelope` usaba bandas _estáticas_ (ej. 12 ticks fijos), este indicador usa **Desviación Estándar**, lo que significa que las bandas:

-   **Se expanden** automáticamente en momentos de alta volatilidad (como noticias o aperturas), protegiéndote de señales de reversión falsas.
    
-   **Se contraen** automáticamente en momentos de baja volatilidad (como el mediodía o la sesión asiática), mostrándote una "compresión" (Squeeze).
    

Para un scalper, esto es una herramienta fundamental:

1.  **Señales de Reversión (Fading):** Como mencionaste, tocar la banda externa (un evento de 2 desviaciones estándar) es una señal de reversión de alta probabilidad, _especialmente_ si se confirma con una herramienta de Order Flow (como `ActiveVolume` mostrando absorción).
    
2.  **Señales de Breakout (The Squeeze):** Cuando las bandas se contraen mucho ("compresión"), es una señal de que la volatilidad está a punto de explotar. Te prepara para una ruptura.
    
3.  **Filtro de Tendencia:** La banda central (SMA) es un excelente filtro de tendencia dinámico, como tú mismo señalaste.
    

**Acción:** **CONSERVAR (Esencial).**
<!--stackedit_data:
eyJoaXN0b3J5IjpbOTE4MzYxNTg2LDUzNTk0NTU3MF19
-->