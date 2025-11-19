---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: OpenInterest.cs
name: Open Interest
category: Volume
score_current: 8/10
version: ATAS Official
recommended_action: 'Conservar'
description: >-
  ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: >-
  Indicador estándar de OI. Implementación sólida con modos por barra, sesión y acumulado. Incluye alertas y filtros visuales útiles.
file_state: Estable
score_potential: 8/10
effort: N/A
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-18
official_code_date: 2025-04-23
user_modification_date: null
---

## 🟦 Open Interest (8/10)

**Nombre del archivo:** [`OpenInterest.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OpenInterest.cs)  
**Nombre del indicador:** Open Interest  
**Web oficial:** [ATAS — Open Interest](https://help.atas.net/support/solutions/articles/72000602439)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión?

![OpenInterest](../../img/OpenInterest.png)

---

### ⚙️ Parámetros configurables

* **Mode**: Modo de cálculo (`ByBar`, `Session`, `Cumulative`)
* **MinimizedMode**: Mostrar en modo minimizado (solo parte superior del histograma)
* **Filter**: Umbral mínimo de cambio para mostrar vela
* **UseAlerts**: Activar alertas ante cambios relevantes

---

### 🧭 Clasificación
📂 Volume — Indicador clásico de interés abierto por vela o sesión

---

### 🧠 Uso más frecuente

* Visualizar el **cambio de posiciones abiertas** por barra o sesión
* Confirmar **acumulación o distribución**
* Generar **alertas ante cambios relevantes en el OI**

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Muestra con claridad el comportamiento del interés abierto  
✅ Múltiples modos de análisis: por barra, sesión o acumulado  
⛔ Requiere un activo con datos fiables de OI y buen filtrado

---

### 🎯 Estrategias de scalping donde se aplica

* **Confirmación de ruptura real** si el OI aumenta tras superar nivel
* **Filtro de trampa** si el precio sube pero el OI cae → posible cierre de posiciones
* **Alertas para scalping rápido** ante picos de OI no habituales

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Mode**: `ByBar`
* **Filter**: `0` (o ajustado al ruido)
* **UseAlerts**: `true`

---

### 🧪 Notas de desarrollo

* Calcula el OI según el modo:
    * `ByBar`: Diferencia `OI[t] - OI[t-1]`
    * `Cumulative`: Valor total `OI[t]`
    * `Session`: Diferencia desde el inicio de la sesión
* Implementa alertas simples basadas en el cambio neto (`Math.Abs(this[bar]) >= _changeSize`)
* Usa una serie secundaria (`_filterSeries`) para colorear barras que superan el filtro

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una implementación estándar y fiable del Interés Abierto. El código es limpio y no tiene problemas de rendimiento ni bugs evidentes.

La inclusión de un sistema de alertas integrado (`ChangeSize`) es muy útil para scalping, ya que permite al trader ser notificado de entradas masivas de posiciones sin tener que mirar el indicador constantemente. Es menos granular que el `OI Analyzer`, pero más fácil de configurar y leer rápidamente.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para una lectura rápida de "están entrando o saliendo", es suficiente y eficaz.

**Acción:** **Conservar (Estándar y eficaz).**

