---
# 1. IDENTIFICACIÓN
cs_file:  DOM.cs  
name:  Depth Of Market  
version:  ATAS Latest  

# 2. CLASIFICACIÓN
group:  Order Flow  
subgroup:  DOM  
comparison_group:  "DOM Visuals"  

# 3. VALORACIÓN (Score & Priority)
score_current:  7/10  
score_potential:  7/10  
file_state:  Estable  
effort:  Bajo  
action_priority:  Baja  
system_priority:  P3  

# 4. DECISIÓN
recommended_action:  Conservar (Reserva)  

# 5. ANÁLISIS
description:  ¿Cuánta liquidez hay AHORA MISMO en cada nivel (agregado)?  
gemini_summary:  "La herramienta básica. Funcional pero redundante si se usa MBO DOM, que incluye esta funcionalidad. Renderizado GDI+ estándar."  
competitor_notes:  "Inferior al MBO DOM en detalle. Inferior a DomLevels en historia."  
reusable_code:  null  

# 6. METADATOS
analysis_date:  2025-11-30  
official_code_date:  2025-10-11  
---

## 📊 Depth Of Market (7/10)

**Nombre del archivo:** [`DOM.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/DOM.cs)  
**Nombre del indicador:** Depth Of Market  
**Web oficial:** [ATAS — Depth of Market](https://help.atas.net/support/solutions/articles/72000602367)  
**Compatibilidad:** ATAS versión latest y superiores.  
**Última revisión del código oficial:** 2025-10-11  

> **La Pregunta Clave:** ¿Cuánta liquidez hay AHORA MISMO en cada nivel de precio (agregado)?

![DOM](../../img/DOM.png)

---

### ⚙️ Parámetros configurables

#### **Histogram Size (Dimensiones)**
* **Visual Mode:** 
    * `Common`: Barras de compra/venta separadas (estilo clásico).
    * `Cumulative`: Barras de suma acumulada (para ver profundidad total).
    * `Combined`: Superpone ambas vistas.
* **Use Auto Size:** Ajusta el ancho de las barras automáticamente al máximo volumen visible.  
* **Proportion Volume:** (Si AutoSize off) Define qué volumen representa el 100% del ancho.
* **Width:** Ancho máximo de las barras en píxeles.
* **Right To Left:** Cambia la dirección de las barras (de derecha a izquierda o viceversa).  

#### **Levels Mode (Estilo)**
* **Bid/Ask Rows & Backgrounds:** Colores para las barras y el fondo de cada nivel.  
* **Filter Colors:** Lista de filtros. Permite asignar colores específicos (ej. amarillo brillante) a niveles que superen X volumen.  

#### **Other Settings (Otros)**
* **Show Cumulative Values:** Muestra el volumen total de bid y ask sumando el de todos los niveles.
* **Price Levels Height:** Altura de cada nivel de precio en píxeles (0 = altura real).

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** DOM  
**Comparison Group:** "DOM Visuals"  

---

### 🧠 Uso más frecuente

* **Lectura Rápida:** Vistazo rápido a la liquidez lateral sin necesidad de interpretación compleja.  
* **Respaldo:** Uso cuando el proveedor de datos no soporta MBO.  


---

### 📊 Nivel de relevancia
7️⃣ **7 / 10**

✅ **Sencillez:** Fácil de leer, consumo mínimo de CPU.  
⛔ **Redundancia:** El indicador `MBO DOM` muestra los mismos datos con mayor nivel de detalle.  
⛔ **Ceguera:** No permite ver si un nivel de 500 lotes es 1 orden o 500 órdenes.  


---

### 🎯 Estrategias de scalping donde se aplica

* **Soporte/Resistencia Básico:** Identificación de muros de volumen agregado.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

* **Visual Mode:** `Common` (Menos intrusivo).
* **Width:** `100` (Estrecho, solo referencia).
* **Filter Colors:** Añadir filtro > 300 lotes en color Amarillo.

---

### 🧪 Notas de desarrollo

* **Código Básico:** Utiliza `OnRender` estándar con bucles sobre `MarketDepthInfo.GetMarketDepthSnapshot()`.  
* **Renderizado:** Dibuja rectángulos (`context.FillRectangle`) y texto para cada nivel de precio visible. No usa aceleración compleja.  


---

### ❗ Incoherencias o aspectos mejorables detectados

* **Superposición:** En momentos de alta volatilidad, el redibujado puede tener ligero retraso visual respecto al precio real en el chart.  


---

### 🛠️ Propuestas de mejora

* **Ninguna:** Es un indicador legado.  


---

### 💎 Valor Reutilizable (Código Donante)

* **Null:** No aporta lógica que no tengamos mejor implementada en MBO DOM o DomLevels.  


---

### ✍️ La opinión de Gemini sobre el Indicador

Un clásico fiable, pero obsoleto en un entorno de trading profesional con acceso a datos MBO. Guárdalo en la recámara por si cambias a un datafeed de menor calidad o necesitas una visualización simplificada.

**Propuestas de Acción:**
* Mover a carpeta de "Reservas".

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí (Básico)**

Útil, pero superado por sus competidores.

**Acción:** **Conservar (Reserva)**