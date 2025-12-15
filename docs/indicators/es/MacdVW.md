---
# 1. IDENTIFICACIÓN  
cs_file: MacdVW.cs  
name: MACD - Volume Weighted  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume  
comparison_group: "Volume Oscillators"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 7.5/10  
score_potential: 9/10  
file_state: Estable  
effort: Bajo  
action_priority: Baja  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Cuál es la convergencia entre dos medias ponderadas por volumen y su señal suavizada para filtrar tendencia con participación?  
gemini_summary: "MACD basado en VWMA: pondera el precio por volumen, mejora el MACD clásico en calidad de señal, pero mantiene lag. Más útil como confirmador tendencial que como herramienta de entrada en M1."  
competitor_notes: "Mejor construcción que TVI y más 'tendencial' que Weis Wave, pero aporta menos valor marginal que el CORE (ratio normalizado) en scalping puro."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-15  
official_code_date: 2025-04-23  

  

---  

## 🛡️ MACD - Volume Weighted (7.5/10)  

**Nombre del archivo:** [`MacdVW.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MacdVW.cs)  
**Nombre del indicador:** MACD - Volume Weighted  
**Web oficial:** [ATAS — MACD - Volume Weighted](https://help.atas.net/support/solutions/articles/72000602231)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es la convergencia entre dos medias ponderadas por volumen y su señal suavizada para filtrar tendencia con participación?  

![MacdVW](../../img/MacdVW.png)  

  

---  

### ⚙️ Parámetros configurables  

- **Period:** periodo de la señal EMA (default 9).   
- **Short Period:** ventana corta de VWMA (default 12).   
- **Long Period:** ventana larga de VWMA (default 26).   

  

---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Oscillators"  

  

---  

### 🧠 Uso más frecuente  

* Confirmación de tendencia: mantener sesgo mientras `MACD_VW` permanezca del lado del trade y la señal acompañe.  
* Divergencias (con cautela): menor frecuencia pero más “filtradas” que MACD clásico por el peso del volumen.   

  

---  

### 📊 Nivel de relevancia  
🔟 **7.5 / 10**  

✅ Filtra movimientos de baja participación al ponderar por volumen.  
✅ Implementación segura: guard ante sumas de volumen cero.  
⛔ Sigue siendo un MACD: lag inevitable y menor utilidad como trigger en M1 puro.  

  

---  

### 🎯 Estrategias de scalping donde se aplica  

* **Trend-following intradía:** usarlo como filtro (solo longs si `MACD_VW > 0`).  
* **Pullback continuation:** entrada por precio/OF, confirmación por MACD_VW recuperando el lado correcto.  

  

---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| Short Period | 8 | Reduce lag y se adapta a M1. |  
| Long Period | 21 | Estructura tendencial corta sin “sobre-suavizar”.|  
| Period | 9 | Señal estándar suficientemente estable.|  

  

---  

### 🧪 Notas de desarrollo  

* Construye dos VWMAs como `Sum(Price * Volume) / Sum(Volume)` en ventana corta y larga; la diferencia es el MACD.  
* La señal es una EMA del MACD calculada con `Period`.  
* En `OnRecalculate` limpia series internas (`_vol`, `_valVol`) y data series.  

  

---  

### ❗ Incoherencias o aspectos mejorables detectados  

* Visual: histograma monocromático (no impacta cálculo, sí lectura rápida).  

  

---  

### 🛠️ Propuestas de mejora  

* **P3 (Baja):** coloreado por pendiente (sube/baja) o por signo (positivo/negativo) para lectura instantánea.  

  

---  

### 💎 Valor Reutilizable (Código Donante)  

* Patrón de cálculo VWMA mediante series auxiliares `_valVol` y `_vol` (útil para otros indicadores volume-weighted).  

  

---  

### ✍️ La opinión de ChatGPT sobre el Indicador  

Es una mejora real sobre el MACD clásico, pero en un set de scalping basado en Order Flow su rol natural es **filtro tendencial**, no trigger. Si tu pantalla ya está “ocupada” por métricas más directas (delta/ratio/tape speed), este se convierte en una reserva valiosa para días de tendencia clara o para traders que prefieren confirmaciones clásicas.  

  

---  

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí, como filtro (Reserva).**  

Aporta confirmación tendencial ponderada por volumen, pero no sustituye herramientas de ejecución.  

**Acción:** **Conservar (Reserva)**  

