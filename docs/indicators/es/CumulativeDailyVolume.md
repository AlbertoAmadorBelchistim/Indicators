---
cs_file: CumulativeDailyVolume.cs
name: Cumulative Daily Volume
group: Order Flow
subgroup: Volume
score_current: 6/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Cuál es el volumen total acumulado desde el inicio de la sesión?
gemini_summary: "El 'odómetro' de la sesión. Una herramienta simple de contexto que te dice si el día tiene actividad (volumen alto) o si está muerto. No sirve para entradas, solo para decidir si operar o no."
comparison_group: "Standard Volume"
competitor_notes: "Único en su función de contexto global."
reusable_code: null
file_state: Estable
score_potential: 6/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Cumulative Daily Volume (6/10)

**Nombre del archivo:** [`CumulativeDailyVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CumulativeDailyVolume.cs)  
**Nombre del indicador:** Cumulative Daily Volume  
**Web oficial:** [ATAS — Cumulative Daily Volume](https://help.atas.net/support/solutions/articles/72000618670)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

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

* **Filtro de Régimen:** Comparar la curva de hoy con la de ayer. ¿Hay más o menos interés?  
* **Detección de Festivos:** Curva plana = Mercado cerrado o sin participantes.  

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ **Contexto Puro:** Responde a "¿Hay gente hoy?".  
⛔ **Información Plana:** No desglosa nada.  

---

### 🎯 Estrategias de scalping donde se aplica

* **No Operar:** Si el volumen acumulado a las 10:00 AM es un 50% menor que el promedio, evitar sistemas de ruptura.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Color:** Gris neutro (Información de fondo).  

---

### 🧪 Notas de desarrollo

* Acumula `candle.Volume` y reinicia con `IsNewSession`. Código trivial.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Hace lo que dice.  

---

### 🛠️ Propuestas de mejora

* **Ninguna.** 

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** 

---

### ✍️ La opinión de Gemini sobre el Indicador

Es útil tenerlo en una esquina de la pantalla o en un layout de "Contexto Diario". No lo mires para entrar, míralo para saber el clima.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (Contexto).**

Para filtrar días malos.

**Acción:** **Conservar (Reserva).**