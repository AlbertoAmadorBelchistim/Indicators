---
cs_file: OIAnalyzer.cs
name: OI Analyzer
group: Order Flow
subgroup: Open Interest
score_current: 10/10
version: Estable
recommended_action: Conservar (Core)
description: ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle?
gemini_summary: "La herramienta definitiva de OI Estructural. Desglosa el OI por dirección de agresión, revelando la intención real (apertura de largos vs cierre de cortos). Es el Rey del Análisis de OI."
comparison_group: "Open Interest Analysis"
competitor_notes: "Superior en profundidad a todos. Complementario a 'BalanceOI' (momentum) y 'OpenInterest' (alertas)."
reusable_code: null
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 OI Analyzer (10/10)

**Nombre del archivo:** [`OIAnalyzer.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/OIAnalyzer.cs)  
**Nombre del indicador:** OI Analyzer  
**Web oficial:** [ATAS — OI Analyzer](https://help.atas.net/support/solutions/articles/72000602437)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle?

![OIAnalyzer](../../img/OIAnalyzer.png)

---

### ⚙️ Parámetros configurables

Este indicador ofrece un desglose profundo del OI con opciones avanzadas de visualización:

#### 📊 Cálculo
* **Mode (OiMode):**
    * `Buys`: Muestra el OI generado por agresiones de compra (Ask).
    * `Sells`: Muestra el OI generado por agresiones de venta (Bid).
* **Calculation Mode:**
    * `CumulativeTrades`: Acumula el valor a lo largo de la sesión (Visión de tendencia).
    * `SeparatedTrades`: Muestra el cambio neto por barra (Delta de OI).
* **Cumulative Mode:** (Checkbox) Alternativa rápida para activar el modo acumulativo.
* **Clusters Mode:** Muestra los valores numéricos exactos sobre el gráfico principal (Estilo Footprint), ideal para ver niveles exactos de absorción/creación de contratos.

#### 🎨 Visualización
* **Custom Diapason:** Permite fijar una escala estática (Rango Min/Max) para comparar días diferentes con la misma referencia visual.
* **Grid Step:** Configura la cuadrícula de fondo para facilitar la lectura de niveles.
* **Colors:** Personalización completa de colores alcistas/bajistas y textos.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Open Interest  
**Comparison Group:** "Open Interest Analysis"  

---

### 🧠 Uso más frecuente

* **Desglose de Intención:** Diferenciar si una subida de precio es por **dinero nuevo** (`Buy OI` subiendo = Largos agresivos) o por **cierre de cortos** (`Sell OI` bajando = Short Covering).  
* **Detección de Stops:** Movimiento rápido contra tendencia + Caída brusca de OI = Ejecución de Stops (Cierre forzoso).  
* **Lectura de Precisión:** Usar el `ClustersMode` para ver exactamente cuántos contratos se han abierto/cerrado en una vela clave.  

---

### 📊 Nivel de relevancia
🔟 **10 / 10 (IMPRESCINDIBLE)**

✅ **Información Única:** Es el único indicador que te dice la *dirección* del flujo de dinero. Saber si subimos por compras o por cierre de cortos cambia totalmente la estrategia.  
✅ **Visualización Pro:** El modo `ClustersMode` integra el dato en el precio, ahorrando espacio en pantalla.  
✅ **Versatilidad:** Funciona tanto para análisis de tendencia (Cumulative) como para ver el impacto inmediato (Separated).  

---

### 🎯 Estrategias de scalping donde se aplica

* **Validación de Ruptura (Breakout):** Breakout alcista + Aumento fuerte de `Buy OI` = Ruptura genuina con iniciativa.  
* **Trampa de Valor:** Precio subiendo pero `Buy OI` plano o bajando = Falta de interés comprador real (posible giro).  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Calculation Mode** | `CumulativeTrades` | Ver la tendencia del día. |
| **Mode** | *Doble Instancia* | Cargar dos veces: uno `Buys` (Verde) y uno `Sells` (Rojo) en el mismo panel. |
| **Clusters Mode** | `False` | Desactivado para visión general, activar solo para análisis fino. |
| **Grid Step** | `Auto` | Ajustar según volatilidad del activo. |

---

### 🧪 Notas de desarrollo

* Solicita datos históricos especiales (`CumulativeTradesRequest`) para reconstruir el OI trade a trade con precisión.
* Asigna el cambio de OI (`dOi`) a compra o venta basándose en la dirección del agresor (`tick.Direction`).
* Renderizado personalizado eficiente en `OnRender` para el modo Clusters y Grid.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Solapamiento Visual:** En modo Clusters, los textos pueden solaparse si hay muchas barras juntas.
* **Doble Carga:** Requiere cargar el indicador dos veces para ver ambos lados del mercado simultáneamente.

---

### 🛠️ Propuestas de mejora

* **Fusión (P2):** Crear un modo "Dual Line" que pinte ambas líneas (Buy y Sell) en una sola instancia del indicador.
* **Alertas (P2):** Incorporar el sistema de alertas sonoras de `OpenInterest`.

---

### 💎 Valor Reutilizable (Código Donante)

* **Algoritmo de Desglose:** La lógica que separa el OI por dirección del agresor es el núcleo de su valor y es exportable.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el "microscopio" del Order Flow.
Saber que el OI ha subido 500 contratos es útil. Saber que esos 500 contratos fueron **compras agresivas** (y no ventas pasivas) es la ventaja que te da este indicador. Es la diferencia entre operar a ciegas y operar con visión de rayos X.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Fundamental para validar la calidad de cualquier movimiento.

**Acción:** **Conservar (Core).**).**

