---
# 1. IDENTIFICACIÓN
cs_file:  MainIndicator.cs  
name:  MBO DOM  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  DOM  
comparison_group:  "DOM Visuals"  

# 3. VALORACIÓN (Score & Priority)
score_current:  10/10  
score_potential:  10/10  
file_state:  Estable  
effort:  Bajo  
action_priority:  Baja  
system_priority:  P1  

# 4. DECISIÓN
recommended_action:  Conservar (Core)  

# 5. ANÁLISIS
description:  ¿La muralla de liquidez es real (una institución) o son 500 traders retail? ¿Hay bloques grandes esperando (Icebergs)?  
gemini_summary:  "El Francotirador. Superior tecnológicamente al desglosar la liquidez agregada en sus componentes reales (órdenes individuales). Arquitectura robusta y thread-safe."  
competitor_notes:  "Sustituye funcionalmente al DOM clásico y complementa al DomLevels (Histórico) con precisión milimétrica en tiempo real."  
reusable_code:  "MboGridController.cs (Lógica completa de gestión MBO y renderizado de bloques)."  

# 6. METADATOS
analysis_date:  2025-11-30  
official_code_date:  2025-08-25  
---

## 🧱 MBO DOM (10/10)

**Nombre del archivo:** [`MainIndicator.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/Technical/DomV3/MainIndicator.cs)  
**Nombre del indicador:** MBO DOM  
**Web oficial:** [ATAS - MBO DOM](https://help.atas.net/support/solutions/articles/72000633231)  
**Compatibilidad:** ATAS Estable (Requiere Datafeed con soporte MBO, ej. Rithmic).  
**Última revisión del código oficial:** 2025-08-25  

> **La Pregunta Clave:** ¿La muralla de liquidez es real (una institución) o son 500 traders retail? ¿Hay bloques grandes esperando (Icebergs)?

![MBO_DOM](../../img/MBODOM.png)


---

### ⚙️ Parámetros configurables

#### **Colors**
* **Bids / Asks:** Define el color de los bloques individuales de compra y venta pasiva.  
* **Text:** Color de los números (volumen y contadores) en el panel lateral.  

#### **MBO Filters (Detección Institucional)**
* **Color Filter (Order Size):** Umbral visual. Las órdenes individuales MAYORES a este valor se pintan con color sólido (institucional). Las menores se pintan solo con borde (retail/ruido).  
* **Total Volume Filter (Min Block):** Filtro de ocultación. Los bloques individuales MENORES a este valor no se dibujan. Útil para limpiar la pantalla en mercados rápidos.  

#### **Summary (Panel Lateral Agregado)**
* **Show Volume:** Muestra la columna con la suma total de volumen en el nivel (equivale al DOM clásico).  
* **Show Orders Count:** Muestra la columna con el número de órdenes activas por precio.  
* **Row Order Volume:** (Filtro) Resalta el fondo de la celda si el volumen total supera este valor.  
* **Row Order Count:** (Filtro) Resalta el fondo de la celda si el número de órdenes supera este valor.  


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "DOM Visuals"  


---

### 🧠 Uso más frecuente

* **Caza de Icebergs:** Identificar órdenes que se rellenan pero no desaparecen de la visualización MBO.  
* **Detección de Spoofing:** Visualizar grandes bloques que parpadean y se retiran ante la llegada del precio.  
* **Análisis de Calidad:** Diferenciar soportes de "hormigón" (pocos bloques grandes) de soportes de "papel" (muchos pequeños).  


---

### 📊 Nivel de relevancia
🔟 **10 / 10**

✅ **Transparencia Total:** Elimina la opacidad de los datos agregados L2.  
✅ **Rendimiento:** Usa `System.Timers` para desacoplar la recepción de datos del dibujado UI.  
✅ **Híbrido:** Integra la vista lateral del DOM clásico, haciéndolo redundante.  
⛔ **Requisito:** Inútil sin un datafeed de nivel profesional (MBO).  


---

### 🎯 Estrategias de scalping donde se aplica

* **Order Book Scalping:** Operativa en el spread basada en microestructura.  
* **Reversión en Bloques:** Entradas defensivas apoyadas en bloques institucionales visibles.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor | Justificación |
| :--- | :--- | :--- |
| **Color Filter** | `10` | En ES (S&P), órdenes >10 lotes suelen ser institucionales o algoritmos agresivos. Diferencia visual crítica. |
| **Min Block Size** | `1` | Ver todo el detalle es preferible para detectar absorción fina. |
| **Show Volume** | `True` | Referencia rápida agregada necesaria para el contexto. |
| **Show Count** | `False` | Reduce carga cognitiva; en scalping rápido el número exacto de órdenes importa menos que el tamaño de los bloques. |


---

### 🧪 Notas de desarrollo

* **Arquitectura MVC:** Separa limpiamente la UI (`MainIndicator`) de la lógica de datos (`MboGridController`).  
* **Thread Safety:** Uso intensivo y correcto de `ConcurrentDictionary` y bloqueos (`lock`) para manejar el flujo de datos de alta frecuencia sin corromper la memoria.  
* **Throttling UI:** Implementa un `System.Timers.Timer` para redibujar a intervalos fijos (1000ms por defecto en init, aunque ajustable) en lugar de a cada tick, evitando que ATAS se congele en noticias.  


---

### ❗ Incoherencias o aspectos mejorables detectados

* **Gestión de Memoria:** El diccionario `_mboHistory` crece indefinidamente. En sesiones de 24h con mucha volatilidad, podría consumir demasiada RAM ya que no hay un mecanismo de purga de órdenes antiguas o muy alejadas del precio.  
* **Hardcoded Timer:** El intervalo de refresco está "quemado" en el código (`_timer.Interval = 1000` en `OnInitialize`). Para scalping puro, esto podría ser lento (1 FPS).  


---

### 🛠️ Propuestas de mejora

* **Auto-Purge:** Implementar una rutina en el `MboGridController` para eliminar órdenes completadas o canceladas del diccionario histórico cada X minutos.  
* **FPS Ajustable:** Exponer el intervalo del Timer como parámetro para permitir refresco más rápido (ej. 100ms) en máquinas potentes.  


---

### 💎 Valor Reutilizable (Código Donante)

* **`MboGridController.cs`:** Es una joya. Contiene toda la lógica para procesar feeds MBO de ATAS, gestionar IDs de órdenes y agrupar por precio. Reutilizable para crear estrategias automáticas que dependan del MBO.  


---

### ✍️ La opinión de Gemini sobre el Indicador

Es la herramienta definitiva para el scalper moderno. Su superioridad técnica radica en que no "interpreta" los datos, sino que los "deconstruye". Ver un bloque de 100 lotes como un solo rectángulo sólido (Institución) vs 100 rectángulos pequeños (Retail) cambia totalmente la psicología de la entrada.

**Propuestas de Acción:**
* Configurar como indicador principal en el template de Order Flow.
* Valorar modificar el código para bajar el Timer a 200ms para mayor fluidez.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**SÍ**

Imprescindible para trading de precisión.

**Acción:** **Conservar (Core)**