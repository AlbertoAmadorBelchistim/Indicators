---

# 1. IDENTIFICACIÓN  
cs_file: Highest.cs  
name: Highest  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Extremes & Range Structure  
comparison_group: "Extremes & Range Structure"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 6/10  
score_potential: 7/10  
file_state: Estable  
effort: N/A  
action_priority: Nula  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Cuál es el valor máximo de la serie de entrada (Source) en las últimas N barras?  
gemini_summary: "Rolling max genérico sobre SourceDataSeries: menos útil como canal, pero valioso como bloque para extremos sobre cualquier fuente (precio, delta, etc.)."  
competitor_notes: "Pierde como herramienta principal de rango frente a Donchian/HighLow. Gana valor cuando se usa sobre fuentes no-High/Low (porque Donchian/HighLow están orientados a extremos de precio)."  
reusable_code: "Implementación mínima y segura de rolling max con control de ventana (start/count) para evitar out-of-range."  

# 6. METADATOS  
analysis_date: 2025-12-28  
official_code_date: 2025-04-23  




---

## 🟦 Highest (6/10)

**Nombre del archivo:** [`Highest.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/Highest.cs)  
**Nombre del indicador:** Highest  
**Web oficial:** [ATAS — Highest](https://help.atas.net/support/solutions/articles/72000602627)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuál es el valor máximo de la serie de entrada (Source) en las últimas N barras?  

![Highest](../../img/Highest.png)



---

### ⚙️ Parámetros configurables

* **Period**: Número de barras usadas para buscar el máximo (por defecto: 10).  



---

### 🧭 Clasificación
**Grupo:** Market Structure  
**Subgrupo:** Extremes & Range Structure  
**Comparison Group:** "Extremes & Range Structure"  



---

### 🧠 Uso más frecuente

* Marcar el máximo reciente de una serie (por defecto el Close, pero puede ser otra fuente seleccionable en la UI).  
* Construir reglas tipo “breakout del máximo de N” sobre fuentes no estándar.  



---

### 📊 Nivel de relevancia
🔟 **6 / 10**

✅ Muy ligero y estable.  
✅ Útil como bloque genérico (rolling max sobre Source).  
⛔ Como herramienta de rango/estructura, queda por detrás de un canal completo (Donchian/HighLow).  



---

### 🎯 Estrategias de scalping donde se aplica

* **Breakout del máximo de N** sobre la serie elegida.  
* **Filtro de contexto**: “no buscar largos si no supera el máximo de N”.  



---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor recomendado | Justificación |
|---|---:|---|
| Period | 10–20 | 10 para micro-rupturas; 20 para contexto de rango más estable. |  



---

### 🧪 Notas de desarrollo

* Ventana definida con `start` y `count` para evitar errores en barras iniciales.  
* Complejidad O(Period) por barra; adecuada para Period típico.  
* Opera sobre `SourceDataSeries`, lo que aumenta su reutilidad frente a canales fijos a High/Low.  



---

### ❗ Incoherencias o aspectos mejorables detectados

* Ninguna técnica evidente; la limitación es de “producto” (solo una banda, no un canal).  



---

### 🛠️ Propuestas de mejora

* Si se quisiera convertirlo en herramienta principal: añadir la banda inferior (Lowest) y opcional midline; en la práctica eso ya es Donchian/HighLow.  



---

### 💎 Valor Reutilizable (Código Donante)

* Patrón de rolling max seguro y mínimo (start/count + loop) reutilizable para otras series.  



---

### ✍️ La opinión de ChatGPT sobre el Indicador

No es un ganador de rango, pero sí un buen “utility indicator”. En un sistema, los extremos sobre fuentes no estándar (delta, volumen, ratios) pueden aportar señales de contexto que un canal de precio no cubre. Mantenerlo como reserva tiene sentido.  



---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (como utilidad / reserva)**  

**Acción:** **Conservar (Reserva)**  
