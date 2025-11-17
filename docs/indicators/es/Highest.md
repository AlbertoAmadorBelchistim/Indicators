## 🟦 Highest (6/10)

**Nombre del archivo:** `Highest.cs`  
**Nombre del indicador:** Highest  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602627](https://help.atas.net/support/solutions/articles/72000602627)

---

### ⚙️ Parámetros configurables

- **Period**: Número de barras usadas para buscar el valor máximo (por defecto: 10)

---

### 🧭 Clasificación  
📂 Levels — Indicadores que marcan extremos locales o históricos

---

### 🧠 Uso más frecuente

- Marcar el **punto más alto** dentro de una ventana móvil  
- Detectar **niveles de resistencia dinámica** o techos de canal  
- Usar como base para trailing stops, rompimientos o validaciones de estructura

---

### 📊 Nivel de relevancia  
🔟 **6 / 10**

✅ Ligero, preciso y útil como componente técnico o visual  
✅ Puede integrarse fácilmente en otros cálculos o estrategias  
⛔ Solo muestra el valor máximo sin contexto adicional (volumen, frecuencia, etc.)  
⛔ No discrimina si el máximo fue reciente o más lejano dentro del periodo

---

### 🎯 Estrategias de scalping donde se aplica

- **Breakout alcista**: entrada si el precio supera el valor de Highest  
- **Trailing dinámico**: usar Highest como nivel de activación de cierre parcial  
- **Zona de rechazo**: actuar si el precio toca el nivel y aparece absorción o falta de agresión

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `20`  
- Dibujar con línea discontinua azul en panel principal  
- Usar en combinación con volumen, delta o DOM para confluencia

✅ Permite validar rupturas reales  
✅ Compatible con estructura de canal dinámico

---

### 🧪 Notas de desarrollo

- El cálculo se realiza recorriendo `SourceDataSeries` desde `bar - Period + 1` hasta `bar`  
- Se usa `Math.Max` para encontrar el mayor valor dentro de esa ventana  
- El valor resultante se guarda en `this[bar]`  
- Si `bar < Period`, la ventana se acorta para evitar acceso fuera de rango

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El valor inicial de `max` se toma como `SourceDataSeries[start]`, lo cual **puede fallar si hay valores nulos o sin inicializar**  
- No se expone el índice o la barra donde ocurrió el máximo, lo que sería útil para análisis de antigüedad  
- No ofrece visualización directa (línea, etiqueta, color)

---

### 🛠️ Propuestas de mejora

- Añadir una opción para mostrar una línea horizontal o marcador en el gráfico  
- Incluir parámetro para mostrar también la barra del máximo (edad del nivel)  
- Permitir calcular también el valor más alto de `High`, `Close`, `Delta`, etc. mediante selector  
- Incluir opción para dejar trazado histórico de máximos rotos o niveles clave
