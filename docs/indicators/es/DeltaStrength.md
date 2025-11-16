## 🟦 Delta Strength (6.5/10)

**Nombre del archivo:** `DeltaStrength.cs`  
**Nombre del indicador:** Delta Strength  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602363](https://help.atas.net/support/solutions/articles/72000602363)

---

### ⚙️ Parámetros configurables

- **MaxFilter**: Porcentaje máximo del delta respecto al máximo/mínimo delta intrabarra  
- **MinFilter**: Porcentaje mínimo del delta respecto al máximo/mínimo delta intrabarra  
- **PosFilter / NegFilter**: Filtrado por tipo de vela (alcista, bajista o cualquiera)

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Indicadores basados en fuerza de delta relativa

---

### 🧠 Uso más frecuente

- Detectar **velas con delta fuerte relativo** al máximo/mín delta intrabarra  
- Visualizar posibles zonas de absorción o agotamiento de intención  
- Identificar **divergencias delta/vela** según si la vela es alcista o bajista pero el delta está en el extremo contrario  
- Señalización rápida de barras con delta inusualmente fuerte en contexto

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Rápido para visualizar zonas con delta concentrado o extremo  
✅ Compatible con estrategias de reversión o confirmación de agresión  
⛔ Filtros poco intuitivos sin visualización numérica ni etiquetas  
⛔ Puede fallar en gráficos de bajo volumen o delta muy oscilante

---

### 🎯 Estrategias de scalping donde se aplica

- **Absorción por reversión**: vela alcista con delta muy negativo (punto rojo)  
- **Ruptura con intención clara**: vela bajista con delta extremadamente negativo  
- **Filtro de entrada por contexto**: evitar entradas cuando no hay delta fuerte relativo  
- **Análisis post-noticia**: comprobar si hay acumulación o distribución silenciosa

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **MinFilter**: `85`  
- **MaxFilter**: `98`  
- **PosFilter / NegFilter**: `All` o según dirección buscada  
- Colores: verde (positiva), rojo (negativa), gris (neutral)  
- **VisualType**: `Dots`, tamaño `4`

✅ Aporta señales visuales limpias  
✅ Complementa bien al CVD y al delta puro

---

### 🧪 Notas de desarrollo

- Calcula si el delta de la vela está dentro de un rango relativo de agresión:  
  - Para delta positivo: compara con MaxDelta  
  - Para delta negativo: compara con MinDelta  
- Se aplica un filtro por dirección de vela (Bull, Bear, All) antes de confirmar la señal  
- Si cumple condiciones, dibuja un **punto en la vela** (por encima si es negativa, por debajo si es positiva)  
- Si no cumple condiciones, coloca un valor 0 en las series  
- Usa `VisualMode.Dots`, sin tooltips ni valores en el eje  
- Se actualiza visualmente tras recalcular (OnFinishRecalculate → RedrawChart)

---

### ❗ Incoherencias o aspectos mejorables detectados

- La condición de filtro para delta negativo usa:
  
```
candle.Delta <= candle.MinDelta * 0.01m * MaxFilter.Value  
candle.Delta >= candle.MinDelta * 0.01m * MinFilter.Value  
```
  Lo cual podría ser incoherente si `MinDelta` ya es negativo: se están comparando números negativos con porcentajes positivos, generando condiciones contradictorias o nunca verdaderas  
- Falta validación robusta si MinDelta o MaxDelta son cero  
- En algunos casos, se sobreescriben valores de series neutrales/negativas en la misma barra (pérdida de señal anterior)  
- No hay etiqueta ni representación visual que indique por qué una vela ha sido señalada

---

### 🛠️ Propuestas de mejora

- Reescribir la lógica de comparación para usar Math.Abs() y simplificar la condición  
- Añadir opción para mostrar una **etiqueta de valor delta exacto** o su porcentaje  
- Añadir soporte para alertas sonoras si se detecta una vela con delta extremo  
- Incluir estadísticas acumuladas (número de señales por sesión, media de delta marcado)  
- Permitir elegir otros puntos de dibujo: en la mecha, en el cuerpo o a una distancia fija