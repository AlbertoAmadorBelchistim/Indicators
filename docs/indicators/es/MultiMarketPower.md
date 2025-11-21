---
cs_file: MultiMarketPower.cs
name: CVD pro(multi) / Multi Market Powers
group: Order Flow
subgroup: Delta
score_current: 10/10
version: Latest
recommended_action: Conservar (Core)
description: ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional)?
gemini_summary: "El Santo Grial del Delta Acumulado. Supera a todos los demás CVDs porque permite desglosar la agresión en 5 capas simultáneas (Retail vs Institucional) en un solo panel. Código robusto y optimizado."
comparison_group: "Cumulative Delta"
competitor_notes: "Vence a 'Market Power' y 'Cumulative Delta' por su capacidad de filtrado múltiple sin penalizar el rendimiento."
reusable_code: null
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 14/08/2025
---

## 🏆 CVD pro(multi) / Multi Market Powers (10/10)

**Nombre del archivo:** [`MultiMarketPower.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MultiMarketPower.cs)  
**Nombre del indicador:** CVD pro(multi) / Multi Market Powers  
**Web oficial:** [ATAS — CVD pro(multi) / Multi Market Powers](https://help.atas.net/support/solutions/articles/72000602434)  
**Compatibilidad:** ATAS versión latest y superiores.  
**Última revisión del código oficial:** 14/08/2025  

> **La Pregunta Clave:** ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional)?

![MultiMarketPower](../../img/MultiMarketPower.png)

---

### ⚙️ Parámetros configurables

Este indicador permite configurar hasta 5 líneas de delta independientes simultáneas:

#### 📊 Configuración General
* **CumulativeTrades:**
    * `True`: Acumula el delta trade a trade (Visión de sesión completa).
    * `False`: Reinicia el delta en cada vela (Modo tick a tick).

#### 🧰 Filtros (1 al 5)
Cada filtro (`Filter1` a `Filter5`) tiene su propio grupo de configuración independiente:
* **Enabled (UseFilterX):** Activa o desactiva el cálculo y visualización de esa línea.
* **MinimumVolume:** Tamaño mínimo de la orden para ser incluida.
* **MaximumVolume:** Tamaño máximo. *Nota: Si es `0`, se considera infinito.*
* **Color / LineWidth:** Personalización visual de la línea.

![Filtros](../../img/MMP_Filters.png)

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta 
**Comparison Group:** "Cumulative Delta"  

---

### 🧠 Uso más frecuente

* **Desglose de Participantes:** Diferenciar visualmente si la subida del precio la están provocando los pequeños traders (retail, filtro 1) o los grandes bloques (institucional, filtro 5).  
* **Divergencias Internas:** Identificar situaciones donde el delta retail compra (FOMO) mientras el delta institucional vende o se aplana (distribución).  
* **Limpieza de Ruido:** Ocultar visualmente el Filtro 1 (<1 contrato) para ver la "verdadera" tendencia de la subasta.  

---

### 📊 Nivel de relevancia
🔟 **10 / 10 (IMPRESCINDIBLE)**

✅ **Superioridad Técnica:** Consolida la función de 5 indicadores en uno solo sin penalizar el rendimiento.  
✅ **Claridad Táctica:** Permite ver la "anatomía" de la vela desglosada por participantes en un solo vistazo.  
✅ **Versatilidad:** Funciona tanto para scalping de micro-estructura (M1) como para análisis de sesión intradía.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Absorción Institucional:** Filtro 5 (Whales) plano o divergente bajista mientras el precio hace nuevos máximos (absorción de compras retail).  
* **Validación de Rupturas:** Breakout válido solo si Filtros 4 y 5 acompañan con pendiente fuerte.  
* **Giro por Agotamiento:** Todos los filtros alineados, pero Filtros 4-5 giran primero anticipando la reversión.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **CumulativeTrades** | `True` | Visión de tendencia de sesión. |
| **Filtro 1 (Retail)** | Min `0` - Max `5` | Ruido y pequeños especuladores. |
| **Filtro 2 (Small)** | Min `6` - Max `20` | Traders activos. |
| **Filtro 3 (Medium)** | Min `21` - Max `50` | Locales / Pequeños fondos. |
| **Filtro 4 (Large)** | Min `51` - Max `100` | Institucional táctico. |
| **Filtro 5 (Whales)** | Min `101` - Max `0` | Institucional pesado / Bloques. |

---

### ✨ Mejoras introducidas (Oficial/Base)

* **Optimización de Rendimiento:** El código utiliza una arquitectura de paso único. Itera sobre la lista de trades una sola vez y asigna cada trade a sus "cubos" (filtros) correspondientes, en lugar de filtrar la lista 5 veces. Esto hace que sea extremadamente ligero incluso en mercados rápidos.

---

### 🧪 Notas de desarrollo

* Mantiene 5 estados independientes (`_delta1` a `_delta5`).
* Maneja correctamente la condición de infinito (`_maxVolume == 0`).
* Solicita datos históricos (`CumulativeTradesRequest`) al cargar, permitiendo ver el CVD desde el inicio de la sesión sin necesidad de tener el gráfico abierto previamente.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta Custom Session:** No tiene la opción de reiniciar el delta a una hora específica (ej. 15:30 para RTH), algo que sí tiene el indicador `CumulativeDelta`.

---

### 🛠️ Propuestas de mejora

* **Fusión (P2):** Portar la lógica de `CustomSession` desde el indicador `CumulativeDelta` para permitir reiniciar el acumulado en la apertura americana.

---

### 💎 Valor Reutilizable (Código Donante)

* **Arquitectura Multi-Filtro:** La estructura del bucle `CalculateTrade` es perfecta para cualquier indicador que necesite segregar volumen por tamaño.

---

### ✍️ La opinión de Gemini sobre el Indicador

Este indicador es el **Santo Grial** del análisis de Delta en ATAS.
Mientras que otros indicadores te dicen *cuánto* se ha comprado, este te dice *quién* lo ha hecho. En el ecosistema actual del S&P 500, donde el volumen retail y el institucional a menudo van en direcciones opuestas, esta información es la ventaja estadística (Edge) definitiva.

**Propuestas de Acción:**
* **Conservar como CORE.**
* Priorizar la implementación del reinicio de sesión RTH.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Sustituye a cualquier otro CVD.

**Acción:** **Conservar (Core).**