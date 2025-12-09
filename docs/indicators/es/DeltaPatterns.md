---
# 1. IDENTIFICACIÓN
cs_file: DeltaPatterns.cs
name: Delta Patterns
version: Custom v2.0 (Optimized Logic)

# 2. CLASIFICACIÓN
group: Order Flow
subgroup: Delta
comparison_group: "Bar Delta Analysis"

# 3. VALORACIÓN (Score & Priority)
score_current: 9/10
score_potential: 9.5/10
file_state: Estable
effort: Medio
action_priority: Baja
system_priority: P2

# 4. DECISIÓN
recommended_action: Conservar (Reserva)

# 5. ANÁLISIS
description: ¿Qué patrones de micro-estructura ocurren dentro de una ventana de volumen constante?
gemini_summary: "Escáner de micro-estructura basado en volumen. Usa una 'Ventana Rodante' para detectar patrones puros (Dominancia, Iniciativa, Divergencia) independientemente del tiempo."
competitor_notes: "Complemento táctico de DeltaModif. Ideal para usar en gráficos de Volumen donde TargetVolume coincide con el tamaño de la vela."
reusable_code: "Algoritmo Rolling Window (Queue)."

# 6. METADATOS
analysis_date: 2025-12-09
user_modification_date: 2025-12-09
---

## 🎯 Delta Patterns (9/10)

**Nombre del archivo:** [`DeltaPatterns.cs`](Link_Repo)  
**Nombre del indicador:** Delta Patterns  
**Versión:** Custom v2.0 (Logic Fixes applied)
**Compatibilidad:** ATAS Beta y superiores. Para compatibilidad con versiones anteriores, debe usarse la compilación "stable" de los indicadores.  
**Agradecimientos:** A **Joan** de Scalping Agresivo por la idea y **Carlos HK** por sus sugerencias e ideas.  


> **La Pregunta Clave:** ¿Qué patrones de micro-estructura ocurren dentro de una ventana de volumen constante?

### ⚙️ Parámetros configurables

#### 🧠 Motor
* **Target Volume:** Tamaño de la ventana de análisis (Recomendado: Igualar al timeframe del chart si es de volumen).

#### 🔎 Patrones (Orden de Prioridad)
1.  **Divergence:** Precio y Delta van en direcciones opuestas.
2.  **Reversal:** Delta tocó un extremo fuerte pero cerró invertido (Trampa).
3.  **Dominance:** Control total. Delta fuerte y **sin mecha en contra** (0% tolerancia). La señal más potente de continuación.
4.  **Aggressive:** Iniciativa estándar. Delta neto supera el % configurado.
5.  **Neutral:** Mucho volumen, delta neto cercano a 0 (Lucha/Empate).

#### 🎨 Visualización
* **Color Mode:** `Semantic` (Recomendado) agrupa las señales en Alcistas (Verdes/Azules) y Bajistas (Rojos/Naranjas).

---

### ⚙️ Parametrización óptima para scalping (S&P 500)

**Estrategia: Sincronía de Volumen**

| Parámetro | Valor | Justificación |
| :--- | :--- | :--- |
| **Chart Timeframe** | `Volumen 2500` | Normaliza la velocidad del mercado. |
| **Target Volume** | `2500` | Sincroniza el indicador con la vela visual. |
| **Dominance %** | `12%` | Señal de ruptura limpia. |
| **Aggressive %** | `15%` | Señal de continuación. |
| **Wick Tolerance** | `0.1%` | Exigir control absoluto para la Dominancia. |

---

### 🧪 Notas de desarrollo v2.0
* **Fix Lógico:** Se ha invertido la prioridad. Ahora `Dominance` se evalúa antes que `Aggressive` para no enmascarar la señal más fuerte.
* **Simplificación:** Eliminado `AggressiveClosePercent` por redundancia matemática en ventanas de volumen fijo.
* **Definiciones:** Corregida la definición de Dominancia a "Control Unilateral" (Iniciativa pura).

### 📈 Veredicto: ¿Es útil para Scalping?
**Sí.**
En gráficos de volumen es una herramienta letal para confirmar rupturas (Dominance) o detectar falsos rompimientos (Divergence/Reversal).