---

# 1. IDENTIFICACIÓN  
cs_file: MurrayMath.cs  
name: Murrey Math  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Structural Levels  
comparison_group: "Structural Levels"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8/10  
score_potential: 8/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Cuáles son los niveles Murrey (0/8–8/8 y derivados) que estructuran soportes/resistencias armónicos para planificar el mapa del día?  
gemini_summary: "Framework armónico consistente y útil en rango/mean-reversion. Reserva P3 por curva de aprendizaje y menor adopción general."  
competitor_notes: "Más 'framework' que Pivots, pero menos universal y más complejo. No supera a LevelsLolo por falta de integración de niveles externos y porque su valor depende del estilo del trader."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🧱 Murrey Math (8/10)  

**Nombre del archivo:** [`MurreyMath.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MurreyMath.cs)  
**Nombre del indicador:** Murrey Math  
**Web oficial:** [ATAS — Murrey Math](https://help.atas.net/support/solutions/articles/72000602435)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuáles son los niveles Murrey (0/8–8/8 y derivados) que estructuran soportes/resistencias armónicos para planificar el mapa del día?  

![MurreyMath](../../img/MurreyMath.png)


---

### ⚙️ Parámetros configurables  
- **FrameMultiplier / FrameSize**: Control del marco base para construir los niveles (impacta sensibilidad).  
- **(Opciones de visualización)**: Mostrar/ocultar niveles secundarios y estilos (si aplica).  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Structural Levels  
**Comparison Group:** "Structural Levels"  


---

### 🧠 Uso más frecuente  
* Mapear niveles armónicos en días de rango para mean-reversion.  
* Identificar “extremos” donde el mercado tiende a pausar o revertir.  
* Usar niveles como targets y zonas de invalidación estructural.  


---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  

✅ Framework consistente para estructura de rango y zonas armónicas.  
✅ Buen mapa para planificar objetivos/invalidation.  
⛔ Requiere comprensión; su utilidad depende del estilo del trader.  


---

### 🎯 Estrategias de scalping donde se aplica  
* **Mean-reversion en escalones**: operar entre niveles adyacentes con confirmación.  
* **Extremos**: esperar reacción en niveles “fuertes” y confirmar con Order Flow.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| FrameSize | Medio | Evita niveles demasiado “nerviosos” en M1. |  
| FrameMultiplier | 1–2 | Mantiene estructura intradía sin sobrerreaccionar. |  
| Niveles secundarios | Ocultos/atenuados | Reduce ruido visual y sesgo cognitivo. |  


---

### 🧪 Notas de desarrollo  
* En scalping, la clave es evitar exceso de líneas: la configuración debe ser conservadora.  
* Rendimiento esperado: bajo coste; principal impacto es el render si hay muchos niveles visibles.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
* Riesgo de “nivelitis” si se muestran demasiados subniveles en M1.  


---

### 🛠️ Propuestas de mejora  
* Preset “Intraday ES”: niveles principales + atenuación de secundarios.  
* Etiquetado opcional de niveles “fuertes” para reducir interpretación manual.  


---

### 💎 Valor Reutilizable (Código Donante)  
* N/A  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
Murrey Math es un mapa estructural válido, pero su valor es más dependiente del trader que Pivots o Camarilla. En tu sistema, lo trataría como Reserva P3: útil si lo integras como ‘framework’, pero no imprescindible para el plan diario.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí**  

Siempre que se use con configuración conservadora y como mapa de contexto, no como señal.  

**Acción:** **Conservar (Reserva)**  
