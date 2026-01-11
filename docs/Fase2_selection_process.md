# FASE 2 — Proceso de Selección de Indicadores por Torneo

Este documento define el **proceso operativo, los criterios cognitivos y la metodología**
utilizados en la **FASE 2** del sistema de selección de indicadores para ATAS.

Es un documento **normativo**.  
Su objetivo es garantizar que las decisiones de selección sean **coherentes, reproducibles
y defendibles**, independientemente del lote de indicadores analizado.

Este documento **NO define el formato de las fichas**.  
La especificación exacta de escritura se encuentra en:

- `/docs/Gold_Standard_Indicator_Sheet_v7.md`

Ambos documentos son **complementarios y vinculantes**.

---

## 1. Contexto del sistema

Estamos desarrollando un **Sistema de Trading Algorítmico y Discrecional** orientado a
**scalping en el S&P 500**, utilizando la plataforma **ATAS**.

Los indicadores evaluados en esta fase:

- se utilizan en **tiempo real**,
- influyen directamente en decisiones de:
  - entrada,
  - filtrado,
  - descarte,
- y tienen impacto directo en:
  - **coste cognitivo**,
  - **fiabilidad operativa**,
  - **claridad visual en M1**.

La FASE 2 trabaja sobre una base amplia de indicadores (centenares), por lo que la
**estandarización del proceso de decisión es crítica** para evitar decisiones arbitrarias
o inconsistentes.

---

## 2. Rol cognitivo del analista (asistente)

En esta fase, el asistente actúa como **analista técnico senior especializado en C# y
trading algorítmico para ATAS**.

Sus evaluaciones priorizan explícitamente las siguientes dimensiones:

### 2.1 Eficiencia informática
- Coste de cálculo.
- Gestión de estado.
- Impacto en rendimiento intradía.
- Riesgo de degradación en sesiones largas o datos densos.

### 2.2 Calidad de los datos mostrados
- Coherencia entre definición conceptual y cálculo real.
- Estabilidad del dato en tiempo real.
- Ausencia de artefactos, repintados engañosos o señales ambiguas.

### 2.3 Visualización
- Claridad y legibilidad en M1.
- Jerarquía visual correcta.
- Ausencia de ruido innecesario o sobrecarga gráfica.

### 2.4 Capacidad para generar decisiones de calidad
- Aporta información **accionable**.
- Reduce ambigüedad operativa.
- Mejora el timing, el filtrado o la gestión del riesgo.

El análisis debe ser **crítico, riguroso y honesto**, incluso si el indicador es:
- popular,
- oficial,
- ampliamente utilizado,
- o “funciona” superficialmente.

---

## 3. Objetivo de la FASE 2

El objetivo de esta fase **NO es mejorar indicadores**, sino:

- comparar indicadores que cumplen **funciones similares**,
- detectar **redundancias funcionales**,
- identificar el indicador **más sólido** para cada función,
- y clasificar el resto según su **valor real dentro del sistema**.

El resultado final de la FASE 2 es:

- un conjunto de **fichas técnicas normalizadas**,
- y una **clasificación explícita y justificada** de cada indicador.

La FASE 2 **no construye el sistema final**, solo prepara el terreno para ello.

---

## 4. Metodología: Torneo por Grupos

La FASE 2 se ejecuta mediante un **Torneo por Grupos**.

### 4.1 Agrupación funcional

Los indicadores se agrupan por su **función principal**, por ejemplo:

- Delta,
- Footprint,
- DOM,
- Order Flow,
- Niveles,
- Contexto / Régimen.

Solo se comparan indicadores que **compiten por resolver el mismo problema operativo**.

No se comparan indicadores de funciones distintas, aunque compartan señales parciales.

---

### 4.2 Análisis técnico previo

Para cada indicador del grupo:

- Se analiza el código fuente (`.cs`):
  - arquitectura general,
  - cálculo,
  - gestión de estado,
  - renderizado.
- Se evalúa su comportamiento esperado en **trading real**.
- Se detectan:
  - redundancias funcionales,
  - debilidades conceptuales,
  - deuda técnica relevante,
  - riesgos operativos.

Este análisis es **previo y obligatorio** antes de cualquier decisión de clasificación.

---

### 4.3 Decisión y clasificación

Tras el análisis comparativo, cada indicador se clasifica en una de las siguientes
categorías:

#### CORE
- Indicador ganador del grupo.
- Aporta información **única, fiable y accionable**.
- Tiene encaje claro en el sistema final.

#### RESERVA
- Indicador sólido, pero:
  - redundante frente al CORE,
  - con peor coste cognitivo,
  - o menor estabilidad en tiempo real.
- Puede usarse como backup o alternativa puntual.

#### DONANTE (Fusionar)
- No es adecuado para uso directo.
- Contiene **ideas, cálculos o fragmentos reutilizables**.
- Su valor es **técnico**, no operativo.
- Se refleja como `recommended_action: Fusionar` en la ficha.

#### DESCARTE
- No aporta valor suficiente.
- Es redundante, confuso o ineficiente.
- No se recomienda su uso ni reutilización.

La clasificación debe quedar **explícitamente justificada** en la ficha técnica.

---

## 5. Relación con la documentación

Cada indicador evaluado en FASE 2 debe disponer de una **ficha técnica individual**
en formato Markdown (`.md`), generada conforme al estándar vigente.

- El **formato, estructura y reglas de escritura** están definidos en:
  - `/docs/Gold_Standard_Indicator_Sheet_v7.md`
- Este documento define **cómo pensar y decidir**,
  no **cómo escribir la ficha**.

Ambos documentos son **complementarios y obligatorios**.

---

## 6. Límites de la FASE 2

En esta fase **NO se debe**:

- modificar o refactorizar código,
- implementar mejoras,
- optimizar rendimiento,
- diseñar el stack operativo final.

La FASE 2 termina cuando:

- los ganadores están claros,
- las fichas están completas y conformes al estándar,
- y la clasificación es defendible.

La construcción del sistema pertenece a la **FASE 3**.

---

## 7. Criterio de validez

Una decisión de FASE 2 es válida **solo si**:

- el proceso ha sido seguido completo,
- el análisis técnico es coherente,
- la clasificación es justificable,
- y la ficha resultante permite entender **por qué gana o pierde** cada indicador.

Este documento existe para **evitar decisiones arbitrarias**
y asegurar **coherencia a largo plazo**.

