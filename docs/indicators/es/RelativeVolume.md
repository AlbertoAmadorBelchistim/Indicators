---
cs_file: RelativeVolume.cs
name: Relative Volume
group: Order Flow
subgroup: Volume
score_current: 7/10
version: Stable
recommended_action: Conservar (Reserva)
description: ¿Es el volumen actual anómalamente alto o bajo comparado con el promedio histórico para esta misma hora?
gemini_summary: "El contexto temporal. Compara el volumen de ahora con el volumen de 'ayer a esta hora'. Vital para no confundir volumen alto de apertura con volumen alto real."
comparison_group: "VSA & Anomalies"
competitor_notes: "Único. Aporta el contexto 'Time-of-Day' que le falta al Better Volume."
reusable_code: "Lógica de promedio por hora (Dictionary<TimeSpan, AvgBar>)"
file_state: Mejorable
score_potential: 8/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Relative Volume (7/10)

**Nombre del archivo:** [`RelativeVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/RelativeVolume.cs)  
**Nombre del indicador:** Relative Volume  
**Web oficial:** [ATAS — Relative Volume](https://help.atas.net/support/solutions/articles/72000602457)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Es el volumen actual anómalamente alto o bajo comparado con el promedio histórico para esta misma hora?

![RelativeVolume](../../img/RelativeVolume.png)

---

### ⚙️ Parámetros configurables

* **LookBack:** Número de días atrás para calcular el promedio (Default: 20).  
* **DeltaColored:** Opción para colorear por delta.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "VSA & Anomalies"  

---

### 🧠 Uso más frecuente

* **Normalización:** Saber si 5000 contratos a las 16:00 es mucho (RVOL > 1.5) o normal (RVOL = 1.0).  
* **Filtro de Ruptura:** Exigir RVOL > 1.5 para validar un breakout.  

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ **Contexto Temporal:** Resuelve el problema de la estacionalidad intradía (la "U" de volumen).  
⛔ **Limitación:** Solo funciona en gráficos de tiempo (TimeFrame). En Renko o Range falla.  

---

### 🎯 Estrategias de scalping donde se aplica

* **News Trading:** Ver si el volumen tras una noticia es realmente excepcional.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **LookBack:** `20` (Un mes de trading aprox).  

---

### 🧪 Notas de desarrollo

* Usa un `Dictionary<TimeSpan, AvgBar>` para guardar el historial de cada minuto del día.  
* **Defecto:** Pequeño lag en la actualización del promedio del día en curso.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **TimeFrame:** No avisa si se usa en un gráfico no soportado (ej. Tick).  

---

### 🛠️ Propuestas de mejora

* **P3:** Añadir aviso visual si `ChartType` no es TimeFrame.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica RVOL:** El uso del Diccionario por `TimeOfDay` es exportable a cualquier indicador que necesite estacionalidad.  

---

### ✍️ La opinión de Gemini sobre el Indicador

Esencial para entender si el mercado está "caliente" o "frío" ajustado a la hora. 2000 contratos a las 3 AM es una locura; a las 9:30 AM es silencio. Este indicador te dice la diferencia.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para filtrar falsas rupturas en horas muertas.

**Acción:** **Conservar (Reserva).**

