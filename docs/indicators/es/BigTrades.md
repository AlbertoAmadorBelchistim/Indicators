---
cs_file: BigTrades.cs  
name: Big Trades  
version: ATAS Stable/Latest (Closed Source)  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Footprint  
comparison_group: "Big Trades Analysis"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8/10  
score_potential: 8/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P2  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Dónde aparecen operaciones grandes definidas por un umbral absoluto configurable?  
gemini_summary: "Es útil cuando quieres un criterio fijo (≥ X) para prints grandes, pero depende del régimen intradía: exige recalibración y puede saturarse o quedarse ciego."  
competitor_notes: "Pierde frente al adaptativo en robustez intradía. Gana en control absoluto y reproducibilidad conceptual (mismo X siempre)."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-23  
official_code_date: Unknown  
---

## 🟦 Big Trades (8/10)

**Nombre del archivo:** No disponible en código abierto.  
**Nombre del indicador:** Big Trades  
**Web oficial:** [ATAS — Big Trades](https://help.atas.net/support/solutions/articles/72000602332)  
**Compatibilidad:** ATAS Stable/Latest  
**Última revisión del código oficial:** Unknown  

> **La Pregunta Clave:** ¿Dónde aparecen operaciones grandes definidas por un umbral absoluto configurable?  

![BigTrades](../../img/BigTrades.png)  


---

### ⚙️ Parámetros configurables

#### [Cálculo / Filtro]  
- **Min Volume**: umbral mínimo absoluto (parámetro crítico).  
- **Max Volume**: techo opcional (si existe; depende de la implementación cerrada).  
- **Mode (Cumulative vs Separate)**:  
  - Agregado: menos ruido.  
  - Separado por ticks: más granularidad pero más carga y riesgo de sobre-señal.  

#### [Filtro por ubicación en vela]  
- **Location**: permite restringir señales a alto/bajo/cuerpo/mechas u otras combinaciones, reduciendo ruido y enfocando “zonas de decisión”.  

#### [Filtro horario]  
- **Time From / Time To**: útil para limitar a apertura o tramos concretos.  

#### [Visualización / Alertas]  
- **Object Size / Show Value**: legibilidad y densidad.  
- **Alerts**: útil para entrenamiento, pero puede saturar en ES M1.  


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Big Trades Analysis"  


---

### 🧠 Uso más frecuente

* Definir un umbral fijo para “prints institucionales” (≥ X) y observar su distribución.  
* Confirmación de ruptura o absorción con criterio absoluto.  
* Alertas duras para “evento” (si buscas triggers mecánicos).  


---

### 📊 Nivel de relevancia
🔟 **8 / 10**  

✅ Control absoluto: útil si tu sistema define “grande” como ≥ X contratos.  
✅ Location filter aporta mucha precisión para lectura discrecional.  
⛔ Dependiente del régimen: MinVolume requiere recalibración intradía o por sesión.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Reversal en extremo:** prints grandes en mínimo/máximo + falta de continuación.  
* **Momentum:** prints grandes encadenados rompiendo nivel.  
* **Filtro de intento de ruptura:** Location restrictivo para ver prints solo en zonas clave.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Grupo | Parámetro | Valor recomendado | Justificación operativa |  
|---|---:|---:|---|  
| Cálculo | Mode | Agregado | Menos ruido y lectura más rápida en M1. |  
| Cálculo | Min Volume | 50–120 | Ajustar por sesión; apertura suele requerir valores más altos. |  
| Ubicación | Location | Any / AtHighOrLow | General vs setups de ruptura/reversión en extremos. |  
| Visualización | Fixed Size | true | Evita que prints extremos tapen el precio. |  
| Alertas | Alerts | false | Evita fatiga por alertas; úsalo solo en entrenamiento. |  


---

### 🧪 Notas de desarrollo

* El enfoque de umbral fijo es conceptualmente simple y útil para reglas absolutas, pero su efectividad en scalping intradía depende del régimen de volumen.  
* Como reserva, su papel ideal es complementar al adaptativo cuando necesitas “un X fijo” por diseño de sistema o para comparar sesiones entre sí.  


---

### ❗ Incoherencias o aspectos mejorables detectados

* Código cerrado: no auditable completamente; validación práctica recomendada mediante pruebas A/B en chart frente a alternativas.  
* Riesgo operativo: si MinVolume no está calibrado, el indicador puede inducir a error (falsos negativos o saturación).  


---

### 🛠️ Propuestas de mejora

* Añadir presets por tramo horario (apertura/medio/cierre) si el indicador lo permite.  


---

### 💎 Valor Reutilizable (Código Donante)

* Al ser código cerrado no puede reutilizarse nada.  


---

### ✍️ La opinión de ChatGPT sobre el Indicador

Tiene utilidad como detector absoluto y como herramienta de estudio, pero en ejecución M1 tiende a ser superado por el adaptativo. Mantenerlo como P2 (reserva) es razonable.  


---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí**  

Útil si tu operativa requiere umbrales absolutos o si lo usas como complemento y no como único criterio.  

**Acción:** **Conservar (Reserva)**   
