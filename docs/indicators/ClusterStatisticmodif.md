## 🟦 Cluster Statistic

- **Nombre del archivo:** `ClusterStatisticModif.cs`  
- **Nombre del indicador:** Cluster Statistic modif 
- **Web oficial:** [ATAS — Cluster Statistic](https://help.atas.net/en/support/solutions/articles/72000602624-cluster-statistics)  
*(Versión extendida y optimizada por Alberto Amador Belchistim)*

---

### ⚙️ Parámetros configurables

- **Lines to show**: selección de métricas por barra (por ejemplo: Volumen, Delta, Trades, Máx. Bid/Ask, etc.).  
- **DigitsAfterComma**: número de decimales mostrados en las métricas.  
- **Color rules**: reglas dinámicas de color según signo o magnitud (por ejemplo delta positivo/negativo).  
- **Font / RowHeight**: tipografía y altura de fila del panel/tablas.  
- **Alignment & Spacing**: alineación y espaciado de columnas/etiquetas.  
- **Panel (New panel / Same panel)**: opción de mostrar en panel nuevo o reutilizar existente.

---

### 🧭 Clasificación  
📂 **VolumeOrderFlow** — Indicadores basados en estadísticas agrupadas por vela (volumen, delta, número de ejecuciones, etc.).

---

### 🧠 Uso más frecuente (versión original)

- Mostrar un **resumen por vela** de variables clave como Volumen, Delta, Trades.  
- Detectar barras con **delta extremo** (agresión de compra o venta).  
- Confirmar **impulso** cuando volumen y delta acompañan una ruptura.  
- Contextualizar **picos de actividad** (spikes de volumen o delta) en el flujo de órdenes.

---

### ✨ Mejoras introducidas en la versión por Alberto Amador Belchistim

Se han incorporado las siguientes mejoras respecto a la versión oficial:

**Visuales / UI**

- Reordenación de campos de la interfaz para mejorar legibilidad de métricas y facilitar interpretación rápida.  
- Ajustes de alineación, espaciado y tipografía para que el panel sea más claro en marcos rápidos (1 minuto).  
- Mejora del contraste y diferenciación de color para métricas clave (por ejemplo delta, volumen por segundo) para facilitar identificación de señales relevantes.

**Funcionales / métricas adicionales**

- Añadido cálculo de **Volumen por segundo (Vol/sec)** como nueva fila/serie en el resumen, permitiendo medir velocidad de acumulación en la vela. :contentReference[oaicite:0]{index=0}  
- Añadido cálculo de **Delta por segundo (Delta/sec)** para medir agresión neta en función del tiempo de vela. :contentReference[oaicite:1]{index=1}  
- Introducción de series “PeakVolPerSec” y “PeakDeltaPerSec” para identificar máximos instantáneos dentro de la vela. :contentReference[oaicite:2]{index=2}  
- Añadido “PeakDeltaPerVol” (Delta/Vol al nivel de máximo volumen/segundo) para evaluar eficiencia de impulso frente a volumen. :contentReference[oaicite:3]{index=3}  
- Integración de filtros avanzados/umbral “mean-based” (media/exponencial) para escala automática de métricas pico, permitiendo destacar eventos excepcionales en función del contexto histórico. :contentReference[oaicite:4]{index=4}  
- Implementación de imbalances de huella (buy/sell/net) con filtro de volumen y opción de alerta cuando el desequilibrio supera umbral configurado. :contentReference[oaicite:5]{index=5}  
- Corrección de cálculo de acumulación máxima de Bid (`maxBid`) y de `_maxDeltaPerVol` en actualizaciones de vela para mejorar precisión del resumen. :contentReference[oaicite:6]{index=6}

**Valor añadido práctico**

- Permite al trader ver **no solo qué se acumuló**, sino **cómo de rápido** (volumen/delta por segundo), lo cual mejora la detección de impulsos reales frente a ruido.  
- Las métricas pico (Vol/sec, Delta/sec) permiten **anticipar rupturas** o confirmarlas antes que el volumen total sea evidente.  
- Los imbalances de huella y filtros de umbral ayudan a detectar **actividad institucional o desequilibrio significativo** en tiempo real.  
- La mejora visual hace que el panel sea **más legible** en marcos rápidos (scalping) y en sesiones donde el espacio gráfico es limitado.

---

### 📊 Nivel de relevancia  
🔟 **8 / 10**

✅ Extiende una herramienta ya relevante en el ecosistema de flujo de órdenes.  
✅ Introduce métricas nuevas que mejoran detección y velocidad de respuesta.  
⛔ Requiere buen dominio del contexto de volumen/delta y cierto espacio visual en el gráfico.

---

### 🎯 Estrategias de scalping donde se aplica

- **Ruptura con convicción**: vela que muestra Volumen alto + Delta/sec elevado + PeakVol/sec > umbral = señal de continuación.  
- **Absorción de mercado**: Volumen elevado + Delta/segs bajo o negativo + desequilibrio de huella importante cerca de soporte/resistencia = posible rechazo.  
- **Falsas rupturas**: Volumen total alto pero Delta/sec bajo + PeakDeltaPerVol bajo = falta de impulso, posible retroceso.  
- **Secuencia de impulso sostenido**: varias velas consecutivas con incremento de Trades, Vol/sec, Delta/sec → confirmar continuidad.

---

### ⚙️ Parametrización óptima para scalping (1 m, S&P 500)

- **Lines to show**: `Volume`, `Delta`, `Vol/sec`, `Delta/sec`, `PeakVolPerSec`, `PeakDeltaPerVol`, `Trades`  
- **DigitsAfterComma**: `0‒1`  
- **Color rules**: Delta/sec > 0 verde (alcista), < 0 rojo (bajista); Peak valores resaltados en amarillo.  
- **RowHeight**: compacto (permite más filas visibles)  
- **Panel**: **New panel** (dedicado, debajo del gráfico principal)

✅ Optimiza la detección de impulsos rápidos y absorciones.  
✅ Permite analizar velocidad + volumen + desequilibrio en un vistazo.

---

### 🧪 Notas de desarrollo

- Basado en la implementación estándar del indicador de ATAS, pero ampliado con nuevas métricas que permiten una lectura más rica del flujo de órdenes.  
- La lógica original de acumulación por vela se mantiene, pero se añade un “window” de tiempo interno para calcular Vol/sec, Delta/sec, picos por segundo y umbrales dinámicos.  
- La interfaz ha sido ajustada para scalping, con mejor alineación, más métricas y mayor claridad visual.  
- Se recomienda actualizar también cualquier preset o configuración de espacio visual para que la nueva versión luzca óptima.

---

### 🛠️ Propuestas de mejora futura

- Añadir opción de **normalización por tamaño de vela** (por ejemplo, Vol/sec relativo al promedio de velas del día).  
- Incluir **histórico de velocidad** (Vol/sec media de última N velas) para detectar cambio de régimen.  
- Permitir **alertas automáticas** cuando Delta/sec o PeakDeltaPerVol superen umbral dinámico calculado por sesión.  
- Añadir **modo visual compacto** para operativa en marcos muy pequeños (Tick, 30 s) donde espacio es mínimo.

---