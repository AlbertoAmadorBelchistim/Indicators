---
# 1. IDENTIFICACIÓN
cs_file: Volume.cs
name: Volume
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Volume
comparison_group: "Standard Volume"

# 3. VALORACIÓN (Score & Priority)
score_current: 9/10
score_potential: 9/10
file_state: Estable
effort: N/A
action_priority: Nula
system_priority: P1

# 4. DECISIÓN
recommended_action: Conservar (Core)

# 5. ANÁLISIS
description: ¿Cuál es el volumen de actividad en cada vela y cómo se colorea según el delta?
gemini_summary: "El indicador de volumen definitivo. Es completo, flexible (permite ver Volume, Ticks, Bid o Ask) y visualmente rico, integrando alertas de volumen alto y reversión. Es la base sobre la que se construye cualquier análisis de VSA."
competitor_notes: "La base sobre la que se construyen los demás. Insuperable en funcionalidad estándar."
reusable_code: "Lógica de Renderizado de Texto en OnRender (muy eficiente)."

# 6. METADATOS
analysis_date: 2025-12-10
official_code_date: 2025-05-14
---

## 🏆 Volume (9/10)

**Nombre del archivo:** [`Volume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Volume.cs)  
**Nombre del indicador:** Volume  
**Web oficial:** [ATAS — Volume](https://help.atas.net/support/solutions/articles/72000602498)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-05-14  

> **La Pregunta Clave:** ¿Cuál es el volumen de actividad en cada vela y cómo se colorea según el delta?

![Volume](../../img/Volume.png)

---

### ⚙️ Parámetros configurables

#### 📊 Cálculo
* **Type (Input):**
    * `Volume`: Volumen total operado (Contratos).
    * `Ticks`: Número de transacciones (Frecuencia).
    * `Asks`: Solo volumen de compra agresiva.
    * `Bids`: Solo volumen de venta agresiva.

#### 🧰 Filtros y Alertas
* **Use Filter:** Resalta visualmente las barras que superan un volumen específico (Clímax).
* **Use Alerts:** Sonido cuando se rompe el umbral de filtro.
* **Reverse Alert:** Detecta divergencias (Vela Alcista con Delta Negativo o viceversa).

#### 🎨 Visualización
* **Delta Colored:** Colorea la barra según quién ganó la batalla interna (Delta), ignorando el cierre de la vela.
* **Maximum Volume:** Línea de referencia del volumen máximo reciente.
* **Volume Label:** Etiquetas numéricas sobre las barras.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Standard Volume"  

---

### 🧠 Uso más frecuente

* **Validación de Rupturas:** Breakout sin volumen es una trampa.
* **Detección de Absorción:** Volumen extremo en soporte sin que el precio baje más.
* **Lectura de Delta:** Usar `DeltaColored` para ver la "intención" real detrás del movimiento.

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (IMPRESCINDIBLE)**

✅ **Versatilidad:** Cubre todas las necesidades básicas de análisis de actividad.  
✅ **Inteligencia:** La alerta de divergencia (`Reverse Alert`) es una joya oculta.  
✅ **Rendimiento:** Optimizado para no consumir recursos innecesarios.  

---

### 🎯 Estrategias de scalping donde se aplica

* **VSA (Volume Spread Analysis):** Identificar "No Demand" (Volumen bajo en subida) o "Stopping Volume" (Volumen climático en bajada).

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Justificación |
| :--- | :--- | :--- |
| **Input** | `Volume` | Estándar. |
| **Delta Colored** | `True` | Vital para ver la agresión real. |
| **Filter** | `2000` (Ajustar) | Para marcar solo velas institucionales. |
| **Volume Label** | `False` | Desactivar para limpiar ruido en M1. |

---

### 🧪 Notas de desarrollo

* El código utiliza una estructura `enum InputType` que facilita la extensión futura a otros tipos de datos.
* La gestión de colores (`DeltaColored`) está bien implementada, permitiendo overriding por el sistema de filtros.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguna. Es un código de referencia.

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* **OnRender Text:** El bloque de código que dibuja las etiquetas de texto es muy reutilizable para otros indicadores que necesiten mostrar valores sobre barras sin usar objetos pesados.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el "Pan y la Mantequilla" del trading. No puedes operar sin volumen, y este indicador te lo da de la mejor forma posible.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Absolutamente fundamental.

**Acción:** **Conservar (Core)**