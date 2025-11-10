## 🟦 Accumulation / Distribution Flow (6/10)

**Nombre del archivo:** `ADF.cs`  
**Nombre del indicador:** Accumulation / Distribution Flow  
**Web oficial:** [ATAS - Accumulation/Distribution Flow](https://help.atas.net/support/solutions/articles/72000602569)

![ADF](../img/ADF.png)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de la media móvil aplicada al flujo acumulado (por defecto: 14)
- **UsePrev**: Si se usa el cierre anterior (en vez de la apertura) para calcular el flujo (por defecto: activado)

---

### 🧭 Clasificación
📂 Volume — Indicadores de volumen clásico basados en precio y volumen de vela

---

### 🧠 Uso más frecuente

- Medir la **acumulación o distribución** del mercado mediante un flujo ponderado por volumen
- Detectar **divergencias** entre el precio y el flujo acumulado
- Confirmar **intensidad de movimientos** en rupturas o retrocesos

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Permite ver si el movimiento está respaldado por flujo de volumen  
✅ Útil como indicador de fondo o filtro de contexto  
⛔ Menos preciso que otros indicadores específicos de order flow o delta

---

### 🎯 Estrategias de scalping donde se aplica

- **Rupturas con confirmación**: si el ADF se alinea con el breakout
- **Divergencias de agotamiento**: ADF descendente mientras el precio sube (posible giro)
- **Confirmación de tendencia**: mantener operaciones mientras el ADF esté alineado

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `8`  
- **UsePrev**: `true`  

✅ Mayor sensibilidad a movimientos recientes  
✅ Útil para confirmar continuidad o anticipar giro  
⛔ No usar como señal única de entrada, sino como confirmador

---

### 🧪 Notas de desarrollo

- Calcula un valor acumulado tipo OBV modificado:  
  - Si `UsePrev = true`: usa `Close - Close[anterior]`  
  - Si `UsePrev = false`: usa `Close - Open`
- La fórmula se pondera por el volumen y el rango de vela (`High - Low`)
- Se aplica una media simple (SMA) al flujo acumulado para suavizarlo
- El resultado se muestra en formato **histograma** mediante `VisualMode.Histogram`

---

### 🛠️ Propuestas de mejora

- Permitir otros tipos de media (EMA, WMA, etc.)
- Opción para mostrar línea en vez de histograma
- Añadir alertas por cruces con cero o giros del histograma
- Agregar línea de señal adicional para análisis de cruces

---

### Comentario de Gemini

Aquí tienes la "pregunta clave" de este indicador:

**¿Dónde está el alto 'Esfuerzo' (Volumen) encontrándose con un bajo 'Resultado' (Rango), señalando una posible absorción o clímax?**

El *concepto* del indicador es un **9/10**. Es una herramienta de VSA que busca picos de "Esfuerzo" (clímax de compra o venta). Es extremadamente relevante para el scalping, ya que su objetivo es detectar **puntos de giro** y **agotamiento**.

Pero tiene un **problema de Implementación (El Lag):**
- El indicador identifica un concepto *adelantado* o *de clímax* (Esfuerzo vs. Resultado).
- Luego, toma esa línea acumulativa (`_adf`) y le aplica una **SMA de 14 períodos**.
- Esto es un error de diseño. Estás cogiendo una señal de clímax (que quieres saber *ahora*) y le estás aplicando un filtro de 14 barras que añade un **retraso (lag) masivo**.

### 🛠️ ¿Merece la pena arreglarlo?

**Sí, absolutamente.** El *concepto* es oro puro. Lo que hay que "arreglar" es el lag que le añadieron.

* **Mejora Crítica:** Cambiar el `VisualMode.Histogram` por `VisualMode.Line`.
* **Mejora Esencial:** El problema real es la `SMA(14)`. Un scalper necesitaría:
    1.  Ver la línea `_adf` acumulada *en bruto* (sin la SMA) para buscar divergencias.
    2.  O, como mucho, poner la `Period` de la SMA en `1` (para ver el valor en bruto) o `2`-`3` para un suavizado mínimo.