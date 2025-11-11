## 🟦 Adaptive Binary Wave (6/10)

  

**Nombre del archivo:** `AdaptiveBinaryWaveMA.cs`

**Nombre del indicador:** Adaptive Binary Wave

**Web oficial:** [ATAS - Adaptative Binary Wave](https://help.atas.net/support/solutions/articles/72000602535)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Periodo base del AMA y la desviación estándar (por defecto: según inicialización)

- **ShortPeriod**: Constante rápida del AMA

- **LongPeriod**: Constante lenta del AMA

- **Percent**: Porcentaje del umbral de desviación estándar que se usa para detectar señales binarias

  

---

  

### 🧭 Clasificación

📂 Trend — Indicadores de seguimiento de tendencia adaptativos basados en media móvil y estadística

  

---

  

### 🧠 Uso más frecuente

  

- Detectar puntos de cambio en la dirección del precio mediante un oscilador binario

- Confirmar señales de momentum basadas en la distancia del AMA respecto a sus extremos recientes

- Filtrar zonas sin tendencia (valores = 0) para evitar operaciones en consolidaciones

  

---

  

### 📊 Nivel de relevancia

🔟 **6 / 10**

  

✅ Proporciona señales binarias claras basadas en lógica adaptativa

✅ Reduce el ruido al usar un umbral dinámico basado en desviación estándar

⛔ Puede requerir ajuste fino de `Percent` para distintos activos o marcos temporales

⛔ No muestra visualmente los niveles del AMA ni la desviación, solo el resultado binario

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Pullback controlado**: señal = -1 dentro de tendencia alcista para esperar giro

- **Confirmación de ruptura**: señal = 1 tras consolidación o soporte reciente

- **Reversión rápida**: detectar cambio con confirmación al pasar de -1 a 1 sin zona neutra

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `21`

- **ShortPeriod**: `2`

- **LongPeriod**: `30`

- **Percent**: `25`

  

✅ Reduce señales falsas en consolidación

✅ Reacciona con agilidad a cambios bruscos sin repintar

⛔ Puede dar señales tardías en velas de rango amplio o gap

  

---

  

### 🧪 Notas de desarrollo

  

- El indicador usa una **Adaptive Moving Average (AMA)** para suavizar el precio

- Se calculan los **máximos y mínimos locales del AMA**, actualizados solo si se supera el anterior

- Usa una **desviación estándar sobre el AMA** como filtro de umbral

- Se genera una **señal binaria**:

- `+1` si AMA se aleja del mínimo previo más que el umbral

- `-1` si AMA cae desde el máximo previo más que el umbral

- `0` en el resto de casos

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir opción para **mostrar visualmente el AMA, AMA High y AMA Low**

- Incluir series auxiliares para la desviación estándar y los umbrales (+/-)

- Permitir cambiar el modo de salida: binario, oscilador suavizado o modo histograma

- Agregar alertas sonoras/visuales al detectar cruce desde 0 a ±1

- Posibilidad de **filtrar señales según volumen o delta** para integrarlo en order flow

## Comentario Gemini
Aquí tienes la "pregunta clave" de este indicador:

¿Ha roto la media móvil adaptativa (AMA) su 'canal' reciente por una cantidad estadísticamente significativa?

----------
### ✍️ Mi Opinión sobre el Indicador

Este es uno de los indicadores conceptualmente **más inteligentes** que hemos visto hasta ahora.

Es una "evolución" del **AMA (Kaufman)** que tanto nos gustó. En lugar de definir un "rango" (chop) simplemente cuando la línea AMA se queda "plana", este indicador hace algo mucho más robusto:

1.  Calcula el AMA.
    
2.  Calcula la **volatilidad del propio AMA** (usando `StdDev`).
    
3.  Crea una **"zona neutral" dinámica** (el `deviation`).
    
4.  Solo te saca del estado `0` (chop) si el AMA hace un movimiento _estadísticamente significativo_ (es decir, que supera ese umbral de desviación).
    

Es un filtro de "régimen de tendencia vs. rango" muy, muy robusto.

----------

### 📈 ¿Es útil para Scalping?

Es una herramienta de **confirmación**, no de **entrada**.

-   Mira el mínimo de las ~22:05. El precio gira. El indicador no pasa a `+1` (alcista) hasta las ~22:20.
    
-   Mira el máximo de las ~23:00. El precio gira. El indicador no pasa a `-1` (bajista) hasta las ~23:15.
    

Para un scalper, 15-20 minutos (3-4 velas de M5) es una eternidad; el grueso del movimiento inicial ya ha pasado.

Por lo tanto, este indicador no te ayudará a _entrar_ en el giro, pero es **excelente** para _confirmar_ que el nuevo impulso es real y para _filtrar_ todas las pequeñas sacudidas (ruido) que ocurrieron entre las 19:20 y las 22:05.

----------
### Veredicto

Es un filtro de régimen (Tendencia/Rango) brillante, mucho más inteligente que el ADX. Pero su valor real está en gráficos de mayor temporalidad (H1/H4) o como un filtro de contexto.

**Acción:** **Conservar.** Es una herramienta de contexto de alta calidad, pero no un sistema de señales.
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTMxMzg2ODUyN119
-->