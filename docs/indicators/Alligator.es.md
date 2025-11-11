## 🟦 Alligator (6/10)

**Nombre del archivo:** `Alligator.cs`  
**Nombre del indicador:** Alligator  
**Web oficial:** [ATAS - Alligator](https://help.atas.net/support/solutions/articles/72000602579)  
**Compatibilidad:** ATAS versión stable y superiores.

![Alligator](../img/Alligator.png)

---

### ⚙️ Parámetros configurables

- **JawPeriod**: Periodo de la mandíbula (por defecto: 13)  
- **JawShift**: Desplazamiento de la mandíbula (por defecto: 8)  
- **TeethPeriod**: Periodo de los dientes (por defecto: 8)  
- **TeethShift**: Desplazamiento de los dientes (por defecto: 5)  
- **LipsPeriod**: Periodo de los labios (por defecto: 5)  
- **LipsShift**: Desplazamiento de los labios (por defecto: 3)

---

### 🧭 Clasificación  
📂 Trend — Indicadores orientados a detectar y seguir tendencias

---

### 🧠 Uso más frecuente

- Identificar tendencias y zonas de consolidación  
- Visualizar cruces de medias con desplazamiento temporal (shifts) para detectar entradas  
- Operar con lógica de "boca del caimán" abierta (tendencia) o cerrada (rango)  
- Filtrar operaciones en función de la fase del mercado (expansión o compresión)

---

### 📊 Nivel de relevancia  
🔟 6 / 10  

✅ Buen indicador visual de fase del mercado  
✅ Fácil de interpretar para estrategias tendenciales  
⛔ Menos útil en entornos de alta volatilidad o reversión rápida

---

### 🎯 Estrategias de scalping donde se aplica

- Tendencia clara: boca abierta, todos los elementos alineados = entrada en favor de la tendencia  
- Cruce de labios con dientes: posible señal anticipada de giro  
- Zona de consolidación: líneas entrelazadas = evitar operar o buscar ruptura  
- Confirmación visual: como filtro secundario junto a Order Flow o Delta

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- JawPeriod: 13  
- JawShift: 8  
- TeethPeriod: 8  
- TeethShift: 5  
- LipsPeriod: 5  
- LipsShift: 3  

✅ Esta configuración es la original de Bill Williams y se adapta bien a tendencias limpias  
✅ Ideal para acompañar movimientos sostenidos tras breakout  
⛔ Puede generar señales atrasadas si se usa como trigger principal

---

### 🧪 Notas de desarrollo

- El indicador usa medias móviles suavizadas (SMMA) con desplazamientos hacia el futuro (shifts)  
- El valor de entrada es el promedio entre el máximo y el mínimo de cada vela  
- Se calculan tres líneas: Jaw (azul), Teeth (roja), Lips (verde), cada una con su propio periodo y desplazamiento  
- El método OnCalculate aplica el shift manualmente restando barras al índice  
- No utiliza Close ni TypicalPrice, sino (High + Low) / 2 como base del cálculo

---

### 🛠️ Propuestas de mejora

- Permitir seleccionar el tipo de precio a usar (Close, Typical, Median...)  
- Añadir opción de alertas por cruce entre líneas  
- Incorporar colores dinámicos según dirección de la tendencia  
- Guardar y mostrar señales de apertura/cierre de la “boca” (p. ej. histogramas)  
- Adaptar el indicador para funcionar también con velas de volumen o rango

--- 

## Comentario de Gemini
Aquí tienes la "pregunta clave" de este indicador:

**¿Está el mercado 'durmiendo' (en rango, con las medias entrelazadas) o está 'despierto y comiendo' (en tendencia, con las medias abiertas)?**

---

### ✍️ Opinión sobre el Indicador

Este indicador es un "clásico", pero para el scalping tiene un problema fundamental: **el LAG masivo**.

Es, por diseño, un "filtro de régimen" (Tendencia vs. Rango) increíblemente lento. Piénsalo:
1.  Usa **SMMA**, que es una de las medias móviles más lentas y con más lag.
2.  Aplica **tres** de ellas.
3.  Y luego **desplaza** (shiftea) esos cálculos hacia adelante, basando lo que ves hoy en lo que pasó hace 3, 5 y 8 barras.

Es el "lag sobre el lag sobre el lag".

**¿Es útil?**
Es un excelente **filtro de fase de mercado**.
* **Líneas entrelazadas ("Caimán durmiendo"):** Te grita "¡NO OPERES RUPTURAS, ESTAMOS EN RANGO!".
* **Líneas abiertas ("Caimán comiendo"):** Te confirma (muy tarde) que "OK, esto es una tendencia".

**El problema para el Scalping:**
Mira la captura de pantalla. La gran caída comienza a las ~08:45. Las medias (que ya son lentas de por sí) no se cruzan y "abren la boca" de forma decisiva hasta las ~09:30. Para un scalper, la operación no es que haya empezado, ¡es que casi ha terminado!

---

### 📈 Veredicto

Ya hemos analizado y decidido **conservar** el **AMA (Kaufman)**. El AMA hace *exactamente el mismo trabajo* que el Alligator (diferenciar entre tendencia y rango), pero lo hace de una forma **infinitamente superior, más rápida y adaptativa**.

El Alligator es el abuelo lento del AMA. Dado que ya tenemos la herramienta moderna y rápida, no hay razón para usar la antigua y lenta.

**Acción:** **Descartar** (Es un buen indicador, pero el AMA lo supera en todo).