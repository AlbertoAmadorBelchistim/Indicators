## 🟦 Positive/Negative Volume Index (8 / 10)  
**Nombre del archivo:** `VolumeIndex.cs`  
**Nombre del indicador:** Positive/Negative Volume Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602304](https://help.atas.net/support/solutions/articles/72000602304)

---

### ⚙️ Parámetros configurables  
- **CalcMode**: Modo de cálculo → `Positive` (por defecto) o `Negative`  
- **StartPriceFilter**: Activar valor personalizado de precio inicial (por defecto: desactivado)  
- **StartPrice**: Valor manual del precio inicial (solo si se activa el filtro)

---

### 🧭 Clasificación  
📂 Volume — Índice de volumen acumulativo condicionado por cambios de volumen

---

### 🧠 Uso más frecuente  
- Evaluar si las subidas de precio están acompañadas de **incrementos o decrementos de volumen**  
- Medir acumulación/distribución bajo lógica de volumen creciente (PVI) o decreciente (NVI)  
- Confirmar tendencias mediante **acumulación condicionada** por volumen relativo

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Útil como indicador de **confirmación de tendencia a medio/largo plazo**  
✅ Basado en teoría clásica con buena base empírica  
⛔ No emite señales inmediatas, sino **cambios graduales de fondo**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de dirección de fondo**: Activar setups solo si el PVI o NVI es creciente  
- **Filtro direccional**: Evitar operar contra la dirección del índice acumulado  
- **Contextualización de volumen**: Validar que un rompimiento está respaldado por tendencia en PVI o NVI

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **CalcMode**: `Positive` para confirmar subidas con volumen creciente  
- **StartPriceFilter**: `false`  
- **StartPrice**: (se ignora si el filtro está desactivado)

✅ Complementa análisis de volumen clásico con lógica acumulativa  
✅ Ayuda a **evitar operar contra la participación institucional**  
⛔ Poco reactivo: requiere muchas barras para producir señales visibles

---

### 🧪 Notas de desarrollo  
- Calcula el índice acumulado usando:  
  `VI[bar] = VI[bar - 1] + (Close[bar] - Close[bar - 1]) × VI[bar - 1] / Close[bar - 1]`  
- Solo se actualiza si el volumen actual es **mayor (PVI)** o **menor (NVI)** que el anterior  
- El valor inicial se toma desde el precio de la serie o desde un **filtro manual** si se activa

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No valida que el precio anterior (`Close[bar - 1]`) sea diferente de cero → **riesgo de división por cero**  
- La lógica de filtro inicial puede ser confusa para usuarios novatos (`StartPriceFilter.Enabled`)  
- No incluye alertas ni umbrales visuales para cambios relevantes en la curva

---

### 🛠️ Propuestas de mejora  
- Añadir protección ante división por cero en `prevValue`  
- Incluir **alertas de cambio de pendiente** o aceleración  
- Permitir personalización visual (colores, grosor, zona de relleno) y líneas de referencia
