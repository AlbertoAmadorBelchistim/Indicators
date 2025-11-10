## 🟦 ADX (Average Directional Index) (6/10)

**Nombre del archivo:** `ADX.cs`  
**Nombre del indicador:** ADX  
**Web oficial:** [ATAS - ADX](https://help.atas.net/support/solutions/articles/72000602313)

![ADX](../img/ADX.png)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de cálculo de DI+ y DI− (por defecto: 14)
- **SmoothPeriod**: Suavizado del ADX mediante media RMA (por defecto: 14)

---

### 🧭 Clasificación
📂 Trend — Indicadores de fuerza de tendencia

---

### 🧠 Uso más frecuente

- Medir la **fuerza de la tendencia** sin importar la dirección
- Detectar **momentos de consolidación** (ADX bajo) o de fuerte dirección (ADX alto)
- Confirmar entradas si el ADX supera cierto umbral (por ejemplo, 20 o 25)
- Combinar con el cruce de DI+ y DI− para validar señales de compra o venta

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Estable y clásico en análisis de tendencias  
✅ Compatible con sistemas de seguimiento de tendencia  
⛔ Poco útil en estrategias de scalping basadas en order flow o microestructura  

---

### 🎯 Estrategias de scalping donde se aplica

- **Filtro de contexto**: Solo operar rupturas cuando el ADX > 25
- **Validación de impulso**: Entrada si DI+ cruza sobre DI− y ADX está subiendo
- **Evitar laterales**: No entrar si ADX < 15

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`  
- **SmoothPeriod**: `5`  

✅ Configuración más rápida para detectar impulsos recientes  
✅ Útil como filtro de fondo en sistemas multivariables  
⛔ No recomendable como señal primaria de entrada

---

### 🧪 Notas de desarrollo

- El indicador usa medias **RMA** (Running Moving Average) para suavizar TR, DI+ y DI−.
- Calcula el ADX como:  
  `ADX = 100 × RMA( |DI+ − DI−| / (DI+ + DI−) )`
- Se implementan tres instancias de RMA:
  - `_plusDmRma`: para el movimiento direccional positivo
  - `_minusDmRma`: para el negativo
  - `_trRma`: para el True Range
  - `_adxRma`: para suavizar la relación entre DI+ y DI−
- Se manejan correctamente las condiciones de inicialización (`bar == 0`) y se evita división por cero.

---

### 🛠️ Propuestas de mejora

- Añadir umbral visual (línea horizontal) para destacar zonas de ADX > 25 o < 15
- Incluir opción de resaltar visualmente el cruce de DI+ y DI−
- Permitir alertas al superar ciertos niveles de ADX
- Incorporar versión rápida del indicador con menor suavizado por defecto

---

### Comentario de Gemini

Aquí tienes la "pregunta clave" de este indicador:

**¿Está el mercado en una tendencia fuerte (ya sea alcista o bajista), o está simplemente en un 'rango' lateral?**

### 📈 ¿Es útil para Scalping en S&P 500?

**Sí, pero solo como un "filtro de régimen"**.

El ADX es un indicador con un **retraso (lag) extremo** por diseño. Es una media móvil de una media móvil. La imagen que has proporcionado lo demuestra perfectamente:

* Observa el *claro* movimiento bajista de 09:20 a 10:45. El ADX (línea verde) sube correctamente, confirmando la tendencia.
* Ahora mira el *giro en V* en las 10:45. El precio revierte bruscamente, pero el ADX sigue subiendo hasta las 11:15, casi 30 minutos *después* de que la tendencia bajista haya muerto.

**Conclusión:**
Nunca debes usar el ADX para *entrar* o *salir* de una operación de scalping. Cuando te da la señal, el movimiento ya ha terminado.

Su único uso es como un "interruptor" en tu sistema general:

* **Si ADX < 20:** El mercado está en rango. (Activar estrategias de reversión a la media, evitar operar rupturas).
* **Si ADX > 25 y subiendo:** El mercado está en tendencia. (Activar estrategias de pullback/continuación, evitar buscar techos/suelos).