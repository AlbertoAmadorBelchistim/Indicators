---
# 1. IDENTIFICACIÓN
cs_file: OrderFlow.cs
name: Order Flow Indicator
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: "Tape Analysis"

# 3. VALORACIÓN (Score & Priority)
score_current: 7.5/10
score_potential: 8/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: P2

# 4. DECISIÓN
recommended_action: Conservar (Reserva)

# 5. ANÁLISIS
description: ¿Cómo se visualiza el flujo de órdenes (trades individuales) en el gráfico?
gemini_summary: "El visualizador clásico de la cinta. Muestra el flujo de órdenes como burbujas o cuadrados que crecen con el volumen. Es menos potente filtrando que el TapePatterns (no tiene filtro de tiempo avanzado), pero visualmente es más intuitivo para percibir la 'agresividad' y velocidad del flujo bruto."
competitor_notes: "Excelente alternativa visual al TapePatterns para usuarios que prefieren intuición visual sobre precisión de datos."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-12-10
official_code_date: 2025-04-23
---

## 🛡️ Order Flow Indicator (7.5/10)

**Nombre del archivo:** [`OrderFlow.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OrderFlow.cs)  
**Nombre del indicador:** Order Flow Indicator  
**Web oficial:** [ATAS — Order Flow Indicator](https://help.atas.net/support/solutions/articles/72000602441)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cómo se visualiza el flujo de órdenes (trades individuales o acumulados) en el gráfico?

![OrderFlow](../../img/OrderFlow.png)

---

### ⚙️ Parámetros configurables

* **VisMode:** `Circles` (Bolas) o `Rectangles`.
* **TradesMode:** `Cumulative` (Agrupado por tick) o `Separated` (Trade a trade).
* **Filter:** Volumen mínimo para mostrar la bola.
* **CombineSmallTrades:** Agrupar trades pequeños contiguos en uno grande visualmente.
* **SpeedInterval:** Control de tasa de refresco (rendimiento).

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Tape Analysis"  

---

### 🧠 Uso más frecuente

* **Lectura de "Flow":** Ver el "río" de órdenes entrando. Si ves muchas bolas verdes grandes seguidas, es momentum comprador.
* **Detección de Pánico:** Una explosión de bolas rojas grandes y rápidas suele marcar un clímax de ventas.

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Intuitivo:** La visualización de "burbujas" que crecen es universalmente entendida. No requiere pensar en números.  
✅ **Rendimiento Controlado:** Usa un temporizador (`SpeedInterval`) para no saturar el dibujado en momentos de alta volatilidad.  
⛔ **Menos Preciso:** Al no tener filtro de tiempo (milisegundos), puede mostrar una orden grande fragmentada como muchas bolas pequeñas, ocultando la realidad institucional.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Scalping de Impulso:** Entrar cuando el flujo de bolas se acelera visualmente en una dirección tras una ruptura.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **VisMode:** `Circles`.
* **TradesMode:** `Cumulative`.
* **Filter:** `20` (Para ver flujo medio-alto, limpiar ruido).

---

### 🧪 Notas de desarrollo

* Usa `OnTimerCall` para redibujar por intervalos, separando la recepción de datos del renderizado gráfico. Es una buena práctica para evitar "lag" visual.
* Renderizado manual en `OnRender` usando primitivas gráficas (`FillEllipse`), lo que da flexibilidad total.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguna crítica grave. Cumple su función visual perfectamente.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es la herramienta "Arcade". Divertida y visual. Si te gusta sentir el ritmo del mercado viendo burbujas, úsalo. Si quieres datos precisos para entrar con stop ajustado, usa `TapePatterns`.


---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (Para lectura visual).**

**Acción:** **Conservar (Reserva)**
