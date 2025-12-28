---

# 1. IDENTIFICACIÓN  
cs_file: Zigzag.cs  
name: ZigZag pro  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Swing-Derived Structure  
comparison_group: "Swing-Derived Structure"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 9/10  
score_potential: 10/10  
file_state: Estable  
effort: Bajo  
action_priority: Media  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Qué métricas acumuladas (Delta, Volumen, Ticks, Tiempo, Barras) describe cada onda confirmada del precio para evaluar esfuerzo–resultado y agotamiento estructural?  
gemini_summary: "Indicador estructural de ondas con métricas por tramo (delta/volumen/ticks/tiempo/barras) y etiquetas en gráfico. Es el puente entre estructura y Order Flow; requiere revisar un detalle de lógica inicial de tendencia (posible condición errónea) para máxima coherencia."  
competitor_notes: "Gana por transformar swings en información cuantificable por onda. Fractals gana por niveles S/R automáticos; ambos son CORE por preguntas distintas. CMS aporta sesgo de estructura pero no cuantifica tramos. SwingHighLow marca pivotes confirmados pero es más redundante. GreatestSwing es más proyección de volatilidad que estructura."  
reusable_code: "Gestión de estado de ondas + acumuladores por tramo y renderizado de etiquetas; patrón DaysLookBack basado en sesiones."  

# 6. METADATOS  
analysis_date: 2025-12-28  
official_code_date: 2025-04-23  




---

## 🟦 ZigZag pro (9/10)

**Nombre del archivo:** [`Zigzag.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Zigzag.cs)  
**Nombre del indicador:** ZigZag pro  
**Web oficial:** [ATAS — ZigZag Pro](https://help.atas.net/support/solutions/articles/72000602632)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Qué métricas acumuladas (Delta, Volumen, Ticks, Tiempo, Barras) describe cada onda confirmada del precio para evaluar esfuerzo–resultado y agotamiento estructural?  

![Zigzag](../../img/Zigzag.png)



---

### ⚙️ Parámetros configurables

**[Calculation / DaysLookBack]**  
- **Days**: Número de sesiones hacia atrás a considerar para iniciar el cálculo. Reduce carga y evita “histórico infinito”.  

**[Calculation Settings / Swing Confirmation]**  
- **CalcMode**: Método de confirmación del giro: `Relative` (porcentaje), `Absolute` (precio), `Ticks` (ticks).  
- **Percentage**: Umbral de reversión requerido para confirmar nueva onda (interpreta “Percentage” como valor genérico según `CalcMode`).  
- **IgnoreWicks**: Si está activo, usa `max(Open,Close)` / `min(Open,Close)` para construir ondas sin mechas (estructura más “limpia”).  

**[Text Settings / Label Content]**  
- **ShowDelta / ShowVolume / ShowTicks / ShowBars**: Activa métricas en etiqueta de onda.  
- **ShowTime**: `None`, `Days`, `Exact` para duración de la onda.  
- **TextColor / TextSize**: Estilo de etiquetas.  
- **VerticalOffset**: Offset vertical (en ticks) para separar el texto del precio.  



---

### 🧭 Clasificación
**Grupo:** Market Structure  
**Subgrupo:** Swing-Derived Structure  
**Comparison Group:** "Swing-Derived Structure"  



---

### 🧠 Uso más frecuente

* Medir **esfuerzo–resultado** por tramo (Wyckoff): volumen/delta por onda vs desplazamiento real.  
* Detectar **agotamiento**: nuevas extensiones de precio con menor delta/volumen acumulado.  
* Comparar **impulsos vs correcciones** de forma cuantitativa (no subjetiva).  
* Contextualizar ejecución Order Flow: operar micro-señales solo a favor de la estructura dominante.  



---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Estructura + métricas por onda: convierte swings en decisiones medibles.  
✅ Etiquetas directas en gráfico: altísimo valor informacional por pixel.  
⛔ El último tramo repinta hasta confirmación (comportamiento inherente al ZigZag).  
⛔ Detectado un posible detalle de lógica en el arranque de tendencia bajista que conviene revisar.  



---

### 🎯 Estrategias de scalping donde se aplica

* **Wave Exhaustion**: onda a favor con menor delta/volumen que la anterior → probabilidad de rotación.  
* **Change of Character**: cambio de secuencia HH/HL a LH/LL confirmado por métrica (no solo precio).  
* **Pullback quality**: correcciones con poco esfuerzo frente a impulsos dominantes.  



---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor recomendado | Impacto | Justificación operativa |
|---|---:|---|---|
| CalcMode | Ticks | Timing | Control fino de confirmación, estable intradía. |
| Percentage | 12–20 | Sensibilidad | 12 (≈ 3 puntos ES) para estructura “corta”; 20 para filtrar ruido en alta volatilidad. |
| IgnoreWicks | true | Calidad estructura | Reduce “barridos” por mecha; mejora lectura HH/HL. |
| Days | 5–20 | Rendimiento | Limita histórico y mantiene contexto relevante. |
| ShowDelta | true | Decisión | Divergencias estructurales de agresión por onda. |
| ShowVolume | true | Decisión | Esfuerzo acumulado por tramo. |
| ShowTicks / ShowBars | opcional | Diagnóstico | Útil para comparar eficiencia del movimiento. |
| ShowTime | Exact | Diagnóstico | Duración como proxy de absorción/rotación. |  



---

### 🧪 Notas de desarrollo

* Mantiene estado con `_direction`, `_lastHighBar`, `_lastLowBar` y confirma giros por `requiredChange`.  
* Acumula por onda `Volume`, `Delta`, `Ticks`, `Bars` y `Duration`, y los renderiza como etiqueta en el extremo confirmado.  
* Implementa recorte por sesiones vía `Days` calculando `_targetBar` con `IsNewSession()`, lo cual mejora rendimiento.  



---

### ❗ Incoherencias o aspectos mejorables detectados

* Posible bug lógico en detección inicial de tendencia bajista cuando `_direction == 0`: condición compara `candleHigh` con `candleZeroLow`, lo cual parece asimétrico respecto al caso alcista y puede retrasar el seteo de `_direction = -1`.  
  - Recomendación: revisar esa condición para coherencia (probable comparación contra `candleZeroHigh`).  



---

### 🛠️ Propuestas de mejora

* Revisar y corregir condición inicial de downtrend para asegurar coherencia del primer tramo.  
* Añadir opción de mostrar HH/HL/LH/LL explícitos (etiquetas mínimas) sin saturar el gráfico.  
* Permitir selección de qué acumuladores se calculan para reducir coste si solo interesa delta/volumen.  



---

### 💎 Valor Reutilizable (Código Donante)

* Patrón de acumulación por tramo y renderizado de métricas estructurales en gráfico.  
* Lógica de recorte por sesiones (Days lookback) para indicadores costosos.  



---

### ✍️ La opinión de ChatGPT sobre el Indicador

Es un CORE real porque hace algo que el ojo humano no puede cuantificar de forma consistente: convierte cada onda confirmada en un “paquete de información” (esfuerzo, duración, tamaño, agresión). Para scalping en M1, esto eleva el contexto estructural y evita operar micro-señales contra un tramo dominante o en agotamiento evidente. Corrigiendo el detalle del arranque de downtrend, el indicador queda prácticamente redondo.  



---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí**  

Es un núcleo de contexto estructural cuantitativo, complementario a niveles y Order Flow.  

**Acción:** **Conservar (Core)**  

