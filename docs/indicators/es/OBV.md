## 🟦 OBV (On Balance Volume) (7/10)

**Nombre del archivo:** `OBV.cs`  
**Nombre del indicador:** OBV  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602436](https://help.atas.net/support/solutions/articles/72000602436)

---

### ⚙️ Parámetros configurables

- **MinimizedMode (Enabled / Period)**: Activar modo de ventana móvil (por defecto: desactivado, periodo 10)

---

### 🧭 Clasificación
📂 Volume — Indicador clásico de volumen acumulado según dirección del precio

---

### 🧠 Uso más frecuente

- Detectar **acumulación o distribución** según el volumen que acompaña al movimiento  
- Identificar **divergencias** entre precio y volumen  
- Medir la **presión compradora o vendedora acumulada**

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Simple y eficaz como indicador de volumen tendencial  
✅ Compatible con múltiples marcos temporales y estrategias  
⛔ Puede ser redundante si ya se usa delta acumulado o CVD

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de ruptura** si el OBV acompaña con fuerza  
- **Divergencia**: precio hace nuevo mínimo pero OBV no → posible giro  
- **Filtro de dirección**: operar solo a favor del sesgo acumulado del OBV

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **MinimizedMode**: `Enabled = true`, `Period = 10`

✅ El modo minimizado filtra ruido en ventanas cortas  
✅ Reacciona rápidamente a cambios de volumen relativo  
⛔ En consolidaciones puede dar señales falsas si se usa sin filtro de contexto

---

### 🧪 Notas de desarrollo

- Calcula la diferencia de cierres entre barras consecutivas  
- Acumula el volumen total si el cierre sube, lo resta si baja, lo mantiene si es igual  
- En modo `MinimizedMode`, usa una ventana deslizante para suavizar la señal  
- Utiliza una serie auxiliar `_volSignedSeries` para guardar el volumen con signo  
- Compatible con modo minimizado en `ValueDataSeries` (`UseMinimizedModeIfEnabled = true`)

---

### ❗ Incoherencias o aspectos mejorables detectadas

- En `bar == 0`, la línea `this[bar] = 0;` **nunca se ejecuta** porque se evalúa antes como `return` → inicialización nula  
- No hay validación para evitar valores negativos de volumen (si se introducen mal por fuente externa)  
- No se permite visualizar la serie de volumen con signo (`_volSignedSeries`), lo que limita el análisis comparado  
- En `MinimizedMode`, no se controla si el periodo solicitado es mayor que las barras cargadas  
- No permite elegir si se acumula sobre el `Close`, `Typical`, etc.

---

### 🛠️ Propuestas de mejora

- Corregir la inicialización en `bar == 0` para que realmente se ejecute `this[bar] = 0;`  
- Añadir validación para asegurar que `MinimizedMode.Value <= CurrentBar`  
- Permitir al usuario visualizar la serie de volumen con signo como histograma  
- Añadir alertas si el OBV cruza ciertos niveles definidos por el usuario  
- Ofrecer opción para cambiar la fuente de comparación (ej: usar `Open` o `Typical`)

