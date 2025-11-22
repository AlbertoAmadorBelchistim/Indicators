---
cs_file: DomPowerModif.cs
name: DOM Power Modif
group: Order Flow
subgroup: DOM
score_current: 9/10
version: Custom (v1.3.0)
recommended_action: Conservar (Core)
description: ¿Cuál es el desequilibrio neto (Bids vs Asks) en el libro de órdenes y su rango?
gemini_summary: "Versión 'Pro' corregida. Arregla el bug de actualización en tiempo real del original y añade el 'Histograma DOM' (CVD Pasivo), una herramienta crítica para ver la intención de las órdenes limitadas frente a la agresión."
comparison_group: "Liquidez vs Agresión"
competitor_notes: "Superior a la versión oficial. Complementa al CVD de agresión."
reusable_code: null
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 DOM Power Modif (9/10)

**Nombre del archivo:** [`DomPowerModif.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/DomPowerModif.cs)  
**Nombre del indicador:** DOM Power Modif  
**Web oficial base:** [ATAS — DOM Power](https://help.atas.net/support/solutions/articles/72000602374)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código base:** 23/04/2025  
**Última revisión del código modificado:** 13/11/2025 (v 1.3.0)  
*(Versión extendida y mejorada por Alberto Amador Belchistim)*

> **La Pregunta Clave:** ¿Cuál es el desequilibrio neto (Bids vs Asks) en el libro de órdenes y cuál es su rango de volatilidad?

![DomPowerModif](../../img/DomPower.png)

---

### ⚙️ Parámetros configurables

#### 📊 Cálculo y Filtros
* **LevelDepth:** Número de niveles de profundidad del DOM a considerar (ej. 5). *Nota: Recomendado desactivar para ver el DOM completo.*
* **Mode:**
    * `SeparateLines`: Líneas de Bids y Asks por separado.
    * `HistogramDom`: **Recomendado.** Histograma neto (CVD Pasivo) y Rango.

#### 🔔 Alertas
* **AlertEnabled:** Activar avisos.
* **DomRangeThreshold:** Umbral de volatilidad del DOM para disparar alerta.
* **AlertExtremesPercent:** Sensibilidad en extremos.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "Liquidez vs Agresión"  

---

### 🧠 Uso más frecuente

* **CVD Pasivo:** El modo Histograma muestra la presión del libro. Si el precio baja pero el Histograma DOM sube (más Bids que Asks), es una divergencia de absorción (Liquidity Trap).  
* **Volatilidad del DOM:** La línea de rango muestra si las instituciones están "jugando" con las órdenes (quitando y poniendo, spoofing) o si el libro es estable.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (PROFESIONAL)**

✅ **Tiempo Real:** Arregla el bug crítico del original que solo actualizaba al cierre de vela.  
✅ **Intención vs Ejecución:** Permite comparar la presión pasiva (DOM Power) con la agresión real (Cumulative Delta).  
✅ **Métrica Única:** El "Rango de Imbalance" es una métrica de volatilidad de liquidez que no existe en otros indicadores.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Divergencia de Intención:** Precio haciendo nuevos mínimos + CVD Bajista + DOM Power Alcista (Verde) = Absorción pasiva en mínimos. Posible rebote.  
* **Breakout de Liquidez:** Si el DOM Range explota, indica que se han retirado o añadido grandes bloques de golpe. Atentos a la dirección.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **LevelDepth** | `Disabled` | Evitar bug de filtro y ver profundidad total. |
| **Mode** | `HistogramDom` | Lectura más clara (Neto). |
| **Alerts** | `True` | Avisar de cambios bruscos de liquidez. |

---

### ✨ Mejoras introducidas (Custom Modif)

1.  **Fix de Actualización:** El indicador original no repintaba en tiempo real. Ahora reacciona a cada cambio del DOM (`MarketDepthChanged`).
2.  **Modo Histograma:** Se añadió la visualización de histograma neto, convirtiéndolo en un oscilador de liquidez.
3.  **Cálculo de Rango:** Nueva métrica para medir la volatilidad interna del libro de órdenes.

---

### 🧪 Notas de desarrollo

* Mantiene cachés de `_maxDomImbalanceCache` para rendimiento.
* **Bug conocido (Heredado):** La lógica de `LevelDepth` original tiene un fallo menor. Se recomienda desactivar el filtro de profundidad (`Enabled=false`) para obtener datos 100% fiables del libro completo.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Filtro LevelDepth:** Sigue teniendo la lógica defectuosa del original. Mejor usar sin filtro.

---

### 🛠️ Propuestas de mejora

* **Ninguna crítica.** La modificación ya ha elevado este indicador a nivel Core.

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Histograma DOM:** El cálculo de `cumBids - cumAsks` en tiempo real es reutilizable para estrategias automáticas.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta vital para leer la "segunda mitad" del mercado (las órdenes pendientes). Sin esto, solo ves la agresión. Con esto, ves contra qué choca la agresión.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para detectar trampas de liquidez y absorción.

**Acción:** **Conservar (Core).**