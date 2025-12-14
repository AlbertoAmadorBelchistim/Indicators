---
# 1. IDENTIFICACIÓN  
cs_file: VBRR.cs  
name: Volume Bar Range Ratio  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume  
comparison_group: "Volume Efficiency"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 7.5/10  
score_potential: 8/10  
file_state: Estable  
effort: Bajo  
action_priority: Baja  
system_priority: P2  

# 4. DECISIÓN  
recommended_action: Conservar (Core)

# 5. ANÁLISIS  
description: ¿Cuánto volumen “cuesta” mover el precio (volumen por unidad de rango) en cada vela?  
gemini_summary: "Mide fricción/absorción: mucho volumen con poco rango sugiere absorción; poco volumen con mucho rango sugiere vacío de liquidez."  
competitor_notes: "Es útil, pero pierde frente a VolumePerTrade para M1 porque depende del rango y sufre en dojis/micro-rangos. Aun así, es buen detector de absorción."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-12  
official_code_date: 2025-04-23  
---

## 🟧 Volume Bar Range Ratio (7.5/10)

**Nombre del archivo:** [`VBRR.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VBRR.cs)  
**Nombre del indicador:** Volume Bar Range Ratio  
**Web oficial:** [ATAS — VBRR](https://help.atas.net/support/solutions/articles/72000602499)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuánto volumen “cuesta” mover el precio (volumen por unidad de rango) en cada vela?  

![VBRR](../../img/VBRR.png)  

  

---  

### ⚙️ Parámetros configurables  

- **N/A**: cálculo directo.  

  

---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Efficiency"  

  

---  

### 🧠 Uso más frecuente  

- **Absorción en nivel:** VBRR alto cuando el precio “no avanza” pese a intercambio fuerte.  
- **Vacío de liquidez:** VBRR bajo cuando el precio se desplaza con poco intercambio (riesgo de V-shape/slippage).  

  

---  

### 📊 Nivel de relevancia  
🔟 **7.5 / 10**  

✅ Bueno para detectar “esfuerzo sin resultado” (absorción).  
✅ Cálculo barato.  
⛔ Muy sensible a velas de rango mínimo; lectura puede degradarse en M1.  

  

---  

### 🎯 Estrategias de scalping donde se aplica  

- **Fade en resistencia/soporte:** toque a nivel + VBRR se dispara + falta continuación = candidato a giro.  
- **Filtro de entradas en mercado “hueco”:** VBRR muy bajo = evitar perseguir.  

  

---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| N/A | N/A | Ideal acompañarlo de un suavizado externo (si se implementa) o leerlo solo en eventos (toque/ruptura). |  

  

---  

### 🧪 Notas de desarrollo  

- Fórmula: `Volume / (High - Low)`. 
- Manejo de doji: si `High == Low`, copia `this[bar - 1]` (arrastre).

  

---  

### ❗ Incoherencias o aspectos mejorables detectados  

- **Arrastre en doji:** puede ocultar un evento real en velas de rango mínimo (especialmente en instrumentos con tick-size grande relativo).

  

---  

### 🛠️ Propuestas de mejora  

- **P3 (Baja):** opción de suavizado (SMA/EMA) y/o “clamp” para evitar spikes extremos.  
- **P3 (Baja):** alternativa al doji: usar rango mínimo = 1 tick (en vez de arrastrar).  
- **P2 (Media, creativo):** versión “Event-based”: mostrar solo cuando el precio interactúa con niveles (VWAP, máximos de sesión, niveles gamma), para reducir ruido.  

  

---  

### 💎 Valor Reutilizable (Código Donante)  

- **Ninguno.**  

  

---  

### ✍️ La opinión de ChatGPT sobre el Indicador  

Es un **buen reserva** para leer microestructura en zonas clave, pero no lo pondría como señal primaria. En M1, el rango puede ser un artefacto (micro-delta, velas estrechas, dojis) y el indicador se vuelve demasiado reactivo. Su mejor uso es “disparador de contexto”: si en un nivel relevante aparece un VBRR alto, se incrementa la probabilidad de absorción y giro.  

  

---  

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí, con limitaciones.**  

Útil como detector de absorción/vacío, mejor como confirmación contextual.  

**Acción:** **Conservar (Core	)**  