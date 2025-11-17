## 🟦 Vertical Grid (3.5/10)  
**Nombre del archivo:** `VerticalGrid.cs`  
**Nombre del indicador:** Vertical Grid  

---

### ⚙️ Parámetros configurables  
- **Period**: Intervalo entre líneas verticales (por defecto: `10`)  
- **PeriodType**: Unidad de periodo (`Seconds`, `Minutes`, `Hours`, `Days`)  
- **GridPen**: Color y estilo de la línea vertical  
- **TextColor**: Color del texto horario mostrado en la parte inferior

---

### 🧭 Clasificación  
📂 Visualization — Rejilla vertical temporal en el gráfico

---

### 🧠 Uso más frecuente  
- Dividir visualmente el gráfico por **intervalos temporales constantes**  
- Añadir **referencias visuales** para análisis cronológico de acción del precio  
- Facilitar la **lectura y alineación de eventos por hora, día o minuto**

---

### 📊 Nivel de relevancia  
🔟 **3.5 / 10**  
✅ Útil como **referencia visual complementaria**  
✅ Flexible para adaptarse a distintos marcos temporales  
⛔ No aporta señales de trading por sí mismo

---

### 🎯 Estrategias de scalping donde se aplica  
- **Segmentación temporal**: Ayuda a dividir el gráfico en bloques de análisis (ej. cada 15 minutos en apertura)  
- **Evaluación por tramos**: Comparar comportamiento entre distintas franjas horarias  
- **Revisión de eventos**: Localizar con precisión la hora de publicación de datos, rupturas o absorciones

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Period**: `15`  
- **PeriodType**: `Minutes`  
- **GridPen**: `Gray, Dash, 1px`  
- **TextColor**: `Black`

✅ Ideal para marcar tramos clave (pre-market, apertura, media sesión…)  
✅ Muy útil para organizar el análisis visual intradía  
⛔ Requiere calibración manual según el marco temporal y nivel de zoom

---

### 🧪 Notas de desarrollo  
- Dibuja líneas verticales y etiquetas horarias cada cierto intervalo de tiempo definido  
- El formato de la etiqueta depende del timeframe activo (`HH:mm`, `dd MMM`, etc.)  
- Las etiquetas se adaptan para no solaparse en pantalla y se alinean automáticamente

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No detecta ni se adapta automáticamente a **cambios de sesión o eventos relevantes**  
- Puede **solaparse** con otras herramientas visuales o indicadores  
- No permite configurar condiciones para **mostrar/ocultar etiquetas** según el zoom

---

### 🛠️ Propuestas de mejora  
- Añadir opción de **alineación con sesiones de mercado o eventos programados**  
- Permitir **mostrar u ocultar etiquetas** dependiendo del zoom o densidad del gráfico  
- Ofrecer integración con otros indicadores para **sincronizar puntos clave**
