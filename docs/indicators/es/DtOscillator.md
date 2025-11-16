## 🟦 DT Oscillator (7/10)

**Nombre del archivo:** `DtOscillator.cs`  
**Nombre del indicador:** DT Oscillator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602379](https://help.atas.net/support/solutions/articles/72000602379)

---

### ⚙️ Parámetros configurables

- **RsiPeriod**: Periodo del RSI base (por defecto: 8)  
- **Period**: Periodo para el estocástico aplicado al RSI (por defecto: 5)  
- **SMAPeriod1**: Periodo de suavizado de la línea %K (por defecto: 3)  
- **SMAPeriod2**: Periodo de suavizado de la línea %D (por defecto: 3)

---

### 🧭 Clasificación  
📂 Momentum — Osciladores compuestos sobre RSI

---

### 🧠 Uso más frecuente

- Detectar **zonas de sobrecompra o sobreventa** basadas en RSI suavizado  
- Confirmar señales de reversión mediante cruces de líneas suavizadas  
- Analizar ciclos de impulso sin ruido de corto plazo

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Suavizado triple que reduce ruido en zonas extremas  
✅ Compatible con estrategias de cruce y reversión  
⛔ Puede tener retardo en señales rápidas  
⛔ No incluye lógica de alerta ni líneas auxiliares por defecto

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce %K y %D**: señal de entrada cuando la línea azul cruza la línea roja  
- **Confirmación de reversión**: si se produce cruce cerca de zona extrema  
- **Filtro de contexto**: evitar entradas si las líneas están planas o sin dirección clara

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **RsiPeriod**: `8`  
- **Period**: `5`  
- **SMAPeriod1**: `3`  
- **SMAPeriod2**: `3`  
- Líneas guía en `20`, `50` y `80`  
- Combinar con volumen o delta para validar giros

✅ Muy útil como filtro visual de setups  
✅ Compatible con estrategias de reversión en extremos

---

### 🧪 Notas de desarrollo

- Calcula un **Stochastic RSI** como base (con subindicador `StochasticRsi`)  
- Aplica un primer suavizado SMA al valor estocástico (línea %K → azul)  
- Luego aplica un segundo suavizado al %K para obtener %D (línea roja)  
- Fórmula general:
  \[
  \text{%K} = \text{SMA}(100 \times \text{StochRSI}) \quad ; \quad \text{%D} = \text{SMA}(\text{%K})
  \]
- Usa dos `ValueDataSeries`:  
  - `_skSeries`: línea principal azul  
  - `_sdSeries`: línea de señal roja

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se protege contra valores fuera del rango [0, 1] en `stochRsi`, aunque por definición debería estar contenido  
- No ofrece líneas horizontales (20 / 80) como referencia visual  
- No permite visualizar por separado el RSI o el estocástico si se quisiera comparar internamente  
- No hay alertas ni lógica de cruce implementada

---

### 🛠️ Propuestas de mejora

- Añadir líneas auxiliares de sobrecompra/sobreventa (20, 80)  
- Incluir alertas visuales o sonoras al cruzar %K y %D  
- Permitir mostrar el RSI base como línea secundaria  
- Ofrecer opción de modo histograma entre %K y %D para destacar giros