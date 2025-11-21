---
cs_file: HRanges.cs
name: HRanges
group: Order Flow
subgroup: Volume Profile
score_current: 8/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Dónde se están formando rangos (consolidaciones) y cuál es el POC interno?
gemini_summary: "Indicador 'Core' de Nivel 2. Utiliza una máquina de estados para detectar automáticamente periodos de consolidación (rangos) y dibuja su POC interno. Vital para estrategias de reversión a la media."
comparison_group: "Volume Profile"
competitor_notes: "Único para detectar consolidaciones automáticamente."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 HRanges (8/10)

**Nombre del archivo:** [`HRanges.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/HRanges.cs)  
**Nombre del indicador:** HRanges  
**Web oficial:** [ATAS — HRanges](https://help.atas.net/support/solutions/articles/72000602573)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Dónde se están formando rangos (consolidaciones) y cuál es el POC interno de esos rangos, filtrado por volumen y duración?

![HRanges](../../img/HRanges.png)

---

### ⚙️ Parámetros configurables

* **Days:** Historial de días a calcular (Default: 20).
* **Volume Filter:** Volumen mínimo acumulado para que el rango sea considerado válido y se muestre el POC.
* **Bars Range:** Duración mínima (en barras) para considerar que una zona es un rango.
* **Hide Options:** Ocultar rangos que no cumplan los filtros de volumen o duración.
* **Colors:** Colores para rango Alcista, Bajista y Neutro.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Volume Profile"  

---

### 🧠 Uso más frecuente

* **Detección de Balance:** Identificar zonas donde el mercado ha aceptado el precio (Value Areas temporales).  
* **POC Táctico:** El indicador dibuja una línea en el nivel de mayor volumen del rango. Este nivel suele actuar como imán o soporte/resistencia en el futuro.  
* **Breakout Failure:** Si el precio rompe un rango pero reingresa, el objetivo es el POC opuesto.  

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Automatización:** Detecta la estructura de mercado (Tendencia vs Rango) sin intervención manual.  
✅ **Filtros Profesionales:** Los filtros `VolumeFilter` y `BarsRange` son esenciales para eliminar el ruido de micro-consolidaciones irrelevantes.  
✅ **Objetividad:** Define "Rango" matemáticamente, eliminando la subjetividad del trazado manual.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Rompimiento Confirmado:** Entrada si el precio sale del rango con agresión y volumen creciente.  
* **Reversión en Borde:** Si aparece absorción en la parte superior/inferior del rango.  
* **POC Test:** Validación de rechazo o aceptación en el POC del rango.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **BarsRange** | `10` | Mínimo 10 minutos de consolidación. |
| **VolumeFilter** | `500` | Filtrar rangos sin liquidez. |
| **HideAll** | `True` | Limpiar el gráfico de ruido. |

---

### 🧪 Notas de desarrollo

* Implementa una **máquina de estados** (`_direction`, `_isRange`) para rastrear la estructura del mercado.
* `RenderLevel` calcula el POC (`maxVol.Key`) iterando los datos de volumen de las velas del rango.
* El uso de `Aggregate` para encontrar el POC es eficiente.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Visualización:** Si hay muchos rangos consecutivos, el gráfico puede saturarse de líneas horizontales.

---

### 🛠️ Propuestas de mejora

* **Extensión (P3):** Opción para extender las líneas del POC hacia el futuro hasta que sean tocadas por el precio (Naked POC).

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Detección de Rangos:** El algoritmo que decide cuándo empieza y termina un rango (`if candle.Close < prevCandle.High...`) es muy valioso para bots de estructura.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta de contexto vital. Saber si estás en un día de tendencia o en un día de rangos (Mean Reversion) es lo primero que debes decidir. Este indicador te lo dice.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para saber cuándo dejar de perseguir el precio y empezar a vender extremos.

**Acción:** **Conservar (Core).**
