## 🟦 Background Picture (2/10)

**Nombre del archivo:** `Logo.cs`  
**Nombre del indicador:** Background Picture  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602528](https://help.atas.net/support/solutions/articles/72000602528)

---

### ⚙️ Parámetros configurables

- **LogoLocation**: Posición de la imagen en el gráfico (Centro, Esquinas)  
- **Scale**: Escalado del tamaño de la imagen (0–100%)  
- **Transparency**: Transparencia de la imagen (0–100%)  
- **HorizontalOffset / VerticalOffset**: Ajustes finos de posición en X/Y  
- **AbovePrice**: Si se muestra sobre o bajo el gráfico de precios  
- **FilePath**: Ruta local de la imagen a mostrar

---

### 🧭 Clasificación
📂 Visualization — Herramienta visual para superponer imágenes (logos, marcas, fondos)

---

### 🧠 Uso más frecuente

- Mostrar **logos de marca o empresa** sobre el gráfico  
- Añadir elementos gráficos decorativos en análisis visuales  
- Usar imágenes como **referencia visual** o branding en presentaciones o vídeos

---

### 📊 Nivel de relevancia
🔟 **2 / 10**

✅ Útil para branding o presentaciones visuales  
✅ Personalización estética sin afectar el análisis técnico  
⛔ No tiene ningún impacto funcional en cálculos o señales

---

### 🎯 Estrategias de scalping donde se aplica

⛔ No aplica directamente. Es un elemento visual sin función analítica.  
✅ Puede usarse para etiquetar el gráfico (por ejemplo, con nombre de estrategia)

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **LogoLocation**: `BottomRight`  
- **Scale**: `20`  
- **Transparency**: `60`  
- **AbovePrice**: `false`  
- **Offset**: ajustar según resolución de pantalla

✅ Permite dejar visible un logo o recordatorio sin interferir con los niveles de precio

---

### 🧪 Notas de desarrollo

- Permite cargar imágenes desde disco local (`.bmp`, `.jpg`, `.png`, etc.)  
- Limita el tamaño de archivo a 1 MB para evitar bloqueos o errores  
- Calcula el rectángulo de renderizado dinámicamente en base a posición, escala y tamaño  
- Usa `SetOpacity` para aplicar transparencia mediante una `ColorMatrix`  
- Usa `EnableCustomDrawing` y `OnRender` para insertar imagen en `RenderContext`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El indicador intenta aplicar la transparencia **cada vez que se cambia el valor**, pero se limita artificialmente a una frecuencia mínima de 200 ms → esto puede causar que no se aplique al instante  
- No hay validación previa para formatos corruptos o incompatibles de imagen  
- La propiedad `DrawAbovePrice` se expone como `AbovePrice` pero puede generar confusión si no se actualiza correctamente  
- No se informa al usuario si la imagen no se carga por no encontrar el archivo o por tamaño excesivo (excepto una alerta textual)  
- El uso de `FileInfo.Length` en bytes no valida tipo MIME ni calidad real del archivo

---

### 🛠️ Propuestas de mejora

- Aplicar siempre la transparencia incluso si se cambia rápidamente  
- Añadir feedback visual en caso de error al cargar la imagen (formato inválido, corrupto, etc.)  
- Separar claramente la lógica de visualización y carga para facilitar depuración  
- Mostrar nombre o información básica de la imagen cargada como tooltip  
- Añadir opción de mantener proporción (aspect ratio) o recortar automáticamente

