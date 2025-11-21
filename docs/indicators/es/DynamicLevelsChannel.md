---
cs_file: DynamicLevelsChannel.cs
name: Dynamic Levels Channel
group: Order Flow
subgroup: Volume Profile
score_current: 9/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Dónde se están formando el POC, VAH y VAL de las últimas N barras (un perfil móvil)?
gemini_summary: "Herramienta 'Core' de Perfil Móvil. Calcula un Volume Profile dinámico sobre una ventana deslizante (ej. últimos 30 mins). Esencial para ver la migración del valor a corto plazo."
comparison_group: "Dynamic Profiles"
competitor_notes: "Único. Complementa al VWAP (Acumulado) ofreciendo una visión de 'Ventana Móvil'."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 Dynamic Levels Channel (9/10)

**Nombre del archivo:** [`DynamicLevelsChannel.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DynamicLevelsChannel.cs)  
**Nombre del indicador:** Dynamic Levels Channel  
**Web oficial:** [ATAS — Dynamic Levels Channel](https://help.atas.net/support/solutions/articles/72000602381)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Dónde se están formando el POC, VAH y VAL de las últimas N barras (un perfil móvil)?

![Dynamic Levels Channel](../../img/DynamicLevelsChannel.png)

---

### ⚙️ Parámetros configurables

Este indicador crea un canal dinámico basado en el perfil de volumen reciente:

#### 📊 Cálculo
* **Period:** Tamaño de la ventana móvil (ej. 40 barras). El perfil se calcula *solo* con estas barras.
* **Calc Mode:**
    * `Volume`: Perfil de volumen estándar.
    * `Delta` / `PosDelta` / `NegDelta`: Perfiles avanzados basados en Order Flow.
* **Days:** Días de historial a cargar.

#### 🎨 Visualización
* **Area Color:** Relleno del Value Area (70%).
* **Lines:** POC, VAL, VAH (Colores y grosores).
* **Signals:** Flechas de compra/venta cuando el precio interactúa con los bordes del canal.

#### 🔔 Alertas
* **Poc Touch:** Alerta al tocar el POC móvil.
* **Approximation:** Alerta al acercarse.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Dynamic Profiles"  

---

### 🧠 Uso más frecuente

* **Rolling Value:** Ver si el valor (POC) está subiendo o bajando *ahora mismo*, independientemente de lo que hizo el mercado hace 2 horas.  
* **Soporte Dinámico:** El VAL (Value Area Low) de un canal móvil alcista actúa como soporte dinámico para la tendencia inmediata.  
* **Divergencia de Valor:** El precio hace un nuevo máximo, pero el POC móvil no logra subir (debilidad).  

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (CORE TÁCTICO)**

✅ **Agilidad:** A diferencia del VWAP (que se vuelve lento al final del día por el peso acumulado), este indicador mantiene la misma reactividad siempre.  
✅ **Versatilidad:** El modo `Delta` permite crear un canal de "Sentimiento" muy potente.  
✅ **Señales:** Incluye lógica de reversión en bordes.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Trend Pullback:** En tendencia alcista, comprar cuando el precio toca el VAL (borde inferior) del canal móvil.  
* **Reversión:** Si el precio rompe el canal y el POC no le sigue, buscar el fallo.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Period** | `40` - `60` | Ventana de ~1 hora de contexto. |
| **Calc Mode** | `Volume` | Estándar. |
| **Area Color** | *Transparente* | Marcar solo el canal. |

---

### 🧪 Notas de desarrollo

* **Algoritmo:** Mantiene una lista de `VolumeInfo` de las últimas N velas.
* En cada tick, añade la nueva data y *remueve* la data de la vela que sale de la ventana (`RemoveAll(x => x.Bar == bar - Period)`). Esto es computacionalmente intensivo pero necesario para un perfil móvil exacto.
* Calcula el Value Area (70%) dinámicamente en cada barra.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Funciona como debe.

---

### 🛠️ Propuestas de mejora

* **Suavizado (P3):** El POC puede saltar bruscamente. Una opción de suavizado visual ayudaría.

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Ventana Deslizante:** El manejo de la lista `_priceInfo` para añadir/quitar datos es un buen ejemplo de buffer circular para perfiles.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el complemento perfecto al VWAP. El VWAP te dice la tendencia del día; el Dynamic Channel te dice la tendencia de la hora.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Te mantiene en el lado correcto del valor inmediato.

**Acción:** **Conservar (Core).**