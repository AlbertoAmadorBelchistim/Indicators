---
cs_file: UpDownVolumeRatio.cs
name: Up/Down Volume Ratio
group: Order Flow
subgroup: Volume
score_current: 9/10
version: Stable
recommended_action: Conservar (Core)
description: ¿Quién controla el flujo de volumen (compradores o vendedores) y con qué intensidad relativa?
gemini_summary: "Oscilador de volumen definitivo. Normaliza el flujo en un ratio porcentual (-100 a +100), lo que lo hace perfecto para detectar condiciones de sobrecompra/sobreventa de flujo y divergencias. Versátil y completo."
comparison_group: "Volume Oscillators"
competitor_notes: "Superior a osciladores simples por su normalización y opciones de suavizado."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: N/A
action_priority: N/A
analysis_date: 2025-11-21
official_code_date: 23/04/2025
---

## 🏆 Up/Down Volume Ratio (9/10)

**Nombre del archivo:** [`UpDownVolumeRatio.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/UpDownVolumeRatio.cs)  
**Nombre del indicador:** Up/Down Volume Ratio  
**Web oficial:** [ATAS — Up/Down Volume Ratio](https://help.atas.net/support/solutions/articles/72000619242)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 23/04/2025  

> **La Pregunta Clave:** ¿Quién controla el flujo de volumen (compradores o vendedores) y con qué intensidad relativa?

![UpDownVolumeRatio](../../img/UpDownVolumeRatio.png)

---

### ⚙️ Parámetros configurables

Este oscilador permite ajustar qué se mide y cómo se suaviza:

#### 📊 Modo de Cálculo
* **Calculation Mode:**
    * `AskBidVolume`: Utiliza el Delta real (Ask - Bid). Es la opción recomendada para Order Flow.
    * `UpDownVolume`: Utiliza la dirección de la vela (Close > Open). Opción clásica.

#### 📉 Suavizado
* **Moving Type:** Tipo de media móvil aplicada al ratio bruto.
    * `EMA`: Exponencial (Rápida).
    * `LinReg`: Regresión Lineal (Reactiva, menos lag).
    * `SMA`: Simple (Suave).
    * ... y otras (WMA, WWMA, SZMA).
* **Period:** Longitud del suavizado (Default: 10).

#### 🎨 Visualización
* **Color:** Color del histograma.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Oscillators"  

---

### 🧠 Uso más frecuente

* **Detector de Divergencias:** El precio hace un nuevo máximo, pero el Ratio hace un máximo más bajo (Agotamiento de compradores).  
* **Confirmación de Tendencia:** El precio sube y el Ratio se mantiene en zona positiva y creciente.  
* **Sobre-extensión:** Un ratio cercano a +100 o -100 indica un flujo unidireccional extremo, posible clímax.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (IMPRESCINDIBLE)**

✅ **Normalización:** Al oscilar entre -100 y +100, permite comparar la intensidad de hoy con la de ayer, independientemente del volumen total.  
✅ **Versatilidad:** Sirve tanto para análisis de velas (`UpDown`) como de microestructura (`AskBid`).  
✅ **Calidad de Código:** Implementa múltiples tipos de medias móviles de forma modular.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Delta Wave:** Usar modo `AskBid` con `LinReg(9)`. Entrar cuando el histograma cruza la línea cero con pendiente fuerte.  
* **Absorción:** Precio rompiendo mínimos pero Ratio `AskBid` subiendo (divergencia positiva) = Vendedores atrapados.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Calc Mode** | `AskBidVolume` | Ver la verdad de la subasta. |
| **Moving Type** | `LinReg` | Minimizar el retraso (lag). |
| **Period** | `14` | Estándar de corto plazo. |

---

### 🧪 Notas de desarrollo

* Fórmula: `100 * (Buy - Sell) / (Buy + Sell)`.
* Maneja correctamente la división por cero (`Buy + Sell == 0`).

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.** Es sólido.

---

### 🛠️ Propuestas de mejora

* **Color Dinámico (P2):** Cambiar el color de la barra si el valor aumenta o disminuye respecto a la anterior (Slope coloring) para ver el momentum visualmente.

---

### 💎 Valor Reutilizable (Código Donante)

* **Ninguno.** ---

### ✍️ La opinión de Gemini sobre el Indicador

Es el mejor oscilador de volumen disponible. La capacidad de elegir entre `UpDown` (Price Action) y `AskBid` (Order Flow) lo hace universal. La normalización porcentual es la clave que lo hace superior al Delta bruto para leer momentum.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para leer el pulso inmediato del mercado.

**Acción:** **Conservar (Core).**