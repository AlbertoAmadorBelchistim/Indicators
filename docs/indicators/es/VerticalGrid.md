---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: VerticalGrid.cs
name: Vertical Grid
category: Visualization
score_current: 4/10
version: Stable
recommended_action: 'Mejorar'
description: >-
  Dibuja líneas verticales y etiquetas de tiempo a intervalos regulares personalizados.
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: >-
  Herramienta visual básica. Dibuja grid vertical. Código correcto pero de utilidad limitada.
file_state: Mejorable
score_potential: 6/10
effort: Bajo
action_priority: P3
# --- Control de Versiones ---
analysis_date: 2025-11-18
official_code_date: 2024-04-25
user_modification_date: null
---

## 🟦 Vertical Grid (4/10)

**Nombre del archivo:** [`VerticalGrid.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VerticalGrid.cs)  
**Nombre del indicador:** Vertical Grid  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 25/04/2024  

> **La Pregunta Clave:** Dibuja líneas verticales y etiquetas de tiempo a intervalos regulares personalizados.

![VerticalGrid](../../img/VerticalGrid.png)

---

### ⚙️ Parámetros configurables

* **Period / PeriodType**: Intervalo (ej. 10 Minutos, 1 Hora).  
* **Estilo**: Color, grosor, texto.  

---

### 🧭 Clasificación
📂 Visualization — Grid auxiliar.

---

### 🧠 Uso más frecuente

* **Estructura Temporal:** Marcar cada hora en punto para ver la periodicidad del mercado.  
* **Gráficos No-Temporales:** En gráficos de Renko, Range o Tick, donde el eje X no es lineal, este indicador ayuda a ver dónde cambió la hora real.  

---

### 📊 Nivel de relevancia
🔟 **4 / 10**

✅ **Útil en Renko/Range:** Su mayor valor es en gráficos donde el tiempo se distorsiona. Ver líneas verticales cada 30 minutos ayuda a mantener la noción del tiempo.  
⛔ **Redundante:** En gráficos de tiempo normales (1m, 5m), la plataforma ya tiene un grid nativo mejor.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Time Patterns:** Si notas que cada 15 minutos hay un giro (algoritmos TWAP), este grid te ayuda a anticiparlo.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Period**: `15`, `Minutes`.

---

### 🧪 Notas de desarrollo

* **Render:** Dibuja líneas y etiquetas de texto en el eje X (`yBot - strSize.Height`).
* **Lógica:** Acumula barras en `_gridBars` si `Time - LastTime > Period`. Simple.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una utilidad menor. Cumple su función, pero no es algo que cambie tu trading.

**Propuestas de Mejora:**
* **Alineación:** Opción para alinear con horas "redondas" (ej. 10:00, 10:15) en lugar de empezar a contar desde el inicio de la sesión arbitrariamente.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Poco.** Salvo que uses gráficos Renko/Range.

**Acción:** **Mejorar (Alineación horaria).**