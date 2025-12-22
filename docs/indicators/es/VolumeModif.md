---
# 1. IDENTIFICACIÓN
cs_file: Volume.cs  
name: Volume  
version: Custom v1.1  

# 2. CLASIFICACIÓN
group: Order Flow  
subgroup: Volume  
comparison_group: "Standard Volume"  

# 3. VALORACIÓN (Score & Priority)
score_current: 9.5/10  
score_potential: 10/10  
file_state: Estable  
effort: Bajo  
action_priority: Nula  
system_priority: P1  

# 4. DECISIÓN
recommended_action: Conservar (Core)  

# 5. ANÁLISIS
description: ¿Cuál es el volumen real y estadísticamente relevante en cada vela según la métrica seleccionada?  
gemini_summary: "La versión Modif convierte el Volume estándar en una herramienta contextual avanzada, añadiendo coherencia interna y umbrales estadísticos dinámicos sin sacrificar rendimiento ni simplicidad visual."  
competitor_notes: "Supera claramente al Volume estándar al introducir contexto adaptativo. Compite directamente con indicadores de volumen avanzados sin perder claridad."  
reusable_code: "Implementación de Welford online para thresholds dinámicos reutilizable en cualquier métrica acumulativa."  

# 6. METADATOS
analysis_date: 2025-12-22  
official_code_date: 2025-05-14  
user_modification_date: 2025-12-22  
---

## 🏆 Volume Modif (9.5/10)

**Nombre del archivo:** [`Volume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Volume.cs)  
**Nombre del indicador:** Volume  
**Web oficial:** [ATAS — Volume](https://help.atas.net/support/solutions/articles/72000602498)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-05-14  
**Última revisión del código modificado:** 2025-12-22  

> **La Pregunta Clave:** ¿Cuál es el volumen real y estadísticamente relevante en cada vela según la métrica seleccionada?

![Volume](../../img/Volume.png)

---

### ⚙️ Parámetros configurables

#### 📊 Cálculo
* **Type (Input):**  
  Define la métrica base utilizada **en todo el indicador** (histograma, máximo y thresholds).  
  * `Volume`: Volumen total negociado (contratos).  
  * `Ticks`: Número de transacciones (frecuencia).  
  * `Asks`: Volumen agresor comprador.  
  * `Bids`: Volumen agresor vendedor.  

#### 🧰 Filtros y Alertas
* **Use Filter:** Resalta visualmente las barras que superan un volumen específico (Clímax).
* **Use Alerts:** Sonido cuando se rompe el umbral de filtro.
* **Reverse Alert:** Detecta divergencias (Vela Alcista con Delta Negativo o viceversa).


#### 📐 Thresholds
* **Threshold Source:**  
  * `Fixed`: Umbrales manuales clásicos.  
  * `DynamicWelford`: Umbrales estadísticos dinámicos basados en media y desviación estándar.  
* **Show Threshold Lines:** Mostrar/ocultar líneas de referencia.  

#### 📏 Fixed Threshold
* **Fixed Minor Level:** Nivel base fijo.  
* **Fixed Major Level:** Nivel climático fijo.  

#### 📈 Dynamic Threshold (Welford)
* **Session Window Mode:**  
  * `RTH`: Reset diario en sesión regular.  
  * `Full24h`: Acumulación continua.  
* **RTH Start / End:** Ventana temporal de sesión.  
* **Samples for Mean/Std:** Mínimo de muestras antes de activar thresholds.  
* **Std Multiplier (k):**  
  * Minor = media  
  * Major = media + k × desviación estándar  

#### 🎨 Visualización
* **Delta Colored:** Colorea la barra según delta interno.  
* **Maximum Volume:** Línea del máximo **coherente con el Input seleccionado**.  
* **Volume Label:** Etiquetas numéricas opcionales.  

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Standard Volume"  

---

### 🧠 Uso más frecuente

* **Contextualizar volumen:** Diferenciar volumen normal de actividad excepcional.  
* **Confirmar rupturas reales:** Breakouts acompañados de volumen estadísticamente anómalo.  
* **Detectar absorción:** Volumen extremo sin continuación direccional.  

---

### 📊 Nivel de relevancia
🔟 **9.5 / 10**

✅ Contexto estadístico adaptativo.  
✅ Coherencia total entre Input, máximo y thresholds.  
⛔ Requiere entender estadística básica para sacarle el máximo partido.  

---

### 🎯 Estrategias de scalping donde se aplica

* **VSA avanzado:** Identificación objetiva de Stopping Volume.  
* **Rupturas con confirmación:** Solo operar rupturas que superan el threshold Major.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor recomendado | Justificación |
| :--- | :--- | :--- |
| Input | `Volume` | Métrica más estable en M1. |
| Threshold Source | `DynamicWelford` | Adaptación automática al contexto. |
| Samples for Mean/Std | `20` | Balance entre rapidez y estabilidad. |
| Std Multiplier | `1.0` | Detecta volumen institucional sin exceso. |
| Delta Colored | `True` | Confirma agresión real. |
| Volume Label | `False` | Reduce ruido visual. |

---

### ✨ Mejoras introducidas (Oficial/Base)

* Ninguna.  

---

### ✨ Mejoras añadidas (Custom)

* Corrección de incoherencia: `MaximumVolume` ahora sigue el `Input` seleccionado.  
* Introducción de thresholds dinámicos basados en Welford sin look-ahead.  
* Reset de acumuladores por sesión (RTH o 24h).  
* Renderizado eficiente mediante `ValueDataSeries` sin impacto en rendimiento.  

---

### 🧪 Notas de desarrollo

* Implementación de Welford online sin recalcular históricos.  
* Alimentación del acumulador con `bar-1` para evitar sesgo temporal.  
* Corte automático de líneas en cambios de sesión.  
* El código utiliza una estructura `enum InputType` que facilita la extensión futura a otros tipos de datos.
* La gestión de colores (`DeltaColored`) está bien implementada, permitiendo overriding por el sistema de filtros.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguna tras las modificaciones actuales.  

---

### 🛠️ Propuestas de mejora

* Añadir alertas basadas en superación de threshold Major.  
* Permitir percentiles como alternativa a Welford.  

---

### 💎 Valor Reutilizable (Código Donante)

* Implementación genérica de thresholds estadísticos online.  
* Lógica de corte visual de series por sesión. 
* * **OnRender Text:** El bloque de código que dibuja las etiquetas de texto es muy reutilizable para otros indicadores que necesiten mostrar valores sobre barras sin usar objetos pesados.

---

### ✍️ La opinión de Gemini sobre el Indicador

La versión Modif convierte un indicador imprescindible en una herramienta de lectura institucional avanzada, sin perder simplicidad ni rendimiento.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**  

Ahora no solo muestra volumen, sino **significado estadístico del volumen**.

**Acción:** **Conservar (Core)**
