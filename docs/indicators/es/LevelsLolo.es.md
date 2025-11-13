## 🟦 LevelsLolo (Levels Text)

- **Nombre del archivo:** [LevelsLolo.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/LevelsLolo.cs)
- **Nombre del indicador:** LevelsLolo
- **Web oficial:** — (indicador propio; documentación en este repositorio)
- **Compatibilidad:** ATAS versión estable y superiores.
- **Versión actual:** 1.1.0 (30/10/2025)
- **Agradecimientos:** Inspirado en la idea original de **Alejandro Uriza — LevelsPro**, que introdujo el concepto de visualización estructurada de niveles SpotGamma.

![Overlay de niveles en el gráfico](../img/LevelsLolo1.png)

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

![Configuración del indicador LevelsLolo](../img/LevelsLoloConfig.png)

---

### 🧭 Clasificación
📂 **Levels** — Indicadores de visualización de niveles externos (texto / CSV) con jerarquías de tipo y magnitud.

---

### 🧠 Uso más frecuente
- Cargar niveles **SpotGamma** (CO, LG, PW, CW, VT, ZG).  
- Interpretar magnitudes (menor = más importante → LG01 > LG05 > LG15).  
- Detectar zonas de **absorción institucional**, **muros de opciones** y **triggers de volatilidad**.  
- Visualizar imanes de precio y puntos de interés (POI) relevantes para el día.

---

### 📊 Nivel de relevancia
🔟 **9 / 10**  
✅ Visualización directa de niveles clave de sesión  
✅ Ajuste fino de grosor, color y jerarquía por rango  
⛔ Necesita mantener actualizada la cadena de texto SpotGamma

---

### 🎯 Estrategias de scalping donde se aplica
- **Rebote controlado:** test en **LG01 / LG05** con rechazo rápido → entrada *fade*.  
- **Ruptura con confirmación:** cruce y retesteo de **VT** → continuación.  
- **Fade de “muros”:** proximidad a **PW/CW** con agotamiento → contramovimiento.  
- **Imanes de precio:** **CO** = objetivos de salida parcial.

---

### ⚙️ Parametrización óptima (1 min, ES Mini)
- **Font size:** 10  
- **Offset X/Y:** 6 / 6  
- **Right-aligned:** ✅  
- **Last bar only:** ✅  
- **ThickMaxRank / MediumMaxRank:** 3 / 10  
- **Enable 0DTE halo:** ✅ (Alpha ≈ 120, ExtraWidth 2)  
- **Only visible price range:** ✅  
- **Pens:** colores por defecto SpotGamma  
- **Opacity tiers:** 255 / 210 / 160  

✅ Muestra líneas más gruesas y opacas para **ranks bajos** (1–3).  
✅ Destaca *0DTE* en rojo brillante y añade acento punteado si es **LG/PW/CW**.  
⛔ No reemplaza el análisis contextual de volumen; es un **overlay informativo**.

---

### 🧪 Notas de desarrollo
- **Parseo in-place:** reconocimiento de tokens `CO`, `LG`, `VT`, `CW`, `PW`, `ZG` con *rank* y sufijo `0DTE`.  
- **Jerarquía de categorías:** `VT > LG > PW/CW > CO > ZG > Other`.  
- **Ancho y alpha:** determinados por el *rank* efectivo (`null / 0 → grueso`).  
- **Lógica de render:**  
  - Línea halo → principal → acento (punteado 0DTE LG/PW/CW).  
  - Texto alineado según `RightAligned` y desplazado por `Offset X/Y`.  
- **Font y re-cálculo:** cambio dinámico de tamaño de fuente dispara `RecalculateValues()`.  
- **Diseño modular:** estructura de clases `LabelToken`, `MergedLevel`, `Category`.

---

### 🆕 Cambios relevantes respecto a versiones anteriores
#### Versión 1.1 (30/10/2025)
- **Corrección del botón "Clear text now"**: ahora además de limpiar el gráfico limpia correctamente el campo de texto de niveles de la UI.

#### Versión 1.0 (25/10/2025)
- **Adaptación del estilo a la importancia del nivel**: el **grosor** y la **opacidad (alpha)** se calculan automáticamente según el *rank* —los niveles más importantes (ranks bajos) se dibujan más gruesos y opacos.  
- **Jerarquía por tipo y número**: cuando varios niveles coinciden en el **mismo precio**, el indicador aplica el estilo correspondiente al **nivel más importante**, evaluado primero por *rank* (menor = más fuerte) y luego por **tipo** (prioridad: VT > LG > PW/CW > CO > ZG > Other).  
- **Nuevas etiquetas detectables**: ahora reconoce sufijos y variantes como `0DTE`, `Zero Gamma`, `LargeGamma`, `PutWall`, `CallWall`, entre otros, ampliando la compatibilidad con distintos formatos de texto SpotGamma.

![Visualización del Call Wall](../img/LevelsLolo2.png)

---

### 🛠️ Propuestas de posibles mejoras
- Importación automática de archivo SpotGamma o portapapeles.   

---
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTg0OTg3MTU4M119
-->