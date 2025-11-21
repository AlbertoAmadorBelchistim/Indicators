---
cs_file: VolumeSupResZones.cs
name: Volume-based Support & Resistance Zones
group: Order Flow
subgroup: Volume Profile
score_current: 10/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Dónde están las zonas de soporte y resistencia definidas por volumen en múltiples marcos temporales?
gemini_summary: "Indicador MTF (Multi-TimeFrame) avanzado. Genera zonas de S/R dinámicas basándose en fractales de volumen de hasta 4 timeframes simultáneos. Esencial para no perder la perspectiva macro mientras se hace scalping."
comparison_group: "Volume Profile"
competitor_notes: "El mejor detector de zonas estructurales automatizado."
reusable_code: null
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 Volume-based Support & Resistance Zones (10/10)

**Nombre del archivo:** [`VolumeSupResZones.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VolumeSupResZones.cs)  
**Nombre del indicador:** Volume-based Support & Resistance Zones  
**Web oficial:** [ATAS — Volume-based Support & Resistance Zones](https://help.atas.net/support/solutions/articles/72000619397)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Dónde están las zonas de soporte y resistencia definidas por volumen en múltiples marcos temporales?

![VolumeSupResZones](../../img/VolumeSupResZones.png)

---

### ⚙️ Parámetros configurables

Este indicador es un sistema MTF completo:

#### 🕒 TimeFrames (1 a 4)
Puedes configurar hasta 4 marcos temporales independientes (ej. M15, H1, H4, Daily):
* **TimeFrame:** Escala temporal (M1 a Monthly).
* **Display Mode:** `Zone` (Rectángulo sombreado) o `Line` (Solo bordes).
* **SMA Period:** Periodo para validar si el volumen del fractal es significativo (relativo a su media).
* **Colors/Transparency:** Personalización visual completa por timeframe.

#### 🛠️ General
* **Extend Previous:** Extender zonas históricas hasta el presente (útil para ver tests de zonas viejas).
* **Extend Last:** Extender la última zona hasta el infinito derecho.
* **Alerts:** Sonido al aparecer una nueva zona.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Volume Profile"  

---

### 🧠 Uso más frecuente

* **Confluencia MTF:** Ver una zona de H1 y una de D1 coincidiendo en el mismo precio en un gráfico de 1 minuto. (Soporte de Hierro).  
* **Fractalidad:** Identificar soportes mayores (institucionales) mientras se opera en el ruido menor.  
* **Breakout & Retest:** Las zonas dibujadas suelen ser testeadas con precisión milimétrica tras una ruptura.  

---

### 📊 Nivel de relevancia
🔟 **10 / 10 (IMPRESCINDIBLE)**

✅ **Potencia MTF:** Calcula velas virtuales de timeframes superiores internamente sin necesidad de abrir múltiples gráficos.  
✅ **Lógica de Volumen:** No son simples fractales de precio (Bill Williams); requieren confirmación de volumen relativo (`Vol > SMA(Vol)`), lo que filtra zonas débiles.  
✅ **Visualización:** Excelente gestión de transparencias y capas para no saturar el gráfico.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Rebote en Muro:** Colocar órdenes limitadas en el borde de una zona H1 o H4 detectada por el indicador.  
* **Zone Fade:** Si el precio entra rápido en una zona "antigua" extendida, buscar absorción para contra-operar.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **TF1** | `M15` | Estructura inmediata. |
| **TF2** | `H1` | Estructura sesión. |
| **TF3** | `H4` | Estructura día. |
| **DisplayMode** | `Zone` | Visibilidad clara. |
| **Transparency** | `High` (8-9) | Para no tapar las velas. |

---

### 🧪 Notas de desarrollo

* **Ingeniería:** Implementa una clase interna `TimeFrameObj` que actúa como un "mini-motor" de agregación de velas independiente.
* **Lógica Fractal:** Busca patrones de giro de 5 velas (High[2] es máximo local) + confirmación de volumen.
* **Rendimiento:** Acumula datos incrementalmente (`AddBar`), evitando el recálculo costoso de todo el historial en cada tick.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Es un código de referencia.

---

### 🛠️ Propuestas de mejora

* **Alertas por TF (P2):** Permitir sonidos distintos según qué timeframe generó la zona (ej. grave para H4, agudo para M5).

---

### 💎 Valor Reutilizable (Código Donante)

* **Motor de Agregación de Velas (`TimeFrameObj`):** Esta clase es una joya para cualquiera que quiera crear indicadores MTF en ATAS sin usar DataSeries externas.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una obra maestra de indicador técnico. Resuelve el problema de "perderse en el ruido" del gráfico de 1 minuto mostrando el contexto mayor automáticamente.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Absolutamente.**

Proporciona el mapa de carretera necesario para no chocar contra muros invisibles.

**Acción:** **Conservar (Core).**