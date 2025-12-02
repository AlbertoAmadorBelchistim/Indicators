---
# 1. IDENTIFICACIÓN
cs_file:  OrderBookAlerts.cs  
name:  Order Book Alerts  
version:  ATAS Stable  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  DOM  
comparison_group:  "DOM Dynamics"  

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
gemini_summary:  "Monitor pasivo de excelente utilidad. Su valor no es analítico, sino de vigilancia. Permite al trader desentenderse del DOM numérico y recibir avisos (sonoros/visuales) solo cuando aparece liquidez relevante y persistente cerca del precio."  
competitor_notes:  "Complementario a DomDynamics. Este busca ESTADO (Nivel > X), DomDynamics busca CAMBIO (Delta > X)."  
reusable_code:  null  

# 6. METADATOS
analysis_date:  2025-12-01  
official_code_date:  2025-05-08  
---

## 🟦 [Order Book Alerts] (9/10)

**Nombre del archivo:** [`OrderBookAlerts.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OrderBookAlerts.cs)  
**Nombre del indicador:** Order Book Alerts  
**Web oficial:** [ATAS Help](https://help.atas.net/support/solutions/articles/72000619055)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 8/05/2025  

> **La Pregunta Clave:** ¿Dónde hay muros de liquidez en el DOM que superan un cierto tamaño y persisten en el tiempo?

![OrderBookAlerts](../../img/OrderBookAlerts.png)


---

### ⚙️ Parámetros configurables

* **Filter:** Volumen mínimo de la orden para activar la alerta.  
* **TimeFilter:** Segundos que la orden debe permanecer activa (anti-spoofing básico, característica clave).  
* **Price Offset:** Distancia máxima desde el precio actual para vigilar (Ticks o %).  
* **CoolDown Period:** Tiempo de espera entre alertas repetidas.  
* **Use Alerts / Alert File:** Configuración de sonido.  
* **Show On Chart:** Dibuja una banda horizontal en el nivel detectado.  


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "DOM Dynamics"  


---

### 🧠 Uso más frecuente

* **Radar de Liquidez:** Aviso sonoro cuando una "ballena" coloca una orden límite cerca del precio.  
* **Zonas de Interés:** Marcar visualmente niveles donde hay interés institucional pasivo sin ensuciar el gráfico permanentemente.  


---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Automatización:** Libera atención visual.  
✅ **Filtro Temporal:** El `TimeFilter` filtra el parpadeo HFT que suele generar falsas alarmas en otros indicadores.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Entrada por aparición de muro de órdenes** en zona de interés
* **Confirmación de soporte/resistencia** con alerta visual + volumen
* **Evitar trades en zonas con alta presión pasiva detectada**

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

* Se suscribe a `MarketDepthChanged` para recibir actualizaciones del DOM
* Mantiene una lista interna `_priceInfos` con los niveles detectados
* Implementa lógica de `TimeFilter` (persistencia) y `CoolDownPeriod` (anti-spam)
* Dibuja rectángulos en el gráfico (`OnRender`) en los niveles de precio detectados

---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta excelente para traer la información del DOM al gráfico. Muchos scalpers no pueden mirar el DOM y el gráfico simultáneamente; este indicador resuelve eso alertando visualmente sobre la liquidez pasiva.

El código es eficiente y maneja bien la alta frecuencia de actualizaciones del DOM. La función `TimeFilter` es crucial para filtrar el "spoofing" (órdenes falsas que aparecen y desaparecen rápidamente).

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

La liquidez pasiva actúa como un imán o como una barrera. Saber dónde está sin tener que abrir el DOM es una gran ventaja.

**Acción:** **Conservar (Utilidad de DOM).**

