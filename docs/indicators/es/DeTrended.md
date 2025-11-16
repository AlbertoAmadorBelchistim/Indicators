## 🟦 DeTrended Price Oscillator (DPO) (6.5/10)

**Nombre del archivo:** `DeTrended.cs`  
**Nombre del indicador:** DeTrended Price Oscillator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602370](https://help.atas.net/support/solutions/articles/72000602370)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para calcular la SMA central que se desplaza (por defecto: 10)

---

### 🧭 Clasificación  
📂 Momentum — Osciladores centrados que eliminan la tendencia

---

### 🧠 Uso más frecuente

- Identificar ciclos de **corto plazo** eliminando la tendencia general  
- Detectar **máximos o mínimos relativos** con mayor claridad  
- Usar como base para **estrategias de reversión** u osciladores compuestos

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Ayuda a ver ciclos ocultos sin interferencia de la tendencia  
✅ Su señal es más clara que una SMA simple  
⛔ No es adecuado para evaluar dirección general del mercado  
⛔ Requiere combinación con otro indicador para entradas claras

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión en sobreextensión**: entrada cuando el DPO cambia de signo tras alejarse del cero  
- **Confirmación de ciclos**: usar máximos/mínimos del DPO como anclajes de microestructuras  
- **Filtro contextual**: evitar trades largos si DPO cae, o cortos si DPO sube con fuerza

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10` a `14`  
- Línea cero activa como referencia  
- Combinar con delta o volumen para validar cambios de ciclo

✅ Excelente para setups cíclicos de reversión  
✅ Compatible con herramientas de absorción o divergencia

---

### 🧪 Notas de desarrollo

- Calcula una SMA sobre el precio con un desfase de:
  \[
  \text{Desfase} = \left( \frac{\text{Period}}{2} \right) / 2 + 1
  \]
- El valor final es la diferencia entre el precio actual y la SMA desplazada:
  \[
  DPO_t = \text{Precio}_t - SMA_{t - \text{Desfase}}
  \]
- Se representa como una serie única (`_renderSeries`)  
- Si `bar < lookBack`, no se calcula valor para evitar errores

---

### ❗ Incoherencias o aspectos mejorables detectados

- El desfase calculado como `Period / 2 / 2 + 1` puede no corresponder con las fórmulas estándar (algunos usan simplemente `Period / 2 + 1`)  
- No hay opción para elegir entre precio de cierre, típico o ponderado  
- No representa visualmente la línea cero ni da señales claras por sí solo  
- La SMA interna (`_sma`) no está expuesta para visualización comparativa

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar la **línea cero** de referencia  
- Permitir elegir el **tipo de precio** (Close, Typical, Weighted)  
- Incluir visualización de la SMA desplazada como línea secundaria  
- Mostrar alertas o etiquetas cuando el DPO cruce cero desde extremos