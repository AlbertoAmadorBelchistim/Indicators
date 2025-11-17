## 🟦 Exhaustion (8/10)

**Nombre del archivo:** `Exhaustion.cs`  
**Nombre del indicador:** Exhaustion  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000641184-exhaustion](https://help.atas.net/support/solutions/articles/72000641184-exhaustion)

---

### ⚙️ Parámetros configurables

- **CalcMode**: Fuente de cálculo (Bid, Ask, BidAndAsk, Volume)  
- **AmoutOfPrices**: Número de niveles de precios a evaluar (por defecto: 5)  
- **VisualType**: Tipo de objeto gráfico (Rectangle, Triangle, etc.)  
- **TopColor / BottomColor**: Color para zonas de resistencia/soporte  
- **TopClusterColor / BottomClusterColor**: Color de clúster en los niveles seleccionados  
- **ShowPriceSelection**: Mostrar o no la zona de selección de clúster  
- **Size**: Tamaño del objeto de visualización  
- **VisualObjectsTransparency**: Transparencia de los objetos (0 a 100)  
- **UseAlerts**: Activar alertas  
- **AlertFile**: Sonido asociado a la alerta  
- **OnBarCloseAlert**: Alerta solo al cierre de vela

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Detección de agotamiento por clúster en niveles

---

### 🧠 Uso más frecuente

- Detectar **agotamiento de compradores o vendedores** en los extremos de la vela  
- Marcar zonas de absorción donde hay subida en volumen o agresión sin continuación  
- Generar **alertas visuales y sonoras** si se detecta agotamiento en los últimos niveles

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Muy útil para operativas en extremos de rango o zonas críticas  
✅ Detecta patrones de falta de continuación tras agresión  
⛔ Solo evalúa el cluster actual (no contexto histórico)  
⛔ Sensible a la parametrización de niveles (AmoutOfPrices)

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión tras agresión fallida**: el precio rompe un nivel, pero se detecta agotamiento en clúster  
- **Entrada en zona de absorción**: si el clúster superior tiene volumen creciente pero no se rompe  
- **Alerta táctica en real time**: actuar si aparece señal de agotamiento y se confirma con volumen o delta

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **CalcMode**: `BidAndAsk`  
- **AmoutOfPrices**: `5`  
- **Size**: `10`  
- **ShowPriceSelection**: `true`  
- **UseAlerts**: `true`  
- **AlertFile**: `alert1`  
- **VisualObjectsTransparency**: `70`

✅ Compatible con estructuras tipo Spring, Upthrust, absorciones en VAH/VAL  
✅ Ideal como señal secundaria en zonas clave

---

### 🧪 Notas de desarrollo

- Evalúa los últimos niveles del **máximo o mínimo de la vela actual**  
- Recorre el clúster desde el extremo hasta encontrar `AmoutOfPrices` niveles consecutivos con volumen creciente  
- Dibuja objetos (`PriceSelectionValue`) sobre esos niveles con color y tooltip  
- Las señales se agrupan por barra en `_topSelection` y `_bottomSelection`  
- Las alertas se lanzan si el patrón se forma en la barra activa y cumple las condiciones definidas

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El nombre del parámetro `AmoutOfPrices` contiene un **error tipográfico** → debería ser `AmountOfPrices`  
- Si no se alcanzan los `AmoutOfPrices` niveles consecutivos con volumen creciente, **no se dibuja nada**, lo que puede confundir al usuario  
- En `OnCalculate`, el método `CalcFromLow` o `CalcFromHigh` se detiene si se encuentra un `info == null`, sin intentar niveles siguientes  
- Puede haber **duplicación de alertas** si el patrón aparece en ambos extremos y `OnBarCloseAlert = false`

---

### 🛠️ Propuestas de mejora

- Corregir el nombre del parámetro (`AmoutOfPrices` → `AmountOfPrices`)  
- Permitir mostrar **niveles parciales** aunque no se cumplan los `AmoutOfPrices` completos  
- Añadir etiquetas con el nivel y valor de volumen/agrupación al pasar el ratón  
- Mostrar líneas o puntos en el gráfico principal si `ShowPriceSelection = false`  
- Añadir modo que combine la lógica de absorción delta y agotamiento para mayor precisión