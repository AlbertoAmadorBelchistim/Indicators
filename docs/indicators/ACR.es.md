## 🟦 Average Candle Range (5/10)

**Nombre del archivo:** `ACR.cs`  
**Nombre del indicador:** Average Candle Range  
**Web oficial:** [ATAS — Average Candle Range](https://help.atas.net/support/solutions/articles/72000602323)  
**Compatibilidad:** ATAS versión estable y superiores.

![ACR](../img/ACR.png)

---

### ⚙️ Parámetros configurables

- **IgnoreWicks**: Ignorar mechas; si está activado, se calcula solo la diferencia entre apertura y cierre.

---

### 🧭 Clasificación
📂 Volatility — Indicadores de rango promedio diario

---

### 🧠 Uso más frecuente

- Medir la **volatilidad promedio** de las velas por sesión
- Evaluar si el rango actual está **por encima o por debajo del promedio reciente**
- Filtrar operaciones si el mercado está demasiado estrecho o extendido

---

### 📊 Nivel de relevancia
🔟 **5 / 10**

✅ Muy útil para scalpers que requieren contexto sobre la volatilidad normal del mercado  
✅ Ayuda a anticipar contracciones o expansiones de rango  
⛔ No ofrece contexto direccional ni volumen

---

### 🎯 Estrategias de scalping donde se aplica

- **Contracción de rango**: identificar consolidaciones previas a ruptura
- **Rango excesivo**: evitar entradas cuando el rango ya excede el promedio
- **Validación post-entrada**: confirmar que el movimiento tiene fuerza si supera el rango medio

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **IgnoreWicks**: `true` si se desea filtrar mechas (movimientos no sostenidos), `false` si se desea captar todo el rango
- No requiere más configuración, ya que el promedio se adapta automáticamente a cada sesión

✅ Útil para definir condiciones de entrada y salida según la extensión del movimiento  
✅ Se puede combinar con delta, volumen o POC para mayor precisión  
⛔ No muestra niveles ni señales directas de entrada

---

### 🧪 Notas de desarrollo

- El indicador calcula el rango de cada vela y lo acumula en `_rangeSeries`.
- Si `IgnoreWicks` está activado, el rango es `|Open - Close|`, de lo contrario es `High - Low`.
- Se reinicia con cada nueva sesión (`IsNewSession(bar)`).
- El resultado visual se dibuja como histograma en `_renderSeries`, que muestra el **promedio del rango de velas desde el inicio de la sesión** hasta el punto actual.

---

### 🛠️ Propuestas de mejora

- Añadir opción para **promedio móvil deslizante** (ej. últimas 20 velas) en lugar de desde inicio de sesión
- Incluir una **línea de referencia horizontal** con el valor promedio actual para facilitar comparaciones visuales
- Permitir mostrar tanto el **rango actual** como el **promedio** superpuestos, para ver cuándo se excede
- Opción de alertas si el rango actual supera cierto múltiplo del promedio (ej. 1.5x)

---

### Comentario de Gemini

Aquí tienes la "pregunta clave" de este indicador:

**¿Cuál es el tamaño promedio de una vela en lo que va de día?**

El código implementa una lógica muy particular. La línea clave es esta:

```csharp
_renderSeries[bar] = _rangeSeries.CalcAverage(bar - _lastSession + 1, bar);
```

Como tú perfectamente has detectado en tus "Notas de desarrollo", esto calcula el **promedio de rango de todas las velas desde que empezó la sesión**.

  * A las 09:31 (vela 1 de la sesión), el promedio es solo de 1 vela.
  * A las 09:32 (vela 2), es el promedio de 2 velas.
  * A las 15:00, es el promedio de cientos de velas.

**El problema:** Esto significa que el valor del indicador por la tarde está "contaminado" por la volatilidad (o falta de ella) de la mañana. No te dice cuál es la volatilidad *reciente*, sino cuál es la volatilidad *promedio de todo el día hasta ahora*.

Tu captura de pantalla lo demuestra: el indicador empieza alto (el rango de la primera vela) y luego va bajando y estabilizándose a medida que se añaden más velas (de rango probablemente menor) al promedio.

-----



### 📈 ¿Es útil para Scalping en S\&P 500?


  * **Como está (Promedio de Sesión): 4/10.**

      * La utilidad es limitada. Te da un contexto de "cómo de volátil ha sido el día en promedio", pero no te dice nada sobre la volatilidad *ahora mismo*. Un scalper necesita saber la volatilidad de los últimos 10-20 minutos, no el promedio de las últimas 5 horas.

  * **Como está, pero con `IgnoreWicks = true`: 6/10.**

      * Esto se vuelve un "Tamaño de Cuerpo Promedio del Día". Es un filtro de "mercado de rango" bastante eficaz. Si el tamaño promedio del cuerpo de las velas del día es muy bajo, es un indicador claro de "chop" y una señal para no operar breakouts.

  * **Con TU MEJORA (Promedio Móvil Deslizante): 9/10.**

      * Si implementaras tu propia sugerencia, este indicador se convertiría en una herramienta **esencial**. Un ACR/ATR de período corto (ej. 20) es vital para el scalping:
        1.  **Filtro de Chop:** Si el ACR es muy bajo, no operes.
        2.  **Filtro de Clímax:** Si el ACR se dispara (p.ej., 2x-3x su valor normal), indica un posible clímax de agotamiento.
        3.  **Gestión de Stops:** Te permite fijar stops dinámicos (ej. 1.5x ACR).
        4.  **Gestión de Objetivos:** Te permite fijar objetivos realistas (ej. 2x ACR).

-----

### 🛠️ ¿Merece la pena arreglarlo?

El código base es bueno. Para convertirlo en la herramienta útil (9/10) que describo, solo necesitarías:

1.  Añadir un parámetro de `Periodo` (ej. `int Period = 20`).
2.  Añadir una instancia de SMA (ej. `private readonly SMA _rollingSma = new();`).
3.  Cambiar la línea de cálculo en `OnCalculate`:
      * *Quitar:* `_renderSeries[bar] = _rangeSeries.CalcAverage(bar - _lastSession + 1, bar);`
      * *Poner:* `_renderSeries[bar] = _rollingSma.Calculate(bar, _rangeSeries[bar]);`

Con ese cambio, este indicador pasaría de ser un "descarte" a ser una pieza central de un sistema de scalping.