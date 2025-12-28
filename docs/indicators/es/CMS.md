---

# 1. IDENTIFICACIÓN  
cs_file: CMS.cs  
name: Clear Method Swing Line  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Swing-Derived Structure  
comparison_group: "Swing-Derived Structure"  

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
description: ¿Cuál es el sesgo estructural actual del swing (alcista/bajista) y cuál es el último nivel de invalidación asociado?  
gemini_summary: "Línea de swing objetiva sin parámetros: excelente para definir sesgo (solo largos/solo cortos) y nivel de invalidación. No genera niveles horizontales ni cuantifica ondas, por eso queda como soporte P2 frente a COREs especializados."  
competitor_notes: "Complementa a Fractals/Zigzag: CMS da sesgo y nivel dinámico, mientras los otros dan niveles horizontales o métricas por tramo. No es redundante, pero tampoco reemplaza a un mapa de niveles."  
reusable_code: "SplitLines() para transiciones limpias de series; patrón de series internas tipo hh/hl/lh/ll."  

# 6. METADATOS  
analysis_date: 2025-12-28  
official_code_date: 2025-04-23  




---

## 🟦 Clear Method Swing Line (8/10)

**Nombre del archivo:** [`CMS.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/CMS.cs)  
**Nombre del indicador:** Clear Method Swing Line  
**Web oficial:** [ATAS — Clear Method Swing Line ](https://help.atas.net/support/solutions/articles/72000602257)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es el sesgo estructural actual del swing (alcista/bajista) y cuál es el último nivel de invalidación asociado?  

![CMS](../../img/CMS.png)  



---

### ⚙️ Parámetros configurables

* Este indicador no expone parámetros en UI.  



---

### 🧭 Clasificación
**Grupo:** Market Structure  
**Subgrupo:** Swing-Derived Structure  
**Comparison Group:** "Swing-Derived Structure"  



---

### 🧠 Uso más frecuente

* Definir sesgo: operar solo en dirección de la línea activa (up/down).  
* Usar la línea como nivel dinámico de invalidación estructural.  
* Identificar transiciones de fase (cambios de línea) como cambio de carácter.  



---

### 📊 Nivel de relevancia
🔟 **8 / 10**

✅ Objetividad total (sin parámetros) y lectura de sesgo muy clara.  
✅ Útil como “filtro” para evitar operar contra estructura.  
⛔ Caja negra y no configurable; difícil ajustar sensibilidad.  
⛔ No genera niveles horizontales persistentes ni métricas por tramo.  



---

### 🎯 Estrategias de scalping donde se aplica

* **Filtro direccional**: solo setups largos si swing alcista y viceversa.  
* **Cambio de carácter**: cuando cambia la línea, reducir agresividad o cambiar plan.  
* **Invalidación**: usar el nivel de la línea como referencia de stop estructural.  



---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* No requiere configuración.  
* Recomendación operativa: usarlo como filtro, no como gatillo único.  



---

### 🧪 Notas de desarrollo

* Usa múltiples series internas (`hh/hl/lh/ll` y variantes) para consolidar estados y validar swing.  
* Dibuja dos series (up/down) y corta líneas con `SplitLines()` para evitar uniones visuales incorrectas.  



---

### ❗ Incoherencias o aspectos mejorables detectados

* No se detectan bugs evidentes; la principal limitación es la falta de controles de sensibilidad.  



---

### 🛠️ Propuestas de mejora

* Etiquetas opcionales (BOS / CHoCH) en los puntos de transición, sin alterar la lógica base.  
* Opción de “modo sensible / modo conservador” si el framework lo permite, manteniendo la objetividad como principio.  



---

### 💎 Valor Reutilizable (Código Donante)

* `SplitLines()` para transiciones limpias entre series (patrón reutilizable en indicadores de línea estructural).  



---

### ✍️ La opinión de ChatGPT sobre el Indicador

CMS es una herramienta de contexto potente para M1: define el sesgo y un nivel de invalidación sin necesidad de parametrización. En un sistema serio esto vale mucho, pero su rol natural es “filtro de régimen” (P2) más que “mapa de niveles” o “análisis cuantitativo de ondas” (P1).  



---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí**  

Como filtro de sesgo e invalidación estructural.  

**Acción:** **Conservar (Reserva)**  

