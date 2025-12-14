---
# 1. IDENTIFICACIÓN  
cs_file: VolumePerTrade.cs  
name: Volume Per Trade  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume  
comparison_group: "Volume Efficiency"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8.5/10  
score_potential: 9/10  
file_state: Estable  
effort: Bajo  
action_priority: Baja  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Cuál es el tamaño medio de la ejecución por trade en cada vela (proxy de participación institucional vs flujo fragmentado)?  
gemini_summary: "Métrica directa y rápida: volumen total dividido por número de prints. Útil para filtrar calidad de rupturas y detectar ejecución en bloques."  
competitor_notes: "Gana el grupo porque es la métrica más accionable en M1: no depende de rango (como VBRR/EMV) y se interpreta instantáneamente en rupturas y tests."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-12  
official_code_date: 2025-04-23  
---

## 🟦 Volume Per Trade (8.5/10)  

**Nombre del archivo:** [`VolumePerTrade.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VolumePerTrade.cs)  
**Nombre del indicador:** Volume Per Trade  
**Web oficial:** [ATAS — Volume Per Trade](https://help.atas.net/support/solutions/articles/72000619357)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es el tamaño medio de la ejecución por trade en cada vela (proxy de participación institucional vs flujo fragmentado)?  

![VolumePerTrade](../../img/VolumePerTrade.png)  

  

---  

### ⚙️ Parámetros configurables  

**Settings**  
- **N/A**: el cálculo es directo (sin parámetros).  

  

---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Efficiency"  

  

---  

### 🧠 Uso más frecuente  

- **Filtro de rupturas (calidad):** ruptura con volumen alto + VPT alto = ejecución menos fragmentada, más “seria”.  
- **Detección de “spray” algorítmico:** mucho volumen con VPT bajo = muchos prints pequeños (HFT/fragmentación), peor para seguir ruptura.  
- **Contexto en tests de nivel:** en toque de nivel, un pico de VPT puede sugerir absorción/ejecución agresiva concentrada.  

  

---  

### 📊 Nivel de relevancia  
🔟 **8.5 / 10**  

✅ Muy accionable en M1: lectura instantánea.  
✅ Cálculo extremadamente barato y robusto.  
⛔ Puede ser ruidoso sin suavizado; requiere contextualizar con rango/estructura.  

  

---  

### 🎯 Estrategias de scalping donde se aplica  

- **Breakout con confirmación:** solo tomar continuación si VPT acompaña (idealmente en la vela de ruptura o inmediatamente posterior).  
- **Fakeout / “air pocket”:** ruptura con volumen aparente pero VPT deprimido (prints pequeños) = mayor probabilidad de fallo.  
- **Pullback entry:** si en el pullback cae el VPT, sugiere que la contra no tiene tamaño; favorece reentrada a favor.  

  

---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| N/A | N/A | Cálculo directo; optimización real se hace vía *interpretación* (filtros/umbral contextual) y/o suavizado opcional futuro. |  

  

---  

### 🧪 Notas de desarrollo  

- Fórmula actual: `candle.Volume / candle.Ticks`.  
- Render: histograma en panel propio, sin dependencias adicionales.  
- Rendimiento: O(1) por barra, óptimo para tiempo real.  

  

---  

### ❗ Incoherencias o aspectos mejorables detectados  

- **Edge-case de división:** si `candle.Ticks == 0`, hay división por cero.
  - Es raro en ES, pero puede ocurrir en instrumentos/sesiones ilíquidas o barras especiales.  

  

---  

### 🛠️ Propuestas de mejora  

- **P2 (Baja/Media):** añadir *guard* (`if (candle.Ticks <= 0) this[bar]=0;`) y/o heredar valor previo.  
- **P2 (Media):** opción de suavizado (SMA/EMA) y/o overlay line para lectura menos ruidosa en M1.  
- **P1 (Opcional, creativo):** modo “Regime”: normalizar VPT por una media de sesión (z-score) para detectar “participación anómala” sin depender del nivel absoluto.  

  

---  

### 💎 Valor Reutilizable (Código Donante)  

- **Ninguno** (cálculo minimalista).  

  

---  

### ✍️ La opinión de ChatGPT sobre el Indicador  

Es un **CORE real** para scalping discrecional y también para un sistema semialgoritmico como filtro de calidad. No intenta “predecir”; cuantifica una propiedad microestructural útil: cuánto tamaño medio se está imprimiendo por vela. Donde más valor aporta es en **rupturas y tests de nivel**: te ayuda a distinguir “intención” (prints grandes) de “ruido” (spray). Su debilidad no es conceptual, sino de ergonomía: sin suavizado o normalización, el ojo humano puede sobre-reaccionar a picos aislados.  

  

---  

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí.**  

Útil como filtro de calidad y como confirmación de ejecución en rupturas/tests.  

**Acción:** **Conservar (Core)**  