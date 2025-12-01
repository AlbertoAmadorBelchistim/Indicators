---
# 1. IDENTIFICACIÓN
cs_file:  DomPressure.cs
name:  DOM Pressure
version:  Custom v1.1 (Smart Absorption)

# 2. CLASIFICACIÓN
group:  Order Flow
subgroup:  DOM
comparison_group:  "Liquidez vs Agresión"

# 3. VALORACIÓN (Score & Priority)
score_current:  10/10
score_potential:  10/10
file_state:  Estable
effort:  Bajo
action_priority:  Baja
system_priority:  P1

# 4. DECISIÓN
recommended_action:  Conservar (Core)

# 5. ANÁLISIS
description:  ¿Está el mercado absorbiendo la agresión o dejándola pasar? Indicador híbrido que superpone la intención pasiva (DOM Power) con la agresión real (Trade Strength).
gemini_summary:  "Herramienta definitiva. Fusiona la lógica de DomPower y DomStrength en una visualización 'Ambiental' normalizada. Introduce un filtro inteligente de Absorción que marca divergencias significativas entre la intención del libro y la ejecución real. Limpia la carga cognitiva al unificar dos paneles en uno."
competitor_notes:  "Sustituye y mejora a DomPowerModif y DomStrengthModif."
reusable_code:  "Lógica de renderizado manual con ValueDataSeries ocultas, normalización independiente y filtro de ratio."

# 6. METADATOS
analysis_date:  2025-12-01
official_code_date:  Unknown
user_modification_date:  2025-12-01
---

## 🏆 DOM Pressure (10/10)

**Nombre del archivo:** [`DomPressure.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/DomPressure.cs)  
**Nombre del indicador:** DOM Pressure  
**Web oficial:** N/A (Desarrollo Propio)  
**Compatibilidad:** ATAS Custom.  
**Última revisión del código modificado:** 2025-12-01  

> **La Pregunta Clave:** ¿Está el mercado absorbiendo la agresión o dejándola pasar?

![DomPressure](../../img/DomPressure.png)

---

### ⚙️ Parámetros configurables

#### **1. Calculation (Filtros)**
* **DOM Depth Limit:** (Default: 20) Niveles de precio por lado (Bid/Ask) a sumar. `20` optimiza la lectura de soporte táctico en S&P 500.  
* **Absorption Threshold %:** (Default: 15) Filtro de sensibilidad. La agresión (Strength) debe representar al menos el 15% del tamaño del muro (Power) para activar la señal de absorción. Elimina señales de ruido con poco volumen.  

#### **2. Visuals (Estilo Ambiental)**
* **Power Width %:** (Default: 95) Ancho de la barra de fondo. Crea el contexto visual.  
* **Strength Width %:** (Default: 30) Ancho de la barra frontal. Destaca la acción del precio.  
* **Power/Strength Opacity:** Transparencia independiente para jerarquizar la información.  

#### **3. Colors (Semáforo)**
* **Buy/Sell Color:** Colores base.  
* **Absorption Marker:** Color del rombo de alerta.  
* **Axis Color:** Color de las etiquetas numéricas de referencia.  


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "Liquidez vs Agresión"  


---

### 🧠 Uso más frecuente

* **Detección de Absorción (Giro):**
    * *Señal:* Barra Fina Roja (Ventas) sobre Barra Ancha Verde (Soporte) + Rombo Amarillo.
    * *Lectura:* Los vendedores chocan con un muro relevante. Posible rebote.
* **Validación de Ruptura (Continuación):**
    * *Señal:* Barra Fina Verde (Compras) sobre Barra Ancha Verde o Neutra.
    * *Lectura:* La agresión tiene respaldo o camino libre.
* **Contexto de Escala:** Lectura rápida de los valores máximos ("DOM: 10k / TRD: 500") en la esquina para no perder la perspectiva del volumen real.  


---

### 📊 Nivel de relevancia
🔟 **10 / 10**

✅ **Normalización Inteligente:** Permite comparar visualmente magnitudes dispares (10k vs 500) sin perder la proporción relativa de cada fuerza.  
✅ **UI Limpia:** Oculta las series de datos de la configuración de ATAS para evitar errores de usuario, pero mantiene los datos accesibles internamente.  
✅ **Filtro de Ruido:** El nuevo `AbsorptionThreshold` asegura que solo las absorciones estadísticamente relevantes generen una señal visual.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Fade the Breakout:** Entrar en contra de una ruptura si aparece el rombo de absorción en el máximo de la vela.  
* **Pullback Support:** Entrar a favor de tendencia cuando el retroceso es absorbido (Fondo a favor, Frente en contra + Rombo).  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor | Justificación |
| :--- | :--- | :--- |
| **Dom Depth Limit** | `20` | Zona táctica de 5 puntos. |
| **Absorption Threshold** | `15` | Filtra el "ruido" de 1-2 contratos chocando con muros. |
| **Power Width** | `95` | Maximizar efecto fondo. |
| **Strength Width** | `30` | Contraste nítido. |
| **Absorption Color** | `Yellow` | Debe ser el elemento más llamativo del panel. |
### 🧪 Notas de desarrollo

* **Técnica de Renderizado:** Usa GDI+ sobre la capa `Final`. Calcula las coordenadas Y manualmente relativas al panel (`Container.Region`) para evitar problemas de escala con ATAS.  
* **Gestión de Datos:** Mantiene dos `ValueDataSeries` ocultas (`ScaleIt=false`) para almacenar el histórico, permitiendo que estrategias automáticas lean los valores `_powerSeries[i]` y `_strengthSeries[i]` sin que afecten al gráfico visual.  
* **Fix Histórico:** Implementa `OnCumulativeTradesResponse` con lógica de asignación de tiempo para reconstruir el Delta histórico al recargar el gráfico.  


---

### ✨ Mejoras introducidas (Custom)
* **Fusión de Motores:** Integración de `DomPower` y `DomStrength`.  
* **Renderizado Normalizado:** Algoritmo propio de escalado dual.  
* **Smart Absorption:** Lógica condicional (Signo opuesto + Ratio > Umbral) para alertas de alta calidad.  


---

### 💎 Valor Reutilizable (Código Donante)

* **Patrón de Normalización Visual:** El método `OnRender` sirve de plantilla para cualquier oscilador multicapa.  


---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta "Head-Up Display" (HUD) para el trader. Te da la información vital (¿Pasa o Choca?) sin obligarte a calcular ratios mentales. El filtro del 15% lo convierte en un arma de precisión.

**Acción:** **Conservar (Core)**