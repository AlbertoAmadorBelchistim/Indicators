---
# 1. IDENTIFICACIÓN
cs_file: OIAnalyzer.cs
name: OI Analyzer
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Open Interest
comparison_group: "Open Interest Analysis"

# 3. VALORACIÓN (Score & Priority)
score_current: 2/10
score_potential: 10/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Conservar (Reserva - Activos Crypto)

# 5. ANÁLISIS
description: ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle?
gemini_summary: "El código es una obra maestra del Order Flow, capaz de desglosar agresiones de OI. SIN EMBARGO, para S&P 500 (CME) es inútil intradía debido a la falta de datos tick-by-tick en Rithmic/CQG. Es vital conservarlo para futuros usos en Crypto."
competitor_notes: "Superior a todos, pero sufre la misma limitación de datos que el resto."
reusable_code: "Lógica de desglose trade-by-trade de OI."

# 6. METADATOS
analysis_date: 2025-12-28
official_code_date: 2025-12-16
---

## 🏆 OI Analyzer (2/10 en ES / 10/10 en Crypto)

**Nombre del archivo:** [`OIAnalyzer.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OIAnalyzer.cs)  
**Nombre del indicador:** OI Analyzer  
**Web oficial:** [ATAS — OI Analyzer](https://help.atas.net/support/solutions/articles/72000602437)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-12-16  

> **La Pregunta Clave:** ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle?

![OIAnalyzer](../../img/OIAnalyzer.png)

---

### ⚠️ ADVERTENCIA DE DATOS (S&P 500)
**Este indicador requiere datos de Open Interest Tick-by-Tick.** El feed de **CME (Rithmic/CQG)** para futuros como el ES (S&P 500) **NO provee este dato intradía**. El indicador mostrará una línea plana o datos erróneos durante la sesión.  
*Solo es funcional en mercados Crypto o feeds específicos que soporten OI streaming.*

---

### ⚙️ Parámetros configurables

* **Mode (OiMode):** `Buys` (OI por agresiones de compra), `Sells` (OI por agresiones de venta).
* **Calculation Mode:**
    * `CumulativeTrades`: Tendencia acumulada de la sesión.
    * `SeparatedTrades`: Cambio neto por barra.
* **Cumulative Mode:** Checkbox rápido para activar acumulación.
* **Clusters Mode:** Pinta los valores numéricos sobre las velas (Estilo Footprint).
* **Visualización:**
  * `Custom Diapason`: Escala fija personalizada.
  * `Grid Step`: Separación de la cuadrícula.
  * `Up Color / Down Color`: Color de velas alcistas y bajistas.
  * `Border Color`: Color del borde de las velas (parámetro independiente).

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Open Interest  
**Comparison Group:** "Open Interest Analysis"  

---

### 🧠 Uso más frecuente

* **Crypto Scalping:** Detectar Longs/Shorts agresivos en tiempo real.
* **Futuros (EOD):** Análisis diario de estructura (Solo cierre de sesión).

---

### 📊 Nivel de relevancia
🔟 **2 / 10 (Contexto S&P 500)**

✅ **Código Perfecto:** La lógica interna es impecable.  
⛔ **Sin Datos:** Rithmic no alimenta este motor en el S&P 500.  

---

### 🎯 Estrategias de scalping donde se aplica

* **N/A en S&P 500.** (Requiere Crypto).

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Justificación |
| :--- | :--- | :--- |
| **Calculation Mode** | `CumulativeTrades` | Único modo con mínima utilidad EOD. |
| **Clusters Mode** | `False` | No habrá datos intradía para mostrar. |

---

### 🧪 Notas de desarrollo

* Usa `CumulativeTradesRequest` para reconstruir el historial.
* Depende críticamente de `tick.OpenInterest`, campo que viene vacío o estático en feeds CME intradía.

Mejoras en versión alfa (2025-12-16):
* Mejora de UI: separación explícita de `BorderColor` respecto a `DownColor` (bugfix visual).
* Render: clipping seguro del valor del eje mediante región dedicada (`try/finally`).
* Grid: redibujado con espaciado mínimo configurable para evitar saturación visual.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Dependencia de Datos:** No avisa al usuario si el feed no soporta OI. Debería mostrar un "Data Not Available".
* **UI menor:** el tooltip de `BorderColor` reutiliza la descripción de color alcista (detalle cosmético).

---

### 🛠️ Propuestas de mejora

* **UI:** Agregar una advertencia visual si se detecta que el OI no cambia en X barras ("Posible falta de datos del proveedor").

---

### 💎 Valor Reutilizable (Código Donante)

* **Algoritmo Core:** La distinción `Direction == Buy` vs `Sell` aplicada al Delta del OI es reutilizable para otros indicadores de flujo.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es un **Ferrari sin gasolina**. El coche es increíble (el código), pero en las carreteras del CME (S&P 500) no hay gasolina (Datos OI) para moverlo.

**Propuestas de Acción:**
* **Hibernar:** Mantener en la librería "Core" pero desactivado para el workspace de ES.
* **Etiquetar:** Marcar claramente como "Crypto Only".

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No (En S&P 500).**

Por falta de datos del proveedor.

**Acción:** **Conservar (Reserva - Activos Crypto)**

