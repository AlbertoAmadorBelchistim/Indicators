---
# 1. IDENTIFICACIÓN
cs_file:  DomDynamics.cs  
name:  DOM Dynamics  
version:  Custom v3.6 (Net Flow)  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  DOM  
comparison_group:  "DOM Liquidity Flow"  

# 3. VALORACIÓN (Score & Priority)
score_current:  9/10  
score_potential:  9/10  
file_state:  Estable  
effort:  Bajo  
action_priority:  Nula  
system_priority:  P2  

# 4. DECISIÓN
recommended_action:  Conservar (Core)  

# 5. ANÁLISIS
description:  ¿Se está añadiendo (Stacking) o retirando (Pulling) liquidez neta del mercado en este instante?  
gemini_summary:  "Es la evolución simplificada del concepto Pulling & Stacking. En lugar de mostrar 4 histogramas confusos, calcula el Flujo Neto de Liquidez (Cambio en Bids - Cambio en Asks) y lo representa como una única vela japonesa. Esto permite leer la 'Presión del Libro' en milisegundos. Código optimizado y ligero."  
competitor_notes:  "Supera a 'Pulling & Stacking Bars' en legibilidad operativa para Scalping rápido al sintetizar la información."  
reusable_code:  "Lógica de Snapshot diferencial de MarketDepth."  

# 6. METADATOS
analysis_date:  2025-12-02  
official_code_date:  Unknown  
user_modification_date:  2025-12-02  
---

## 🚀 DOM Dynamics (9/10)

**Nombre del archivo:** [`DomDynamics.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/DomDynamics.cs)  
**Nombre del indicador:** DOM Dynamics  
**Web oficial:** N/A (Desarrollo Propio)  
**Compatibilidad:** ATAS Stable.  
**Última revisión del código custom:** 2025-12-02  

> **La Pregunta Clave:** ¿Se está añadiendo (Stacking) o retirando (Pulling) liquidez neta del mercado en este instante?

![DomDynamics](../../img/DomDynamics.png)

---

### ⚙️ Parámetros configurables

#### **Filters (Filtros de Ruido)**
* **DOM Depth Limit:** (Default: 10) Define cuántos niveles de profundidad del libro (desde el Best Bid/Ask) se monitorizan[cite: 3].  
    * *Nota:* Mantener bajo (5-10) para ver la intención inmediata que afecta al precio actual.
* **Min Change Filter:** (Default: 0) Filtro de volumen mínimo para registrar un cambio[cite: 3].  
    * *Uso:* Si se pone a 10, ignora cambios de 1-9 lotes (ruido de robots market makers).

#### **Visualización**
* **Candle Colors:** Configurable nativamente en ATAS (UpCandle/DownCandle). Representa el flujo neto positivo (Stacking Bids / Pulling Asks) o negativo (Stacking Asks / Pulling Bids)[cite: 3].

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "DOM Liquidity Flow"  

---

### 🧠 Uso más frecuente

* **Confirmación de Ruptura (Breakout):** Si el precio rompe una resistencia y DomDynamics muestra una vela verde fuerte (Net Stacking), la ruptura tiene "gasolina" (soporte subiendo).  
* **Detección de Spoofing (Divergencia):** Precio sube, pero DomDynamics muestra vela roja fuerte (Net Pulling). Significa que están retirando soporte o añadiendo resistencia oculta. Señal de reversión.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Síntesis Visual:** Convierte miles de datos del DOM en una sola vela fácil de leer[cite: 3].  
✅ **Código Optimizado:** Usa `Dictionary` para snapshots y solo calcula diferenciales, carga de CPU mínima[cite: 3].  
✅ **Escalado Automático:** Implementa `ScaleIt = true` en la serie, integrándose perfectamente en sub-paneles[cite: 3].  
⛔ **Ambigüedad Operativa (Trade vs Cancel):** Una reducción de liquidez (vela roja) puede ser por retirada de órdenes (Pulling) o por ejecución de ventas (Trading). El indicador no distingue la causa, solo el efecto en el libro.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Scalping de Rebotes:** Entrar en un nivel clave solo si DomDynamics muestra "Stacking" a favor del rebote.  
* **Gestión de Trade:** Cerrar si el flujo de liquidez se invierte drásticamente contra la posición.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor | Justificación |
| :--- | :--- | :--- |
| **Depth Limit** | `10` | Monitorizar solo los 10 primeros niveles (la "zona de batalla"). Profundidades mayores suelen ser bluff. |
| **Min Volume Filter** | `15` | Filtrar el "ruido de fondo" y algoritmos de mantenimiento. Solo queremos ver cambios institucionales significativos. |
| **Panel** | `New Panel` | Usar en panel inferior para no ensuciar las velas de precio. |

---

### ✨ Mejoras introducidas (Custom)
* **Unificación de Flujo:** Se ha reescrito la lógica para calcular un `NetChange` único en lugar de 4 series separadas[cite: 3].
* **Optimización de Memoria:** Uso de `CandleDataSeries` nativa de ATAS en lugar de dibujar histogramas manuales, mejorando el rendimiento de renderizado[cite: 3].
* **Reset Intra-bar:** Lógica `_currentNetChange = 0` al cambiar de barra para asegurar que cada vela representa flujo fresco[cite: 3].

---

### 🧪 Notas de desarrollo

* **Arquitectura:** Event-driven (`MarketDepthChanged`)[cite: 3].
* **Estado:** El indicador mantiene un `_bidSnapshot` y `_askSnapshot` en memoria. Al llegar un evento, compara el volumen nuevo con el del snapshot para obtener el delta[cite: 3].
* **Critical Path:** El cálculo se hace en cada tick del DOM. El código es eficiente, pero un `Depth Limit` muy alto (ej. 100) podría impactar en momentos de alta volatilidad.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Persistencia Histórica:** No funciona en backfill (histórico). Solo dibuja datos en tiempo real.
* **Ambigüedad:** Como se mencionó, no separa Trades de Cancels. Para arreglar esto, habría que escuchar también el evento `OnNewTrade` y sumar el volumen ejecutado al cambio para neutralizarlo, aislando así solo los cambios pasivos. (Posible mejora futura compleja).

---

### 🛠️ Propuestas de mejora

* Añadir una **Alerta Sonora** si el flujo neto supera un umbral (ej. > 500 lotes en 1 segundo).

---

### 💎 Valor Reutilizable (Código Donante)

* La gestión de los diccionarios `_bidSnapshot` y `_askSnapshot` es perfecta para cualquier indicador que necesite calcular deltas de libro de órdenes[cite: 3].

---

### ✍️ La opinión de Gemini sobre el Indicador

DomDynamics es la herramienta que el scalper necesita: **Información compleja simplificada**. Mientras que `Pulling & Stacking Bars` te da los "datos crudos", este indicador te da la "conclusión". En el calor de la batalla (Scalping S&P 500), prefiero ver una vela verde grande (Presión Alcista) que tener que interpretar 4 barras de colores distintos.

**Propuestas de Acción:**
* Establecer como el indicador de Flujo de Liquidez predeterminado en el template.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí**

Proporciona una ventaja visual inmediata sobre la intención de las órdenes limitadas.

**Acción:** **Conservar (Core)**