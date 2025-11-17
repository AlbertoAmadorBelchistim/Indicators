## 🟦 Force Index (7/10)

**Nombre del archivo:** `ForceIndex.cs`  
**Nombre del indicador:** Force Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602387](https://help.atas.net/support/solutions/articles/72000602387)

---

### ⚙️ Parámetros configurables

- **UseEma (PeriodFilter.Enabled)**: Activar suavizado mediante EMA  
- **Period (PeriodFilter.Value)**: Periodo del suavizado EMA (por defecto: 10)

---

### 🧭 Clasificación  
📂 Volume — Indicadores que combinan volumen con movimiento de precio

---

### 🧠 Uso más frecuente

- Medir la **fuerza del movimiento** combinando volumen y variación de precio  
- Confirmar **momentum direccional** en movimientos de alta participación  
- Filtrar señales débiles sin acompañamiento de volumen

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Indicador simple y reactivo para confirmar impulsos  
✅ Suavizado EMA opcional para eliminar ruido  
⛔ Puede generar ruido en velas con gaps o sin volumen  
⛔ Valor bruto depende de la escala del activo, por lo que no es normalizado

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de ruptura**: entrada si el Force Index crece junto con breakout  
- **Reversión sin fuerza**: evitar entradas si el valor es bajo o negativo  
- **Filtro direccional**: aceptar trades solo si el indicador coincide con la dirección de la entrada

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **UseEma**: `true`  
- **Period**: `8`  
- Visualizar con eje cero como referencia  
- Combinar con Delta, POC o DOM para confirmar agresión

✅ Lectura clara y directa  
✅ Compatible con herramientas visuales de volumen

---

### 🧪 Notas de desarrollo

- Calcula el índice como:
  $$
  FI_t = \text{Volumen}_t \times (\text{Cierre}_t - \text{Cierre}_{t-1})
  $$
- Si `UseEma` está activado, el valor se suaviza mediante EMA  
- Los parámetros se gestionan mediante un objeto `PeriodFilter`, que activa o desactiva el suavizado  
- La propiedad `UseEma` está oculta en UI (`[Browsable(false)]`), pero se controla desde el `Filter`

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El valor inicial (`bar == 0`) no se calcula, por lo que la primera barra siempre es cero  
- `UseEma` y `Period` están ocultos en la interfaz pero son configurables indirectamente  
- No hay validación explícita si `Period == 0`, aunque está acotado en los atributos `[Range]`

---

### 🛠️ Propuestas de mejora

- Hacer visibles las propiedades `UseEma` y `Period` en la UI para mayor transparencia  
- Añadir opción de representación tipo histograma  
- Permitir normalización por ATR o tick value para comparabilidad entre activos  
- Añadir alertas cuando el Force Index cruce 0 o supere máximos recientes
