## 🟦 Adaptive Moving Average (AMA) (7/10)

**Nombre del archivo:** `AMA.cs`  
**Nombre del indicador:** Adaptive Moving Average  
**Web oficial:** [ATAS - Adaptive Moving Average](https://help.atas.net/support/solutions/articles/72000602310)  
**Compatibilidad:** ATAS versión stable y superiores.

![AMA](../img/AMA.png)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de cálculo para la eficiencia adaptativa (por defecto: 15)
- **FastConstant**: Constante para el extremo rápido de la media móvil (por defecto: 3)
- **SlowConstant**: Constante para el extremo lento de la media móvil (por defecto: 20)

---

### 🧭 Clasificación
📂 Trend — Indicadores de seguimiento de tendencia con adaptación dinámica

---

### 🧠 Uso más frecuente

- Obtener una media móvil que **se adapta dinámicamente a la eficiencia del mercado**
- Detectar cambios de tendencia con menor retardo en comparación con medias tradicionales
- Filtrar señales en sistemas de seguimiento de tendencia

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Se ajusta automáticamente a la volatilidad del mercado  
✅ Buena respuesta en movimientos rápidos sin sobreajustar  
⛔ Puede mostrar señales falsas en mercados con poco movimiento

---

### 🎯 Estrategias de scalping donde se aplica

- **Entrada en ruptura con confirmación AMA**: operar cuando el precio cruza y la pendiente del AMA cambia
- **Filtro de tendencia adaptativo**: solo tomar operaciones en la dirección de la pendiente del AMA
- **Salida por pérdida de eficiencia**: cuando la AMA se aplana tras fuerte movimiento

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `15`  
- **FastConstant**: `3`  
- **SlowConstant**: `20`  

✅ Ofrece equilibrio entre sensibilidad y estabilidad  
✅ Buena adaptación a impulsos con poco retardo  
⛔ En consolidaciones prolongadas puede generar sobreajustes

---

### 🧪 Notas de desarrollo

- Se calcula una "Efficiency Ratio" basada en el movimiento neto dividido por la suma de cambios absolutos.
- Esta eficiencia pondera la constante de suavizado: más rápida si el movimiento es eficiente, más lenta si hay ruido.
- La fórmula es una variación del **Kaufman’s Adaptive Moving Average (KAMA)**.
- Usa una serie interna `_diffSeries` para medir volatilidad local.

---

### 🛠️ Propuestas de mejora

- Añadir opción para **visualizar la eficiencia actual** como serie secundaria
- Implementar un **suavizado adicional** para reducir ruido extremo
- Incluir opción para usar precios diferentes al cierre (open, high, low, median)
- Permitir **alertas visuales o sonoras** al cambio de pendiente

---

## Comentario de Gemini

Aquí tienes la "pregunta clave" de este indicador:

**¿Cómo puedo obtener una media móvil suave que *no* tenga retardo (lag) durante una ruptura fuerte, pero *sí* filtre el 'ruido' en un mercado lateral?**

---

### 📈 ¿Es útil para Scalping en S&P 500?

**Sí, absolutamente. Este es un indicador para "Conservar".**

Es una herramienta de contexto fantástica que funciona como un **filtro de régimen** (Tendencia vs. Rango) de una forma mucho más rápida y visual que el ADX.

Mira tu propia captura de pantalla. Es el ejemplo perfecto de por qué este indicador es tan bueno:

1.  **Modo Rango (08:30 - 09:25):** El precio se mueve lateral. El AMA detecta este "ruido" (baja eficiencia) y se **aplana como una tabla**. Te está diciendo visualmente: "No operes rupturas, estamos en un rango".
2.  **Modo Tendencia (09:25):** El precio rompe el AMA plano con fuerza. El indicador detecta un movimiento "eficiente" (alto `dir`, bajo `vol`) y **acelera bruscamente**, pegándose al precio y actuando como una resistencia dinámica.
3.  **Modo Rango (10:20 - 14:30):** El precio vuelve al "chop". El AMA se **aplana de nuevo**, definiendo perfectamente el nuevo rango de consolidación.
4.  **Modo Tendencia (14:30):** El precio rompe a la baja, y el AMA **acelera de nuevo**.

Para un scalper, esto es oro. Te da un "interruptor" visual inmediato para cambiar tu mentalidad de "operar rangos" a "operar tendencias".

**Sobre tus Parámetros:**
Tu configuración `(15, 3, 20)` es muy buena y estable, especialmente para el gráfico de 5 minutos (M5) que muestras en la captura. Si fueras a usarlo en un gráfico de 1 minuto (M1), podrías considerar la configuración estándar de Kaufman `(10, 2, 30)` para hacerlo un poco más reactivo.

**Veredicto:** Gran indicador. **Recomiendo conservarlo y probarlo** como tu filtro de tendencia principal.