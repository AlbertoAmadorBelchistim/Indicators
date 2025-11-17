## 🟦 McClellan Oscillator (6/10)

**Nombre del archivo:** `McClellanOscillator.cs`  
**Nombre del indicador:** McClellan Oscillator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602558](https://help.atas.net/support/solutions/articles/72000602558)

---

### ⚙️ Parámetros configurables

- **ShortPeriod**: Periodo de la media exponencial corta (por defecto: 19)  
- **LongPeriod**: Periodo de la media exponencial larga (por defecto: 39)

---

### 🧭 Clasificación
📂 Momentum — Oscilador basado en diferencia de medias exponenciales

---

### 🧠 Uso más frecuente

- Medir el **impulso del mercado** comparando dos EMAs  
- Detectar **cambios en la dirección de la tendencia**  
- Identificar condiciones de sobrecompra o sobreventa de forma dinámica

---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Útil como oscilador de impulso suavizado  
✅ Proporciona señales de giro cuando cruza la línea cero  
⛔ No muestra divergencias ni tiene señal visual de sobrecompra/sobreventa

---

### 🎯 Estrategias de scalping donde se aplica

- **Cruce con cero**: señal de cambio de dirección  
- **Confirmación de giro** cuando el valor cambia de pendiente con fuerza  
- **Filtro de tendencia**: operar solo si el oscilador está en zona positiva o negativa

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **ShortPeriod**: `10`  
- **LongPeriod**: `21`

✅ Aumenta la sensibilidad para gráficos rápidos  
✅ Responde bien a cambios de corto plazo en 1M  
⛔ Puede requerir filtro adicional para evitar ruido

---

### 🧪 Notas de desarrollo

- Calcula la diferencia entre dos EMAs (EMA corta − EMA larga)  
- El resultado se guarda en una sola serie `RenderSeries`, tipo línea  
- Representa el valor en un panel separado (`NewPanel`)  
- Usado históricamente como componente del McClellan Summation Index

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se valida si `ShortPeriod > LongPeriod`, lo cual podría invertir la lógica esperada  
- No incluye línea cero visual, a pesar de ser el punto clave de cruce  
- No hay opción para mostrar alertas por cruce de niveles relevantes  
- No se permite elegir tipo de media (solo `EMA`)  
- No incluye ninguna codificación de color por pendiente o zona

---

### 🛠️ Propuestas de mejora

- Añadir validación o advertencia si `ShortPeriod > LongPeriod`  
- Incluir línea cero visible en el panel para ayudar en la interpretación  
- Añadir alertas visuales o sonoras al cruce con cero o cambios de pendiente  
- Permitir colorear la línea según dirección (verde si sube, rojo si baja)  
- Añadir opción para cambiar el tipo de media (EMA, SMA, etc.)

