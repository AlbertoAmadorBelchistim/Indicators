---
cs_file: VWAP.cs
name: VWAP/TWAP
group: Order Flow
subgroup: Volume Profile
score_current: 10/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Cuál es el precio medio ponderado por volumen (institucional) y sus desviaciones estándar?
gemini_summary: "El indicador más completo y profesional de la plataforma. Implementa VWAP (Volume Weighted Average Price) y TWAP con múltiples desviaciones estándar, reinicio por periodos (Semanal, Mensual) y la función crítica de 'Custom Start Point' (Anchored VWAP)."
comparison_group: "Dynamic Profiles"
competitor_notes: "El estándar de oro. Superior a cualquier media móvil simple."
reusable_code: null
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 08/05/2025
---

## 🏆 VWAP/TWAP (10/10)

**Nombre del archivo:** [`VWAP.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VWAP.cs)  
**Nombre del indicador:** VWAP/TWAP  
**Web oficial:** [ATAS — VWAP / TWAP](https://help.atas.net/support/solutions/articles/72000602503)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 08/05/2025  

> **La Pregunta Clave:** ¿Cuál es el precio medio ponderado por volumen (institucional) y sus desviaciones estándar?

![VWAP](../../img/VWAP.png)

---

### ⚙️ Parámetros configurables

Este indicador es una suite completa de gestión de precio medio:

#### 📊 Cálculo
* **Type (Period):** Define cuándo se reinicia el cálculo.
    * `Daily`: Reinicio por sesión (Estándar intradía).
    * `Weekly/Monthly`: Para análisis de swing.
    * `Custom`: Define horas específicas.
* **Mode:**
    * `VWAP`: Ponderado por Volumen (Estándar).
    * `TWAP`: Ponderado por Tiempo (Usado en días de bajo volumen o para ejecución algorítmica).
* **Volume Type:** `Total`, `Bid` o `Ask`.

#### 📉 Bandas de Desviación (SD)
* **StDev 1, 2, 3:** Multiplicadores para las bandas de desviación estándar (ej. 1.0, 2.0, 3.0).
* Permite configurar colores y rellenos de fondo (`UpperBackground`) para crear zonas de valor visuales.

#### ⚓ Anchored VWAP (Custom Start)
* **Allow Custom Start Point:** Activa la función de anclaje manual.
* **Teclas:** `F` (Fijar inicio), `G` (Borrar inicio). Permite anclar el VWAP a un evento (noticia, máximo/mínimo) en tiempo real.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Dynamic Profiles"  

---

### 🧠 Uso más frecuente

* **Fair Value:** El VWAP es el "imán" de la sesión. Las instituciones lo usan para evaluar si compraron barato (bajo VWAP) o caro (sobre VWAP).  
* **Mean Reversion:** El precio tiende a regresar al VWAP cuando se extiende a las bandas de 2SD o 3SD.  
* **Trend Following:** Un precio que se mantiene consistentemente por encima del VWAP y la banda 1SD indica una tendencia alcista fuerte.  

---

### 📊 Nivel de relevancia
🔟 **10 / 10 (IMPRESCINDIBLE)**

✅ **Institucional:** Es el benchmark real del mercado, no un indicador lag.  
✅ **Anchored VWAP:** La capacidad de anclarlo manualmente (`StartKeyFilter`) es una característica premium vital para el análisis contextual.  
✅ **Completo:** Incluye TWAP y gestión de desviaciones estándar en un solo módulo robusto.  

---

### 🎯 Estrategias de scalping donde se aplica

* **VWAP Bounce:** El primer test del VWAP después de una tendencia extendida suele ofrecer un rebote técnico.  
* **Band Fade:** Venta ciega en la banda +3SD con objetivo en la +1SD (Reversión a la media).  
* **Anchored Defense:** Anclar el VWAP al inicio de un impulso fuerte; si el precio vuelve a tocarlo, suele defenderse.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Type** | `Daily` | Visión de la sesión actual. |
| **Mode** | `VWAP` | Ponderación real. |
| **StDev 1** | `1.0` | Zona de Valor (68%). |
| **StDev 2** | `2.0` | Zona de Extensión (95%). |
| **StDev 3** | `3.0` | Zona de Clímax (99%). |
| **Colors** | *Fondo suave* | Usar rellenos con alta transparencia. |

---

### 🧪 Notas de desarrollo

* **Ingeniería:** Mantiene acumuladores de `TotalVolume` y `TotalVolToClose` (Vol*Price).
* **Varianza:** Calcula la desviación estándar acumulada correctamente usando la fórmula de varianza ponderada.
* **Interacción:** Implementa `FilterKey` para la interacción teclado-ratón, lo cual es avanzado para un indicador estándar.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Es un código de referencia.

---

### 🛠️ Propuestas de mejora

* **Ninguna.** ---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica Anchored:** El sistema de `StartKeyFilter` y `BarFromDate` es reutilizable para cualquier indicador que necesite un punto de inicio manual.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el rey de los indicadores dinámicos. Si solo pudieras tener una línea en tu gráfico además del precio, debería ser el VWAP. La implementación de ATAS es de nivel profesional.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Es la referencia de gravedad del mercado.

**Acción:** **Conservar (Core).**
