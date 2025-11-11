## 🟦 Awesome Oscillator (4/10)

  

**Nombre del archivo:**  `AO.cs`

**Nombre del indicador:** Awesome Oscillator

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602325](https://help.atas.net/support/solutions/articles/72000602325)

  

---

  

### ⚙️ Parámetros configurables

  

- **P1 (LongPeriod)**: Periodo largo de la media (por defecto: 34)

- **P2 (ShortPeriod)**: Periodo corto de la media (por defecto: 5)

- **PosColor**: Color cuando el histograma sube (por defecto: verde)

- **NegColor**: Color cuando el histograma baja (por defecto: rojo)

- **NeutralColor**: Color cuando el histograma permanece sin cambio (por defecto: gris)

  

---

  

### 🧭 Clasificación

📂 Momentum — Indicadores que miden la fuerza o velocidad del movimiento del precio

  

---

  

### 🧠 Uso más frecuente

  

- Evaluar **momentum del mercado** a corto plazo

- Confirmar **cambios de dirección** en la tendencia

- Identificar **divergencias** con el precio

  

---

  

### 📊 Nivel de relevancia

🔟 **4 / 10**

  

✅ Indicador clásico ampliamente conocido

✅ Fácil de interpretar visualmente como histograma

⛔ No considera volumen ni agresión, por lo que no refleja intención real del mercado

⛔ Puede generar señales falsas en consolidaciones

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Cruces de línea cero**: entrada en favor del impulso cuando el histograma cruza desde negativo a positivo

- **Confirmación de rompimientos**: histograma creciente como validación del impulso

- **Divergencias**: cuando el precio marca nuevos máximos/mínimos pero el AO no lo confirma

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **P1**: `34`

- **P2**: `5`

- **PosColor / NegColor / NeutralColor**: usar colores muy visibles en tu plantilla (verde, rojo, gris)

- Se recomienda usarlo junto con otros indicadores de flujo (como delta o volumen agresivo) para validar señales

  

✅ Proporciona una lectura sencilla de momentum

✅ Útil como filtro adicional en contextos de alta direccionalidad

⛔ Debe evitarse como única fuente de señal en scalping

  

---

  

### 🧪 Notas de desarrollo

  

- Calcula la diferencia entre dos medias móviles simples del **precio medio** (High + Low) / 2.

- Si el valor actual supera el anterior, colorea el histograma en verde; si es menor, en rojo; si es igual, en gris.

- El histograma se construye con `ValueDataSeries` y `VisualMode.Histogram`.

  

### 🛠️ Propuestas de mejora

  

- Añadir una **línea de cero** como referencia visual directa

- Incluir alertas cuando se crucen niveles clave (como el cero)

- Ofrecer opción para calcular usando cierre en lugar de precio medio

- Agregar una **media del AO** para detectar cruce de impulso

- Permitir filtros de volumen o volatilidad para reducir señales falsas
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTc4MjQ0OTEwMCwtMjA4ODc0NjYxMl19
-->