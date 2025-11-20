---
cs_file: AverageDelta.cs
name: Average Delta
category: Order Flow
group: Order Flow
subgroup: Delta
score_current: 5/10
version: Estable
recommended_action: Fusionar Lógica
description: ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas?
gemini_summary: "Indicador 'Huérfano'. La idea de suavizar el delta con una SMA/EMA es válida para ver el régimen de fondo, pero no justifica un indicador separado."
comparison_group: "Bar Delta"
competitor_notes: "Su funcionalidad es una característica, no un producto completo."
reusable_code: "Lógica de cálculo de SMA/EMA sobre valores puros de Delta"
file_state: Estable
score_potential: 5/10
effort: Medio
action_priority: Medio
analysis_date: 2025-11-17
official_code_date: 23/04/2025
---

## 💡 Average Delta (5/10)

**Nombre del archivo:** [`AverageDelta.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/AverageDelta.cs)  
**Nombre del indicador:** Average Delta  
**Web oficial:** [ATAS - Average Delta](https://help.atas.net/support/solutions/articles/72000618456)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es la presión agresiva promedio (Delta) durante las últimas X velas?

![AverageDelta](../../img/AverageDelta.png)

---

### ⚙️ Parámetros configurables

* **Period**: Número de barras para calcular la media (por defecto: `10`).
* **CalcType**: Tipo de media móvil (`Sma` o `Ema`).
* **PosColor / NegColor**: Colores del histograma.

---

### 🧭 Clasificación
**Grupo:** Order Flow
**Subgrupo:** Delta (Por Barra)

---

### 🧠 Uso más frecuente

* **(Teórico)** Ver el "régimen" del mercado: ¿Estamos en modo compras sostenidas o ventas sostenidas?
* **(Teórico)** Filtrar el ruido de velas individuales para ver la tendencia de fondo del delta.

---

### 📊 Nivel de relevancia
5️⃣ **5 / 10 (FEATURE)**

✅ **Claridad:** Elimina el ruido de los "latigazos" del delta vela a vela.  
⛔ **Ineficiente:** Ocupa un panel entero solo para mostrar una media móvil. Esto es un desperdicio de espacio en pantalla.  
⛔ **Lag:** Añade retraso a una señal que debería ser inmediata.

---

### 🎯 Estrategias de scalping donde se aplica

* **Filtro de Tendencia de Fondo:** Si AverageDelta > 0, solo buscar largos en retrocesos.
* *Nota:* Esta estrategia se implementa mejor usando la SMA integrada en `CumulativeDelta` o `MarketPower`.

---

### ⚙️ Parametrización óptima para scalping

* **Period**: `5` (Para minimizar el lag).
* **CalcType**: `Ema` (Más reactivo).

---

### 🧪 Notas de desarrollo

* Calcula una media móvil (`SMA` o `EMA`) del **Delta puro** de cada vela (`candle.Delta`).
* Utiliza las clases `SMA` y `EMA` del framework de ATAS.
* El valor se visualiza como histograma.
* **Defecto:** No incluye una línea de cero (`ShowZeroValue = false`), lo que dificulta la lectura visual.

---

### 🛠️ Propuestas de mejora

* **Fusión (La mejora real):** No mejorar este archivo. Integrar su lógica (cálculo de SMA del Delta) como una opción de "Fondo de Color" en el indicador principal `DeltaModif` o `MultiMarketPower`.

---

### 💎 Valor Reutilizable

* **Lógica de Régimen:** Este cálculo es perfecto para crear una "Luz de Fondo" o un "Filtro de Color" en el indicador principal. Ej: Si `AverageDelta` > 0, pintar el fondo del gráfico de verde suave.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es un indicador "Huérfano". La idea de suavizar el delta es válida, pero tener un indicador separado solo para esto es un diseño pobre.

En el trading moderno, el espacio en pantalla es oro. No puedes gastar un panel entero (sub-chart) para ver una media de 10 periodos. Esta información debería estar integrada como una ayuda visual en el panel principal o en el panel del CVD.

### 📈 Veredicto: ¿Es útil para Scalping?

**Como concepto, sí. Como indicador suelto, no.**

Su lógica debe ser absorbida por los indicadores "Core".

**Acción:** **No usar (Pendiente de Fusión).**