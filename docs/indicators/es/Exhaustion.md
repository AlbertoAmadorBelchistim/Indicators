---

# 1. IDENTIFICACIÓN  
cs_file: Exhaustion.cs  
name: Exhaustion  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Footprint  
comparison_group: "Microstructure"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8/10  
score_potential: 9/10  
file_state: Defectuoso  
effort: Medio  
action_priority: Media  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Está el precio mostrando agotamiento (desaceleración de Bid/Ask/Volumen) en los extremos de la vela en los últimos N niveles?  
gemini_summary: "Idea potente (leer el 'secarse' de la subasta en el extremo), pero la implementación es rígida: exige exactamente N niveles válidos y si falta 1 no dibuja nada. En práctica intradía eso destruye señal útil."  
competitor_notes: "Frente a Ratio: Exhaustion depende de una secuencia estricta de N niveles; Ratio da lectura más estable y con menos condiciones de fallo. Exhaustion puede complementar, pero hoy es menos fiable para ejecución M1."  
reusable_code: "Uso de PriceSelectionDataSeries + PriceSelectionValue con Tooltips y colores por lado (support/resistance). Buen patrón de visualización reutilizable."  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🛠️ Exhaustion (8/10)  

**Nombre del archivo:** [`Exhaustion.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Exhaustion.cs)  
**Nombre del indicador:** Exhaustion  
**Web oficial:** [ATAS — Exhaustion](https://help.atas.net/support/solutions/articles/72000641184-exhaustion)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Está el precio mostrando agotamiento (desaceleración de Bid/Ask/Volumen) en los extremos de la vela en los últimos N niveles?  

![Exhaustion](../../img/Exhaustion.png)


---

### ⚙️ Parámetros configurables  
- **CalcMode:** Fuente de cálculo. `Bid`, `Ask`, `BidAndAsk`, `Volume`.  
- **AmoutOfPrices:** Nº de niveles desde el extremo a evaluar (typo en el nombre).  
- **VisualType:** Objeto (Rectangle u otros) para marcar la zona.  
- **ShowPriceSelection:** Activa el resaltado por “selección” en el cluster.  
- **Size:** Tamaño del objeto visual.  
- **VisualObjectsTransparency:** Transparencia de los objetos.  
- **TopColor / BottomColor:** Color del objeto en resistencia/soporte.  
- **TopClusterColor / BottomClusterColor:** Color del resaltado de selección (si está activo).  
- **UseAlerts:** Activar alertas.  
- **AlertFile:** Sonido/archivo de alerta.  
- **OnBarCloseAlert:** Alertar en cierre de barra o inmediato.  


---

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Footprint  
**Comparison Group:** "Microstructure"  


---

### 🧠 Uso más frecuente  
- **Lectura de “subasta agotada” en extremos:** detectar patrones donde el extremo muestra progresión de volumen (o agresión) y luego se corta, sugiriendo incapacidad de continuar.  
- **Zonas de reversión potencial:** caja en high/low que actúa como “marca” visual para fade/mean reversion.  


---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  

✅ Visualmente muy claro: marca una zona (varios niveles) en el extremo, no un único tick.  
✅ Buen patrón de render con `PriceSelectionDataSeries` (útil para otros indicadores).  
⛔ Lógica demasiado rígida: si no hay exactamente N niveles “válidos”, no dibuja nada.  
⛔ Robustez mejorable: si falta `PriceVolumeInfo` en un nivel, hace `return` y aborta todo el cálculo de la barra.  


---

### 🎯 Estrategias de scalping donde se aplica  
- **Fade en extensión:** extensión a un extremo + aparece zona de exhaustion → buscar entrada contra con confirmación adicional (delta/estructura).  
- **Pullback failure:** tras ruptura, si en el retest aparece exhaustion en el extremo contrario, anticipa fallo del movimiento.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación operativa |  
| :--- | :--- | :--- |  
| **CalcMode** | `BidAndAsk` | Mantiene simetría de lectura (soporte y resistencia). |  
| **AmoutOfPrices** | `3` | Menos rigidez: sube tasa de señal y reduce “no dibujo nada”. |  
| **VisualType** | `Rectangle` | Lectura inmediata de zona. |  
| **ShowPriceSelection** | `true` | Resalta el rango exacto afectado (si no tapa el footprint). |  
| **VisualObjectsTransparency** | `70`–`85` | Evita tapar clusters/volumen. |  
| **UseAlerts** | `false` (por defecto) | En M1 puede saturar; activar solo en investigación o condiciones raras. |  


---

### 🧪 Notas de desarrollo  
- El cálculo recorre desde **High→Low** (resistencia) y desde **Low→High** (soporte) usando `_step` del contenedor de precios.  
- Criterio central: construye una secuencia donde el `sourceValue` debe ser **estrictamente creciente** respecto al nivel previo.  
- Renderiza como arrays de `PriceSelectionValue` con: tooltip, colores, transparencia y rangos mínimo/máximo.  
- Alertas: acumula tooltips y dispara según `OnBarCloseAlert`.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
- **Diseño “todo o nada”:** `if (pvInfos.Count != _amoutOfPrices) return;` elimina señales parciales útiles (por ejemplo 4/5 niveles).  
- **Abort por null:** `if (info is null) return;` corta el cálculo completo de la barra si falta un solo nivel.  
- **Typo público:** `AmoutOfPrices` debería ser `AmountOfPrices` (impacto UX/documentación y seriedad del parámetro).  


---

### 🛠️ Propuestas de mejora  
- Cambiar a lógica **“hasta N niveles”**: dibujar los niveles encontrados (>= 2) aunque no llegue a N.  
- Cambiar el manejo de `info is null` a “skip/break” (no `return`) para evitar perder toda la barra.  
- Corregir el nombre del parámetro expuesto (manteniendo compatibilidad si ATAS guarda templates; ideal: alias/obsoleto).  
- Opcional: permitir condición `>=` o tolerancia (creciente con margen) para no depender de micro-ruido.  


---

### 💎 Valor Reutilizable (Código Donante)  
- Implementación de `PriceSelectionDataSeries` / `PriceSelectionValue` con tooltips, color por lado y control de transparencia (muy útil para construir overlays limpios).  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
Exhaustion tiene un concepto muy alineado con scalping de microestructura, pero su implementación actual penaliza demasiado la señal real por reglas rígidas. Como **Reserva**, puede tener valor si lo “reparamos” para que entregue información parcial y sea robusto a huecos de datos por nivel. Hasta entonces, Ratio es el core natural del grupo.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí, pero de forma limitada (hoy).**  

Cuando dibuja, aporta una lectura visual muy potente; sin embargo, ahora mismo puede “silenciar” la señal por rigidez.  

**Acción:** **Conservar (Reserva)**  
