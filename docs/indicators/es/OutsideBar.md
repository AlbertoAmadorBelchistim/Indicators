## 🟦 Outside Bar (6/10)

**Nombre del archivo:** `OutsideBar.cs`  
**Nombre del indicador:** Outside Bar  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602280](https://help.atas.net/support/solutions/articles/72000602280)

---

### ⚙️ Parámetros configurables

- **IncludeEqual**: Considerar o no los valores iguales en High/Low al comparar velas (por defecto: false)

---

### 🧭 Clasificación
📂 PriceAction — Detección de patrones de expansión de rango (barras externas)

---

### 🧠 Uso más frecuente

- Detectar **barras que engloban completamente la anterior** (expansión de rango)  
- Confirmar **rupturas** o **trampas** si la barra exterior falla en continuar  
- Usar como base para estrategias de **reversión o continuación estructural**

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Identifica un patrón clásico de análisis técnico  
✅ Útil como disparador o filtro estructural  
⛔ No ofrece dirección ni confirmación adicional por sí solo

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por reversión** si la barra siguiente rechaza el extremo de una outside bar  
- **Rompimiento estructural** si hay continuación con volumen tras una barra exterior  
- **Estrategia de trampa**: esperar falsa ruptura seguida de vuelta dentro del rango previo

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **IncludeEqual**: `true`  
✅ Permite detectar la mayoría de patrones de expansión estructural  
✅ Compatible con otras herramientas de volumen o delta  
⛔ Puede marcar muchas barras en mercados muy estrechos si se activa `IncludeEqual`

---

### 🧪 Notas de desarrollo

- Compara la vela actual con la anterior:  
  - Si `IncludeEqual` = `false`: `High > High[-1]` y `Low < Low[-1]`  
  - Si `IncludeEqual` = `true`: `High ≥ High[-1]` y `Low ≤ Low[-1]`  
- Dibuja un punto azul en el máximo de la vela si se cumple la condición  
- Usa `VisualMode.Dots` con grosor 3 para destacar el punto  
- No requiere panel adicional ni genera líneas ni rellenos

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El punto se dibuja siempre en el `High`, incluso si la señal viene por ruptura del mínimo  
- No distingue entre barras alcistas o bajistas en color o ubicación  
- No permite configurar el estilo, color o tamaño del punto desde la UI  
- No hay validación explícita para barras sin datos (ej: `null` en histórico incompleto)  
- No ofrece opción para dibujar líneas verticales o rectángulos que enmarquen la barra completa

---

### 🛠️ Propuestas de mejora

- Añadir opción para diferenciar visualmente entre barras alcistas y bajistas  
- Permitir personalizar color, tamaño y estilo del punto  
- Añadir opción para dibujar un rectángulo que abarque la barra completa  
- Incluir alertas visuales/sonoras al detectar una outside bar  
- Añadir etiqueta flotante con `High`, `Low` o la palabra “Outside Bar”

