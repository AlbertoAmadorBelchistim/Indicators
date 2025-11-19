---
# --- Campos Públicos (Para INDICATORS.es) ---
cs_file: StackedImbalance.cs
name: Stacked Imbalance
category: VolumeOrderFlow
score_current: 8/10
version: Stable
recommended_action: Conservar
description: ¿Dónde existen zonas de desequilibrio agresivo de compra/venta apiladas que actúan como soporte/resistencia?
# --- Campos de Triaje (Para ROADMAP.md) ---
gemini_summary: "Indicador complejo de Order Flow. Usa lógica intensiva de escaneo de niveles de precio. Funcionalidad TillTouch útil."
file_state: Estable
score_potential: 9/10
effort: Alto
action_priority: N/A
# --- Control de Versiones ---
analysis_date: 2025-11-18
official_code_date: 23/04/2025
user_modification_date: null
---

## 🟦 Stacked Imbalance (8/10)

**Nombre del archivo:** [`StackedImbalance.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/StackedImbalance.cs)  
**Nombre del indicador:** Stacked Imbalance  
**Web oficial:** [ATAS — Stacked Imbalance](https://help.atas.net/support/solutions/articles/72000602474)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Dónde existen zonas de desequilibrio agresivo de compra/venta apiladas que actúan como soporte/resistencia?

![StackedImbalance](../../img/StackedImbalance.png)

---

### ⚙️ Parámetros configurables

* **ImbalanceRatio**: Porcentaje de diferencia requerido entre Ask y Bid (ej. 300%).
* **ImbalanceRange**: Cantidad de niveles de precio consecutivos con desequilibrio para activar la señal (ej. 3 niveles).
* **ImbalanceVolume**: Volumen mínimo en el nivel para considerarlo.
* **TillTouch**: Si es `true`, extiende la línea horizontal hasta que el precio futuro la toque.
* **Days**: Días hacia atrás para calcular zonas históricas.

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Indicador de microestructura y zonas de absorción/agresión.

---

### 🧠 Uso más frecuente

* **Soporte/Resistencia Fresco:** Un "Stacked Imbalance" de compra (varios niveles donde Ask >>> Bid) actúa como soporte inmediato.
* **Re-test:** El mercado suele volver a probar el inicio del desequilibrio.
* **Trampas:** Si un Stacked Imbalance es atravesado rápidamente en contra, indica una trampa de mercado fuerte.

---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Información Institucional:** Revela huellas que las velas normales ocultan.  
✅ **Visualización Inteligente:** La función `TillTouch` mantiene el nivel relevante en pantalla hasta que es invalidado (tocado).  
⛔ **Consumo de Recursos:** Analiza el volumen por nivel de precio (Tick data), lo que puede ser pesado en instrumentos muy líquidos.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Defensa de Zona:** Entrar largo cuando el precio vuelve a tocar una zona de *Stacked Imbalance* alcista reciente.  
* **Breakout Failure:** Si hay un imbalance bajista pero el precio cierra por encima, entrar largo (absorción de ventas).  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **ImbalanceRatio**: `300` o `400` (3x o 4x veces más volumen en un lado).
* **ImbalanceRange**: `3` (Tres ticks consecutivos).
* **TillTouch**: `True`.

---

### 🧪 Notas de desarrollo

* **Acceso a Datos:** Usa `candle.GetPriceVolumeInfo(price)`, lo que implica que requiere datos de tipo "Order Log" o "Tick" cargados. No funcionará con datos históricos que solo tengan OHLCV.
* **Gestión de Líneas:** Usa la colección especial `HorizontalLinesTillTouch`. Esta es una característica optimizada de ATAS para no redibujar líneas infinitas manualmente.
* **Lógica de Días:** El parámetro `Days` controla cuánto historial se escanea al iniciar. Está implementado correctamente para limitar la carga inicial.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta de "Clase A" para scalpers profesionales. Automatiza la lectura del Footprint chart. En lugar de mirar números, ves líneas extendidas. El código es robusto y aprovecha las APIs específicas de ATAS para dibujo eficiente.

**Propuestas de Mejora:**
* **Alerta de Rotura:** Añadir una alerta específica cuando una zona de Imbalance es rota (invalidad) con fuerza, no solo cuando se crea.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.** Vital para entender por qué el precio se detiene en "la nada" (generalmente un imbalance previo).

**Acción:** **Conservar.** de días para retroceder correctamente en el cálculo de desequilibrio