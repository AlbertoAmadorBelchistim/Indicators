## 🟦 MACD - Volume Weighted (8/10)

**Nombre del archivo:** `MacdVW.cs`  
**Nombre del indicador:** MACD - Volume Weighted  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602231](https://help.atas.net/support/solutions/articles/72000602231)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de suavizado de la línea de señal (por defecto: 9)  
- **ShortPeriod**: Periodo de cálculo de media corta ponderada por volumen (por defecto: 12)  
- **LongPeriod**: Periodo de cálculo de media larga ponderada por volumen (por defecto: 26)

---

### 🧭 Clasificación
📂 Momentum — Versión ponderada por volumen del MACD clásico

---

### 🧠 Uso más frecuente

- Identificar cambios en la tendencia ponderando la acción del precio por volumen  
- Confirmar movimientos de mayor relevancia al tener en cuenta la participación  
- Filtrar señales débiles en sesiones con bajo volumen

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Refina el MACD clásico incorporando el volumen como peso en el cálculo  
✅ Mejora la fiabilidad de señales en entornos institucionales o con fuerte liquidez  
⛔ Menos útil en activos o momentos con volumen muy bajo o errático

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por cruce** de MACD y señal, con mayor fiabilidad si el volumen acompaña  
- **Divergencia ponderada**: si el MACDVW diverge mientras el volumen es elevado  
- **Confirmación de ruptura**: histograma creciente junto con volumen ascendente

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ShortPeriod**: `8`  
- **LongPeriod**: `21`  
- **Period (signal)**: `5`

✅ Mejora la respuesta en gráficos de 1 minuto con alto volumen relativo  
✅ Aumenta la sensibilidad sin sacrificar consistencia  
⛔ Puede necesitar filtros adicionales para eliminar ruido en zonas de bajo volumen

---

### 🧪 Notas de desarrollo

- Sustituye las EMAs clásicas del MACD por medias **ponderadas por volumen**  
- Calcula dos valores VWAP (corto y largo) y los resta para obtener el MACD  
- Usa una EMA adicional para suavizar el resultado (línea de señal)  
- Representa el histograma (`MacdSeries`) y la línea de señal (`SignalSeries`) en un panel separado  
- Usa internamente dos `ValueDataSeries` auxiliares (`valVol` y `vol`) para cálculos acumulados

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se valida si `ShortPeriod` ≥ `LongPeriod`, lo que puede romper el significado del MACD  
- No hay visualización diferenciada entre histogramas positivos y negativos (sin color dinámico)  
- El cálculo puede fallar silenciosamente si `volSumShort` o `volSumLong` son cero (solo se retorna 0 sin alerta)  
- No permite cambiar el tipo de suavizado (solo EMA como línea de señal)  
- La lógica no permite visualizar la diferencia entre MACD y señal directamente como tercera serie

---

### 🛠️ Propuestas de mejora

- Añadir codificación de color en el histograma según signo del valor (verde/rojo, por ejemplo)  
- Mostrar tercera serie opcional: diferencia entre MACD y señal  
- Añadir validación cruzada de parámetros para evitar configuraciones incoherentes  
- Permitir seleccionar el tipo de media para suavizar la señal (`EMA`, `SMA`, etc.)  
- Implementar alertas visuales o auditivas para cruces clave

