---

# 1. IDENTIFICACIÓN  
cs_file: Donchian.cs  
name: Donchian Channel  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Extremes & Range Structure  
comparison_group: "Extremes & Range Structure"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8/10  
score_potential: 9/10  
file_state: Estable  
effort: Bajo  
action_priority: Media  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Cuál es el rango (máximo y mínimo) de las últimas N barras y cuál es su punto medio (equilibrio)?  
gemini_summary: "Canal de rango intradía muy accionable (alto/bajo + midline opcional), con visualización completa. Requiere revisar coherencia del lookback (posible +1 barra) para alinear con la definición estándar."  
competitor_notes: "Gana a HighLow por añadir midline opcional y robustez ante High/Low=0. Highest/Lowest quedan como utilidades para extremos sobre SourceDataSeries, pero no compiten como canal completo."  
reusable_code: "Patrón de multi-series (high/low/average) con descripciones y colores; fallback de datos cuando High/Low=0."  

# 6. METADATOS  
analysis_date: 2025-12-28  
official_code_date: 2025-04-23  




---

## 🟦 Donchian Channel (8/10)

**Nombre del archivo:** [`Donchian.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Donchian.cs)  
**Nombre del indicador:** Donchian Channel  
**Web oficial:** [ATAS — Donchian Channel](https://help.atas.net/support/solutions/articles/72000602376)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es el rango (máximo y mínimo) de las últimas N barras y cuál es su punto medio (equilibrio)?  

![Donchian Channel](../../img/Donchian.png)



---

### ⚙️ Parámetros configurables

**[Common / Period]**  
- **Period**: Número de barras usadas para calcular el máximo y el mínimo del canal (por defecto: 20).  

**[Visualization / Midline]**  
- **ShowAverage**: Si está activo, dibuja la línea media del canal: (High + Low) / 2. Útil como imán de equilibrio y referencia de mean reversion.  



---

### 🧭 Clasificación
**Grupo:** Market Structure  
**Subgrupo:** Extremes & Range Structure  
**Comparison Group:** "Extremes & Range Structure"  



---

### 🧠 Uso más frecuente

* Delimitar el “campo de juego” inmediato: **resistencia dinámica** (canal superior) y **soporte dinámico** (canal inferior).  
* Detectar **rupturas** (breakouts) y **falsas rupturas** (fail breaks) contra el rango reciente.  
* Usar la **línea media** (si `ShowAverage=true`) como pivote de equilibrio intradía (mean reversion / rotation).  



---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Canal completo (alto/bajo) y midline opcional para lectura de equilibrio.  
✅ Muy legible y directamente accionable en M1.  
⛔ Posible incoherencia de lookback (puede estar usando N+1 velas cuando `bar >= Period`).  



---

### 🎯 Estrategias de scalping donde se aplica

* **Breakout validado**: ruptura del canal superior/inferior y continuación (idealmente confirmada con Order Flow).  
* **Fail break / Reversión**: barrida del canal y retorno rápido hacia midline.  
* **Rotación de rango**: entradas hacia el centro cuando el precio respeta canal y la midline actúa como imán.  



---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor recomendado | Impacto | Justificación operativa |
|---|---:|---|---|
| Period | 20 | Contexto | Captura un rango “suficientemente reciente” sin micro-ruido. |
| ShowAverage | true | Timing | La midline mejora decisiones de rotación/mean reversion y gestión. |  



---

### 🧪 Notas de desarrollo

* Arquitectura: 3 series (`High`, `Low`, `Average`) con colores y descripciones; diseño correcto para visualización.  
* Robustez de datos: si `candle.High` o `candle.Low` vienen a cero, usa fallback con `Open/Close` para evitar distorsiones.  
* Rendimiento: cálculo O(Period) por barra; aceptable para M1 con Period típico (15–40).  



---

### ❗ Incoherencias o aspectos mejorables detectados

* **Posible off-by-one en el lookback**: el bucle recorre desde `bar` hasta `bar - Period` inclusive cuando `bar >= Period`, lo que equivale a **Period+1 velas**.  
  - Riesgo: canal ligeramente más ancho de lo esperado y divergencia con otros Donchian estándar.  
  - Recomendación: ajustar límite inferior a `bar - (Period - 1)` (manteniendo el caso inicial `bar < Period`).  



---

### 🛠️ Propuestas de mejora

* Corregir el lookback para garantizar “N barras” exactas (coherencia matemática y comparabilidad).  
* Añadir opción de coloreado contextual (por ejemplo, midline ascendente/descendente).  
* Añadir alertas cuando el precio toque o rompa el canal (si el framework del repo lo permite).  



---

### 💎 Valor Reutilizable (Código Donante)

* Patrón de **canal multi-serie** (alto/bajo/medio) con control de visibilidad y metadatos de serie.  
* Fallback defensivo cuando High/Low vienen a cero (útil para feeds incompletos).  



---

### ✍️ La opinión de ChatGPT sobre el Indicador

Es un indicador de **contexto estructural** de primer orden para scalping: reduce el espacio de decisión a tres niveles operables (alto, bajo, y medio). En M1 aporta un “mapa” inmediato del rango reciente y permite transformar muchas decisiones discrecionales en reglas: ruptura, fallo de ruptura o rotación hacia el centro.  
La única razón por la que no sube directamente a 9/10 es la **coherencia del lookback**: aunque no rompe el indicador, puede sesgar comparativas y expectativas. Arreglado eso, su valor estratégico aumenta.  



---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí**  

Aporta contexto y niveles operables con gran claridad visual.  

**Acción:** **Conservar (Core)**  

