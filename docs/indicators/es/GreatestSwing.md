---
cs_file: GreatestSwing.cs
name: Greatest Swing Value
category: Structure
group: Structure
subgroup: Static Levels
score_current: 7/10
version: Estable
recommended_action: Conservar
description: ¿Dónde están los niveles S/R dinámicos, proyectados desde el Open actual?
gemini_summary: "Implementación estable. Proyecta la media de los 'swings de rechazo' (mechas). Útil para volatilidad."
comparison_group: "Volatility Range"
competitor_notes: "Alternativa a ADR."
reusable_code: null
file_state: Estable
score_potential: 7/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-17
official_code_date: 23/04/2025
---

## 🟦 Greatest Swing Value (7/10)

**Nombre del archivo:** [`GreatestSwing.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/GreatestSwing.cs)  
**Nombre del indicador:** Greatest Swing Value  
**Web oficial:** [ATAS — Greatest Swing Value](https://help.atas.net/support/solutions/articles/72000602635)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Dónde están los niveles S/R dinámicos, proyectados desde el Open actual usando la media de los 'swings de rechazo' anteriores?

![Greatest Swing Value](../../img/GreatestSwing.png)

---

### ⚙️ Parámetros configurables

* **Period**: Número de barras usadas para el promedio de swings (por defecto: 10)
* **Multiplier**: Factor de ampliación aplicado al swing promedio (por defecto: 5)

---

### 🧭 Clasificación
📂 Levels — Indicadores de canales dinámicos calculados a partir de swings

---

### 🧠 Uso más frecuente

* Calcular **niveles dinámicos de soporte y resistencia** basados en swings
* Determinar zonas donde el mercado tiende a revertir según **amplitud histórica del movimiento**
* Proyectar rangos extremos para entrada o salida según impulso reciente

---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Lectura estructural basada en movimientos reales (swings)  
✅ Se adapta dinámicamente al comportamiento reciente del mercado  
⛔ Lógica peculiar: ignora velas doji y tiene un lag de 1 barra en el promedio  
⛔ No incluye confirmación por volumen o delta

---

### 🎯 Estrategias de scalping donde se aplica

* **Entrada contraria en zonas de proyección**: cuando el precio alcanza los niveles proyectados y aparece señal de absorción
* **Breakout validado**: si el precio atraviesa el nivel sin retroceso
* **Trailing stop táctico**: usar el canal como guía para trailing de posiciones abiertas

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Period**: `10`
* **Multiplier**: `5`
* Usar junto con delta o CVD para confirmar agresión en extremos

---

### 🧪 Notas de desarrollo

* Calcula "Buy Swings" (`High - Open`) solo en velas bajistas (`Close < Open`).
* Calcula "Sell Swings" (`Open - Low`) solo en velas alcistas (`Close > Open`).
* **Ignora velas Doji** (`Close == Open`), no registrando ningún swing.
* Calcula una media (`SkipZeroMa`) de estos swings **hasta la barra anterior** (`bar - 1`), introduciendo un lag de 1 barra.
* Proyecta esta media multiplicada (`Multiplier`) desde el `Open` de la barra actual para crear el canal.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Esta es una implementación estable de un canal de volatilidad/reversión. Su concepto es tomar el "rechazo" promedio (las mechas) de las barras anteriores y proyectar esos niveles desde el Open actual como un "rango esperado".

Como se detectó en el `.md` original y se confirmó en el código, el indicador tiene dos peculiaridades de diseño:
1.  **Ignora Dojis**: Las velas con `Close == Open` no contribuyen al cálculo de la media de swings.
2.  **Lag de 1 Barra**: Usa la media de swings *hasta la barra anterior* (`bar - 1`) y la aplica al `Open` de la barra actual.

Estas no son "bugs" que rompan el indicador, sino "quirks" de diseño. El indicador es estable y el concepto es válido para scalpers que buscan niveles de S/R dinámicos basados en la volatilidad reciente.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Es una buena herramienta de contexto para identificar niveles de S/R dinámicos. Es útil para estrategias de reversión a la media y para definir "zonas de sobreextensión" intradía.

**Acción:** **Conservar (Estable).**