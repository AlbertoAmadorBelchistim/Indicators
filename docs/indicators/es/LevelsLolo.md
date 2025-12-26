---

# 1. IDENTIFICACIÓN  
cs_file: LevelsLolo.cs  
name: LevelsLolo  
version: Custom v1.0  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Structural Levels  
comparison_group: "Structural Levels"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 9/10  
score_potential: 10/10  
file_state: Estable  
effort: Bajo  
action_priority: Baja  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Dónde están los niveles externos clave (jerarquizados) y cómo los proyecto en el chart sin ruido para planificar entradas/salidas?  
gemini_summary: "Ganador del torneo: integra niveles externos (texto) con jerarquía visual, reduce coste cognitivo y convierte un input discrecional (lista de niveles) en un mapa operativo estable."  
competitor_notes: "Frente a Pivots/Camarilla/Murrey/Round Numbers, LevelsLolo no compite por 'método de cálculo', sino por capacidad de integración: convierte fuentes externas (SpotGamma, levels text) en estructura accionable. Es el único que resuelve el problema real del sistema: mapa + priorización + limpieza visual."  
reusable_code: "Parser/normalización de líneas de niveles + lógica de priorización visual por tipo/rank (si está desacoplable del render)."  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: Unknown  
user_modification_date: 2025-10-30  

---

## 🧱 LevelsLolo (9/10)  

**Nombre del archivo:** [LevelsLolo.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/LevelsLolo.cs)
**Nombre del indicador:** LevelsLolo  
**Web oficial:** N/A (Custom)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** Unknown  
**Última revisión del código modificado:** 2025-10-30  
**Agradecimientos:** Inspirado en la idea original de **Alejandro Uriza — LevelsPro**, que introdujo el concepto de visualización estructurada de niveles SpotGamma.

> **La Pregunta Clave:** ¿Dónde están los niveles externos clave (jerarquizados) y cómo los proyecto en el chart sin ruido para planificar entradas/salidas?  

![Overlay de niveles en el gráfico](../../img/LevelsLolo1.png)


---

### ⚙️ Parámetros configurables  

#### 🖋️ Texto y alineación
- **Font size**: tamaño de fuente (6–48 px, por defecto 10).  
- **Right-aligned text**: alinear el texto a la derecha de la línea (si se desactiva se alinea a la izquierda).  
- **Last bar only**: extender desde la izquierda hasta la **última barra visible** en lugar del borde completo.  
- **Offset X / Offset Y**: desplazamiento del texto en píxeles en ejes X e Y respecto al punto base.

#### 📏 Grosor y transparencia
- **Thick / Medium / Thin width**: anchura de línea para cada categoría de grosor.  
- **Thick / Medium / Thin alpha**: opacidad por nivel de grosor (0–255).  
- **Thick / Medium max rank**: nivel de *rank* máximo para entrar en cada categoría  
  (`≤ ThickMaxRank → grueso`, `≤ MediumMaxRank → medio`, resto → fino).

#### 🎨 Colores y estilos
- **Pens (CO, LG, VT, CW, PW, ZG, Other)**: color y grosor base por tipo.
- Se han predefinido colores cálidos para diferenciarlos de otros indicadores:  
  - `PW` → soporte (verde) `CW` → resistencia (rojo)  
  - `VT` → gatillo de volatilidad (amarillo) `LG` → absorción institucional (naranja)  
  - `CO` → imán de precio (ámbar) `ZG` → contexto de régimen (gris neutro)  
- **Enable 0DTE halo**: trazo rojo brillante bajo la línea principal para niveles *0DTE*.  
  - **0DTE halo alpha / extra width**: opacidad y grosor adicional del halo.

#### 💾 Datos y visibilidad
- **Raw text**: entrada textual de niveles. 
- Los niveles de precio se separan por comas, y los niveles múltiples en el mismo precio por `&`. 
  Ejemplo:  
  `$SP: CO44, 7073, LG07, 7048, CO05 & LG14, 6898, VT 0DTE, 6743, LG1 0DTE, 6720`
- **Clear text now**: limpia la entrada manualmente.  
- **Only visible price range**: no dibujar niveles fuera del rango visible.

![Configuración del indicador LevelsLolo](../../img/LevelsLoloConfig.png)



---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Structural Levels  
**Comparison Group:** "Structural Levels"  


---

### 🧠 Uso más frecuente  
* Cargar niveles externos (SpotGamma / roadmap) como **mapa base** previo a la sesión.  
* Priorizar niveles por jerarquía (LG, CO, PW/CW, VT) para reducir indecisión en M1.  
* Planificar: “zona operable” → “nivel candidato” → “confirmación por Order Flow”.  


---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  

✅ Integra niveles externos y los convierte en estructura operable (bajo coste cognitivo).  
✅ Jerarquía visual: facilita decisiones rápidas sin saturar el gráfico.  
⛔ Depende de la calidad del input (si el feed/texto es erróneo, el mapa también).  


---

### 🎯 Estrategias de scalping donde se aplica  
* **Mapa + trigger**: entrar solo cuando el Order Flow valide reacción en un nivel jerarquizado (LG/CO).  
* **Gestión por niveles**: TP/SL por escalones (nivel siguiente como objetivo, nivel previo como invalidación).  
* **Filtro de trades**: evitar operaciones en “no man’s land” lejos de niveles priorizados.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| FontSize | 11–13 | Legibilidad rápida sin llenar pantalla. |  
| Grosor líneas (niveles top) | Medio/Alto | Los niveles “rank 1” deben saltar a la vista. |  
| Grosor líneas (niveles secundarios) | Bajo | Mantener mapa limpio, evitar sesgo visual por exceso de niveles. |  
| Etiquetas | Solo en niveles top | Reduce clutter; el eje + colores ya informan. |  


---

### ✨ Mejoras introducidas (Oficial/Base)  
* N/A (Custom)  


---

### ✨ Mejoras añadidas (Custom)  
* Integración de niveles externos en formato texto con jerarquía visual por tipo/rank.  
* Enfoque “mapa operativo” orientado a scalping: legibilidad y priorización por importancia. 

![Visualización del Call Wall](../../img/LevelsLolo2.png)


---

### 🧪 Notas de desarrollo  
* Indicador orientado a **render / visualización**, no a cálculo histórico pesado.  
* El rendimiento depende principalmente de: frecuencia de repintado, número de niveles activos y complejidad de etiquetas.  
* Recomendación: minimizar allocations en render y cachear recursos (fonts/pens) para estabilidad.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
* Si el parser no valida bien formatos atípicos, puede introducir niveles mal interpretados (riesgo de “mapa falso”).  
* Sería deseable un “lint” del input: avisos de líneas no parseadas / duplicadas.  


---

### 🛠️ Propuestas de mejora  
* Añadir validación/diagnóstico del input: conteo de líneas válidas, duplicados y conflictos.  
* Opción para “solo niveles relevantes” según régimen (por ejemplo: intradía vs swing).  
* Exportar snapshot de niveles (log) para auditoría post-trade.  


---

### 💎 Valor Reutilizable (Código Donante)  
* Parser de niveles + normalización de texto (si está desacoplado).  
* Sistema de priorización visual por tipo/rank reutilizable para otros mapas.  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
LevelsLolo es el único competidor que resuelve un problema estructural real del sistema: transformar niveles externos jerarquizados en un mapa operativo estable y legible. En scalping, reducir el coste cognitivo y estandarizar el “mapa” tiene más impacto que añadir otro método de cálculo de niveles. Por eso es CORE.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí**  

Aporta el mapa de niveles más accionable del grupo, con jerarquía visual y bajo ruido, y se integra perfectamente como capa “Niveles / Mapa” previa al trigger de Order Flow.  

**Acción:** **Conservar (Core)**  


