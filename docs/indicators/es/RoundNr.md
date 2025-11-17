## 🟦 Round Numbers (7/10)

**Nombre del archivo:** `RoundNr.cs`  
**Nombre del indicador:** Round Numbers  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602459](https://help.atas.net/support/solutions/articles/72000602459)

---

### ⚙️ Parámetros configurables

- **Step**: Número de ticks entre niveles de precios redondos (por defecto: 100)  
- **Pen**: Color y grosor de las líneas horizontales de los niveles

---

### 🧭 Clasificación
📂 Level — Indicador de niveles de precios redondos fijos en el gráfico

---

### 🧠 Uso más frecuente

- Visualizar **niveles de precios redondos** (ej. 4000, 4050, 4100…) como referencia estructural  
- Evaluar la reacción del precio en **zonas psicológicas** o de concentración de órdenes  
- Utilizar como **soporte/resistencia pasivos** para scalping técnico

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Niveles clave fáciles de visualizar y personalizar  
✅ Útil para complementar estructuras, order flow o volumen  
⛔ No se adapta dinámicamente al contexto de mercado

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada tras test de un número redondo** con confirmación de volumen  
- **Evitar entradas** justo en niveles clave si no hay absorción clara  
- **Proyección de stops o targets** usando múltiplos del nivel base

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Step**: `50` (equivale a niveles cada 12.5 puntos en ES)  
- **Pen.Color**: gris claro o rojo tenue  
- **Pen.Width**: `1`

✅ Niveles precisos cada 50 ticks ofrecen buena guía estructural  
✅ Proporciona contexto sin sobrecargar el gráfico  
⛔ Si se usa con `Step` muy bajo, puede saturar el entorno visual

---

### 🧪 Notas de desarrollo

- Calcula el primer valor redondo por debajo del mínimo del gráfico visible  
- Dibuja líneas horizontales desde `low` hasta `high` cada `Step × TickSize`  
- Mide la altura vertical entre niveles y la compara con el alto del texto para decidir si renderizar etiquetas  
- Si hay espacio, añade el valor numérico del nivel en el borde derecho del gráfico  
- Todo el dibujo se realiza en `OnRender`, sin lógica en `OnCalculate`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El método `GetFirstValue` no redondea correctamente si `low` ya es múltiplo exacto pero está justo por debajo del primer nivel visible  
- No hay validación visual si `Step × TickSize` es menor que el espacio vertical disponible — podría saturar el gráfico sin advertencia  
- El valor `"TextCheck"` se utiliza para medir altura de texto, pero no representa un número real → pequeña incoherencia semántica  
- No hay forma de ocultar las etiquetas numéricas si el usuario solo desea líneas  
- No se ofrecen opciones para líneas discontinuas o relleno entre zonas

---

### 🛠️ Propuestas de mejora

- Permitir activar/desactivar la visualización del texto numérico desde la UI  
- Añadir opción de estilo de línea (`sólida`, `discontinua`, `punteada`)  
- Incluir filtro para limitar la visualización a un rango definido (ej: ±X puntos desde el precio actual)  
- Mejorar lógica de `GetFirstValue` para evitar redondeo erróneo en niveles exactos  
- Reemplazar `"TextCheck"` por una muestra representativa (ej: `10000.00`) acorde al número de decimales del instrumento

