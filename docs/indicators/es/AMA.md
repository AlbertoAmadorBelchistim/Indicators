---
cs_file: AMA.cs
name: Adaptive Moving Average
category: Trend
group: Trend
subgroup: Moving Average
score_current: 7/10
version: Estable
recommended_action: Mejorar
description: ¿Cómo puedo obtener una media móvil suave que _no_ tenga retardo (lag)
  durante una ruptura fuerte, pero _sí_ filtre el 'ruido' en un mercado lateral?
gemini_summary: El mejor filtro de régimen (7/10). Rápido en tendencias, lento en
  rangos. Potencial 9/10 si colorea su línea según la velocidad.
file_state: Mejorable
score_potential: 9/10
effort: Medio
action_priority: P2 (Mejora Estratégica)
analysis_date: 2025-11-17
official_code_date: 23/04/2025
---

## 🟦 Adaptive Moving Average (AMA) (7/10 | Potencial: 9/10)

**Nombre del archivo:** [`AMA.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/AMA.cs)  
**Nombre del indicador:** Adaptive Moving Average  
**Web oficial:** [ATAS - Adaptive Moving Average](https://help.atas.net/support/solutions/articles/72000602310)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

>**La Pregunta Clave:** ¿Cómo puedo obtener una media móvil suave que _no_ tenga retardo (lag) durante una ruptura fuerte, pero _sí_ filtre el 'ruido' en un mercado lateral?

![AMA](../../img/AMA.png)

----------

### ⚙️ Parámetros configurables

-   **Period**: Periodo de cálculo para la eficiencia adaptativa (por defecto: `15`)
    
-   **FastConstant**: Constante para el extremo rápido de la media móvil (por defecto: `3`)
    
-   **SlowConstant**: Constante para el extremo lento de la media móvil (por defecto: `20`)
    

----------

### 🧭 Clasificación

📂 Trend — Indicador de filtro de régimen (Tendencia vs. Rango) adaptativo.

----------

### 🧠 Uso más frecuente

-   Obtener una media móvil que **se adapta dinámicamente a la eficiencia del mercado**.
    
-   Identificar visualmente el régimen del mercado:
    
    -   **Línea plana:** Mercado en rango ("chop").
        
    -   **Línea con pendiente:** Mercado en tendencia.
        
-   Actuar como un soporte/resistencia dinámico que se acelera en tendencias y se frena en rangos.
    

----------

### 📊 Nivel de relevancia

🔟 **7 / 10**

✅ Excelente filtro de régimen: El mejor que hemos visto para diferenciar "tendencia" de "rango" de forma visual.

✅ Adaptativo: Se acelera para pegarse al precio en breakouts (reduciendo el lag) y se frena para cortar el ruido (aumentando el lag) en rangos.

✅ Muy superior a indicadores clásicos como Alligator o ADX para el mismo propósito.

⛔ No es una señal de entrada por sí misma, sino un filtro de contexto.

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Filtro de Contexto / Régimen:**
    
    -   **Si AMA está plano:** Activar estrategias de reversión a la media. Prohibido operar breakouts.
        
    -   **Si AMA tiene pendiente:** Activar estrategias de continuación/pullback. Prohibido operar contra-tendencia.
        
-   **Soporte/Resistencia Dinámico:** En una tendencia fuerte (AMA con pendiente), comprar en los pullbacks al AMA.
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **Period**: `10`
    
-   **FastConstant**: `2`
    
-   **SlowConstant**: `30`
    
-   _Nota: Esta es la configuración canónica de Kaufman (creador del KAMA), que es ligeramente más reactiva que la de por defecto, siendo ideal para 1M._
    

----------

### 🧪 Notas de desarrollo

-   El indicador implementa el **Kaufman’s Adaptive Moving Average (KAMA)**.
    
-   Calcula un "Ratio de Eficiencia" (ER): `ER = |Movimiento Neto (Period)| / Suma(Movimientos Absolutos(Period))`.
    
-   Este ER (de 0 a 1) ajusta la constante de suavizado de una EMA.
    
-   **Si el ER es alto** (movimiento eficiente/tendencia), la media se acelera (usa `FastConstant`).
    
-   **Si el ER es bajo** (movimiento ineficiente/rango), la media se frena (usa `SlowConstant`).
    
-   La fórmula (`c = c * c;`) aplica un "cuadrado" a la constante de suavizado, lo que hace que la media se vuelva _extremadamente_ lenta en los rangos.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   El indicador es funcional y fiel a la fórmula original. No se detectan incoherencias.
    

----------

### 🛠️ Propuestas de mejora (Prioridad P2)

* **¡Mejora Transformacional!:** Añadir un modo de **coloreado de línea adaptativo**. La línea AMA debería cambiar de color según su velocidad (el factor `c` interno).
    * Ej: Color `Lento` (gris) si `c` está cerca del `SlowConstant`.
    * Ej: Color `Rápido` (azul) si `c` está cerca del `FastConstant`.
* Añadir opción para **visualizar el "Ratio de Eficiencia"** en un panel separado para ver *por qué* la media se está acelerando o frenando.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

**Sí, absolutamente. Este es un indicador para "Conservar".**

Es una herramienta de contexto fantástica que funciona como un **filtro de régimen** (Tendencia vs. Rango) de una forma mucho más rápida y visual que el ADX.

La imagen de la ficha es el ejemplo perfecto de por qué este indicador es tan bueno:

1.  **Modo Rango (08:30 - 09:25):** El precio se mueve lateral. El AMA detecta este "ruido" (baja eficiencia) y se **aplana como una tabla**. Te está diciendo visualmente: "No operes rupturas, estamos en un rango".
    
2.  **Modo Tendencia (09:25):** El precio rompe el AMA plano con fuerza. El indicador detecta un movimiento "eficiente" y **acelera bruscamente**, pegándose al precio y actuando como una resistencia dinámica.
    
3.  **Modo Rango (10:20 - 14:30):** El precio vuelve al "chop". El AMA se **aplana de nuevo**, definiendo perfectamente el nuevo rango de consolidación.
    

Para un scalper, esto es oro. Te da un "interruptor" visual inmediato para cambiar tu mentalidad de "operar rangos" a "operar tendencias".

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una herramienta de contexto (filtro de régimen) de primera categoría (7/10).**

Es superior a los filtros de régimen clásicos (como Alligator o ADX) porque es adaptativo: se aplana en rangos (filtrando ruido) y se acelera en tendencias (reduciendo el lag). Para un scalper, es un "interruptor" visual inmediato para cambiar de mentalidad "rango" a "tendencia".

**Acción:** **Mejorar (Prioridad P2).**

**¿Merece la pena mejorarlo?** **Sí.** El indicador funciona perfectamente (7/10). Las mejoras propuestas (`effort: Medio`), especialmente el coloreado de línea adaptativo, lo convertirían en un filtro de régimen 9/10 visualmente instantáneo.
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTM2MTIyNjU5M119
-->