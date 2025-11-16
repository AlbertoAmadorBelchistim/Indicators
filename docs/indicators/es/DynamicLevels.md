## 🟦 Dynamic Levels (9/10)

**Nombre del archivo:** `DynamicLevels.cs`  
**Nombre del indicador:** Dynamic Levels  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602380](https://help.atas.net/support/solutions/articles/72000602380)

---

### ⚙️ Parámetros configurables

- **Days**: Número de días hacia atrás desde los que acumular datos  
- **PeriodFrame**: Periodo de reinicio (Daily, Weekly, Monthly, Hourly, H4, All)  
- **Type**: Fuente del clúster (`Volume`, `Delta`, `Bid`, `Ask`, `Tick`, `Time`)  
- **Filter**: Mínimo valor para mostrar el POC  
- **ShowVolumes**: Mostrar etiquetas con los volúmenes  
- **VizualizationType**: `Accumulated` o `AtStart`  
- **Alertas**: Aproximación, toque de POC, VAH, VAL  
- **Colores**: Fondo y texto para alertas, color del texto de etiquetas  
- **AlertFile**: Archivo de sonido para alerta  
- **TextColor**: Color del texto mostrado junto al POC

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Niveles dinámicos basados en volumen, delta y otras métricas de clúster

---

### 🧠 Uso más frecuente

- Detectar y visualizar el **nivel de valor dominante (POC)** dentro de un periodo móvil  
- Mostrar **VAH y VAL** (Value Area High / Low) con sus rangos y alertas asociadas  
- Confirmar confluencias en zonas de soporte/resistencia dinámicas  
- Observar zonas de absorción o intención agresiva en tiempo real

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Altamente configurable, con soporte para real time y backtest  
✅ Compatible con múltiples fuentes de clúster (Volume, Delta, Ask, etc.)  
⛔ Código complejo con múltiples niveles de anidación  
⛔ No incluye histórico gráfico de niveles anteriores

---

### 🎯 Estrategias de scalping donde se aplica

- **Rechazo o ruptura del POC**: entrada si el precio rebota o atraviesa el nivel dominante  
- **Absorción detectada en VAH/VAL**: alerta cuando el precio toca esos niveles  
- **Confluencia con Delta/DOM**: validar setups cuando el POC se alinea con señales de agresión  
- **Uso táctico de alertas**: actuar si el precio se aproxima al POC con volumen creciente

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **PeriodFrame**: `Daily`  
- **Days**: `3`  
- **Type**: `Volume` o `Delta`  
- **Filter**: `1000`  
- **UseApproximationAlert / UsePocTouchAlert**: `true`  
- **VizualizationType**: `Accumulated`  
- **TextColor**: blanco o verde intenso sobre fondo negro

✅ Permite reactividad táctica  
✅ Compatible con setups de reversión y ruptura

---

### 🧪 Notas de desarrollo

- Acumula datos por vela o tick usando una clase interna `DynamicCandle`  
- Calcula `POC`, `VAH`, `VAL`, volumen total y valor máximo por nivel  
- Representa:
  - POC como línea naranja cuadrada  
  - VAH / VAL como líneas marrones  
  - Área de valor como fondo sombreado  
- Incluye lógica de alertas cuando el precio toca o se aproxima a estos niveles  
- Soporta cálculo por clúster en tiempo real (`OnNewTrades`) y por vela (`OnCalculate`)  
- Controla visualmente los textos, colores y puntos finales de línea

---

### ❗ Incoherencias o aspectos mejorables detectados

- En `GetValueArea`, los niveles se expanden desde el POC, pero no garantizan simetría ni contexto con volumen reciente  
- El método `CalculateValues` reescribe etiquetas aunque no haya cambio de valor, lo que **puede generar sobrecarga gráfica**  
- El uso de `value >= Filter` para determinar visibilidad puede ocultar niveles importantes si hay poco volumen general  
- La lógica de `AddTick` y `AddCandle` duplica cálculos en escenarios mixtos sin consolidación

---

### 🛠️ Propuestas de mejora

- Añadir opción para **mostrar niveles anteriores** en histórico gráfico  
- Incluir suavizado o agregación de ticks para evitar sobreprocesamiento  
- Optimizar gestión de etiquetas para reducir carga visual  
- Permitir agrupar por rango de precios (binning) para mercados volátiles  
- Exponer `_closedCandle.GetValueArea()` como serie secundaria para trazados cruzados
