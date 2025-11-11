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
Aquí tienes la "pregunta clave" de este indicador:

> The Key Question: "Instead of just the 'Close', what is the internal average price (e.g., Median, Typical) of each individual bar?"
> 
> (¿En lugar de solo el 'Cierre', cuál es el precio promedio interno (ej. Mediana, Típico) de cada vela individual?)

----------
### ✍️ Mi Opinión sobre el Indicador

Este indicador es el ejemplo perfecto de una herramienta que es **100% redundante**.

**Me explico:**

-   Muchos otros indicadores (SMA, EMA, RSI, etc.) te permiten elegir _sobre qué_ se calculan. En la configuración de un SMA, puedes elegir `Source = Close`, `Source = Open`, `Source = Typical (HLC3)`, `Source = Median (HL2)`, etc.
    
-   Este indicador (`AveragePriceBar`) es, literalmente, un `SMA(1)` con la fuente cambiada. No es una _media móvil_, no suaviza nada, no tiene "período".
    
-   Es solo una línea que sigue el "Precio Típico" o el "Precio Mediano" de cada vela.
    

Como puedes ver en tu captura de pantalla, el resultado es una línea ruidosa que sigue al precio casi tick por tick, pero que no es ni el `High`, ni el `Low`, ni el `Close`.

### 📈 Veredicto: ¿Es útil para Scalping?

**No. Es un claro "Descartar".**

1.  **Añade Ruido Visual:** Es una línea nerviosa más en el gráfico que no te da información clara sobre la tendencia o el momentum.  
2.  **No Filtra Nada:** Al no ser una _media móvil_, no suaviza el precio.
3.  **Es Redundante:** Si quisieras el "Precio Típico", simplemente usarías una `EMA(20)` aplicada al "Precio Típico". Este indicador no aporta valor.
    

**Acción:** **Descartar.**
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTc2MzMzMDA2NF19
-->