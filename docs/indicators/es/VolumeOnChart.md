---
# 1. IDENTIFICACIÓN
cs_file: VolumeOnChart.cs
name: Volume On The Chart
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: "Standard Volume"

# 3. VALORACIÓN (Score & Priority)
score_current: 8/10
score_potential: 9/10
file_state: Estable
effort: Bajo
action_priority: Nula
system_priority: P2

# 4. DECISIÓN
recommended_action: Conservar (Reserva)

# 5. ANÁLISIS
description: Visualiza el volumen como un histograma de fondo superpuesto al precio para ahorrar espacio.
gemini_summary: "Versión optimizada para el espacio de trabajo. Dibuja el volumen en el fondo del gráfico de precios, eliminando la necesidad de un sub-panel dedicado. Ideal para portátiles o configuraciones multi-gráfico."
competitor_notes: "Alternativa visual a 'Volume' para ahorrar espacio."
reusable_code: "Lógica de escalado visual relativo al panel."

# 6. METADATOS
analysis_date: 2025-12-10
official_code_date: 2025-04-23
---

## 🛡️ Volume On The Chart (8/10)

**Nombre del archivo:** [`VolumeOnChart.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VolumeOnChart.cs)  
**Nombre del indicador:** Volume On The Chart  
**Web oficial:** [ATAS — Volume On The Chart](https://help.atas.net/support/solutions/articles/72000619334)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** Visualiza el volumen como un histograma de fondo superpuesto al precio para ahorrar espacio.

![VolumeOnChart](../../img/VolumeOnChart.png)

---

### ⚙️ Parámetros configurables

* **Height:** Porcentaje de altura del panel para el volumen máximo (ej. 15%).
* **Location:** Posición (`Up`, `Down`, `Middle`).
* **Heredados:** Mantiene filtros y alertas del indicador `Volume` base.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Standard Volume"  

---

### 🧠 Uso más frecuente

* **Optimización de Espacio:** Eliminar paneles inferiores para maximizar la visión de la acción del precio.
* **Correlación Visual:** Evaluar "Esfuerzo (Volumen) vs Resultado (Rango de Vela)" de un vistazo rápido sin mover los ojos.

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Eficiencia:** Libera espacio valioso en la pantalla.  
✅ **Potencia:** Al heredar de `Volume.cs`, mantiene todas las funciones avanzadas (Delta Color, Alertas).  
✅ **Estética:** Se integra muy bien visualmente.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Las mismas que Volume Standard.**

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Justificación |
| :--- | :--- | :--- |
| **Height** | `15` | Suficiente para ver picos sin interferir con las velas. |
| **Location** | `Down` | Base del gráfico. |

---

### 🧪 Notas de desarrollo

* Hereda de `Volume`, lo que garantiza estabilidad y paridad de funciones.
* Usa `context.FillRectangle` en el panel principal (`CandlesPanel`).

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Opacidad:** Los colores sólidos pueden a veces dificultar la visión de la mecha inferior de una vela si coinciden en posición.

---

### 🛠️ Propuestas de mejora

* **Transparencia (Alpha):** Sería ideal añadir un control de opacidad (Alpha channel) a los colores para que siempre se vea el precio detrás.

---

### 💎 Valor Reutilizable (Código Donante)

* **Escalado Visual:** El cálculo de `maxHeight` relativo al panel es útil para cualquier indicador que dibuje en el fondo.

---

### ✍️ La opinión de Gemini sobre el Indicador

Si operas en un portátil o te gusta tener muchos gráficos abiertos, este indicador es mejor que el estándar. Si tienes una pantalla 4K dedicada, el estándar es más limpio. Cuestión de gustos, pero técnicamente es impecable.

**Propuestas de Acción:**
* **Conservar como Reserva** (Alternativa preferente para setups compactos).

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

**Acción:** **Conservar (Reserva)**