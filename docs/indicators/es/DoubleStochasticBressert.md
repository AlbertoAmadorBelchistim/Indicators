## 🟦 Double Stochastic - Bressert (6.5/10)

**Nombre del archivo:** `DoubleStochasticBressert.cs`  
**Nombre del indicador:** Double Stochastic - Bressert  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602377](https://help.atas.net/support/solutions/articles/72000602377)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo base para los máximos y mínimos del estocástico (por defecto: 10)  
- **SmaPeriod**: Suavizado del estocástico base (por defecto: 10)  
- **Smooth**: EMA adicional aplicada al valor final (por defecto: 10)

---

### 🧭 Clasificación  
📂 Momentum — Osciladores suavizados para impulso adaptativo

---

### 🧠 Uso más frecuente

- Detectar **reversiones con suavizado avanzado**  
- Reducir ruido del doble estocástico original aplicando una **tercera capa de suavizado**  
- Filtrar señales erráticas en entornos de alta volatilidad

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Más robusto frente al ruido de mercado que su versión clásica  
✅ Útil como filtro avanzado para setups basados en estructura  
⛔ Ligeramente más retardado que el Double Stochastic convencional  
⛔ Requiere ajuste fino de los tres periodos para cada activo

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión confirmada**: entrada tras giro del Bressert tras zona extrema  
- **Doble confirmación**: usar el Bressert como filtro adicional del estocástico normal  
- **Salida táctica**: si el valor se aplana o gira en zona sobrecomprada o sobrevendida

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`  
- **SmaPeriod**: `5`  
- **Smooth**: `3`  
- Líneas auxiliares en `20`, `50`, `80`  
- Combinar con volumen o delta para validar señales

✅ Reduce señales erráticas en ciclos cortos  
✅ Ideal para zonas de giro anticipadas

---

### 🧪 Notas de desarrollo

- Usa internamente una instancia de `DoubleStochastic`, con sus propios cálculos de %K1, %D1 y %K2  
- Sobre el resultado final del Double Stochastic, aplica una **EMA adicional (`_ema`)**  
- El valor representado en `_renderSeries` es:
  \[
  \text{DStoch}_{\text{Bressert}} = EMA(\text{DStoch})
  \]
- Todas las configuraciones de periodo están sincronizadas con la instancia `DoubleStochastic`

---

### ❗ Incoherencias o aspectos mejorables detectados

- No hay control si `_ds[bar]` es `NaN` o fuera de rango: podría dar errores visuales en el arranque  
- No se exponen los valores intermedios (%K2 o EMA sin suavizar) para comparación  
- No ofrece ningún tipo de señal visual o línea auxiliar, sólo la curva final suavizada

---

### 🛠️ Propuestas de mejora

- Permitir mostrar los valores internos del `DoubleStochastic` como líneas auxiliares  
- Añadir alertas o marcas visuales al cruzar niveles clave (20 / 80)  
- Incluir opción de líneas horizontales de referencia y etiquetas automáticas  
- Documentar claramente que se trata de una capa adicional sobre el Double Stochastic
