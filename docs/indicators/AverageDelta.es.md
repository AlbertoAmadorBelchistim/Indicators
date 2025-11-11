## 🟦 Average Delta (6.5/10)

  

**Nombre del archivo:** `AverageDelta.cs`
**Nombre del indicador:** Average Delta
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000618456](https://help.atas.net/support/solutions/articles/72000618456)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: número de barras para calcular la media (por defecto: `10`)
- **CalcType** (`CalculationType`):
- `Sma`: media simple del delta por vela
- `Ema`: media exponencial del delta
- **PosColor / NegColor**: color del histograma para delta medio positivo o negativo
- *(VisualMode y otras propiedades visuales se gestionan internamente como `Histogram`)*

  

---

  

### 🧭 Clasificación

  

📂 VolumeOrderFlow — Indicadores de delta suavizado por media móvil

  

---

  

### 🧠 Uso más frecuente

  

- Identificar la **presión neta de agresión** (compradora o vendedora) de forma suavizada
- Confirmar la **tendencia del flujo de órdenes** sin ruido vela a vela
- Filtrar movimientos erráticos o divergencias poco significativas
- Estudiar el equilibrio o desequilibrio del mercado en un periodo concreto

  

---

  

### 📊 Nivel de relevancia

  

🔟 **6.5 / 10**

✅ Visualización clara y directa del sesgo del delta
✅ Relevante para filtrar setups direccionales
⛔ No capta desequilibrios puntuales, absorciones o clústeres extremos

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Filtro de contexto**: evitar entradas contra el flujo dominante de delta
- **Confirmación en ruptura**: delta medio acompaña el breakout
- **Debilitamiento de impulso**: delta medio se aplana antes de reversión
- **Validación de patrones de agotamiento** si se combina con Footprint

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `5`
- **CalcType**: `Ema`
- **PosColor / NegColor**: verde intenso / rojo intenso
- *(VisualMode del histograma ya está predefinido)*

  

✅ Detecta cambios de sesgo direccional con menos retraso
✅ Efectivo como filtro complementario al Cumulative Delta
⛔ No representa intensidad intrabarra ni absorciones ocultas

  

---

  

### 🧪 Notas de desarrollo

  

- Calcula una **media móvil del delta** de cada vela, seleccionable entre `SMA` y `EMA`.
- Utiliza las clases auxiliares `SMA` y `EMA` del framework de ATAS para el cálculo.
- El valor medio se almacena en `_data`, un `ValueDataSeries` visualizado como histograma.
- El color del histograma cambia dinámicamente según el signo del valor medio.
- La propiedad `VisualMode` está fijada internamente como `Histogram` y no es editable desde la UI.

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir opción para **visualizar también el delta bruto por vela** como línea secundaria
- Permitir **línea media suavizada** en lugar de solo histograma
- Incluir opción para **mostrar el valor actual** en el gráfico
- Posibilidad de **comparar con el volumen promedio**, creando un ratio de agresión neta
- Añadir soporte para **zonas de sobrecompra/sobreventa** relativas al delta

## Comentario Gemini
Aquí tienes la "pregunta clave" de este indicador:

> The Key Question: "What is the average aggressive pressure (Delta) over the last X bars, smoothing out the bar-to-bar noise?"
> 
> (¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas, suavizando el ruido de vela a vela?)

----------

### ✍️ Mi Opinión (Confirmando tu Análisis)

-   **Lo Bueno (El 6.5):** Ganas **claridad**. Es un filtro de contexto excelente. Si el `AverageDelta` está por encima de cero, el "régimen" es alcista (la agresión compradora está ganando en promedio). Es genial para confirmar que no estás operando contra el flujo de órdenes reciente.
    
-   **Lo Malo (El -3.5):** Pierdes **información clave** y añades **LAG (retraso)**.
    
    -   **Lag:** Por definición, una media móvil es un indicador con retraso. Estás suavizando el Delta, que ya es una confirmación de la acción del precio. Es "retraso sobre retraso".
        
    -   **Información Clave:** Para un scalper, a veces las señales más importantes del Delta son los _picos de ruido_ que este indicador filtra: los picos de clímax (agotamiento) o las grandes absorciones. Al suavizarlo, "escondes" esos eventos críticos.
        

### Veredicto

**Acción:** **Conservar (con reservas)**.

Es una herramienta de contexto válida si entiendes sus limitaciones. Tu "Parametrización óptima" (`Period=5`, `CalcType=Ema`) es la elección perfecta para minimizar el lag y hacerlo lo más útil posible para el scalping.

Excelente trabajo. ¿Continuamos?
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTUxNTc1MDAxMV19
-->