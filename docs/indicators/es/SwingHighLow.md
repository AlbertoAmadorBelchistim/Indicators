---

# 1. IDENTIFICACIÓN  
cs_file: SwingHighLow.cs  
name: Swing High and Low  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Swing-Derived Structure  
comparison_group: "Swing-Derived Structure"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 7/10  
score_potential: 7/10  
file_state: Estable  
effort: Bajo  
action_priority: Nula  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Cuáles son los puntos de giro confirmados (swing highs/lows) con un periodo configurable y alertas opcionales?  
gemini_summary: "Detector de swings confirmado con lag (correcto por diseño) y alertas. Es útil, pero queda por detrás de Fractals+ShowLine como generador de niveles operables persistentes."  
competitor_notes: "Más configurable que Fractals (Period e igualdad), pero no traza líneas. Aporta alertas, por eso se mantiene como reserva."  
reusable_code: "Uso de Highest/Lowest internos para validación de pivotes y patrón de alertas con deduplicación por barra."  

# 6. METADATOS  
analysis_date: 2025-12-28  
official_code_date: 2025-04-23  




---

## 🟦 Swing High and Low (7/10)

**Nombre del archivo:** [`SwingHighLow.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/SwingHighLow.cs)  
**Nombre del indicador:** Swing High and Low  
**Web oficial:** [ATAS — Swing High and Low](https://help.atas.net/support/solutions/articles/72000602483)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuáles son los puntos de giro confirmados (swing highs/lows) con un periodo configurable y alertas opcionales?  

![SwingHighLow](../../img/SwingHighLow.png)



---

### ⚙️ Parámetros configurables

* **Period**: Profundidad del swing (barras a cada lado para confirmación).  
* **IncludeEqual**: Considera máximos/mínimos iguales como swing.  
* **UseAlerts**: Activar alertas al confirmarse swing.  
* **AlertFile**: Sonido de alerta.  
* **FontColor / BackgroundColor**: Estilo visual de la alerta.  



---

### 🧭 Clasificación
**Grupo:** Market Structure  
**Subgrupo:** Swing-Derived Structure  
**Comparison Group:** "Swing-Derived Structure"  



---

### 🧠 Uso más frecuente

* Marcar pivotes confirmados para BOS/CHOCH manual.  
* Alertas cuando se confirma swing (útil en multitarea).  
* Identificación de últimos swings para stops/invalidación.  



---

### 📊 Nivel de relevancia
🔟 **7 / 10**

✅ Pivote confirmado (no “señal fantasma”), lag honesto.  
✅ Incluye alertas y opción de igualdad, práctico.  
⛔ No genera líneas de nivel; menos accionable que Fractals+ShowLine.  
⛔ Puede sentirse redundante si ya trabajas con fractales como niveles.  



---

### 🎯 Estrategias de scalping donde se aplica

* **BOS**: trazar manualmente el último swing confirmado y operar ruptura.  
* **Stops técnicos**: swing low/high como invalidación estructural.  



---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor recomendado | Justificación |
|---|---:|---|
| Period | 3–5 | 3 agresivo; 5 más estándar y filtra ruido. |
| IncludeEqual | true | Evita perder pivotes por igualdad en rangos. |
| UseAlerts | opcional | Útil si no puedes mirar continuamente. |  



---

### 🧪 Notas de desarrollo

* Usa indicadores internos `Highest` y `Lowest` sobre high/low para validar pivotes.  
* Dibuja flechas en `calcBar = bar - Period` y alerta solo cuando barra actual es la última (`bar == CurrentBar - 1`).  
* Deduplica alertas con `_lastHighAlert` / `_lastLowAlert`.  



---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguna crítica; el lag es inherente a confirmar swing.  



---

### 🛠️ Propuestas de mejora

* Opción de dibujar líneas horizontales desde el swing hasta mitigación (lo acercaría a Fractals+ShowLine).  



---

### 💎 Valor Reutilizable (Código Donante)

* Patrón de alertas con deduplicación por barra y parámetros de estilo.  



---

### ✍️ La opinión de ChatGPT sobre el Indicador

Buen detector de pivotes confirmados con alertas, pero si tu objetivo principal es un mapa de niveles operables, Fractals con líneas hasta mitigación es superior. Aun así, se justifica como reserva porque aporta alertas y configuración de sensibilidad.  



---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (como reserva)**  

**Acción:** **Conservar (Reserva)**  
