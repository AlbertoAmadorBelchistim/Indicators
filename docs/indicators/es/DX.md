## 🟦 DX (Directional Index) (7/10)

**Nombre del archivo:** `DX.cs`  
**Nombre del indicador:** DX  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000606735-dx-indicator](https://help.atas.net/support/solutions/articles/72000606735-dx-indicator)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras para el cálculo de DI+ y DI- (por defecto: 10)

---

### 🧭 Clasificación  
📂 Trend — Indicadores de fuerza direccional sin sesgo de dirección

---

### 🧠 Uso más frecuente

- Medir la **fuerza relativa del movimiento direccional**, sin distinguir si es alcista o bajista  
- Evaluar si el mercado presenta **tendencia clara o comportamiento lateral**  
- Componente básico del **ADX**, aunque en este caso se representa directamente el DX

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**

✅ Permite cuantificar si el movimiento tiene dirección dominante  
✅ Compatible con estrategias de tendencia o rango  
⛔ No indica si la dirección es alcista o bajista  
⛔ Puede resultar confuso si no se combina con DI+ y DI-

---

### 🎯 Estrategias de scalping donde se aplica

- **Confirmación de fuerza direccional**: operar solo si DX > 20 o 25  
- **Evitar operar en consolidación**: si DX está plano o por debajo de cierto umbral  
- **Filtrar señales**: aceptar setups solo si el mercado muestra direccionalidad activa

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `10`  
- Usar línea horizontal auxiliar en `20` o `25` como umbral  
- Complementar con indicadores de dirección (DI+, DI-, CVD)

✅ Ideal para validar si hay contexto tendencial  
✅ Compatible con setups reactivos o seguimiento de impulso

---

### 🧪 Notas de desarrollo

- Usa dos subindicadores internos:
  - `DIPos`: calcula fuerza direccional alcista  
  - `DINeg`: calcula fuerza direccional bajista  
- Calcula el índice como:
  $$
  DX_t = 100 \times \frac{|DI^+ - DI^-|}{DI^+ + DI^-}
  $$
- El valor final (`this[bar]`) es 0 si `DI+ + DI- == 0`  
- Los valores de `DI+` y `DI-` también se exponen como `DataSeries[0]` y `DataSeries[1]` respectivamente

---

### ❗ Incoherencias o aspectos mejorables detectadas

- Aunque se añaden las series de `DI+` y `DI-` como `DataSeries`, no se documenta visualmente que el valor mostrado en el gráfico principal es **DX**, lo cual puede inducir a confusión si se muestran todas las líneas  
- No hay umbral predeterminado o guía visual para ayudar a la interpretación  
- El nombre `DX` puede confundirse con **ADX**, que implica un suavizado adicional (aquí no se realiza)

---

### 🛠️ Propuestas de mejora

- Añadir opción para mostrar **ADX** con suavizado si se desea  
- Incluir líneas guía opcionales en niveles como `20` y `40`  
- Documentar explícitamente que el valor calculado es **DX y no ADX**  
- Permitir alertas cuando el DX cruce valores clave definidos por el usuario