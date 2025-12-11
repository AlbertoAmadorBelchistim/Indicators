---
# 1. IDENTIFICACIÓN
cs_file: SpreadVolume.cs
name: Spread Volume
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: "VSA & Anomalies"

# 3. VALORACIÓN (Score & Priority)
score_current: 4/10
score_potential: 4/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: NA

# 4. DECISIÓN
recommended_action: Descartar

# 5. ANÁLISIS
description: ¿Quién está agrediendo más dentro del spread actual?
gemini_summary: "Intento de visualizar la agresión Bid/Ask mediante histogramas flotantes junto a la vela. En mercados rápidos (S&P 500) genera ruido visual masivo, tapando la acción del precio. Su función es realizada de forma mucho más ordenada, profesional y legible por cualquier gráfico de Footprint (Cluster)."
competitor_notes: "Totalmente superado por el Footprint."
reusable_code: null

# 6. METADATOS
analysis_date: 2025-12-11
official_code_date: 2025-10-20
---

## 💀 Spread Volume (4/10)

**Nombre del archivo:** [`SpreadVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/SpreadVolume.cs)  
**Nombre del indicador:** Spread Volume  
**Web oficial:** [ATAS — Spread Volume](https://help.atas.net/support/solutions/articles/72000602630)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-10-20  

> **La Pregunta Clave:** ¿Quién está agrediendo más dentro del spread actual?

![SpreadVolume](../../img/SpreadVolume.png)

---

### ⚙️ Parámetros configurables

* **Visuals:** Ancho, Offset, Colores.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "VSA & Anomalies"  

---

### 🧠 Uso más frecuente

* **(Obsoleto):** Intentar ver el flujo interno de la vela sin usar un gráfico de Clusters.

---

### 📊 Nivel de relevancia
🔟 **4 / 10**

⛔ **Ruido Crítico:** Dibuja rectángulos sólidos sobre el gráfico de precios. En volatilidad alta, tapa las velas y dificulta la lectura del Price Action.  
⛔ **Redundancia:** El gráfico de Footprint (Bid/Ask Ladder) organiza estos mismos datos en celdas numéricas o mapas de calor ordenados, haciendo este indicador innecesario.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Usar Footprint.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Eliminar.**

---

### 🧪 Notas de desarrollo

* El código renderiza manualmente rectángulos (`FillRectangle`) basados en una lista en memoria de los últimos trades. Funciona, pero la UX es pobre.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Visualización invasiva.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* Ninguno.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta "sucia". Si quieres ver dentro de la vela, usa la herramienta diseñada para ello (Footprint). No pintes cajas flotantes encima de tus velas.


---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Ensucia la visión.

**Acción:** **Descartar**

