## 🟦 OI Analyzer (9/10)

**Nombre del archivo:** `OIAnalyzer.cs`  
**Nombre del indicador:** OI Analyzer  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602437](https://help.atas.net/support/solutions/articles/72000602437)

---

### ⚙️ Parámetros configurables

- **OiMode**: Tipo de operación a mostrar (`Buys` o `Sells`)  
- **CalculationMode**: Tipo de análisis (`CumulativeTrades` o `SeparatedTrades`)  
- **CumulativeMode**: Mostrar valores acumulados o reseteados por barra  
- **ClustersMode**: Modo visual en clúster o estilo tradicional  
- **CustomDiapason / FilterRange**: Rango personalizado de escala (mínimo/máximo)  
- **GridStep / Pen**: Tamaño y estilo de la rejilla horizontal  
- **Colores visuales**: para velas alcistas, bajistas y texto  
- **ShowCurrentValue**: Mostrar valor actual en el eje  
- **Autor**: Sotnikov Denis

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Indicador basado en cambios de interés abierto por tipo de operación

---

### 🧠 Uso más frecuente

- Medir la **intensidad y dirección del flujo institucional** mediante el interés abierto  
- Identificar acumulación/distribución según `Buy OI` o `Sell OI`  
- Confirmar rupturas, absorciones o agotamientos mediante el comportamiento del OI

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Herramienta clave para analizar **apertura o cierre real de posiciones**  
✅ Soporta granularidad por `trade` o `tick`, y acumulación total o parcial  
⛔ Necesita buena configuración y comprensión del contexto para evitar malinterpretaciones

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por acumulación institucional**: OI aumenta + agresión visible  
- **Reversión tras trampa**: OI cae mientras el precio se mantiene o sube  
- **Confirmación de ruptura** si el OI acompaña la dirección del precio

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **OiMode**: `Buys`  
- **CalculationMode**: `CumulativeTrades`  
- **CumulativeMode**: `true`  
- **ClustersMode**: `false`  
- **GridStep**: `500`  
- **CustomDiapason**: desactivado salvo análisis concreto

✅ Permite seguir a los participantes dominantes en tiempo real  
✅ Compatible con análisis de absorción y presión  
⛔ Su lectura aislada puede ser engañosa sin delta, volumen y contexto

---

### 🧪 Notas de desarrollo

- Solicita datos de tipo `CumulativeTrade` y actualiza en tiempo real y al cargar histórico  
- Representa velas sobre eje de volumen según el cambio en el OI y dirección de los trades  
- Soporta visualización en clúster o en eje horizontal con valores dinámicos  
- Filtra operaciones por dirección (`Buy` / `Sell`) y por tipo (`Cumulative` o `Tick`)  
- Permite configurar un rango visual y alerta visual en la zona actual

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se valida si el `GridStep == 0` al activar la cuadrícula, lo que puede causar fallo silencioso  
- Si se activa `CustomDiapason` pero los valores `From > To`, no hay validación ni mensaje de error  
- El objeto `_prevCandle` se clona sin validación de nulidad en casos iniciales  
- En `CumulativeTradesResponse`, si no se devuelven datos se recurre a `Calculate(0,0)`, lo cual puede confundir al usuario si el gráfico no se dibuja  
- No hay alertas ni anotaciones si se detecta una caída brusca o ruptura en el OI

---

### 🛠️ Propuestas de mejora

- Añadir validación cruzada entre `From` y `To` en `CustomDiapason`  
- Mostrar mensaje informativo si el `CumulativeTradesRequest` no devuelve datos  
- Implementar alertas visuales al detectar cambios bruscos en OI  
- Incluir opción para mostrar tooltips o etiquetas flotantes con el valor del OI por vela  
- Añadir opción de colorear las velas según intensidad o pendiente del cambio de OI

