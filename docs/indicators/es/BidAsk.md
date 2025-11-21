---
cs_file: BidAsk.cs
name: Bid Ask
group: Order Flow
subgroup: Delta
score_current: 6.5/10
version: Estable
recommended_action: Conservar (Reserva)
description: ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela?
gemini_summary: "Muestra la 'batalla' completa (Bid vs Ask) en bruto. Es información pura, pero visualmente más ruidosa y difícil de interpretar rápidamente que el ratio normalizado de su competidor."
comparison_group: "Bar Delta Details"
competitor_notes: "Inferior a BidAskVR para toma de decisiones rápida, ya que requiere interpretar dos barras en lugar de una señal sintetizada."
reusable_code: null
file_state: Estable
score_potential: 7/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🛡️ Bid Ask (6.5/10)

**Nombre del archivo:** [`BidAsk.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BidAsk.cs)  
**Nombre del indicador:** Bid Ask  
**Web oficial:** [ATAS — Bid Ask](https://help.atas.net/support/solutions/articles/72000602329)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cuáles fueron los volúmenes brutos de agresión de compra (Ask) y de agresión de venta (Bid) en cada vela?

![BidAsk](../../img/BidAsk.png)

---

### ⚙️ Parámetros configurables

* **N/A:** El indicador no tiene parámetros lógicos.  
* *Nota: Los colores se heredan de la configuración de Footprint del gráfico.* ---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Bar Delta Details"  

---

### 🧠 Uso más frecuente

* **Análisis de Estructura:** Ver el volumen total de agresión por lado.  
* **Comparativa Bruta:** Ver si una vela tuvo mucho volumen en ambos lados (lucha) o solo en uno.  

---

### 📊 Nivel de relevancia
🔟 **6.5 / 10**

✅ **Transparencia:** Muestra el dato crudo sin procesar.  
⛔ **Ruidoso:** Ver dos histogramas opuestos por barra requiere más carga cognitiva para interpretar el neto.  
⛔ **No Configurable:** Sin filtros ni opciones.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Análisis Post-Morten:** Útil para diseccionar qué pasó en una vela específica, más que para señales en tiempo real.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **N/A.** ---

### 🧪 Notas de desarrollo

* Dibuja dos histogramas: `_asks` (positivo) y `_bids` (negativo, multiplicado por -1).  
* Simple y directo.  

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta línea cero:** Aunque los histogramas parten de cero, una línea guía ayudaría.  

---

### 🛠️ Propuestas de mejora

* **P3:** Añadir opción para mostrar el Delta Neto como una línea superpuesta para dar contexto.  

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** Lógica básica de visualización.  

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta honesta pero básica. Para scalping, queremos información procesada (¿Quién gana?), no datos crudos que tenemos que sumar y restar mentalmente. `BidAskVR` hace ese trabajo por nosotros.

**Propuestas de Acción:**
* **Conservar como Reserva.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Moderadamente.**

Bueno para análisis detallado, lento para ejecución.

**Acción:** **Conservar (Reserva).**