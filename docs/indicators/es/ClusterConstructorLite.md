

---
# 1. IDENTIFICACIÓN  
cs_file: ClusterConstructorLiteLab.cs  
name: Cluster Constructor Lite
version: Custom v0.1 (Lab)  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Footprint  
comparison_group: "Cluster Analysis"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 4/10  
score_potential: 7/10  
file_state: Estable  
effort: Medio  
action_priority: Nula  
system_priority: NA  

# 4. DECISIÓN  
recommended_action: Descartar  

# 5. ANÁLISIS  
description: ¿Aparece en esta vela un patrón intrabar muy específico de doble máximo exacto de volumen (Double Max Volume) y cómo se distribuye dentro de la vela?  
gemini_summary: "Detecta exclusivamente patrones de doble máximo exacto de volumen intrabar, pero carece de filtros contextuales y de accionabilidad directa."  
competitor_notes: "Pierde claramente frente a ClusterSearchModif (selector CORE) y ClusterStatisticModif (validador estadístico). Su detección es extremadamente restrictiva y no integra delta, velocidad ni imbalances, quedando como herramienta de laboratorio."  
reusable_code: "Implementación exacta de detección de rachas consecutivas de volumen máximo (==2) y lógica de clearing intrabar con alerta en cierre de vela."  

# 6. METADATOS  
analysis_date: 2025-12-25  
official_code_date: Unknown  
user_modification_date: 2025-12-25  
---  

## 🟦 Cluster Constructor Lite (4/10)  

**Nombre del indicador:** Cluster Constructor Lite  
**Web oficial:** [JustScalpit — Cluster Constructor Lite](https://justscalpit.com/free-indicators-for-atas-platform/)  
**Compatibilidad:** ATAS Stable/Latest  
**Última revisión del código oficial:** Unknown  

> **La Pregunta Clave:** ¿Aparece en esta vela un patrón intrabar muy específico de doble máximo exacto de volumen (Double Max Volume) y cómo se distribuye dentro de la vela?  

![Imagen](../../img/ClusterConstructorLite.png)  


---

### ⚙️ Parámetros configurables  

#### Filters  
- **Search Pattern (SearchDoubleMax)**: Activa la detección estricta de patrón *Double Max Volume*.  
- **Region (Search Area)**:  
  - `FullCandle`: Evalúa toda la vela.  
  - `WicksOnly`: Limita la detección a las mechas.  
- **Position (Cluster Position)**:  
  - `All`: Dos niveles máximos en cualquier posición.  
  - `Nearby`: Exactamente dos niveles consecutivos (racha == 2).  
  - `Separate`: Dos niveles máximos no consecutivos.  
- **Min Volume / Max Volume**: Filtros de volumen absoluto del clúster máximo.  

#### Visuals  
- **Signal Color**: Color de la vela cuando se detecta el patrón.  

#### Alerts  
- **Use Alerts**: Activa alerta sonora al cierre de la vela válida.  
- **Alert File**: Archivo de sonido asociado.  


---

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Cluster Analysis"  


---

### 🧠 Uso más frecuente  

* **Investigación de micro-patrones intrabar** en footprint.  
* **Auditoría visual** de velas con doble nodo exacto de volumen máximo.  
* Validar hipótesis estadísticas sobre **bloqueos o indecisión extrema** en una vela aislada.  


---

### 📊 Nivel de relevancia  
🔟 **4 / 10**  

⛔ Patrón extremadamente restrictivo (igualdad exacta de volumen).  
⛔ Sin delta, velocidad, imbalances ni filtros contextuales.  
⛔ No genera setups operables por sí solo.  


---

### 🎯 Estrategias de scalping donde se aplica  

* **Ninguna directa.**  
Este indicador **no debe usarse como generador de entradas**. Su rol es exclusivamente analítico o experimental.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor | Justificación |  
|---|---:|---|  
| Region | FullCandle | Mayor probabilidad estadística del patrón. |  
| Position | Nearby | Aísla el caso más “estructural” (dos nodos contiguos). |  
| Min Volume | 500–1000 | Evita dobles máximos irrelevantes. |  
| Use Alerts | False | Evita ruido operativo; solo para estudio. |  


---

### 🧪 Notas de desarrollo  

* La detección exige **exactamente dos niveles consecutivos con volumen máximo** (`maxStreak == 2`).  
* Incluye lógica de **clearing intrabar** y disparo de alerta **solo al cierre de vela**.  


---

### ❗ Incoherencias o aspectos mejorables detectados  

* El patrón “Double Max exacto” es estadísticamente poco frecuente en mercados líquidos.  
* Carece de tolerancia relativa (% del maxVol), lo que reduce severamente su aplicabilidad.  


---

### 🛠️ Propuestas de mejora  

* Crear un **nuevo indicador independiente** (no este) con:  
  - tolerancia relativa (≥ X% del maxVol),  
  - integración de delta o velocidad,  
  - filtros de localización coherentes con ClusterSearchModif.  


---

### 💎 Valor Reutilizable (Código Donante)  

* Algoritmo de detección de **rachas consecutivas exactas** por nivel de precio.  
* Patrón de gestión de estado intrabar y alerta diferida al cierre.  


---

### ✍️ La opinión de Gemini sobre el Indicador  

ClusterConstructorLite no tiene cabida como herramienta operativa dentro de un sistema moderno de Order Flow. Su valor está en el aprendizaje, la auditoría y la validación de hipótesis, no en la ejecución.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**No.**

El indicador no aporta edge operativo frente a los CORE del grupo (*ClusterSearchModif* y *ClusterStatisticModif*).  
Su patrón es demasiado restrictivo y no se integra en el flujo real de ejecución.

**Acción:** **Descartar**
