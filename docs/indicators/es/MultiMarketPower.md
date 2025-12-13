---
# 1. IDENTIFICACIÓN
cs_file: MultiMarketPowerModif.cs  
name: CVD pro(multi) / Multi Market Powers (Modif)  
version: Custom v1.2 (Visual Update)  

# 2. CLASIFICACIÓN
group: Order Flow  
subgroup: Delta  
comparison_group: "Cumulative Delta"  

# 3. VALORACIÓN (Score & Priority)
score_current: 10/10  
score_potential: 10/10  
file_state: Estable  
effort: Medio  
action_priority: Baja  
system_priority: P1  

# 4. DECISIÓN
recommended_action: Conservar (Core)  

# 5. ANÁLISIS
description: ¿Está el Smart Money empujando ahora mismo o estamos ante una corrección pasiva dentro de una tendencia acumulada masiva?  
gemini_summary: "La versión v1.2 soluciona el problema de 'ceguera por escala' en acumulados grandes. Introduce un sistema de coloración por pendiente (Slope Coloring) y una línea de señal SMA, convirtiendo el histograma en un monitor de momentum institucional en tiempo real, legible incluso en extremos del gráfico."  
competitor_notes: "Supera a cualquier CVD estándar al permitir leer giros de mercado (reversals) mediante cambios de intensidad de color, independientemente de la posición absoluta del delta."  
reusable_code: "Lógica de 'Slope Coloring' de 4 estados y gestión dinámica de visibilidad de series en ATAS."  

# 6. METADATOS
analysis_date: 2025-12-13  
official_code_date: 2025-08-14  
user_modification_date: 2025-12-13  
---

## 🏆 CVD pro(multi) / Multi Market Powers — Modif (10/10)

**Nombre del archivo:** [`MultiMarketPowerModif.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/MultiMarketPowerModif.cs)  
**Nombre del indicador:** CVD pro(multi) / Multi Market Powers (Modif)  
**Web oficial base:** [ATAS — CVD pro(multi)](https://help.atas.net/support/solutions/articles/72000602434) 
**Compatibilidad:** ATAS Latest / Beta.  
**Última revisión del código base:** [`MultiMarketPower.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MultiMarketPower.cs): 2025-08-14  
**Última revisión del código modificado:** 2025-12-13  

> **La Pregunta Clave:** ¿Está el Smart Money empujando ahora mismo o estamos ante una corrección pasiva dentro de una tendencia acumulada masiva?

![MultiMarketPower](../../img/MultiMarketPower.png)


---

### ⚙️ Parámetros configurables

#### 🧭 General
* **View Mode:**
    * `Lines`: Muestra los 5 CVD segmentados individualmente.
    * `SmartMoneySpread`: Muestra el histograma sintético (Institucional - Retail).

#### 🕒 Session
- **Session Mode**
  - `None`: Acumulación continua.
  - `Default Session`: Reinicio por sesión oficial del instrumento.
  - `Custom Session`: Reinicio a una hora definida (ej. RTH 09:30).
- **Custom Session Start**
  - Hora exacta de reinicio del delta.
- **Sessions Back**
  - Número de sesiones históricas a reconstruir.

#### Visuals - Spread (NUEVO v1.2)
* **Use 4-Color System:** (`True`/`False`) Activa la lógica de colores basada en la pendiente de la SMA. Fundamental para ver cambios de tendencia local.
* **Show Signal Line (SMA):** Muestra una Media Móvil Simple sobre el histograma para referencia visual.
* **Signal Period:** Periodo de la SMA (Defecto: 14) para el cálculo de pendiente.
* **Colores de Estado:**
    * *Pos / SMA Up (Lime):* Tendencia alcista fuerte.
    * *Pos / SMA Down (DarkGreen):* Corrección en zona positiva.
    * *Neg / SMA Up (DarkRed):* Recuperación/Freno en zona negativa.
    * *Neg / SMA Down (Red):* Tendencia bajista fuerte.

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

* **Detección de "Freno" (Reversal):** El histograma cambia de *Rojo Vivo* a *Rojo Oscuro* (Neg Rising). Indica que la venta agresiva ha cesado, aunque el acumulado siga siendo muy negativo.
* **Confirmación de Ruptura:** El precio rompe nivel y el histograma muestra color brillante (Lime/Red) con la SMA inclinada a favor.
* **Divergencia de Momentum:** El precio hace un nuevo mínimo, pero el histograma no logra poner la barra en *Rojo Vivo*, quedándose en *Oscuro* o divergiendo respecto a la SMA.

---

### 📊 Nivel de relevancia
🔟 **10 / 10**

✅ **Feedback Visual Inmediato:** El sistema de 4 colores elimina la necesidad de interpretar numéricamente la pendiente.  
✅ **Solución de Escala:** Permite ver actividad pequeña relevante incluso cuando el acumulado es de -15.000 contratos.  
✅ **Limpieza:** La UI reagrupada facilita la configuración rápida.  
⛔ **Curva de Aprendizaje:** Requiere entender qué significa cada uno de los 4 colores (fuerza vs corrección).  
---

### 🎯 Estrategias de scalping donde se aplica

* **Scalping de Continuación:** Entradas a favor de tendencia cuando el color es Brillante y la SMA tiene pendiente pronunciada.
* **Giros en Soportes/Resistencias:** Buscar el cambio de tono (de Brillante a Oscuro) justo en el testeo del nivel clave.

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Justificación |
| :--- | :--- | :--- |
| **View Mode** | `SmartMoneySpread` | Visión sintética Institucional vs Retail. |
| **Session Mode** | `Default Session` | Para ver qué han hecho los distintos actores durante el overnight |
| **CumulativeTrades** | `True` | Obligatorio para reconstruir órdenes grandes fragmentadas. |
| **Use 4-Color System** | `True` | **Esencial** para detectar cambios de flujo en tiempo real. |
| **Signal Period** | `14` | Balance entre suavizado y reactividad para 1M. |


| Filtro | Rango contratos | Visual | Rol operativo |
|------|-----------------|--------|---------------|
| F1 | 0 – 5 | Gris / 1px | Ruido / HFT |
| F2 | 6 – 20 | Cian / 2px | Retail |
| F3 | 21 – 50 | Azul Real / 2px | Profesional local |
| F4 | 51 – 100 | Naranja / 3px | Institucional táctico |
| F5 | ≥ 101 | Magenta / 4px | Ballenas reales |


---

### ✨ Mejoras introducidas (Oficial/Base)
* **Algoritmo Single-Pass:** El código base ya optimiza la clasificación de trades, evitando recorrer la lista múltiples veces para cada filtro.


---
### ✨ Mejoras introducidas (Modif v1.1)

- **Smart Money Spread**
  - Histograma sintético: (Filtros 4 + 5) – (Filtros 1 + 2).
- **Sesiones Históricas**
  - Reconstrucción multi-sesión mediante request por rango temporal.
- **Sesión Custom**
  - Reinicio exacto del delta a hora definida (RTH).
- **Preset Profesional ES**
  - Rangos y visual optimizados para scalping institucional en S&P 500.
- **Correcciones de estabilidad**
  - Fix de indexado de colores en histórico.
  - Cursor correcto en reconstrucción tick-based.
  - Manejo seguro de replay realtime tras histórico.
  
### ✨ Mejoras añadidas (Modif v1.2)

* **Slope Coloring (4-Color System):** Implementación de lógica visual que compara la tendencia local de la SMA en lugar del valor absoluto cero. Permite identificar correcciones dentro de tendencias fuertes.
* **SMA Signal Line:** Inclusión de una línea de señal integrada visualmente con el histograma para referencia de "Cero Flotante".
* **UI Reorganization:** Agrupación de todos los parámetros visuales en una categoría dedicada "2. Visuals - Spread" para mejorar la UX.
* **Visibility Fix:** Corrección del bug donde las líneas individuales o la media móvil quedaban "flotando" al cambiar entre modos de vista.

---

### 🧪 Notas de desarrollo

- Arquitectura híbrida (`CumulativeTrade` + `MarketDataArg`).
- Reconstrucción histórica determinista y reproducible.
- Separación clara entre lógica de sesión, cálculo y visualización.
- Compatible con replay y carga tardía de datos.
- **Eficiencia:** El cálculo de la SMA y el color se realiza dentro del ciclo principal de actualización (`UpdateVisualsWithSMA`), sin añadir overhead significativo de procesamiento.
- **Lógica de Acumulación:** Se mantiene `CumulativeTrades = true` como estándar duro. Desactivarlo rompería la lógica de detección de institucionales al desglosar sus órdenes en ticks individuales.


---

### ❗ Incoherencias o aspectos mejorables detectados

* Actualmente no hay deuda técnica. La versión v1.2 es estable y robusta.


---

### 🛠️ Propuestas de mejora

* **P3 (Futuro):** Crear una colección de `Presets` hardcoded para diferentes instrumentos (ej. NQ vs ES vs RTY) para ajustar los umbrales de volumen automáticamente.


---

### 💎 Valor Reutilizable (Código Donante)

* Segmentación por buckets de volumen.
* Lógica genérica de sesión (Default / Custom / Back).
* Patrón de Spread sintético institucional.
* **Método `UpdateVisualsWithSMA`:** El bloque lógico que determina el color basado en `isSmaRising` y `CurrentSpread >= 0` es altamente portable a cualquier oscilador (MACD, Delta, etc.) para mejorar su lectura visual.


---

### ✍️ La opinión de Gemini sobre el Indicador

Esta modificación (v1.2) eleva el indicador de "informativo" a **"operativo"**. El problema clásico del Cumulative Delta es que, a mitad de sesión, el acumulado es tan grande que los movimientos pequeños (pero críticos para el scalping) se vuelven invisibles.

La solución de **Colores por Pendiente + SMA** resuelve esto elegantemente. Ahora, el trader no necesita calcular mentalmente si el histograma está subiendo o bajando; el color se lo grita. Es una herramienta **Core P1** indispensable para leer la intención institucional.



---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Es la herramienta definitiva para filtrar el ruido y alinear las entradas con la fuerza real del mercado, eliminando la ambigüedad de los osciladores tradicionales.

**Acción:** **Conservar (Core)**
