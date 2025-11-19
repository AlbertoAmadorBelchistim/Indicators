---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: Logo.cs
name: Background Picture
category: Visualization
score_current: 2/10
version: ATAS Official
recommended_action: Conservar
description: ¿Cómo puedo superponer una imagen o logo personalizado en el gráfico?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: Utilidad visual estable para branding o decoración; no es un indicador de trading. El código es seguro.
file_state: Estable
score_potential: 2/10
effort: N/A
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-17
official_code_date: 2025-04-23
user_modification_date: null
---

## 🟦 Background Picture (2/10)

**Nombre del archivo:** [`Logo.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Logo.cs)    
**Nombre del indicador:** Background Picture    
**Web oficial:** [ATAS — Background Picture](https://help.atas.net/support/solutions/articles/72000602528)    
**Compatibilidad:** ATAS versión estable y superiores.    
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Cómo puedo superponer una imagen o logo personalizado en el gráfico?

![BackgroundPicture](../../img/BackgroundPicture.png)

---

### ⚙️ Parámetros configurables

* **LogoLocation**: Posición de la imagen en el gráfico (Centro, Esquinas)
* **Scale**: Escalado del tamaño de la imagen (0–100%)
* **Transparency**: Transparencia de la imagen (0–100%)
* **HorizontalOffset / VerticalOffset**: Ajustes finos de posición en X/Y
* **AbovePrice**: Si se muestra sobre o bajo el gráfico de precios
* **FilePath**: Ruta local de la imagen a mostrar

---

### 🧭 Clasificación
📂 Visualization — Herramienta visual para superponer imágenes (logos, marcas, fondos)

---

### 🧠 Uso más frecuente

* Mostrar **logos de marca o empresa** sobre el gráfico
* Añadir elementos gráficos decorativos en análisis visuales
* Usar imágenes como **referencia visual** o branding en presentaciones o vídeos

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

✅ Útil para branding o presentaciones visuales
✅ Personalización estética sin afectar el análisis técnico
⛔ No tiene ningún impacto funcional en cálculos o señales

---

### 🎯 Estrategias de scalping donde se aplica

* No aplica directamente. Es un elemento visual sin función analítica.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **LogoLocation**: `BottomRight`
* **Scale**: `20`
* **Transparency**: `60`
* **AbovePrice**: `false`

---

### 🧪 Notas de desarrollo

* Permite cargar imágenes desde disco local (`.bmp`, `.jpg`, `.png`, etc.)
* Limita el tamaño de archivo a 1 MB (`Math.Pow(2, 20)`) para evitar bloqueos o errores
* Calcula el rectángulo de renderizado dinámicamente en base a posición, escala y tamaño
* Usa `SetOpacity` para aplicar transparencia mediante una `ColorMatrix`
* Usa `EnableCustomDrawing` y `OnRender` para insertar imagen en `RenderContext`

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Esto no es un indicador de análisis técnico, sino una utilidad de visualización para branding o decoración. El código en `Logo.cs` es estable y seguro. Utiliza `EnableCustomDrawing` y `OnRender` para dibujar la imagen.

Implementa dos buenas prácticas de seguridad:
1.  Comprueba si el archivo existe con `File.Exists(_filePath)`.
2.  Limita el tamaño del archivo a 1MB (`new FileInfo(_filePath).Length <= Math.Pow(2, 20)`) para evitar cargar imágenes excesivamente grandes que podrían consumir mucha memoria.

El único "code smell" es un "throttle" manual de 200ms en el *setter* de la propiedad `Transparency`, que es una forma inusual de manejar la actualización, pero no rompe la funcionalidad. Es una herramienta estable con un propósito puramente no analítico.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Es una herramienta de decoración, no de análisis.

**Acción:** **Conservar (Utilidad de visualización).**

