## 🟦 Open Interest (8/10)

**Nombre del archivo:** `OpenInterest.cs`  
**Nombre del indicador:** Open Interest  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602439](https://help.atas.net/support/solutions/articles/72000602439)

---

### ⚙️ Parámetros configurables

- **Mode**: Modo de cálculo (`ByBar`, `Session`, `Cumulative`)  
- **MinimizedMode**: Mostrar en modo minimizado (solo parte superior del histograma)  
- **Filter**: Umbral mínimo de cambio para mostrar vela  
- **FilterColor**: Color de las velas filtradas  
- **UseAlerts / AlertFile / ChangeSize**: Activar alertas y sensibilidad  
- **AlertForeColor / AlertBGColor**: Colores del texto y fondo de la alerta

---

### 🧭 Clasificación
📂 Volume — Indicador clásico de interés abierto por vela o sesión

---

### 🧠 Uso más frecuente

- Visualizar el **cambio de posiciones abiertas** por barra o sesión  
- Confirmar **acumulación o distribución**  
- Generar **alertas ante cambios relevantes en el OI**

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Muestra con claridad el comportamiento del interés abierto  
✅ Múltiples modos de análisis: por barra, sesión o acumulado  
⛔ Requiere un activo con datos fiables de OI y buen filtrado

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de ruptura real** si el OI aumenta tras superar nivel  
- **Filtro de trampa** si el precio sube pero el OI cae → posible cierre de posiciones  
- **Alertas para scalping rápido** ante picos de OI no habituales

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Mode**: `ByBar`  
- **MinimizedMode**: `true`  
- **Filter**: `0` o ajustado al ruido típico  
- **UseAlerts**: `true`  
- **ChangeSize**: `50` (ajustable según contrato)

✅ Útil como filtro adicional de contexto institucional  
✅ Compatible con análisis de absorción, presión o rotura  
⛔ Depende fuertemente de la calidad de los datos de OI

---

### 🧪 Notas de desarrollo

- El cálculo se adapta según el modo elegido (`ByBar`, `Session`, `Cumulative`)  
- Las velas se pintan solo si el valor de `OI` cambia o cumple con el filtro  
- Se colorea una segunda serie (`_filterSeries`) solo si el cambio supera el umbral  
- Las alertas se lanzan si el cambio supera `ChangeSize` en la última barra  
- Compatible con modo minimizado para mostrar solo parte positiva

---

### ❗ Incoherencias o aspectos mejorables detectadas

- En `bar == 0`, si el OI es cero y el modo es `ByBar`, la vela se omite completamente  
- No se valida que `ChangeSize` sea coherente con el filtro visual → puede lanzar alerta aunque no se vea vela  
- El valor por defecto de `MinimizedMode` puede ocultar parte de la señal si no se entiende su funcionamiento  
- No permite mostrar `OI` como línea continua (solo velas)  
- La condición de alerta depende de `this[bar]`, que puede estar oculta por filtro o minimización

---

### 🛠️ Propuestas de mejora

- Mostrar una línea auxiliar de OI acumulado si se desea  
- Incluir leyenda visual que indique el modo de cálculo activo  
- Permitir representar el `OI` como histograma o línea según preferencia  
- Añadir codificación de color dinámica según crecimiento o decrecimiento  
- Validar coherencia entre alerta y visibilidad de la vela (evitar señales ocultas)

