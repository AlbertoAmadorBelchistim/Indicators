---

# 1. IDENTIFICACIÓN  
cs_file: Fractals.cs  
name: Fractals  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Swing-Derived Structure  
comparison_group: "Swing-Derived Structure"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 9/10  
score_potential: 9/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Dónde están los pivotes fractales (5 barras) y qué niveles horizontales de soporte/resistencia quedan activos hasta su mitigación?  
gemini_summary: "Fractales 5 barras + opción ShowLine para proyectar niveles horizontales hasta touch (LineTillTouch). Es un generador automático de niveles operables, muy eficiente y visualmente limpio: CORE por productividad y decisión."  
competitor_notes: "Gana como generador de niveles frente a SwingHighLow (que solo marca flechas). No compite directamente con Zigzag (estructura cuantificada por onda). CMS aporta sesgo, no niveles. GreatestSwing es proyección de rango, no pivote validado."  
reusable_code: "Uso robusto de HorizontalLinesTillTouch con actualización intra-bar y PenSettings reactivo."  

# 6. METADATOS  
analysis_date: 2025-12-28  
official_code_date: 2025-12-15  




---

## 🟦 Fractals (9/10)

**Nombre del archivo:** [`Fractals.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Fractals.cs)  
**Nombre del indicador:** Fractals  
**Web oficial:** [ATAS — Fractals](https://help.atas.net/support/solutions/articles/72000602388)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-12-15  

> **La Pregunta Clave:** ¿Dónde están los pivotes fractales (5 barras) y qué niveles horizontales de soporte/resistencia quedan activos hasta su mitigación?  

![Fractals](../../img/Fractals.png)



---

### ⚙️ Parámetros configurables

**[Line / Projection]**  
- **ShowLine**: Si está activo, dibuja líneas horizontales (`LineTillTouch`) desde el fractal hasta ser tocadas (mitigadas).  
- **HighPen / LowPen**: Estilo del trazo para líneas de fractal alto/bajo.  

**[Visualization]**  
- **Mode**: Mostrar `High`, `Low`, `All` o `None`.  



---

### 🧭 Clasificación
**Grupo:** Market Structure  
**Subgrupo:** Swing-Derived Structure  
**Comparison Group:** "Swing-Derived Structure"  



---

### 🧠 Uso más frecuente

* Generar niveles de S/R automáticos desde swings (fractales) sin dibujar manualmente.  
* Definir zonas de ruptura y zonas de reacción (mitigación).  
* Mantener gráfico limpio: puntos + líneas solo cuando aporta decisión.  



---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Niveles horizontales automáticos hasta mitigación (altísima productividad).  
✅ Lógica estándar 5 barras, estable y universal.  
⛔ No valida con volumen/delta (necesita confirmación externa).  



---

### 🎯 Estrategias de scalping donde se aplica

* **Ruptura de nivel fractal**: entrada al romper y confirmar (ideal con Order Flow).  
* **Reversión por mitigación**: reacción en el primer toque a nivel reciente (con señal).  
* **Mapa de niveles**: priorizar setups en niveles que aún no han sido mitigados.  



---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor recomendado | Impacto | Justificación operativa |
|---|---:|---|---|
| Mode | All | Cobertura | Mantiene ambos lados del mapa (alto/bajo). |
| ShowLine | true | Decisión | Convierte pivotes en niveles operables persistentes. |
| HighPen / LowPen | Líneas finas | Legibilidad | Evita saturación del gráfico en M1. |  



---

### 🧪 Notas de desarrollo

* Fractal estándar: la barra central `bar-2` debe ser mayor/menor que las 2 previas y 2 posteriores.  
* Dibuja puntos desplazados 3 ticks para visibilidad.  
* `ShowLine` gestiona alta/baja con `HorizontalLinesTillTouch` y actualiza correctamente en la misma barra (evita “líneas fantasma”).  
* `PenSettings` reactivo: cambios de estilo se propagan a líneas existentes.  



---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguna técnica crítica; el comportamiento de confirmación con lag es correcto por definición de fractal.  



---

### 🛠️ Propuestas de mejora

* Añadir opción de “máximo número de líneas activas” o caducidad por barras para controlar saturación en histórico largo.  
* Añadir etiquetas opcionales de prioridad (más reciente / no mitigado / etc.).  



---

### 💎 Valor Reutilizable (Código Donante)

* Patrón de niveles “till touch” con actualización intrabar y configuración de estilo reactiva.  



---

### ✍️ La opinión de ChatGPT sobre el Indicador

Es CORE porque transforma una señal clásica (fractal) en un **mapa de niveles accionables** con disciplina: una línea vive hasta que el mercado la mitiga. En scalping, esto reduce mucho el ruido y te obliga a operar alrededor de niveles objetivos. No pretende predecir; pretende organizar el espacio de decisión, y lo hace muy bien.  



---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí**  

Es uno de los generadores de niveles más útiles y productivos para M1.  

**Acción:** **Conservar (Core)**  
