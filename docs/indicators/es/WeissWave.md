---
# 1. IDENTIFICACIÓN  
cs_file: WeissWave.cs  
name: Weis Wave  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume  
comparison_group: "Volume Oscillators"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 7.5/10  
score_potential: 9/10  
file_state: Estable  
effort: Bajo  
action_priority: Baja  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Cuánto volumen acumulado (esfuerzo) hay en la onda de precio actual?  
gemini_summary: "Acumulador estructural de volumen por tramos direccionales (ondas). Excelente para contexto Wyckoff/VSA, pero en M1 puede ser sensible: una vela contraria rompe la onda."  
competitor_notes: "No compite como oscilador puro; aporta lectura de estructura (ondas) que complementa al CORE. Pierde como herramienta principal de timing por sensibilidad al color de vela."  
reusable_code: null  

# 6. METADATOS  
analysis_date: 2025-12-15  
official_code_date: 2025-04-23  

  

---  

## 🛡️ Weis Wave (7.5/10)  

**Nombre del archivo:** [`WeissWave.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/WeissWave.cs)  
**Nombre del indicador:** Weis Wave  
**Web oficial:** [ATAS — Weis Wave](https://help.atas.net/support/solutions/articles/72000602507)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Cuánto volumen acumulado (esfuerzo) hay en la onda de precio actual?  

![WeissWave](../../img/WeissWave.png)  

  

---  

### ⚙️ Parámetros configurables  

- **Filter:** umbral numérico para colorear “ondas grandes” con `FilterColor`.   
- **PosColor / NegColor:** colores de onda alcista/bajista.   
- **FilterColor:** color cuando `WaveVolume > Filter`.   

  

---  

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume  
**Comparison Group:** "Volume Oscillators"  

  

---  

### 🧠 Uso más frecuente  

* Contexto Wyckoff/VSA: **esfuerzo vs resultado** por ondas, no por velas sueltas.  
* Detectar **wave failure**: una onda de esfuerzo menor que la previa en un nuevo extremo sugiere agotamiento.  

  

---  

### 📊 Nivel de relevancia  
🔟 **7.5 / 10**  

✅ Aporta lectura estructural de “esfuerzo” acumulado por tramo.   
✅ Muy útil como complemento a delta/ratio cuando buscas divergencias de contexto.  
⛔ Sensible en M1: una sola vela contraria rompe onda (ruido).   

  

---  

### 🎯 Estrategias de scalping donde se aplica  

* **Divergencia estructural:** precio hace nuevo extremo, pero la onda asociada (esfuerzo) es significativamente menor.  
* **Absorción contextual:** ondas de alto esfuerzo con poco avance neto de precio.  

  

---  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
|---|---:|---|  
| Filter | 5000 | Resalta solo clímax/ondas relevantes; reduce ruido visual. |  
| PosColor / NegColor | Default | Ajuste visual, no afecta cálculo. |  
| FilterColor | Default | Ajuste visual, no afecta cálculo. |  

  

---  

### 🧪 Notas de desarrollo  

* La onda se mantiene mientras el signo de `(Open - Close)` coincida con el de la vela previa; si cambia, reinicia y toma el volumen de la vela actual.  
* El coloreado usa `Open < Close` para Up/Down, más un filtro por umbral. 

  

---  

### ❗ Incoherencias o aspectos mejorables detectados  

* En mercados laterales (M1), dojis/cambios pequeños de color fragmentan ondas y degradan la señal. 

  

---  

### 🛠️ Propuestas de mejora  

* **P2 (Media):** definir ondas con lógica tipo ZigZag (cambio mínimo de ticks) o umbral de desplazamiento, para robustez en intradía.  
* **P3 (Baja):** opción de “merge” de dojis (no romper onda si rango o cuerpo < umbral).  

  

---  

### 💎 Valor Reutilizable (Código Donante)  

* Patrón simple de acumulación por tramos y recoloreado por filtro (útil para prototipos rápidos).  

  

---  

### ✍️ La opinión de ChatGPT sobre el Indicador  

Como herramienta de **estructura**, es muy buena; como herramienta de **timing** en M1, es ruidosa tal y como está implementada. En tu sistema la mantendría como reserva de contexto (especialmente si trabajas Wyckoff/VSA), y solo consideraría elevarla si se refactoriza la definición de onda (ZigZag/umbral).  

  

---  

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí, pero como contexto (Reserva).**  

Aporta divergencias estructurales, no una señal de entrada directa.  

**Acción:** **Conservar (Reserva)**  

