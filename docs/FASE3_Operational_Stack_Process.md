# FASE 3 — Construcción del Stack Operativo Final (ATAS · Scalping S&P 500)

Este documento define el **proceso operativo y normativo** de la **FASE 3** del sistema
de selección de indicadores para ATAS.

En FASE 3 **NO se reevalúan indicadores individualmente**: se construye el
**STACK OPERATIVO FINAL** a partir de indicadores ya clasificados en FASE 2.

FASE 3 existe para:
- eliminar redundancias,
- maximizar edge,
- minimizar coste cognitivo,
- y asegurar fiabilidad operativa en **M1** (scalping).

---

## 1. Contexto y prerequisitos

### 1.1 Prerequisito obligatorio
FASE 3 solo puede ejecutarse si:

- Se ha completado la **FASE 2 — Torneo por Grupos**.
- Cada indicador candidato dispone de ficha técnica individual conforme a:
  - `/docs/Gold_Standard_Indicator_Sheet_v7.md`
- Cada ficha incluye clasificación **CORE / RESERVA** (los demás quedan fuera del pool FASE 3).

---

## 2. Objetivo de la FASE 3

Seleccionar un conjunto **mínimo, ortogonal y operativo** de indicadores que cubra todas
las capas funcionales del sistema, dejando solo los estrictamente necesarios para:

- decidir si operar,
- dónde operar,
- cuándo entrar,
- cuándo NO entrar,
- cómo gestionar el riesgo.

---

## 3. Alcance (reglas no negociables)

- SOLO se evalúan indicadores previamente clasificados como **CORE o RESERVA** en FASE 2.
- NO se reescriben ni modifican fichas individuales en FASE 3.
- FASE 3 genera **DOCUMENTOS DE SISTEMA** independientes (no fichas de indicador).
- La capa **Post-trade / Auditoría** queda explícitamente **excluida** del torneo de FASE 3.

---

## 4. Capas funcionales del sistema

### 4.1 Capas operativas (tiempo real)
- **Contexto / Régimen**
- **Niveles / Mapa**
- **Timing / Trigger**
- **Confirmación / Calidad**
- **Gestión / Riesgo**

### 4.2 Capas transversales (filtros no visibles)
- **Fiabilidad / Disponibilidad de datos**
- **Coste cognitivo / Ergonomía**

---

## 5. Regla de asignación de capas (núcleo de la fase)

- Un indicador puede contribuir a varias capas.
- Un indicador SOLO puede ser **CORE definitivo** en **UNA** capa.
- En el resto de capas debe documentarse como contribución **secundaria** (no competitiva).
- Si un indicador ya es CORE definitivo en una capa, **NO puede competir** como CORE en otra.

Esta regla existe para forzar **ortogonalidad** y evitar stacks redundantes.

---

## 6. Estructura del torneo FASE 3

### 6.1 Subtorneos por capa
FASE 3 se divide en **SUBTORNEOS** por capa funcional.

- Cada subtorneo evalúa packs de **máximo 5–10** indicadores.
- Cada subtorneo produce:
  - **1 CORE definitivo**
  - **0–1 RESERVA**, solo si aporta valor ortogonal claro
- El resto de indicadores queda **fuera del stack operativo final**, aunque sigan siendo
  CORE/RESERVA en FASE 2.

### 6.2 Orden recomendado
Se recomienda ejecutar los subtorneos en este orden para minimizar re-trabajo:

1. Contexto / Régimen
2. Niveles / Mapa
3. Timing / Trigger
4. Confirmación / Calidad
5. Gestión / Riesgo

---

## 7. Criterios de decisión (NO numéricos)

En FASE 3 está prohibido usar puntuaciones 0–10.

Las decisiones se toman respondiendo explícitamente:

- ¿Qué información única aporta este indicador en esta capa?
- ¿Qué otros indicadores cubren lo mismo?
- ¿Cuál tiene menor coste cognitivo en M1?
- ¿Cuál es más estable y fiable técnicamente?
- ¿Qué ocurre cuando falla (modo degradado)?  

Toda exclusión debe estar justificada.

---

## 8. Matriz de decisión (obligatoria)

Toda decisión debe reflejarse en una matriz **Capas × Indicadores** con:

- contribución: **Fuerte / Media / Secundaria / Nula**
- fiabilidad: **Alta / Media / Baja**
- coste cognitivo: **Bajo / Medio / Alto**
- capa primaria asignada (si aplica)

La matriz es parte del documento de selección de capa (output obligatorio).

---

## 9. Outputs obligatorios

### 9.1 Output por subtorneo (por capa)
Por cada subtorneo se genera un documento Markdown independiente:

- `Layer_<nombre>_selection.md`

Cada documento debe incluir, como mínimo:

- objetivo de la capa,
- indicadores evaluados (solo CORE/RESERVA de FASE 2),
- matriz resumida,
- CORE seleccionado,
- reservas (si existen),
- indicadores descartados y motivo (con justificación explícita).

### 9.2 Output final del stack
Al finalizar todos los subtorneos se genera:

- `OrderFlow_Operational_Stack.md`

Este documento debe describir:

- el stack definitivo,
- el rol claro de cada indicador,
- dependencias operativas (qué indicadores son prerequisites),
- modos degradados: qué pasa si un indicador falla o no tiene datos.

---

## 10. Estilo de respuesta (normativo)

- Preciso, estructurado y técnico.
- Orientado a ejecución real de scalping en M1.
- Sin abstracciones innecesarias.
- Cada exclusión debe estar justificada.
- Prioriza claridad, edge y reducción de errores operativos.

---

## 11. Instrucción inicial por defecto

Al iniciar un trabajo de FASE 3, comenzar con UNA sola pregunta:

- ¿Qué CAPA FUNCIONAL evaluamos primero y qué indicadores (CORE/RESERVA de FASE 2) entran en ese subtorneo?

Si el usuario ya lo deja claro, no preguntar.
