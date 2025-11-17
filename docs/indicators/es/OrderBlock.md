## 🟦 Order Block (9/10)

**Nombre del archivo:** `OrderBlock.cs`  
**Nombre del indicador:** Order Block  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000641186-order-block](https://help.atas.net/support/solutions/articles/72000641186-order-block)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para calcular los swings relevantes (por defecto: 10)  
- **UsBody**: Ignorar mechas y calcular bloques usando solo el cuerpo de la vela  
- **Transparency**: Transparencia de las áreas dibujadas (0 a 10)  
- **BullishNumber / BearishNumber**: Cantidad máxima de bloques visibles por tipo  
- **BullishColor / BearishColor**: Color del área del bloque válido  
- **BullishBreakColor / BearishBreakColor**: Color del área si el bloque es roto  
- **ShowPocLevel**: Mostrar línea horizontal en el POC del bloque  
- **PocColor / PocWidth / PocStyle**: Configuración de estilo para la línea de POC

---

### 🧭 Clasificación
📂 Level — Detección y visualización de bloques de órdenes institucionales (Order Blocks)

---

### 🧠 Uso más frecuente

- Identificar **bloques de órdenes donde actuaron instituciones**  
- Confirmar zonas de interés con **estructura, POC y reacción**  
- Evaluar si un bloque fue **defendido o roto**

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Extraordinariamente útil en análisis institucional y Wyckoff  
✅ Compatible con estructuras de absorción, trampa, rechazo o continuación  
⛔ No incluye alertas ni valores visibles, requiere análisis visual completo

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión en bloque válido** tras test y rechazo  
- **Entrada por ruptura** si el bloque es invalidado con volumen/delta  
- **Confirmación de trampa** si se rompe un bloque y el precio no continúa

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`  
- **UsBody**: `true`  
- **Transparency**: `3`  
- **BullishNumber / BearishNumber**: `3`  
- **ShowPocLevel**: `true`

✅ Muestra bloques estructurales recientes  
✅ Útil para validar rupturas y trampas  
⛔ Requiere contexto: no todos los bloques son operables sin confluencia

---

### 🧪 Notas de desarrollo

- Detecta swings de máximos/mínimos para definir bloques con base estructural  
- Evalúa si los bloques son rotos o respetados, cambiando su coloración  
- Calcula máximos/mínimos dentro del rango del bloque usando cuerpo o mecha  
- Almacena POC de cada bloque y lo muestra como línea horizontal si está activado  
- El renderizado usa rectángulos rellenos diferenciados según validez y ruptura

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El nombre `Prise` está mal escrito en `Swing`, debería ser `Price`  
- La lógica de ruptura usa `Close` pero no contempla máximos/mínimos, lo que puede causar errores en trampas  
- No hay validación de solapamiento o duplicidad de bloques en misma zona  
- No hay alertas ni etiquetas con valores clave (ni fecha, ni barra, ni precio)  
- Se recalcula todo el bloque si cambia una vela → puede afectar rendimiento en gráficos densos

---

### 🛠️ Propuestas de mejora

- Corregir `Prise` a `Price` por claridad y coherencia  
- Permitir mostrar valores numéricos o etiquetas con info del bloque  
- Añadir opción para alertas visuales/auditivas al testeo o ruptura del bloque  
- Incluir lógica para agrupar bloques solapados o muy cercanos  
- Optimizar el cálculo para evitar recalcular bloques enteros innecesariamente

