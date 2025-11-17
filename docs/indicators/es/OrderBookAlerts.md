## 🟦 Order Book Alerts (9/10)

**Nombre del archivo:** `OrderBookAlerts.cs`  
**Nombre del indicador:** Order Book Alerts  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000619055](https://help.atas.net/support/solutions/articles/72000619055)

---

### ⚙️ Parámetros configurables

- **Filter**: Volumen mínimo requerido para generar alerta (por defecto: 100)  
- **TimeFilter**: Tiempo mínimo en segundos que el nivel debe mantenerse (opcional)  
- **POMode**: Modo de offset desde el precio actual (`Percent` o `Ticks`)  
- **PriceOffset**: Desplazamiento desde el último precio (en %) o en ticks  
- **UseAlerts**: Activar alertas sonoras y visuales  
- **AlertFile**: Archivo de sonido a reproducir  
- **AlertForeColor / AlertBGColor**: Colores de texto y fondo de la alerta  
- **ShowOnChart**: Mostrar los niveles detectados sobre el gráfico  
- **CoolDownPeriod**: Tiempo mínimo entre alertas para un mismo nivel (segundos)

---

### 🧭 Clasificación
📂 OrderBook — Alertas dinámicas por profundidad de mercado (DOM)

---

### 🧠 Uso más frecuente

- Detectar **niveles relevantes en el libro de órdenes (DOM)**  
- Activar alertas al aparecer **volumen significativo cerca del precio**  
- Confirmar zonas de **absorción, spoofing o presión institucional**

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Alertas dinámicas por aparición de volumen fuera de lo común  
✅ Visualización directa y configurable en el gráfico  
⛔ Requiere buena calibración de filtros para evitar exceso de ruido

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada por aparición de muro de órdenes** en zona de interés  
- **Confirmación de soporte/resistencia** con alerta visual + volumen  
- **Evitar trades en zonas con alta presión pasiva detectada**

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Filter**: `150`  
- **POMode**: `Ticks`  
- **PriceOffset**: `10`  
- **TimeFilter.Enabled**: `true`, `Value`: `1`  
- **CoolDownPeriod**: `3`  
- **ShowOnChart**: `true`

✅ Detecta y resalta zonas activas del DOM en tiempo real  
✅ Compatible con estrategias basadas en spoofing o absorciones  
⛔ No detecta intención real, solo presencia estática de volumen

---

### 🧪 Notas de desarrollo

- Evalúa el DOM cada vez que cambia mediante `MarketDepthChanged()`  
- Filtra precios dentro de un rango dinámico alrededor del último `Close`  
- Crea objetos `PriceInfo` con control de tiempo, volumen, alertado y visibilidad  
- Si `UseAlerts` está activo, lanza alerta sonora y visual si se cumple el `CoolDown`  
- Dibuja rectángulos horizontales sobre el gráfico si `ShowOnChart` está activado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No hay validación de coherencia entre `POMode` y `PriceOffset` (pueden resultar en rango inapropiado)  
- El volumen detectado no distingue entre lado bid/ask → limita interpretación en ciertas estrategias  
- No se elimina automáticamente un `PriceInfo` si desaparece del DOM (solo desactiva `IsActive`)  
- No hay opción para aplicar un rango de alerta diferente por lado (bid vs ask)  
- No se muestra etiqueta con precio/volumen sobre el rectángulo dibujado

---

### 🛠️ Propuestas de mejora

- Añadir opción para diferenciar entre órdenes de compra y venta en DOM  
- Mostrar etiquetas flotantes con `precio` y `volumen` sobre los rectángulos  
- Permitir definir rangos de `PriceOffset` distintos para arriba y abajo  
- Añadir alertas visuales con gradación según intensidad del volumen  
- Incorporar filtro de persistencia: no solo volumen, sino cuánto tiempo permanece

