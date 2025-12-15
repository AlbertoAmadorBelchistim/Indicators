---
# 1. IDENTIFICACIÓN  
cs_file: UpDownVolumeRatio.cs  
name: Up/Down Volume Ratio  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume  
comparison_group: "Volume Oscillators"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 9/10  
score_potential: 9/10  
file_state: Estable  
effort: Bajo  
action_priority: Nula  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Quién controla el flujo de volumen (compradores o vendedores) y con qué intensidad relativa?  
gemini_summary: "Oscilador de volumen muy sólido: normaliza el flujo en rango -100..+100, permite cálculo por Ask/Bid o Up/Down, y ofrece suavizados rápidos para lectura de momentum y divergencias en scalping."  
competitor_notes: "Gana el combate por normalización, versatilidad (AskBid vs UpDown) y calidad de implementación. Es el único que aporta una lectura de flujo comparable entre sesiones sin deriva."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-15  
official_code_date: 2025-04-23  

  

---  

## 🏆 Up/Down Volume Ratio (9/10)  

**Nombre del archivo:** [`UpDownVolumeRatio.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/UpDownVolumeRatio.cs)  
**Nombre del indicador:** Up/Down Volume Ratio  
**Web oficial:** [ATAS — Up/Down Volume Ratio](https://help.atas.net/support/solutions/articles/72000619242)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Quién controla el flujo de volumen (compradores o vendedores) y con qué intensidad relativa?  

![UpDownVolumeRatio](../../img/UpDownVolumeRatio.png)  

  

---  

### ⚙️ Parámetros configurables  

#### 📌 Calculation  
- **Calculation Mode (CalcMode):** define qué “volumen direccional” se usa.  
  - `AskBidVolume`: usa Ask y Bid del candle para aproximar delta relativo (recomendado en Order Flow).
  - `UpDownVolume`: asigna el volumen de la vela a Up o Down según `Close vs Open` (modo clásico). 
- **Period:** periodo del suavizado (default 10).
- **Moving Type (MovType):** tipo de media aplicada al ratio (EMA, LinReg, SMA, WMA, WWMA, SZMA, SMMA).

#### 🎨 Visualization  
- **Histogram Color (HistogramColor):** color del histograma.

  

---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Oscillators"  

  

---  

### 🧠 Uso más frecuente  

* Detectar **divergencias de flujo** (precio hace nuevo extremo y el ratio no confirma).  
* Confirmar **momentum direccional** (histograma sostenido por encima/debajo de 0).  
* Identificar **clímax de flujo** cuando el ratio se acerca a ±100 (unidireccionalidad extrema).   

  

---  

### 📊 Nivel de relevancia  
🔟 **9 / 10**  

✅ **Normalización -100..+100:** comparable entre sesiones y regímenes de volumen.   
✅ **Modo Ask/Bid:** lectura de Order Flow sin necesidad de footprint en todo momento.
⛔ En días de chop puede oscilar rápido; requiere filtro contextual (niveles/volatilidad).  

  

---  

### 🎯 Estrategias de scalping donde se aplica  

* **Momentum confirmation:** entrada solo si el ratio cruza 0 con pendiente fuerte y el precio rompe nivel.  
* **Absorción / trapping:** el precio hace nuevo mínimo pero el ratio mejora (divergencia positiva) → posible giro.  
* **Trend hold:** mantener mientras el ratio permanezca del lado del trade y no haga divergencia clara.  

  

---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| CalcMode | AskBidVolume | Señal más “real” para Order Flow (Ask/Bid). |  
| MovType | LinReg | Respuesta rápida con menos lag para intradía. |  
| Period | 14 | Ventana corta estable para M1/2500V sin hiper-ruido. |  
| HistogramColor | Default | Ajuste puramente visual. |  

  

---  

### 🧪 Notas de desarrollo  

* Fórmula base (conceptual): `100 * (Buy - Sell) / (Buy + Sell)` con guard de división por cero.   
* En modo `AskBidVolume`, usa `candle.Ask` y `candle.Bid` para construir el ratio.  
* El suavizado se aplica con el tipo seleccionado (EMA/LinReg/etc.) sobre el ratio bruto. 

  

---  

### ❗ Incoherencias o aspectos mejorables detectados  

* **Ninguna crítica funcional**: implementación coherente, segura ante división por cero y con opciones suficientes.  
  

---  

### 🛠️ Propuestas de mejora  

* **P2 (Baja):** coloreado dinámico por pendiente (slope coloring) para distinguir aceleración/desaceleración del flujo sin mirar valores numéricos.  

  

---  

### 💎 Valor Reutilizable (Código Donante)  

* Implementación modular de suavizados (EMA, LinReg, WMA, SMA, WWMA, SZMA, SMMA) como patrón reutilizable en otros osciladores. 

  

---  

### ✍️ La opinión de ChatGPT sobre el Indicador  

Si solo pudieras quedarte con **un oscilador de volumen** para scalping, este es el candidato lógico: normaliza, no deriva, y permite lectura “order-flow-ish” sin depender del footprint constantemente. No es un trigger por sí solo, pero como **filtro/confirmador de momentum** es de los más rentables cognitivamente.  

  

---  

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí.**  

Aporta lectura de control y momentum con una métrica normalizada y operativa en M1/2500V.  

**Acción:** **Conservar (Core)**  
