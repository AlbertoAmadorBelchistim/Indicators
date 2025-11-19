---
cs_file: Volume.cs
name: Volume
category: Order Flow
group: Order Flow
subgroup: Volume
score_current: 9/10
version: Stable
recommended_action: Conservar
description: ¿Cuál es el volumen de actividad (o ticks/bid/ask) en cada vela, y cómo
  se relaciona con el movimiento?
gemini_summary: Indicador de volumen definitivo. Completo, flexible (InputType) y
  visualmente rico (Etiquetas, Alertas).
file_state: Estable
score_potential: 9/10
effort: Bajo
action_priority: N/A
analysis_date: 2025-11-18
official_code_date: 2025-05-14
---

## 🟦 Volume (9/10)

**Nombre del archivo:** [`Volume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Volume.cs)  
**Nombre del indicador:** Volume  
**Web oficial:** [ATAS — Volume](https://help.atas.net/support/solutions/articles/72000602498)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 14/05/2025  

> **La Pregunta Clave:** ¿Cuál es el volumen de actividad (o ticks/bid/ask) en cada vela, y cómo se relaciona con el movimiento?

![Volume](../../img/Volume.png)

---

### ⚙️ Parámetros configurables

* **Input**: `Volume`, `Ticks`, `Asks`, `Bids`.  
* **Filter**: Resaltar barras que superen X volumen.  
* **Alerts**: Volumen alto y Reversión (Vela alcista con Delta negativo y viceversa).  
* **Visuals**: Colores por Delta o por Vela. Línea de Máximo Volumen.  
* **Labels**: Mostrar texto con el valor numérico en el gráfico.  

---

### 🧭 Clasificación
📂 Volume — El indicador de actividad de mercado por excelencia.

---

### 🧠 Uso más frecuente

* **Confirmación:** Volumen alto confirma ruptura. Volumen bajo sugiere falta de interés.  
* **Clímax:** Volumen ultra-alto en extremos indica parada y giro (Absorción).  
* **Divergencia Delta:** Vela sube pero Delta es negativo (coloreado por Delta) → Venta limitadas absorbiendo compras mercado.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Todo en Uno:** No necesitas otro indicador de volumen.  
✅ **Alertas Inteligentes:** La alerta de "Reversión" (Divergencia precio-delta) es una estrategia de trading en sí misma.  
✅ **UX:** Etiquetas de texto integradas (`ShowVolume`) que se adaptan al zoom.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Volume Stopping:** Buscar la vela con mayor volumen de la sesión en un soporte. Si la siguiente vela es alcista, entrar.  
* **Effort vs Result:** Mucho volumen (esfuerzo) y poco avance del precio (resultado) = Giro inminente.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Input**: `Volume`.  
* **DeltaColored**: `True` (Vital para ver quién ganó la batalla interna).  
* **Filter**: `2000` (o valor relevante para la sesión).  

---

### 🧪 Notas de desarrollo

* **Código:** Muy completo. Gestiona renderizado de texto manual (`OnRender`) para optimizar rendimiento.
* **Flexibilidad:** El `enum InputType` permite usarlo como contador de Ticks o indicador de Ask/Bid específico.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es el estándar. La capacidad de colorear por Delta y las alertas de divergencia integradas lo elevan por encima de un simple histograma de volumen.

**Propuestas de Mejora:**
* Ninguna. Es la referencia.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.** El volumen es la gasolina del mercado. No se puede scalpear ciego al volumen.

**Acción:** **Conservar.**