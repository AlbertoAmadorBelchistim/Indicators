## 🟦 Ichimoku Kinko Hyo (9/10)

**Nombre del archivo:** `Ichimoku.cs`  
**Nombre del indicador:** Ichimoku Kinko Hyo  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602553](https://help.atas.net/support/solutions/articles/72000602553)

---

### ⚙️ Parámetros configurables

- **Tenkan (Conversión)**: Periodo de la línea Tenkan-Sen (por defecto: 9)  
- **Kijun (Base)**: Periodo de la línea Kijun-Sen (por defecto: 26)  
- **Senkou (Span B)**: Periodo de la línea Senkou Span B (por defecto: 52)  
- **Displacement**: Desplazamiento hacia adelante (por defecto: 26)  
- **Days**: Número de sesiones atrás desde las que se empieza a calcular (0 = todo el histórico)  

---

### 🧭 Clasificación  
📂 Trend — Indicadores estructurales completos de tendencia y equilibrio

---

### 🧠 Uso más frecuente

- Evaluar tendencia con múltiples referencias: **Tenkan, Kijun, Senkou Span A/B, Lagging**  
- Confirmar rupturas con desplazamiento visual (Kumo)  
- Detectar zonas de **equilibrio/desbalance** mediante el cloud (Kumo)

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Ofrece visión estructural avanzada en un solo indicador  
✅ Soporta cálculo y visualización desplazada y retrospectiva  
⛔ Requiere comprensión técnica del método para interpretar correctamente  
⛔ Puede saturar el gráfico si se usa sin filtros

---

### 🎯 Estrategias donde se aplica

- **Confirmación de tendencia**: cuando precio > nube y Tenkan > Kijun  
- **Reversión en borde del Kumo**: entrada si el precio rebota en la nube  
- **Cruce de Tenkan/Kijun**: como señal anticipada de cambio  
- **Lagging span**: confirmación de momentum si sigue la dirección del precio

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Tenkan**: `9`  
- **Kijun**: `26`  
- **Senkou**: `52`  
- **Displacement**: `26`  
- Visualizar solo nube y líneas principales, omitir Lagging si hay saturación

✅ Visualmente robusto para contextos de ruptura  
✅ Compatible con indicadores de volumen o absorción

---

### 🧪 Notas de desarrollo

- Calcula:
  - **Tenkan** = (High + Low)/2 en los últimos `9` periodos  
  - **Kijun** = (High + Low)/2 en los últimos `26`  
  - **Senkou A** = (Tenkan + Kijun)/2 desplazado `+26`  
  - **Senkou B** = (High + Low)/2 de los últimos `52` desplazado `+26`  
  - **Chikou** (Lagging) = Cierre desplazado `-26`  
- Dibuja la nube (`Kumo`) usando `RangeDataSeries` (`_upSeries` y `_downSeries`) según cruce de Senkou A y B  
- Permite establecer un número de sesiones (`Days`) para limitar el cálculo histórico

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El método `SetPointOfEndLine` se usa sin validación si el gráfico tiene menos datos de los esperados  
- Los valores `lineBar` se calculan como `bar + displacement - 1`, lo cual puede causar conflictos si se cambia el desplazamiento sin recalcular  
- No se permite seleccionar cuáles líneas mostrar desde la UI  
- No existe soporte directo para alertas al cruzarse líneas (ej. Tenkan > Kijun)

---

### 🛠️ Propuestas de mejora

- Añadir checkboxes para mostrar/ocultar cada componente (Tenkan, Kijun, Senkou A/B, Lagging)  
- Incluir lógica de alertas para cruces Tenkan/Kijun o ruptura del Kumo  
- Permitir colorear la nube según dirección (verde/roja según A > B o B > A)  
- Añadir etiquetas con los valores actuales de cada línea para lectura rápida
