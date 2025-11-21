---
cs_file: BarVolumeFilter.cs
name: Bar's Volume Filter
group: Order Flow
subgroup: Volume
score_current: 7/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks?
gemini_summary: "Herramienta de utilidad pura. No analiza, solo filtra. Su valor reside en limpiar el gráfico visualmente, resaltando solo las velas 'relevantes' (Alto Volumen/Delta) y ocultando el ruido, especialmente útil con su filtro horario RTH."
comparison_group: "VSA & Anomalies"
competitor_notes: "Complementario. Funciona bien junto a Better Volume para destacar extremos."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: Medio
action_priority: P2
analysis_date: 2025-11-17
official_code_date: 23/04/2025
---

## 🛡️ Bar's Volume Filter (7/10)

**Nombre del archivo:** [`BarVolumeFilter.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BarVolumeFilter.cs)  
**Nombre del indicador:** Bar's Volume Filter  
**Web oficial:** [ATAS — Bar's Volume Filter](https://help.atas.net/support/solutions/articles/72000602326)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks?

![BarVolumeFilter](../../img/BarVolumeFilter.png)

---

### ⚙️ Parámetros configurables

* **Type:** Criterio de filtro (`Volume`, `Ticks`, `Delta`, `Bid`, `Ask`).  
* **Min/Max Filter:** Rango de valores aceptados.  
* **TimeFilter:** Horario activo (Inicio/Fin).  
* **Color:** Color de resaltado.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "VSA & Anomalies"  

---

### 🧠 Uso más frecuente

* **Limpieza de Ruido:** Dejar en gris las velas de bajo volumen y colorear solo las institucionales.  
* **Foco RTH:** Usar el filtro horario para ignorar la sesión asiática.  

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Utilidad:** Simple pero efectiva para reducir la carga cognitiva.  
✅ **Configurable:** Permite crear "lentes" personalizadas (ej. solo ver velas con Delta > 500).  
⛔ **Pasivo:** No genera señales, solo resalta.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ignición:** Buscar velas coloreadas (alto volumen) al inicio de un movimiento.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Type:** `Volume`.  
* **MinFilter:** `1500`.  
* **TimeFilter:** `15:30 - 22:00` (RTH).  

---

### 🧪 Notas de desarrollo

* Usa `PaintbarsDataSeries`.  
* Lógica simple de comparación.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Defaults:** El `MaxFilter` activado por defecto en 100 es confuso.  

---

### 🛠️ Propuestas de mejora

* **P2:** Añadir alertas sonoras cuando se detecta una vela filtrada.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es el "marcador fluorescente" del trader. No te dice qué leer, pero te subraya lo importante.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para focalizar la atención.

**Acción:** **Conservar (Reserva).**