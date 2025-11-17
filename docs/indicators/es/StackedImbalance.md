## 🟦 Stacked Imbalance (8/10)  
**Nombre del archivo:** `StackedImbalance.cs`  
**Nombre del indicador:** Stacked Imbalance  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602474](https://help.atas.net/support/solutions/articles/72000602474)

---

### ⚙️ Parámetros configurables  
- **IgnoreZeroValues**: Ignorar valores de volumen cero en el cálculo de desequilibrio (por defecto: `false`)  
- **ImbalanceRatio**: Relación mínima de desequilibrio entre las cantidades de Bid/Ask (por defecto: `300`)  
- **ImbalanceRange**: Rango mínimo de desequilibrio a considerar (por defecto: `3`)  
- **ImbalanceVolume**: Volumen mínimo para que un desequilibrio sea considerado significativo (por defecto: `30`)  
- **Days**: Número de días para retroceder en el cálculo (por defecto: `20`)  
- **TillTouch**: Determina si dibujar las líneas de desequilibrio hasta que se toquen (por defecto: `false`)  
- **AskBidImbalanceColor**: Color para el desequilibrio Bid/Ask (por defecto: `Green`)  
- **BidAskImbalanceColor**: Color para el desequilibrio Ask/Bid (por defecto: `DarkRed`)  
- **LineWidth**: Ancho de las líneas de desequilibrio (por defecto: `10`)  
- **DrawBarsLength**: Longitud de las barras de desequilibrio (por defecto: `10`)

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Indicador de desequilibrio acumulado entre Bid y Ask

---

### 🧠 Uso más frecuente  
- Visualizar **desequilibrios entre Bid y Ask** a lo largo de un periodo de tiempo  
- Analizar **zonas de alta agresión** entre compradores y vendedores  
- Identificar niveles **claves de soporte y resistencia** basados en desequilibrios de volumen

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Ideal para identificar **puntos de alta presión de compra/venta**  
✅ Útil para observar **desequilibrios prolongados**  
⛔ Requiere interpretación manual para determinar señales de entrada claras

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de ruptura**: Observar desequilibrios de volumen en puntos de ruptura clave  
- **Entrada en desequilibrio**: Buscar entradas cuando el desequilibrio sea significativo y sostenido  
- **Detección de manipulación de mercado**: Identificar grandes **fluctuaciones de volumen** que preceden a cambios de precio

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **ImbalanceRatio**: `300`  
- **ImbalanceRange**: `3`  
- **ImbalanceVolume**: `30`  
- **LineWidth**: `10`  
- **DrawBarsLength**: `10`

✅ Detecta rápidamente **zonas de desequilibrio** entre compradores y vendedores  
✅ Ideal para **confirmar movimientos** a corto plazo  
⛔ Menos efectivo en mercados con **bajo volumen o baja actividad**

---

### 🧪 Notas de desarrollo  
- Mide el **desequilibrio entre Bid y Ask** utilizando el volumen acumulado  
- Las señales de desequilibrio se representan con **líneas de diferentes colores**  
- El indicador se basa en el análisis de **volúmenes de compra/venta** y calcula el desequilibrio mediante ratios  
- El indicador dibuja **líneas horizontales** para mostrar los niveles de desequilibrio y su evolución

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No se valida correctamente si el volumen de **Bid o Ask** es cero antes de calcular el desequilibrio  
- No permite **ajustar visualmente** las líneas de desequilibrio (colores, grosor, estilo)  
- La **configuración de días** no se refleja completamente en los cálculos, lo que puede resultar en resultados inexactos en ciertos mercados  
- Las alertas no son configurables más allá de los valores de `Price`

---

### 🛠️ Propuestas de mejora  
- Añadir **alertas automáticas** cuando el desequilibrio alcance umbrales específicos  
- Permitir **personalización avanzada** de la visualización de las líneas (grosor, color, tipo)  
- Mejorar el sistema de **gestión de valores nulos o cero** para asegurar mayor precisión en el cálculo  
- Implementar una **validación más robusta** de la configuración de días para retroceder correctamente en el cálculo de desequilibrio