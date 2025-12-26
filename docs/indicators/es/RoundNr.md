---

# 1. IDENTIFICACIÓN  
cs_file: RoundNr.cs  
name: Round Numbers  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Structural Levels  
comparison_group: "Structural Levels"  

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
description: ¿Dónde están los niveles psicológicos (grid) más probables para reacción del precio, y cómo los represento con un step fijo en ticks?  
gemini_summary: "Indicador baseline muy ligero. No jerarquiza ni contextualiza, pero aporta referencias psicológicas útiles para entradas/salidas y gestión."  
competitor_notes: "No compite con Pivots/Camarilla/Murrey en cálculo, sino en simplicidad. Pierde el torneo porque no aporta estructura contextual, pero se conserva como fondo operativo."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🧱 Round Numbers (7/10)  

**Nombre del archivo:** [`RoundNr.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/RoundNr.cs)  
**Nombre del indicador:** Round Numbers  
**Web oficial:** [ATAS — Round Numbers](https://help.atas.net/support/solutions/articles/72000602459)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Dónde están los niveles psicológicos (grid) más probables para reacción del precio, y cómo los represento con un step fijo en ticks?  

![RoundNr](../../img/Roundnumbers.png)


---

### ⚙️ Parámetros configurables  
- **Step**: Tamaño del escalón en ticks (distancia entre niveles).  
- **Pen**: Estilo visual de los niveles (color/grosor).  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Structural Levels  
**Comparison Group:** "Structural Levels"  


---

### 🧠 Uso más frecuente  
* Referencias psicológicas para TP/SL y “micro-gestión” en ES.  
* Medir distancias rápidas sin herramientas adicionales (grid).  
* Filtrar zonas: detectar cuándo el precio está “entre niveles” sin estructura cercana.  


---

### 📊 Nivel de relevancia  
🔟 **7 / 10**  

✅ Extremadamente ligero y limpio; coste computacional mínimo.  
✅ Útil como referencia psicológica universal (00/50, etc. según step).  
⛔ No aporta jerarquía ni contexto: requiere capa adicional para decisión.  


---

### 🎯 Estrategias de scalping donde se aplica  
* **Gestión**: TP en el siguiente round level; SL detrás del round anterior.  
* **Confluencia**: si coincide con nivel estructural (pivots, gamma, etc.), aumenta importancia.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| Step | 40 ticks (10 puntos) o 20 ticks (5 puntos) | Ajusta a tu estilo: 10p para mapa limpio; 5p para micro-gestión. |  
| Pen | Gris tenue, grosor bajo | Debe ser fondo, no señal dominante. |  


---

### 🧪 Notas de desarrollo  
* Indicador puramente visual: recorre el rango visible y dibuja niveles equiespaciados.  
* Riesgo principal: clutter si Step es demasiado pequeño para el rango intradía.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
* Falta potencial opción “Hide Text” (petición típica): si dibuja texto redundante, puede saturar.  


---

### 🛠️ Propuestas de mejora  
* Opción para ocultar etiquetas (si existen) y dejar solo líneas.  
* Preset ES: Step 40 ticks con atenuación automática por zoom.  


---

### 💎 Valor Reutilizable (Código Donante)  
* N/A  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
Round Numbers es una reserva P3 sensata: aporta una base psicológica útil con coste nulo, pero no debe confundirse con estructura “informativa”. Su valor real aparece cuando se usa como confluencia con niveles de mayor jerarquía.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí**  

Como capa de fondo para gestión y confluencias; no como generador de señales.  

**Acción:** **Conservar (Reserva)**  


