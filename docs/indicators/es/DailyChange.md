## 🟦 Daily Change (4.5/10)

**Nombre del archivo:** `DailyChange.cs`  
**Nombre del indicador:** Daily Change  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602542](https://help.atas.net/support/solutions/articles/72000602542)

---

### ⚙️ Parámetros configurables

- **BuyColor / SellColor**: Color del texto para variación positiva o negativa  
- **BackGroundBuyColor / BackGroundSellColor**: Color de fondo para variaciones positivas o negativas  
- **CalcType**: Precio de referencia (Apertura del día / Cierre del día anterior)  
- **ValType**: Tipo de valor mostrado (Porcentaje, Ticks, Diferencia de precio)  
- **Alignment**: Posición en pantalla (esquina superior/inferior izquierda/derecha)  
- **FontSize**: Tamaño de la fuente

---

### 🧭 Clasificación  
📂 Visualization — Indicadores de representación gráfica o etiquetas de contexto

---

### 🧠 Uso más frecuente

- Mostrar en pantalla la **variación diaria del precio** en tiempo real  
- Ver de forma rápida si el instrumento sube o baja respecto a la referencia  
- Confirmar visualmente cambios de fase en apertura, post-noticia o transición de sesión

---

### 📊 Nivel de relevancia  
🔟 **4.5 / 10**

✅ Visualmente útil como complemento informativo  
✅ Altamente configurable en color, posición y formato  
⛔ No aporta análisis técnico directo  
⛔ Su precisión depende de que esté cargado el día anterior completo

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de tendencia diaria**: solo tomar entradas a favor de la dirección diaria  
- **Filtro para reversión**: evitar trades contra la variación diaria fuerte  
- **Marcador de contexto**: ubicar trades cerca del +0.00 % para buscar rompimientos

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **CalcType**: `PreviousDayClose`  
- **ValType**: `Percent`  
- **FontSize**: `16`  
- **Alignment**: `BottomRight`  
- **BuyColor / SellColor**: Verde y rojo intensos  
- **BackGroundBuyColor / SellColor**: Gris oscuro o negro

✅ Aporta contexto diario sin ocupar espacio en el gráfico de precios  
✅ Compatible con estrategias intradía que requieren dirección de fondo

---

### 🧪 Notas de desarrollo

- Calcula la diferencia entre el precio actual y el precio de inicio definido por el usuario  
- El tipo de cambio mostrado puede ser en:
  - Porcentaje  
  - Ticks (según `Step` del instrumento)  
  - Precio absoluto  
- Usa `OnRender` para dibujar un recuadro con la información configurada en la zona elegida del gráfico  
- Detecta sesiones nuevas usando `IsNewSession(bar)` y guarda la primera vela para comparación

---

### ❗ Incoherencias o aspectos mejorables detectados

- Si `CalcType` es `PreviousDayClose`, pero **no se ha cargado el día anterior completo**, se muestra `"Previous day is not loaded"` permanentemente  
- No hay verificación robusta para evitar errores cuando `bar == 0` y `GetCandle(_lastSession - 1)` se ejecuta  
- No hay opción para ocultar dinámicamente si hay error en el cálculo

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar `0.00%` con transparencia si el día anterior no está cargado  
- Incluir modo “Auto” que cambie de `Open` a `Close` si falta información  
- Posibilidad de mostrar cambio desde la apertura y desde el cierre **al mismo tiempo**  
- Integrar líneas guía (por ejemplo, línea de apertura o cierre del día anterior)