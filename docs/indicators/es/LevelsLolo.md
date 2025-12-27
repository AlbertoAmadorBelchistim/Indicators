---

# 1. IDENTIFICACIÓN  
cs_file: LevelsLolo.cs  
name: LevelsLolo  
version: Custom v1.0  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Exogenous Structure  
comparison_group: "Exogenous Structure"  

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
gemini_summary: "CORE por impacto sistémico: convierte un input externo (niveles SpotGamma / texto) en un mapa jerárquico, legible y de bajo coste cognitivo, optimizado para planificar y ejecutar scalping sobre niveles de alta relevancia."  
competitor_notes: "Este torneo queda como arena unipersonal tras la refactorización taxonómica: LevelsLolo es el único indicador de Market Structure capaz de representar niveles exógenos (no derivados del precio) con jerarquía visual por tipo/rank y soporte específico para 0DTE. No compite contra pivots o grids: resuelve otra pregunta funcional (mapa externo institucional)."  
reusable_code: "Parser robusto (normalización de caracteres invisibles + tokenización por ',' y '&' + mapping semántico CO/LG/VT/CW/PW/ZG + reglas de prioridad por rank y categoría) y estructura de fusión por precio (Dictionary<decimal, MergedLevel>)."  

# 6. METADATOS  
analysis_date: 2025-12-27  
official_code_date: Unknown  
user_modification_date: 2025-10-30  

---

## 🛰️ LevelsLolo (9/10)  

**Nombre del archivo:** [LevelsLolo.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/LevelsLolo.cs)  
**Nombre del indicador:** LevelsLolo  
**Web oficial:** N/A (Custom)  
**Compatibilidad:** ATAS Stable/Latest  
**Última revisión del código oficial:** Unknown  
**Última revisión del código modificado:** 2025-10-30  
**Agradecimientos:** Inspirado en la idea original de **Alejandro Uriza — LevelsPro** (visualización estructurada de niveles SpotGamma).  

> **La Pregunta Clave:** ¿Dónde están los niveles externos clave (jerarquizados) y cómo los proyecto en el chart sin ruido para planificar entradas/salidas?  

![Overlay de niveles en el gráfico](../../img/LevelsLolo1.png)  


---

### ⚙️ Parámetros configurables  

#### 🖋️ Text (texto y alineación)  
- **Font size**: tamaño de fuente (6–48 px).  
- **Right-aligned text**: alinea el texto al borde derecho (si se desactiva, lo alinea a la izquierda).  
- **Last bar only**: extiende la línea hasta la **última barra visible** (si se desactiva, hasta el borde derecho del panel).  
- **Offset X / Offset Y**: desplazamiento del texto en píxeles respecto a la línea.  

#### 📏 Width tiers (grosor por jerarquía)  
- **Thick up to rank (ThickMaxRank)**: rango máximo considerado “top-tier” (≤ ThickMaxRank ⇒ grueso).  
- **Medium up to rank (MediumMaxRank)**: rango máximo considerado “tier medio” (≤ MediumMaxRank ⇒ medio).  
- **Width (thick/medium/thin)**: grosor en píxeles para cada tier.  

#### 🌫️ Alpha tiers (transparencia por jerarquía)  
- **Alpha (thick/medium/thin)**: opacidad (0–255) aplicada al color base según tier.  
- Nota: **ZG (Zero Gamma)** fuerza siempre estilo informativo (fino + alpha “thin”).  

#### 🎨 Pens (paleta semántica por tipo)  
- **Combo (CO)**: color/grosor base para combos.  
- **Large Gamma (LG)**: color/grosor base para grandes strikes gamma.  
- **Volatility Trigger (VT)**: color/grosor base para trigger de régimen.  
- **Call Wall (CW)**: color/grosor base para muro de calls.  
- **Put Wall (PW)**: color/grosor base para muro de puts.  
- **Zero Gamma (ZG)**: color/grosor base para contexto (informativo).  
- **Other/Unknown**: fallback para etiquetas no reconocidas.  

#### ✨ Accents (0DTE)  
- **Enable 0DTE halo**: dibuja un halo por debajo de la línea principal en niveles marcados como *0DTE*.  
- **0DTE halo alpha (0-255)**: opacidad del halo.  
- **0DTE halo extra width (px)**: grosor adicional del halo sobre el ancho del tier.  

#### 💾 Data (input)  
- **Raw text**: entrada textual de niveles. Formato: etiquetas y precios separados por comas, y múltiples etiquetas para un mismo precio separadas por `&`.  
  Ejemplo: `$SP: CO44, 7073, LG07, 7048, CO05 & LG14, 6898, VT 0DTE, 6743, LG1 0DTE, 6720`  
- **Clear text now**: borra el input y limpia el estado interno de niveles.  

#### 👁️ Visibility (filtros de dibujo)  
- **Only visible price range**: no dibuja niveles fuera del rango visible del eje Y (reduce ruido y coste de render).  

![Configuración del indicador LevelsLolo](../../img/LevelsLoloConfig.png)  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Exogenous Structure  
**Comparison Group:** "Exogenous Structure"  


---

### 🧠 Uso más frecuente  
- Cargar niveles externos (SpotGamma / roadmap) como **mapa base** previo a la sesión.  
- Priorizar niveles por jerarquía (LG, VT, PW/CW, CO, ZG) para reducir indecisión en M1.  
- Estandarizar la rutina: **Mapa → Zona → Trigger de Order Flow → Gestión por escalones**.  
- Evitar operar en “no man’s land” (zonas sin referencias exógenas relevantes).  


---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  

✅ Convierte input exógeno en estructura operable (impacto directo en planificación y gestión).  
✅ Jerarquía visual consistente por tipo/rank + soporte explícito para 0DTE (scalping).  
⛔ Dependencia del input: si los niveles están mal copiados/actualizados, el mapa induce sesgo.  


---

### 🎯 Estrategias de scalping donde se aplica  
- **Mapa + Trigger (Order Flow)**: solo ejecutar cuando el Order Flow valide reacción en un nivel exógeno top-tier (LG/PW/CW/VT).  
- **Gestión por escalones**: TP/SL por niveles (nivel siguiente = objetivo; nivel previo = invalidación).  
- **Filtro de setups**: priorizar operaciones cerca de niveles jerárquicos y evitar entradas sin “ancla estructural”.  
- **Plan de sesión**: escenario A/B/C por comportamiento del precio en VT/PW/CW y magnetismo CO.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| Font size | 11–13 | Lectura rápida sin saturar el chart. |  
| Right-aligned text | ✅ | Minimiza interferencia con el cuerpo de precio. |  
| Last bar only | ✅ | Reduce clutter en rangos laterales y simplifica lectura intradía. |  
| ThickMaxRank | 3 | Enfatiza solo niveles “top” (convención: menor número = mayor importancia). |  
| MediumMaxRank | 10 | Mantiene estructura secundaria sin competir con el precio. |  
| ThickWidth / MediumWidth / ThinWidth | 3 / 2 / 1 | Jerarquía visual clara y estable. |  
| ThickAlpha / MediumAlpha / ThinAlpha | 255 / 210 / 160 | Mantiene contraste sin tapar el precio. |  
| Only visible price range | ✅ | Reduce coste de render y evita sesgo por niveles fuera de contexto. |  
| Enable 0DTE halo | ✅ | Señalización inmediata de niveles con peso intradía (0DTE). |  
| HaloAlpha / HaloExtraWidth | 120 / 2 | Destaca 0DTE sin convertirlo en “neón” dominante. |  


---

### ✨ Mejoras introducidas (Oficial/Base)  
- N/A (Custom)  


---

### ✨ Mejoras añadidas (Custom)  
- Parser tolerante a formatos reales: normaliza caracteres invisibles, acepta prefijo `$SP:` y separadores `,` y `&`.  
- Clasificación semántica por tipo (CO/LG/VT/CW/PW/ZG) con paleta dedicada y fallback “Other”.  
- Priorización por importancia: grosor y alpha por tiers (rank bajo = más fuerte) y estilo informativo para ZG.  
- Soporte visual específico para 0DTE: halo subyacente y acento adicional para 0DTE high-priority (LG/PW/CW top-tier).  

![Visualización del Call Wall](../../img/LevelsLolo2.png)  


---

### 🧪 Notas de desarrollo  
- Arquitectura centrada en **render**: `OnCalculate` vacío; el estado vive en `_levelsByPrice` y se actualiza al cambiar `RawText`.  
- Modelo de datos: `Dictionary<decimal, MergedLevel>` fusiona múltiples etiquetas en un mismo precio y decide un “Winner” por reglas deterministas.  
- Parsing:  
  - Sanitiza caracteres invisibles (`UnicodeCategory.Format`) para evitar fallos por copy/paste.  
  - Tokeniza por comas y permite múltiples etiquetas antes de un precio mediante `&`.  
  - Regla de ganador: **rank efectivo menor gana**; empate rompe por prioridad de categoría (VT > LG > PW/CW > CO > ZG > Other).  
- Render:  
  - Recorta por visibilidad (`OnlyVisiblePriceRange`) para reducir draw calls.  
  - Construye `PenSettings` efectivos por nivel combinando color base + alpha tier + estilo (dash si 0DTE).  
- Rendimiento: correcto para uso normal (docenas de niveles), pero con margen de optimización en `OnRender` (ver mejoras).  


---

### ❗ Incoherencias o aspectos mejorables detectados  
- **Código muerto / inconsistencia de intención**: existe `_pen0DTEHalo` (cian) con comentario de uso, pero el render implementa un halo rojo nuevo y `_pen0DTEHalo` no se utiliza. :contentReference[oaicite:0]{index=0}  
- **Allocations en render**: en cada nivel y frame se instancian nuevos `PenSettings` (y `halo/accent`). En sesiones largas y repaints frecuentes, esto incrementa presión de GC. :contentReference[oaicite:1]{index=1}  
- **Normalización de etiquetas**: `FormatLabel` no hace “zero-pad” (LG01 vs LG1). Si tu convención visual depende de alineación o consistencia, conviene normalizar. :contentReference[oaicite:2]{index=2}  
- **Validación del input**: si hay tokens que no encajan con el patrón, se silencian (no hay feedback). Eso puede ocultar errores de copy/paste. :contentReference[oaicite:3]{index=3}  


---

### 🛠️ Propuestas de mejora  
- Añadir **diagnóstico de parsing**: contador de tokens válidos/ignorados, lista de fragmentos no parseados y duplicados por precio.  
- Cachear `PenSettings` efectivos por (Category, tier, is0DTE) y reutilizarlos para eliminar allocations en `OnRender`.  
- Unificar la implementación de halo 0DTE: o se usa `_pen0DTEHalo` (parametrizable) o se elimina para evitar deuda técnica.  
- Opción “Show labels only for top tiers” (por rank) para reducir aún más el clutter.  
- Exportar snapshot (texto normalizado) a log para auditoría post-trade y reproducibilidad del mapa.  


---

### 💎 Valor Reutilizable (Código Donante)  
- `NormalizeInvisible` para limpiar copy/paste (caracteres invisibles) en inputs de usuario.  
- `ParseRawText` con separación por `,` y `&`, soporte de prefijo y fusión por precio (patrón reutilizable para otros “maps”).  
- Regla de priorización determinista (rank + category priority) reutilizable en overlays jerárquicos.  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
LevelsLolo es un CORE por razón sistémica: en scalping del S&P 500, el “mapa” determina qué setups son operables y reduce el coste cognitivo. Este indicador estandariza el mapa exógeno con jerarquía visual y soporte 0DTE, habilitando un flujo disciplinado (nivel → trigger → gestión). La principal deuda técnica está en micro-optimización de render y en feedback del parser, no en la lógica estructural.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí**  

Aporta el mapa exógeno más accionable del sistema (estructura previa), con jerarquía visual consistente y bajo ruido, integrándose de forma natural como capa “Niveles / Mapa” antes del trigger de Order Flow.  

**Acción:** **Conservar (Core)**  



