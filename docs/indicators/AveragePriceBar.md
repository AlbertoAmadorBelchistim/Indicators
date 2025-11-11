## 🟦 Average Price for Bar (4/10)

  

**Nombre del archivo:** `AveragePriceBar.cs`
**Nombre del indicador:** Average Price for Bar
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602324](https://help.atas.net/support/solutions/articles/72000602324)

  

---

  

### ⚙️ Parámetros configurables

  

- **CalcMode**: Método de cálculo del precio medio por vela (por defecto: Hlc3)
- `Hl2` = (High + Low) / 2
- `Hlc3` = (High + Low + Close) / 3
- `Ohlc4` = (Open + High + Low + Close) / 4
- `Hl2c4` = (High + Low + 2 × Close) / 4

  

---

  

### 🧭 Clasificación

📂 Price — Indicadores de media por vela

  

---

  

### 🧠 Uso más frecuente

- Calcular el **precio medio ponderado de una vela**
- Suavizar la lectura del precio sin usar medias móviles
- Complementar indicadores de tendencia o momentum

  

---

  

### 📊 Nivel de relevancia

🔟 **4 / 10** 

✅ Muy fácil de interpretar
✅ Aporta una media rápida de precio sin lag
⛔ No tiene lógica propia de señales, es auxiliar

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Confirmación rápida de dirección de vela**: comparar Close con la media de la vela
- **Filtro de microestructuras**: evitar operar contra la media interna de la vela
- **Soporte/Resistencia dinámica intra-barra**: observar reacciones repetidas sobre la media

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **CalcMode**: `Hl2c4`
✅ Pondera el cierre sin ignorar el rango completo
✅ Da valores más realistas en velas direccionales
⛔ Sensible a mechas largas en velas de reversión

  

---

  

### 🧪 Notas de desarrollo

  

- El cálculo se hace en `OnCalculate()` usando `GetCandle(bar)`.
- El resultado se guarda en `_renderSeries`, con color azul por defecto.
- La propiedad `CalcMode` dispara `RecalculateValues()` al cambiar.
- El valor retornado depende del modo seleccionado (switch expression).

  

### 🛠️ Propuestas de mejora

  

- Añadir opción para **dibujar la línea solo si el cuerpo de la vela supera cierto umbral**, para evitar ruido en velas doji o neutras.
- Incluir **color dinámico** según la pendiente del valor calculado (verde si sube, rojo si baja).
- Permitir **mostrar un canal o envolvente** alrededor del valor medio (± X ticks o %), útil como microsoporte/resistencia.
- Incorporar una opción para **cálculo dinámico ponderado por volumen**, no solo por OHLC.

  

✅ Estas mejoras aportarían contexto adicional sin comprometer el rendimiento
⛔ Algunas requieren datos adicionales (volumen, rango relativo)

## Comentario Gemini
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE2MzcwNDE5MDBdfQ==
-->