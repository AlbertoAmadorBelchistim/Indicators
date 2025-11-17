## 🟦 Ergodic Oscillator (6.5/10)

**Nombre del archivo:** `Ergodic.cs`  
**Nombre del indicador:** Ergodic  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602382](https://help.atas.net/support/solutions/articles/72000602382)

---

### ⚙️ Parámetros configurables

- **ShortPeriod**: Periodo de las EMAs rápidas (por defecto: 5)  
- **LongPeriod**: Periodo de las EMAs lentas (por defecto: 20)  
- **SignalPeriod**: Periodo del suavizado de la señal (por defecto: 5)

---

### 🧭 Clasificación  
📂 Momentum — Osciladores basados en movimiento relativo suavizado

---

### 🧠 Uso más frecuente

- Detectar **cambios de momentum suavizado** mediante el doble suavizado de diferencias de precio  
- Evaluar **aceleración o desaceleración** del precio respecto a su trayectoria reciente  
- Usar la **diferencia entre el TSI (True Strength Index) y su señal** como activador táctico

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Útil como filtro para confirmar dirección del impulso  
✅ Más estable que otros osciladores de ciclo rápido  
⛔ Puede tener retardo en entornos de alta volatilidad  
⛔ No representa el TSI ni su señal por separado

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada en reversión suave**: si el valor del Ergodic cruza de negativo a positivo  
- **Confirmación de impulso**: entrada si el valor se aleja del eje cero con pendiente creciente  
- **Cruce de línea base (0)** como disparador para setups en ruptura o continuidad

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ShortPeriod**: `5`  
- **LongPeriod**: `20`  
- **SignalPeriod**: `5`  
- Añadir líneas guía horizontales en `0.0`, `+0.1`, `-0.1`  
- Usar en conjunto con Delta, DOM Strength o CVD para validación

✅ Suaviza el impulso sin sobreajustar  
✅ Ideal para detectar giros estructurales consistentes

---

### 🧪 Notas de desarrollo

- Calcula la diferencia entre `value` y el valor previo (`SourceDataSeries[bar - 1]`)  
- Aplica:
  - EMA larga sobre la diferencia  
  - EMA larga sobre el valor absoluto de la diferencia  
  - Luego, EMA corta sobre cada uno de los resultados anteriores  
- Calcula el **TSI** como:
  $$
  \text{TSI}_t = \frac{\text{EMA}_\text{corta}(\text{EMA}_\text{larga}(\Delta P))}{\text{EMA}_\text{corta}(\text{EMA}_\text{larga}(|\Delta P|))}
  $$
- Aplica una EMA adicional al TSI como línea de señal  
- El valor representado es la **diferencia entre TSI y su señal**, es decir:  
  $$
  \text{Ergodic}_t = \text{TSI}_t - \text{Señal}_t
  $$

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se protege contra división por cero si `emaShortAbs[bar] == 0` (aunque improbable, puede darse si no hay movimiento)  
- No se expone visualmente la línea de señal ni el propio TSI, lo cual puede dificultar la interpretación completa  
- No permite cambiar el tipo de media móvil (solo EMA disponible)

---

### 🛠️ Propuestas de mejora

- Exponer el TSI y la señal como series auxiliares para análisis comparativo  
- Incluir opción para activar **alertas de cruce por cero** o cruce con la señal  
- Permitir personalizar el tipo de suavizado (SMA, WMA, SMMA)  
- Añadir visualización tipo histograma para representar la diferencia (valor actual)
