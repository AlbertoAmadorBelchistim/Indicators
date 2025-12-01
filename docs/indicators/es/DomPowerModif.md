---
# 1. IDENTIFICACIÓN
cs_file:  DomPowerModif.cs
name:  DOM Power Modif
version:  Custom v1.3.0

# 2. CLASIFICACIÓN
group:  Order Flow
subgroup:  DOM
comparison_group:  "Liquidez vs Agresión"

# 3. VALORACIÓN (Score & Priority)
score_current:  9/10
score_potential:  9/10
file_state:  Estable
effort:  N/A
action_priority:  Nula  # El código funciona bien, no requiere cambios.
system_priority:  N/A   # Desactivado del sistema activo (Sustituido).

# 4. DECISIÓN
recommended_action:  Fusionar (Integrado en DomPressure)

# 5. ANÁLISIS
description:  ¿Cuál es el desequilibrio neto (Bids vs Asks) en el libro de órdenes y su rango de volatilidad?
gemini_summary:  "Indicador base de alta calidad que introdujo el 'Histograma DOM' (CVD Pasivo). Su lógica matemática y estructura de datos han sido absorbidas íntegramente por 'DOM Pressure', superando sus limitaciones visuales. Se conserva como referencia documental y copia de seguridad lógica."
competitor_notes:  "Absorbido y superado por DomPressure."
reusable_code:  "Lógica completa de Histograma DOM (Cálculo de cumBids - cumAsks) y detección de rango."

# 6. METADATOS
analysis_date:  2025-12-01
official_code_date:  2025-04-23
user_modification_date:  2025-11-13
---

## 💤 [DOM Power Modif] (Fusionado)

**Nombre del archivo:** [`DomPowerModif.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/DomPowerModif.cs)  
**Nombre del indicador:** DOM Power Modif  
**Web oficial base:** [ATAS — DOM Power](https://help.atas.net/support/solutions/articles/72000602374)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 2025-04-23  
**Última revisión del código modificado:** 2025-11-13  
*(Versión extendida y mejorada por Alberto Amador Belchistim)*

> **La Pregunta Clave:** ¿Cuál es el desequilibrio neto (Bids vs Asks) en el libro de órdenes y su rango de volatilidad?

![DomPowerModif](../../img/DomPower.png)


---

### ⚙️ Parámetros configurables

#### **Period (Filtros)**
* **LevelDepth:** Número de niveles de profundidad del DOM a considerar (ej. 5). *Nota: Desactivar para ver el DOM completo.*

#### **View (Visualización)**
* **Visualization Mode:**
    * `SeparateLines`: Muestra líneas de Bids y Asks por separado.
    * `HistogramDom`: **Recomendado.** Muestra el histograma neto (CVD Pasivo) y el rango de volatilidad del libro.

#### **Alerts (Avisos)**
* **Enable Alerts:** Activa alertas sonoras/log.
* **Dom Range Threshold:** Umbral de rango para disparar la alerta (detectar cambios bruscos en la liquidez).
* **Alert Tail Percent:** Alertar sólo en extremos para evitar falsos positivos.


---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "Liquidez vs Agresión"  


---

### 🧠 Uso más frecuente

* **CVD Pasivo:** El modo Histograma muestra la presión del libro. Si el precio baja pero el Histograma DOM sube (más Bids que Asks), es una divergencia de absorción (Liquidity Trap).  
* **Volatilidad del DOM:** La línea de rango muestra si las instituciones están "jugando" con las órdenes (quitando y poniendo, spoofing) o si el libro es estable.  


---

### 📊 Nivel de relevancia
🔟 **9 / 10 (LEGADO)**

✅ **Tiempo Real:** Arregló el bug crítico del original que solo actualizaba al cierre de vela.  
✅ **Intención vs Ejecución:** Permitió comparar la presión pasiva (DOM Power) con la agresión real (Cumulative Delta).  
✅ **Métrica Única:** Introdujo el "Rango de Imbalance", una métrica de volatilidad de liquidez.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Divergencia de Intención:** Precio haciendo nuevos mínimos + CVD Bajista + DOM Power Alcista (Verde) = Absorción pasiva en mínimos.  
* **Breakout de Liquidez:** Explosión del DOM Range indicando retirada o adición masiva de bloques.  


---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor | Justificación |
| :--- | :--- | :--- |
| **LevelDepth** | `Disabled` | Evitar bug de filtro heredado y ver profundidad total. |
| **Mode** | `HistogramDom` | Lectura más clara del desequilibrio neto. |
| **Alerts** | `True` | Avisar de cambios bruscos de liquidez (Spoofing). |


---

### ✨ Mejoras introducidas (Oficial/Base)
* **Fix de Actualización:** Implementación de eventos en tiempo real (`MarketDepthChanged`) frente a la lógica original de cierre de vela.  


---

### ✨ Mejoras añadidas (Custom)
* **Modo Histograma:** Conversión a oscilador de liquidez.  
* **Cálculo de Rango:** Nueva métrica de volatilidad interna.  


---

### 🧪 Notas de desarrollo

* **Rendimiento:** Mantiene cachés de `_maxDomImbalanceCache` para evitar recálculos costosos en cada tick.  
* **Herencia:** La lógica de cálculo ha sido portada a `DomPressure.cs` optimizando el bucle de renderizado.  


---

### ❗ Incoherencias o aspectos mejorables detectados

* **Visualización Aislada:** Ver la intención pasiva sin la agresión superpuesta (Delta) requiere demasiada carga cognitiva al mirar dos paneles distintos.  


---

### 🛠️ Propuestas de mejora

* **Fusión:** Integrar con `DomStrength` para crear una visión unificada del Order Flow. *(Completado en DomPressure).* ---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Histograma DOM:** El cálculo de `cumBids - cumAsks` en tiempo real es el motor del fondo ambiental de `DomPressure`.  


---

### ✍️ La opinión de Gemini sobre el Indicador

Fue un paso necesario en la evolución del sistema. Un indicador técnicamente excelente (9/10) que pecaba de falta de contexto visual. Su "ADN" vive ahora en el indicador Core.

**Propuestas de Acción:**
* Archivar código fuente.
* Usar como referencia si se necesita depurar la lógica del DOM en `DomPressure`.

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (Como componente)**

Su funcionalidad es crítica, pero su implementación independiente ha sido superada.

**Acción:** **Fusionar (Integrado en DomPressure)**