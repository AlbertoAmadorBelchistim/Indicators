---

# 1. IDENTIFICACIÓN  
cs_file: DynamicLevelsChannel.cs  
name: Dynamic Levels Channel  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume Profile  
comparison_group: "Profile Levels (POC/VA)"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 8.5/10  
score_potential: 9/10  
file_state: Estable  
effort: N/A  
action_priority: Baja  
system_priority: P2  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Dónde se sitúan el POC, VAH y VAL del valor reciente calculado sobre una ventana móvil de volumen?  
gemini_summary: "Perfil de valor dinámico basado en ventana móvil. Aporta mayor reactividad que el perfil acumulado, a costa de estabilidad estructural. Útil como complemento táctico."  
competitor_notes: "Frente a DynamicLevels (Core), es más sensible a cambios recientes pero menos representativo del valor institucional acumulado. Frente a MaxLevels, aporta VAH/VAL dinámicos en lugar de un único nivel estático."  
reusable_code: "Lógica de perfil móvil con buffer de barras y recálculo continuo de POC/VAH/VAL."  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🟡 Dynamic Levels Channel (8.5/10)  

**Nombre del archivo:** [`DynamicLevelsChannel.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DynamicLevelsChannel.cs)  
**Nombre del indicador:** Dynamic Levels Channel  
**Web oficial:** [ATAS — Dynamic Levels Channel](https://help.atas.net/support/solutions/articles/72000602381)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Dónde se sitúan el POC, VAH y VAL del valor reciente calculado sobre una ventana móvil de volumen?  

![Dynamic Levels Channel](../../img/DynamicLevelsChannel.png)


---

### ⚙️ Parámetros configurables  

- **Period:** Número de barras que forman la ventana móvil del perfil.  
- **CalcMode:** Fuente del cálculo (Volume, Delta, Bid, Ask).  
- **Days:** Límite de días históricos a procesar.  
- **Filter:** Filtro mínimo de volumen para incluir niveles.  
- **ShowPOC / ShowVAH / ShowVAL:** Activación individual de niveles.  
- **Colors / LineWidth:** Ajustes visuales de líneas y etiquetas.  


---

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Profile Levels (POC/VA)"  


---

### 🧠 Uso más frecuente  

* Detectar **cambios rápidos de valor** tras noticias o rupturas.  
* Complementar el perfil acumulado cuando el mercado rota con velocidad.  
* Identificar micro-zonas de aceptación recientes dentro de una sesión activa.  


---

### 📊 Nivel de relevancia  
🔟 **8.5 / 10**  

✅ Mayor reactividad que el perfil acumulado clásico.  
✅ Mantiene la lógica POC/VAH/VAL en entornos dinámicos.  
⛔ Menor estabilidad estructural: puede generar ruido en laterales.  


---

### 🎯 Estrategias de scalping donde se aplica  

* **Scalping reactivo:** entradas rápidas hacia POC móvil tras impulsos.  
* **Confirmación de rotación:** alineación del valor móvil con ruptura reciente.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| :--- | :--- | :--- |  
| **Period** | `50`–`150` | Ventana suficientemente sensible sin exceso de ruido. |  
| **CalcMode** | `Volume` | Estándar institucional para definir valor. |  
| **Filter** | `0`–`50` | Ajustar según liquidez del instrumento. |  
| **Days** | `2`–`5` | Evita sobrecarga histórica innecesaria. |  


---

### 🧪 Notas de desarrollo  

* El perfil se recalcula continuamente sobre una **ventana deslizante**.  
* Más costoso computacionalmente que DynamicLevels, pero aceptable en M1.  
* Indicador claramente de **contexto táctico**, no de estructura principal.  


---

### ❗ Incoherencias o aspectos mejorables detectados  

* Puede duplicar información si se usa simultáneamente con DynamicLevels sin una regla clara de prioridad.  


---

### 🛠️ Propuestas de mejora  

* Añadir preset “**Post-news / High Volatility**” con ventana más corta.  
* Opción para fijar (“freeze”) el valor móvil tras ruptura validada.  


---

### 💎 Valor Reutilizable (Código Donante)  

* Implementación de **perfil móvil por precio** reutilizable para otros indicadores reactivos.  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  

DynamicLevelsChannel es un perfil de valor **táctico**, pensado para mercados que cambian de régimen rápido. No sustituye al perfil acumulado, pero aporta una lectura adelantada del valor reciente cuando el mercado deja atrás el equilibrio previo.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí, como complemento.**  

Debe usarse con reglas claras de prioridad frente al perfil acumulado para evitar sobreinterpretación.  

**Acción:** **Conservar (Reserva)**  

