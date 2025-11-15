## 🟦 Chaikin Money Flow (CMF) (7/10)

**Nombre del archivo:** `CMF.cs`  
**Nombre del indicador:** Chaikin Money Flow  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602540](https://help.atas.net/support/solutions/articles/72000602540)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para el cálculo del flujo monetario acumulado (por defecto: 21)

---

### 🧭 Clasificación  
📂 Volume — Indicadores de volumen tradicional acumulado

---

### 🧠 Uso más frecuente

- Medir el **flujo de dinero** que entra o sale del mercado en un periodo  
- Detectar divergencias entre el precio y el volumen  
- Confirmar la validez de una ruptura con volumen creciente o decreciente  
- Identificar presión compradora o vendedora oculta

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Combina información de precio y volumen en una sola línea de interpretación sencilla  
✅ Muy utilizado en estrategias de acumulación/distribución  
⛔ Suavizado lento, menos útil en contextos de alta frecuencia  
⛔ Puede dar señales falsas en mercados sin tendencia o de bajo volumen

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de entrada**: solo operar rupturas cuando el CMF es positivo (presión compradora)  
- **Filtrado de falsas rupturas**: evitar entradas si el CMF contradice el movimiento del precio  
- **Divergencias**: aprovechar discrepancias entre el CMF y el precio como señal anticipada de reversión

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10` o `13` (valores más reactivos que el estándar de 21)

✅ Aumenta la sensibilidad para detectar cambios de flujo rápido  
✅ Requiere confirmación adicional para evitar señales erróneas

---

### 🧪 Notas de desarrollo

- Calcula el **ADL (Acumulation/Distribution Line)** diario en función del rango de cada vela:  
  \[ AD = \left(\frac{(Close - Low) - (High - Close)}{High - Low}\right) \times Volume \]
- La curva CMF es el cociente entre la suma del AD y la suma del volumen durante el periodo seleccionado  
- Se utilizan tres series de datos:
  - `_cmf`: línea continua gris del flujo total  
  - `_cmfHigh`: área verde si el valor es positivo  
  - `_cmfLow`: área roja si el valor es negativo  
- El cálculo reinicia máximos y mínimos diarios al detectar una nueva sesión

---

### 🛠️ Propuestas de mejora

- Permitir mostrar directamente la **línea cero** como referencia visual  
- Añadir opción para **cambiar los periodos de los colores positivos/negativos** por separado  
- Incorporar señales visuales (círculos, flechas) en los cruces del nivel cero  
- Posibilidad de suavizar el resultado final con una media móvil adicional