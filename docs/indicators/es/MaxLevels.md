## 🟦 Maximum Levels (9/10)

**Nombre del archivo:** `MaxLevels.cs`  
**Nombre del indicador:** Maximum Levels  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602426](https://help.atas.net/support/solutions/articles/72000602426)

---

### ⚙️ Parámetros configurables

- **Period**: Periodo de análisis (día actual, semana, mes, etc.)  
- **TradingSession**: Sesión específica a analizar  
- **Type**: Tipo de nivel máximo (Volume, Bid, Ask, Delta positivo, negativo, etc.)  
- **Color / Width / Length**: Personalización visual de la línea  
- **Label**: Configuración del texto, valor, tamaño y color  
- **UseAlert / AlertFile**: Activar alertas si el precio alcanza el nivel  
- **AlertForeColor / AlertBgColor**: Colores de las alertas

---

### 🧭 Clasificación
📂 VolumeOrderFlow — Detección de niveles máximos de volumen u otros datos de clúster

---

### 🧠 Uso más frecuente

- Detectar **puntos de máximo interés institucional** en un periodo  
- Usar los niveles como **referencia clave de soporte/resistencia dinámica**  
- Confirmar entrada/salida si el precio interactúa con ese nivel

---

### 📊 Nivel de relevancia
🔟 **9 / 10**

✅ Muy eficaz para marcar niveles técnicos relevantes  
✅ Compatible con alertas automáticas y etiquetas  
⛔ Limitado a un único nivel por tipo y periodo; no muestra evolución

---

### 🎯 Estrategias de scalping donde se aplica

- **Reversión en máximos de volumen** (zona defendida)  
- **Entrada tras ruptura con volumen dominante**  
- **Confirmación institucional** si el precio se frena en el nivel

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Period**: `CurrentDay`  
- **Type**: `Volume`  
- **Length**: `300`  
- **UseAlert**: `true`  
- **ShowText / ShowValue**: activados  
- **TradingSession**: `Custom` (para aislar la sesión de cash US)

✅ Traza el nivel más relevante del día y permite actuar si el precio lo alcanza  
✅ Compatible con niveles de absorción, ruptura o consolidación  
⛔ No almacena históricos ni permite visualizar niveles anteriores en simultáneo

---

### 🧪 Notas de desarrollo

- Utiliza un objeto `FixedProfileRequest` para obtener el perfil máximo del periodo elegido  
- Traza una línea horizontal en el precio donde se produjo el valor máximo según el tipo seleccionado  
- Soporta niveles de máximo volumen, bid, ask, delta (positivo/negativo), ticks y tiempo  
- Permite personalizar etiquetas, colores, grosor, longitud y alertas sonoras/visuales  
- Solo se actualiza en tiempo real si se alcanza un nuevo máximo y el periodo ha sido cargado

---

### ❗ Incoherencias o aspectos mejorables detectadas

- El tipo `Time` sigue apareciendo como opción aunque esté marcado como `[Obsolete]`  
- No hay historial de niveles anteriores: al cambiar de día se pierde el nivel anterior  
- Si `GetFixedProfile` falla por falta de datos, el nivel no se actualiza y no hay advertencia al usuario  
- El uso de `_candleRequested` puede impedir recalculado en gráficos con gaps si no se resetea correctamente  
- El sistema de alertas no distingue entre rupturas fuertes y pequeños cruces → puede dar señales ruidosas

---

### 🛠️ Propuestas de mejora

- Ocultar `MaxLevelType.Time` de la interfaz o marcarlo claramente como obsoleto  
- Añadir opción para mostrar niveles de días anteriores (histórico)  
- Incluir opción de alertas condicionadas (por ejemplo: cerrar por encima o volumen mínimo)  
- Añadir relleno o banda en vez de solo línea si se quiere marcar zona  
- Implementar persistencia de niveles en estructuras para análisis retrospectivo

