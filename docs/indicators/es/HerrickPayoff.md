## 🟦 Herrick Payoff Index (HPI) (8/10)

**Nombre del archivo:** `HerrickPayoff.cs`  
**Nombre del indicador:** Herrick Payoff Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602286](https://help.atas.net/support/solutions/articles/72000602286)

---

### ⚙️ Parámetros configurables

- **Divisor**: Factor divisor aplicado al cálculo principal para escalar los valores  
- **Smooth**: Factor de suavizado aplicado al valor acumulado (por defecto: 10)  
- **PosColor / NegColor**: Colores para valores positivos o negativos del histograma

---

### 🧭 Clasificación  
📂 Volume — Indicadores que combinan volumen, OI y desplazamiento de precio

---

### 🧠 Uso más frecuente

- Evaluar la **fuerza real del movimiento** considerando el volumen, la variación del precio y el interés abierto (OI)  
- Confirmar tendencias con **participación activa (creciente)** o **salidas silenciosas**  
- Detectar divergencias entre precio y flujo de contratos

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Altamente informativo en mercados de futuros y opciones  
✅ Combinación única de volumen, OI y dirección  
⛔ Puede ser difícil de interpretar sin formación técnica previa  
⛔ Sensible a errores de OI en datos históricos

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de impulso real**: si el HPI crece con dirección y volumen creciente  
- **Divergencia oculta**: si el precio sube pero el HPI cae (disminución de interés)  
- **Salida anticipada**: si el HPI se aplana mientras el precio sigue subiendo

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Divisor**: `10000` (ajustado al tick size del activo)  
- **Smooth**: `5`  
- **PosColor / NegColor**: verde para compras, rojo para ventas  
- Combinar con POC o CVD para validar escenarios de reversión

✅ Muy potente en productos con OI fiable  
✅ Aporta contexto de participación institucional

---

### 🧪 Notas de desarrollo

- Fórmula base:
  $$
  \text{HPI}_t = \text{TickSize} \times \text{Volume} \times \frac{(HL_t - HL_{t-1})}{\text{Divisor}} \times \left(1 + 2 \cdot \frac{|OI_t - OI_{t-1}|}{\max(OI_t, OI_{t-1})}\right)
  $$
- HL es el punto medio: (High + Low) / 2  
- Se acumula una serie secundaria (`_hpiSec`) que se suaviza:  
  $$
  \text{Render}_t = \text{Render}_{t-1} + \text{Smooth} \cdot (\text{HPI}_t - \text{HPI}_{t-1})
  $$
- Los colores se aplican dinámicamente según signo del valor final

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se actualiza el OI si ambos `candle.OI` y `prev.OI` son 0 → el valor puede quedar indefinido  
- Se utiliza una lógica de acumulación que puede **desbordarse en sesiones largas** sin reinicio o normalización  
- El parámetro `Smooth` no actúa como una media móvil, sino como multiplicador de diferencia → puede generar valores muy grandes si hay picos en `_hpiSec`

---

### 🛠️ Propuestas de mejora

- Añadir opción para reiniciar acumulación por sesión o día  
- Incluir un modo alternativo con media móvil real (EMA/SMA) sobre `_hpiSec`  
- Exponer `_hpiSec` como serie secundaria para análisis más fino  
- Añadir línea cero y alertas opcionales cuando se crucen umbrales relevantes
