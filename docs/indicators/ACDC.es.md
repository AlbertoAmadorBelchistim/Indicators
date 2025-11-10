## 🟦 AC DC Histogram (2/10)

**Nombre del archivo:** `ACDC.cs`  
**Nombre del indicador:** AC DC Histogram  
**Web oficial:** [ATAS — AC DC Histogram](https://help.atas.net/support/solutions/articles/72000602293)

![ACDC](../img/ACDC.png)

---

### ⚙️ Parámetros configurables

- **SmaPeriod1**: Periodo de la SMA lenta del Average Price (por defecto: 34)  
- **SmaPeriod2**: Periodo de la SMA rápida del Average Price (por defecto: 5)  
- **SmaPeriod3**: Periodo de la SMA de suavizado de la diferencia AO - SMA (por defecto: 10)  
- **SmaPeriod4**: Periodo de la SMA para suavizar el AO (por defecto: 5)  
- **PosColor / NegColor**: Colores para barras crecientes o decrecientes  

---

### 🧭 Clasificación
📂 Momentum — Indicadores basados en aceleración de precios relativa

---

### 🧠 Uso más frecuente

- Visualizar **momentum de mercado suavizado**, similar al Awesome Oscillator  
- Detectar **cambios de dirección en la fuerza del precio**  
- Utilizar como filtro de entrada en combinación con otros indicadores  

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

✅ Añade un filtro visual suavizado del oscilador AO  
✅ Fácil de interpretar para traders visuales  
⛔ Poca información de volumen u order flow puro  
⛔ No se basa en datos de clúster, delta o imbalances  

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro de entrada**: solo tomar trades en la dirección del histograma  
- **Divergencias**: si el histograma cae pero el precio sube (o viceversa)  
- **Confirmación de ruptura**: usar como confirmación adicional de momentum  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **SmaPeriod1**: `34`  
- **SmaPeriod2**: `5`  
- **SmaPeriod3**: `10`  
- **SmaPeriod4**: `5`  
- **Colores**: verde para crecimiento, rojo para caída  

✅ Esta configuración ofrece una lectura rápida del momentum suavizado  
⛔ No sustituye a indicadores de flujo de órdenes en decisiones críticas  

---

### 🧪 Notas de desarrollo

- Calcula el **Awesome Oscillator (AO)** como la diferencia entre dos SMAs del precio medio (High + Low) / 2  
- Luego aplica una segunda capa de suavizado para construir el histograma  
- El color de las barras depende de la diferencia entre valores sucesivos del histograma suavizado (`_sma3`)  
- Utiliza `ValueDataSeries` con modo `Histogram` para visualizar  

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar **línea AO adicional**, no solo histograma  
- Permitir **cambio de fuente de datos** (por ejemplo: Close, Typical Price)  
- Añadir alertas sonoras o visuales en **cambios de pendiente**  
- Incluir opción para **mostrar línea cero** como referencia visual  
- Posibilidad de **combinarlo con delta o volumen** para mayor contexto

---

### Comentario de Gemini

Aquí tienes la "pregunta clave" de este indicador:

**¿Cuál es la dirección suavizada y con retardo de la aceleración del mercado?**

El código es funcional, pero la lógica que implementa es, en mi opinión, conceptualmente defectuosa para el scalping.
 
Tu análisis de desarrollo es correcto, pero quiero recalcar lo que este indicador *realmente* está haciendo, porque es la clave de por qué no funciona:

1.  **Paso 1: Calcula el AO (Awesome Oscillator)** `_ao[bar] = _sma2[bar] - _sma1[bar];`
     (Esto es una `SMA(5) - SMA(34)` del precio medio. Es el AO estándar de Bill Williams).
2.  **Paso 2: Calcula el AC (Accelerator Oscillator)** `_ao[bar] - _sma4[bar]` (Esto es `AO - SMA(5, AO)`. Es *exactamente* el indicador `ACBW` que vimos en el paso anterior).
3.  **Paso 3: Suaviza el AC con *otra* media móvil** `_renderSeries[bar] = _sma3.Calculate(bar, ...);` (Esto aplica una `SMA(10)` al indicador `ACBW`).
 
**Conclusión clave:** Este indicador `ACDC` es literalmente el indicador `ACBW` (el "Accelerator") pasado por un **filtro de suavizado SMA(10)**.
 
Ya habíamos concluido que el `ACBW` (el 4/10) era ruidoso y tenía cierto retraso. Aplicarle una SMA de 10 períodos encima solo hace una cosa: **añadir un retraso (lag) masivo**.

- Añade la línea 0:

```
private readonly ValueDataSeries _renderSeries = new("RenderSeries", Strings.Visualization)
{
    VisualType = VisualMode.Histogram,
    ShowZeroValue = true, // <-- ARREGLADO
    //...
};
```

- Corrige la lógica de color para que el valor igual use el color neutral:
```
if (_sma3[bar] - lastValue > 0)
    _renderSeries.Colors[bar] = _posColor;
else if (_sma3[bar] - lastValue < 0)
    _renderSeries.Colors[bar] = _negColor;
else
    _renderSeries.Colors[bar] = _renderSeries.Colors[bar-1]; // O un color neutral
```