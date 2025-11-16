## 🟦 Delta Turnaround (7.5/10)

**Nombre del archivo:** `DeltaTurnaround.cs`  
**Nombre del indicador:** Delta Turnaround  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602364](https://help.atas.net/support/solutions/articles/72000602364)

---

### ⚙️ Parámetros configurables

- **UseAlerts**: Activar alertas automáticas  
- **AlertOnNewCandle**: Lanzar alerta al abrir nueva vela si hubo señal en la anterior  
- **AlertFile**: Archivo de sonido para la alerta  
- **AlertBGColor / AlertForeColor**: Colores de fondo y texto de alerta

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Señales de giro basadas en delta y estructura

---

### 🧠 Uso más frecuente

- Detectar patrones de **reversión en el delta** tras dos velas fuertes en dirección contraria  
- Visualizar señales de giro con confirmación por delta negativo (venta) o positivo (compra)  
- Lanzar alertas cuando se forma el patrón completo de vuelta delta  
- Combinar con otros indicadores para entradas tácticas o detección de absorciones

---

### 📊 Nivel de relevancia  
🔟 **7.5 / 10**

✅ Patrón claro de estructura + delta, fácil de validar  
✅ Visualización simple con flechas (up/down arrows)  
⛔ No configurable: el patrón es fijo y no admite variantes  
⛔ Puede lanzar señales falsas si la tercera vela es pequeña o de consolidación

---

### 🎯 Estrategias de scalping donde se aplica

- **Vuelta bajista** (flecha roja): dos velas alcistas seguidas de una bajista con delta negativo  
- **Vuelta alcista** (flecha verde): dos velas bajistas seguidas de una alcista con delta positivo  
- **Confirmación**: se puede usar como trigger tras ruptura fallida o spike de agresión  
- **Entrada contraria al movimiento previo** cuando se forma la tercera vela del patrón

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **UseAlerts**: `true`  
- **AlertOnNewCandle**: `true`  
- **AlertFile**: `"alert1"`  
- **Colores**: rojo para ventas, verde para compras  
- **Visibilidad**: `UpArrow` y `DownArrow` activados

✅ Permite reacción rápida con señales visuales y sonoras  
✅ Compatible con lógica de ruptura/absorción en extremos

---

### 🧪 Notas de desarrollo

- Detecta dos patrones:
  - **Giro bajista**:
    - Dos velas alcistas seguidas de una bajista  
    - La tercera vela debe cerrar por debajo de su apertura  
    - Su máximo debe ser igual o mayor que el de la vela previa  
    - Su delta debe ser negativo
  - **Giro alcista**:
    - Dos velas bajistas seguidas de una alcista  
    - El mínimo debe ser igual o inferior al de la vela anterior  
    - Delta debe ser positivo
- En ambos casos se dibuja una flecha (`VisualMode.UpArrow` o `DownArrow`)  
- Si `AlertOnNewCandle` está activo, se lanza alerta al abrir nueva vela si hubo señal previa  
- Si `UseAlerts` está activo, lanza alerta en la misma barra cuando se detecta la señal

---

### ❗ Incoherencias o aspectos mejorables detectados

- El texto de alerta es **el mismo para señales alcistas y bajistas**:  
  `"Delta turnaround down signal."` → debería diferenciarse entre "up signal" y "down signal"  
- No hay parámetros para modificar la **cantidad de velas previas necesarias** ni umbrales de delta  
- La señal se borra si el patrón se rompe en tiempo real (no queda marcado si luego cambia)  
- No permite filtrar por tamaño de vela o volumen, lo cual puede generar ruido en laterales

---

### 🛠️ Propuestas de mejora

- Diferenciar alertas de compra y venta con textos y sonidos distintos  
- Añadir opción para requerir delta mínimo en la tercera vela  
- Permitir definir cuántas velas previas considerar (ej. 3 en vez de 2)  
- Incluir lógica para dejar la señal fija una vez confirmada  
- Añadir etiquetas con el valor del delta que generó la señal