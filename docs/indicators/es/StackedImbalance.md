---
cs_file: StackedImbalance.cs
name: Stacked Imbalance
group: Order Flow
subgroup: Footprint
score_current: 8/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Dónde existen zonas de desequilibrio agresivo apiladas que actúan como soporte?
gemini_summary: "La evolución estructural del Imbalance. Busca zonas donde ocurren múltiples desequilibrios consecutivos (apilados) y proyecta esas zonas hacia el futuro como soporte/resistencia. Es una herramienta de memoria de mercado."
comparison_group: "Imbalance Analysis"
competitor_notes: "Complementa al Imbalance Ratio añadiendo persistencia en el gráfico."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 Stacked Imbalance (8/10)

**Nombre del archivo:** [`StackedImbalance.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/StackedImbalance.cs)  
**Nombre del indicador:** Stacked Imbalance  
**Web oficial:** [ATAS — Stacked Imbalance](https://help.atas.net/support/solutions/articles/72000602474)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Dónde existen zonas de desequilibrio agresivo apiladas (consecutivas) que actúan como soporte/resistencia futuro?

![StackedImbalance](../../img/StackedImbalance.png)

---

### ⚙️ Parámetros configurables

Este indicador busca patrones de fuerza:

#### 📊 Definición de Stacked
* **Imbalance Ratio:** Ratio mínimo (ej. 300%).
* **Imbalance Range:** Cantidad de niveles consecutivos necesarios para activar la zona (ej. 3 niveles).
* **Imbalance Volume:** Volumen mínimo.

#### 🛠️ Gestión de Zonas
* **Line Till Touch:** Extender la zona hasta que el precio la toque de nuevo (Test).
* **Days:** Historial a analizar.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Imbalance Analysis"  

---

### 🧠 Uso más frecuente

* **Soporte de Alta Calidad:** Una zona de Stacked Imbalance de compra (verde) es un soporte muy fuerte. El mercado suele rebotar en el primer test.  
* **Zona de Aceleración:** Si el precio atraviesa un Stacked Imbalance sin parar, indica una fuerza de tendencia extrema.  

---

### 📊 Nivel de relevancia
🔟 **8 / 10 (TACTICO)**

✅ **Memoria:** Mantiene en pantalla la zona de agresión para operar el re-test.  
✅ **Filtrado:** El parámetro `Range` (mínimo 3 niveles) filtra los imbalances aleatorios y deja solo los institucionales.  
⛔ **Recursos:** Puede ser pesado si se configuran muchos días de historial.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Defensa de Zona:** Poner orden límite de compra en el borde superior de un Stacked Imbalance alcista reciente.  
* **Inversión:** Si una zona Stacked es rota con volumen, se convierte en resistencia (Flip).  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Ratio** | `300` | Filtro estándar. |
| **Range** | `3` | Mínimo para ser significativo. |
| **Till Touch** | `True` | Vital para ver objetivos. |

---

### 🧪 Notas de desarrollo

* Usa `HorizontalLinesTillTouch` para gestionar la extensión de líneas automáticamente.
* Escanea arrays de volúmenes diagonales (`volumes[i]` vs `volumes[i+1]`).

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Código robusto.

---

### 🛠️ Propuestas de mejora

* **Alertas de Quiebre (P3):** Avisar no solo cuando se crea, sino cuando se rompe la zona.

---

### 💎 Valor Reutilizable (Código Donante)

* **Algoritmo de Detección de Secuencias:** La lógica para encontrar `count >= _imbalanceRange` consecutivos es útil.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el mejor amigo del trader de soporte/resistencia. Convierte el flujo de órdenes efímero en niveles de precio operables.

**Propuestas de Acción:**
* **Conservar como CORE Secundario.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Define dónde poner el Stop o el Limit.

**Acción:** **Conservar (Core).**