## 🟦 VSA – WSD Histogram (8 / 10)  
**Nombre del archivo:** `VsaWsd.cs`  
**Nombre del indicador:** VSA – WSD Histogram  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602501](https://help.atas.net/support/solutions/articles/72000602501)

---

### ⚙️ Parámetros configurables  
- **Period**: Periodo para la media móvil exponencial del rango en ticks (por defecto: `100`)

---

### 🧭 Clasificación  
📂 Volume — Análisis clásico de vela y rango basado en estructura, sin order flow

---

### 🧠 Uso más frecuente  
- Visualizar de forma codificada **rango total**, **mecha superior**, **mecha inferior** y **volumen relativo**  
- Detectar patrones de vela de tipo VSA como **shakeouts, stopping volume, clímax o test**  
- Clasificar cada vela como señal de compra, venta o neutral según reglas de rango y dirección

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Útil para interpretar **estructura de la vela con lógica VSA**  
✅ Visualmente claro con histogramas codificados y puntos de señal  
⛔ No evalúa order flow ni clústeres directamente

---

### 🎯 Estrategias de scalping donde se aplica  
- **Entrada por test**: Detectar velas con mecha baja, cuerpo pequeño y punto verde  
- **Salida por clímax**: Observar contracción de rango con punto rojo tras gran vela expansiva  
- **Filtro de contexto**: Confirmar si hay disminución de intención (contracción de rango sin reversión)

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Period**: `100`  
- Usar en conjunto con volumen total o clúster para validar  
- Ajustar `TickSize` según activo si se desea alta sensibilidad

✅ Compatible con lógica de Wyckoff y detección visual rápida  
✅ Puede identificar zonas de reversión antes de que aparezcan otros indicadores  
⛔ No válido como única fuente para entradas sin validación externa

---

### 🧪 Notas de desarrollo  
- Calcula tres histogramas: **rango completo**, **mecha superior**, **mecha inferior**  
- Usa una **EMA** del volumen/rango en ticks para marcar la media  
- Asigna señales según:  
  - Contracción de rango  
  - Relación con apertura anterior  
  - Dirección relativa (cierre > apertura anterior = compra, etc.)

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No valida el valor de `TickSize` si es muy pequeño (puede exagerar escalado)  
- El criterio de señal solo considera el `Open` anterior, no el `Close` o rango total  
- No representa los puntos con etiquetas ni incluye explicación visual en el gráfico

---

### 🛠️ Propuestas de mejora  
- Añadir **tooltip explicativo** al pasar el cursor sobre puntos Buy/Sell/Neutral  
- Permitir lógica más sofisticada: volumen creciente + rango decreciente  
- Incluir opción de mostrar etiquetas con tipo de señal (“Clímax”, “Test”, “Neutral”, etc.)
