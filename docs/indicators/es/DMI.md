## 🟦 Dynamic Momentum Index (DMI) (6.5/10)

**Nombre del archivo:** `DMI.cs`  
**Nombre del indicador:** Dynamic Momentum Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602261](https://help.atas.net/support/solutions/articles/72000602261)

---

### ⚙️ Parámetros configurables

- **RsiPeriod**: Periodo base del RSI dinámico (por defecto: 14)  
- **RsiMin / RsiMax**: Límites inferior y superior del periodo dinámico permitido  
- **StdPeriod**: Periodo para el cálculo de la desviación estándar  
- **SmaPeriod**: Periodo para la media móvil usada como base de comparación

---

### 🧭 Clasificación  
📂 Momentum — Osciladores adaptativos basados en volatilidad

---

### 🧠 Uso más frecuente

- Detectar condiciones de **sobrecompra o sobreventa ajustadas a la volatilidad actual**  
- Utilizar un RSI dinámico que se adapta al mercado en vez de ser fijo  
- Evaluar momentos de aceleración o desaceleración del momentum con mayor precisión

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Más reactivo que un RSI clásico en mercados volátiles  
✅ Útil para adaptar setups en función del entorno (rango vs impulso)  
⛔ Más difícil de interpretar sin conocer su lógica interna  
⛔ Requiere más parámetros y calibración que un RSI tradicional

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada en reversión adaptativa**: cuando el DMI se cruza desde zona extrema con periodo corto  
- **Filtro de impulso**: operar solo si el DMI se encuentra en zona media con volatilidad creciente  
- **Divergencia dinámica**: detectar pérdida de momentum comparado con precio

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **RsiPeriod**: `14`  
- **RsiMin**: `3`  
- **RsiMax**: `30`  
- **StdPeriod**: `5`  
- **SmaPeriod**: `10`

✅ Permite adaptar señales de reversión a la velocidad del mercado  
✅ Mejora el timing en entornos cambiantes

---

### 🧪 Notas de desarrollo

- Calcula la **desviación estándar (`_std`)** y la **media móvil (`_sma`)** del precio  
- La relación `vi = std / sma` determina el periodo dinámico para el RSI  
- El RSI resultante se calcula como:
  $$
  RSI = 100 - \frac{100}{1 + \frac{\text{avgGain}}{\text{avgLoss}}}
  $$
- Usa promedios suavizados (`_posSmma`, `_negSmma`) en lugar de EMA clásica  
- El periodo final es ajustado entre `RsiMin` y `RsiMax` antes de calcular

---

### ❗ Incoherencias o aspectos mejorables detectadas

- Dentro de `RsiDynamic()`, se comprueba si `_negSmma != 0`, pero justo después hay otro `if (_negSmma == 0)`, lo cual es contradictorio y **puede dejar pasar errores lógicos**  
- No hay control explícito sobre valores NaN si la volatilidad (`vi`) se vuelve inestable  
- No se exponen visualmente los valores del periodo dinámico (`td`) que se está usando en cada vela

---

### 🛠️ Propuestas de mejora

- Corregir el bloque `if (_negSmma != 0)` que contiene un `if (_negSmma == 0)` redundante  
- Añadir opción para **mostrar el periodo dinámico como serie secundaria**  
- Permitir activar alertas visuales o sonoras cuando el DMI cruce niveles clave  
- Incluir bandas opcionales en los niveles clásicos (30, 70) adaptados al contexto