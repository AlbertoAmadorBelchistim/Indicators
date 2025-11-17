## 🟦 True Range (8/10)  
**Nombre del archivo:** `TrueRange.cs`  
**Nombre del indicador:** True Range  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602234](https://help.atas.net/support/solutions/articles/72000602234)

---

### ⚙️ Parámetros configurables  
- No tiene parámetros configurables desde la UI.

---

### 🧭 Clasificación  
📂 Volatility — Indicador de volatilidad basado en el **True Range**

---

### 🧠 Uso más frecuente  
- Medir la **volatilidad del mercado** mediante el **True Range**  
- Identificar **rangos de precio** en un periodo determinado para evaluar el riesgo y la volatilidad  
- Utilizar como parte de estrategias para establecer **niveles de stop loss** y **take profit** basados en la volatilidad del mercado

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**  
✅ Ideal para **medir la volatilidad** del mercado  
✅ Funciona bien en **estrategias que requieren análisis de riesgo y volatilidad**  
⛔ Menos útil en **mercados sin fluctuaciones significativas** o con **baja volatilidad**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Rangos de volatilidad**: Usar el **True Range** para detectar **niveles de volatilidad extrema**  
- **Ajuste de stop loss**: Establecer **niveles de protección** en función de la **volatilidad actual del mercado**  
- **Tendencias y consolidaciones**: Confirmar **movimientos fuertes de precio** en mercados que atraviesan períodos de baja volatilidad

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **No aplica parámetros específicos**, ya que no tiene configuraciones desde la UI.

✅ Excelente para **medir rangos de volatilidad** y adaptar estrategias a la situación actual del mercado  
✅ Ideal para **ajustar stops dinámicamente** en función de la volatilidad  
⛔ Menos útil en **mercados sin cambios significativos** en el rango de precios

---

### 🧪 Notas de desarrollo  
- Calcula el **True Range** utilizando tres componentes:  
  1. La diferencia entre el **máximo y el mínimo** de la vela  
  2. La diferencia entre el **máximo de la vela** y el **cierre de la vela anterior**  
  3. La diferencia entre el **mínimo de la vela** y el **cierre de la vela anterior**  
- El valor del **True Range** es el máximo de los tres cálculos anteriores

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No permite personalizar el **estilo visual** del indicador  
- No tiene soporte para **alertas automáticas** cuando el True Range supera ciertos umbrales  
- **No ajusta dinámicamente** el cálculo en función de la volatilidad o el contexto de mercado

---

### 🛠️ Propuestas de mejora  
- Añadir **alertas automáticas** cuando el True Range alcance un valor específico  
- Mejorar la **personalización visual** del indicador (colores, grosor de línea)  
- Implementar un **ajuste dinámico** que permita calcular el True Range de manera más precisa durante eventos de alta volatilidad
