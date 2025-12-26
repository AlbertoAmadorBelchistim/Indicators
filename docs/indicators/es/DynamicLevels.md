---

# 1. IDENTIFICACIÓN  
cs_file: DynamicLevels.cs  
name: Dynamic Levels  
version: ATAS Stable/Latest  

# 2. CLASIFICACIÓN  
group: Order Flow  
subgroup: Volume Profile  
comparison_group: "Profile Levels (POC/VA)"  

# 3. VALORACIÓN (Score & Priority)  
score_current: 9/10  
score_potential: 9/10  
file_state: Estable  
effort: N/A  
action_priority: Baja  
system_priority: P1  

# 4. DECISIÓN  
recommended_action: Conservar (Core)  

# 5. ANÁLISIS  
description: ¿Dónde se están formando en tiempo real el POC, VAH y VAL del periodo activo, y cuándo el precio los toca o los cruza?  
gemini_summary: "DynamicLevels es el perfil de valor operativo base: calcula POC/VAH/VAL en tiempo real para un periodo (sesión/semana/mes, etc.) con render robusto y alertas. Gana por ser el más completo y estable del grupo para mapear valor intradía, con buen equilibrio entre utilidad y coste cognitivo."  
competitor_notes: "Frente a MaxLevels (nivel único) ofrece la triada completa POC/VAH/VAL y evolución dinámica. Frente a DynamicLevelsChannel (ventana móvil) es menos reactivo pero más estable y representativo del ‘valor’ acumulado del periodo; por ello debe ser Core y el canal queda como Reserva táctica."  
reusable_code: "Cálculo de Value Area basado en distribución por precio (PriceVolumeInfo) + lógica de render consistente de POC/VAH/VAL y sistema de alertas de toque."  

# 6. METADATOS  
analysis_date: 2025-12-26  
official_code_date: 2025-04-23  

---

## 🟢 Dynamic Levels (9/10)  

**Nombre del archivo:** [`DynamicLevels.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DynamicLevels.cs)  
**Nombre del indicador:** Dynamic Levels  
**Web oficial:** [ATAS — Dynamic Levels](https://help.atas.net/support/solutions/articles/72000602380)  
**Compatibilidad:** ATAS Stable/Latest.  
**Última revisión del código oficial:** 2025-04-23  

> **La Pregunta Clave:** ¿Dónde se están formando en tiempo real el POC, VAH y VAL del periodo activo, y cuándo el precio los toca o los cruza?  

![Dynamic Levels](../../img/DynamicLevels.png)


---

### ⚙️ Parámetros configurables  

**[Period & Source]**  
- **Period:** Periodo del perfil a calcular (sesión/semana/mes, etc.).  
  - Define el “reset” del cálculo y la ventana de acumulación.  
- **Type:** Fuente base para la distribución por precio.  
  - Normalmente `Volume`, y opcionalmente métricas derivadas (si el indicador lo soporta).  
- **Days:** Lookback en días para limitar el cálculo histórico y mejorar rendimiento en charts largos.  

**[Value Area / Filtering]**  
- **ValueAreaPercent:** Porcentaje de Value Area (por defecto típico 70%).  
- **Filter:** Umbral mínimo para incluir niveles en la distribución (reduce ruido de micro-volumen).  

**[Visual]**  
- **ShowPoc / ShowVah / ShowVal:** Mostrar u ocultar cada nivel.  
- **LineWidth:** Grosor de líneas.  
- **PocColor / VahColor / ValColor:** Colores por nivel.  
- **ExtendLines:** Extensión de los niveles hacia la derecha (contexto intradía).  
- **Text / FontSize / Offset:** Etiquetas y desplazamientos visuales.  

**[Alerts]**  
- **UseAlerts:** Activación general de alertas.  
- **AlertOnTouch / AlertOnCross:** Alertas por toque o cruce de nivel.  
- **AlertFile:** Sonido/archivo de alerta.  


---

### 🧭 Clasificación  
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Profile Levels (POC/VA)"  


---

### 🧠 Uso más frecuente  

* Definir el **mapa intradía de valor** (POC/VAH/VAL) para contextualizar el scalping.  
* Detectar **aceptación vs rechazo**: rotación dentro de VA vs ruptura y aceptación fuera.  
* Usar POC como **imán** y VAH/VAL como **fronteras operables** para fades o pullbacks.  


---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  

✅ Mapa de valor claro y estable (POC/VAH/VAL) para el periodo activo.  
✅ Utilidad directa para contexto de ejecución en M1 (sin necesidad de interpretación compleja).  
⛔ Menos reactivo que una ventana móvil: puede “arrastrar” el valor del inicio de sesión si hay cambio de régimen fuerte.  


---

### 🎯 Estrategias de scalping donde se aplica  

* **Fade dentro de Value Area:** rotación entre VAH ↔ VAL con confirmación de Order Flow.  
* **Reversión a POC:** extensiones que vuelven al punto de control del periodo.  
* **Pullback en tendencia:** aceptación fuera de VA + retest de VAH/VAL como soporte/resistencia dinámica.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  

| Parámetro | Valor recomendado | Justificación |  
| :--- | :--- | :--- |  
| **Period** | `Daily` | Referencia intradía estándar para scalping. |  
| **ValueAreaPercent** | `70` | Convención de mercado (área de valor). |  
| **Filter** | `0`–`50` | Ajustar según ruido del feed; filtra micro-niveles irrelevantes. |  
| **ExtendLines** | `true` | Mantiene niveles como mapa visual operable. |  
| **UseAlerts** | `false` | En M1 puede saturar; activar solo en investigación o con 1–2 niveles clave. |  


---

### 🧪 Notas de desarrollo  

* Indicador de **perfil por precio**: la lógica base deriva de una distribución `volumen ↔ precio` y extrae POC/VAH/VAL.  
* En ejecución, debe priorizarse un lookback (`Days`) razonable para no penalizar rendimiento en históricos largos.  
* En términos de “capas”, esto es un componente de **Niveles / Mapa**, no un trigger.  


---

### ❗ Incoherencias o aspectos mejorables detectados  

* Falta una opción explícita de **reset por sesión RTH/ETH** (si tu operativa diferencia).  
* Si el mercado cambia de régimen fuerte (tendencia post-noticia), el perfil acumulado puede tardar en “rotar” y el POC puede quedar rezagado respecto al valor reciente.  


---

### 🛠️ Propuestas de mejora  

* Añadir modo alternativo “**Rolling Value Area**” (ventana móvil) o integración opcional con el enfoque de DynamicLevelsChannel.  
* Añadir opción de **POC naked** (mantener POC previos como niveles heredados hasta ser testeados).  
* Exponer presets rápidos para scalping (RTH reset, VA 70, filtros por instrumento).  


---

### 💎 Valor Reutilizable (Código Donante)  

* Cálculo y extracción de **POC/VAH/VAL** desde distribución por precio.  
* Patrón de **líneas extendidas** con etiquetas consistentes y alertas por eventos de precio.  


---

### ✍️ La opinión de ChatGPT sobre el Indicador  

DynamicLevels es el “mapa de valor” que justifica un sistema de scalping disciplinado: te da contexto estructural inmediato (POC/VAH/VAL) con una carga cognitiva baja. En un stack de capas, es el nivel base para decidir dónde operar (zonas) antes de pasar a triggers (delta, imbalance, tape).  


---

### 📈 Veredicto: ¿Es útil para Scalping?  

**Sí.**  

Como capa de **Niveles / Mapa**, es una referencia central para planificar entradas y objetivos con coherencia.  

**Acción:** **Conservar (Core)**  


