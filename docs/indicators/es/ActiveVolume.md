---
cs_file: ActiveVolume.cs
name: Active Volume
group: Order Flow
subgroup: Volume Profile
score_current: 9/10
version: Custom (v1.3.0)
recommended_action: Conservar (Core)
description: Filtrando ruido, ¿dónde está el volumen significativo y agresivo?
gemini_summary: "Herramienta 'Core' de Order Flow. Es un perfil de volumen filtrado por tamaño de orden. Permite ver la acumulación de agresión institucional en niveles de precio específicos, limpiando el ruido de los pequeños traders."
comparison_group: "Volume Profile"
competitor_notes: "Complementario a perfiles estándar. Se centra en la CALIDAD del volumen, no en la cantidad total."
reusable_code: null
file_state: Estable
score_potential: 9/10
effort: Medio
action_priority: P1
analysis_date: 2025-11-21
official_code_date: 13/11/2025
---

## 🏆 Active Volume (9/10)

**Nombre del archivo:**  [`ActiveVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/Develop/Technical/ActiveVolume.cs)  
**Versión modificada:** [`ActiveVolume.cs`](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/ActiveVolume.cs)  
**Nombre del indicador:** Active Volume  
**Web oficial:** [ATAS — Active Volume](https://help.atas.net/ru-RU/support/solutions/articles/72000608343-active-volume)  
**Compatibilidad:** ATAS versión estable y superiores.  
**Última revisión del código oficial:** 3/12/2024  
**Última revisión del código modificado:** 13/11/2025  

> **La Pregunta Clave:** Filtrando todas las pequeñas operaciones de 'ruido', ¿dónde está apareciendo realmente el volumen significativo y agresivo de compra y venta en la escala de precios?

![Active Volume](../../img/ActiveVolume.png)

---

### ⚙️ Parámetros configurables

Este indicador filtra y acumula trades para crear un perfil de "Calidad":

#### 📊 Filtros
* **Filter:** Volumen mínimo del trade para ser contabilizado (ej. 50 lotes). Todo lo menor se ignora.
* **DateFrom:** Fecha de inicio de la acumulación.

#### 🎨 Visualización
* **Mode:** `BidAsk` (Doble perfil), `Bid` o `Ask`.
* **Table:** Muestra una tabla numérica con los valores acumulados por precio.
* **Profile:** Dibuja el histograma visual en el lateral.
* **Colores:** Personalización de Bid/Ask y fondos.

---

### 🧭 Clasificación
**Grupo:** Order Flow  
**Subgrupo:** Volume Profile  
**Comparison Group:** "Volume Profile"  

---

### 🧠 Uso más frecuente

* **Mapa de Ballenas:** Ver dónde han acumulado posiciones los grandes jugadores (filtrando por tamaño).  
* **Niveles de Defensa:** Si ves una gran acumulación de `Bid` (Ventas) en un nivel que no baja, es absorción institucional.  
* **Validación de Ruptura:** Una ruptura debe dejar atrás un nodo de alto volumen agresivo (`Ask` fuerte) para ser válida.  

---

### 📊 Nivel de relevancia
🔟 **9 / 10 (IMPRESCINDIBLE)**

✅ **Filtrado Real:** A diferencia del Volume Profile normal, este ignora el ruido. Solo ves lo que importa.  
✅ **Desglose Bid/Ask:** Permite ver quién fue el agresor en cada nivel de precio.  
✅ **Precisión:** Acumula tick a tick, no es una estimación.  

---

### 🎯 Estrategias de scalping donde se aplica

* **Absorción Visual:** Ask elevado + Precio no rompe resistencia = Venta.  
* **Soporte Institucional:** Buscar el nivel con mayor volumen filtrado del día y usarlo como soporte para rebotar.  

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

| Parámetro | Valor Recomendado | Razón |
| :--- | :--- | :--- |
| **Filter** | `50` - `100` | Filtrar todo el ruido retail y HFT menor. |
| **Mode** | `BidAsk` | Ver ambos lados de la agresión. |
| **Show Table** | `True` | Ver los números exactos es vital. |
| **Digits** | `0` | Limpiar decimales innecesarios. |

---

### ✨ Mejoras añadidas (Custom)

* **Optimizaciones de UI:** Mejoras en la visualización de la tabla y redondeo de cifras.

---

### 🧪 Notas de desarrollo

* Utiliza `CumulativeTradesRequest` para obtener datos históricos filtrados.
* Mantiene diccionarios `_bidValues` y `_askValues` en memoria para acceso rápido por precio.
* Renderizado manual de la tabla y el perfil en `OnRender`.

---

### ❗ Incoherencias o aspectos mejorables detectados

* **Reset Diario:** Actualmente requiere cambiar la fecha manualmente o usar lógica de sesión. Sería ideal un "Auto-Reset Daily".

---

### 🛠️ Propuestas de mejora

* **Auto-Reset (P2):** Opción para reiniciar automáticamente al inicio de la sesión RTH.
* **Delta Mode (P1):** Añadir modo para ver el Delta Neto por precio (Ask - Bid) en lugar de los dos valores separados.

---

### 💎 Valor Reutilizable (Código Donante)

* **Lógica de Perfilado Manual:** El código de dibujo de la tabla y el histograma es muy limpio y reutilizable.

---

### ✍️ La opinión de Gemini sobre el Indicador

Es el complemento perfecto al `Volume Profile` estándar. El perfil estándar te dice "aquí se negoció mucho". Este indicador te dice "aquí negociaron LOS GRANDES". Esa distinción es oro.

**Propuestas de Acción:**
* **Conservar como CORE.**

---

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí.**

Para identificar dónde están posicionados los jugadores grandes.

**Acción:** **Conservar (Core).**