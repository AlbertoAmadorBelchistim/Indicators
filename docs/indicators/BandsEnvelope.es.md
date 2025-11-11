## 🟦 Bands / Envelope (3/10)

  

**Nombre del archivo:** `BandsEnvelope.cs`

**Nombre del indicador:** Bands / Envelope

**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602522](https://help.atas.net/support/solutions/articles/72000602522)

  

---

  

### ⚙️ Parámetros configurables

  

- **CalcMode**: Modo de cálculo del rango (Percentage / Value / Ticks)

- **RangeFilter**: Rango de desviación respecto al precio (solo para Value y Ticks)

  

---

  

### 🧭 Clasificación

📂 Volatility — Indicadores de bandas alrededor del precio

  

---

  

### 🧠 Uso más frecuente

  

- Visualizar **envolventes de precio** con distintos métodos de cálculo (porcentual, valor fijo o en ticks)

- Identificar zonas de sobreextensión o sobrecompra/sobreventa

- Estimar **canales de rango dinámico** para sistemas basados en reversión

  

---

  

### 📊 Nivel de relevancia

🔟 **3 / 10**

  

✅ Versátil y compatible con distintos enfoques (ticks, porcentaje o valor)

✅ Útil para sistemas de reversión simples

⛔ No considera volatilidad implícita ni volumen

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Reversión al centro**: entrada cuando el precio toca la banda superior/inferior y hay señales de agotamiento

- **Confirmación de breakout**: rotura sostenida de la banda con volumen creciente

- **Canal de referencia**: combinación con otros indicadores para definir zonas de entrada/salida

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **CalcMode**: `Ticks`

- **RangeFilter**: `12` (aprox. 3 puntos o 12 ticks)

  

✅ Permite detectar sobreextensiones de corto plazo

⛔ No reacciona a cambios de volatilidad si no se ajusta manualmente

  

---

  

### 🧪 Notas de desarrollo

  

- El indicador calcula bandas simétricas alrededor del precio actual según el modo seleccionado:

- **Percentage**: `(± range % del precio)`

- **Value**: `(± valor absoluto)`

- **Ticks**: `(± valor en ticks del instrumento)`

- El área entre bandas se representa mediante un `RangeDataSeries` con líneas superior e inferior

- El cálculo se actualiza en tiempo real para cada vela nueva

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir **media central** entre bandas (tipo media móvil) para análisis de reversión

- Incluir **opción de suavizado** para evitar repintado excesivo en entornos volátiles

- Posibilidad de activar **alertas cuando el precio cruza una banda**

- Soporte para **desplazamiento horizontal** de las bandas para backtesting visual

### Comentario Gemini
Veo que ha habido un pequeño error en este envío. Me has pasado el código de `BalanceOI.cs` (el que acabamos de analizar), pero la **imagen** y la **ficha** que has preparado son para un indicador completamente nuevo: **Bands / Envelope**.

Ignoraré el código duplicado y me centraré en analizar tu nueva ficha y la imagen, que es lo que corresponde.

Tu análisis de este nuevo indicador es, de nuevo, **totalmente correcto**. Tu puntuación de 4/10 es la clave.

Aquí está la "pregunta clave" de este indicador:

> The Key Question: "How far can the price move (in fixed ticks or percentage) before it's considered 'over-extended' from its current level?"
> 
> (¿Cuán lejos puede moverse el precio (en ticks fijos o porcentaje) antes de considerarse 'sobre-extendido' de su nivel actual?)

----------

### ✍️ Mi Opinión sobre tu Ficha y el Indicador

Tu ficha es perfecta y no necesita corrección. Has identificado el propósito (reversión a la media), los modos de cálculo (Ticks, %, Valor) y, lo más importante, su **defecto fatal**.

Tu puntuación de **4/10** es perfecta. Yo le daría incluso un **3/10**.

**El porqué de la nota baja (El Defecto Fatal):**

Tu propia "Nivel de relevancia" lo dice todo:

> ⛔ No considera volatilidad

Este es el clavo en el ataúd. Este indicador dibuja bandas **estáticas**. Tu "Parametrización óptima" de **12 ticks** (3 puntos del S&P 500) es un ejemplo perfecto:

-   **En un mercado lento** (ej. sesión asiática), el precio ni siquiera tocará esas bandas. Son inútiles.
    
-   **En un evento de noticias** (ej. FOMC o NFP), el precio volará 50 ticks (12.5 puntos) en un segundo, rompiendo tus bandas de 12 ticks y haciéndote perder dinero si intentas una "reversión".
    

Este indicador es una versión "tonta" (estática) de indicadores "inteligentes" (adaptativos) como las **Bandas de Bollinger** (que se adaptan a la Desviación Estándar) o los **Canales de Keltner** (que se adaptan al ATR).

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**No. Es un claro "Descartar".**

Un scalper moderno _vive_ de la volatilidad. Una herramienta que no se adapta a los cambios de volatilidad no solo es inútil, sino que es **peligrosa**.

-   El **ATR** (que ya conservamos) es una herramienta 10 veces superior para medir la extensión.
    
-   El **AMA (Kaufman)** (que ya conservamos) es 10 veces superior para definir un "canal" de tendencia/rango.
    

Tu análisis ha sido impecable al identificar una herramienta de bajo valor.

**Acción:** **Descartar.**

Gracias por enviar el código.

Mi análisis anterior fue 100% correcto, y revisar el código lo confirma e incluso lo refuerza. Mi veredicto de **Descartar (3/10)** se mantiene firme.

La lógica en `OnCalculate` es exactamente la que describiste y la que yo critiqué:

C#

```
// ...
case Mode.Percentage:
    percValue = value * _rangeFilter * 0.01m; // % Fijo
    break;
case Mode.Value:
    percValue = _rangeFilter; // Valor Fijo
    break;
case Mode.Ticks:
    tickValue = _rangeFilter * InstrumentInfo.TickSize; // Ticks Fijos
    break;
// ...
_renderSeries[bar].Upper = value + ...;
_renderSeries[bar].Lower = value - ...;

```

El indicador dibuja bandas **estáticas**. No se adapta a la volatilidad, lo que lo hace inútil y peligroso para el scalping.

----------

### 🚨 ¡He encontrado un bug crítico en el código!

Además de ser conceptualmente débil, el indicador está **roto** y no funciona como crees.

El modo "Percentage" es inutilizable. Fíjate en el código de la propiedad `RangeFilter`:

C#

```
[Parameter]
[Display(Name = "Range")]
[Range(0, 100)]
public decimal RangeFilter
{
    get => _rangeFilter;
    set
    {
        if (_calcMode == Mode.Percentage) // BUG: Si el modo es Percentage...
            return;                     // ...la función termina aquí.

        _rangeFilter = value; // Esta línea NUNCA se ejecuta en modo Percentage
        RecalculateValues();
    }
}

```

**Esto significa que:**

1.  El valor de `_rangeFilter` se inicializa en `1` (el valor por defecto).
    
2.  Si seleccionas `Mode.Percentage` en la configuración, el indicador usará `_rangeFilter = 1` (es decir, bandas del 1%).
    
3.  Si intentas cambiar el valor del rango (por ejemplo, a 0.5%), la lógica del `set` lo impide. El indicador **ignora tu cambio** y sigue usando el 1%.
    

Por lo tanto, el indicador no solo es conceptualmente inferior a las Bandas de Bollinger o Canales de Keltner (porque no es adaptativo), sino que también está **roto en su funcionalidad principal**.

**Acción (reconfirmada):** **Descartar.**
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTk3NzI3MTE0OSwyMzYyMDQ0XX0=
-->