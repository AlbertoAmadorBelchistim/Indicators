---
cs_file: TapePattern.cs
name: Tape Patterns
group: Order Flow
subgroup: Volume
score_current: 9/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Dónde están los bloques de órdenes grandes y patrones de ejecución específicos en la cinta?
gemini_summary: "Herramienta de nivel institucional. Transforma la cinta en señales visuales mediante un motor multihilo avanzado. Detecta icebergs, bloques y absorciones con una precisión que ningún otro indicador iguala."
comparison_group: "Tape Analysis"
competitor_notes: "Más potente y profundo que 'Order Flow Indicator', aunque más complejo de configurar."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 25/08/2025
---

## 🏆 Tape Patterns (9/10)

**Nombre del archivo:** [`TapePattern.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/TapePattern.cs)  
**Nombre del indicador:** Tape Patterns  
**Web oficial:** [ATAS — Tape Patterns](https://help.atas.net/support/solutions/articles/72000602248)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 25/08/2025  

> **La Pregunta Clave:** ¿Dónde están los bloques de órdenes grandes y patrones de ejecución específicos en la cinta?

![TapePattern](../../img/TapePattern.png)

---

### ⚙️ Parámetros configurables

Este indicador es un escáner complejo con múltiples filtros:

#### 📊 Cálculo y Filtros
* **Cumulative Trades:** Activa la reconstrucción inteligente de trades (juntar fragmentos de una misma orden).
* **Volume Filters:**
    * `MinVol` / `MaxVol`: Filtro por impresión individual.
    * `MinCumulative` / `MaxCumulative`: Filtro por el total de la orden reconstruida.
* **Pattern Filters:**
    * `TimeFilter`: Agrupa trades que ocurren en menos de X milisegundos (Detección HFT).
    * `RangeFilter`: Agrupa trades que ocurren en un rango de X ticks de precio.
    * `MinCount` / `MaxCount`: Número de impresiones mínimas para formar el patrón.

#### 🎨 Visualización
* **Shape:** Forma del marcador (Rectángulo, Círculo, etc.).
* **Size:** Tamaño fijo o dinámico según volumen.
* **Transparency:** Ajuste alfa para no tapar las velas.
* **Colors:** Configuración completa de Bid/Ask/Between.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Tape Analysis"  

---

### 🧠 Uso más frecuente

* **Detección de Icebergs:** Identificar múltiples impresiones pequeñas en el mismo precio que suman un volumen masivo (usando `RangeFilter=0` y `MinCumulative` alto).  
* **Bloques Institucionales:** Detectar ejecuciones únicas de gran tamaño que barren el libro.  
* **Defensa de Nivel:** Ver absorción agresiva en un tick específico.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (PROFESIONAL)**

✅ **Ingeniería Superior:** Utiliza un hilo de fondo dedicado (`_tradesThread`) y colas concurrentes para procesar miles de trades por segundo sin congelar la pantalla.  
✅ **Granularidad:** Permite definir qué es un "patrón" para ti (tiempo, precio, conteo).  
✅ **Precisión:** Al usar `CumulativeTrades`, ve la orden real, no los fragmentos.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Momentum Ignition:** Entrar al mercado cuando aparece un patrón de cinta masivo a favor de la ruptura.  
* **Reversal por Absorción:** Detectar un patrón de "Muro" (muchos trades, precio no se mueve) en mínimos.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Cumulative Trades** | `True` | Ver la orden completa. |
| **Min Cumulative Vol** | `50` (ES) | Filtrar ruido retail. |
| **Time Filter** | `100` ms | Agrupar ejecuciones de algoritmos rápidos. |
| **Range Filter** | `1` | Permitir deslizamiento de 1 tick. |
| **Visual** | `Rectángulo` | Más limpio que los círculos para apilar. |

---

### 🧪 Notas de desarrollo

* **Arquitectura:** Usa `BlockingCollection<object>` para manejar la cola de trades entre el hilo de datos y el hilo de procesamiento. Es un diseño robusto para alta frecuencia.
* **Gestión de Memoria:** Agrupa los datos en objetos `PriceSelectionValue` optimizados para el renderizado.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Complejidad:** Puede ser abrumador para principiantes. La configuración por defecto a veces es demasiado sensible.

---

### 🛠️ Propuestas de mejora

* **Alertas Dinámicas (P2):** Sonidos diferentes según el tamaño del bloque (ej. Alarma nuclear para >1000 lotes).

---

### 💎 Valor Reutilizable (Código Donante)

* **Motor Multihilo:** El patrón de `StartProcessQueueThread` y `ProcessQueue` es la forma correcta de manejar datos intensivos en ATAS. Debería ser el estándar para cualquier indicador pesado.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta de élite. Mientras que otros indicadores simplemente "pintan cosas", este **procesa y estructura** la información del mercado. Si te tomas en serio el Order Flow, este es tu mejor amigo.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Imprescindible.**

Te da la visión de rayos X sobre quién está moviendo el mercado.

**Acción:** **Conservar (Core).**