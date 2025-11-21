---
cs_file: MaxLevels.cs
name: Maximum Levels
group: Order Flow
subgroup: Volume Profile
score_current: 8.5/10
version: Stable
recommended_action: Conservar (Core)
description: ¿En qué nivel de precio se produjo el máximo Volumen (o Bid, Ask, Delta) para el período seleccionado?
gemini_summary: "Detector de niveles clave estáticos. A diferencia del Dynamic Levels que muestra la evolución, este indicador marca el nivel GANADOR final de un periodo (ayer, semana pasada, mes pasado). Esencial para soportes/resistencias mayores."
comparison_group: "Session Profile"
competitor_notes: "Complemento estático al perfil dinámico."
reusable_code: null
file_state: Estable
score_potential: 8.5/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 Maximum Levels (8.5/10)

**Nombre del archivo:** [`MaxLevels.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MaxLevels.cs)  
**Nombre del indicador:** Maximum Levels  
**Web oficial:** [ATAS — Maximum Levels](https://help.atas.net/support/solutions/articles/72000602426)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿En qué nivel de precio se produjo el máximo Volumen (o Bid, Ask, Delta) para el período seleccionado?

![MaxLevels](../../img/MaxLevels.png)

---

### ⚙️ Parámetros configurables

* **Period:** Periodo de referencia (`CurrentDay`, `LastDay`, `LastWeek`, `Contract`, etc.).
* **Type:** Dato a buscar (`Volume`, `Bid`, `Ask`, `Delta`, `Tick`).
* **Trading Session:** Filtro de sesión específico.
* **Visuals:** Longitud de línea, texto, valor, colores.
* **Alerts:** Alerta al tocar el nivel.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Session Profile"  

---

### 🧠 Uso más frecuente

* **Niveles de Ayer:** Marcar el POC de ayer (`LastDay` + `Volume`) como soporte/resistencia clave para hoy.  
* **Niveles Semanales:** Marcar el POC de la semana pasada.  
* **Niveles de Delta:** Marcar dónde hubo la mayor agresión compradora/vendedora (`PositiveDelta` / `NegativeDelta`).  

---

### 📊 Nivel de relevancia
🔟 **8.5 / 10 (ESTRUCTURAL)**

✅ **Referencia Sólida:** Los niveles de máximo volumen son zonas de alta liquidez que el mercado tiende a respetar.  
✅ **Asíncrono:** Usa `FixedProfileRequest` para cargar datos pesados sin congelar el chart.  
✅ **Flexible:** Puede buscar máximos de Delta, lo cual es muy potente para ver zonas de "atrapados".  

---

### 🎯 Estrategias de scalping donde se aplica

* **Poc Bounce:** El precio suele rebotar en el primer test del POC de ayer.  
* **Delta Defense:** Si el precio vuelve al nivel de `MaxNegativeDelta` y rebota, los vendedores están defendiendo su posición.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Period** | `LastDay` | Referencia inmediata. |
| **Type** | `Volume` | POC clásico. |
| **Length** | `300` | Línea discreta a la derecha. |

---

### 🧪 Notas de desarrollo

* Implementación asíncrona ejemplar (`GetFixedProfile`).
* Renderizado manual eficiente en `OnRender` (Dibuja línea y etiqueta).

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** ---

### 🛠️ Propuestas de mejora

* **Ninguna.** ---

### 💎 Valor Reutilizable (Código Donante)

* **Solicitud de Perfiles:** El patrón de `FixedProfileRequest` es la forma correcta de pedir datos de volumen agregados al servidor de ATAS.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es simple pero vital. No quieres calcular el perfil de ayer a mano. Este indicador te pone la línea donde debe estar.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Los niveles históricos son imanes.

**Acción:** **Conservar (Core).**

