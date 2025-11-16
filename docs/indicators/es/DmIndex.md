## 🟦 Directional Movement Index (DMI) (8/10)

**Nombre del archivo:** `DmIndex.cs`  
**Nombre del indicador:** Directional Movement Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602285](https://help.atas.net/support/solutions/articles/72000602285)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras utilizadas para suavizar DI+ y DI- mediante suma móvil (por defecto: 14)

---

### 🧭 Clasificación  
📂 Trend — Indicadores de dirección y fuerza de movimiento

---

### 🧠 Uso más frecuente

- Detectar qué fuerza direccional domina (DI+ vs DI-)  
- Confirmar si hay **tendencia definida** cuando uno de los valores se impone claramente  
- Usar junto con ADX para evaluar si la tendencia es fuerte o débil

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Implementación completa de los componentes DI+ y DI- en un solo indicador  
✅ Muy útil para sistemas direccionales de seguimiento de tendencia  
⛔ No incluye ADX, por lo que no mide fuerza de la tendencia  
⛔ Puede dar señales falsas en rangos laterales si se usa sin filtros

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce DI+ > DI-**: señal de entrada alcista (fortaleza compradora)  
- **Cruce DI- > DI+**: entrada bajista  
- **Filtro de tendencia**: operar solo si uno de los valores supera un umbral (ej. 20 o 25)

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10` a `14`  
- Color: azul para DI+, rojo para DI-  
- Umbrales visuales en `20` o `25` pueden marcar zonas de interés

✅ Muy eficaz para filtrar operaciones direccionales  
✅ Complemento ideal a ADX o setups de continuación

---

### 🧪 Notas de desarrollo

- Calcula:
  - `dmUp = High actual - High previo`  
  - `dmDown = Low previo - Low actual`  
- Realiza una **suma móvil suavizada** para cada uno (no media exponencial):  
  $$
  \text{Smooth}_{Up} = \text{Sum}_{Up} - \frac{\text{Sum}_{Up}}{Period} + dmUp
  $$
- Divide cada valor suavizado entre el ATR para normalizarlo y multiplicarlo por 100  
- Guarda los resultados en dos series: `_upSeries` y `_downSeries`  
- El ATR se calcula mediante el subindicador `_atr`

---

### ❗ Incoherencias o aspectos mejorables detectados

- El uso de `.CalcSum()` seguido de un suavizado no corresponde a la fórmula clásica de Welles Wilder, que usa un **True Range exponencialmente suavizado**  
- Los nombres `_dmDown` y `_dmUp` están **invertidos en cuanto a contenido**, lo que puede inducir a errores al leer el código (el campo `"DmUp"` contiene el cálculo de diferencia hacia abajo y viceversa)  
- No se incluye la **línea de ADX**, que suele acompañar al DMI para medir la fuerza

---

### 🛠️ Propuestas de mejora

- Corregir los nombres de las series `_dmUp` y `_dmDown` para evitar confusión  
- Añadir cálculo y visualización opcional de **ADX** en el mismo panel  
- Permitir visualización de líneas horizontales como umbrales (ej. 20 o 25)  
- Incluir alertas al producirse el cruce entre DI+ y DI-
