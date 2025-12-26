---

# 1. IDENTIFICACIÓN  
cs_file: MaxLevels.cs  
name: Maximum Levels  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume Profile  
comparison_group: "Profile Levels (POC/VA)"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8.5/10  
score_potential: 8.5/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P2  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿En qué nivel de precio se concentró el máximo volumen (o delta) de un periodo cerrado y cómo actúa ese nivel como referencia estructural?  
gemini_summary: "Indicador estructural de niveles estáticos: identifica el precio con mayor concentración de volumen (POC) o delta en periodos cerrados. Muy fiable como referencia heredada, aunque sin información dinámica."  
competitor_notes: "Frente a DynamicLevels (perfil vivo) y DynamicLevelsChannel (perfil móvil), MaxLevels ofrece un único nivel estático, más simple pero extremadamente robusto como soporte/resistencia histórica."  
reusable_code: "Uso ejemplar de FixedProfileRequest para cálculo asíncrono de perfiles históricos y render manual eficiente de líneas y etiquetas."  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🟡 Maximum Levels (8.5/10)  

**Nombre del archivo:** [`MaxLevels.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MaxLevels.cs)  
**Nombre del indicador:** Maximum Levels  
**Web oficial:** [ATAS — Maximum Levels](https://help.atas.net/support/solutions/articles/72000602426)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿En qué nivel de precio se concentró el máximo volumen (o delta) de un periodo cerrado y cómo actúa ese nivel como referencia estructural?  

![MaxLevels](../../img/MaxLevels.png)


---

### ⚙️ Parámetros configurables  

- **Period:** Periodo de referencia cerrado (`CurrentDay`, `LastDay`, `LastWeek`, `LastMonth`, `Contract`, etc.).  
- **Type:** Métrica a maximizar (`Volume`, `Bid`, `Ask`, `Delta`, `PositiveDelta`, `NegativeDelta`).  
- **TradingSession:** Filtro de sesión específica (RTH/ETH).  
- **LineLength:** Longitud de la línea proyectada a la derecha.  
- **ShowPrice / ShowValue:** Mostrar precio y/o valor en la etiqueta.  
- **LineWidth / Colors / Font:** Ajustes visuales de línea y texto.  
- **Alerts:** Alerta cuando el precio toca el nivel.  


---

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Profile Levels (POC/VA)"  


---

### 🧠 Uso más frecuente  

* Marcar el **POC del día anterior** como soporte/resistencia clave.  
* Identificar **niveles semanales/mensuales** de alto interés institucional.  
* Usar máximos de **delta positivo/negativo** como zonas de defensa de posiciones.  


---

### 📊 Nivel de relevancia  
🔟 **8.5 / 10**  

✅ Nivel único, claro y robusto.  
✅ Ideal como referencia histórica (“naked POC”).  
⛔ No aporta información sobre evolución o aceptación actual del valor.  


---

### 🎯 Estrategias de scalping donde se aplica  

* **POC bounce:** primer test del POC del periodo previo.  
* **Nivel heredado:** usar el máximo de volumen semanal como imán o rechazo.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| :--- | :--- | :--- |  
| **Period** | `LastDay` | Referencia inmediata para la sesión actual. |  
| **Type** | `Volume` | POC clásico y más estable. |  
| **LineLength** | `300` | Mantiene el nivel visible sin saturar el gráfico. |  
| **Alerts** | `false` | Activar solo en niveles estructurales mayores. |  


---

### 🧪 Notas de desarrollo  

* Uso correcto de **cálculo asíncrono** para perfiles cerrados (`FixedProfileRequest`).  
* Renderizado manual en `OnRender` eficiente y estable.  
* Indicador puramente **estructural**: no depende de ticks en tiempo real.  


---

### ❗ Incoherencias o aspectos mejorables detectados  

* No diferencia entre RTH/ETH si el usuario no configura explícitamente la sesión.  
* No gestiona múltiples máximos secundarios (solo el nivel ganador).  


---

### 🛠️ Propuestas de mejora  

* Opción para conservar **POCs antiguos no testeados** (“naked POCs”).  
* Posibilidad de mostrar **top-N niveles** (2–3 máximos) para mayor contexto.  


---

### 💎 Valor Reutilizable (Código Donante)  

* Patrón de **FixedProfileRequest** como referencia para cualquier perfil histórico.  
* Render manual desacoplado del cálculo (buena separación de responsabilidades).  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  

Maximum Levels es simple y precisamente por eso es fiable. No intenta explicar el mercado, solo marcar dónde estuvo el mayor interés real. Como nivel heredado, encaja perfectamente en un sistema de scalping disciplinado.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí, como nivel estructural.**  

Debe combinarse con Order Flow en tiempo real para entradas precisas.  

**Acción:** **Conservar (Reserva)**  



