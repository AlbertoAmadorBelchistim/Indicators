---
cs_file: OpenInterest.cs
name: Open Interest
category: Order Flow
group: Order Flow
subgroup: Open Interest
score_current: 6/10
version: Estable
recommended_action: Conservar (Reserva / Donante)
description: ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión?
gemini_summary: "Indicador básico. Muestra el OI total sin desgloses. Útil solo como referencia de fondo o para alertas simples, pero superado en todo por el OI Analyzer."
comparison_group: "Open Interest Analysis"
competitor_notes: "Muy inferior al OI Analyzer. Solo gana en simplicidad y menor consumo de recursos."
reusable_code: "Sistema básico de Alertas por cambio de valor (ChangeSize)"
file_state: Estable
score_potential: 6/10
effort: N/A
action_priority: Bajo
analysis_date: 2025-11-20
official_code_date: 2025-04-23
---

## 🛡️ Open Interest (6/10)

**Nombre del archivo:** [`OpenInterest.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OpenInterest.cs)  
**Nombre del indicador:** Open Interest  
**Web oficial:** [ATAS — Open Interest](https://help.atas.net/support/solutions/articles/72000602439)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión?

![OpenInterest](../../img/OpenInterest.png)

---

### ⚙️ Parámetros configurables

* **Mode**:
    * `ByBar`: Cambio neto en la vela actual.
    * `Cumulative`: Valor total del OI (curva clásica).
    * `Session`: Cambio desde el inicio de la sesión.
* **Filter**: Umbral mínimo para colorear o resaltar la barra.
* **Alerts**: Alerta sonora si el cambio supera `ChangeSize`.

---

### 🧭 Clasificación
**Grupo:** Order Flow
**Subgrupo:** Open Interest

---

### 🧠 Uso más frecuente

* **Referencia General:** Monitorizar la salud del contrato (si sube o baja el interés general a largo plazo).
* **Alertas:** Configurar una alarma para "ballenas" (cambios bruscos de OI en una sola vela).

---

### 📊 Nivel de relevancia
6️⃣ **6 / 10 (BÁSICO)**

✅ **Ligero:** Consume muy pocos recursos y es fácil de configurar.  
✅ **Alertas:** Su sistema de alertas es simple y funcional.  
⛔ **Ciego:** No distingue dirección. Un aumento de OI puede ser alcista (compras) o bajista (cortos). Este indicador no te lo dice, por lo que su utilidad táctica es limitada.

---

### 🎯 Estrategias de scalping donde se aplica

* **Contexto de Sesión:** Si el OI acumulado cae durante todo el día, es un día de liquidación (evitar buscar tendencias largas, buscar reversiones).

---

### ⚙️ Parametrización óptima para scalping

* **Mode**: `ByBar` (Para ver el cambio inmediato).
* **UseAlerts**: `True` (Configurar un umbral alto para que avise de actividad inusual).

---

### 🧪 Notas de desarrollo

* Código estándar y eficiente. Calcula `currentOpen - prevValue`.
* Lógica simple de visualización mediante `CandleDataSeries`.

---

### ❗ Incoherencias o aspectos mejorables detectados
* **Filtrado Visual Engañoso:** La lógica del filtro (`if (oiValue < Filter)...`) no oculta la vela, sino que la pinta plana (`Open=Close`). Esto puede crear "Dojis artificiales" en el gráfico que confunden visualmente. Debería pintar `0` o `Transparent` para ocultarla de verdad.

---

### 🛠️ Propuestas de mejora
* Ninguna. Es un indicador básico.

---

### 💎 Valor Reutilizable (Código Donante)
* **Sistema de Alertas (`ChangeSize`):** La implementación de la alerta basada en el cambio absoluto (`Math.Abs(this[bar]) >= _changeSize`) es un bloque de código muy limpio y reutilizable para añadir alertas sonoras a cualquier indicador que carezca de ellas.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el indicador "Vainilla" del grupo. Hace lo que dice la caja, ni más ni menos.

Para un scalper avanzado, se queda corto porque falta el desglose direccional que ofrece `OIAnalyzer`. Sin embargo, es perfecto para tener una visión periférica del mercado (Contexto) o para configurar alertas sonoras de actividad inusual sin cargar el gráfico.

### 📈 Veredicto: ¿Es útil para Scalping?

**Solo para alertas y contexto general.**

**Acción:** **Conservar (Reserva).**
