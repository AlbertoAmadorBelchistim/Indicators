---
cs_file: LinRegChannel.cs
name: Linear Regression Channel
category: Trend
group: Trend
subgroup: Volatility
score_current: 3/10
version: ATAS Official
recommended_action: Reparar
description: ¿Cuál es el canal de tendencia dominante (regresión lineal) y dónde están
  sus límites de desviación estándar?
gemini_summary: Concepto excelente (canal + flecha) pero ROTO por un bug crítico de
  división por cero si InstrumentInfo.TickSize es 0.
file_state: Roto
score_potential: 9/10
effort: Bajo
action_priority: P1
analysis_date: 2025-11-17
official_code_date: 2025-04-23
---

## 🟦 Linear Regression Channel (3/10)

**Nombre del archivo:** [`LinRegChannel.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/LinRegChannel.cs)  
**Nombre del indicador:** Linear Regression Channel  
**Web oficial:** [ATAS — Linear Regression Channel](https://help.atas.net/support/solutions/articles/72000618910)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Cuál es el canal de tendencia dominante (regresión lineal) y dónde están sus límites de desviación estándar?

![LinearRegressionChannel](../../img/LinearRegressionChannel.png)

---

### ⚙️ Parámetros configurables

* **Type**: Fuente de datos para el canal (Close, Open, HL2, HLC3, etc.)
* **Period**: Número de barras para calcular la regresión (por defecto: 100)
* **Deviation**: Multiplicador de la desviación estándar (por defecto: 2)
* **ExtendLines**: Mostrar las líneas extendidas más allá del último bar
* **ShowFibonacci**: Mostrar niveles de Fibonacci dentro del canal
* **ShowBrokenChannel**: Dibujar líneas especiales si el precio rompe el canal
* **BullishColor / BearishColor / BrokenChannelColor**: Colores de las líneas
* **LineWidth**: Grosor de las líneas del canal
* **ArrowColor / ArrowSize / LabelTransparency**: Personalización visual del icono de pendiente

---

### 🧭 Clasificación
📂 Trend — Canal de regresión lineal con desviaciones estándar y niveles de Fibonacci

---

### 🧠 Uso más frecuente

* Visualizar la dirección dominante del precio mediante una regresión lineal
* Operar dentro de un canal de precios ajustado con límites dinámicos
* Detectar rupturas estructurales del canal con alertas visuales

---

### 📊 Nivel de relevancia
🔟 **3 / 10**

✅ Combina dirección (pendiente) y volatilidad (desviación) en una única herramienta  
✅ Muestra claramente fases de ruptura o continuación  
⛔ **BUG CRÍTICO:** Causa un crash de ATAS si el `TickSize` del instrumento es 0  
⛔ Puede necesitar ajustes finos en `Deviation` y `Period` según el activo

---

### 🎯 Estrategias de scalping donde se aplica

* **Entrada por rebote dentro del canal** tras testear banda superior/inferior
* **Confirmación de tendencia** si la pendiente es fuerte y el precio permanece en el canal
* **Ruptura estructural**: señal cuando se dibuja línea de canal roto (`ShowBrokenChannel`)

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Type**: `HighLowClose3`
* **Period**: `80`
* **Deviation**: `1.5`
* **ShowFibonacci**: `false`
* **ShowBrokenChannel**: `true`

---

### 🧪 Notas de desarrollo

* Usa regresión lineal con pendiente adaptativa (`LinRegSlope`)
* Calcula desviación estándar respecto al canal para construir bandas
* Soporta visualización de líneas de Fibonacci internas
* Usa `EnableCustomDrawing` y `OnRender` para dibujar una flecha personalizada que indica la dirección y pendiente del canal
* Detecta rupturas del canal y dibuja una línea especial (`_brokenPen`)
* Todas las líneas son objetos `TrendLine` con opciones de extensión (`IsRay`)

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Este indicador es conceptualmente excelente, uno de los más avanzados del lote. La implementación en `LinRegChannel.cs` incluye características de alto nivel como `EnableCustomDrawing` y `OnRender` para dibujar una flecha de pendiente, además de lógica para `ShowFibonacci` y `ShowBrokenChannel`.

**PERO**, la implementación está **ROTA** y es peligrosa. La función `RoundToFraction` se llama repetidamente usando `InstrumentInfo.TickSize` como denominador (ej. `RoundToFraction(_currDev[bar] * _deviation, InstrumentInfo.TickSize)`). Si el `TickSize` del instrumento es 0 (lo cual es posible en spreads sintéticos o instrumentos mal configurados), esto causará un **crash de ATAS por división por cero**.

Este bug P1 hace que el indicador no sea fiable en producción.

**Propuesta de Reparación (P1):**
* **URGENTE:** Validar el `TickSize` antes de usarlo.
    * Ejemplo: `var tick = Math.Max(InstrumentInfo.TickSize, 0.0000001m);`
    * Y luego usar: `RoundToFraction(..., tick);`

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (una vez reparado).**

El canal dinámico y la detección de rupturas son muy útiles, pero actualmente es inutilizable por el riesgo de crash.

**Acción:** **Reparar (Bug P1 - Crash por división por cero).**
