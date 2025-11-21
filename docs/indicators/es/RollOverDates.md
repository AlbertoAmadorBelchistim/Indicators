---
cs_file: RolloverDates.cs
name: Rollover Dates
category: Utility
group: Utility
subgroup: Visuals
score_current: 8/10
version: Stable
recommended_action: Conservar
description: ¿Cuándo vence el contrato de futuros actual?
gemini_summary: "Excelente utilidad técnica. Usa async/await para obtener datos. Esencial para futuros."
comparison_group: "Time Tools"
competitor_notes: "Sin competencia."
reusable_code: null
file_state: Estable
score_potential: 8/10
effort: Bajo
action_priority: N/A
analysis_date: 2025-11-18
official_code_date: 18/07/2025
---

## 🟦 Rollover Dates (8/10)

**Nombre del archivo:** [`RolloverDates.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/RolloverDates.cs)  
**Nombre del indicador:** Rollover Dates  
**Web oficial:** No disponible aún.  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 18/07/2025  

> **La Pregunta Clave:** ¿Cuándo vence el contrato de futuros actual y debo cambiar al siguiente?

![RolloverDates](../../img/RolloverDates.png)

---

### ⚙️ Parámetros configurables

* **RolloverType**: Criterio para definir el rollover (Por expiración, por volumen, etc.).
* **LineSettings**: Estilo (Color, grosor, tipo) de la línea vertical de aviso.
* **Labels**: Configuración de fuente y posición del texto con la fecha/nombre del contrato.

---

### 🧭 Clasificación
📂 Utility — Herramienta informativa sobre la estructura del contrato, no sobre el precio.

---

### 🧠 Uso más frecuente

* **Gestión de Riesgo:** Evitar operar el día de expiración o rollover debido a la volatilidad errática y bajada de volumen.
* **Contexto:** Saber en qué contrato estás operando visualmente sin abrir menús.

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Técnicamente Superior:** Usa llamadas asíncronas (`GetContractRolloversAsync`) para no congelar la interfaz mientras busca datos.
✅ **Interactivo:** Muestra información detallada al pasar el ratón (`ProcessMouseMove`).
✅ Fundamental para traders de futuros profesionales.
⛔ No genera señales de trading (es puramente informativo).

---

### 🎯 Estrategias de scalping donde se aplica

* **Defensiva:** "No trading" si la línea de Rollover está en la sesión de hoy o mañana. El volumen se divide entre dos contratos y el Order Flow se vuelve "sucio".

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **ShowLabels**: `True` (Importante ver la fecha exacta).
* **Color**: Rojo brillante (Para que sirva de advertencia clara).

---

### 🧪 Notas de desarrollo

* **Async/Await:** Implementación correcta de concurrencia. Usa `Interlocked.CompareExchange` para evitar múltiples llamadas simultáneas a la API de datos.
* **Mouse Interaction:** Sobrescribe `ProcessMouseMove` para detectar si el cursor está sobre una línea y mostrar info extra.
* **Rendering:** Dibuja directamente sobre el canvas (`OnRender`), lo que es eficiente.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es un **indicador de infraestructura** excelente. Muchos traders novatos pierden dinero por operar el contrato vencido (o a punto de vencer) con baja liquidez. Este indicador automatiza esa alerta.

El código es de alta calidad, utilizando patrones modernos de C# y la API de ATAS correctamente para tareas pesadas (datos externos) sin bloquear el hilo de la UI.

**Propuestas de Mejora:**
* Ninguna crítica. Cumple su función perfectamente.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (Indirectamente).**

No te dice cuándo comprar, pero te dice **cuándo NO operar** o cuándo cambiar de ticker. Eso es vital para la supervivencia del scalper.

**Acción:** **Conservar.**