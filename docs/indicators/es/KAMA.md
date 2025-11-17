## 🟦 Kaufman Adaptive Moving Average (KAMA) (7/10)

**Nombre del archivo:** `KAMA.cs`  
**Nombre del indicador:** Kaufman Adaptive Moving Average  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602525](https://help.atas.net/support/solutions/articles/72000602525)

---

### ⚙️ Parámetros configurables

- **EfficiencyRatioPeriod**: Periodo usado para calcular la eficiencia del movimiento (por defecto: 10)
- **ShortPeriod**: Periodo para la constante de suavizado rápida (por defecto: 2)
- **LongPeriod**: Periodo para la constante de suavizado lenta (por defecto: 30)

---

### 🧭 Clasificación
📂 Trend — Media móvil adaptativa basada en eficiencia de movimiento

---

### 🧠 Uso más frecuente

- Suavizar precios respetando la dirección del mercado  
- Adaptarse a cambios en volatilidad: más sensible en tendencias, más suave en consolidaciones  
- Identificar cambios de dirección relevantes sin ruido

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Se ajusta automáticamente según la eficiencia del movimiento  
✅ Útil en sistemas de seguimiento de tendencia  
⛔ Menos útil en mercados extremadamente laterales

---

### 🎯 Estrategias de scalping donde se aplica

- **Seguimiento de tendencia en M1**: se adapta rápido a cambios de dirección real  
- **Confirmación de breakout**: línea de KAMA gira o se inclina bruscamente  
- **Filtro direccional**: evita operar en contra del sesgo dominante

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **EfficiencyRatioPeriod**: `10`  
- **ShortPeriod**: `2`  
- **LongPeriod**: `30`  

✅ Rápida respuesta a cambios en tendencias en gráficos de 1 minuto  
✅ Filtro útil para operaciones impulsivas  
⛔ Puede producir retardos en giros muy bruscos si LongPeriod es demasiado alto

---

### 🧪 Notas de desarrollo

- El cálculo usa una lista interna de cierres (`_closeList`) para medir la eficiencia como cambio / suma de volatilidades.  
- La constante de suavizado (`sc`) se adapta dinámicamente entre los extremos definidos por `ShortPeriod` y `LongPeriod`.  
- Se aplica una fórmula cuadrática sobre `sc` para aumentar el contraste entre momentos de alta y baja eficiencia.  
- La inicialización ocurre en `bar == 0` y gestiona limpieza de datos si hay repetición de `bar`.

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se comprueba si la lista `_closeList` está vacía antes de acceder a `LastOrDefault` (aunque está protegida indirectamente, sería más robusto validarlo)  
- La eliminación de datos antiguos en `_closeList` se realiza fuera del control de condiciones de repetición de barra, lo que puede provocar desalineación  
- No se ofrece al usuario la posibilidad de visualizar el valor de la eficiencia (ER), lo cual puede ser informativamente útil  
- No se han incluido validaciones explícitas contra divisiones por cero, aunque el código intenta gestionarlo con un `if (volatilitySum == 0)`  

---

### 🛠️ Propuestas de mejora

- Añadir opción de visualizar la línea en un panel independiente (ahora `DenyToChangePanel = true`)  
- Permitir aplicar sobre series distintas al cierre (por ejemplo: `Typical`, `Median`)  
- Incluir alertas cuando el ángulo de inclinación supere cierto umbral (giros bruscos)  
- Mostrar visualmente la eficiencia (ER) en otro DataSeries para análisis adicional



