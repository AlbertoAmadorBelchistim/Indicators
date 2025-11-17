## 🟦 Momentum (7/10)

**Nombre del archivo:** `Momentum.cs`  
**Nombre del indicador:** Momentum  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602429](https://help.atas.net/support/solutions/articles/72000602429)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras hacia atrás para medir el impulso (por defecto: 10)  
- **ShowSMA**: Mostrar media del valor del Momentum  
- **SmaPeriod**: Periodo para la media del Momentum si está activada (por defecto: 10)

---

### 🧭 Clasificación
📂 Momentum — Indicador clásico de impulso basado en diferencia de precios

---

### 🧠 Uso más frecuente

- Medir la **fuerza y dirección** del movimiento reciente del precio  
- Identificar fases de **aceleración** o **debilitamiento** del mercado  
- Servir como **base para estrategias de cruce de línea cero**

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Intuitivo y reactivo en tendencias rápidas  
✅ Compatible con sistemas de cruce de línea cero o divergencias  
⛔ Sensible a picos de precio, especialmente con periodos bajos

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce con cero** como señal de entrada/salida  
- **Confirmación de impulso**: valores crecientes con pendiente positiva  
- **Soporte para divergencias**: comparación con máximos/mínimos del precio

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `9`  
- **SmaPeriod**: `5`  
- **ShowSMA**: `true`

✅ Responde rápidamente a los movimientos de corto plazo  
✅ La media ayuda a filtrar señales ruidosas  
⛔ Menos útil en consolidaciones sin dirección clara

---

### 🧪 Notas de desarrollo

- Calcula la diferencia entre el valor actual y el de `Period` barras atrás  
- Añade una SMA opcional sobre la serie de momentum  
- La SMA se representa como segunda serie (`SmaSeries`) y puede activarse desde el panel  
- Compatible con el modo minimizado para mejorar rendimiento

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay validación si `Period > CurrentBar`, aunque se mitiga con `Math.Max(0, ...)`  
- La visualización por defecto no incluye línea cero, clave en este tipo de osciladores  
- La media (`SmaSeries`) no puede personalizar su color o grosor desde la UI  
- No se ofrece modo de histograma, que suele ser útil para visualizar impulso  
- No hay alerta ni codificación de color por cruce con cero o pendiente

---

### 🛠️ Propuestas de mejora

- Añadir línea cero visible como referencia clave  
- Permitir configuración visual completa de la SMA (color, grosor, estilo)  
- Añadir opción de mostrar el Momentum como histograma  
- Incluir alertas visuales/sonoras para cruces con cero o cambios de pendiente  
- Ofrecer opción de suavizado adicional (por ejemplo, EMA en lugar de SMA)

