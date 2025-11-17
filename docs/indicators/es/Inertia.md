## 🟦 Inertia (6.5/10)

**Nombre del archivo:** `Inertia.cs`  
**Nombre del indicador:** Inertia  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602555](https://help.atas.net/support/solutions/articles/72000602555)

---

### ⚙️ Parámetros configurables

- **RviPeriod**: Periodo del indicador RVI subyacente (por defecto: 10)  
- **LinearRegPeriod**: Periodo de la regresión lineal aplicada al RVI (por defecto: 14)

---

### 🧭 Clasificación  
📂 Momentum — Suavizado del impulso mediante regresión sobre oscilador RVI

---

### 🧠 Uso más frecuente

- Medir la **persistencia del impulso** en una dirección mediante suavizado del RVI  
- Confirmar continuidad de tendencia o inicio de fatiga  
- Filtrar señales erráticas de impulso en fases laterales

---

### 📊 Nivel de relevancia  
🔟 **6.5 / 10**

✅ Proporciona una lectura clara del momentum suavizado  
✅ Filtra ruido de corto plazo gracias a la regresión lineal  
⛔ Puede tener retardo en fases de giro rápido  
⛔ No representa niveles de sobrecompra/sobreventa por defecto

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de ruptura** si Inertia está creciente y sostenida  
- **Entrada direccional**: operar a favor del sesgo si el valor acelera  
- **Evitar operaciones si Inertia es plana**: posible lateralidad o transición

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **RviPeriod**: `10`  
- **LinearRegPeriod**: `14`  
- Dibujar en panel separado  
- Combinar con volumen/delta para confirmar la dirección

✅ Evita entradas en falso por ruido momentáneo  
✅ Compatible con estructuras direccionales limpias

---

### 🧪 Notas de desarrollo

- El valor base es el **Relative Vigor Index (RVI)** aplicado sobre el precio  
- Luego se aplica una **regresión lineal móvil** al RVI:
  \[
  \text{Inertia}_t = \text{LinearRegression}(\text{RVI}_t)
  \]
- Se usan dos objetos internos:
  - `_rvi`: instancia de `RVI2`, configurado con `RviPeriod`  
  - `_linReg`: instancia de `LinearReg`, configurado con `LinearRegPeriod`  
- La salida se guarda en `_renderSeries`, visible en nuevo panel

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se expone el valor del RVI como serie secundaria, lo que impide comparar directamente ambas curvas  
- La regresión lineal se calcula sobre una serie ya suavizada, lo cual puede ocultar señales tempranas  
- No permite seleccionar el tipo de fuente para el RVI (siempre basado en cierre implícitamente)

---

### 🛠️ Propuestas de mejora

- Exponer la serie del RVI original como opción visual adicional  
- Añadir alertas si la pendiente de Inertia cambia de signo  
- Incluir líneas guía opcionales para marcar niveles neutros o extremos  
- Permitir aplicar otros suavizados (EMA, WMA) en lugar de regresión lineal para comparación
