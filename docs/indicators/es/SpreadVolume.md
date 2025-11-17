## 🟦 Spread Volume (7/10)  
**Nombre del archivo:** `SpreadVolume.cs`  
**Nombre del indicador:** Spread Volume  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602630](https://help.atas.net/support/solutions/articles/72000602630)

---

### ⚙️ Parámetros configurables  
- **BuyColor**: Color para el volumen de compra (por defecto: `Green`)  
- **SellColor**: Color para el volumen de venta (por defecto: `Red`)  
- **TextColor**: Color del texto de los valores (por defecto: `Black`)  
- **Spacing**: Espaciado entre las barras de volumen (por defecto: `10`)  
- **Offset**: Desplazamiento horizontal de las barras (por defecto: `1`)  
- **Width**: Ancho de las barras de volumen (por defecto: `20`)

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Volumen de compra y venta en el spread

---

### 🧠 Uso más frecuente  
- Visualizar el **volumen de compra y venta** en el spread entre el bid y el ask  
- Analizar la **presión de compra/venta** a través de la relación de volúmenes  
- Representar el volumen de **operaciones ejecutadas** en los precios de bid y ask

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**  
✅ Ideal para ver la **distribución de volúmenes** en cada nivel de precios  
✅ Buena visualización de **presión compradora y vendedora**  
⛔ Requiere interpretación manual para análisis de señales directas

---

### 🎯 Estrategias de scalping donde se aplica  
- **Presión de compra/venta**: Confirmar si el volumen de compra o venta está dominando el spread  
- **Ruptura de niveles clave**: Usar el indicador para observar la acumulación de volumen en niveles técnicos  
- **Lectura de flujo de órdenes**: Detectar agresión de compradores o vendedores a través de los volúmenes en el spread

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **BuyColor**: `Green`  
- **SellColor**: `Red`  
- **Spacing**: `10`  
- **Offset**: `1`  
- **Width**: `20`

✅ Excelente para captar **volumen en el spread** y detectar desequilibrios  
✅ Ideal para análisis de **presión de mercado en tiempo real**  
⛔ No es adecuado para mercados con bajo volumen o sin tendencias claras

---

### 🧪 Notas de desarrollo  
- Muestra **barras de volumen** para cada transacción en los niveles de precio bid y ask  
- El volumen se representa en **colores personalizados** (compra/venta) y se ajusta visualmente con el **espaciado y el ancho de las barras**  
- El cálculo de volúmenes se basa en los datos de **CumulativeTrade** y se actualiza en tiempo real

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No se valida si el valor de `Volume` es cero, lo que puede resultar en barras vacías no deseadas  
- No ofrece opciones para **alertas basadas en volúmenes** o cambios de presión de mercado  
- El **cálculo de volúmenes** puede no reflejar adecuadamente las operaciones en mercados con alta dispersión de precios  
- La visualización podría mejorarse con **más configuraciones de personalización** de las barras

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **alertas automáticas** cuando el volumen de compra o venta supere ciertos umbrales  
- Mejorar la **precisión de visualización** ajustando dinámicamente el **ancho y la altura de las barras**  
- Ofrecer la opción de **colorear las barras** según otras condiciones de mercado (p. ej. acumulación de volumen o delta)

