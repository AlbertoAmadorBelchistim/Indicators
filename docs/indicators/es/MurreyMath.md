---

# 1. IDENTIFICACIÓN  
cs_file: MurrayMath.cs  
name: Murrey Math  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Geometric / Grid Structure  
comparison_group: "Geometric / Grid Structure"  

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
description: ¿Qué niveles armónicos Murrey (0/8–8/8 y extensiones) estructuran el espacio de precios para planificar reversiones o targets en rango?  
gemini_summary: "Indicador estable y consistente para particionar el precio en niveles armónicos. Útil como estructura auxiliar (targets/zonas) pero no debe ser mapa principal del sistema."  
competitor_notes: "En el torneo Geometric / Grid Structure, Murrey aporta más estructura que Round Numbers, pero con mayor carga cognitiva y dependencia del trader. Se mantiene como reserva P3; no alcanza CORE por falta de causalidad y por riesgo de sobreponderar grids en M1."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-27  
official_code_date: 2025-04-23  

---

## 📐 Murrey Math (8/10)  

**Nombre del archivo:** [`MurreyMath.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MurreyMath.cs)  
**Nombre del indicador:** Murrey Math  
**Web oficial:** [ATAS — Murrey Math](https://help.atas.net/support/solutions/articles/72000602435)  
**Compatibilidad:** ATAS Stable/Latest  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Qué niveles armónicos Murrey (0/8–8/8 y extensiones) estructuran el espacio de precios para planificar reversiones o targets en rango?  

![MurreyMath](../../img/MurreyMath.png)


---

### ⚙️ Parámetros configurables  
- **FrameSize**: Tamaño del marco base usado para discretizar el rango.  
- **FrameMultiplier**: Multiplicador de escala del marco (amplía o contrae el grid).  
- **IgnoreWicks**: Si se activa, reduce sensibilidad ignorando mechas para calcular el rango.  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Geometric / Grid Structure  
**Comparison Group:** "Geometric / Grid Structure"  


---

### 🧠 Uso más frecuente  
* Mapear zonas armónicas de soporte/resistencia en días laterales.  
* Definir targets e invalidaciones por escalones (niveles consecutivos).  
* Buscar confluencias con niveles exógenos o de sesión para filtrar trades.  


---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  

✅ Estructura armónica consistente para rango y mean reversion.  
✅ Útil como mapa auxiliar de objetivos (targets) y gestión.  
⛔ Curva de aprendizaje y potencial de clutter si no se filtran niveles en M1.  


---

### 🎯 Estrategias de scalping donde se aplica  
* Mean reversion: reacción en extremos (0/8, 8/8) con confirmación de Order Flow.  
* Escalones de gestión: TP parcial en nivel siguiente; SL detrás del nivel anterior.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| FrameSize | Medio | Evita grids excesivamente reactivos en M1. |  
| FrameMultiplier | 1–2 | Mantiene estabilidad intradía sin “micro-niveles”. |  
| IgnoreWicks | Según régimen | Activar en ruido alto para estabilizar; desactivar si buscas extremos reales. |  


---

### 🧪 Notas de desarrollo  
* Indicador determinista: el riesgo no está en cómputo sino en interpretación y saturación visual.  
* Su mayor valor está en confluencias y gestión, no como trigger de entrada.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
* Falta un preset explícito “Scalping M1” que atenúe subniveles y reduzca clutter.  


---

### 🛠️ Propuestas de mejora  
* Opción para dibujar solo niveles clave (0/8, 4/8, 8/8 y extensiones) en modo “Minimal”.  
* Atenuar alpha o grosor de niveles secundarios para mantener jerarquía visual.  


---

### 💎 Valor Reutilizable (Código Donante)  
* N/A  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
Murrey Math es una reserva válida como “framework de objetivos” en rango, pero no debe ocupar el rol de mapa principal. En un sistema con LevelsLolo + estructura de sesión, su aportación real es táctica: escalones de gestión y confluencia, evitando que el grid domine decisiones en M1.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí**  

Útil como capa auxiliar de gestión y confluencia en días laterales, siempre subordinado a niveles exógenos y estructura de sesión.  

**Acción:** **Conservar (Reserva)**  

