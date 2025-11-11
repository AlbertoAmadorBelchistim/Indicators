## 🟨 Study Angle (2/10)

  

**Nombre del archivo:**  `Angle.cs`

**Nombre del indicador:** Study Angle

**Web oficial:**  [ATAS - Study Angle](https://help.atas.net/support/solutions/articles/72000602533)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Número de barras utilizadas para calcular el ángulo (por defecto: 10)

  

---

  

### 🧭 Clasificación

📂 Trend — Indicadores que derivan la dirección o pendiente de una serie temporal

  

---

  

### 🧠 Uso más frecuente

  

- Evaluar la **pendiente de una media móvil** o serie de datos para anticipar cambios de tendencia

- Cuantificar la **fuerza direccional** de un movimiento reciente

- Aplicar como filtro para estrategias de tendencia (ángulos positivos = sesgo alcista)

  

---

  

### 📊 Nivel de relevancia

🔟 **2 / 10**

  

✅ Ofrece una medida objetiva de pendiente basada en ángulo

✅ Intuitivo para interpretar visualmente inclinación de movimientos

⛔ No distingue entre movimientos bruscos o suaves, ni entre tendencias y retrocesos

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Confirmación de entrada**: solo entrar si el ángulo supera cierto umbral (p. ej., ±30°)

- **Filtro direccional**: evitar operar contra ángulos muy pronunciados

- **Salida anticipada**: si el ángulo se aplana (se acerca a 0°), posible debilidad

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `5`

➤ Responde más rápido a cambios direccionales, útil en marcos cortos como 1 minuto

➤ Puede generar más ruido, por lo que conviene combinar con filtros adicionales (p. ej., delta o volumen)

  

---

  

### 🧪 Notas de desarrollo

  

- El ángulo se calcula como la arcotangente del cambio de valor en `Period` barras, dividido por el número de barras.

- El resultado se convierte a grados usando `* 180 / Math.PI`.

- Los valores se guardan en una `ValueDataSeries` y se visualizan en un nuevo panel.

- La línea cero sirve de referencia visual para distinguir inclinaciones positivas y negativas.

  

---

  

### 🛠️ Propuestas de mejora

  

- ✅ **[✔] Código funcional y sin errores**

- ❕ Añadir protección ante divisiones por cero o valores nulos/extremos (seguridad defensiva)

- ❕ Añadir opción para elegir la fuente (`Close`, `Median`, `TypPrice`) mediante propiedad `SeriesSource`

- ❕ Opcional: permitir normalizar el ángulo según la escala del activo (por volatilidad o rango medio)

- ❕ Agregar visualización de color (verde si ángulo > 0, rojo si < 0) para mejor claridad

<!--stackedit_data:
eyJoaXN0b3J5IjpbMTcyMDI5Mjc1N119
-->