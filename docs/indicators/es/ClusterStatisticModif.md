---
# 1. IDENTIFICACIÓN  
cs_file: ClusterStatisticModif.cs  
name: Cluster Statistic Modif  
version: Custom v1.1.0  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Footprint  
comparison_group: "Cluster Analysis"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 9/10  
score_potential: 10/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P2  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Cuál es el dashboard estadístico completo de cada vela (volumen, delta, velocidad e imbalances) para validar si el movimiento es ignición, absorción o agotamiento?  
gemini_summary: "Es el mejor validador del grupo: aporta contexto y confirmación (velocidad pico, delta en el pico, imbalances y stacked) pero no selecciona setups por filtros como ClusterSearchModif."  
competitor_notes: "Pierde el CORE frente a ClusterSearchModif por menor accionabilidad directa (es panel de validación). Supera a Absorption porque mide muchas más variables."  
reusable_code: "Cálculo SoT histórico + tiempo real (ventana deslizante) y publicación monotónica de picos por barra; lógica de buy/sell/net/stacked imbalances."  

# 6. METADATOS  
analysis_date: 2025-12-24  
official_code_date: 2025-04-23  
user_modification_date: 2025-10-01  
---

## 🟦 Cluster Statistic Modif (9/10)  

**Nombre del archivo:** [ClusterStatisticModif.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/ClusterStatisticModif.cs)  **Nombre del indicador:** Cluster Statistic Modif  
**Nombre del indicador:** Cluster Statistic Modif
**Web oficial (base):** [ATAS — Cluster Statistic](https://help.atas.net/en/support/solutions/articles/72000602624-cluster-statistics)  
**Compatibilidad:** ATAS Stable/Latest (requiere compilación en fork).  
**Última revisión del código oficial** [`ClusterStatistic.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ClusterStatistic.cs): 2025-04-23  
**Última revisión del código modificado:** 2025-10-01  

> **La Pregunta Clave:** ¿Cuál es el dashboard estadístico completo de cada vela (volumen, delta, velocidad e imbalances) para validar si el movimiento es ignición, absorción o agotamiento?  

![Imagen](../../img/ClusterStatistics.png)  


---  

### ⚙️ Parámetros configurables  

#### Filas (Rows)  
- **Trades / Ticks / Volume / Bid / Ask / Delta**: métricas base por vela.  
- **Delta/seg**: agresión neta normalizada por tiempo.  
- **Max Vol/seg (peak)**: pico de velocidad intravela.  
- **Delta at peak / Delta/Vol at peak**: calidad del pico (eficiencia).  
- **Buy / Sell / Net Imbalances**: desequilibrios diagonales básicos por vela.  
- **Stacked Imbalances**: consecutividad (presión sostenida).  
- **Session Volume / Delta / Delta/Vol**: acumulados de sesión.  

#### Max vol/seg (Speed of Tape)  
- **Time Window (sec.)**: tamaño de ventana móvil para medir velocidad.  
- **Min Volume per Window**: filtro de ruido (evita picos falsos).  
- **Use Auto Filter / Period / EMA vs SMA**: heatmap adaptativo según contexto reciente.  

#### Imbalance  
- **Imbalance Threshold (%)**: ratio mínimo (p. ej. 300% = 3:1).  
- **Imbalance Volume Filter**: diferencia mínima para considerar imbalance.  

#### Visualización / Texto / Encabezados  
- Colores, transparencia, degradados, fuente, alineación y formato de ratios.  

#### Alertas  
- Umbrales y sonidos para volumen/delta/net imbalance (según filas activas).  


---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Cluster Analysis"  


---  

### 🧠 Uso más frecuente  
* Validar si una vela es **ignición** (alta velocidad) o **absorción** (alto volumen, baja velocidad o delta inconsistente).  
* Confirmar setups detectados por ClusterSearchModif mediante **picos de velocidad** y **delta en el pico**.  
* Diagnosticar agotamiento: velocidad extrema con falta de continuidad o divergencias.  


---  

### 📊 Nivel de relevancia  
🔟 **9 / 10**  

✅ Contexto superior: velocidad pico + delta en pico aporta una capa que el volumen total no ofrece.  
✅ Imbalances y stacked integrados en el mismo panel para confirmación rápida.  
⛔ No “busca” setups: es confirmador, no selector. Requiere saber qué mirar y qué filas ocultar.  


---  

### 🎯 Estrategias de scalping donde se aplica  
* **Breakout real**: Max Vol/seg alto + Delta at peak alineado + Net/Stacked coherentes.  
* **Absorción en nivel**: volumen alto con delta débil o contrario + imbalances “contra” el avance.  
* **Clímax**: pico extremo de velocidad con divergencia posterior.  


---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| Rows (activar) | Delta, Delta/seg, Delta/Vol, Max Vol/seg, Delta at peak, Delta/Vol at peak, Net Imb, Stacked Imb | Maximiza señal/ruido para ejecución. |  
| Time Window (sec) | 5 | Captura ignición sin “suavizar” demasiado en M1. |  
| Min Volume per Window | 150 | Filtra micro-ruido. |  
| Use Auto Filter | true | Heatmap adaptativo a la sesión. |  
| Auto Filter Period | 3 (EMA) | Reacción rápida en intradía. |  
| Imbalance Threshold | 300 | Estándar 3:1. |  
| Imbalance Volume Filter | 30 | Evita falsos positivos. |  


---  

### ✨ Mejoras introducidas (Oficial/Base)  
Ninguna.


---  

### ✨ Mejoras añadidas (Custom)  
* **SoT (Speed of Tape)**: picos intravela (vol/seg y delta/seg) con publicación robusta por barra.  
* **Imbalances + stacked** integrados para lectura rápida sin cambiar de herramienta.  
* Protección de UI/series: comportamiento monotónico del pico por barra (no “baila” hacia abajo).  


---  

### 🧪 Notas de desarrollo  
* SoT histórico: request/iteración de cumulatives para reconstrucción por barra.  
* SoT en vivo: ventana deslizante en cola, con picos publicados y sin sobreescritura si el pico previo es mayor.  
* Imbalances: comparación diagonal upper Ask vs lower Bid, con conteo y stacking por consecutividad.  


---  

### ❗ Incoherencias o aspectos mejorables detectados  
* El cálculo histórico SoT puede ser costoso si se abre demasiado histórico; conviene limitar ventanas (sesión/RTH) o caching adicional.  


---  

### 🛠️ Propuestas de mejora  
* Añadir alertas por “eventos compuestos” (pico de velocidad + delta at peak + stacked) para reducir carga cognitiva.  
* Presets de filas (Ignición / Absorción / Clímax) para alternar rápidamente.  


---  

### 💎 Valor Reutilizable (Código Donante)  
* Patrón de ventana deslizante + picos monotónicos por barra.  
* Lógica de imbalances/stacked embebible en otros indicadores.  


---  

### ✍️ La opinión de ChatGPT sobre el Indicador  
Es el validador ideal para tu sistema: cuando ClusterSearchModif te señala un candidato, aquí decides si es impulso real, absorción o agotamiento. Su mayor riesgo es el exceso de filas; bien parametrizado, es una ventaja clara.  


---  

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí.**  

No es CORE por selección, pero es P2 por confirmación y contexto.  

**Acción:** **Conservar (Reserva)**  
