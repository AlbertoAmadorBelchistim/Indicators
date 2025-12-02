---
# 1. IDENTIFICACIÓN
cs_file:  OrderBookAlerts.cs  
name:  Order Book Alerts  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  DOM  
comparison_group:  "DOM Liquidity Flow"  

# 3. VALORACIÓN (Score & Priority)
score_current:  9/10  
score_potential:  9/10  
file_state:  Estable  
effort:  N/A  
action_priority:  Nula  
system_priority:  P2  

# 4. DECISIÓN
recommended_action:  Conservar (Reserva)  

# 5. ANÁLISIS
description:  ¿Dónde hay muros de liquidez en el DOM que superan un cierto tamaño y persisten en el tiempo?  
gemini_summary:  "Monitor pasivo. Su valor no es analítico (flujo), sino de vigilancia (estado). Permite al trader desentenderse del DOM numérico y recibir avisos (sonoros/visuales) solo cuando aparece liquidez relevante y persistente (anti-spoofing) cerca del precio."  
competitor_notes:  "Complementario a DomDynamics. Este busca ESTADO (Nivel > X), DomDynamics busca CAMBIO (Delta > X)."  
reusable_code:  "Lógica de `TimeFilter` para validar la persistencia de una orden."  

# 6. METADATOS
analysis_date:  2025-12-02  
official_code_date:  2025-05-08  
---

## 🔔 Order Book Alerts (9/10)

**Nombre del archivo:** [`OrderBookAlerts.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OrderBookAlerts.cs)  
**Nombre del indicador:** Order Book Alerts  
**Web oficial:** [ATAS Help](https://help.atas.net/support/solutions/articles/72000619055)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 2025-05-08  

> **La Pregunta Clave:** ¿Dónde hay muros de liquidez en el DOM que superan un cierto tamaño y persisten en el tiempo?

![OrderBookAlerts](../../img/OrderBookAlerts.png)

---

### ⚙️ Parámetros configurables

* **Filter:** Volumen mínimo de la orden para activar la alerta.
* **TimeFilter:** Segundos que la orden debe permanecer activa sin desaparecer (Anti-Spoofing). Característica clave.
* **Price Offset:** Distancia máxima desde el precio actual (Ticks o %) para monitorizar. Evita alertas de niveles irrelevantes lejanos.
* **CoolDown Period:** Tiempo de espera entre alertas repetidas.
* **Show On Chart:** Dibuja una banda horizontal visual en el nivel detectado.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "DOM Liquidity Flow"  

---

### 🧠 Uso más frecuente

* **Radar de Liquidez:** Aviso sonoro cuando una "ballena" coloca una orden límite cerca del precio.
* **Zonas de Interés:** Marcar visualmente niveles donde hay interés institucional pasivo sin ensuciar el gráfico permanentemente.

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Automatización:** Libera atención visual del trader.  
✅ **Filtro Temporal:** El `TimeFilter` es la joya de este indicador. Elimina el 90% de las señales falsas causadas por HFT (Spoofing) que solo "flashean" órdenes.  
⛔ **Estático:** No te dice si están agrediendo el nivel, solo que el nivel *existe*.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Fade the Wall:** Si aparece una alerta de gran liquidez en resistencia y el precio llega exhausto, buscar cortos (usando la liquidez como Stop Loss).  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Filter**: `150`
* **POMode**: `Ticks`
* **PriceOffset**: `10`
* **TimeFilter.Enabled**: `true`, `Value`: `1`
* **CoolDownPeriod**: `3`
* **ShowOnChart**: `true`

---

### 🧪 Notas de desarrollo

* Se suscribe a `MarketDepthChanged`.
* Mantiene una lista interna `_priceInfos` con los niveles detectados y sus timestamps.
* Es muy ligero porque filtra por precio (`PriceOffset`) antes de procesar lógica pesada.

---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguno. Cumple su función de utilidad perfectamente.

---

### 🛠️ Propuestas de mejora

* Ninguna requerida.

---

### 💎 Valor Reutilizable (Código Donante)

* La implementación del `TimeFilter` (verificar `MarketTime - AppearanceTime`) es reusable para cualquier sistema de alertas que quiera evitar el ruido HFT.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta "set and forget". No la usas para analizar, la usas para que te avise. Es esencial en cualquier configuración profesional para no tener que estar mirando el DOM Ladder constantemente.

**Propuestas de Acción:**
* Configurar siempre con `TimeFilter = 1` segundo mínimo para evitar ruido.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí**

Alerta de obstáculos inmediatos (Soportes/Resistencias dinámicos).

**Acción:** **Conservar (Reserva)**

