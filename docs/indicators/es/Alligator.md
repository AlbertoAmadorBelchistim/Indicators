---
cs_file: Alligator.cs
name: Alligator
category: Trend
group: Trend
subgroup: Trend Filter
score_current: 6/10
version: Estable
recommended_action: Conservar
description: ¿Está el mercado 'durmiendo' (en rango, con las medias entrelazadas) o está 'despierto y comiendo' (en tendencia)?
gemini_summary: "Filtro de régimen clásico. Implementación correcta (SMMA + Shifts) pero con lag masivo por diseño."
comparison_group: "Trend Systems"
competitor_notes: "Sistema clásico. Útil para identificar rangos."
reusable_code: null
file_state: Estable
score_potential: 6/10
effort: N/A
action_priority: P3
analysis_date: 2025-11-17
official_code_date: 23/04/2025
---

## 🟦 Alligator (6/10)

**Nombre del archivo:** [`Alligator.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Alligator.cs)  
**Nombre del indicador:** Alligator  
**Web oficial:** [ATAS - Alligator](https://help.atas.net/support/solutions/articles/72000602579)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  
>**La Pregunta Clave:** ¿Está el mercado 'durmiendo' (en rango, con las medias entrelazadas) o está 'despierto y comiendo' (en tendencia, con las medias abiertas)?

![Alligator](../../img/Alligator.png)

----------

### ⚙️ Parámetros configurables

-   **JawPeriod**: Periodo de la mandíbula (por defecto: `13`)
    
-   **JawShift**: Desplazamiento de la mandíbula (por defecto: `8`)
    
-   **TeethPeriod**: Periodo de los dientes (por defecto: `8`)
    
-   **TeethShift**: Desplazamiento de los dientes (por defecto: `5`)
    
-   **LipsPeriod**: Periodo de los labios (por defecto: `5`)
    
-   **LipsShift**: Desplazamiento de los labios (por defecto: `3`)
    

----------

### 🧭 Clasificación

📂 Trend — Indicadores de filtro de régimen (Tendencia vs. Rango)

----------

### 🧠 Uso más frecuente

-   Identificar tendencias y zonas de consolidación.
    
-   Visualizar cruces de medias con desplazamiento temporal (shifts) para detectar entradas.
    
-   Operar con lógica de "boca del caimán" abierta (tendencia) o cerrada (rango).
    
-   Filtrar operaciones en función de la fase del mercado (expansión o compresión).
    

----------

### 📊 Nivel de relevancia

🔟 **6 / 10**

✅ Buen indicador visual de fase del mercado.

✅ Fácil de interpretar para estrategias tendenciales.

✅ Fiel a la implementación canónica de Bill Williams (usa SMMA y Median Price).

⛔ LAG MASIVO: Es un indicador extremadamente lento por diseño (usa SMMA + Shifts).

⛔ Redundante y obsoleto si ya se utiliza el AMA (Kaufman).

----------

### 🎯 Estrategias de scalping donde se aplica

-   **Filtro de Contexto (No Operar):** Si las líneas están entrelazadas ("Caimán durmiendo"), se prohíbe operar breakouts o tendencias.
    
-   **Filtro de Contexto (Operar):** Si las líneas están abiertas y ordenadas ("Caimán comiendo"), solo se permiten operaciones a favor de esa tendencia (ej. pullbacks a la línea de los "labios").
    
-   _Nota: Es demasiado lento para usarse como señal de entrada primaria._
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

-   **JawPeriod**: `13`, **JawShift**: `8`
    
-   **TeethPeriod**: `8`, **TeethShift**: `5`
    
-   **LipsPeriod**: `5`, **LipsShift**: `3`
    
-   _Nota: Se deben usar los valores por defecto. Son los canónicos del sistema de Bill Williams. Cambiarlos rompe la lógica del indicador._
    

----------

### 🧪 Notas de desarrollo

-   El indicador usa medias móviles suavizadas (**SMMA**), que son inherentemente lentas y con mucho lag.
    
-   Utiliza el **Precio Medio** (`(GetCandle(bar).Low + GetCandle(bar).High) / 2`) como fuente de datos, lo cual es correcto según el sistema de Bill Williams.
    
-   Aplica **desplazamientos (`Shifts`)** a las medias, lo que añade _aún más lag_ al cálculo.
    
-   Dibuja 3 líneas: Jaw (azul, lenta), Teeth (roja, media), Lips (verde, rápida).
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   El indicador funciona como fue diseñado. El "problema" es el diseño en sí mismo: es conceptualmente muy lento para el scalping.
    

----------

### 🛠️ Propuestas de mejora

-   Permitir seleccionar el tipo de media móvil (ej. `EMA` en lugar de `SMMA`) para hacerlo más rápido.
    
-   Añadir alertas por cruce entre líneas.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (Análisis de Fase 1)

Este indicador es un "clásico" filtro de régimen (Tendencia vs. Rango). Es, por diseño, un indicador **extremadamente lento**. Utiliza:
1.  **SMMA:** La media móvil con más lag.
2.  **Shifts (Desplazamientos):** Añade aún más lag.

Para un scalper, esto es un problema. Las señales de "boca abierta" (tendencia) llegan muy tarde, a menudo cuando el movimiento principal ya ha ocurrido.

Sin embargo, es una herramienta visual decente para confirmar una sola cosa:
* **Líneas entrelazadas ("Caimán durmiendo"):** Es una señal clara de "NO OPERES RUPTURAS, ESTAMOS EN RANGO".

### 📈 Veredicto: ¿Es útil para Scalping?

**No como señal de entrada, pero sí como filtro de contexto (6/10).**

Es demasiado lento para cualquier decisión de entrada. Sin embargo, un scalper puede usarlo como un "interruptor" de fondo para evitar operar en mercados laterales ("chop"). Su lentitud lo hace seguro para este propósito (es poco probable que te dé un falso "en rango").

**Acción:** **Conservar (como Filtro de Contexto Lento).**

**¿Merece la pena mejorarlo?** No realmente. Sus "fallos" (el lag) son sus "características". Cambiar el tipo de MA (a EMA) lo convertiría en un indicador diferente, pero no necesariamente mejor.
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTM1NTYzNTc3MCwxMTM4NDY5ODhdfQ==
-->