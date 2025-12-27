---

# 1. IDENTIFICACIÓN  
cs_file: Pivots.cs  
name: Pivots  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Session-Derived Reference Levels  
comparison_group: "Session-Derived Reference Levels"  

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
description: ¿Cuáles son los pivots y niveles derivados (S/R) relevantes del periodo seleccionado para mapear soportes y resistencias probables de la sesión?  
gemini_summary: "Indicador de referencia clásico y ampliamente observado. Útil como mapa contextual base, pero sin jerarquía ni filtrado interno; su valor depende del uso disciplinado junto a Order Flow."  
competitor_notes: "Frente a Camarilla es más simple y estable, con menor ruido visual. Ninguno gana el torneo: ambos son niveles derivados del precio sin causalidad externa, por lo que quedan como reservas P2."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-27  
official_code_date: 2025-04-23  

---

## 🧭 Pivots (8/10)  

**Nombre del archivo:** [`PivotsModif.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Pivots.cs)  
**Nombre del indicador:** Pivots  
**Web oficial:** [ATAS — Pivots modif](https://help.atas.net/support/solutions/articles/72000602446)  
**Compatibilidad:** ATAS Stable/Latest  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuáles son los pivots y niveles derivados (S/R) relevantes del periodo seleccionado para mapear soportes y resistencias probables de la sesión?  

![PivotsModif](../../img/PivotsModif.png)


---

### ⚙️ Parámetros configurables  
- **Periodo / Range**: Marco temporal usado para el cálculo (diario, semanal, etc.).  
- **UseCustomSession**: Fuerza sesión personalizada (RTH) para futuros USA.  
- **SessionBegin / SessionEnd**: Ventana horaria usada en modo sesión custom.  
- **RenderPeriodsFilter**: Número de periodos históricos a dibujar.  
- **Estilos por nivel**: Colores y estilos para PP, S/R principales y secundarios.  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Session-Derived Reference Levels  
**Comparison Group:** "Session-Derived Reference Levels"  


---

### 🧠 Uso más frecuente  
- Mapa base de **contexto intradía** (PP, S1/R1 como referencias).  
- Definición de **targets naturales** en días de rango.  
- Validación de **rechazos o rupturas** solo tras confirmación de Order Flow.  


---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  

✅ Amplio consenso de mercado (muchos traders lo observan).  
✅ Coste computacional y visual bajo si se filtra correctamente.  
⛔ Puede inducir sobreoperativa si se interpreta como señal y no como mapa.  


---

### 🎯 Estrategias de scalping donde se aplica  
- **Fade en niveles** con absorción confirmada.  
- **Break & retest** del PP con validación de delta/volumen.  
- **Gestión de salidas** usando pivots como objetivos parciales.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| --- | --- | --- |  
| UseCustomSession | Activado (RTH) | Alinea niveles con la sesión relevante del ES. |  
| RenderPeriodsFilter | 1–2 | Reduce clutter en M1. |  
| Estilos | Minimal | El nivel es referencia, no señal. |  


---

### 🧪 Notas de desarrollo  
- Indicador ligero; la principal fuente de error es la **coherencia de sesión** (RTH vs ETH).  


---

### ❗ Incoherencias o aspectos mejorables detectados  
- Falta de jerarquía interna entre niveles principales y secundarios.  


---

### 🛠️ Propuestas de mejora  
- Preset específico “ES RTH Scalping”.  
- Atenuar niveles secundarios mediante alpha o grosor.  


---

### 💎 Valor Reutilizable (Código Donante)  
* N/A  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  
Pivots cumple bien su función como estructura convencional de consenso. No debe liderar el mapa del sistema, pero sí puede complementar la lectura cuando se usa con disciplina y confirmación de Order Flow.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí**, como **reserva contextual**, nunca como señal directa.  

**Acción:** **Conservar (Reserva)**  



