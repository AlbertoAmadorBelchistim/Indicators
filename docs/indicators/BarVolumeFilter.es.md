## 🟦 Bar's Volume Filter (7/10)

  

**Nombre del archivo:** `BarVolumeFilter.cs`

**Nombre del indicador:** Bar's Volume Filter

**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602326](https://help.atas.net/support/solutions/articles/72000602326)

  

---

  

### ⚙️ Parámetros configurables

  

- **Type** (`VolumeType`): Tipo de volumen usado para filtrar:

- `Volume`: volumen total de la vela

- `Ticks`: número de ejecuciones

- `Delta`: diferencia entre agresión ask y bid

- `Bid`: volumen vendido agresivamente

- `Ask`: volumen comprado agresivamente

- **MinimumFilter**: valor mínimo requerido para marcar la vela (opcional)

- **MaximumFilter**: valor máximo permitido para marcar la vela (opcional)

- **FilterColor**: color aplicado a las velas que cumplan el criterio

- **TimeFilterEnabled**: activa filtro por horario

- **StartTime / EndTime**: define el rango horario de aplicación

  

---

  

### 🧭 Clasificación

📂 VolumeOrderFlow — Filtro visual de volumen/ticks/delta por vela

  

---

  

### 🧠 Uso más frecuente

  

- Resaltar **barras con volumen, delta o ticks inusuales**

- Filtrar visualmente **eventos significativos de actividad institucional**

- Centrar el análisis solo en **horarios relevantes o activos**

- Ignorar información fuera de sesión o durante mercados sin liquidez

  

---

  

### 📊 Nivel de relevancia

🔟 **7 / 10**

  

✅ Altamente configurable, rápido de interpretar

✅ Ideal como **filtro visual auxiliar** para setups complejos

⛔ No distingue entre direcciones ni contexto técnico

⛔ No tiene alertas ni representación numérica adicional

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Rupturas con volumen real**: solo marcar velas con volumen superior a `X`

- **Reacción institucional**: detectar compras o ventas agresivas con `Ask` / `Bid`

- **Validación de zonas clave**: volumen filtrado en test de soporte/resistencia

- **Evitar señales falsas fuera de horario** (pre-market, post-market)

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Type**: `Volume`

- **MinimumFilter**: `1500`

- **TimeFilterEnabled**: `true`

- **StartTime**: `15:30`

- **EndTime**: `22:00`

- **FilterColor**: naranja brillante o cian

  

✅ Muestra claramente barras de alta actividad durante la sesión americana

✅ Ayuda a enfocar solo donde hay participación institucional

⛔ El umbral puede necesitar ajuste según el día y contexto

  

---

  

### 🧪 Notas de desarrollo

  

- Evalúa cada vela individualmente según el tipo seleccionado (`VolumeType`)

- Aplica filtros mínimos y máximos, y condicionalmente por horario si está activado

- Usa `PaintBarsDataSeries` para colorear las velas que cumplen los criterios

- No posee alertas, etiquetas ni valores visibles adicionales

- Admite filtros cruzados por tiempo (incluso para sesiones nocturnas que cruzan medianoche)

  

---

  

### 🛠️ Propuestas de mejora

  

- Incluir opción de **alerta sonora o visual** al detectar volumen extremo

- Añadir **etiquetas numéricas** opcionales sobre la vela filtrada

- Soporte para **colores condicionales según intensidad del volumen**

- Posibilidad de guardar histórico de barras filtradas para análisis estadístico
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTI3ODIwNDU3NF19
-->