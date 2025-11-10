## 🟦 Average Daily Range (ADR) (7/10)

**Nombre del archivo:** `ADR.cs`  
**Nombre del indicador:** Average Daily Range  
**Web oficial:** [ATAS - ADR (Average Daily Range)](https://help.atas.net/support/solutions/articles/72000602312)

![ADR](../img/ADR.png)

---

### ⚙️ Parámetros configurables

- **Pen**: Configuración del color y estilo de las líneas
- **CalculationMode**: Punto de referencia para proyectar el ADR (`OpenSession`, `HighSession`, `LowSession`)
- **FontSize**: Tamaño del texto de la etiqueta (por defecto: 12)
- **Period**: Número de sesiones consideradas para el cálculo del ADR (por defecto: 3)

---

### 🧭 Clasificación
📂 Volatility — Indicadores de rango promedio diario

---

### 🧠 Uso más frecuente

- Visualizar niveles proyectados de **rango diario esperado** (ADR)
- Determinar **posibles zonas de reversión o agotamiento** del precio
- Confirmar si un movimiento ha alcanzado ya el recorrido típico diario
- Complementar con indicadores de momentum o volumen para validar entradas/salidas

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Útil como contexto de volatilidad esperada  
✅ Proyecta niveles visuales directamente en el gráfico  
⛔ No incluye alertas contextuales

---

### 🎯 Estrategias de scalping donde se aplica

- **Take Profit estructural**: cerrar en zona de ADR superior/inferior  
- **Reversión estadística**: entrada contraria al movimiento cuando se alcanza el extremo del ADR  
- **Breakout + Proyección**: entrada en ruptura con target proyectado por ADR  
- **Filtro de escenarios**: evitar trades cuando el rango ya ha sido recorrido

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `5`  
- **CalculationMode**: `OpenSession`  
- **FontSize**: `10`  
- **Pen**: color gris o azul, líneas finas  
- Colocar el ADR sobre **gráficos de 1 minuto** o superiores con sesiones bien definidas

✅ Esta configuración ofrece una referencia diaria clara de extensión esperada  
✅ Ideal para colocar objetivos de beneficios o invalidaciones estadísticas  
⛔ No se adapta automáticamente a eventos atípicos de volatilidad

---

### 🧪 Notas de desarrollo

- El indicador calcula la media del rango diario como diferencia entre el máximo y el mínimo de cada sesión.
- Se proyectan dos líneas: superior e inferior, con base en el punto de control elegido (`Open`, `High`, `Low`).
- La visualización se realiza con objetos `LineTillTouch` y una etiqueta flotante con el valor promedio.
- Se actualiza dinámicamente al recibir nuevos datos de vela (`OnCalculate`, `ProcessNewBar`, `ProcessNewTick`).
- El valor del ADR se recalcula sólo al detectarse el inicio de una nueva sesión.

---

### 🛠️ Propuestas de mejora

- Añadir **línea media** entre los niveles superior e inferior (ADR central)
- Incluir **zonas coloreadas** entre los extremos del ADR para mejorar la visualización
- Agregar **alertas** cuando el precio alcance o supere el nivel de ADR
- Permitir ajustar el ADR según **rango real (High-Low)** o **true range**
- Opción para **reset automático intradía** o personalización de sesiones

---

### Comentario de Gemini

Aquí tienes la "pregunta clave" de este indicador:

**¿El mercado ha completado ya su rango diario total habitual?**


### 🔍 Análisis de la Implementación


Tu propuesta de "Permitir ajustar el ADR según... **true range**" no es solo una mejora, es **la corrección más importante que necesita este indicador.**

El código actual calcula el rango como `_currentSessionHigh - _currentSessionLow`. El problema es que esto **ignora los gaps** entre sesiones.
* Si el ES cierra a 4500 y abre con gap alcista a 4520, el `High - Low` de la sesión anterior no captura esa volatilidad.
* El **True Range** (ATR) sí lo haría, ya que compara el máximo/mínimo actual con el cierre anterior.

Para un cálculo de volatilidad *real*, el True Range es muy superior. Tu propuesta de mejora es la que convierte este indicador de "bueno" (7/10) a "excelente" (9/10).

---

### 📈 Veredicto para Scalping (¿Lo incluirías?)

**Sí, al 100%.** Con la configuración que tú mismo definiste (`Period=5`, `Mode=OpenSession`).

Es una herramienta de contexto esencial.

* **Como filtro de entradas:** Si el precio ya ha tocado la línea superior del ADR a las 10:00 AM, sabes que estás en un día de tendencia extrema, o que cualquier compra adicional es estadísticamente arriesgada (una "reversión" es probable).
* **Como Take Profit:** Es el lugar más lógico y estadístico para colocar un Take Profit en una operación intradía.
* **Como Fader (Reversión):** Si el precio llega al ADR superior/inferior con una divergencia de volumen o delta, es una señal de alta probabilidad para buscar un "fade" (operación en contra) de vuelta a la media.