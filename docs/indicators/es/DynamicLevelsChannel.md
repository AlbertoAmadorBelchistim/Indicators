## 🟦 Dynamic Levels Channel (8.5/10)

**Nombre del archivo:** `DynamicLevelsChannel.cs`  
**Nombre del indicador:** Dynamic Levels Channel  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602381](https://help.atas.net/support/solutions/articles/72000602381)

---

### ⚙️ Parámetros configurables

- **CalcMode**: Fuente de cálculo para el POC (Volume, PosDelta, NegDelta, Delta)  
- **Period**: Número de velas consideradas (por defecto: 40)  
- **Days**: Número de días hacia atrás a partir de los cuales iniciar el análisis (por defecto: 20)  
- **AreaColor**: Color del canal de valor (entre VAL y VAH)  
- **UseApproximationAlert**: Activar alerta por proximidad al POC  
- **ApproximationFilter**: Número de ticks de margen para lanzar alerta de aproximación  
- **UseAlerts / UsePocTouchAlert / UseValTouchAlert / UseVahTouchAlert**: Activación de alertas específicas  
- **AlertFile**: Sonido asociado a las alertas  
- **AlertBGColor / AlertForeColor**: Colores de fondo y texto para las alertas

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Indicadores de volumen por nivel con áreas dinámicas

---

### 🧠 Uso más frecuente

- Visualizar **POC, VAH y VAL dinámicos** dentro de un canal móvil de volumen o delta  
- Detectar zonas de posible **rechazo, ruptura o absorción**  
- Generar alertas cuando el precio alcanza o se aproxima a niveles relevantes  
- Mostrar señales de entrada (flechas) cuando se cumple cierta proporción entre el POC y los extremos

---

### 📊 Nivel de relevancia  
🔟 **8.5 / 10**

✅ Visualmente claro y táctico, con canal de valor incluido  
✅ Soporta múltiples modos de cálculo (Volume, Delta, PosDelta, NegDelta)  
⛔ No guarda niveles históricos anteriores  
⛔ La lógica de señales puede resultar opaca si no se comprende la relación entre POC y extremos

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión con test de VAL / VAH**: si hay confluencia con desequilibrios  
- **Breakout validado**: si el precio sale del canal y no vuelve en X velas  
- **Alerta de aproximación**: para preparar entrada manual cuando el precio se acerca al POC  
- **Entrada confirmada**: si la mecha alcanza un extremo y el precio se mueve contra el POC (señal de flecha)

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `40`  
- **Days**: `3`  
- **CalcMode**: `Delta` o `PosDelta`  
- **ApproximationFilter**: `2`  
- **AreaColor**: rojo translúcido  
- **AlertFile**: `alert1`  
- **AlertBGColor / AlertForeColor**: negro y blanco

✅ Preciso para operativas tácticas basadas en presión institucional  
✅ Compatible con DOM Strength y CVD

---

### 🧪 Notas de desarrollo

- Acumula volumen por nivel mediante `GetPriceVolumeInfo()`  
- Calcula el **POC** dependiendo del `CalcMode`: por volumen, delta, positivo o negativo  
- El canal (área de valor) se construye expandiendo desde el POC hasta cubrir el 70% del volumen  
- Las líneas POC, VAL, VAH se actualizan cada barra  
- Si se detecta rechazo en VAH o VAL y se cumple una proporción respecto al POC, se dibuja una **flecha de señal**  
- Genera alertas configurables cuando el precio toca o se aproxima a los niveles claves

---

### ❗ Incoherencias o aspectos mejorables detectadas

- La lógica para determinar VAL y VAH no garantiza simetría ni continuidad entre barras si el volumen se dispersa rápidamente  
- No hay protección si la lista `_volumeGroup` está vacía o tiene niveles muy próximos entre sí  
- El uso de `_signals` no elimina duplicados si la vela actual forma dos señales contrarias consecutivas  
- En `GetArea()`, el bucle puede seguir creciendo aunque ya se haya superado el volumen objetivo

---

### 🛠️ Propuestas de mejora

- Exponer el área como `RangeDataSeries` para compatibilidad visual cruzada  
- Incluir visualización de los últimos niveles históricos anteriores  
- Añadir opción de extensión de líneas horizontales hasta el margen derecho  
- Incluir leyendas o etiquetas con los valores exactos de POC, VAH y VAL  
- Mostrar número de veces que el precio tocó un nivel dentro del periodo para filtrar ruido