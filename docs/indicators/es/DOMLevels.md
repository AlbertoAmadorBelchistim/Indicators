---
# 1. IDENTIFICACIÓN
cs_file:  DomLevels.cs  
name:  DOM Levels/Heatmap  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  DOM  
comparison_group:  "DOM Visuals"  

# 3. VALORACIÓN (Score & Priority)
score_current:  9/10  
score_potential:  9/10  
file_state:  Estable  
effort:  N/A  
action_priority:  Nula  
system_priority:  P1  

# 4. DECISIÓN
recommended_action:  Conservar (Core)  

# 5. ANÁLISIS
description:  ¿Dónde ha estado la liquidez históricamente? ¿Es este nivel de soporte nuevo o lleva ahí todo el día?  
gemini_summary:  "El Tanque. Insuperable en rendimiento gráfico gracias a su motor nativo acelerado por hardware. Es la contraparte estratégica del MBO: ofrece el contexto histórico."  
competitor_notes:  "Complemento del MBO. MBO domina el presente (Tactical), DomLevels domina el pasado (Strategic)."  
reusable_code:  "DataCollector.cs (Lógica matemática de agregación)."  

# 6. METADATOS
analysis_date:  2025-11-30  
official_code_date:  Desconocida  
---

## 🟦 DOM Levels/Heatmap (9/10)

**Nombre del archivo:** `DomLevels.cs`  
**Nombre del indicador:** DOM Levels/Heatmap  
**Web oficial:** [ATAS Help - DomLevels](https://help.atas.net/support/solutions/articles/72000602241)  
**Compatibilidad:** ATAS versión estable y superiores. **Requiere datos L2 (Market Depth).**  

> **La Pregunta Clave:** ¿Cómo ha evolucionado la liquidez del libro de órdenes (Heatmap) a lo largo del tiempo en el gráfico?

![DOMLevels](../../img/DOMLevels.png)


---

### ⚙️ Parámetros configurables

#### **HeatmapSettings (Motor Gráfico)**
* **Heatmap Type:** Selecciona la paleta de colores (Térmico, Azul monocromo, etc.).  
* **Upper Cutoff:** (%) Umbral de saturación. Define qué porcentaje del volumen máximo se pinta con el color más intenso.  
* **Contrast:** (%) Ajuste gamma. Aumenta la diferencia visual entre niveles de volumen alto y medio.  
* **Smoothing Mode:** `Auto` (calculado por zoom) o `Manual`. Define si los bloques se difuminan verticalmente para crear efecto "nube".  
* **Smoothing:** (Si Manual) Intensidad del desenfoque vertical.  
* **Pow Scale:** (Logarítmica) Acentúa visualmente los volúmenes bajos.  
* **Extend DOM Levels:** El mapa de calor llega hasta la vela actual.
* **Show inactive levels:** Muestra las últimas órdenes conocidas en niveles alejados del precio actual que no reciben actualizaciones del proveedor de datos.

#### **Filters**
* **Min Volume Filter:** Oculta completamente niveles con volumen inferior a X (limpieza de ruido).  
* **Max Volume Filter:** Resalta en un color específico (ej. Rojo) los niveles que superan X volumen.  

#### **HistorySaving**
* **Days To Save:** Días de historial a mantener en caché de disco.  
* **Bars Limit:** Límite de barras para renderizar en pantalla.  


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "DOM Visuals"  


---

### 🧠 Uso más frecuente

* **Contexto de Mercado:** Ver zonas de liquidez históricas que actuaron como imán o rechazo.  
* **Trailing Liquidity:** Detectar algoritmos que mueven la liquidez acompañando al precio (soporte dinámico).  


---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Rendimiento:** Utiliza texturas GPU (`OFT.Rendering`).
✅ **Persistencia:** Guarda datos históricos en disco mediante `BinaryWriter`.  
⛔ **Caja Negra:** Lógica cerrada.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Reingreso a Rango:** Usar bordes de liquidez histórica como zonas de "fade".  
* **Breakout Failure:** Confirmar falsas rupturas cuando chocan con liquidez histórica.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor | Justificación |
| :--- | :--- | :--- |
| **Heatmap Type** | `RedToDarkToGreen` | Alto contraste visual térmico. |
| **Contrast** | `30` | Resaltar "ballenas" sobre el ruido de fondo. |
| **Auto Smoothing** | `True` | Visión orgánica de "zonas" en lugar de líneas pixeladas. |
| **Min Volume Filter** | `5` | Limpiar ruido de 1-4 lotes. |
| **Max Volume (Custom)** | `500` | Estandarizar la escala visual para ES. |


---

### 🧪 Notas de desarrollo

* **Ingeniería Inversa:** El código decompilado revela que usa `HeatmapControl` (interno de ATAS) y gestiona una caché de disco propia en `C:\ATAS\UserData`.  
* **Optimización:** La clase `DataCollector` usa diccionarios sincronizados para agregar volúmenes antes de enviarlos al motor gráfico, asegurando que el renderizado no se ralentice con millones de ticks.  


---

### ❗ Incoherencias o aspectos mejorables detectados

* **Caja negra:** No se puede recompilar fácilmente como indicador custom porque depende de DLLs con código cerrado (`ATAS.Indicators.Other`).  


---

### 🛠️ Propuestas de mejora

* Ninguna viable a nivel de código dado que es un indicador nativo cerrado. Su mejora pasa por complementarlo con el MBO DOM.  


---

### 💎 Valor Reutilizable (Código Donante)

* **`DataCollector.cs`:** La lógica matemática de agregación de volúmenes por precio y barra es extraíble y reutilizable para crear osciladores de liquidez o indicadores de presión de DOM sin parte gráfica.  


---

### ✍️ La opinión de Gemini sobre el Indicador

Es insustituible como fondo de pantalla del trader. Su valor está en su motor gráfico propietario.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Muy útil.** Ver la historia de la liquidez da contexto a los movimientos actuales.

**Acción:** **Conservar (Core)**