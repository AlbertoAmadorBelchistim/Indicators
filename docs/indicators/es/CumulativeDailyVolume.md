---
# 1. IDENTIFICACIÓN
cs_file: CumulativeDailyVolume.cs
name: Cumulative Daily Volume
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: "Standard Volume"

# 3. VALORACIÓN (Score & Priority)
score_current: 6/10
score_potential: 6/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: P3

# 4. DECISIÓN
recommended_action: Conservar (Reserva)

# 5. ANÁLISIS
description: ¿Cuál es el volumen total acumulado desde el inicio de la sesión?
gemini_summary: "El 'odómetro' de la sesión. Una herramienta simple de contexto que te dice si el día tiene actividad (volumen alto) o si está muerto. No sirve para entradas (timing), solo para filtro de régimen (operar o no operar)."
competitor_notes: "Único en su función de contexto global acumulativo."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-12-10
official_code_date: 2025-04-23
---

## 🛡️ Cumulative Daily Volume (6/10)

**Nombre del archivo:** [`CumulativeDailyVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CumulativeDailyVolume.cs)  
**Nombre del indicador:** Cumulative Daily Volume  
**Web oficial:** [ATAS — Cumulative Daily Volume](https://help.atas.net/support/solutions/articles/72000618670)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es el volumen total acumulado desde el inicio de la sesión?

![Cumulative Daily Volume](../../img/CumulativeDailyVolume.png)

---

### ⚙️ Parámetros configurables

* **HistogramColor:** Color del gráfico.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Standard Volume"  

---

### 🧠 Uso más frecuente

* **Filtro de Régimen:** Comparar la curva de hoy con la de ayer a la misma hora. ¿Estamos por encima o por debajo?
* **Detección de "Días Muertos":** Si la curva es plana, mejor no operar rupturas.

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ **Contexto Global:** Aporta la visión "macro" de la sesión.  
⛔ **Poca Acción:** No genera señales de entrada ni salida.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Filtro NO-GO:** Si volumen acumulado < X a las 10:00, no tomar trades de continuación.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Color:** Gris suave. Debe ser información de fondo, no protagonista.

---

### 🧪 Notas de desarrollo

* Código extremadamente simple: Acumula `candle.Volume` y reinicia cuando `IsNewSession` es true. Robusto y ligero.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguna. Cumple su función exacta.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es como mirar el termómetro antes de salir de casa. No te dice qué camino tomar, pero te dice si necesitas abrigo. Útil para tener en un layout de "Dashboard".

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (Como filtro).**

**Acción:** **Conservar (Reserva)**