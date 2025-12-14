---
# 1. IDENTIFICACIÓN  
cs_file: MarketFacilitation.cs  
name: Market Facilitation Index  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume  
comparison_group: "Volume Efficiency"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 5.5/10  
score_potential: 7.5/10  
file_state: Estable  
effort: Medio  
action_priority: Media  
system_priority: NA  

# 4. DECISIÓN  
recommended_action: Descartar  

# 5. ANÁLISIS  
description: ¿Cuánta “facilidad” tiene el mercado para mover el precio por unidad de volumen (rango por volumen) en cada vela?  
gemini_summary: "Calcula rango/volumen correctamente, pero es redundante con VBRR (inverso) y sin clasificación visual pierde gran parte de la utilidad operativa."  
competitor_notes: "Dentro del grupo, es inferior a VBRR: misma idea (eficiencia precio-volumen) pero invertida y sin un plus funcional que justifique un slot adicional."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-14  
official_code_date: 2025-04-23  
---

## 🟫 Market Facilitation Index (5.5/10)  

**Nombre del archivo:** [`MarketFacilitation.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MarketFacilitation.cs)  
**Nombre del indicador:** Market Facilitation Index  
**Web oficial:** [ATAS — Market Facilitation Index](https://help.atas.net/support/solutions/articles/72000602423)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuánta “facilidad” tiene el mercado para mover el precio por unidad de volumen (rango por volumen) en cada vela?  

![MarketFacilitation](../../img/MarketFacilitation.png)  

  

---  

### ⚙️ Parámetros configurables  

- **Multiplier:** factor de escala para ajustar la magnitud del histograma. 

  

---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Efficiency"  

  

---  

### 🧠 Uso más frecuente  

- Lectura de **eficiencia mecánica**: cuánto rango genera cada vela por volumen negociado.
- Contexto (secundario) para distinguir movimientos “baratos” (mucho rango con poco volumen) de movimientos “caros” (poco rango con mucho volumen).  

  

---  

### 📊 Nivel de relevancia  
🔟 **5.5 / 10**  

✅ Cálculo simple y barato (O(1) por vela).
✅ Interpretación razonable como “rango/volumen”. 
⛔ Redundante con VBRR (inverso) y aporta poco edge adicional en el set final.  
⛔ Sin clasificación/visual “event-driven” se convierte en otro histograma más.  

  

---  

### 🎯 Estrategias de scalping donde se aplica  

- **Filtro de “vacío de liquidez”** (con cautela): si el valor se dispara y el mercado se mueve con poco volumen, puede indicar desplazamiento frágil y riesgo de V-shape.  
- **No recomendado como trigger**; solo contexto si ya estás trabajando con niveles/footprint.  

  

---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| Multiplier | 1 | Mantener escala neutra; cualquier ajuste es puramente visual.|  

  

---  

### 🧪 Notas de desarrollo  

- Fórmula actual: `(High - Low) * Multiplier / Volume`.  
- Incluye guard para `Volume == 0` devolviendo 0.
- Es, en la práctica, la **inversa conceptual** de VBRR (`Volume / Range`).   

  

---  

### ❗ Incoherencias o aspectos mejorables detectados  

- **Redundancia fuerte** con VBRR (mide lo mismo desde el inverso).  
- **Falta de capa interpretativa**: no hay señales, clasificación, ni normalización por sesión; en vivo requiere demasiada interpretación subjetiva.  

  

---  

### 🛠️ Propuestas de mejora  

- **P2 (Media):** añadir modo “Bill Williams-style” opcional: clasificación por estados basada en (a) cambio del índice vs vela previa y (b) cambio de volumen vs vela previa, con 4 colores/estados y leyenda.  
- **P2 (Media):** normalización por sesión (z-score o percentil) para que los umbrales sean estables entre días.  
- **P3 (Baja):** si existe VBRR en el set, este indicador debería desaparecer o quedar solo como alternativa de visual (invertido).  

  

---  

### 💎 Valor Reutilizable (Código Donante)  

- **Ninguno** (implementación minimalista).  

  

---  

### ✍️ La opinión de ChatGPT sobre el Indicador  

Este indicador no es “malo”; es **demasiado parecido a VBRR** y no ofrece una lectura superior para scalping. Con un set limitado (que es lo que necesita un scalper), su presencia compite por atención sin aportar edge incremental claro. Solo lo reconsideraría si implementas una **capa interpretativa objetiva** (estados/colores + normalización) que lo convierta en una herramienta “event-driven” y no en un histograma más.  

  

---  

### 📈 Veredicto: ¿Es útil para Scalping?  

**No, en su estado actual (dentro del set final).**  

Redundante con VBRR y sin capa interpretativa aporta poco en tiempo real.  

**Acción:** **Descartar**  