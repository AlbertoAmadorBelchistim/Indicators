---
# 1. IDENTIFICACIÓN
cs_file: BarVolumeFilter.cs
name: Bar's Volume Filter
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: "VSA & Anomalies"

# 3. VALORACIÓN (Score & Priority)
score_current: 3/10
score_potential: 4/10
file_state: Estable (Funcionalidad Limitada)
effort: N/A
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Descartar

# 5. ANÁLISIS
description: ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks?
gemini_summary: "Herramienta de filtrado visual demasiado básica. Solo permite aplicar una condición lógica a la vez (ej. Volumen > X), obligando a cargar múltiples instancias para análisis complejos. Además, su filtro horario es problemático en gráficos no temporales (Ticks, Range). Superado totalmente por 'VSA Better Volume' o la coloración nativa del indicador de Volumen."
competitor_notes: "Inferior y redundante."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-12-11
official_code_date: 2025-04-23
---

## 💀 Bar's Volume Filter (3/10)

**Nombre del archivo:** [`BarVolumeFilter.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BarVolumeFilter.cs)  
**Nombre del indicador:** Bar's Volume Filter  
**Web oficial:** [ATAS — Bar's Volume Filter](https://help.atas.net/support/solutions/articles/72000602326)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Qué velas de este gráfico cumplen mi criterio específico de Volumen, Delta o Ticks?

![BarVolumeFilter](../../img/BarVolumeFilter.png)

---

### ⚙️ Parámetros configurables

* **Type:** Criterio único (`Volume`, `Ticks`, `Delta`).
* **Min/Max Filter:** Umbral simple.
* **TimeFilter:** Filtro horario rígido.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "VSA & Anomalies"  

---

### 🧠 Uso más frecuente

* **(Limitado):** Resaltar velas con volumen extremo manual.

---

### 📊 Nivel de relevancia
🔟 **3 / 10**

⛔ **Mono-Criterio:** Solo permite una regla de color. Si quieres ver velas de alto volumen en amarillo y velas de alto delta en rojo, necesitas poner el indicador dos veces. Ineficiente.  
⛔ **Incompatible:** El filtro horario (`TimeFilter`) no tiene lógica robusta para gráficos no temporales (Ticks, Range), donde las velas no respetan límites de minutos.  
⛔ **Redundante:** El indicador *Volume* estándar ya tiene alertas y colores por Delta. El *VSA Better Volume* ya colorea por anomalías. Este indicador sobra.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Usar herramientas más avanzadas.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Eliminar.**

---

### 🧪 Notas de desarrollo

* Código simple (`if volume > min then color`). No hay lógica avanzada.

---

### ❗ Incoherencias o aspectos mejorables detectados

* UX pobre. Obliga a apilar indicadores.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es un indicador "muleta". Si tu plataforma no tuviera otra forma de colorear velas, serviría. Pero ATAS tiene herramientas mucho mejores.


---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Demasiado básico.

**Acción:** **Descartar**