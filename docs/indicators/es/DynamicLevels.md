---
cs_file: DynamicLevels.cs
name: Dynamic Levels
group: Order Flow
subgroup: Volume Profile
score_current: 9/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Dónde se están formando el POC, VAH y VAL del período actual en tiempo real?
gemini_summary: "Herramienta 'Core' de Nivel 2. Calcula el Perfil de Volumen expansivo (desde el inicio de la sesión hasta ahora), mostrando la evolución del POC y el Área de Valor. Indispensable para el contexto intradía."
comparison_group: "Session Profile"
competitor_notes: "El estándar para niveles dinámicos de sesión."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 Dynamic Levels (9/10)

**Nombre del archivo:** [`DynamicLevels.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DynamicLevels.cs)  
**Nombre del indicador:** Dynamic Levels  
**Web oficial:** [ATAS — Dynamic Levels](https://help.atas.net/support/solutions/articles/72000602380)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Dónde se están formando el POC, VAH y VAL del período actual (ej. Día, Semana, Hora) en tiempo real?

![Dynamic Levels](../../img/DynamicLevels.png)

---

### ⚙️ Parámetros configurables

Este indicador construye un perfil de volumen acumulativo:

#### 📊 Cálculo
* **Period Frame:** Ventana de tiempo para el perfil (`Daily`, `Weekly`, `Monthly`, `Hourly`, `H4`, `All`).
* **Type:** Fuente de datos para el perfil.
    * `Volume`: Estándar.
    * `Delta`: POC de agresión neta.
    * `Bid` / `Ask` / `Tick`.
* **Filter:** Valor mínimo para considerar un nivel relevante.

#### 🎨 Visualización
* **Show Volume:** Muestra etiquetas numéricas con el volumen acumulado en el nivel.
* **Colors:** Personalización de líneas (POC, VAH, VAL) y área de valor.

#### 🔔 Alertas
* **Touch Alerts:** Sonido al tocar POC, VAH o VAL.
* **Approximation:** Sonido al acercarse al nivel.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Session Profile"  

---

### 🧠 Uso más frecuente

* **POC en Desarrollo:** Ver cómo el "precio justo" del día se mueve. Si el precio sube pero el POC no le sigue, la subida es débil (divergencia de valor).  
* **Value Area:** Identificar si el mercado está aceptando precios más altos (VAH subiendo) o rechazándolos.  
* **Soportes Dinámicos:** El VAL (Value Area Low) suele actuar como soporte en tendencias alcistas intradía.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (IMPRESCINDIBLE)**

✅ **Contexto en Vivo:** A diferencia de un perfil estático (que ves al final del día), este te muestra la *formación* del perfil tick a tick.  
✅ **Versatilidad:** El modo `Delta` permite ver dónde se ha producido la mayor lucha direccional.  
✅ **Eficiencia:** Código optimizado con `DynamicCandle` para no saturar la CPU.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Reversión a la Media:** Si el precio se aleja mucho del POC dinámico (extensión), buscar reversión hacia el POC.  
* **Defensa de Valor:** Comprar en el test del VAL en un día de tendencia alcista.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Period** | `Daily` | Ver la estructura de la sesión RTH. |
| **Type** | `Volume` | Estándar. |
| **Alerts** | `Touch` | Avisar si volvemos al POC. |

---

### 🧪 Notas de desarrollo

* Usa una clase interna `DynamicCandle` que gestiona un `SortedList<decimal, PriceInfo>` para mantener el perfil en memoria.
* Calcula el Área de Valor (VAH/VAL) iterando desde el POC hacia afuera hasta cubrir el 70% (o el % configurado en la plataforma) del volumen total.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Configuración VA:** El % del Value Area (ej. 70%) se toma de la configuración global de la plataforma (`PlatformSettings.ValueAreaPercent`), no del indicador. Sería mejor tenerlo como parámetro local.

---

### 🛠️ Propuestas de mejora

* **Historial (P3):** Opción para mostrar los niveles finales de los días anteriores como líneas estáticas de referencia.

---

### 💎 Valor Reutilizable (Código Donante)

* **Algoritmo VAH/VAL:** El método `GetValueArea` dentro de `DynamicCandle` es la implementación canónica de cálculo de Value Area.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el mapa de carretera del día. Sin él, no sabes si estás operando dentro o fuera de valor.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para contextualizar cada trade.

**Acción:** **Conservar (Core).**
