---

# 1. IDENTIFICACIÓN  
cs_file: Pivots.cs  
name: Pivots  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Structural Levels  
comparison_group: "Structural Levels"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8/10  
score_potential: 8/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P2  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Cuáles son los pivots y niveles derivados (S/R) relevantes del periodo seleccionado para mapear soportes/resistencias probables?  
gemini_summary: "Mapa clásico de niveles ampliamente observado. Muy útil como estructura base, pero con riesgo de saturación y baja selectividad sin filtro adicional."  
competitor_notes: "Frente a Camarilla, Pivots es más estándar; frente a LevelsLolo carece de jerarquía externa y priorización; frente a Murrey es menos 'framework', más utilitario; frente a Round Numbers aporta más estructura formal."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🧱 Pivots (8/10)  

**Nombre del archivo:** [`PivotsModif.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Pivots.cs)  
**Nombre del indicador:** Pivots  
**Web oficial:** [ATAS — Pivots modif](https://help.atas.net/support/solutions/articles/72000602446)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuáles son los pivots y niveles derivados (S/R) relevantes del periodo seleccionado para mapear soportes/resistencias probables?  

![PivotsModif](../../img/PivotsModif.png)


---

### ⚙️ Parámetros configurables  
- **(Periodo / Range)**: Selección del marco para cálculo (diario/semanal/mensual, según implementación).  
- **RenderPeriodsFilter**: Número de periodos a dibujar (controla ruido visual).  
- **UseCustomSession**: Activar sesión personalizada (RTH) para cálculos consistentes con futuros USA.  
- **SessionBegin / SessionEnd**: Ventana horaria de sesión si se usa modo custom.  
- **(Estilos por nivel)**: Colores/estilos para PP, S1–S3, R1–R3 (según series).  
- **(Texto / ubicación)**: Mostrar etiquetas y ubicación de texto (si aplica).  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Structural Levels  
**Comparison Group:** "Structural Levels"  


---

### 🧠 Uso más frecuente  
* Definir **zonas de referencia** (PP, R1/R2, S1/S2) para plan de sesión.  
* Identificar “targets naturales” en días de rango y mean-reversion.  
* Combinar con Order Flow para validar rechazos/rupturas en S/R.  


---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  

✅ Nivelado clásico y ampliamente observado (valor de consenso).  
✅ Muy útil como mapa de contexto con mínimo coste computacional.  
⛔ Puede generar exceso de niveles y sesgo visual si no se filtra.  


---

### 🎯 Estrategias de scalping donde se aplica  
* **Reversión en niveles**: buscar absorción/agotamiento en S1/R1.  
* **Break & retest**: ruptura de PP y retesteo con confirmación por delta/footprint.  
* **Targets de salida**: usar pivots como TP parciales.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| UseCustomSession | Activado (RTH) | Alinea niveles con sesión relevante de ES. |  
| SessionBegin / SessionEnd | RTH | Evita mezclar overnight si tu sistema es RTH-centric. |  
| RenderPeriodsFilter | 1–3 | Reduce clutter (solo hoy + 1–2 históricos). |  
| Etiquetas | Minimal | Menos ruido visual; el nivel es lo importante. |  


---

### 🧪 Notas de desarrollo  
* Cálculo ligero; el riesgo principal es la coherencia de sesión (RTH/ETH).  
* Prioridad: estabilidad y claridad visual por encima de “feature creep”.  


---

### ❗ Incoherencias o aspectos mejorables detectados  
* Sin un filtro explícito de relevancia, el trader puede sobreinterpretar niveles secundarios.  


---

### 🛠️ Propuestas de mejora  
* Preset “RTH ES scalping”: sesión + filtro + estilos ya optimizados.  
* Opción de atenuar niveles secundarios (alpha/line width).  


---

### 💎 Valor Reutilizable (Código Donante)  
* N/A  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
Pivots es una estructura clásica con valor de consenso. No gana el torneo porque no jerarquiza ni integra fuentes externas, pero es una reserva P2 sólida para mapas de sesión y objetivos naturales.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  
**Sí**  

Especialmente como mapa de contexto y targets, siempre que se combine con confirmación por Order Flow para evitar entradas “por fe”.  

**Acción:** **Conservar (Reserva)**  


