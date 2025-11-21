---
cs_file: UnfinishedAuction.cs
name: Unfinished Auction
group: Order Flow
subgroup: Volume Profile
score_current: 8.5/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Quedaron órdenes pendientes en los extremos de la vela que el precio debe volver a visitar?
gemini_summary: "Herramienta táctica de precisión. Detecta 'Subastas Inacabadas' (Unfinished Auctions) en máximos y mínimos, trazando líneas que actúan como imanes para el precio. Basado en teoría de Market Profile."
comparison_group: "Session Profile"
competitor_notes: "Único. No tiene competencia directa."
reusable_code: null
file_state: Estable
score_potential: 8.5/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 08/05/2025
---

## 🏆 Unfinished Auction (8.5/10)

**Nombre del archivo:** [`UnfinishedAuction.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/UnfinishedAuction.cs)  
**Nombre del indicador:** Unfinished Auction  
**Web oficial:** [ATAS — Unfinished Auction](https://help.atas.net/support/solutions/articles/72000602495)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 08/05/2025  

> **La Pregunta Clave:** ¿Quedaron órdenes pendientes (desequilibrio) en los extremos de la vela que el precio debe volver a visitar?

![UnfinishedAuction](../../img/UnfinishedAuction.png)

---

### ⚙️ Parámetros configurables

* **Bid/Ask Filter:** Volumen mínimo en el extremo para considerar la subasta inacabada (ej. > 0).
* **Days:** Días de historial a analizar.
* **Visuals:** Colores de líneas y clusters.
* **Alerts:** Aviso al crear o cerrar una UA.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Session Profile"  

---

### 🧠 Uso más frecuente

* **Target Magnet:** Las líneas UA actúan como imanes. El mercado tiende a volver para completar la subasta (dejar 0 volumen en el tick extremo).  
* **Weak High/Low:** Un máximo con UA es un "máximo débil", probable de ser roto.  
* **Gap Fill:** Similar a los gaps, las UA suelen rellenarse.  

---

### 📊 Nivel de relevancia
🔟 **8.5 / 10 (TACTICO)**

✅ **Teoría Sólida:** Basado en principios de subasta. Un mercado eficiente debe terminar con volumen cero en el extremo.  
✅ **Auto-Limpieza:** Las líneas desaparecen automáticamente cuando el precio las toca (`HorizontalLinesTillTouch`), manteniendo el gráfico limpio.  
✅ **Precisión:** Mira dentro del tick exacto del extremo.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Repair Trade:** Si hay una UA arriba y el precio gira al alza, el objetivo mínimo es tocar esa línea UA.  
* **Breakout Confirmation:** Una ruptura que deja UA atrás es fuerte, pero espera un retest inmediato para cerrarla.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Filter** | `1` (o `10`) | Cualquier volumen > 0 es técnicamente UA. |
| **Days** | `2` | Foco en el corto plazo. |

---

### 🧪 Notas de desarrollo

* Utiliza `LineTillTouch` con `IsRay = true`. Esta clase especial de ATAS gestiona automáticamente la extensión y corte de la línea.
* Escanea `candle.GetPriceVolumeInfo(High)` y `Low`.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Implementación perfecta.

---

### 🛠️ Propuestas de mejora

* **Ninguna.** ---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica TillTouch:** El uso de `HorizontalLinesTillTouch` es un ejemplo perfecto de cómo dibujar niveles de soporte/resistencia dinámicos que se autogestionan.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta de "francotirador". Te da objetivos de precio exactos al tick.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Define objetivos claros.

**Acción:** **Conservar (Core).**