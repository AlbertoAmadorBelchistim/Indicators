## 🟦 Bill Williams AC (4/10)

**Nombre del archivo:** `ACBW.cs`  
**Nombre del indicador:** Bill Williams AC  
**Web oficial:** [ATAS — Bill Williams AC](https://help.atas.net/support/solutions/articles/72000602333)

![ACBW](../img/ACBW.png)

---

### ⚙️ Parámetros configurables

- **ShortPeriod**: Periodo de la media móvil corta (por defecto: 50)  
- **LongPeriod**: Periodo de la media móvil larga (por defecto: 51)  
- **SignalPeriod**: Periodo de la media móvil de señal (por defecto: 50)  
- **PosColor / NegColor / NeutralColor**: Colores para barras crecientes, decrecientes o sin cambio

---

### 🧭 Clasificación  
📂 Momentum — Indicadores basados en aceleración de precios relativa

---

### 🧠 Uso más frecuente

- Visualizar la **aceleración del momentum** del mercado  
- Confirmar la **dirección de las barras AO** mediante su aceleración  
- Usar como **indicador adelantado** para entradas tempranas en tendencias

---

### 📊 Nivel de relevancia  
🔟 **4 / 10**

✅ Fácil de interpretar visualmente mediante colores  
✅ Compatible con otras herramientas de Bill Williams  
⛔ Basado exclusivamente en medias móviles simples, con posible retardo  
⛔ Su utilidad baja en gráficos de muy corto plazo o en consolidaciones

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de entrada** cuando dos barras consecutivas son verdes tras cruce de medias  
- **Evitar entradas** si la aceleración disminuye (cambio de color)  
- **Soporte al AO** (Awesome Oscillator) para validar cambios de momentum

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ShortPeriod**: `5`  
- **LongPeriod**: `34`  
- **SignalPeriod**: `5`  
- **PosColor / NegColor / NeutralColor**: colores diferenciados para cambio inmediato  

✅ Aumenta la sensibilidad a cambios rápidos de dirección  
✅ Ideal para entradas agresivas tras consolidaciones  
⛔ Puede generar señales falsas en movimientos erráticos o sin tendencia

---

### 🧪 Notas de desarrollo

- El indicador representa el **Accelerator Oscillator (AC)** de Bill Williams.  
- Se calcula como la diferencia entre el AO (`SMA corto - SMA largo`) y una media de señal del mismo:


```
    var diff = _shortSma - _longSma;
    var ac = diff - SMA(diff);
```

- El color de la barra se asigna comparando el valor actual de AC con el anterior:  
  - Si `AC > previo`: color positivo  
  - Si `AC < previo`: color negativo  
  - Si igual: también se pinta como positivo (esto puede ser **confuso** visualmente)  
- No hay errores de compilación ni advertencias funcionales.  
- El comportamiento visual es coherente con lo esperado, aunque el tratamiento de los valores "iguales" puede llevar a una interpretación errónea.

---

### 🛠️ Propuestas de mejora

- 🧠 **Cambiar el color cuando el valor es igual**: usar el color neutral para evitar confusión  
- 📉 Añadir una **línea base en cero** para mayor referencia visual  
- 🔄 Permitir opción de usar **medias exponenciales** para reducir el retardo  
- 📊 Añadir una serie adicional con el valor numérico del AC si se desea análisis cuantitativo más detallado

---

### Comentario de Gemini

Aquí tienes la "pregunta clave" de este indicador:

**¿El momentum de la tendencia actual está acelerando o frenando?**

Recomendaciones de mejora específicas:
- Cambiar los parámetros por defecto a los originales de Bill WIlliam (5,34,5)

 ```
public ACBW()
{
    Panel = IndicatorDataProvider.NewPanel;

    _shortSma.Period = 5;      // <-- CORREGIDO
    _signalSma.Period = 5;     // <-- CORREGIDO
    _longSma.Period = 34;      // <-- CORREGIDO

    DataSeries[0] = _renderSeries;
}

 ```
- Arreglar lógica de color

 ```
// ...
var prevValue = _renderSeries[bar - 1];

if (ac > prevValue)
    _renderSeries.Colors[bar] = _posColor;
else if (ac < prevValue)
    _renderSeries.Colors[bar] = _negColor;
else
    _renderSeries.Colors[bar] = _neutralColor; // <-- CORREGIDO
```

- Fidelidad canónica: Bill Williams usaba el precio medio para sus cálculos

 ```
protected override void OnCalculate(int bar, decimal value)
{
    // Calcular el precio medio, que es el canónico de Bill Williams
    var medianPrice = (High[bar] + Low[bar]) / 2;

    var diff = _shortSma.Calculate(bar, medianPrice) - _longSma.Calculate(bar, medianPrice);
    var ac = diff - _signalSma.Calculate(bar, diff);
    // ... resto del código ...
}
 ```