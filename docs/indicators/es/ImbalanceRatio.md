---
# 1. IDENTIFICACIÓN  
cs_file: ImbalanceRatio.cs  
name: Imbalance Ratio  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Footprint  
comparison_group: "Imbalance Analysis"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 9/10  
score_potential: 9/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Dónde aparecen desequilibrios diagonales (Ask vs Bid / Bid vs Ask) que superan un ratio mínimo y un filtro de volumen?  
gemini_summary: "Implementación canónica y eficiente del imbalance diagonal en Footprint. Señal clara, configurable y con un resumen visual (NxM) que reduce la carga cognitiva en M1."  
competitor_notes: "Gana frente a Stacked Imbalance por ser más directo, más ligero y menos propenso a errores lógicos; Stacked añade persistencia (líneas) pero introduce riesgos de índices y omite el flush final."  
reusable_code: "PriceSelectionDataSeries + PriceSelectionValue (OnlyCluster) como patrón reutilizable para resaltados por nivel de precio."  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-05-27  
---  

## 🏆 Imbalance Ratio (9/10)  

**Nombre del archivo:** [`ImbalanceRatio.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ImbalanceRatio.cs)  
**Nombre del indicador:** Imbalance Ratio  
**Web oficial:** [ATAS — Imbalance Ratio](https://help.atas.net/support/solutions/articles/72000602404)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-05-27  

> **La Pregunta Clave:** ¿Dónde aparecen desequilibrios diagonales (Ask vs Bid / Bid vs Ask) que superan un ratio mínimo y un filtro de volumen?  

![Imbalance Ratio](../../img/ImbalanceRatio.png)


---  

### ⚙️ Parámetros configurables  

#### Settings  
- **Ratio**: Umbral de desequilibrio. Se evalúa como:  
  - Compra: `upperInfo.Ask / lowerInfo.Bid >= Ratio`  
  - Venta: `lowerInfo.Bid / upperInfo.Ask >= Ratio`  
- **VolumeFilter**: Filtro mínimo de volumen agregado del par de ticks (`lowerInfo.Volume + upperInfo.Volume`). Evita señales por microvolumen.  

#### Visualization  
- **BuyColor / SellColor**: Color del bloque resaltado (modo Footprint).  
- **TextColor**: Color del texto del contador `buyRows x sellRows`.  
- **Transparency**: Opacidad del resaltado (0–100).  
- **ShowTop / ShowBot**: Muestra el bloque-resumen por vela:  
  - Si `candle.Delta >= 0` dibuja abajo (zona inferior).  
  - Si `candle.Delta < 0` dibuja arriba (zona superior).  


---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Imbalance Analysis"  


---  

### 🧠 Uso más frecuente  
* Confirmar iniciativa (agresión) en ruptura: múltiples imbalances a favor cerca del nivel.  
* Detectar “trapped traders”: imbalance fuerte en extremo + vela que rechaza (cierre contrario).  
* Validar absorción: aparición de imbalances sin continuación del precio (especialmente en resistencia/soporte).  


---  

### 📊 Nivel de relevancia  
🔟 **9 / 10**  

✅ Lectura diagonal correcta (microestructura Bid/Ask por tick adyacente).  
✅ Resumen `NxM` por vela reduce tiempo de lectura y carga cognitiva en M1.  
✅ Arquitectura de render clara: cálculo (OnCalculate) separado de dibujo (OnRender).  
⛔ No persiste niveles S/R por sí mismo (eso lo aporta Stacked Imbalance, con sus riesgos).  


---  

### 🎯 Estrategias de scalping donde se aplica  
* **Breakout con confirmación**: entrar solo si aparecen imbalances a favor en el extremo de ruptura.  
* **Fade / reversión**: imbalance extremo en máximo/mínimo + falta de continuación = entrada contraria con stop corto.  
* **Pullback y continuación**: tras ruptura, esperar retroceso y buscar reaparición de imbalances a favor del movimiento.  


---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| Ratio | 3–4 | 300–400% suele filtrar ruido sin perder señales útiles. |  
| VolumeFilter | 50–150 | Ajustar a tu footprint/agrupación; evita 1vs0 o microprints. |  
| Transparency | 35–55 | Visible sin tapar números del footprint. |  
| ShowTop / ShowBot | True / True | Mantiene el conteo `NxM` siempre disponible. |  


---  

### 🧪 Notas de desarrollo  
* Usa `PriceSelectionDataSeries` y añade `PriceSelectionValue` con `VisualObject = OnlyCluster`, evitando sobrecarga visual innecesaria.  
* `OnRender` calcula un rectángulo por barra y dibuja un bloque con el conteo; el coste está acotado por barras visibles.  
* Ajuste correcto de transparencia: reescribe `PriceSelectionColor` al cambiar `Transparency`.  


---  

### ❗ Incoherencias o aspectos mejorables detectados  
* No se detectan errores de cálculo evidentes en la lógica diagonal.  
* (Mejora menor) En `OnRender`, el cálculo del `priceHeight` depende de `GetYByPrice(0)` y tick; suele funcionar, pero en escalas raras podría ajustarse a una altura fija por fuente tipográfica.  


---  

### 🛠️ Propuestas de mejora  
* Añadir modo opcional para mostrar el conteo `NxM` separado por compra/venta con colores distintos o iconografía mínima (sin saturar).  
* Permitir anclar el bloque-resumen a una posición fija (arriba/abajo) independientemente del delta de la vela.  


---  

### 💎 Valor Reutilizable (Código Donante)  
* Patrón completo de “cálculo por niveles + resaltado OnlyCluster + overlay de resumen por vela” reutilizable en otros detectores (stacking, absorción, micro-poc, etc.).  


---  

### ✍️ La opinión de ChatGPT sobre el Indicador  
Imbalance Ratio es el “medidor base” del Footprint: rápido, interpretable y directamente accionable. En un sistema de scalping, este es el detector primario; el resto de herramientas deberían complementar (persistencia, contexto, filtros), no sustituirlo.  


---  

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí.**  

Es un indicador Core para confirmar agresión real y evitar entradas basadas solo en precio.  

**Acción:** **Conservar (Core)**  
