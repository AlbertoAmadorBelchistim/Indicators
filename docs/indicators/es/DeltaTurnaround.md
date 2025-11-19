---
cs_file: DeltaTurnaround.cs
name: Delta Turnaround
category: Order Flow
group: Order Flow
subgroup: Delta
score_current: 4/10
version: Latest
recommended_action: Descartar
description: ¿Se ha producido un patrón de giro de 3 velas confirmado por el delta?
gemini_summary: "Demasiado rígido. Busca un patrón 'hard-coded' de 3 velas que rara vez se da perfecto en mercados modernos. La detección de giros debe hacerse por contexto (Estructura + Delta), no por una secuencia fija de colores de vela."
comparison_group: "Bar Delta"
competitor_notes: "Inferior a la lectura discrecional usando DeltaModif o a la configuración flexible de BarsPattern."
reusable_code: null
file_state: Estable
score_potential: 4/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-19
official_code_date: 2025-07-31
---

## ⚠️ Delta Turnaround (4/10)

**Nombre del archivo:** [`DeltaTurnaround.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DeltaTurnaround.cs)  
**Nombre del indicador:** Delta Turnaround  
**Web oficial:** [ATAS — Delta Turnaround](https://help.atas.net/support/solutions/articles/72000602364)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 31/07/2025  

> **La Pregunta Clave:** ¿Se ha producido un patrón de giro de 3 velas (dos en una dirección, una en la opuesta) confirmado por el delta?

![DeltaTurnaround](../../img/DeltaTurnaround.png)

---

### ⚙️ Parámetros configurables

* **UseAlerts**: Activar alertas sonoras.
* **AlertOnNewCandle**: Lanzar alerta al abrir la siguiente vela (confirmación) o en la actual.
* **AlertFile / Colores**: Configuración de audio y visual de la alerta.
* *Nota: La lógica del patrón (nº de velas, etc.) no es configurable.*

---

### 🧭 Clasificación
**Grupo:** Order Flow
**Subgrupo:** Delta (Por Barra)

---

### 🧠 Uso más frecuente

* **(Teórico)** Automatizar la detección de patrones de reversión "Vela de Giro" con confirmación de Delta.
* **(Teórico)** Scalping de contratendencia en rangos.

---

### 📊 Nivel de relevancia
4️⃣ **4 / 10 (RÍGIDO)**

✅ **Concepto Válido:** El patrón de reversión de 3 velas es un clásico del Price Action.  
⛔ **Hard-Coding:** El código busca una secuencia exacta (A-A-B). Si el mercado gira en 4 velas (A-A-A-B) o hace una pausa (A-Doji-B), el indicador falla.  
⛔ **Inflexible:** No permite ajustar la sensibilidad ni el umbral de delta.

---

### 🎯 Estrategias de scalping donde se aplica

* **Giro Bajista:** Busca 2 velas alcistas previas + 1 vela bajista actual que falle en hacer nuevo máximo + Delta Negativo.
* **Giro Alcista:** Busca 2 velas bajistas previas + 1 vela alcista actual que falle en hacer nuevo mínimo + Delta Positivo.

---

### ⚙️ Parametrización óptima para scalping

* **No recomendado.**
* La falta de parámetros de "tolerancia" lo hace inútil en mercados con ruido como el S&P 500.

---

### 🧪 Notas de desarrollo

* El indicador itera sobre `prevCandle` y `prev2Candle`.
* **Lógica Bajista:** `High >= Prev.High` AND `Delta < 0`.
* **Bug de Alerta:** Si `AlertOnNewCandle = true`, el texto de la alerta es genérico ("Delta turnaround signal") y no dice la dirección.

---

### 🛠️ Propuestas de mejora

Ninguna. Para tener un detector de patrones configurable, es infinitamente mejor usar el indicador `BarsPattern` (que permite definir secuencias complejas) en lugar de parchear este código rígido.

---

### 💎 Valor Reutilizable

Ninguno. Es lógica condicional básica (`if A and B and C`) sin valor algorítmico especial.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es un indicador de "Juguete". Funciona en los libros de texto donde los giros son perfectos y de 3 velas, pero falla en la realidad sucia del mercado.

Un trader profesional no quiere una caja negra que solo pite si se alinean los astros exactamente como el programador pensó en 2015. Quiere herramientas que le muestren la **presión** (`DeltaModif`) para poder juzgar el giro él mismo.

### 📈 Veredicto: ¿Es útil para Scalping?

**No.** Es redundante y demasiado rígido.

Si quieres automatizar este patrón, usa `BarsPattern`. Si quieres operarlo discrecionalmente, usa `DeltaModif`.

**Acción:** **DESCARTAR.**