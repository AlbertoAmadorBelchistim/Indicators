---

# 1. IDENTIFICACIÓN  
cs_file: RoundNr.cs  
name: Round Numbers  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Geometric / Grid Structure  
comparison_group: "Geometric / Grid Structure"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 7/10  
score_potential: 7/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Dónde están los niveles psicológicos equiespaciados (round numbers) para apoyar gestión y confluencia en el intradía?  
gemini_summary: "Indicador de coste mínimo que añade una malla psicológica simple. Aporta valor como fondo de gestión, pero no tiene jerarquía ni contexto propio."  
competitor_notes: "En el torneo Geometric / Grid Structure, Round Numbers es el más simple y barato. Pierde frente a Murrey en estructura, pero gana en coste cognitivo. Se conserva como reserva P3 para gestión y medición, sin aspiración CORE."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-27  
official_code_date: 2025-04-23  

---

## 🔢 Round Numbers (7/10)  

**Nombre del archivo:** [`RoundNr.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/RoundNr.cs)  
**Nombre del indicador:** Round Numbers  
**Web oficial:** [ATAS — Round Numbers](https://help.atas.net/support/solutions/articles/72000602459)  
**Compatibilidad:** ATAS Stable/Latest  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Dónde están los niveles psicológicos equiespaciados (round numbers) para apoyar gestión y confluencia en el intradía?  

![RoundNr](../../img/Roundnumbers.png)


---

### ⚙️ Parámetros configurables  
- **Step**: Distancia entre niveles (en ticks).  
- **Pen**: Color y grosor de las líneas.  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Geometric / Grid Structure  
**Comparison Group:** "Geometric / Grid Structure"  


---

### 🧠 Uso más frecuente  
* Fondo de gestión: TP/SL por escalones psicológicos.  
* Medición rápida de distancias (aproximación visual) en el chart.  
* Confluencia: refuerzo cuando coincide con niveles exógenos o de sesión.  


---

### 📊 Nivel de relevancia  
🔟 **7 / 10**  

✅ Coste computacional mínimo y visual limpio si el step está bien elegido.  
✅ Referencia psicológica universal (niveles “redondos”).  
⛔ No aporta jerarquía ni contexto; puede inducir exceso de líneas si el step es pequeño.  


---

### 🎯 Estrategias de scalping donde se aplica  
* Gestión por escalones: TP en el siguiente round; SL detrás del round previo.  
* Confluencia: priorizar reacción si coincide con nivel exógeno o de sesión.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| Step | 20–40 ticks | Grid suficientemente espaciado para evitar clutter en M1. |  
| Pen | Fino y tenue | Debe ser referencia de fondo, no señal dominante. |  


---

### 🧪 Notas de desarrollo  
* El riesgo principal es el ajuste de Step: demasiado pequeño produce ruido visual y sesgo de “ver niveles en todas partes”.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
* Falta un preset “ES M1” que sugiera steps típicos y estilo tenue para evitar sobrecarga.  


---

### 🛠️ Propuestas de mejora  
* Añadir presets por instrumento (ES/NQ) y por escala (M1/M5).  
* Opción para resaltar solo múltiplos mayores (por ejemplo, cada 4 niveles) con jerarquía visual.  


---

### 💎 Valor Reutilizable (Código Donante)  
* N/A  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
Round Numbers es una reserva de bajo coste y bajo riesgo cuando se usa como fondo. No debe competir con niveles exógenos ni de sesión; su valor real es ayudar a gestionar y medir sin saturar el proceso de decisión.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí**  

Útil como capa auxiliar de gestión y confluencia, con Step suficientemente grande para no contaminar M1.  

**Acción:** **Conservar (Reserva)**  



