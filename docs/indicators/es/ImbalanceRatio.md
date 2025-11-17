## 🟦 Imbalance Ratio (9/10)

**Nombre del archivo:** `ImbalanceRatio.cs`  
**Nombre del indicador:** Imbalance Ratio  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602404](https://help.atas.net/support/solutions/articles/72000602404)

---

### ⚙️ Parámetros configurables

- **Ratio**: Relación mínima Bid/Ask para considerar un desequilibrio (por defecto: 4)  
- **VolumeFilter**: Volumen mínimo en el par de niveles para considerar el desequilibrio  
- **Transparency**: Transparencia del clúster visualizado (0 a 100)  
- **BuyColor / SellColor**: Colores para desequilibrios de compra o venta  
- **TextColor**: Color del texto que muestra la cantidad de desequilibrios por vela  
- **ShowTop / ShowBot**: Mostrar desequilibrios solo por encima o debajo según el delta

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Detección de desequilibrios agresivos entre Bid y Ask por nivel

---

### 🧠 Uso más frecuente

- Detectar **zonas de desequilibrio agresivo** en el clúster  
- Visualizar acumulaciones de compras o ventas dominantes  
- Usar como filtro o confirmación de **absorciones, agotamientos o empuje institucional**

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**

✅ Ofrece visualización precisa de desequilibrios por nivel de precio  
✅ Permite ajustar sensibilidad y volumen mínimo para evitar ruido  
⛔ Solo representa desequilibrios instantáneos, no acumulados  
⛔ Requiere comprensión de clúster y estructura para su correcta interpretación

---

### 🎯 Estrategias de scalping donde se aplica

- **Ruptura con desequilibrio agresivo**: entrada si aparecen desequilibrios en dirección de ruptura  
- **Absorción inversa**: entrada contraria si aparece desequilibrio justo en zona clave  
- **Empuje institucional**: desequilibrios agrupados indican presión de grandes órdenes

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Ratio**: `4` o `5`  
- **VolumeFilter**: `300` a `800`  
- **Transparency**: `50`  
- **ShowTop / ShowBot**: `true`  
- Colores: azul para compras, rojo para ventas

✅ Compatible con DOM Strength, CVD, Delta  
✅ Ideal como confirmación visual de presión agresiva

---

### 🧪 Notas de desarrollo

- Recorre todos los niveles de precio de la vela actual y compara los valores Bid/Ask entre ticks consecutivos:  
  - Si `(Ask_upper / Bid_lower) >= Ratio` y volumen >= filtro → señal de compra  
  - Si `(Bid_lower / Ask_upper) >= Ratio` y volumen >= filtro → señal de venta  
- Los desequilibrios se representan con `PriceSelectionValue` sobre el clúster (solo visual, no gráfico)  
- En `OnRender`, muestra la cantidad de desequilibrios por vela (`buy x sell`) como texto  
- Permite controlar si se muestran los desequilibrios arriba (ShowTop) o abajo (ShowBot) según el signo del delta

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El cálculo se basa en volumen bid/ask por nivel, pero **no distingue entre agresión real o pasiva**  
- El uso de `PriceSelectionValue` es visual pero no almacena datos accesibles para lógica externa  
- No hay opción para exportar ni usar los desequilibrios como señal numérica o condición lógica

---

### 🛠️ Propuestas de mejora

- Añadir opción para exponer el número de desequilibrios por vela como `ValueDataSeries`  
- Incluir lógica de alerta visual/sonora si supera cierto número de desequilibrios por barra  
- Permitir mostrar desequilibrios acumulados por clúster y no sólo instantáneos  
- Añadir filtro direccional (mostrar solo desequilibrios en favor del delta total)
