## 🟦 Current Price (4.5/10)

**Nombre del archivo:** `CurrentPrice.cs`  
**Nombre del indicador:** Current Price  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602361-current-price](https://help.atas.net/support/solutions/articles/72000602361-current-price)

---

### ⚙️ Parámetros configurables

- **Background**: Color de fondo del recuadro que muestra el precio actual (por defecto: azul)  
- **TextColor**: Color del texto (por defecto: azul claro)  
- **FontSize**: Tamaño de la fuente del precio (6–30)  
- **ShowTime**: Mostrar hora actual junto al precio (por defecto: activado)  
- **TimeFormat**: Formato horario (`HH:mm:ss`, etc.)

---

### 🧭 Clasificación  
📂 Visualization — Indicadores de representación gráfica o elementos de apoyo visual

---

### 🧠 Uso más frecuente

- Mostrar de forma visible el **último precio de cierre**  
- Facilitar la lectura rápida del valor actual sin mirar el eje derecho  
- Combinar con otros indicadores para destacar momentos clave con precio + hora

---

### 📊 Nivel de relevancia  
🔟 **4.5 / 10**

✅ Aporta valor visual como complemento  
✅ Útil para traders discrecionales que siguen precio en tiempo real  
⛔ No tiene lógica de cálculo técnico ni análisis de datos  
⛔ Puede superponerse a otros elementos en gráficos densos

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación visual rápida** de nivel actual de precio antes de entrar  
- **Visualización clara** del precio tras un evento (noticia, absorción, ruptura)  
- **Coordinación con plataformas externas** (ej. copy trading) si necesitas ver hora exacta del tick

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Background**: Azul oscuro o negro para no distraer  
- **TextColor**: Blanco o verde fosforito  
- **FontSize**: `18` o superior  
- **ShowTime**: `true`  
- **TimeFormat**: `HH:mm:ss`

✅ Mejora la reactividad y control visual en operaciones rápidas  
✅ Compatible con entornos multitarea o doble pantalla

---

### 🧪 Notas de desarrollo

- El valor mostrado es el **Close de la última vela visible en pantalla**  
- Se dibuja un rectángulo a la derecha del gráfico con el precio y la hora  
- Usa `OnRender()` para pintar directamente en el gráfico (modo `Final`)  
- `OnCalculate()` está vacío, ya que no realiza cálculos, solo visualización  
- La posición se alinea con la última barra visible y el valor del cierre

---

### ❗ Incoherencias o aspectos mejorables detectados

- La condición `if (LastVisibleBarNumber != CurrentBar - 1)` **puede ocultar el precio si se cambia el zoom o se hace scroll rápido**, aunque el gráfico esté actualizado correctamente  
- No hay control para evitar que el **recuadro del precio se superponga con otros indicadores** en el margen derecho  
- No contempla el caso de gráficos invertidos o escalas logarítmicas

---

### 🛠️ Propuestas de mejora

- Añadir opción para **mostrar el precio medio, apertura o máximo/mínimo** además del cierre  
- Permitir **mover manualmente la etiqueta** o anclarla a una posición fija  
- Incluir opción para **resaltar si el precio ha cambiado desde la última barra**  
- Añadir visibilidad condicional (ej. solo mostrar en horario de mercado activo)
