## 🟦 Repulse (6/10)

**Nombre del archivo:** `Repulse.cs`  
**Nombre del indicador:** Repulse  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602283](https://help.atas.net/support/solutions/articles/72000602283)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para el cálculo principal (por defecto: 10)

---

### 🧭 Clasificación
📂 Momentum — Indicador de presión compradora vs. vendedora suavizada mediante EMAs

---

### 🧠 Uso más frecuente

- Medir la **intensidad comparada entre fuerzas alcistas y bajistas**  
- Confirmar fases de impulso o debilidad  
- Filtrar movimientos sin intención en estructuras laterales

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Indicador poco común pero útil como filtro de fuerza estructural  
✅ Suavizado interno reduce el ruido y facilita la lectura  
⛔ Interpretación más abstracta que otros osciladores clásicos

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de dirección** si la línea se mantiene claramente positiva o negativa  
- **Detección de giro** si cambia de pendiente bruscamente  
- **Filtro de contexto** para evitar operar en zonas sin presión dominante

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `8`  
✅ Suavizado suficiente sin perder sensibilidad en M1  
✅ Detecta fuerza direccional en tramos estructurados  
⛔ Puede dar señales erróneas si hay gap o vela con rango extremo

---

### 🧪 Notas de desarrollo

- Calcula dos componentes: `bull` (fuerza alcista) y `bear` (fuerza bajista), en función del `Close`, `Open` y extremos  
- Usa dos EMAs (`_emaHigh`, `_emaLow`) con periodo igual a `Period × 5`  
- El valor final es: `Repulse = EMA(bull) − EMA(bear)`  
- En las primeras `Period` barras se usa `GetCandle(0)` como referencia inicial  
- Se guarda el resultado en `_renderSeries`, mostrada en panel separado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El uso de `GetCandle(0)` para las primeras barras puede generar sesgos importantes si el primer `Open` es atípico  
- El factor fijo de multiplicación `Period × 5` para las EMAs no es configurable desde la UI  
- No se valida si `Close == 0`, lo que podría causar división por cero en escenarios extremos  
- No hay opción de visualizar las líneas `bull` y `bear` por separado  
- No existe codificación de color ni alertas al cruce de cero o al cambio de pendiente

---

### 🛠️ Propuestas de mejora

- Permitir configurar el multiplicador de suavizado de las EMAs  
- Añadir validación o manejo explícito si `Close == 0` para evitar errores silenciosos  
- Incluir visualización opcional de `EMA(bull)` y `EMA(bear)` como líneas separadas  
- Implementar alertas al cruce con cero o cambios significativos de pendiente  
- Permitir colorear la línea principal según dirección (verde/rojo, por ejemplo)

