---
# 1. IDENTIFICACIÓN  
cs_file: AccountInfoDisplayModif.cs  
name: Account Info Display Modif  
version: Custom v5.4.2  

# 2. CLASIFICACIÓN  
group: Risk Management  
subgroup: Account / Funded Accounts  
comparison_group: "Account Risk Management Overlays (Provisional)"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 9/10  
score_potential: 10/10  
file_state: Estable  
effort: Alto  
action_priority: Baja  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: "¿Estoy en riesgo de violar reglas económicas (daily loss / trailing DD / disciplina) y dónde debería colocar TP/SL en precio para operar sin codicia ni miedo?"  
gemini_summary: "Indicador core para cuentas fondeadas: agrega estado de cuenta, trailing drawdown y daily rails per-account con persistencia robusta. Añade recomendaciones soft (OK/CAUTION/STOP) y price rails en el gráfico para mapear el TP/SL hacia el profit cap y el stop efectivo más cercano. Display-only y extensible."  
competitor_notes: "En ausencia de arena definida, destaca por integrar en una sola HUD: (1) reglas económicas de prop firms, (2) reseteos por horario de trading, (3) control conductual (trades, rachas, consistency, giveback) y (4) rails en precio. Un 'Account dashboard' estándar no cubre funded constraints ni cálculo de stop efectivo."  
reusable_code: "Persistencia JSON per-account (config + runtime), motor de reseteos por sesión/horario, cálculo $→precio con tick metadata, evaluación de estados CAUTION/STOP extensible."  

# 6. METADATOS  
analysis_date: 2025-12-19  
official_code_date: 2025-11-17  
user_modification_date: 2025-12-19  
---

## 🟦 Account Info Display Modif (9/10)

**Nombre del archivo:** [`AccountInfoDisplayModif.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/AccountInfoDisplayModif.cs)  
**Nombre del indicador:** Account Info Display Modif  
**Web oficial (base):** [ATAS — Account Info Display](https://help.atas.net/en/support/solutions/articles/72000648751-account-info-display)  
**Compatibilidad:** ATAS Stable / Latest (custom fork)  
**Última revisión del código oficial:** 2025-11-17   
**Última revisión del código modificado:** 2025-12-19  

> **La Pregunta Clave:** ¿Estoy en riesgo de violar reglas económicas (daily loss / trailing DD / disciplina) y dónde debería colocar TP/SL en precio para operar sin codicia ni miedo?

![Account Info Display Modif](../../img/AccountInfoDisplay.png)

> Ejemplo real de uso: el indicador traduce límites monetarios (daily loss y profit cap)
> en niveles de precio accionables, mostrando simultáneamente el estado psicológico
> del trader (STOP sugerido por racha de pérdidas).

---

### ⚙️ Parámetros configurables

A continuación se describen **todos los grupos funcionales** y cómo configurarlos en un entorno típico de **scalping 1M S&P 500 (ES)** con cuentas fondeadas tipo Bulenox.  

#### 1) Account / Scope (per-account)  
**Objetivo:** todo lo que afecta a reglas y estado se guarda **por cuenta**. Cambiar de cuenta no debe “arrastrar” la configuración de otra.  

- **Account selector / Account ID**: el indicador identifica la cuenta activa y aplica su estado persistido.  
- **Reinitialize now**: fuerza a recalcular y sincronizar estado (útil tras cambiar parámetros críticos o importar JSON).  
- **Reset mensual condicionado**: el reset mensual se ejecuta **solo si la cuenta está perdida** (equity bajo stop) y en la fecha configurada; evita “resetear” cuentas que van bien y respeta reglas realistas de prop firms.  

**Cómo usarlo bien:**  
- Configura cada cuenta una vez, valida que el JSON se guarda, y no vuelvas a tocarlo salvo cambios de reglas de la prop firm.  
- Si una cuenta tiene reglas distintas (fecha de reset, stop inicial, payout objective), configúralo específicamente en esa cuenta.   

**Errores típicos de usuario:**  
- Copiar la configuración de una cuenta a otra con reglas distintas (produce falsas sensaciones de seguridad).  
- Cambiar el reset mensual sin entender la condición “solo si la cuenta está perdida”.  

  
---  

#### 2) Trailing Drawdown (Funded-style)  
**Objetivo:** modelar la regla clásica de funded accounts: el **stop equity** sigue al **máximo (peak)** en tiempo real o al final del día.  

**Conceptos clave que expone el panel:**  
- **StartEquity**: equity de referencia inicial de la cuenta para el trailing.  
- **PeakEquity**: máximo equity alcanzado (según modo Realtime o End-of-Day).  
- **StopEquity**: `PeakEquity - MaxTrailingDrawdown`.  
- **Remaining DD**: cuánto margen queda hasta el stop.  
- **Current DD**: drawdown actual desde el peak.  

**Parámetros típicos (interpretación):**  
- **Enable Trailing Drawdown**: activa el módulo funded.  
- **MaxTrailingDrawdown**: distancia máxima permitida desde el peak.  
- **Peak update mode (Realtime / End-of-Day)**:  
  - **Realtime**: el peak sube con el equity intradía. Más “estricto” y realista en evaluaciones.  
  - **End-of-Day**: el peak se consolida al cierre del día. Útil si tu firma lo modela así.  
- **StartEquity / Manual initial stop** (si aplica en tu versión): define el punto inicial si tu cuenta requiere stop manual (p. ej., 49,551.85).  

**Recomendación práctica:**  
- Si operas cuentas funded tipo trailing intradía, usa **Realtime**.  
- Ajusta `MaxTrailingDrawdown` exactamente al contrato de la cuenta (p. ej., 2,500$).  
- Verifica que `StopEquity` se mueve cuando haces nuevos máximos (si Realtime).  
- No confundas `Balance` con `Equity`: el trailing está basado en equity (incluye PnL no realizado).  

**Trampas frecuentes (funded):**  
- “Hice +500$ y vuelvo a BE, sigo seguro”: falso si el trailing ha subido el stop.  
- Pensar que el stop es fijo: en Realtime, sube con el peak.  

  
---  

#### 3) Daily Rails (pérdida diaria y profit cap)  
**Objetivo:** imponer disciplina intradía independiente del trailing global.  

**A) Daily Loss Limit (trailing opcional)**  
- **Enable Daily Loss Limit**: activa el límite de pérdida diaria.  
- **DailyLossLimit**: pérdida máxima permitida en el día (en $).  
- **Loss mode (From start / From peak)**:  
  - **From session start**: el límite se mide desde el equity al inicio del día.  
  - **From session peak**: el límite tralea desde el máximo del día (más estricto).  
- **RemainingDailyLoss** (panel): lo que queda antes de brecha.  

**Cómo configurarlo bien:**  
- Si tu firma aplica “daily loss fijo” → usa **From session start**.  
- Si quieres disciplina anti-tilt intradía → **From session peak** (más duro).  
- Define un daily loss suficientemente bajo como para cortar espiral, pero no tan bajo que sea ruido (depende del nº de contratos).  

**B) Daily Profit Cap (no trailing)**  
- **Enable Daily Profit Cap**: activa el “tope” de beneficio diario.  
- **DailyProfitCap**: objetivo máximo diario (en $) tras el cual **deberías parar**.  
- Importante: **no es trailing**; es un target fijo del día.  

**Lo que aporta el indicador (clave):**  
- Calcula la **línea de target en precio** teniendo en cuenta el **PnL cerrado del día**. Si ya cerraste +300$ y el cap es +1000$, el target del segundo trade queda más cerca.  
- Esto reduce la tentación de “seguir apilando” cuando ya estás cerca del cap, y te da un punto objetivo para cerrar.  

**Errores típicos:**  
- Poner el cap demasiado alto y convertirlo en irrelevante (no te disciplina).  
- Poner el cap demasiado bajo y forzarte a cerrar siempre en micro-moves (si tu edge necesita runners).  

  
---  

#### 4) Reset por día de trading (NY 17:00 o custom local)  
**Objetivo:** que “hoy” sea “día de trading” y no “día calendario”.  

- **Trading day reset: NY 17:00**: estándar recomendado para futuros (CME).  
- **Custom local time**: alternativa cuando quieres alinear con tu sesión o hábitos.  

**Qué resetea (a alto nivel):**  
- métricas intradía (TradesToday, rachas, baseline de ClosedPnL del día).  
- daily rails (start/peak diarios).  
- sugerencias soft asociadas al día (si procede).  

**Recomendación práctica (ES / scalping):**  
- Mantén **NY 17:00** salvo que tengas una razón fuerte. Es el estándar de sesión.  
- Si operas solo RTH, sigue usando NY 17:00 igualmente para consistencia con el producto.  

  
---  

#### 5) Métricas de sesión (Trades/Wins/Losses/Streak)  
**Objetivo:** reforzar disciplina conductual básica (sin entrar en order flow).  

- **TradesToday**: nº de operaciones cerradas del día.  
- **WinsToday / LossesToday**: conteo de trades ganadores/perdedores.  
- **Win/Loss streak**: racha actual (muy útil para frenar tilt).  
- **Realized PnL Today**: calculado con baseline de `ClosedPnL`.  

**Cómo interpretarlas:**  
- Si estás cerca de tu límite de trades o en racha negativa, el coste de oportunidad de seguir operando suele ser negativo (fatiga/tilt).  
- Úsalas como “señal de comportamiento”, no como señal de entrada.  

  
---  

#### 6) Soft Recommendations (OK / CAUTION / STOP)  
**Objetivo:** **no bloquear**, solo sugerir. Unifica razones para actuar como “semáforo”.  

- **OK**: condiciones normales.  
- **CAUTION**: estás acercándote a un límite o patrón de riesgo.  
- **STOP**: condición de alto riesgo o breach; recomendación de parar.  

**Principio de UI:**  
- Se muestra **una sola fila de estado/razón** (STOP tiene prioridad sobre CAUTION).  
- El objetivo es que la UI no cree ambigüedad: o estás bien, o estás en precaución, o debes parar.  

**Triggers típicos (ejemplos):**  
STOP:  
- **Max trades per day exceeded**: excediste tu máximo de trades del día.  
- **Max consecutive losses exceeded**: excediste tu máximo de pérdidas consecutivas.  
- **Daily loss breached**: has roto el daily loss.  

CAUTION:  
- **Payout concentration**: `RealizedPnLToday > 30% * PayoutObjective` (ejemplo).  
- **Near trades limit**: `TradesToday > 75% * MaxTradesPerDay`.  
- **Near loss streak limit**: `LossStreak == MaxConsecutiveLosses - 1`.  
- **Low remaining daily loss**: `RemainingDailyLoss < 20%` del límite.  
- **Giveback after peak today**: riesgo de tilt si devuelves parte del pico intradía.  

**Configuración recomendada (filosofía):**  
- CAUTION debe saltar cuando aún puedes “salvar el día” (bajar tamaño o parar).  
- STOP debe saltar cuando la probabilidad de hacer daño es alta o ya has roto la regla.  
- Si dudas, baja el umbral de CAUTION y deja STOP conservador.  

  
---  

#### 7) Price Rails (5.4 / 5.4.2) — LÍNEAS EN EL PRECIO  
**Objetivo:** convertir reglas económicas en un **mapa visual de TP/SL** directamente sobre el chart para ejecutar con menos emoción.  

**A) Target Rail (Daily Profit Cap)**  
- Se dibuja **solo si hay posición abierta**.  
- Se calcula desde el **AvgEntryPrice** de la posición actual.  
- Tiene en cuenta el **PnL cerrado del día**, por lo que:  
  - si llevas ganancias cerradas, el target queda más cerca;  
  - si llevas pérdidas cerradas, queda más lejos (te obliga a ser realista).  

**Lectura práctica:**  
- La línea target no es “señal de salida obligatoria”, sino un límite psicológico/operativo: si tu objetivo es parar al hacer el cap, esa línea representa “lo que te falta” en precio.  

**B) Stop Rail (Effective stop)**  
- Se dibuja **solo si hay posición abierta**.  
- El stop **NO es único**: el indicador evalúa varios stops “económicos” y escoge el **más restrictivo / más cercano**:  
  - daily loss stop (según modo start/peak)  
  - trailing drawdown stop (funded)  

**Regla importante (tu requisito):**  
- “La línea de stop debe ser la más cercana al precio entre los posibles motivos de stop.”  
  En la práctica se traduce en elegir el stop **más restrictivo** para la dirección de la operación:  
  - Long: el stop más alto (más cerca del precio adverso).  
  - Short: el stop más bajo.  

**C) Parámetros de Price Rails**  
- **Enable Price Rails**: activa las líneas en panel y en chart.  
- **Show Target Rail**: dibuja la línea del cap.  
- **Show Stop Rail**: dibuja la línea de stop efectiva.  
- **Show Rail Labels**: etiqueta con texto (útil en beta).  
- **Rail Line Width**: grosor de las líneas (mantener 1–3).  

**Recomendación práctica:**  
- En beta inicial: labels ON.  
- En uso diario: labels OFF si molestan, dejando líneas.  
- Si tu chart es muy cargado, usa width 1–2.  

  
---  

### 🧭 Clasificación  
**Grupo:** Risk Management  
**Subgrupo:** Account / Funded Accounts  
**Comparison Group:** "Account Risk Management Overlays (Provisional)"  

  
---  

### 🧠 Uso más frecuente  
* Visualizar riesgo de violar reglas funded (trailing DD / daily loss) en tiempo real.  
* Reducir overtrading controlando trades del día y rachas.  
* Ejecutar con menos emoción usando rails en precio para TP/SL hacia profit cap y stop efectivo.  
* Identificar tilt risk con giveback tras pico intradía.  

  
---  

### 📊 Nivel de relevancia  
🔟 **9 / 10**  

✅ Integra funded constraints (trailing + daily) per-account con persistencia robusta.  
✅ Rails en el precio: traduce límites monetarios a niveles accionables de TP/SL.  
✅ Soft recommendations extensibles (OK/CAUTION/STOP) sin bloquear trading.  
⛔ Requiere configuración cuidadosa por cuenta (pero es lo correcto en funded).  

  
---  

### 🎯 Estrategias de scalping donde se aplica  
* **Anti-revenge trading:** CAUTION por racha de pérdidas + stop rail visible.  
* **Cierre disciplinado:** target rail hacia profit cap; reduce “dejar correr por codicia” cuando ya has hecho el día.  
* **Gestión de sesión:** parar tras X trades o tras X pérdidas consecutivas aunque “sientas que recuperas”.  
* **Control de giveback:** si ya hiciste un pico y empiezas a devolver, reduce tamaño o para.  

  
---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

> Ajusta a tu prop firm y tamaño habitual. Los valores abajo son una base realista para evitar tilt.  

| Bloque | Parámetro | Recomendación | Justificación |  
|---|---|---:|---|  
| Trailing DD | Peak mode | Realtime | Replica funded intradía en la mayoría de evaluaciones. |  
| Trailing DD | MaxTrailingDrawdown | Según cuenta (p. ej. 2500) | Debe replicar exactamente la regla contractual. |  
| Daily Rails | Enable Daily Loss | ON | Disciplina diaria independiente del trailing global. |  
| Daily Rails | DailyLossMode | From session start (base) | Más simple y predecible; cambia a peak si necesitas anti-tilt. |  
| Daily Rails | DailyLossLimit | 300–600 (ejemplo) | Debe ser “paro por el día” antes de entrar en espiral. |  
| Daily Rails | Enable Profit Cap | ON | Objetivo operacional diario. |  
| Daily Rails | DailyProfitCap | 500–1200 (ejemplo) | Evitar regalar el día tras hacerlo; ajustar a tu edge. |  
| Reset | Trading day reset | NY 17:00 | Estándar de sesión en futuros (CME). |  
| Soft Reco | MaxTradesPerDay | 6–12 | Controla fatiga/overtrading en 1M. |  
| Soft Reco | MaxConsecutiveLosses | 2–3 | Anti-tilt: tras 3 pérdidas seguidas baja calidad decisional. |  
| Soft Reco | CAUTION: trades fraction | 0.75 | Te avisa antes de “pasarte” de trades. |  
| Soft Reco | CAUTION: remaining loss | 0.20 | Te avisa cuando ya estás “sin margen” para operar bien. |  
| Consistency | PayoutObjective | 1500 | Primera retirada típica 1000–1500 en muchas firms. |  
| Consistency | CAUTION / STOP concentration | 0.30 / 0.39 | Reduce spikes emocionales y devolver el día. |  
| Price Rails | Enable Price Rails | ON | Núcleo “anti emoción”: TP/SL en precio. |  
| Price Rails | Labels | ON (beta) / OFF (uso) | Aprendizaje primero; luego limpieza visual. |  
| Price Rails | Line width | 2 | Visible sin ensuciar. |  

  
---  

### ✨ Mejoras introducidas (Oficial/Base)  
* Panel HUD de información de cuenta y PnL como control rápido.  
* Base visual ligera y extensible.  

  
---  

### ✨ Mejoras añadidas (Custom)  
* Trailing Drawdown per-account (Start/Peak/Stop/Remaining).  
* Daily Rails per-account (Daily Loss + Daily Profit Cap).  
* Reset por día de trading (NY 17:00 / custom).  
* Métricas intradía (trades, wins/losses, rachas, realized PnL today).  
* Soft Recommendations (OK/CAUTION/STOP) con reglas económicas y conductuales.  
* Consistency rules (concentración de beneficio vs payout objective).  
* Giveback after peak today (anti-tilt).  
* Price rails en precio (Target vs Profit Cap y Stop efectivo más restrictivo).  
* Persistencia JSON completa per-account (config + runtime).  

  
---  

### 🧪 Notas de desarrollo  
* Arquitectura per-account: cada cuenta tiene su `state` persistido; evita contaminación cruzada al cambiar de cuenta.  
* Persistencia JSON con compatibilidad hacia atrás: campos nuevos se cargan como nullable sin romper estados previos.  
* Separación clara entre:  
  * **config** (parámetros)  
  * **runtime** (estado dinámico: peaks, baselines, métricas intradía)  
* Cálculo $→precio mediante `TickCost / TickSize`: permite convertir límites monetarios a niveles de precio sin depender de heurísticas.  
* Diseño display-only: no hay bloqueo de trading ni alertas en esta fase.  

  
---  

### ❗ Incoherencias o aspectos mejorables detectados  
* La validez del stop rail depende de que `Balance/Equity` en la API se interpreten como el indicador asume (conviene validarlo en beta con varias firms).  
* Labels pueden solaparse en charts estrechos; mitigable con offsets o toggle (ya existe).  
* Falta aún la capa “activa” (alarmas) para transición a STOP (deliberadamente fuera de alcance).  

  
---  

### 🛠️ Propuestas de mejora  
* Añadir **alarmas sonoras** por transición a STOP (y opcional CAUTION) con cooldown.  
* Añadir opción para ocultar rails si el stop/target cae en dirección “no adversa” (para que el mapa sea más intuitivo).  
* Mejorar ergonomía de labels: offset configurable, y “only one label at a time” si el chart es estrecho.  
* Añadir un “Debug row” opcional para beta (TargetPrice/StopPrice/Reason) para facilitar feedback de usuarios.  

  
---  

### 💎 Valor Reutilizable (Código Donante)  
* Persistencia JSON per-account (config/runtime) reusable para cualquier indicador multi-cuenta.  
* Motor de reseteo por sesión (NY 17:00 / custom).  
* Framework de evaluación de estados (OK/CAUTION/STOP) extensible por reglas.  
* Conversión monetaria a precio basada en metadata del instrumento.  

  
---  

### ✍️ La opinión de ChatGPT sobre el Indicador  
Este indicador ya no es un “account info display”: es un **Risk Cockpit** para funded accounts. La combinación de (1) trailing DD + daily rails, (2) disciplina conductual (trades/rachas/consistencia/giveback) y (3) rails en precio lo convierte en una herramienta de ejecución práctica, no solo informativa. En un sistema de scalping 1M, donde el riesgo real suele ser psicológico y de overtrading, este tipo de HUD aporta una ventaja estructural: te fuerza a “operar el plan” con métricas objetivas. Su mayor fortaleza es la coherencia per-account y su extensibilidad; su mayor riesgo es que los usuarios configuren mal los parámetros. Por eso la documentación (y una imagen clara) es crítica.  

  
---  

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí**  

Te ayuda a mantener consistencia y disciplina en el componente más frágil del scalping: la gestión del riesgo económico y conductual bajo fatiga, especialmente en cuentas funded.  

**Acción:** **Conservar (Core)**  
