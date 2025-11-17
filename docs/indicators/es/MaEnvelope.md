## 🟦 Moving Average Envelope (5.5/10)

**Nombre del archivo:** `MaEnvelope.cs`  
**Nombre del indicador:** Moving Average Envelope  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602431](https://help.atas.net/support/solutions/articles/72000602431)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de la media móvil simple base (por defecto: 10)  
- **CalcMode**: Modo de cálculo de la desviación (valor fijo o porcentaje)  
- **Value**: Valor de desviación (en puntos si es `FixedValue`, en % si es `Percentage`)

---

### 🧭 Clasificación
📂 Level — Canal basado en media móvil con bandas superior e inferior

---

### 🧠 Uso más frecuente

- Detectar **sobrecompra o sobreventa relativa** al promedio reciente  
- Confirmar movimientos extendidos o reversiones hacia la media  
- Usar como **canal visual de comportamiento del precio**

---

### 📊 Nivel de relevancia
🔟 **5.5 / 10**

✅ Canal clásico usado en sistemas de reversión o breakout  
✅ Flexibilidad con dos modos de cálculo (fijo o porcentual)  
⛔ No refleja volatilidad del precio (bandas son fijas)

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión desde bandas externas**: entrada cuando el precio toca la banda superior o inferior  
- **Filtro de rango**: evitar operar fuera del canal si hay sobreextensión  
- **Confirmación de breakout** si el precio se mantiene fuera del canal por varias velas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `20`  
- **CalcMode**: `Percentage`  
- **Value**: `0.3`

✅ Captura variaciones pequeñas y relevantes en gráficos de 1 minuto  
✅ Proporciona referencia clara para niveles de entrada/salida  
⛔ En mercados de alta volatilidad puede resultar insuficiente sin adaptación dinámica

---

### 🧪 Notas de desarrollo

- El canal se construye a partir de una `SMA` y una desviación configurable  
- Si `CalcMode` es `FixedValue`, se usa un desplazamiento fijo en puntos  
- Si es `Percentage`, las bandas se calculan como ±% del valor de la `SMA`  
- Usa tres `ValueDataSeries`: banda superior, inferior y línea central (`SMA`)  
- No emplea volatilidad ni desviación estándar

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay validación contextual si `Value` es inapropiado para el modo (por ejemplo, 1 punto fijo en Nasdaq no equivale a 1% de `SMA`)  
- No se visualiza la banda central (`SMA`) si no se configura explícitamente su color  
- No hay indicación ni alerta si el precio rompe el canal  
- No hay compatibilidad con otras medias (solo SMA), lo que limita su flexibilidad  
- No permite relleno entre bandas, lo que podría mejorar la visualización

---

### 🛠️ Propuestas de mejora

- Añadir compatibilidad con otros tipos de medias móviles (`EMA`, `SMMA`, etc.)  
- Incluir opción para mostrar relleno visual entre bandas superior e inferior  
- Incorporar alertas cuando el precio toque o cruce las bandas  
- Añadir etiquetas de precio para las bandas  
- Permitir diferentes colores dinámicos según si el precio está dentro o fuera del canal

