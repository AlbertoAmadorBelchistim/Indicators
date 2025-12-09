---
# 1. IDENTIFICACIÓN
cs_file: MultiMarketPower.cs
name: CVD pro(multi) / Multi Market Powers
version: ATAS Stable

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Delta
comparison_group: "Cumulative Delta"

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
description: ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional) en tiempo real?
gemini_summary: "El Santo Grial del Delta Acumulado. Supera a todos los demás CVDs porque permite desglosar la agresión en 5 capas simultáneas (Retail vs Institucional) en un solo panel. Su arquitectura de paso único lo hace extremadamente eficiente."
competitor_notes: "Vence a 'Market Power' (solo 1 filtro) y 'Cumulative Delta' (sin filtros) por su capacidad de disección del flujo sin penalizar el rendimiento."
reusable_code: "La arquitectura de bucle único para clasificar trades en buckets es un patrón de diseño excelente para cualquier indicador multifiltro."

# 6. METADATOS
analysis_date: 2025-11-21
official_code_date: 2025-08-14
---

## 🏆 CVD pro(multi) / Multi Market Powers (10/10)

**Nombre del archivo:** [`MultiMarketPower.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MultiMarketPower.cs)  
**Nombre del indicador:** CVD pro(multi) / Multi Market Powers  
**Web oficial:** [ATAS — CVD pro(multi)](https://help.atas.net/support/solutions/articles/72000602434)  
**Compatibilidad:** ATAS versión latest y superiores.  
**Última revisión del código oficial:** 2025-08-14  

> **La Pregunta Clave:** ¿Cómo se distribuye el delta acumulado entre 5 rangos de tamaño de orden diferentes (filtro institucional) en tiempo real?

![MultiMarketPower](../../img/MultiMarketPower.png)

---

### ⚙️ Parámetros configurables

Este indicador es una "navaja suiza" que permite configurar hasta 5 líneas de delta independientes.

#### 📊 Configuración General (Filters)
* **CumulativeTrades:**
    * `True`: Muestra la tendencia de toda la sesión (CVD clásico).
    * `False`: Reinicia el cálculo en cada vela (Delta por vela desglosado).

#### 🧰 Filtros de Volumen (Filter 1 - 5)
Cada uno de los 5 filtros tiene su propia lógica independiente:
* **Enabled (UseFilterX):** Activa/Desactiva el cálculo. (Recomendado: Desactivar los que no se usen para ahorrar ciclos CPU, aunque es muy eficiente).
* **Minimum Volume:** El tamaño de orden mínimo para entrar en este "cubo".
* **Maximum Volume:** El tamaño máximo. *Truco: Poner `0` significa "Sin límite" (infinito).*
* **Visuals (Color/Width):** Personalización completa.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Delta  
**Comparison Group:** "Cumulative Delta"  

---

### 🧠 Uso más frecuente

* **Radiografía de Participantes:** Diferenciar si una ruptura es apoyada por "Ballenas" (Filtros 4-5) o es puro FOMO "Retail" (Filtro 1).  
* **Divergencias de Calidad:** El precio sube, el CVD total sube, pero el CVD Institucional (Filtros grandes) baja. Señal de trampa alcista inminente.  
* **Limpieza de Ruido:** Usar solo el Filtro 2 o 3 en adelante para ver la "verdadera" subasta, eliminando el ruido de los algos de alta frecuencia (HFT) de 1 contrato.  

---

### 📊 Nivel de relevancia
🔟 **10 / 10**

✅ **Potencia de Fuego:** Consolida la función de 5 indicadores en uno solo.  
✅ **Eficiencia:** Itera los datos una sola vez, clasificando cada trade en su categoría al vuelo.  
✅ **Ventaja Competitiva:** Permite ver lo que la mayoría de traders retail no ven: la acción oculta de las órdenes grandes.  
⛔ **Curva de Aprendizaje:** Requiere entender bien qué volumen se considera "grande" en cada instrumento (S&P 500 vs Nasdaq).  

---

### 🎯 Estrategias de scalping donde se aplica

* **Absorción Institucional:** Precio rompiendo máximos + Filtro 5 (Whales) plano o bajando = **Reversal Short**.  
* **Validación de Breakout:** Precio rompiendo nivel + Filtros 3, 4 y 5 con pendiente agresiva = **Long Continuation**.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Filtro | Rango (Contratos) | Visual (Color / Grosor) | Rol & Lógica |
| :--- | :--- | :--- | :--- |
| **CumulativeTrades** | `True` | N/A | Tendencia de Sesión. |
| **Filtro 1** | Min `0` - Max `5` | **Gris / 1px** | **Ruido (Retail/HFT).** Fondo neutro, casi invisible. |
| **Filtro 2** | Min `6` - Max `20` | **Cian / 2px** | **Peces Pequeños.** Color frío, actividad normal. |
| **Filtro 3** | Min `21` - Max `50` | **Azul Real / 2px** | **Profesional Local.** Transición a relevante. |
| **Filtro 4** | Min `51` - Max `100` | **Naranja / 3px** | **Institucional Táctico.** Alerta visual (Color cálido). |
| **Filtro 5** | Min `101` - Max `0` | **Rojo o Magenta / 4px** | **Ballenas (Whales).** La "Línea de la Verdad". Máxima prioridad. |

---

### ✨ Mejoras introducidas (Oficial)
* **Algoritmo Single-Pass:** El código oficial ya está altamente optimizado. No filtra la lista 5 veces, sino que recorre cada trade y pregunta `IsFiltered` para cada categoría, asignando los deltas en una sola pasada.

---

### 🧪 Notas de desarrollo

* **Arquitectura:** Usa `List<CumulativeTrade>` y eventos `OnNewTrade`.
* **Manejo de Historia:** Solicita `CumulativeTradesRequest` al inicio para pintar el pasado sin necesidad de tener el chart abierto.
* **Estabilidad:** Maneja correctamente la desconexión/conexión de datos (`_bigTradesIsReceived`).

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Falta Custom Session:** A diferencia de su hermano menor (`CumulativeDelta`), este indicador no permite reiniciar el delta a una hora específica (ej. 15:30). Acumula desde la carga de datos o inicio de sesión del servidor.

---

### 🛠️ Propuestas de mejora

* **P2 (Alta):** Implementar la lógica `CheckStartBar` del indicador `CumulativeDelta` para permitir reinicio en RTH.

---

### 💎 Valor Reutilizable (Código Donante)

* **Estructura de Filtrado:** El patrón de 5 filtros configurables es el estándar de oro para cualquier indicador de volumen segmentado.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es, posiblemente, **el indicador más valioso de todo el paquete ATAS** para un trader de Order Flow moderno. En mercados dominados por algoritmos, ver el "Delta Total" es ver una mentira promediada. Ver el "Delta por Capas" con `MultiMarketPower` es ver la realidad.
Es un **Core P1** indiscutible.

**Propuestas de Acción:**
* Planificar la fusión con la lógica de sesión personalizada.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Imprescindible.**

Es la base para filtrar falsas rupturas y detectar absorciones reales.

**Acción:** **Conservar (Core)**