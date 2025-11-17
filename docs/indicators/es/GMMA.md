## 🟦 Guppy Multiple Moving Average (GMMA) (8.5/10)

**Nombre del archivo:** `GMMA.cs`  
**Nombre del indicador:** Guppy Multiple Moving Average  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602390](https://help.atas.net/support/solutions/articles/72000602390)

---

### ⚙️ Parámetros configurables

- **EmaPeriod1-6 (corto)**: Seis periodos para EMAs de corto plazo  
- **EmaLongPeriod1-6 (largo)**: Seis periodos para EMAs de largo plazo  
- **ShortColor / LongColor**: Colores de las EMAs de corto y largo plazo

---

### 🧭 Clasificación  
📂 Trend — Indicadores compuestos para evaluación de tendencia y compresión

---

### 🧠 Uso más frecuente

- Evaluar la **fuerza y estabilidad de una tendencia** observando la separación entre grupos de medias  
- Confirmar cambios de fase mediante la **contracción o expansión** del haz de EMAs  
- Identificar **momentos de transición entre consolidación y tendencia**

---

### 📊 Nivel de relevancia  
🔟 **8.5 / 10**

✅ Muy útil para evaluar estructura de mercado sin necesidad de osciladores  
✅ Proporciona lectura visual clara de compresión y expansión  
⛔ Ocupa espacio visual considerable  
⛔ Requiere configuración adecuada para cada activo o temporalidad

---

### 🎯 Estrategias de scalping donde se aplica

- **Expansión clara de GMMA corto**: entrada en favor del movimiento  
- **Contracción de GMMA largo + cruce del corto**: posible inicio de reversión  
- **Fase neutra**: evitar operar si ambos grupos están entrelazados  
- **Confirmación estructural**: validar setups solo si las EMAs se expanden en la dirección correcta

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

- **Cortos**: 3, 5, 7, 10, 12, 15  
- **Largos**: 30, 35, 40, 45, 50, 60  
- **Colores**: azul (corto), rojo (largo)  
- Ocultar valores (`IsHidden = true`) y usar solo visualización directa

✅ Muy efectivo para confirmar dirección antes de ejecutar  
✅ Ideal para filtrar zonas planas y operar solo en contexto direccional

---

### 🧪 Notas de desarrollo

- Implementa 12 EMAs: 6 para corto plazo y 6 para largo plazo  
- Cada una se calcula y dibuja de forma independiente con su serie respectiva  
- El código permite asignar un único color a todo el grupo corto/largo mediante propiedades unificadas  
- Todas las series están ocultas (`IsHidden = true`), lo que indica que están pensadas para representación visual, no para cálculo secundario

---

### ❗ Incoherencias o aspectos mejorables detectadas

- No se expone ninguna lógica de señal, cruce o validación estructural, lo cual **obliga al usuario a interpretarlo visualmente sin ayuda**  
- Se repite mucho código para cada EMA (corto y largo); podría implementarse una estructura con arrays para simplificar  
- Los colores de cada línea son uniformes por grupo; no se pueden configurar individualmente

---

### 🛠️ Propuestas de mejora

- Incluir lógica opcional de cruce entre grupos para generar señales de entrada/salida  
- Permitir definir los 6 periodos como lista desde la UI en vez de 6 propiedades separadas  
- Añadir opción para mostrar **ancho del haz** como histograma o línea secundaria  
- Incluir alertas visuales o sonoras al producirse apertura o cruce relevante
