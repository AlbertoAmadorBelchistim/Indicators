## 🟦 KDJ (6/10)

**Nombre del archivo:** `KDJ.cs`  
**Nombre del indicador:** KDJ  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602287](https://help.atas.net/support/solutions/articles/72000602287)

---

### ⚙️ Parámetros configurables

- **PeriodK**: Periodo de cálculo para la línea %K del estocástico  
- **PeriodD**: Periodo de suavizado inicial para la línea %D  
- **SlowPeriodD**: Periodo de suavizado adicional para la línea lenta (%J implícita)

---

### 🧭 Clasificación
📂 Momentum — Variante del oscilador estocástico que incluye línea %J como señal extrema

---

### 🧠 Uso más frecuente

- Detección de giros rápidos con confirmación por línea %J  
- Identificar condiciones extremas mediante divergencias o cruces entre %K, %D y %J  
- Generar señales más agresivas en comparación con el estocástico tradicional

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Señales más tempranas que KD-Fast o KD-Slow  
✅ Buena visualización de aceleración del momentum  
⛔ Mayor probabilidad de ruido y señales falsas sin confirmación adicional

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce de %J con %K / %D** para anticipar entrada/salida  
- **Reversión extrema**: cuando %J supera ampliamente el rango normal  
- **Confirmación de divergencia** entre precio y %K/%J

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **PeriodK**: `9`  
- **PeriodD**: `3`  
- **SlowPeriodD**: `3`

✅ Ofrece sensibilidad suficiente para identificar giros de corto plazo  
✅ La línea %J puede actuar como señal temprana para operaciones rápidas  
⛔ Requiere filtro de tendencia o volumen para evitar falsas señales

---

### 🧪 Notas de desarrollo

- Este indicador encapsula el cálculo del `KdSlow`, y deriva una tercera línea:  
  **%J = 3 * %K - 2 * %D**, generada como `RenderSeries`  
- Usa tres `ValueDataSeries`: %K, %D y %J  
- Se inicializa añadiendo internamente `KdSlow` y reaprovechando sus series  
- Dibuja todas las líneas en un panel independiente (`NewPanel`)

---

### ❗ Incoherencias o aspectos mejorables detectadas

- Se duplica el nombre de propiedad `PeriodD` tanto en el grupo "ShortPeriod" como en "LongPeriod", lo que puede causar confusión o errores de configuración  
- El nombre del tercer componente (%J) no aparece explícitamente en la UI, dificultando su interpretación  
- No se incluye validación para valores negativos de la línea %J (puede salir del rango 0-100 y confundir al usuario)  
- No se ofrece visualización de zonas típicas de sobrecompra/sobreventa  
- Depende del indicador `KdSlow`, lo cual impide extender o modificar fácilmente la lógica de %K y %D desde `KDJ` sin duplicación

---

### 🛠️ Propuestas de mejora

- Clarificar en la interfaz el nombre y valor de la línea %J, incluyendo su propósito  
- Añadir opción para mostrar zonas 20 / 80 como referencia visual  
- Permitir elegir entre diferentes fórmulas de %J (no solo `3K - 2D`)  
- Agregar alertas configurables para cruces y zonas extremas  
- Corregir la duplicación de nombre en la propiedad `PeriodD`

