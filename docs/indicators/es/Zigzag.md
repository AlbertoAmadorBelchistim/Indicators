---
# 1. IDENTIFICACIÓN  
cs_file: Zigzag.cs  
name: ZigZag Pro  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Swing-Derived Structure  
comparison_group: "Swing-Derived Structure"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 10/10  
score_potential: 10/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Qué revelan las métricas acumuladas (Delta, Volumen, Ticks, Tiempo) de cada onda de precio sobre la estructura real del mercado y el equilibrio esfuerzo–resultado?  
gemini_summary: "Analizador estructural de ondas de precio con métricas acumuladas. Convierte la estructura del mercado en información cuantificable y accionable, combinando Price Action con Order Flow por tramo."  
competitor_notes: "No compite con indicadores de clúster o footprint. Su función es estructural: definir ondas, comparar impulsos vs correcciones y detectar agotamiento mediante métricas acumuladas."  
reusable_code: "Gestión de estado de ondas + acumuladores por tramo (delta, volumen, ticks, tiempo) y renderizado contextual de etiquetas."  

# 6. METADATOS  
analysis_date: 2025-12-25  
official_code_date: 2025-04-23  
---  

## 🟦 ZigZag Pro (10/10)  

**Nombre del archivo:** [`Zigzag.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Zigzag.cs)  
**Nombre del indicador:** ZigZag Pro  
**Web oficial:** [ATAS — ZigZag Pro](https://help.atas.net/support/solutions/articles/72000602632)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Qué revelan las métricas acumuladas de cada onda de precio sobre la estructura del mercado y la relación esfuerzo–resultado?

![Zigzag](../../img/Zigzag.png)


---

### ⚙️ Parámetros configurables  

- **Mode**: `Ticks`, `Percentage`, `Absolute` — método de confirmación del giro de onda.  
- **Percentage / Value**: Umbral de reversión necesario para confirmar una nueva onda.  
- **Labels**: Mostrar **Delta**, **Volume**, **Ticks**, **Time**, **Bars** por onda.  
- **Visuals**: Colores, tamaño del texto y desplazamientos de etiquetas.  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Swing-Derived Structure  
**Comparison Group:** "Swing-Derived Structure"  


---

### 🧠 Uso más frecuente  

* **Estructura de mercado:** Identificar HH / HL / LH / LL de forma objetiva.  
* **Wyckoff – Esfuerzo vs Resultado:** Comparar volumen y delta entre ondas impulsivas y correctivas.  
* **Agotamiento:** Nuevo máximo de precio con menor delta o volumen acumulado por onda.  
* **Contexto para ejecución:** Definir sesgo estructural antes de usar Order Flow micro.  


---

### 📊 Nivel de relevancia  
🔟 **10 / 10**  

✅ Traduce la estructura del mercado en métricas cuantificables.  
✅ Permite leer divergencias estructurales imposibles de detectar vela a vela.  
✅ Altamente configurable y estable en cualquier activo.  
⛔ El último tramo repinta hasta confirmación del giro (comportamiento inherente al ZigZag).  


---

### 🎯 Estrategias de scalping donde se aplica  

* **Wave Exhaustion:** Onda alcista con volumen alto pero delta débil o negativo → posible absorción.  
* **1-2-3 / Change of Character:** Uso del ZigZag para confirmar cambio estructural sin subjetividad.  
* **Pullbacks estructurales:** Correcciones con poco esfuerzo frente a impulsos dominantes.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

- **Mode:** `Ticks`  
- **Value:** `12` (≈ 3 puntos ES) o `20` según volatilidad.  
- **Labels:** Delta + Volume activados.  


---

### 🧪 Notas de desarrollo  

* Mantiene el estado de la tendencia mediante `_direction`.  
* Una onda se cierra solo cuando el precio retrocede más que `requiredChange`.  
* En el cierre de cada onda se calculan y fijan sus acumulados (delta, volumen, ticks, tiempo).  
* El último tramo **siempre repinta** hasta confirmación: es comportamiento correcto, no un bug.  


---

### 💎 Valor Reutilizable (Código Donante)  

* Patrón de acumulación por tramo (ondas).  
* Renderizado contextual de métricas estructurales directamente en el gráfico.  


---

### ✍️ La opinión de Gemini sobre el Indicador  

ZigZag Pro es un **indicador estructural CORE**. Su valor no está en predecir entradas, sino en convertir el Price Action en información cuantificable. Es el puente natural entre estructura y Order Flow, y debe vivir fuera de cualquier grupo de “Cluster” o “Statistics”.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí.**  

Es el **microscopio de la estructura de mercado** sobre el que se apoyan el resto de herramientas.  

**Acción:** **Conservar (Core)**  
