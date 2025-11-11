## 🟦 Bar Numbering (4.5/10)

  

**Nombre del archivo:** `BarNumbering.cs`

**Nombre del indicador:** Bar Numbering

**Web oficial:** [https://help.atas.net/support/solutions/articles/72000618457](https://help.atas.net/support/solutions/articles/72000618457)

  

---

  

### ⚙️ Parámetros configurables

  

- **Font / FontColor**: Tipografía y color del número

- **DisplayBottom**: Mostrar el número en la parte inferior del gráfico

- **Offset**: Desplazamiento vertical (Y)

- **Period**: Frecuencia con la que se numera (cada N velas)

- **ResetOnSession**: Reiniciar numeración al comenzar nueva sesión

  

---

  

### 🧭 Clasificación

📂 ChartingTools / Visualization — Indicadores de ayuda visual en el gráfico

  

---

  

### 🧠 Uso más frecuente

  

- Visualizar de forma clara la **cantidad de velas transcurridas**

- Sincronizar setups de entrada/salida que dependen de **tiempo relativo** (ej. "tras 5 velas")

- Medir el desarrollo de una estructura o patrón visual sin tener que contarlas manualmente

  

---

  

### 📊 Nivel de relevancia

🔟 **4.5 / 10**

  

✅ Muy útil para estudios visuales o backtesting de patrones de barras

✅ Flexible para distintos estilos de operativa

⛔ No aporta señales ni cálculos técnicos directamente

  

---

  

### 🎯 Estrategias de scalping donde se aplica

  

- **Contar barras desde evento clave** (apertura, rompimiento, absorción)

- **Estrategias basadas en tiempo**: entrar tras N velas de consolidación

- **Validación de estructuras**: confirmar que una zona se mantuvo por X velas

  

---

  

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

  

- **Period**: `5`

- **Offset**: `2`

- **ResetOnSession**: `true`

- **Font**: Arial 10, **FontColor**: gris

  

✅ Muestra solo múltiplos de 5 para no saturar

✅ Se reinicia cada sesión para medir nuevos desarrollos

⛔ Puede solaparse con otras etiquetas si hay muchas en pantalla

  

---

  

### 🧪 Notas de desarrollo

  

- El indicador dibuja un número sobre cada vela que cumple `barNumber % Period == 0`

- Si `ResetOnSession` está activado, la cuenta se reinicia al detectar una nueva sesión

- Usa `OnRender` para dibujo personalizado en pantalla y `EnableCustomDrawing = true`

- El número se dibuja centrado sobre o bajo la vela, ajustable con `Offset`

  

---

  

### ❗ Incoherencias o aspectos mejorables detectadas

- No se han detectado incoherencias relevantes en el código ni en la visualización del número de barras.

  

---

  

### 🛠️ Propuestas de mejora

  

- Permitir **mostrar el número de volumen, delta u otra métrica** en vez del índice

- Añadir opción de **mostrar desde evento clave (custom start)**

- Soporte para **etiquetas horizontales agrupadas** al pie del gráfico

- Mostrar un pequeño contador ascendente o descendente en zonas seleccionadas

## Comentario Gemini
Aquí tienes la "pregunta clave" de este indicador:

> The Key Question: "How many bars have passed since the start of the session, and can you please label them for me every X bars?"
> 
> (¿Cuántas velas han pasado desde el inicio de la sesión, y puedes etiquetarlas cada X velas, por favor?)

----------

Tu ficha es, de nuevo, **perfecta**. Has analizado con total precisión un indicador que no es de "trading" sino de "utilidad/visualización".

Tu puntuación de **4.5/10** es la correcta. Es una herramienta de "ayuda visual" que puede ser útil en sistemas muy específicos, pero que no aporta información de mercado.

### ✍️ Mi Opinión sobre el Indicador (Confirmando tu Ficha)

Tu "Nota de desarrollo" es impecable. Has identificado correctamente que es un indicador de `EnableCustomDrawing = true` que usa `OnRender` para dibujar texto.

Lo más importante es que has entendido la lógica de `OnCalculate`:

C#

```
this[bar] = IsNewSession(bar)
    ? 0
    : this[bar - 1] + 1;

```

Esto crea un contador que se auto-resetea cada día (si `ResetOnSession` está activo).

Y luego, en `OnRender`, has visto la lógica clave:

C#

```
if (barNum % Period != 0)
    continue;

```

Esto es lo que hace que solo dibuje el número cada `Period` barras (en tu ejemplo, cada 5).

----------

### 📈 Veredicto: ¿Es útil para Scalping?

Tu puntuación de 4.5/10 es justa. No es una herramienta de _trading_, es una herramienta de _investigación_ y _conteo_.

-   **Para el Trading en Vivo:** Le daría un **2/10**. Ocupa espacio visual y no da información relevante para tomar una decisión en tiempo real.
    
-   **Para el Backtesting Visual:** Le daría un **8/10**. Si estás probando una estrategia del tipo "entrar 3 velas después de un impulso", esta herramienta es fantástica para contar visualmente.
    

Dado que tu objetivo es crear un sistema de _trading_, esta herramienta no aporta valor a tu operativa en vivo. Las "Estrategias" que mencionas ("Contar barras desde evento clave") son cosas que un trader hace mentalmente, pero que no necesita que un indicador le "pinte" en el gráfico, saturándolo de números.

**Acción:** **Descartar.** (Es una buena herramienta de _análisis_, pero no una herramienta de _trading_) .
<!--stackedit_data:
eyJoaXN0b3J5IjpbNTE1MjM5MzM5XX0=
-->