---
cs_file: Absorption.cs
name: Absorption
category: Order Flow
group: Order Flow
subgroup: Footprint
score_current: 10/10
version: Stable
recommended_action: Conservar
description: ¿En qué niveles de precio se frenó el mercado a pesar de una gran agresión
  de volumen?
gemini_summary: Detector de absorción intra-vela. Compara Bid vs Ask en cada nivel
  de precio. Crítico para scalping.
file_state: Estable
score_potential: 10/10
effort: Medio
action_priority: N/A
analysis_date: 2025-11-19
user_modification_date: 2025-11-19
---

## 🟦 Absorption (10/10)

**Nombre del indicador:** Absorption  
**Web oficial:** [ATAS — Absorption](https://help.atas.net/support/solutions/articles/72000641183)  
**Compatibilidad:** ATAS versión estable y superiores.


> **La Pregunta Clave:** ¿En qué niveles de precio se frenó el mercado a pesar de una gran agresión de volumen?

![Absorption](../../img/Absorption.png)

---

### ⚙️ Parámetros configurables

* **Ratio**: Desequilibrio necesario (ej. 300 = 3x más volumen en un lado que en el otro).
* **Min Volume**: Filtro de ruido para el lado agresivo.
* **Stacked Levels**: Cuántos ticks consecutivos de absorción se necesitan para marcar la zona.
* **Days Look Back**: Limitar el cálculo histórico para rendimiento.

---

### 🧭 Clasificación
📂 OrderFlow — Detección de manos pasivas (Limit Orders) absorbiendo manos agresivas (Market Orders).

---

### 🧠 Uso más frecuente

* **Techos/Suelos de Mercado:** Los giros en V casi siempre ocurren por absorción.
* **Soporte Confirmado:** Una línea de absorción verde (Bid Support) es un lugar excelente para poner un Stop Loss defensivo o una orden de compra limitada.

---

### 📊 Nivel de relevancia
🔟 **10 / 10**

✅ **Lógica Institucional:** Detecta la mecánica real del mercado: para girar el precio, primero hay que frenarlo.  
✅ **Visualización Persistente:** Usa `LineTillTouch`. La línea se queda en el gráfico hasta que el mercado la "rompe" o testea, actuando como memoria del precio.  
✅ **Precisión:** Analiza nivel por nivel dentro de la vela (`GetVolumeLevels`).  

---

### 🎯 Estrategias de scalping donde se aplica

* **Absorption Fade:** El precio sube, aparece línea roja de absorción (Resistencia), el precio cierra lejos del máximo -> Venta.
* **Defense:** Si entras largo y aparece absorción en tu contra, salir inmediatamente.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Ratio**: `300` (3:1).
* **Min Volume**: `150` (Para filtrar ruido).
* **Stacked Levels**: `1` (Para precisión máxima) o `3` (Para zonas fuertes).

---

### 🧪 Notas de desarrollo

* **Algoritmo:** Recorre el perfil de volumen de la vela. Si `Bid > Ask * Ratio`, es soporte (alguien compró todo lo que vendían). Si `Ask > Bid * Ratio`, es resistencia.
* **Optimización:** Limpia líneas obsoletas (`HorizontalLinesTillTouch.Remove`) al recalcular en tiempo real.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta fundamental. Muchos indicadores de "Imbalance" buscan agresión que mueve el precio. Este busca agresión que *NO* mueve el precio, lo cual es una señal de reversión mucho más potente.

**Propuestas de Mejora:**
* **Grosor por Volumen:** Hacer la línea más gruesa cuanto mayor sea el volumen absorbido.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Indispensable.** Te dice dónde están "las manos grandes" parando el precio.

**Acción:** **Conservar.**