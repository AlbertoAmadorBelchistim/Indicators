---
cs_file: OIAnalyzer.cs
name: OI Analyzer
category: Order Flow
group: Order Flow
subgroup: Open Interest
score_current: 10/10
version: Estable
recommended_action: Conservar (Indicador Core)
description: ¿Cómo cambia el Interés Abierto (OI) filtrado por dirección (Buy/Sell) y visualizado en detalle?
gemini_summary: "La herramienta definitiva de OI Estructural. Desglosa el OI por dirección de agresión, revelando la intención real. Es el Rey del Análisis, pero carece de la agilidad (momentum) de BalanceOI y de las alertas de audio del OpenInterest estándar."
comparison_group: "Open Interest Analysis"
competitor_notes: "Superior en profundidad, pero complementario a 'BalanceOI' (que ofrece momentum rápido) y 'OpenInterest' (que ofrece alertas simples). Aún no los reemplaza al 100%."
reusable_code: "Lógica de desglose Buy/Sell de OI, Visualización de Clusters numéricos (OnRender)"
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-20
official_code_date: 2025-04-23
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

* **OiMode**: Qué mostrar. `Buys` (OI generado por compras a mercado) o `Sells` (por ventas).
* **CalculationMode**:
    * `CumulativeTrades`: Acumula el valor a lo largo de la sesión (Tendencia).
    * `SeparatedTrades`: Muestra el cambio neto por trade/barra (Delta de OI).
* **ClustersMode**: Muestra los valores numéricos exactos sobre el gráfico (Estilo Footprint).
* **GridStep**: Configuración de la cuadrícula visual.

---

### 🧭 Clasificación
**Grupo:** Order Flow
**Subgrupo:** Open Interest

---

### 🧠 Uso más frecuente

* **Desglose de Intención:** Ver si el aumento de OI es por largos agresivos (`Mode: Buys` subiendo) o cortos agresivos (`Mode: Sells` subiendo).
* **Detección de Stops (Cierres):** Si el precio sube pero el `Buy OI` baja, los largos están cerrando posiciones (Take Profit), no abriendo nuevas.
* **Lectura de Precisión:** Usar el `ClustersMode` para ver exactamente cuántos contratos se han abierto en cada nivel de precios.

---

### 📊 Nivel de relevancia
🔟 **10 / 10 (IMPRESCINDIBLE)**

✅ **Información Única:** Es el único indicador que te dice la *dirección* del flujo de dinero nuevo.  
✅ **Visualización Pro:** El modo `ClustersMode` permite leer el OI exacto vela a vela sin usar un panel inferior.  
⛔ **Falta Agilidad:** No tiene un modo "Oscilador" (como BalanceOI) para ver giros rápidos de momentum.  
⛔ **Falta Alerta:** No avisa sonoramente de cambios bruscos (como OpenInterest).

---

### 🎯 Estrategias de scalping donde se aplica

* **Validación de Ruptura:** Breakout alcista + Aumento fuerte de `Buy OI` = Ruptura genuina (Dinero Nuevo).
* **Caza de Stops:** Movimiento rápido contra tendencia + Disminución brusca de OI = Ejecución de Stops (Cierre de posiciones, no iniciativa).

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **OiMode**: Tener dos instancias cargadas (una `Buys` Verde, una `Sells` Roja) para ver ambos flujos.
* **CalculationMode**: `CumulativeTrades`.
* **ClustersMode**: `False` (para visión general) o `True` (para entrada fina).

---

### 🧪 Notas de desarrollo

* El indicador solicita datos históricos especiales (`CumulativeTradesRequest`) para reconstruir el OI trade a trade.
* Calcula el delta de OI (`dOi`) por cada trade y lo asigna a compra o venta según la dirección del agresor (`tick.Direction`).
* Implementa un renderizado personalizado eficiente en `OnRender` para el modo Clusters.

---

### ❗ Incoherencias o aspectos mejorables detectados
* **Solapamiento Visual:** En el modo `ClustersMode`, los textos numéricos no tienen lógica de escalado dinámico.
* **Configuración Doble:** Obliga a cargar dos indicadores para ver Buy y Sell a la vez.

---

### 🛠️ Propuestas de mejora
Para convertirlo en el "Santo Grial" único y eliminar a los otros dos indicadores, necesitaría:
1.  **Modo Oscilador (Rolling Sum):** Copiar la lógica de `BalanceOI` (`MinimizedMode`) para ver el momentum del OI direccional.
2.  **Alertas:** Copiar el sistema de alertas de `OpenInterest`.
3.  **Modo Dual:** Opción para pintar líneas Buy y Sell en el mismo panel.

---

### 💎 Valor Reutilizable (Código Donante)
* **Lógica de Desglose:** El algoritmo que asigna el cambio de OI a Compradores o Vendedores (`if (dOi > 0 && tick.Direction == Buy)...`) es fundamental.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el "microscopio" del grupo y el ganador claro en profundidad.

Sin embargo, **no mata a sus rivales todavía**.
* Si quieres ver **Inercia/Momentum** rápido, `BalanceOI` sigue siendo más visual.
* Si quieres **Alertas pasivas**, `OpenInterest` es necesario.

`OIAnalyzer` es tu herramienta de *Análisis*, los otros son herramientas de *Señal*.

### 📈 Veredicto: ¿Es útil para Scalping?

**Absolutamente.** Es la herramienta Core.

**Acción:** **Conservar (Core Analítico).**

