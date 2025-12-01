---
# 1. IDENTIFICACIÓN
cs_file:  DomPressure.cs
name:  DOM Pressure
version:  Custom v1.0

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
gemini_summary:  "La herramienta definitiva del grupo. Fusiona la lógica de DomPower y DomStrength en una visualización 'Ambiental'. Resuelve el problema de escala mediante normalización independiente y ofrece señales visuales claras de Absorción (Divergencia) y Vacuum (Convergencia) sin ensuciar la UI."
competitor_notes:  "Sustituye y mejora a DomPowerModif y DomStrengthModif."
reusable_code:  "Lógica de renderizado manual con ValueDataSeries ocultas y normalización de escalas independientes."

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
* **DOM Depth Limit:** (Default: 20) Número de niveles de precio por lado (Bid/Ask) a sumar. `20` es óptimo para ES (S&P 500) para capturar la liquidez táctica sin ruido lejano.  

#### **2. Visuals (Estilo Ambiental)**
* **Power Width %:** (Default: 95) Ancho de la barra de fondo (Intención Pasiva). Ocupa casi todo el espacio para crear un efecto de "terreno".  
* **Strength Width %:** (Default: 30) Ancho de la barra frontal (Agresión Real). Más fina para destacar sobre el fondo.  
* **Power Opacity:** (Default: 60) Transparencia alta para que el fondo no distraiga.  
* **Strength Opacity:** (Default: 255) Opacidad total (Sólido) para que la agresión sea el foco visual.  

#### **3. Colors (Semáforo)**
* **Buy / Sell Color:** Colores base para presión alcista/bajista.  
* **Absorption Marker:** (Amarillo) Color del rombo que aparece en la punta de la barra cuando hay divergencia de signos (Ej: Agresión Vendedora sobre Soporte Pasivo de Compras).  
* **Axis/Grid Color:** Color de la línea cero y etiquetas de escala.  


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "Liquidez vs Agresión"  


---

### 🧠 Uso más frecuente

* **Detección de Absorción (Reversal):**
    * *Visual:* Barra Fina Roja (Ventas) dentro de una Barra Ancha Verde (Soporte Pasivo) + Rombo Amarillo.
    * *Significado:* Los vendedores atacan, pero el libro los absorbe. Posible giro alcista.
* **Validación de Ruptura (Vacuum):**
    * *Visual:* Barra Fina Verde (Compras) sobre una Barra Ancha Verde (Soporte) o Gris (Vacío).
    * *Significado:* No hay resistencia pasiva (Asks) frenando la subida. Camino libre.
* **Falsas Rupturas:** Agresión fuerte (Barra fina grande) que deja un rombo amarillo en un extremo y el precio se gira inmediatamente.  


---

### 📊 Nivel de relevancia
🔟 **10 / 10**

✅ **Normalización Visual:** Resuelve el problema de comparar 10.000 órdenes pasivas con 500 agresivas. Ambas se ven grandes y claras gracias a la escala independiente.  
✅ **Limpieza de UI:** Utiliza `VisualMode.Hide` internamente, por lo que la lista de indicadores de ATAS no se llena de series basura, pero los datos siguen accesibles para estrategias automáticas.  
✅ **Escala de Referencia:** Muestra los valores máximos numéricos (ej. "P: 12k, S: 800") en la esquina para mantener el contexto real.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Scalping de Rebote:** Entrar a la contra cuando aparece el **Rombo de Absorción** en un Key Level.  
* **Gestión de Salidas:** Cerrar un trade de tendencia si vemos que nuestra agresión empieza a ser absorbida (Aparición de fondo opuesto).  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor | Justificación |
| :--- | :--- | :--- |
| **Dom Depth Limit** | `20` | Enfocarse en la liquidez que realmente puede frenar el precio ahora. |
| **Power Width** | `95` | Maximizar el efecto "contexto de fondo". |
| **Strength Width** | `30` | Maximizar contraste visual. |
| **Absorption Color** | `Yellow` | Debe ser el elemento más llamativo del panel. |


---

### 🧪 Notas de desarrollo

* **Técnica de Renderizado:** Usa GDI+ sobre la capa `Final`. Calcula las coordenadas Y manualmente relativas al panel (`Container.Region`) para evitar problemas de escala con ATAS.  
* **Gestión de Datos:** Mantiene dos `ValueDataSeries` ocultas (`ScaleIt=false`) para almacenar el histórico, permitiendo que estrategias automáticas lean los valores `_powerSeries[i]` y `_strengthSeries[i]` sin que afecten al gráfico visual.  
* **Fix Histórico:** Implementa `OnCumulativeTradesResponse` con lógica de asignación de tiempo para reconstruir el Delta histórico al recargar el gráfico.  


---

### 🛠️ Propuestas de mejora

* Ninguna inmediata. El indicador está en estado final de producción.  


---

### 💎 Valor Reutilizable (Código Donante)

* **Patrón de Renderizado Normalizado:** El método `OnRender` es una plantilla perfecta para cualquier oscilador que necesite comparar dos magnitudes muy diferentes (ej. Volumen vs Delta) en el mismo espacio.  


---

### ✍️ La opinión de Gemini sobre el Indicador

Ha pasado de ser un concepto teórico a una herramienta letal. Elimina la necesidad de mirar dos paneles y hacer cálculos mentales. Si ves un rombo amarillo, sabes que hay "lucha". Si ves colores alineados, hay "flujo". Simple y efectivo.

**Acción:** **Conservar (Core)**