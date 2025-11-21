---
cs_file: VolumeOnChart.cs
name: Volume On The Chart
group: Order Flow
subgroup: Volume
score_current: 8/10
version: Stable
recommended_action: Conservar (Reserva)
description: Visualiza el volumen como un histograma de fondo superpuesto al precio.
gemini_summary: "Versión optimizada para el espacio de trabajo. Dibuja el volumen en el fondo del gráfico de precios, eliminando la necesidad de un sub-panel dedicado. Ideal para portátiles o configuraciones multi-gráfico."
comparison_group: "Standard Volume"
competitor_notes: "Alternativa visual a 'Volume' para ahorrar espacio."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: Bajo
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Volume On The Chart (8/10)

**Nombre del archivo:** [`VolumeOnChart.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VolumeOnChart.cs)  
**Nombre del indicador:** Volume On The Chart  
**Web oficial:** [ATAS — Volume On The Chart](https://help.atas.net/support/solutions/articles/72000619334)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

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

* **Pantallas Pequeñas:** Ganar espacio vertical eliminando paneles inferiores.  
* **Correlación Visual:** Ver la relación Tamaño de Vela vs Volumen sin mover los ojos.  

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Eficiencia:** Libera un 15-20% de pantalla.  
✅ **Funcionalidad:** Al heredar de `Volume`, no pierde potencia (alertas, delta color).  
✅ **Escalado:** Se auto-ajusta al volumen máximo visible.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Igual que Volumen Estándar.** ---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Height:** `15` (Suficiente para ver picos sin tapar precio).  
* **Location:** `Down`.  

---

### 🧪 Notas de desarrollo

* Hereda de `Volume` (`public class VolumeOnChart : Volume`), lo que es una excelente práctica de POO.
* Usa `context.FillRectangle` en el panel principal (`CandlesPanel`).

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Opacidad:** A veces los colores sólidos pueden tapar las velas si el volumen es muy alto. Se recomienda usar colores con transparencia (Alpha).

---

### 🛠️ Propuestas de mejora

* **P3:** Añadir transparencia por defecto a los colores.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Escalado Visual:** El cálculo de `maxHeight` relativo al panel es útil para cualquier indicador "On Chart".

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una gran solución de interfaz. Si tienes poco espacio, este es tu indicador. Si tienes monitores grandes, el panel separado es más limpio.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Excelente para setups compactos.

**Acción:** **Conservar (Reserva).**