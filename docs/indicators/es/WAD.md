## 🟦 Williams Accumulation / Distribution (WAD) (7 / 10)  
**Nombre del archivo:** `WAD.cs`  
**Nombre del indicador:** Accumulation / Distribution - Williams  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602568](https://help.atas.net/support/solutions/articles/72000602568)

---

### ⚙️ Parámetros configurables  
Este indicador **no tiene parámetros configurables desde la UI**.

---

### 🧭 Clasificación  
📂 Volume — Indicador clásico de acumulación/distribución basado en el precio de cierre

---

### 🧠 Uso más frecuente  
- Medir la **presión de compra o venta acumulada** sin utilizar directamente el volumen  
- Confirmar **tendencias** mediante la dirección del acumulado  
- Detectar **divergencias entre precio y presión acumulativa** del mercado

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**  
✅ Muy útil como confirmador de dirección en sistemas simples  
✅ Fácil de interpretar y calcular  
⛔ No tiene en cuenta el volumen real, por lo que puede ser engañoso en movimientos sin participación

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de dirección**: Si el WAD sube mientras el precio sube, tendencia válida  
- **Divergencias**: Si el precio sube pero el WAD baja, posible agotamiento  
- **Filtro de entrada**: Evitar compras si el WAD cae mientras el precio sube (discrepancia)

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- No tiene parámetros configurables  
- Se recomienda combinar con indicadores de volumen o delta para validar

✅ Útil como **indicador de contexto acumulativo**  
✅ Ayuda a detectar **fallos de continuación en movimientos extendidos**  
⛔ No válido como única fuente de decisión sin validación externa

---

### 🧪 Notas de desarrollo  
- Si el cierre actual es mayor que el anterior, suma la diferencia con el mínimo  
- Si el cierre actual es menor, resta la diferencia con el máximo  
- Si el cierre es igual, mantiene el valor anterior  
- No utiliza volumen ni delta, solo precios de la vela

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No valida `GetCandle(bar - 1)` al inicio (aunque el cálculo lo evita si `bar == 0`)  
- No incluye volumen real en su lógica → el nombre puede llevar a error  
- No tiene etiquetas ni elementos visuales auxiliares para interpretación rápida

---

### 🛠️ Propuestas de mejora  
- Añadir opción de mostrar **divergencias visuales** frente al precio  
- Incluir posibilidad de aplicar una **media móvil** sobre el WAD  
- Integrar opción de mostrar **acumulado porcentual relativo**
