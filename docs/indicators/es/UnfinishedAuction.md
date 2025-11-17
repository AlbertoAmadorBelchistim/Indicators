## 🟦 Unfinished Auction (9/10)  
**Nombre del archivo:** `UnfinishedAuction.cs`  
**Nombre del indicador:** Unfinished Auction  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602495](https://help.atas.net/support/solutions/articles/72000602495)

---

### ⚙️ Parámetros configurables  
- **BidFilter**: Filtro mínimo de volumen Bid para validar una subasta inacabada (por defecto: `20`)  
- **AskFilter**: Filtro mínimo de volumen Ask para validar una subasta inacabada (por defecto: `20`)  
- **Days**: Días hacia atrás a considerar para buscar subastas inacabadas (por defecto: `20`)  
- **LineWidth**: Grosor de las líneas dibujadas (por defecto: `10`)  
- **LowLineColor / HighLineColor**: Color de las líneas de subasta inacabada para mínimo/máximo  
- **LowColor / HighColor**: Color de los marcadores de clúster para subasta inacabada  
- **UseAlerts**: Activar alertas al detectar o cerrar una subasta inacabada  
- **AlertFile**: Archivo de alerta a reproducir (por defecto: `"alert1"`)

---

### 🧭 Clasificación  
📂 VolumeOrderFlow — Subastas inacabadas por volumen agresivo en extremos de vela

---

### 🧠 Uso más frecuente  
- Identificar **subastas inacabadas** cuando el precio toca un extremo con desequilibrio de volumen  
- Detectar **niveles de posible reversión** o zonas que pueden actuar como **soporte/resistencia**  
- Confirmar contextos de **rechazo de precios** en máximos o mínimos no agresivamente defendidos

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  
✅ Clave para **confirmar rechazo o absorción** en extremos del rango  
✅ Permite detectar **niveles técnicos potentes** donde no hubo liquidez suficiente  
⛔ Requiere gráficos con clústeres y **no es útil en velas sin extremos claros**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Reversión en extremos**: Buscar subasta inacabada en el high/low como señal anticipada de giro  
- **Ruptura fallida**: Confirmar falsos breakouts si hay subasta inacabada y luego recuperación  
- **Retesteo y entrada**: Esperar que el precio vuelva a tocar la línea hasta que se "toque" (touch), luego entrar

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **BidFilter**: `30`  
- **AskFilter**: `30`  
- **Days**: `20`  
- **LineWidth**: `8`  
- **UseAlerts**: `true`  
- **LowLineColor / HighLineColor**: `Blue` / `Red`  
- **LowColor / HighColor**: `Aqua` / `Crimson`

✅ Muestra **líneas claras y persistentes** hasta que el precio las toca  
✅ Excelente para **confirmar zonas clave** antes de entrada agresiva  
⛔ Puede generar muchas líneas si se usan filtros demasiado bajos

---

### 🧪 Notas de desarrollo  
- Dibuja líneas horizontales hasta que el precio las "toca" (`LineTillTouch`)  
- Usa datos de clúster (`GetPriceVolumeInfo`) para validar subasta inacabada en extremos  
- Muestra **clusters visuales** con color personalizado en el precio afectado  
- Limpia líneas obsoletas y actualiza estado dinámicamente en cada vela

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No permite visualizar **historial de subastas inacabadas cerradas**  
- No incluye **filtros por sesión u horario**, lo que puede ensuciar gráficos intradía  
- No permite ajustar el tipo de visualización de los clústeres (tamaño, forma)

---

### 🛠️ Propuestas de mejora  
- Añadir opción para **guardar y mostrar historial** de subastas inacabadas tocadas  
- Permitir filtrado adicional por **hora o sesión activa**  
- Incluir más opciones visuales como **etiquetas con precio, tipo y volumen total**
