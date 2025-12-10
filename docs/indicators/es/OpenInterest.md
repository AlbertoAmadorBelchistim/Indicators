---
# 1. IDENTIFICACIÓN
cs_file: OpenInterest.cs
name: Open Interest
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Open Interest
comparison_group: "Open Interest Analysis"

# 3. VALORACIÓN (Score & Priority)
score_current: 4/10
score_potential: 8/10
file_state: Estable
effort: Bajo
action_priority: Baja
system_priority: P3

# 4. DECISIÓN
recommended_action: Conservar (Reserva)

# 5. ANÁLISIS
description: ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión?
gemini_summary: "Indicador básico. En S&P 500 solo sirve para ver el nivel macro de OI día tras día, no para scalping. Su sistema de alertas es útil si alguna vez llega un dato de 'block trade' que actualice el OI, pero es raro."
competitor_notes: "Versión simplificada del Analyzer."
reusable_code: "Sistema de Alertas por cambio de valor (ChangeSize)."

# 6. METADATOS
analysis_date: 2025-11-21
official_code_date: 2025-04-23
---

## 🛡️ Open Interest (4/10)

**Nombre del archivo:** [`OpenInterest.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OpenInterest.cs)  
**Nombre del indicador:** Open Interest  
**Web oficial:** [ATAS — Open Interest](https://help.atas.net/support/solutions/articles/72000602439)  
**Compatibilidad:** ATAS versión estable.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es el Interés Abierto total (o su cambio neto) por barra o sesión?

![OpenInterest](../../img/OpenInterest.png)

---

### ⚙️ Parámetros configurables

* **Mode:** `ByBar`, `Cumulative` (Total), `Session`.
* **Filter:** Umbral visual.
* **Alerts:** Sonido si el OI cambia más de X contratos.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Open Interest  
**Comparison Group:** "Open Interest Analysis"  

---

### 🧠 Uso más frecuente

* **Contexto Macro:** Ver si el interés en el contrato sube o baja respecto a ayer.

---

### 📊 Nivel de relevancia
🔟 **4 / 10**

✅ **Alertas:** El código de alertas es limpio y reutilizable.  
⛔ **Datos Estáticos:** En S&P 500 será una línea recta intradía.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Ninguna intradía.** Solo análisis pre-mercado.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

*(No aplicable para scalping. Configuración para análisis diario)*

| Parámetro | Valor Recomendado | Justificación |
| :--- | :--- | :--- |
| **Mode** | `Cumulative` | Ver el total de contratos abiertos. |
| **Alerts** | `False` | No sonarán por falta de datos. |

---

### 🧪 Notas de desarrollo

* Código estándar y robusto.
* Visualización `ByBar` depende de cambios tick-by-tick que no existen en CME.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Visualización de barras filtradas (las pinta planas en vez de ocultarlas).

---

### 🛠️ Propuestas de mejora

* Ninguna.

---

### 💎 Valor Reutilizable (Código Donante)

* **Sistema de Alertas:** Extraer la lógica `ChangeSize` para otros indicadores de volumen.

---

### ✍️ La opinión de Gemini sobre el Indicador

Guárdalo como referencia. Es útil tener el dato del "Open Interest Total" en un gráfico diario (D1) para saber la salud del contrato de futuros (Rollover, vencimientos, etc.), pero quítalo de tus gráficas de 1 minuto.

**Propuestas de Acción:**
* **Conservar** para workspaces de análisis D1.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**No.**

**Acción:** **Conservar (Reserva)**
