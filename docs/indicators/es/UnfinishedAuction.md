---
cs_file: UnfinishedAuction.cs
name: Unfinished Auction
category: Order Flow
group: Order Flow
subgroup: Volume
score_current: 9/10
version: Stable
recommended_action: Conservar
description: ¿Quedaron órdenes pendientes (desequilibrio) en los extremos de la vela
  que el precio debe volver a visitar?
gemini_summary: Herramienta de Order Flow avanzada. Detecta anomalías en extremos
  y dibuja líneas hasta que se mitigan.
file_state: Estable
score_potential: 9/10
effort: Alto
action_priority: N/A
analysis_date: 2025-11-18
official_code_date: 2025-05-8
---

## 🟦 Unfinished Auction (9/10)

**Nombre del archivo:** [`UnfinishedAuction.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/UnfinishedAuction.cs)  
**Nombre del indicador:** Unfinished Auction  
**Web oficial:** [ATAS — Unfinished Auction](https://help.atas.net/support/solutions/articles/72000602495)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 8/05/2025  

> **La Pregunta Clave:** ¿Quedaron órdenes pendientes (desequilibrio) en los extremos de la vela que el precio debe volver a visitar?

![UnfinishedAuction](../../img/UnfinishedAuction.png)

---

### ⚙️ Parámetros configurables

* **Filters**: BidFilter y AskFilter (Volumen mínimo para considerar la subasta inacabada).  
* **Days**: Historial a escanear.  
* **Visuals**: Colores y grosores de líneas y clústeres.  
* **Alerts**: Notificación al crear o cerrar una zona.  

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Indicador de microestructura de mercado (Market Profile Theory).

---

### 🧠 Uso más frecuente

* **Target Magnet:** Las líneas de "Unfinished Auction" actúan como imanes. El mercado odia la ineficiencia y suele volver para completar la subasta (0 volumen en el extremo).  
* **Soporte Débil:** Un mínimo con Unfinished Auction no es un buen soporte, es probable que se rompa o se testee de nuevo.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ **Teoría Robusta:** Basado en la teoría de subastas. Un extremo de mercado sólido debe tener volumen decreciente (0). Si hay volumen alto, la subasta no acabó.  
✅ **Gestión Automática:** Borra las líneas cuando el precio las toca (`HorizontalLinesTillTouch`), manteniendo el gráfico limpio de niveles viejos.  
⛔ **Requiere Tick Data:** No funciona con datos OHLC simples.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Repair Trade:** Si dejamos una subasta inacabada arriba y el precio baja, buscar largos para el "retest" de esa línea.  
* **Breakout Confirmation:** Si rompe un nivel y deja UA, es señal de fuerza, pero cuidado con el pullback inmediato.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Bid/Ask Filter**: `10` o `20` (Depende de la liquidez del horario).  

---

### 🧪 Notas de desarrollo

* **Lógica:** `candle.GetPriceVolumeInfo(High/Low)`. Verifica si en el tick exacto del máximo/mínimo hubo volumen de Ask/Bid superior al filtro.
* **Persistencia:** Usa `LineTillTouch` con `IsRay = true`. El sistema de ATAS se encarga de cortar la línea cuando el precio la cruza en el futuro.

---
---

### ✍️ La opinión de Gemini sobre el Indicador

Es una herramienta táctica de primer nivel. Para el trader de Order Flow, ver estas líneas es ver "asuntos pendientes" del mercado. El código es excelente y muy específico para la plataforma ATAS.

**Propuestas de Mejora:**
* Ninguna. Es una implementación canónica.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.** Define objetivos (Targets) de muy alta probabilidad.

**Acción:** **Conservar.**