# Gold Standard — Ficha Técnica de Indicadores  
## Versión 7.0 (FASE 2)

Este documento define la **especificación obligatoria y exacta** para la generación de
**fichas técnicas individuales de indicadores** (`.md`) durante la  
**FASE 2 — Selección por Torneo** del sistema de trading para ATAS.

Este documento es **normativo**.  
No describe cómo decidir, sino **cómo debe quedar escrita una ficha válida**.

Cualquier desviación del estándar debe ser **explícita y justificada**.

---

## 1. Alcance

Este estándar se aplica **exclusivamente** a:

- Fichas técnicas individuales de indicadores.
- Documentación generada en **FASE 2**.
- Indicadores clasificados como **CORE, RESERVA, DONANTE o DESCARTE**.

No se aplica a:
- Documentación de sistema (FASE 3).
- Auditorías técnicas.
- Arquitectura de código.

---

## 2. Lógica condicional de generación

La estructura y el nivel de detalle de la ficha dependen de la **clasificación final
del indicador**.

### 2.1 Indicadores CORE / MODIF

Usar **MODO EXPANDIDO**.

Obligatorio:
- Incluir `user_modification_date` en YAML.
- Detallar **TODOS** los grupos de parámetros.
- Incluir **tabla de parametrización óptima** en Markdown.
- Añadir las secciones:
  - ✨ Mejoras introducidas (Oficial/Base)
  - ✨ Mejoras añadidas (Custom)

---

### 2.2 Indicadores RESERVA / DESCARTE

Usar **MODO ESTÁNDAR**.

Características:
- Formato conciso.
- Lista simple de parámetros.
- Sin tabla de parametrización óptima.
- Sin secciones de mejoras.

---

### 2.3 Indicadores DONANTE

DONANTE **no es una acción final**, sino un **estado de reutilización**.

Reglas:
- Se documentan en **MODO ESTÁNDAR**.
- `recommended_action` debe ser **Fusionar**.
- `system_priority` debe ser **NA**.
- La sección **💎 Valor reutilizable (Código Donante)** es **obligatoria**.

---

## 3. Reglas de oro — YAML y formato

### 3.1 Saltos de línea y separadores

- En bloques técnicos y listas:
  - añadir **SIEMPRE** dos espacios al final de cada línea antes del salto.
- Antes de cada separador `---`:
  - insertar **dos saltos de línea** (`\n\n`).

El incumplimiento invalida la ficha.

---

### 3.2 Reglas YAML (estrictas)

- **`category` está prohibido**.  
  Usar solo `group` y `subgroup`.

- **Fechas**:
  - Formato obligatorio: `YYYY-MM-DD`.
  - Si no existe fecha oficial: `Unknown`.

- **`user_modification_date`**:
  - SOLO si el indicador es custom o modificado.
  - Si es oficial sin tocar: eliminar la línea completa.
  - Prohibido usar `N/A`.

- **`comparison_group`**:
  - Debe ser **idéntico (copia exacta)** para todos los competidores del mismo torneo.

- **`description`**:
  - Debe formular la **Pregunta Clave** que responde el indicador.

---

## 4. Prioridades

### 4.1 `action_priority` — Urgencia técnica

- Alta — Roto / peligroso.
- Media — Sucio / frágil.
- Baja — Pulido / cosmético.
- Nula — Estable / cerrado.

---

### 4.2 `system_priority` — Valor estratégico

- P1 — Core indispensable.
- P2 — Soporte útil.
- P3 — Reserva / backup.
- NA — Fuera del sistema operativo (Donante / Descarte).

---

## 5. Puntuaciones

### 5.1 `score_current`

Puntuación del indicador **tal como está**, combinando:
- calidad informática,
- utilidad real para scalping,
- claridad y valor visual.

---

### 5.2 `score_potential`

Puntuación **máxima razonable** que podría alcanzar:
- aplicando mejoras propuestas,
- considerando las tres dimensiones anteriores.

Reglas:
- `score_current` ≠ `score_potential` **si existe margen real de mejora**.
- Pueden ser iguales **solo** si:
  - el indicador no puede mejorar,
  - o el esfuerzo no merece la pena.

---

## 6. Plantilla obligatoria de ficha

El orden y la estructura son **no negociables**.

---

### 6.1 Cabecera YAML

    ---
    # 1. IDENTIFICACIÓN
    cs_file: [NombreExacto.cs]
    name: [DisplayName del código]
    version: [ATAS Stable/Latest | Custom vX.X]

    # 2. CLASIFICACIÓN
    group: [Grupo funcional]
    subgroup: [Subgrupo taxonómico]
    comparison_group: "[Etiqueta del Torneo]"

    # 3. VALORACIÓN
    score_current: [0-10]/10
    score_potential: [0-10]/10
    file_state: [Estable | Defectuoso]
    effort: [Bajo | Medio | Alto | NA]
    action_priority: [Alta | Media | Baja | Nula]
    system_priority: [P1 | P2 | P3 | NA]

    # 4. DECISIÓN
    recommended_action: [Conservar (Core) | Conservar (Reserva) | Fusionar | Descartar]

    # 5. ANÁLISIS
    description: [Pregunta Clave]
    gemini_summary: "Resumen ejecutivo de por qué gana o pierde."
    competitor_notes: "Comparativa directa con sus rivales en el torneo."
    reusable_code: "Clases o métodos valiosos para reciclar (o null)."

    # 6. METADATOS
    analysis_date: YYYY-MM-DD
    official_code_date: [YYYY-MM-DD | Unknown]
    user_modification_date: YYYY-MM-DD  # SOLO SI ES CUSTOM
    ---

---

### 6.2 Encabezado del cuerpo

    ## [ICONO] [Nombre] ([score_current]/10)

    **Nombre del archivo:** [`[cs_file]`](Link_Repo)  
    **Nombre del indicador:** [DisplayName]  
    **Web oficial:** [Referencia ATAS]  
    **Compatibilidad:** ATAS [Versión].  
    **Última revisión del código oficial:** [official_code_date]  
    *(Solo si Custom)* **Última revisión del código modificado:** [user_modification_date]  

    > **La Pregunta Clave:** [Description]

    ![NombreIndicador](../../img/[NombreIndicador].png)


    ---

---

## 7. Secciones obligatorias (orden fijo)

1. ⚙️ Parámetros configurables  
2. 🧭 Clasificación  
3. 🧠 Uso más frecuente  
4. 📊 Nivel de relevancia  
5. 🎯 Estrategias de scalping donde se aplica  
6. ⚙️ Parametrización óptima para scalping (1M, S&P 500) *(solo CORE / MODIF)*  
7. ✨ Mejoras introducidas (Oficial/Base) *(solo CORE / MODIF)*  
8. ✨ Mejoras añadidas (Custom) *(solo CORE / MODIF)*  
9. 🧪 Notas de desarrollo  
10. ❗ Incoherencias o aspectos mejorables detectados  
11. 🛠️ Propuestas de mejora  
12. 💎 Valor reutilizable (Código Donante)  
13. ✍️ La opinión de ChatGPT sobre el Indicador  
14. 📈 Veredicto: ¿Es útil para Scalping?

---

## 8. Criterio de conformidad

Una ficha es válida **solo si**:

- cumple esta estructura exactamente,
- respeta las reglas de formato,
- aplica correctamente la lógica condicional.

Las fichas no conformes deben corregirse.
