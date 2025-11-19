---
cs_file: HRanges.cs
name: HRanges
category: Order Flow
group: Order Flow
subgroup: Volume
score_current: 8.5/10
version: ATAS Official
recommended_action: Conservar
description: ¿Dónde se están formando rangos (consolidaciones) y cuál es el POC interno
  de esos rangos, filtrado por volumen y duración?
gemini_summary: Indicador 'Core' de Nivel 2; implementa una máquina de estados para
  detectar rangos, clasificarlos (Up/Down/Flat) y encontrar su POC, con filtros profesionales.
file_state: Estable
score_potential: 8.5/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-17
official_code_date: 2025-04-23
---

## 🟦 HRanges (8.5/10)

**Nombre del archivo:** [`HRanges.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/HRanges.cs)  
**Nombre del indicador:** HRanges  
**Web oficial:** [ATAS — HRanges](https://help.atas.net/support/solutions/articles/72000602573)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025

> **La Pregunta Clave:** ¿Dónde se están formando rangos (consolidaciones) y cuál es el POC interno de esos rangos, filtrado por volumen y duración?

![HRanges](../../img/HRanges.png)

---

### ⚙️ Parámetros configurables

* **Days**: Número de sesiones hacia atrás desde las que comenzar a calcular rangos (por defecto: 20)
* **VolumeFilter**: Volumen mínimo para mostrar el nivel de máximo volumen
* **BarsRange**: Número mínimo de barras para validar la zona como rango significativo
* **HideAllVolume / HideAllBarsFilter**: Ocultar zonas si no se cumplen los filtros
* **SwingUpColor / SwingDnColor / NeutralColor / VolumeColor**: Colores para rangos alcistas, bajistas, neutros y POC

---

### 🧭 Clasificación
📂 Volume — Rangos validados por volumen por nivel, no por agresión

---

### 🧠 Uso más frecuente

* Detectar **zonas de consolidación** o balance del mercado
* Confirmar rompimientos desde rangos con acumulación previa
* Identificar **POC del rango** y clasificarlo como alcista, bajista o neutro

---

### 📊 Nivel de relevancia
🔟 **8.5 / 10**

✅ **Herramienta "Core" de Perfil**: Lógica robusta para análisis de rangos con filtro por volumen y duración.  
✅ **Filtros Profesionales**: Los filtros `VolumeFilter` y `BarsRange` son esenciales para eliminar el ruido.  
✅ Detección estructurada de zonas clave sin necesidad de intervención manual.  
⛔ Visualización compleja si hay muchos rangos consecutivos.

---

### 🎯 Estrategias de scalping donde se aplica

* **Rompimiento confirmado**: entrada si el precio sale del rango con agresión y volumen creciente
* **Reversión en borde de rango**: si aparece absorción en la parte superior/inferior
* **POC como test táctico**: validación de rechazo o aceptación en el POC del rango.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Days**: `3`
* **BarsRange**: `10` (ej. 10 minutos)
* **VolumeFilter**: `500` (depende del volumen del activo)
* **HideAllBarsFilter / HideAllVolume**: `true`

---

### 🧪 Notas de desarrollo

* Implementa una **máquina de estados** para rastrear si está en tendencia (`_direction`) o en rango (`_isRange`).
* Inicia un rango (`_isRange = true`) cuando el precio falla en continuar una tendencia (ej. `_direction == 1 && candle.Close < prevCandle.High`).
* Termina un rango y lo dibuja (`RenderLevel`) cuando hay dos cierres consecutivos fuera de él.
* `RenderLevel` calcula el **POC del rango** (`maxVol.Key`) iterando los `PriceVolumeInfo` de las barras del rango.
* Aplica los filtros `VolumeFilter` y `BarsRange` antes de dibujar el POC o el rango completo.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Este es un indicador "Core" de Nivel 2, muy avanzado y específico para el análisis de rangos. El código es una máquina de estados compleja que detecta rangos, los clasifica (`Up`, `Down`, `Flat`) y calcula su POC interno.

Lo más importante son los filtros `VolumeFilter` y `BarsRange`. Estos son filtros profesionales que evitan que el gráfico se llene de "ruido" de rangos insignificantes (de bajo volumen o corta duración). La opción `HideAllBarsFilter` es crucial.

Para un scalper que opera rupturas de rangos o "juega" dentro del rango (operando el POC), esta es una herramienta de automatización de análisis técnico de altísimo valor. Es estable y robusta.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Es una herramienta de contexto "Core" para el trading de rangos.**

Define objetivamente las zonas de consolidación y sus niveles de valor (POC), permitiendo al scalper planificar entradas de ruptura o de reversión.

**Acción:** **Conservar (Herramienta Principal).**
