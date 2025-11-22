---
cs_file: DomStrengthModif.cs
name: DOM Strength Modif
group: Order Flow
subgroup: DOM
score_current: 9/10
version: Estable
recommended_action: Conservar (Core)
description: ¿Cuál es la fuerza de la agresión (Trades) en relación con la liquidez pasiva (DOM)?
gemini_summary: "Concepto de 10/10 (Agresión vs Liquidez) rescatado de una implementación original rota. Esta versión 'Modif' corrige los bugs matemáticos y añade opciones de visualización limpia. Mide si la agresión está 'comiéndose' la liquidez o chocando contra un muro."
comparison_group: "Liquidez vs Agresión"
competitor_notes: "Complementario a Dom Power. Power mide la intención, Strength mide el impacto."
reusable_code: null
file_state: Estable
score_potential: 10/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 DOM Strength Modif (9/10)

**Nombre del archivo:** [`DomStrengthModif.cs`]([Link])  
**Nombre del indicador:** DOM Strength Modif  
**Web oficial base:** [ATAS — DOM Strength](https://help.atas.net/support/solutions/articles/72000602375)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código modificado:** 13/11/2025  
*(Versión extendida y mejorada por Alberto Amador Belchistim)*

> **La Pregunta Clave:** ¿Cuál es la fuerza de la agresión (Trades) en relación con la liquidez pasiva (DOM)?

![DomStrength](../../img/DomStrengthModif.png)

---

### ⚙️ Parámetros configurables

#### 📊 Filtros
* **LevelDepth:** Niveles del DOM a comparar (ej. 10).
* **Period:** Ventana de tiempo para acumular volumen de trades.
* **Percent:** Base de normalización.

#### 🎨 Visualización
* **ShowDelta:** **(Nuevo)** Opción para ocultar las velas de Delta y ver solo las barras de fuerza.
* **Colors:** Semáforo de colores (Rojo, Naranja, Verde) según la intensidad de la fuerza.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "Liquidez vs Agresión"  

---

### 🧠 Uso más frecuente

* **Detector de Barrido:** Si hay mucha compra (Trades) y poca liquidez (Ask DOM), la barra de fuerza será Verde Oscuro. El precio subirá fácil (Vacuum).  
* **Detector de Absorción:** Si hay mucha venta (Trades) pero mucha liquidez (Bid DOM), la barra de fuerza será Naranja o Roja clara. El precio no bajará (Muro).  

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (PROFESIONAL)**

✅ **Corrección de Bugs:** La versión original tenía errores graves de cálculo (sumaba Asks donde debía sumar Bids). Esta versión es matemáticamente correcta.  
✅ **Ratio Impacto:** Es el único indicador que te dice "cuánto daño" está haciendo el volumen entrante al libro de órdenes.  
✅ **Limpieza:** El parámetro `ShowDelta` permite usarlo como un panel dedicado sin duplicar información.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Absorción en Soporte:** Precio en mínimos + Delta Rojo fuerte + DomStrength Venta Débil (Naranja) = Los vendedores están chocando contra un muro de Bids. Compra.  
* **Breakout Fácil:** Precio rompe resistencia + DomStrength Compra Fuerte (Verde) = No hay Asks frenando la subida. Largo.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **LevelDepth** | `10` | Capturar la liquidez inmediata. |
| **Period** | `1` | Análisis instantáneo. |
| **ShowDelta** | `False` | Usar `DeltaModif` para ver el delta. |

---

### ✨ Mejoras introducidas (Custom Modif)

1.  **Corrección Matemática:** Arreglados los bucles de suma de liquidez que estaban invertidos en el original.
2.  **QoL Visual:** Añadido interruptor para ocultar el histograma de Delta de fondo.

---

### 🧪 Notas de desarrollo

* Compara `_buyVolume` (Trades) con `_cumAsks` (DOM).
* Fórmula: `(BuyVol / CumAsks) * 100`.
* Usa `RequestForCumulativeTrades` para inicializar datos históricos.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** El código está saneado.

---

### 🛠️ Propuestas de mejora

* **Ninguna.** 
 
---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Comparación:** La fórmula de ratio Agresión/Liquidez es exportable.

---

### ✍️ La opinión de Gemini sobre el Indicador

Si `DomPower` es la intención, `DomStrength` es la realidad del choque. Es una herramienta táctica muy fina. Requiere práctica para interpretarla, pero una vez que entiendes que "Barra débil con mucho volumen = Absorción", es oro puro.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para validar si una ruptura tiene camino libre o no.

**Acción:** **Conservar (Core).**