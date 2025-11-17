## 🟦 Tape Patterns (9/10)  
**Nombre del archivo:** `TapePattern.cs`  
**Nombre del indicador:** Tape Patterns  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602248](https://help.atas.net/support/solutions/articles/72000602248)

---

### ⚙️ Parámetros configurables  
- **UseTimeFilter**: Activar el filtro de **tiempo** para incluir solo las transacciones dentro de un rango horario (por defecto: `false`)  
- **TimeFrom**: Hora de inicio del filtro de tiempo (por defecto: `00:00`)  
- **TimeTo**: Hora de finalización del filtro de tiempo (por defecto: `23:59`)  
- **CumulativeTrades**: Activar el uso de **comercio acumulado** para el cálculo (por defecto: `false`)  
- **MinVol**: Volumen mínimo de las transacciones a considerar (por defecto: `1`)  
- **MaxVol**: Volumen máximo de las transacciones a considerar (por defecto: `10000`)  
- **MinCount**: Número mínimo de transacciones a considerar (por defecto: `1`)  
- **MaxCount**: Número máximo de transacciones a considerar (por defecto: `100`)  
- **AlertFile**: Nombre del archivo de alerta a activar cuando se detecte una transacción relevante (por defecto: `alert1`)  
- **AlertSensitivity**: Sensibilidad de las alertas para la proximidad al nivel de la transacción (por defecto: `1`)

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Análisis de patrones en el flujo de transacciones

---

### 🧠 Uso más frecuente  
- Detectar **patrones de volumen** en el flujo de órdenes, como clusters de volumen significativos  
- Utilizar para analizar las **transacciones acumuladas** y obtener información sobre el sentimiento del mercado  
- Confirmar **rupturas de precios** basadas en grandes transacciones dentro de un rango específico

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  
✅ Ideal para **analizar flujos de órdenes** y detectar cambios significativos en el mercado  
✅ Útil para **identificar clusters de volumen** que preceden a movimientos fuertes de precio  
⛔ Puede no ser útil en **mercados con transacciones de bajo volumen o sin patrones claros**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Captura de grandes transacciones**: Detectar **cambios de precio significativos** cuando el volumen es anormalmente alto  
- **Confirmación de rupturas**: Observar **movimientos de precio** en momentos de alta concentración de volumen  
- **Identificación de manipulación de mercado**: Analizar patrones de **compras/vendas** para detectar **movimientos manipulativos**

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **MinVol**: `100`  
- **MaxVol**: `5000`  
- **MinCount**: `2`  
- **MaxCount**: `50`

✅ Ideal para detectar **movimientos de precio respaldados por grandes transacciones**  
✅ Funciona bien para **confirmar grandes rupturas de volumen**  
⛔ Menos útil en **mercados con bajo volumen o sin movimientos significativos**

---

### 🧪 Notas de desarrollo  
- El indicador analiza **patrones de transacciones** utilizando los datos de **volumen** y **precio**  
- Utiliza **transacciones acumuladas** para identificar movimientos significativos de volumen y precio  
- El cálculo y visualización se basan en las **transacciones dentro de un rango de tiempo y volumen específico**  
- El indicador genera **alertas automáticas** cuando el precio y el volumen alcanzan ciertos umbrales

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- La **configuración de las alertas** no es flexible, limitando las opciones de personalización  
- **Visualización limitada**: no permite personalizar completamente el estilo o los colores de las transacciones y patrones  
- No permite **ajustes dinámicos** en función de la volatilidad o de la actividad del mercado

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **alertas más personalizables** basadas en el volumen y el precio  
- Mejorar la **visualización** de los patrones y transacciones con más opciones de personalización (colores, formas, etc.)  
- Implementar **ajustes dinámicos** en los parámetros de análisis según el comportamiento del mercado en tiempo real
