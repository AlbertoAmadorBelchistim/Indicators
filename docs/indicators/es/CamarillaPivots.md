---

# 1. IDENTIFICACIÓN  
cs_file: CamarillaPivots.cs  
name: Camarilla Pivots  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Structural Levels  
comparison_group: "Structural Levels"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8/10  
score_potential: 9/10  
file_state: Estable  
effort: Bajo  
action_priority: Baja  
system_priority: P2  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Cuáles son los niveles Camarilla (reversión/ruptura) que definen extremos y pivotes intradía para estructurar un plan de trading?  
gemini_summary: "Estructura táctica más accionable que pivots clásicos. Reserva P2 excelente; subiría a 9/10 si el indicador está libre de bugs visuales/cálculo y se optimiza el filtrado."  
competitor_notes: "Comparte dominio con Pivots, pero Camarilla es más 'táctico' (niveles de reversión/ruptura). No supera a LevelsLolo porque no integra jerarquía externa ni prioriza por importancia contextual."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🧱 Camarilla Pivots (8/10)  

**Nombre del archivo:** [`CamarillaPivots.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CamarillaPivots.cs)  
**Nombre del indicador:** Camarilla Pivots  
**Web oficial:** [ATAS — Camarilla Pivots](https://help.atas.net/support/solutions/articles/72000602341)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuáles son los niveles Camarilla (reversión/ruptura) que definen extremos y pivotes intradía para estructurar un plan de trading?  

![CamarillaPivots](../../img/CamarillaPivots.png) 


---

### ⚙️ Parámetros configurables  
- **PivotColor**: Color de líneas pivote.  
- **UpperColor**: Color de niveles superiores (resistencias).  
- **LowerColor**: Color de niveles inferiores (soportes).  
- **BetweenColor**: Color para niveles intermedios (si aplica).  
- **HighLowColor**: Color para high/low de referencia (si aplica).  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Structural Levels  
**Comparison Group:** "Structural Levels"  


---

### 🧠 Uso más frecuente  
* Identificar niveles de **reversión** intradía (zonas donde el mercado suele reaccionar).  
* Definir niveles de **ruptura** (expansión) para días de tendencia.  
* Planificar objetivos y stops con una estructura predefinida.  


---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  

✅ Más accionable que pivots clásicos para setups intradía.  
✅ Buen mapa de extremos y zonas de decisión repetibles.  
⛔ Sin filtro adicional puede añadir niveles excesivos (depende de estilo).  


---

### 🎯 Estrategias de scalping donde se aplica  
* **Fade en extremos**: buscar agotamiento/absorción cerca de niveles clave.  
* **Breakout estructurado**: confirmar ruptura con Order Flow y usar niveles como targets.  
* **Mean reversion**: retorno a zona central tras excursión a extremos.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| UpperColor / LowerColor | Contraste medio | Diferenciar extremos sin “gritar” más que el precio. |  
| PivotColor | Neutro | El pivote central debe ser referencia, no señal. |  
| HighLowColor | Suave | Evita saturar con líneas redundantes. |  


---

### 🧪 Notas de desarrollo  
* Indicador predominantemente de render; coste bajo.  
* Valor práctico alto cuando se combina con confirmación de Order Flow en el nivel.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
* Si hay discrepancias de cálculo por sesión, el indicador pierde credibilidad (prioridad: coherencia).  


---

### 🛠️ Propuestas de mejora  
* Presets por instrumento (ES/NQ) para estilos/filtrado.  
* Opción “solo niveles principales” para reducir ruido en M1.  


---

### 💎 Valor Reutilizable (Código Donante)  
* N/A  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
Camarilla es un mapa táctico excelente para intradía. En un sistema serio, funciona como estructura P2: define zonas y deja que el Order Flow decida el timing. No gana frente a LevelsLolo por falta de jerarquía externa y priorización contextual.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí**  

Especialmente si tu operativa distingue “días de rango” vs “días de tendencia” y usa Camarilla como mapa previo.  

**Acción:** **Conservar (Reserva)**  
