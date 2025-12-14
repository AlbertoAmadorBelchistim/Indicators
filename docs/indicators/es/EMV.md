---
# 1. IDENTIFICACIÓN  
cs_file: EMV.cs  
name: Arms Ease of Movement  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume  
comparison_group: "Volume Efficiency"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 6/10  
score_potential: 7/10  
file_state: Estable  
effort: Bajo  
action_priority: Baja  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Qué “facilidad” tiene el precio para desplazarse al ajustar el movimiento del punto medio por la fricción volumen/rango?  
gemini_summary: "Indicador clásico suavizado. Útil para contexto y divergencias, pero introduce lag: menos apto para decisión táctica en M1."  
competitor_notes: "Pierde frente a VPT y VBRR en scalping porque mezcla varios componentes y se interpreta más lento. Puede ser útil en M5/M15 como contexto de régimen."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-12  
official_code_date: 2025-04-23  
---

## 🟨 Arms Ease of Movement (6/10)  

**Nombre del archivo:** [`EMV.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/EMV.cs)  
**Nombre del indicador:** Arms Ease of Movement  
**Web oficial:** [ATAS — Arms Ease of Movement](https://help.atas.net/support/solutions/articles/72000602315)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Qué “facilidad” tiene el precio para desplazarse al ajustar el movimiento del punto medio por la fricción volumen/rango?  

![EMV](../../img/EMV.png)  

  

---  

### ⚙️ Parámetros configurables  

**Settings**  
- **MovingType (MaType):** tipo de media (EMA, SMA, WMA, SMMA, regresión lineal). 
- **Period:** periodo de suavizado (default 9).

  

---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Efficiency"  

  

---  

### 🧠 Uso más frecuente  

- **Contexto de tendencia:** EMV alto sugiere desplazamiento “fácil” (menos fricción).  
- **Divergencias:** precio avanza pero EMV cae = avance “caro” (posible agotamiento).  

  

---  

### 📊 Nivel de relevancia  
🔟 **6 / 10**  

✅ Más estable que métricas sin suavizado.  
✅ Útil para lectura de contexto (especialmente en marcos mayores).  
⛔ Lag intrínseco: en M1 suele llegar tarde para gatillo de entrada.  

  

---  

### 🎯 Estrategias de scalping donde se aplica  

- **Filtro de régimen (no trigger):** evitar operar continuaciones si el EMV indica fricción creciente.  
- **Mejor en M5/M15:** como capa de contexto para decisiones en M1.  

  

---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| Period | 5 | Reduce lag (a costa de más ruido). |  
| MovingType | EMA | Respuesta más rápida que SMA/WMA en práctica. |  

  

---  

### 🧪 Notas de desarrollo  

- Calcula: movimiento del punto medio entre velas / (volumen dividido por rango).
- Suaviza el EMV con una media seleccionable.
- Observación de maintainability: `_renderSeries` se nombra `"ADXR"` (probable arrastre), convendría renombrar a `"EMV"` por claridad.

  

---  

### ❗ Incoherencias o aspectos mejorables detectados  

- **Naming/UI:** etiqueta `"ADXR"` puede confundir en panel/leyenda. 

  

---  

### 🛠️ Propuestas de mejora  

- **P3 (Baja):** renombrar `_renderSeries` a `"EMV"` (solo UI/claridad).  
- **P2 (Media, creativo):** modo “Scalping”: exponer una versión *unsmoothed* adicional (o periodo mínimo) para usarlo como detector de cambio de régimen intradía.  
- **P2 (Media):** normalización por sesión (z-score) para que sea comparable entre sesiones con volatilidad distinta.  

  

---  

### 💎 Valor Reutilizable (Código Donante)  

- **Selección dinámica de medias** (patrón de switch + recalculo) si te interesa reutilizarlo en otros indicadores multi-MA. 

  

---  

### ✍️ La opinión de ChatGPT sobre el Indicador  

En un sistema de scalping para ES (S&P 500), EMV no debería ser “botón de disparo”. Donde sí puede brillar es como **capa de contexto**: régimen de fricción, confirmación de agotamiento, o filtro para evitar perseguir movimientos cuando el mercado deja de moverse “con facilidad”. Si decides mantenerlo, lo haría como reserva y lo usaría en M5/M15, no como señal primaria en M1.  

  

---  

### 📈 Veredicto: ¿Es útil para Scalping?  

**Limitado.**  

Útil como contexto/filtro; no recomendado como trigger principal en M1.  

**Acción:** **Conservar (Reserva)** 