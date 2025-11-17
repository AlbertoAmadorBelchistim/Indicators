## 🟦 Murrey Math (8/10)

**Nombre del archivo:** `MurrayMath.cs`  
**Nombre del indicador:** Murrey Math  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602435](https://help.atas.net/support/solutions/articles/72000602435)

---

### ⚙️ Parámetros configurables

- **Days**: Número de sesiones hacia atrás para calcular los niveles (por defecto: 20)  
- **IgnoreWicks**: Si se ignoran las mechas y se usan solo cuerpo (Open/Close)  
- **FrameSize**: Base de tamaño del marco (4, 8, 16, ..., 512)  
- **FrameMultiplier**: Multiplicador del marco (1.0 / 1.5 / 2.0)

---

### 🧭 Clasificación
📂 Level — Indicador de niveles estructurales calculados según la teoría Murrey Math

---

### 🧠 Uso más frecuente

- Trazar **niveles horizontales** de soporte/resistencia según una estructura fija  
- Identificar **zonas de reversión o extensión** basadas en proporciones armónicas  
- Establecer rangos de operativa dentro de una jerarquía técnica

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Niveles simétricos, coherentes y reutilizables en múltiples marcos temporales  
✅ Excelente para estructura de rango o rupturas técnicas  
⛔ Requiere conocimiento específico de Murrey Math para interpretación óptima

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada en reversión** cerca de niveles 0/8, 2/8, 6/8 u 8/8  
- **Ruptura con dirección** cuando se supera 4/8 con impulso  
- **Confirmación estructural** si el precio rebota entre líneas armónicas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Days**: `20`  
- **IgnoreWicks**: `true`  
- **FrameSize**: `64`  
- **FrameMultiplier**: `1.5`

✅ Proporciona una estructura clara con niveles fijos  
✅ Buena lectura visual para trabajar entre zonas  
⛔ Necesita ajuste según el activo y marco temporal

---

### 🧪 Notas de desarrollo

- Utiliza `Highest` y `Lowest` para detectar el rango en el periodo definido  
- Calcula los niveles matemáticos a partir de una serie de transformaciones logarítmicas y exponenciales  
- Dibuja **13 líneas horizontales** desde `-3/8` hasta `+3/8` más allá del rango principal  
- Asigna **colores y grosores** distintos a líneas clave (módulo 4)  
- Adapta automáticamente el nivel si se produce un desajuste significativo entre `top` y `bottom`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- Si `Days` es 0, se usa todo el histórico pero no hay indicación visual de este comportamiento  
- No se permite modificar el color individual de cada nivel desde la UI  
- El cálculo usa muchos `Math.Log`, `Math.Exp` y condiciones anidadas poco documentadas  
- El valor de `Shift` y el sentido de `absTop` pueden causar confusión en activos negativos  
- No hay alertas, etiquetas ni valores mostrados directamente en los niveles

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar los valores numéricos en cada línea (etiquetas)  
- Permitir personalizar color y estilo de cada nivel desde la UI  
- Incluir tooltip explicativo sobre cada nivel (ej: "Soporte fuerte", "Zona de reversión")  
- Añadir alertas si el precio cruza ciertos niveles (ej: 4/8, 8/8 o -2/8)  
- Refactorizar el cálculo de niveles para mayor claridad y mantenibilidad

