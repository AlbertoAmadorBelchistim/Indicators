## 🟦 Arms Ease of Movement (EMV) (6.5/10)

**Nombre del archivo:** `EMV.cs`  
**Nombre del indicador:** Arms Ease of Movement  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602315](https://help.atas.net/support/solutions/articles/72000602315)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para suavizar el valor de EMV (por defecto: 9)  
- **MaType**: Tipo de media móvil utilizada para el suavizado (`EMA`, `SMA`, `WMA`, `SMMA`, `LinearReg`)

---

### 🧭 Clasificación  
📂 Volume — Indicadores que relacionan volumen con desplazamiento del precio

---

### 🧠 Uso más frecuente

- Medir la **facilidad con la que el precio se desplaza en relación al volumen**  
- Detectar **fases de movimiento eficiente o ineficiente**  
- Confirmar rupturas o fases de expansión con mayor claridad

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Mide relación entre volumen y rango, no solo precio  
✅ Ofrece información de momentum con ajuste por fricción  
⛔ No es intuitivo sin conocer la fórmula  
⛔ Depende de datos completos y puede oscilar bruscamente con velas anómalas

---

### 🎯 Estrategias de scalping donde se aplica

- **Breakout eficiente**: si el EMV sube con vela expansiva y volumen bajo → movimiento limpio  
- **Reversión por agotamiento**: caída del EMV indica falta de eficiencia en impulso  
- **Confirmación contextual**: combinar con volumen o delta para validar fases de movimiento

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `5` a `9`  
- **MaType**: `EMA` o `WMA` para mayor reactividad  
- Combinar con VWAP o CVD para contextos estructurales

✅ Compatible con setups de impulso y ruptura  
✅ Puede anticipar falta de eficiencia en movimientos fuertes

---

### 🧪 Notas de desarrollo

- Calcula:
  $$
  EMV = \frac{\text{Midpoint}_{t} - \text{Midpoint}_{t-1}}{\text{Volume} / (\text{High} - \text{Low})}
  $$
- El resultado se suaviza usando el tipo de media móvil seleccionado  
- El valor final se guarda en `_renderSeries[bar]`  
- Usa objetos de tipo `EMA`, `SMA`, `SMMA`, `WMA` o `LinearReg` internamente para suavizado  
- Inicializa el valor en `bar == 0` con `value = 0`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- Si `High == Low`, el cálculo de `ratio` da 0, lo cual impide calcular el EMV aunque pueda haber datos relevantes  
- La inicialización de `_movingIndicator` no se protege ante errores si el tipo es desconocido  
- La lógica de `OnRecalculate()` recrea el objeto cada vez que se llama, lo que podría optimizarse

---

### 🛠️ Propuestas de mejora

- Añadir control sobre el caso `High == Low` usando una tolerancia mínima para evitar división por cero sin eliminar señal  
- Exponer los valores sin suavizar como una segunda serie para análisis más fino  
- Añadir opción para representar el indicador como histograma o con zonas de color  
- Permitir alertas cuando el EMV cruza ciertos niveles o cambia de signo
