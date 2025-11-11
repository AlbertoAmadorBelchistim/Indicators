## 🟦 Bar Difference (3.5/10)

  

**Nombre del archivo:**  `BarDifference.cs`

**Nombre del indicador:** Bar Difference

**Web oficial:**  [https://help.atas.net/support/solutions/articles/72000602523](https://help.atas.net/support/solutions/articles/72000602523)

  

---

  

### ⚙️ Parámetros configurables

  

- **Period**: Número de velas hacia atrás con las que se calcula la diferencia

  

---

  

### 🧭 Clasificación

📂 Momentum — Indicadores de cambio de precio entre barras

  

---

  

### 🧠 Uso más frecuente

  

- Medir **la diferencia de precio entre la vela actual y una anterior**

- Detectar movimientos abruptos o microimpulsos

- Identificar contextos de aceleración o reversión por sobreextensión

  

---

  

### 📊 Nivel de relevancia

🔟 **3.5 / 10**

  

✅ Simple y directo para detectar desplazamientos de corto plazo

✅ Compatible con cálculos adicionales de momentum o breakouts

⛔ No considera contexto ni volumen, solo el precio

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Micropullbacks**: detectar retrocesos en tendencia de baja magnitud

- **Reversión rápida**: diferencias negativas tras tramo alcista fuerte

- **Impulso inicial**: usar cambios altos en las primeras velas del día para entradas rápidas

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `3`

  

✅ Detecta microimpulsos y sobreextensiones útiles en aperturas o rupturas

⛔ Puede dar señales falsas en rangos laterales o en alta volatilidad

  

---

  

### 🧪 Notas de desarrollo

  

- Calcula: `(Close actual - Close n velas atrás) / TickSize`, por lo que muestra la diferencia en ticks

- La serie `RenderSeries` se representa en un panel aparte (histograma por defecto)

- No depende de volumen ni de otros indicadores

  

---

  

### 🛠️ Propuestas de mejora

  

- Añadir **modo absoluto** para diferenciar solo por magnitud

- Incluir **coloración condicional** según dirección o umbral

- Combinar con delta o volumen para filtros más sofisticados

- Opción para usar máximo/mínimo en vez de cierre
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTEwNTc2MTkzNl19
-->