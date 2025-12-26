---

# 1. IDENTIFICACIÓN  
cs_file: VWAP.cs  
name: VWAP / TWAP  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume  
comparison_group: "VWAP & Benchmarks"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 10/10  
score_potential: 10/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Cuál es el precio medio ponderado por volumen (benchmark institucional) y sus desviaciones relevantes durante un periodo definido o anclado?  
gemini_summary: "Benchmark institucional de referencia absoluta. Implementación completa de VWAP/TWAP con reinicio por periodos, desviaciones estándar múltiples y Anchored VWAP interactivo. Código robusto, eficiente y de calidad profesional."  
competitor_notes: "Grupo singleton temporal. VWAP establece el estándar frente al cual competirán futuros benchmarks (Anchored VWAP dedicados, Rolling VWAP, Session VWAP, TWAP independientes)."  
reusable_code: "Sistema de cálculo incremental ponderado, gestión de reinicios por periodo, lógica de Anchored VWAP mediante teclas y control avanzado de fondos/bandas."  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-05-08  

---

## 🏆 VWAP / TWAP (10/10)  

**Nombre del archivo:** [`VWAP.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VWAP.cs)  
**Nombre del indicador:** VWAP / TWAP  
**Web oficial:** [ATAS — VWAP / TWAP](https://help.atas.net/support/solutions/articles/72000602503)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-05-08  

> **La Pregunta Clave:** ¿Cuál es el precio medio ponderado por volumen (benchmark institucional) y sus desviaciones relevantes durante un periodo definido o anclado?  

![VWAP](../../img/VWAP.png)


---

### ⚙️ Parámetros configurables  

**[Settings]**  
- **Type (Period):** Define el reinicio del cálculo.  
  - `Daily`, `Weekly`, `Monthly`, `Custom`, `All`, intradía (`M15`, `M30`, `Hourly`, `H4`).  
- **Mode:**  
  - `VWAP`: ponderado por volumen (estándar institucional).  
  - `TWAP`: ponderado por tiempo (útil en baja liquidez o ejecución algorítmica).  
- **VolumeType:** Fuente de volumen (`Total`, `Bid`, `Ask`).  
- **ShowFirstPeriod:** Muestra u oculta el primer periodo parcial.  
- **VWAPOnly:** Oculta bandas y rellenos, dejando solo la línea base.  

**[Deviation Bands]**  
- **StDev / StDev1 / StDev2:** Multiplicadores de desviación estándar (1σ, 2σ, 3σ).  
- Bandas superiores e inferiores con rellenos configurables e independientes.  

**[Anchored VWAP / Custom Start]**  
- **AllowCustomStartPoint:** Activa el VWAP anclado manualmente.  
- **StartKey / DeleteKey:** Teclas para fijar (`F`) y borrar (`G`) el punto de inicio.  
- **SavePoint / ResetOnSession:** Persistencia y reinicio del anclaje por sesión.  

**[Visualization]**  
- **ColoredDirection:** Colorea la línea según pendiente (bullish/bearish).  
- **BullishColor / BearishColor:** Colores direccionales personalizados.  
- Control avanzado de fondos (`RangeDataSeries`) por zonas estadísticas.  


---

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "VWAP & Benchmarks"  


---

### 🧠 Uso más frecuente  
- **Fair Value institucional:** referencia central de equilibrio intradía.  
- **Mean Reversion:** extensiones a ±2σ / ±3σ tienden a revertir hacia VWAP.  
- **Trend Context:** aceptación sostenida por encima/debajo del VWAP define sesgo direccional.  
- **Anchored Analysis:** anclar el VWAP a eventos clave (high/low, noticias, apertura).  


---

### 📊 Nivel de relevancia  
🔟 **10 / 10**  

✅ Benchmark institucional real (no lag, no suavizado artificial).  
✅ Implementación completa: VWAP, TWAP, desviaciones y anclaje manual.  
⛔ Ninguna debilidad técnica relevante detectada.  


---

### 🎯 Estrategias de scalping donde se aplica  
- **VWAP Bounce:** primer test del VWAP tras extensión direccional.  
- **Band Fade:** fade en ±3σ con objetivo en ±1σ o VWAP.  
- **Trend Pullback:** entradas en pullback hacia VWAP en tendencias limpias.  
- **Anchored Defense:** defensa del VWAP anclado tras impulsos institucionales.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| :--- | :--- | :--- |  
| **Type** | `Daily` | Contexto intradía estándar. |  
| **Mode** | `VWAP` | Benchmark institucional real. |  
| **StDev** | `1.0` | Zona de valor (≈68%). |  
| **StDev1** | `2.0` | Extensión operable (≈95%). |  
| **StDev2** | `3.0` | Zona extrema / clímax. |  
| **VWAPOnly** | `false` | Mantener bandas para contexto. |  


---

### 🧪 Notas de desarrollo  
- Cálculo incremental eficiente usando acumuladores (`totalVolume`, `totalVolToClose`).  
- Varianza ponderada correctamente implementada para VWAP (no aproximación).  
- Gestión avanzada de reinicios por periodo y sesiones custom.  
- Sistema de fondos duplicados (`Res`) para alternar visibilidad sin recalcular.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
- **Ninguna relevante.** Código estable, limpio y de referencia.  


---

### 🛠️ Propuestas de mejora  
- (Opcional) Extraer Anchored VWAP como módulo independiente si en el futuro se desea un torneo específico de anclajes.  


---

### 💎 Valor Reutilizable (Código Donante)  
- Lógica de Anchored VWAP mediante `FilterKey` y `BarFromDate`.  
- Implementación genérica de bandas estadísticas con `RangeDataSeries`.  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
VWAP es el eje gravitacional del mercado. En un sistema de scalping profesional, no es un “indicador más”, sino el benchmark contra el que se mide todo lo demás. La implementación de ATAS es de nivel institucional y no presenta deuda técnica relevante.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí.**  

Es la referencia central de equilibrio y extensión del precio.  

**Acción:** **Conservar (Core)**  

