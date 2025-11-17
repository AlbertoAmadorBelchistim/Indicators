## 🟦 Hurst Exponent (8/10)

**Nombre del archivo:** `HurstExponent.cs`  
**Nombre del indicador:** Hurst Exponent  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602551](https://help.atas.net/support/solutions/articles/72000602551)

---

### ⚙️ Parámetros configurables

- **Length**: Periodo base sobre el cual se evalúa el exponente, en potencias de 2:  
  - 32 (2⁵),  
  - 64 (2⁶),  
  - 128 (2⁷)

---

### 🧭 Clasificación  
📂 Statistical — Indicadores basados en propiedades estadísticas del comportamiento del precio

---

### 🧠 Uso más frecuente

- Evaluar si un mercado presenta **comportamiento persistente, aleatorio o antipersistente**  
- Detectar **tendencias estables** o **estructura de reversión** mediante análisis fractal  
- Usar el Hurst exponent como **filtro estructural** en sistemas de seguimiento o reversión

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Proporciona una medida objetiva de la "memoria del mercado"  
✅ Útil como filtro estructural o de volatilidad implícita  
⛔ Difícil de interpretar sin contexto estadístico  
⛔ Sensible al tipo de activo y ruido en temporalidades cortas

---

### 🎯 Estrategias donde se aplica

- **Seguir tendencia si H > 0.5**: el mercado tiende a continuar  
- **Operar reversión si H < 0.5**: el mercado tiende a revertir  
- **Evitar operar si H ≈ 0.5**: comportamiento aleatorio  
- Filtrar estrategias automáticas en función del régimen estructural

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Length**: `64` (equilibrio entre sensibilidad y fiabilidad)  
- Mostrar en panel separado  
- Marcar líneas guía en `0.5` (umbral aleatoriedad) y `0.7` (alta persistencia)

✅ Permite ajustar setups al régimen actual del mercado  
✅ Compatible con sistemas adaptativos o autocontenidos

---

### 🧪 Notas de desarrollo

- Calcula una regresión logarítmica sobre múltiples escalas de subdivisión del periodo base (potencias de 2)  
- En cada subdivisión:
  - Calcula media, desviación estándar y rango acumulado (máximo-mínimo)  
  - Evalúa el valor de log(range / stdDev) y log(period)  
- Se estima el exponente Hurst como la pendiente de la regresión logarítmica:
  $$
  H = \frac{n \cdot \sum(xy) - \sum x \cdot \sum y}{n \cdot \sum(x^2) - (\sum x)^2}
  $$
- El valor se toma en **valor absoluto** y se guarda en `_renderSeries`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El valor calculado se **normaliza por valor absoluto**, lo cual puede eliminar información de dirección útil en estudios más avanzados  
- No hay visualización del grado de confianza o dispersión de la regresión (ajuste R²)  
- El uso de `maxSum == 0` y `minSum == 0` como condiciones iniciales puede generar sesgo si el acumulado es simétrico  
- Las subdivisiones se limitan a potencias de 2 sin validación flexible o adaptativa

---

### 🛠️ Propuestas de mejora

- Permitir mostrar el signo real del exponente, no sólo su valor absoluto  
- Añadir una línea guía dinámica con color si el exponente está por encima o debajo de 0.5  
- Incluir opción de exportar o mostrar `R²` de la regresión para evaluar calidad de ajuste  
- Exponer como serie secundaria el valor del rango promedio o desviación estándar usada
