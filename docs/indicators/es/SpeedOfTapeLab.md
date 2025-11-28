---
cs_file: SpeedOfTapeLab.cs
name: Speed of Tape (Lab)
group: Order Flow
subgroup: Volume
score_current: 6/10
version: Lab (v1.6)
recommended_action: Conservar (Reserva / Educativo)
description: ¿Cuál es la velocidad de ejecución del mercado calculada por interpolación?
gemini_summary: "Versión 'Lab' con visualización geométrica mejorada (Marcadores vs Líneas) y cálculo de filtro de delta bien ejecutado. Matemáticamente sigue limitada por la interpolación de velas, lo que la hace ciega a ráfagas HFT reales, pero su interfaz es ahora limpia y moderna."
comparison_group: "Tape Speed"
competitor_notes: "Inferior a la V2. Mientras la V2 ve la realidad tick a tick, esta versión ve un promedio suavizado."
reusable_code: "Sistema de renderizado de formas geométricas (DrawMarker) y lógica de interpolación lineal."
file_state: Estable
score_potential: 6/10
effort: Bajo
action_priority: P3
analysis_date: 2025-11-28
official_code_date: Desconocida
---

## 🧪 Speed of Tape (Lab) (6/10)

**Nombre del archivo:** [`SpeedOfTapeLab.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/SpeedOfTapeLab.cs)  
**Nombre del indicador:** Speed of Tape (Lab)  
**Web oficial base:** [ATAS — Speed of Tape](https://help.atas.net/support/solutions/articles/72000602472)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código modificado:** 28/11/2025 (v1.6 - UI Overhaul)  

> **La Pregunta Clave:** ¿Se está acelerando el mercado (estimación basada en velas)?

![SpeedOfTapeLab](../../img/SpeedOfTapeLab.png)

---

### ⚙️ Parámetros configurables

#### 🛡️ Filtros (Lógica)
* **AutoFilter:** Activa el promedio móvil para el umbral dinámico.
* **AutoFilter Period:** Periodo de la media móvil interna.
* **TimeWindowSec:** Ventana de tiempo a analizar (Se interpola si es menor que la vela).
* **Fixed Threshold:** Valor manual si se desactiva el AutoFilter.
* **Calculation Type:** `Volume`, `Ticks`, `Buys`, `Sells`, `Delta`.

#### 🎨 Visualization (Estética)
* **Pintar Velas (Highlight):** Colorea el cuerpo de la vela si hay alta velocidad.
* **Color de Highlight:** Color para la vela (ej. Amarillo).
* **Marker Shape:** Forma del indicador visual (`Triangle`, `Circle`, `Diamond`, `Square`).
* **Marker Size:** Tamaño en píxeles de la forma.
* **Vertical Offset (Px):** Distancia de separación desde el High/Low de la vela para no tapar el precio.
* **Buy/Sell Signal Color:** Colores independientes para aceleraciones alcistas o bajistas.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Tape Speed"  

---

### 🧠 Uso más frecuente

* **Comparativa Académica:** Útil para demostrar la diferencia entre "Velocidad Interpolada" (este indicador) y "Velocidad Real" (V2).
* **Swing Trading:** En gráficos de minutos altos donde el ruido tick a tick es irrelevante.

---

### 📊 Nivel de relevancia
🔟 **6 / 10 (NO APTO PARA SCALPING)**

✅ **Limpieza Visual (Nuevo):** El sistema de marcadores geométricos (Rombos/Círculos) con *Offset* configurable resuelve el problema de las líneas que ensuciaban el gráfico.  
✅ **Ligereza:** Consume casi cero recursos al no pedir ticks al servidor.  
⛔ **Falsedad de Datos (Interpolación):** Asume que el volumen se distribuye uniformemente.  
⛔ **Ceguera HFT:** Incapaz de detectar "Icebergs" o algoritmos de ejecución rápida dentro de la vela.

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna.** Visualmente atractivo, tácticamente peligroso por el lag de datos.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **TimeWindowSec:** `60` o más. (Poner menos es engañarse a uno mismo).
* **AutoFilter:** `True`.
* **Shape:** `Diamond` (Distingue bien de otros indicadores).

---

### ✨ Mejoras añadidas (Lab v1.6 - UI Overhaul)

Esta versión soluciona problemas visuales críticos y mejora la precisión del filtro en modos Delta.

#### 🔧 Correcciones Lógicas
* **Filtro de Magnitud Absoluta:** Se ha corregido el cálculo del *threshold* en el modo `Delta`. Ahora se utiliza `Math.Abs(accumulatedSpeed)`, lo que permite detectar aceleraciones fuertes de venta (valores negativos) que antes alteraban el cálculo del filtro.

#### 🎨 Sistema de Marcadores Geométricos
Se ha realizado una limpieza total del código de dibujo para modernizar la visualización:
* **Eliminación de Líneas:** Se eliminó la lógica antigua (`DrawLines`, `PenSettings`) que dibujaba líneas horizontales confusas a través de la vela.
* **Marcadores Dinámicos:** Implementación de `DrawMarker` en `OnRender` que soporta 4 formas distintas:
    * `Triangle` (Direccional: apunta arriba/abajo).
    * `Circle` (Discreto).
    * `Square` (Bloque).
    * `Diamond` (Clásico).
* **Control de Posición:** Nuevo parámetro `MarkerOffset` que permite alejar el marcador de la mecha de la vela, evitando superposiciones críticas.
* **Código de Color Dual:** Colores separados para señales de compra (`BuyColor`) y venta (`SellColor`).

---

### 🧪 Notas de desarrollo

* **Renderizado:** El uso de `FillPolygon` y `FillEllipse` en la capa `DrawingLayouts.Final` asegura que los marcadores se dibujen siempre por encima de las velas y rejillas, mejorando la visibilidad.

---

### 🛠️ Propuestas de mejora

* **Migración:** La única mejora real posible es cambiar el motor interno por el de la V2 (Ticks reales). Sin eso, esto es solo "maquillaje".

---

### ✍️ La opinión de Gemini sobre el Indicador

Visualmente, esta versión v1.6 es excelente. Ha pasado de parecer un boceto sucio a una herramienta profesional gracias a los nuevos marcadores y el control de offset.
Sin embargo, **"aunque la mona se vista de seda, mona se queda"**. El motor matemático sigue siendo interpolación de velas. Se ve mejor, pero sigue "mintiendo" sobre la velocidad instantánea.

**Propuestas de Acción:**
* Guardar como **ejemplo de referencia** de cómo implementar un buen sistema de renderizado geométrico (`OnRender` con `MarkerShape`).

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

Mejor visualización, mismos defectos estructurales.

**Acción:** **Conservar (Reserva / Educativo)**