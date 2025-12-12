---
# 1. IDENTIFICACIÓN
cs_file: MultiMarketPowerModif.cs  
name: CVD pro(multi) / Multi Market Powers (Modif)  
version: Custom v1.1  

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
description: ¿Cómo se distribuye el delta acumulado separando participantes por tamaño de orden y sesión, permitiendo identificar Smart Money real frente a ruido y retail?  
gemini_summary: "La versión modificada de MultiMarketPower convierte un gran indicador en una herramienta definitiva de Order Flow. Añade lectura sintética de Smart Money, control de sesiones y una parametrización profesional orientada a scalping institucional."  
competitor_notes: "Supera claramente al Cumulative Delta clásico y a la versión oficial de MultiMarketPower al introducir sesiones históricas, Spread Smart Money y un preset operativo realista para ES."  
reusable_code: "Lógica de segmentación por volumen + detección de inicio de sesión reutilizable en cualquier indicador de flujo acumulado."  

# 6. METADATOS
analysis_date: 2025-12-12  
official_code_date: 2025-08-14  
user_modification_date: 2025-12-12  
---

## 🏆 CVD pro(multi) / Multi Market Powers — Modif (10/10)

**Nombre del archivo:** [`MultiMarketPowerModif.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/MultiMarketPowerModif.cs)  
**Nombre del indicador:** CVD pro(multi) / Multi Market Powers (Modif)  
**Web oficial base:** [ATAS — CVD pro(multi)](https://help.atas.net/support/solutions/articles/72000602434) 
**Compatibilidad:** ATAS latest / alpha / beta.  
**Última revisión del código base:** [`MultiMarketPower.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/MultiMarketPower.cs): 2025-08-14  
**Última revisión del código modificado:** 2025-12-12  

> **La Pregunta Clave:**  
> ¿Quién está moviendo realmente el mercado (retail, profesional o ballenas) y cómo cambia su agresión a lo largo de la sesión?

![MultiMarketPower](../../img/MultiMarketPower.png)

---

### ⚙️ Parámetros configurables

#### 🧭 General
- **View Mode**
  - `Lines`: Muestra los 5 CVD segmentados por tamaño de orden.
  - `SmartMoneySpread`: Histograma sintético (Smart Money – Dumb Money).
- **Spread Positive / Negative Color**
  - Colores configurables para el histograma de Spread.

#### 🕒 Session
- **Session Mode**
  - `None`: Acumulación continua.
  - `Default Session`: Reinicio por sesión oficial del instrumento.
  - `Custom Session`: Reinicio a una hora definida (ej. RTH 09:30).
- **Custom Session Start**
  - Hora exacta de reinicio del delta.
- **Sessions Back**
  - Número de sesiones históricas a reconstruir.

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

- **Identificación de Participantes:** Diferenciar ruido, retail, profesional e institucional real.  
- **Validación de Rupturas:** Breakouts confirmados solo si Smart Money acompaña.  
- **Absorciones:** Precio avanza pero el Spread Smart Money diverge.  
- **Contexto de Sesión:** Comparar comportamiento institucional entre sesiones consecutivas.  

---

### 📊 Nivel de relevancia
🔟 **10 / 10**

✅ Segmentación institucional real por tamaño de orden.  
✅ Lectura sintética del flujo mediante Smart Money Spread.  
✅ Control total de sesiones (histórico + realtime).  
⛔ Requiere comprensión de Order Flow avanzado.  

---

### 🎯 Estrategias de scalping donde se aplica

- **Fake Breakout:** Ruptura sin apoyo del Spread Smart Money.  
- **Continuation Trades:** Precio + Filtros 4–5 + Spread positivo creciente.  
- **Reversals en niveles:** Ballenas (Filtro 5) contra dirección del movimiento.  

---

### ⚙️ Parametrización óptima por defecto (Scalping ES 1M / 2500V)

| Filtro | Rango contratos | Visual | Rol operativo |
|------|-----------------|--------|---------------|
| F1 | 0 – 5 | Gris / 1px | Ruido / HFT |
| F2 | 6 – 20 | Cian / 2px | Retail |
| F3 | 21 – 50 | Azul Real / 2px | Profesional local |
| F4 | 51 – 100 | Naranja / 3px | Institucional táctico |
| F5 | ≥ 101 | Magenta / 4px | Ballenas reales |

- **CumulativeTrades:** `True`  
- **View Mode por defecto:** `SmartMoneySpread`  
- **Session Mode:** `Default Session`  
- **SessionsBack:** `1`  


---

### ✨ Mejoras introducidas (Oficial)
* **Algoritmo Single-Pass:** El código oficial ya está altamente optimizado. No filtra la lista 5 veces, sino que recorre cada trade y pregunta `IsFiltered` para cada categoría, asignando los deltas en una sola pasada.

---

### ✨ Mejoras introducidas (Modif)

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

---

### 🧪 Notas de desarrollo

- Arquitectura híbrida (`CumulativeTrade` + `MarketDataArg`).
- Reconstrucción histórica determinista y reproducible.
- Separación clara entre lógica de sesión, cálculo y visualización.
- Compatible con replay y carga tardía de datos.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Ninguna.**  
  Las limitaciones históricas del indicador (reinicio de sesión fijo, falta de sesiones previas y acumulación dependiente de la carga inicial) han sido resueltas en la versión custom actual.

---

### 🛠️ Propuestas de mejora

* **P3 (Opcional):** Añadir presets automáticos por instrumento (ES / NQ / RTY) para ajustar rangos de volumen y visualización sin modificar defaults manualmente.

---

### 💎 Valor Reutilizable (Código Donante)

- Segmentación por buckets de volumen.
- Lógica genérica de sesión (Default / Custom / Back).
- Patrón de Spread sintético institucional.

---

### ✍️ La opinión de ChatGPT

Esta versión convierte **MultiMarketPower** en un **indicador de nivel profesional real**.  
No solo muestra delta: **explica quién está detrás del movimiento y cuándo cambia el régimen de participación**.

Es un **Core P1 absoluto** para cualquier sistema serio de scalping con Order Flow.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí. Esencial.**

Permite filtrar ruido, detectar absorciones y operar únicamente cuando el Smart Money confirma.

**Acción:** **Conservar (Core)**  
