---
# 1. IDENTIFICACIÓN
cs_file: TapePattern.cs
name: Tape Patterns
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: "Tape Analysis"

# 3. VALORACIÓN (Score & Priority)
score_current: 9/10
score_potential: 10/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: P1

# 4. DECISIÓN
recommended_action: Conservar (Core)

# 5. ANÁLISIS
description: ¿Dónde están los bloques de órdenes grandes y patrones de ejecución específicos en la cinta?
gemini_summary: "Herramienta de nivel institucional. Transforma la cinta (Time & Sales) en señales visuales mediante un motor multihilo avanzado. Su valor diferencial es la capacidad de reconstruir órdenes fragmentadas ('Icebergs' y algoritmos HFT) usando filtros de tiempo y precio. Es un detector de manipulación profesional."
competitor_notes: "Mucho más potente y profundo que 'Order Flow Indicator'. Permite lógica compleja (tiempo + precio + volumen)."
reusable_code: "Motor Multihilo (StartProcessQueueThread) y estructura Concurrent Queue para alto rendimiento."

# 6. METADATOS
analysis_date: 2025-12-10
official_code_date: 2025-08-25
---

## 🏆 Tape Patterns (9/10)

**Nombre del archivo:** [`TapePattern.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/TapePattern.cs)  
**Nombre del indicador:** Tape Patterns  
**Web oficial:** [ATAS — Tape Patterns](https://help.atas.net/support/solutions/articles/72000602248)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-08-25  

> **La Pregunta Clave:** ¿Dónde están los bloques de órdenes grandes y patrones de ejecución específicos en la cinta?

![TapePattern](../../img/TapePattern.png)

---

### ⚙️ Parámetros configurables

Este indicador es un escáner complejo con múltiples filtros:

#### 📊 Cálculo y Filtros (El Motor de Reconstrucción)

| Opción en UI (Español) | Parámetro Interno | Valor | Explicación "LEGO" |
| :--- | :--- | :--- | :--- |
| **Transacciones acumuladas** | `CumulativeTrades` | **☑️ (Marcado)** | **El Reconstructor.** Si está desmarcado, el indicador ve "piezas sueltas" (ruido). Al marcarlo, activa el motor que pega las piezas para mostrarte el bloque institucional completo. |
| **Volumen mínimo de tick** | `MinVol` | **0** | **Filtro de Pieza Suelta.** ¿Ignoramos las piezas pequeñas? **NO**. Ponemos `0` para recoger hasta la orden de 1 contrato, porque a menudo las ballenas trocean sus órdenes en piezas minúsculas. Si pones 10 aquí, perderías gran parte de la orden oculta. |
| **Volumen máximo de tick** | `MaxVol` | **0** | **Techo de Pieza.** `0` significa "Sin límite". Queremos ver todo, desde lo más pequeño hasta lo más grande. |
| **Número mínimo de transacciones** | `MinCount` | **0** | **Filtro de Fragmentación.** ¿Te importa si la orden entró en 1 solo golpe o en 50 trozos? Al poner `0`, le dices: "No me importa cómo entró, solo quiero saber cuánto suma al final". |
| **Número máximo de transacciones** | `MaxCount` | **0** | `0` significa infinito. Sin límite de fragmentos. |
| **Volumen total mínimo** | `MinCumulativeVolume` | **100** | **EL DETECTOR DE BALLENAS.** Este es el filtro final. El indicador suma todas las piezas que ha encontrado. Si el **Castillo Terminado** suma 100 o más, te avisa. Si suma 99, lo ignora. Aquí es donde filtras el ruido del mercado ($20M+ nocional en ES). |
| **Volumen total máximo** | `MaxCumulativeVolume` | **0** | `0` significa infinito. Queremos ver hasta la orden más gigante posible. |
| **Filtro de tiempo, milisegundos** | `TimeFilter` | **100** | **El Pegamento Temporal.** Si entra una orden y **menos de 100ms** después entra otra, el indicador asume que es el mismo "francotirador" disparando en ráfaga y las agrupa. Es el estándar para contrarrestar algoritmos HFT de ejecución. |
| **Buscar huellas dentro del filtro...** | `SearchPrintsInsideTimeFilter` | **⬜ (Desmarcado)** | **Modo de Agrupación.** <br>• *Desmarcado (Rolling):* Si la ráfaga de órdenes es continua (sin pausas >100ms), sigue agrupando aunque dure 5 segundos. Es mejor para ver la "intención completa". <br>• *Marcado:* Corta estrictamente a los 100ms. |
| **Ticks en el rango** | `RangeFilter` | **1** | **El Pegamento de Precio.** Si la ballena compra tanto que mueve el precio 1 tick (Slippage), ¿lo contamos como la misma orden? **SÍ**. Poner `1` permite agrupar ejecuciones en precios adyacentes (ej. 4000.00 y 4000.25). |
| **Tipo de cálculo** | `CalculationMode` | **Cualquiera** | Filtra si solo quieres ver compras (Bid), ventas (Ask) o todo. Déjalo en `Cualquiera` para ver ambas partes de la batalla. |

#### 🎨 Visualización (Zona vs Punto)
* **Shape:** `Rectángulo` (Recomendado).
    * *Nota Técnica:* El indicador no dibuja un punto en el precio promedio. Dibuja una caja que abarca desde el `MinimumPrice` hasta el `MaximumPrice` de la ejecución reconstruida.
    * *Significado:* Si ves una caja alta, significa que la orden "barrió" el libro de órdenes (Slippage). Si ves una caja plana, la orden se ejecutó en un solo nivel (Iceberg o Limitada pasiva).
* **Size:** Tamaño fijo o dinámico (proporcional al volumen) para distinguir visualmente la magnitud de la ballena.
* **Colors:** Configuración completa para diferenciar agresiones de Compra (Bid) vs Venta (Ask).
---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Tape Analysis"  

---

### 🧠 Uso más frecuente

* **Detector de Icebergs:** Identificar múltiples impresiones pequeñas en el mismo precio que suman un volumen masivo (usando `RangeFilter=0`, `MinCount>5` y `MinCumulative` alto).
* **Bloques Institucionales:** Detectar ejecuciones únicas de gran tamaño que barren varios niveles del libro (`RangeFilter>1`).
* **Defensa de Nivel:** Ver absorción agresiva en un tick específico que frena el precio.

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (PROFESIONAL)**

✅ **Ingeniería Superior:** Utiliza un hilo de fondo dedicado (`_tradesThread`) y colas concurrentes (`BlockingCollection`) para procesar miles de trades por segundo sin congelar la pantalla. Esto es programación de alto nivel.  
✅ **Granularidad:** Te permite definir *exactamente* qué es una "orden grande" para ti, filtrando el ruido retail.  
✅ **Visión Real:** Al usar `CumulativeTrades` con `TimeFilter`, ves la intención real del institucional, no el ruido del HFT.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Momentum Ignition:** Entrar al mercado cuando aparece un patrón de cinta masivo (>200 lotes en ES) a favor de la ruptura de un nivel clave.
* **Reversal por Absorción:** Detectar un patrón de "Muro" (muchos trades, volumen alto, precio no se mueve) en mínimos del día.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

*Objetivo: Ver solo Ballenas, eliminar ruido.*

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Cumulative Trades** | `True` | Ver la orden completa. |
| **Min Cumulative Vol** | `100` (ES) | Filtrar todo lo que no sea institucional. |
| **Time Filter** | `100` ms | Agrupar ejecuciones de algoritmos rápidos. |
| **Range Filter** | `1` | Permitir deslizamiento de 1 tick. |
| **Visual** | `Rectángulo` | Más limpio que los círculos para apilar. |

---

### 🧪 Notas de desarrollo

* **Arquitectura:** El uso de `BlockingCollection<object>` para manejar la cola de trades entre el hilo de datos y el hilo de procesamiento es un diseño robusto y necesario para instrumentos de alta volatilidad.
* **Gestión de Memoria:** Agrupa los datos en objetos `PriceSelectionValue` optimizados para el renderizado, minimizando el impacto en la RAM gráfica.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Curva de Aprendizaje:** Es difícil de configurar bien. Si pones filtros bajos, te llenará la pantalla de basura. Requiere ajuste fino por instrumento.

---

### 🛠️ Propuestas de mejora

* **Alertas Dinámicas (P2):** Sería útil tener sonidos diferentes según el tamaño del bloque (ej. "Ding" para 100 lotes, "Sirena" para 1000 lotes).

---

### 💎 Valor Reutilizable (Código Donante)

* **Motor Multihilo:** El patrón de `StartProcessQueueThread` y `ProcessQueue` es la referencia de cómo manejar datos intensivos en ATAS. Debería estudiarse para optimizar otros indicadores.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el "Francotirador" del Order Flow. Si sientes que no te sirve es porque estás mirando "todo". Úsalo solo para que te avise cuando entra una orden descomunal. Ahí es donde brilla.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Imprescindible (Bien configurado).**

Te da la visión de rayos X sobre quién está moviendo el mercado.

**Acción:** **Conservar (Core)**