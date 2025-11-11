## 🟨 ATR Normalized (5/10)

  

**Nombre del archivo:**  `ATRN.cs`

**Nombre del indicador:** ATR Normalized

**Web oficial:**  [ATAS -   ATR Normalized](https://help.atas.net/support/solutions/articles/72000602633)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Periodo del ATR clásico utilizado para normalizar el valor (por defecto: 10)

  

---

  

### 🧭 Clasificación

📂 Volatility — Indicadores de volatilidad y rango normalizado

  

---

  

### 🧠 Uso más frecuente

  

- Evaluar la **volatilidad relativa** de cada vela en relación con su precio de cierre

- Normalizar la lectura del ATR para diferentes activos o marcos temporales

- Detectar **cambios abruptos de volatilidad** con un valor comparable entre sesiones o instrumentos

  

---

  

### 📊 Nivel de relevancia

🔟 **5 / 10**

  

✅ Ofrece una medida estandarizada de volatilidad útil para sistemas cuantitativos

✅ Fácil de combinar con filtros de entrada o salida basados en rangos

⛔ No incluye niveles de alerta ni zonas visuales por defecto

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Filtrado de entradas**: evitar entradas en zonas de baja volatilidad (ATRN bajo)

- **Salidas basadas en expansión**: salir tras alcanzar un umbral de ATR normalizado

- **Comparación de activos**: detectar cuáles tienen mayor volatilidad relativa intradiaria

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `10` (valor estándar que captura la volatilidad de las últimas 10 velas)

  

✅ Este valor permite detectar de forma ágil los cambios en el rango relativo

✅ Compatible con filtros dinámicos de entrada por rango

⛔ Requiere integración con otros indicadores para generar señales completas

  

---

  

### 🧪 Notas de desarrollo

  

- Internamente utiliza el indicador ATR clásico con un periodo configurable

- La fórmula aplicada es:

(ATR / Cierre actual) × 100

- El resultado se guarda en la serie `RenderSeries` para ser mostrado en un panel independiente

- La visualización está normalizada en porcentaje, lo que permite comparaciones entre distintos instrumentos o precios

---

## Comentario Gemini
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTEyMzYwMDc2MTUsLTE2NDU0MjU3OTNdfQ
==
-->