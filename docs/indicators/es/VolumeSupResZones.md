---

# 1. IDENTIFICACIÓN  
cs_file: VolumeSupResZones.cs  
name: Volume Support & Resistance Zones  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Market Structure  
subgroup: Support & Resistance  
comparison_group: "Volume-Filtered S/R Zones"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 7/10  
score_potential: 8/10  
file_state: Estable  
effort: N/A  
action_priority: Baja  
system_priority: P3  

# 4. DECISIÓN  
recommended_action: Conservar (Reserva)  

# 5. ANÁLISIS  
description: ¿Qué zonas de soporte y resistencia estructural siguen activas al filtrar swings por volumen y marco temporal?  
gemini_summary: "Motor de zonas estructurales multi-TF que utiliza volumen como filtro de validación. No es Order Flow ni Profile, sino estructura de mercado reforzada por volumen."  
competitor_notes: "Grupo singleton temporal. No compite con perfiles, VWAP ni indicadores de agresión; su función es zonal-estructural."  
reusable_code: "Motor de gestión de zonas persistentes multi-timeframe y lógica de activación/desactivación por ruptura."  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🧱 Volume Support & Resistance Zones (7/10)  

**Nombre del archivo:** [`VolumeSupResZones.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/VolumeSupResZones.cs)  
**Nombre del indicador:** Volume Support & Resistance Zones  
**Web oficial:** [ATAS — Volume-based Support & Resistance Zones](https://help.atas.net/support/solutions/articles/72000619397)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Qué zonas de soporte y resistencia estructural siguen activas al filtrar swings por volumen y marco temporal?  

![VolumeSupResZones](../../img/VolumeSupResZones.png)


---

### ⚙️ Parámetros configurables  

- **TimeFrame:** Marco temporal usado para detectar la estructura (multi-TF).  
- **Strength:** Número mínimo de swings requeridos para validar una zona.  
- **VolumePeriod:** Periodo de la media de volumen usada como filtro.  
- **VolumeMultiplier:** Multiplicador mínimo de volumen para validar el swing.  
- **ZoneWidth:** Anchura de la zona dibujada.  
- **MaxZones:** Número máximo de zonas simultáneas en el gráfico.  
- **SupportColor / ResistanceColor:** Colores de zonas de soporte y resistencia.  
- **ZoneTransparency:** Transparencia de la zona.  
- **ExtendZones:** Extiende las zonas hasta que sean invalidadas.  


---

### 🧭 Clasificación  
**Grupo:** Market Structure  
**Subgrupo:** Support & Resistance  
**Comparison Group:** "Volume-Filtered S/R Zones"  


---

### 🧠 Uso más frecuente  

* Identificar **zonas estructurales relevantes** sin depender de niveles puntuales.  
* Filtrar soportes/resistencias por **participación real** (volumen).  
* Construir mapas de contexto multi-timeframe para scalping y day trading.  


---

### 📊 Nivel de relevancia  
🔟 **7 / 10**  

✅ Enfoque estructural claro (HH/LL + validación por volumen).  
✅ Zonas persistentes, más robustas que líneas simples.  
⛔ No ofrece timing ni lectura de agresión; requiere triggers adicionales.  


---

### 🎯 Estrategias de scalping donde se aplica  

* **Contexto estructural:** usar las zonas como marco para buscar entradas con Order Flow.  
* **Rechazo de zona:** confirmar absorción o fallo de ruptura dentro de una zona relevante.  
* **Gestión de trade:** zonas como TP parciales o invalidaciones.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| :--- | :--- | :--- |  
| **TimeFrame** | `5M`–`15M` | Estructura limpia sin ruido excesivo. |  
| **Strength** | `2`–`3` | Evita zonas débiles o anecdóticas. |  
| **VolumeMultiplier** | `1.5`–`2.0` | Filtra swings sin participación real. |  
| **ZoneWidth** | Moderada | Evita zonas demasiado amplias. |  
| **ExtendZones** | `true` | Mantiene contexto hasta invalidación. |  


---

### 🧪 Notas de desarrollo  

* La lógica se basa en **estructura de swings**, no en microestructura.  
* El volumen actúa como **filtro de calidad**, no como métrica primaria.  
* Las zonas se invalidan por ruptura clara, no por simple toque.  


---

### ❗ Incoherencias o aspectos mejorables detectados  

* No distingue entre **tipo de volumen** (Bid/Ask/Delta); solo volumen total.  
* Puede generar **zonas solapadas** si el mercado está muy lateral.  


---

### 🛠️ Propuestas de mejora  

* Añadir filtro opcional por **delta o agresión** para refinar zonas.  
* Incorporar jerarquía visual por timeframe (prioridad de zonas HTF).  
* Permitir alertas contextuales al entrar/salir de una zona.  


---

### 💎 Valor Reutilizable (Código Donante)  

* Motor genérico de **zonas persistentes** con invalidación estructural.  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  

VolumeSupResZones no es Order Flow: es estructura de mercado reforzada por volumen. Su valor está en el **contexto**, no en el timing. Bien utilizado, reduce decisiones impulsivas y ayuda a alinear el scalping con niveles estructurales de mayor probabilidad.  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí, como contexto.**  

No genera entradas por sí solo, pero mejora significativamente la calidad de las decisiones cuando se combina con Order Flow.  

**Acción:** **Conservar (Reserva)**  
