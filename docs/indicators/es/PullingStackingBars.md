---
# 1. IDENTIFICACIÓN
cs_file:  PullingStackingBars.cs  
name:  Pulling & Stacking Bars Lite  
version:  Atas Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  DOM  
comparison_group:  "DOM Liquidity Flow"  

# 3. VALORACIÓN (Score & Priority)
score_current:  8/10  
score_potential:  8/10  
file_state:  Estable  
effort:  N/A  
action_priority:  Baja  
system_priority:  P3  

# 4. DECISIÓN
recommended_action:  Conservar (Reserva)  

# 5. ANÁLISIS
description:  ¿Qué lado (Bid/Ask) está añadiendo (Stacking) o quitando (Pulling) órdenes y en qué magnitud exacta?  
gemini_summary:  "Versión 'microscópica' del flujo de liquidez. Separa los cambios del DOM en 4 categorías: Bid Add, Bid Remove, Ask Add, Ask Remove. Es extremadamente potente para análisis forense del mercado, pero visualmente abrumador para scalping rápido comparado con DomDynamics. El código es decompilado y muy complejo, difícil de mantener."  
competitor_notes:  "Más detallado que DomDynamics, pero menos usable en tiempo real. Se guarda como herramienta de análisis profundo."  
reusable_code:  "Lógicas de filtrado complejo (`FilterValueCalculator`)."  

# 6. METADATOS
analysis_date:  2025-12-02  
official_code_date:  Unknown  
---

## 🔬 Pulling & Stacking Bars Lite (8/10)


**Nombre del indicador:** Pulling & Stacking Bars Lite  
**Web oficial:** [Momentum Trading](https://momentumtradingeu.com/dom-pulling-stacking/)  
**Compatibilidad:** ATAS Stable (Requiere L2 Data).  
**Última revisión del código oficial:** Unknown  

> **La Pregunta Clave:** ¿Qué lado (Bid/Ask) está añadiendo (Stacking) o quitando (Pulling) órdenes y en qué magnitud exacta?

![PullingStackingBars](../../img/PullingStackingBars.png)


---

### ⚙️ Parámetros configurables

* **Limit Filters (Min/Max):** Filtros separados para Bid Stacking, Bid Pulling, Ask Stacking, Ask Pulling. Permite aislar solo grandes movimientos.
* **Colors:** Configuración individual de color para cada uno de los 4 estados.
* **Combine Mode:** Opción para sumar valores o verlos separados.


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "DOM Liquidity Flow"  


---

### 🧠 Uso más frecuente

* **Análisis de Manipulación:** Ver claramente si una subida de precio está apoyada por "Bid Stacking" (real) o "Ask Pulling" (dejar paso, posible trampa).
* **Defensa de Niveles:** Identificar si en un soporte están recargando órdenes (Stacking) agresivamente.


---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ **Granularidad Total:** Nada se esconde. Ves exactamente quién mueve las fichas.  
✅ **Filtros Avanzados:** Puedes ignorar el pulling y ver solo el stacking, por ejemplo.  
⛔ **Sobrecarga Cognitiva:** Requiere interpretar 2 histogramas dobles (arriba y abajo) simultáneamente.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Order Flow Drill-down:** Cuando el mercado está lento y rotacional, usar este indicador para ver quién domina la liquidez pasiva.  


---

### 🧪 Notas de desarrollo

* Código cerrado.


---

### ❗ Incoherencias o aspectos mejorables detectados

Código cerrado.


---

### 🛠️ Propuestas de mejora

* Ninguna.


---

### 💎 Valor Reutilizable (Código Donante)

Código cerrado.


---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta de "cirujano". Demasiado detalle para la guerra diaria del scalping (donde DomDynamics gana), pero invaluable para entender *por qué* falló un nivel o *cómo* se construyó una ruptura.

**Propuestas de Acción:**
* Mantener en la carpeta de "Herramientas de Análisis" pero fuera del template de ejecución rápida.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (Condicional)**

Útil para traders experimentados en DOM que necesitan ver el desglose total.

**Acción:** **Conservar (Reserva)**