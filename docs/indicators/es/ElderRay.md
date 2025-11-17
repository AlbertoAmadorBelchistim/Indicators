## 🟦 Elder Ray (6.5/10)

**Nombre del archivo:** `ElderRay.cs`  
**Nombre del indicador:** Elder Ray  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602580](https://help.atas.net/support/solutions/articles/72000602580)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de la media exponencial (EMA) sobre la cual se calculan los valores de fuerza alcista y bajista (por defecto: 10)

---

### 🧭 Clasificación  
📂 Momentum — Indicadores que evalúan presión alcista y bajista relativa al promedio

---

### 🧠 Uso más frecuente

- Evaluar la **presión de compra (bull power)** y la **presión de venta (bear power)**  
- Determinar si los compradores o vendedores están dominando respecto al equilibrio medio (EMA)  
- Confirmar señales de entrada/salida basadas en desbalance de poder

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Intuitivo, fácil de visualizar e interpretar  
✅ Útil para detectar divergencias o cambios de dominancia  
⛔ Puede ser sensible a velas con mechas extremas  
⛔ Requiere contexto para evitar señales falsas

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada tras cruce de bear power a positivo**: señal de recuperación del control por parte de los compradores  
- **Confirmación de impulso**: si bull power aumenta en expansión alcista  
- **Divergencia bajista**: si el precio sube pero bear power es cada vez menos negativo

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`  
- Representar en panel separado como histograma dual (positivo y negativo)  
- Línea base en 0 como referencia clave  
- Combinar con absorciones o DOM para filtrar señales

✅ Aporta contexto útil de presión  
✅ Compatible con otras herramientas de microestructura

---

### 🧪 Notas de desarrollo

- Usa una **EMA de cierre** como línea base de comparación  
- Calcula:
  - **Bull Power**: `High - EMA`  
  - **Bear Power**: `Low - EMA`  
- Ambas series (`_bullSeries`, `_bearSeries`) se dibujan en el mismo panel  
- La EMA se calcula internamente por objeto dedicado (`_ema`) con periodo ajustable  
- Se usa color verde por defecto para la línea de presión alcista

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay control visual si ambas fuerzas (bull y bear) son negativas simultáneamente → puede confundirse en zonas de compresión  
- No expone directamente la EMA utilizada, lo cual podría ser útil para trazar en el gráfico principal  
- El nombre de los parámetros no especifica que es una EMA, lo que podría inducir a pensar que es configurable (SMA/WMA)

---

### 🛠️ Propuestas de mejora

- Añadir opción para visualizar también la EMA en el gráfico de precio  
- Permitir seleccionar el tipo de media (EMA, SMA, WMA)  
- Incorporar alertas visuales o sonoras al cruce por 0  
- Incluir lógica de divergencias automáticas como herramienta opcional
