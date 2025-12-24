---
# 1. IDENTIFICACIÓN  
cs_file: ClusterSearchModif.cs  
name: Cluster Search Modif  
version: Custom v1.5.0  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Footprint  
comparison_group: "Cluster Analysis"  

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
description: ¿Qué clústeres de precio específicos cumplen TODOS mis criterios de filtro (volumen, delta, localización e imbalances) para señalar un setup operable?  
gemini_summary: "Es el filtro microestructural más accionable del grupo: convierte el footprint en una búsqueda dirigida, con soporte de stacked diagonal imbalances para detectar agresión institucional con precisión."  
competitor_notes: "Gana por accionabilidad directa (selecciona y marca clusters que cumplen criterios). ClusterStatisticModif valida y contextualiza, pero no filtra setups. Absorption pretende señalar precios en los que hay stacked imbalances en los extremos de la vela.."  
reusable_code: "Lógica de DiagonalImbalance por ventanas (PriceRange) + stacking no solapado; patrón reutilizable para otros detectores de desequilibrio."  

# 6. METADATOS  
analysis_date: 2025-12-24  
official_code_date: 2025-11-19  
user_modification_date: 2025-11-21  
---

## 🟦 Cluster Search Modif (10/10)  

**Nombre del archivo:** [`ClusterSearchModif.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/ClusterSearchModif.cs)  
**Nombre del indicador:** Cluster Search Modif  
**Web oficial (Base):** [ATAS — Cluster Search](https://help.atas.net/support/solutions/articles/72000602240)  
**Compatibilidad:** ATAS Stable/Latest (requiere compilación en tu fork).  
**Última revisión del código oficial**   [`ClusterSearch.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ClusterSearch.cs): 2025-11-19  
**Última revisión del código modificado:** 2025-11-21  

> **La Pregunta Clave:** ¿Qué clústeres de precio específicos cumplen TODOS mis criterios de filtro (volumen, delta, localización e imbalances) para señalar un setup operable?  

![ClusterSearch](../../img/ClusterSearchModif.png)  


---  

### ⚙️ Parámetros configurables  

#### Filtros principales (Filters)  
- **CalcType**: Modo de cálculo del “valor objetivo” del clúster (Volume / Delta / Bid / Ask / Tick / MaxVolume / **DiagonalImbalance**).  
- **AutoFilter**: Filtro automático (top clusters) para escenarios exploratorios; al usar DiagonalImbalance se desactiva.  
- **MinimumFilter / MaximumFilter**: Umbral mínimo/máximo del valor objetivo (según CalcType).  
- **MinAverageTrade / MaxAverageTrade**: Filtra por tamaño medio de trade (volumen/ticks).  
- **MinPercent / MaxPercent**: Filtra por % del volumen del clúster respecto al volumen total de la vela.  

#### Filtros Delta (DeltaFilters)  
- **DeltaImbalance**: % mínimo de Ask o Bid dentro del clúster (asimetría).  
- **DeltaFilter**: Umbral directo de delta (positivo o negativo).  

#### Filtros de Imbalance Diagonal (Diagonal Imbalances Filters)  
- **ImbalanceRatio**: Ratio mínimo (3 = 3:1).  
- **MinVolumeDifference**: Diferencia mínima (lotes) entre lados para evitar falsos positivos.  
- **MinDominantVolume**: Volumen mínimo en el lado dominante.  
- **ImbalanceStackedRange**: Nº de “ventanas” consecutivas con imbalance para confirmar stacked.  

#### Visualización de Imbalances (Imbalances Visualization)  
- **UseSeparateColors**: Colores distintos para buy/sell imbalance.  
- **BuyImbalanceColor / SellImbalanceColor**: Colores de resaltado.  

#### Filtros de localización (Location Filters)  
- **CandleDir**: Dirección de vela (Bullish/Bearish/Any/Neutral).  
- **BarsRange**: Ventana de barras a considerar (reduce carga).  
- **PriceRange**: Agrupa N ticks por clúster (clave en DiagonalImbalance por ventanas).  
- **PipsFromHigh / PipsFromLow**: Filtra por cercanía a extremos.  
- **PriceLoc**: Posición del clúster (cuerpo/mechas/extremos).  

#### Tamaño de vela (Candle size filters)  
- **Min/MaxCandleHeight**: Altura total (ticks).  
- **Min/MaxCandleBodyHeight**: Altura del cuerpo (ticks).  

#### Tiempo (Time filtration)  
- **UseTimeFilter**: Activa el filtro horario.  
- **TimeFrom / TimeTo**: Ventana horaria.  

#### Visualización (Visualization)  
- **OnlyOneSelectionPerBar**: Solo 1 selección por vela (reduce ruido).  
- **VisualType**: Forma del marcador.  
- **ClusterColor / VisualObjectsTransparency**: Color/transparencia base.  
- **ShowPriceSelection / PriceSelectionColor**: Resalte en eje de precios.  
- **FixedSizes / Size / MinSize / MaxSize**: Control de tamaños.  

#### Alertas (Alerts)  
- **UseAlerts / AlertFile / AlertColor**: Alertas sonoras/visuales.  

#### Cálculo (Calculation)  
- **Days**: Días a calcular (0 = todos).  
- **UsePrevClose**: Evalúa en vela cerrada vs tiempo real.  


---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Cluster Analysis"  


---  

### 🧠 Uso más frecuente  
* Filtrar el gráfico para mostrar solo **clusters que cumplen un setup** (no “mirar todo”).  
* Detectar **stacked diagonal imbalances** como proxy de agresión institucional sostenida.  
* Marcar setups cerca de niveles (high/low, zonas técnicas) mediante filtros de localización.  


---  

### 📊 Nivel de relevancia  
🔟 **10 / 10**  

✅ Accionabilidad directa: “encuentra” setups, no solo los describe.  
✅ DiagonalImbalance + stacked: lectura micro muy alineada con ejecución en M1.  
⛔ Puede saturar si no se limitan BarsRange/Days/OnlyOneSelectionPerBar.  


---  

### 🎯 Estrategias de scalping donde se aplica  
* **Ruptura con confirmación**: stacked buy imbalances cerca del máximo y continuación.  
* **Fade/absorción**: stacked imbalances contra un nivel, pero sin progreso (fallo de continuación).  
* **Reversal en extremos**: filtros PipsFromHigh/Low + PriceLoc para aislar mechas/extremos.  


---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| CalcType | DiagonalImbalance | Modo CORE: detecta agresión diagonal estándar de footprint. |  
| PriceRange | 1 | Tick-by-tick; maximiza precisión. |  
| ImbalanceRatio | 3 | Ratio 3:1 como baseline robusto. |  
| MinVolumeDifference | 30 | Reduce falsos positivos por micro-lotes. |  
| MinDominantVolume | 100 | Exige presencia real de agresión. |  
| ImbalanceStackedRange | 3 | Confirma “stack” (persistencia) sin exigir extremos. |  
| PipsFromHigh / PipsFromLow | 10 | Enfoca extremos donde el setup es más útil. |  
| OnlyOneSelectionPerBar | true | Evita ruido y acelera lectura. |  
| BarsRange | 200–400 | Control de carga y foco operativo (no histórico infinito). |  
| UseTimeFilter | true | Limita a RTH o ventana operativa real. |  


---  

### ✨ Mejoras introducidas (Oficial/Base)  
Ninguna


---  

### ✨ Mejoras añadidas (Custom)  
* **CalcType = DiagonalImbalance**: búsqueda nativa de desequilibrio diagonal.  
* **Stacking por ventanas no solapadas** (PriceRange): evita duplicidades y mejora robustez.  
* **Colores separados Buy/Sell** opcionales para lectura inmediata.  


---  

### 🧪 Notas de desarrollo  
* El núcleo de valor está en `CheckCluster()` y la rama `CalcMode.DiagonalImbalance`: compara Ask (ventana superior) vs Bid (ventana inferior) y valida ratio + diferencia + volumen dominante, con stacking por grupos.  
* En DiagonalImbalance se desactivan filtros clásicos (AutoFilter/Min/Max) para no mezclar lógicas.  


---  

### ❗ Incoherencias o aspectos mejorables detectados  
* Si el usuario sube Days/ BarsRange sin límites, el coste puede crecer de forma notable en charts con footprint denso.  
* Falta un tooltip/resumen por selección (ratio real, diff, stacked alcanzado) para auditoría rápida.  


---  

### 🛠️ Propuestas de mejora  
* Añadir tooltips con métricas DI (ratio, diff, stacked) y fuente (bar/price range).  
* Presets (RTH, NY Open, Afterhours) para no reconfigurar manualmente.  


---  

### 💎 Valor Reutilizable (Código Donante)  
* Patrón de “diagonal imbalance por ventanas + stacking no solapado” reutilizable en detectores de absorción/continuación.  


---  

### ✍️ La opinión de ChatGPT sobre el Indicador  
Este indicador es el CORE del grupo porque convierte el footprint en un motor de búsqueda: impone disciplina (criterios) y produce señales repetibles. En M1, la ventaja competitiva no es “ver más datos”, sino ver menos, pero mejor filtrado.  


---  

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí.**  

Es una herramienta de ejecución microestructural: filtra, marca y prioriza.  

**Acción:** **Conservar (Core)**  
