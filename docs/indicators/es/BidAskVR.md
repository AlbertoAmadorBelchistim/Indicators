---
# 1. IDENTIFICACIÓN
cs_file: BidAskVR.cs
name: Bid Ask Volume Ratio
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Delta
comparison_group: "Bar Delta Details"

# 3. VALORACIÓN (Score & Priority)
score_current: 8.5/10
score_potential: 9.0/10
file_state: Estable
effort: Bajo
action_priority: Baja
system_priority: P2

# 4. DECISIÓN
recommended_action: Conservar (Core)

# 5. ANÁLISIS
description: ¿Cuál es el desequilibrio normalizado (de -100% a +100%) del volumen agresivo y su momentum?
gemini_summary: "Supera al desglose bruto al normalizar la batalla entre compradores y vendedores en un ratio porcentual. Su lógica de color de 4 vías es excelente para detectar divergencias de flujo y agotamiento antes de que el precio gire."
competitor_notes: "Superior a 'Bid Ask' porque contextualiza el volumen. Un desequilibrio del 80% es visible aquí, mientras que en volumen bruto podría perderse en una vela de rango estrecho."
reusable_code: "La lógica de 4 colores (SetColor) basada en Valor vs Valor Previo es un patrón de diseño reusable para cualquier oscilador de momentum."

# 6. METADATOS
analysis_date: 2025-11-21
official_code_date: 2025-04-23
user_modification_date: null
---

## 🏆 Bid Ask Volume Ratio (8.5/10)

**Nombre del archivo:** [`BidAskVR.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/BidAskVR.cs)  
**Nombre del indicador:** Bid Ask Volume Ratio  
**Web oficial:** [ATAS — Bid Ask Volume Ratio](https://help.atas.net/support/solutions/articles/72000602330)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es el desequilibrio normalizado (de -100% a +100%) del volumen agresivo y su momentum?

![BidAskVR](../../img/BidAskVR.png)

---

### ⚙️ Parámetros configurables

Este indicador permite ajustar la sensibilidad del ratio y su suavizado para filtrar el ruido del Order Flow:

#### 📊 Cálculo y Suavizado (Settings)
* **Mode (CalcMode):**
    * `AskBid`: Ratio estándar `(Ask - Bid) / Total`. Positivo indica compras.
    * `BidAsk`: Ratio invertido.
* **Moving Type (MaType):** Algoritmo de suavizado. Recomendado `Ema` para scalping.
* **Period:** Ventana de cálculo de la media móvil (Default: `10`). A mayor periodo, menos ruido pero más lag.

#### 🎨 Visualización (Drawing)
Controla la **Lógica de 4 Colores**, vital para leer el momentum del flujo:
* **Upper (UpperColor):** Valor positivo y **creciendo** (Fuerza Compradora Acelerando).
* **Up (UpColor):** Valor positivo pero **decreciendo** (Fuerza Compradora Agotándose).
* **Low (LowColor):** Valor negativo pero **subiendo** hacia cero (Fuerza Vendedora Agotándose).
* **Lower (LowerColor):** Valor negativo y **bajando** (Fuerza Vendedora Acelerando).

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Bar Delta Details"  

---

### 🧠 Uso más frecuente

* **Detector de Divergencias:** El precio hace un nuevo máximo, pero el BidAskVR muestra un color "oscuro" (`UpColor`), indicando que el desequilibrio comprador está perdiendo fuerza.  
* **Confirmación de Ruptura:** En un breakout, buscamos colores "brillantes" (`Upper` o `Lower`) que confirmen entrada agresiva de volumen en la dirección de la ruptura.  
* **Normalización de Volatilidad:** Permite evaluar la agresividad relativa incluso en horarios de bajo volumen (pre-market).  

---

### 📊 Nivel de relevancia
🔟 **8.5 / 10**

✅ **Normalización Inteligente:** Convierte datos brutos en un porcentaje comparable (-100 a +100), eliminando la distorsión del volumen absoluto.  
✅ **Lectura de Momentum:** Los 4 colores permiten leer la "segunda derivada" (aceleración/desaceleración) del flujo sin mirar números.  
⛔ **UI por Defecto:** `ShowZeroValue = false` hace que el histograma flote sin referencia clara en paneles comprimidos.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Reversión por Agotamiento:** Precio en extremo + BidAskVR cambiando de Brillante a Oscuro.  
* **Pullback Continuación:** Durante un pullback, el BidAskVR se acerca a 0 pero no cruza, y vuelve a acelerar (cambio de Oscuro a Brillante).  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor | Justificación |
| :--- | :--- | :--- |
| **Moving Type** | `Ema` | La Media Móvil Exponencial pondera más la acción reciente, vital para 1M. |
| **Period** | `10` - `14` | `10` es reactivo para entradas rápidas; `14` filtra mejor falsos positivos. |
| **Mode** | `AskBid` | Estándar intuitivo: Barras arriba = Compras. |
| **Colores** | *High Contrast* | Usar Lima/Verde Oscuro y Rojo Vivo/Granate para distinguir fases de un vistazo. |

---

### 🧪 Notas de desarrollo

* **Cálculo:** `100 * (Ask - Bid) / (Ask + Bid)`. Esto normaliza el Delta respecto al Volumen Total.
* **Inicialización:** Tiene una lógica manual `if (bar < period)` para los primeros compases, aunque las clases `EMA`, `SMA`, etc., de ATAS ya manejan esto. No es crítico pero es código legacy.
* **Rendering:** Usa `VisualMode.Histogram`.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta Línea Cero:** El oscilador fluctúa entre positivo y negativo. La ausencia de una línea cero visual (`ShowZeroValue = true` o una `LineSeries` fija) dificulta la lectura rápida del cruce de equilibrio.

---

### 🛠️ Propuestas de mejora

* **Mejora UI (Baja):** Activar `ShowZeroValue = true` en el constructor o añadir una línea horizontal gris en el nivel 0.0.

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Coloreado (SetColor):** El método `SetColor` es perfectamente exportable para añadir "Momentum Coloring" a cualquier otro oscilador (ej. RSI, CCI o Delta personalizado).

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el "Delta Inteligente". Mientras el Delta normal te da un número absoluto que varía salvajemente según la hora del día, el **Bid Ask Volume Ratio** te da la "calidad" de la subasta. Un Delta de +500 en la apertura es ruido; un Ratio de +80% es una declaración de intenciones, sea la hora que sea.

La lógica de colores es su mayor fortaleza: te avisa del giro *antes* de que el histograma cruce cero.

**Propuestas de Acción:**
* Mantener como indicador **Core** del subgrupo Delta Details.
* Ajustar configuración por defecto para incluir la línea cero.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Esencial para filtrar falsas rupturas y detectar agotamientos en scalping de 1 minuto.

**Acción:** **Conservar (Core)**