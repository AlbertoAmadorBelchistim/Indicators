## 🟦 Double Stochastic (7/10)

**Nombre del archivo:** `DoubleStochastic.cs`  
**Nombre del indicador:** Double Stochastic  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602610](https://help.atas.net/support/solutions/articles/72000602610)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo base para calcular el máximo y mínimo (por defecto: 10)  
- **SmaPeriod**: Periodo de suavizado EMA para ambas capas del estocástico (por defecto: 10)

---

### 🧭 Clasificación  
📂 Momentum — Osciladores suavizados de impulso relativo

---

### 🧠 Uso más frecuente

- Detectar **condiciones de sobrecompra y sobreventa suavizadas**  
- Confirmar señales de reversión con un doble filtrado por momentum  
- Filtrar falsas señales del estocástico tradicional mediante doble capa de suavizado

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Más robusto que el estocástico clásico en entornos volátiles  
✅ Menos propenso a señales falsas gracias al doble suavizado  
⛔ Puede tener más retardo en la señal  
⛔ Requiere calibración específica de ambos periodos para cada activo

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión suave**: entrar en favor del giro cuando el indicador cambia de pendiente  
- **Zona extrema confirmada**: esperar que el indicador cruce desde zonas superiores a 80 o inferiores a 20  
- **Cruce de la línea central**: como activador de entrada en zonas neutras

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`  
- **SmaPeriod**: `5`  
- Líneas auxiliares en `20`, `50` y `80`  
- Combinar con Delta o CVD para validar rupturas o absorciones

✅ Ideal como confirmación visual de setups  
✅ Evita la mayoría de señales erráticas de un %K básico

---

### 🧪 Notas de desarrollo

- Calcula una **primera capa de estocástico (%K1)** con:
  - High - Low sobre `Period`  
  - Cierre como punto de referencia  
- Aplica una **EMA al %K1** para obtener %D1  
- Luego, calcula el **%K2** sobre los valores de %D1:  
  - Busca el máximo y mínimo de %D1 en el mismo `Period`  
  - Normaliza el valor actual de %D1 en esa banda  
- Finalmente, **suaviza el %K2 con una segunda EMA**, que es el valor representado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El uso de `.MAX()` y `.MIN()` sobre `DataSeries[0]` de `_ema` puede **no estar alineado temporalmente** con el valor real de `fastD1` en barras anteriores si hay retraso en cálculos  
- El uso de `: 100m * (candle.Close - min)` cuando `(max - min == 0)` puede generar **valores artificiales muy altos o muy bajos**, en lugar de mantener un valor neutro  
- No hay opción para exponer o visualizar las curvas intermedias (`%K1`, `%D1`, `%K2`), lo que dificulta el análisis fino

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar los valores intermedios de %K1, %D1 y %K2 como series auxiliares  
- Corregir el caso degenerado `(max == min)` usando un valor neutro como `50`  
- Permitir seleccionar tipo de suavizado (EMA, SMA) para cada capa  
- Añadir soporte visual para zonas extremas y cruce de niveles relevantes (líneas guía en 20 y 80)