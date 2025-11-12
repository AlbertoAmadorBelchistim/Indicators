## 🟦 Account Info Display (7/10)


**Nombre del archivo**: AccountInfoDisplay.cs  
**Nombre del indicador**: Account Info Display  
**Web oficial**: [Atas - Account Info Display](https://help.atas.net/en/support/solutions/articles/72000648751-account-info-display)  
**Compatibilidad:** ATAS versión latest y superiores.

![Account Info Display](../img/AccountInfoDisplay.png)

**La Pregunta Clave:** ¿Cuál es el estado de mi cuenta (Balance, PnL, Margen) en tiempo real, sin tener que apartar la vista del gráfico?

----------

### ⚙️ Parámetros configurables

Este indicador es puramente visual y tiene 3 grupos de parámetros:

1.  **Contenido (Show/Hide):** Permite activar o desactivar cada línea de información (ej. `ShowAccountId`, `ShowBalance`, `ShowOpenPnL`, `ShowMargin`, etc.).
    
2.  **Visualización:** Colores de fondo, texto, y colores específicos para PnL positivo, negativo o neutral. También el tamaño de la fuente.
    
3.  **Posicionamiento (Layout):** Permite anclar el panel a cualquier esquina o centro del gráfico (`HorizontalPosition`, `VerticalPosition`) y ajustar la distancia con `OffsetX` y `OffsetY`.
    

----------

### 🧭 Clasificación

📂 Utilidad — Indicadores de visualización de datos (Dashboard)

----------

### 🧠 Uso más frecuente

-   **Monitorización del PnL:** Ver el beneficio o pérdida de las operaciones abiertas en tiempo real sobre el gráfico.
    
-   **Gestión de Riesgo:** Vigilar el margen bloqueado (`Blocked Margin`) y el balance disponible para no sobre-apalancarse.
    
-   **Dashboard Integrado:** Evitar tener que cambiar de pestaña o mirar a otro monitor para ver el estado de la cuenta.
    

----------

### 📊 Nivel de relevancia

🔟 **7 / 10**

✅ Utilidad Alta: Extremadamente útil para la gestión de la operativa en tiempo real.

✅ Altamente Configurable: Permite al trader mostrar solo lo que le importa (ej. solo el PnL abierto).

✅ Evita Distracciones: Mantiene la información clave en un solo lugar (el gráfico).

⛔ Ocupa Espacio: Es un elemento visual que ocupa espacio en el gráfico.

⛔ No da Señales: Es puramente informativo, no genera señales de compra/venta.

----------

### 🎯 Estrategias de scalping donde se aplica

Este indicador no da señales, pero **apoya la ejecución y gestión** del scalping:

-   **Gestión de Riesgo Visual:** Ver el `Open PnL` te permite gestionar la operación (ej. cerrar al alcanzar un objetivo monetario o un stop de pérdida monetario) sin apartar la vista del precio.
    
-   **Monitorización de Margen:** Un scalper que opera con múltiples contratos puede vigilar el `Blocked Margin` para asegurar que tiene capital suficiente para añadir posiciones.
    

----------

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)

La "optimización" aquí es minimalista, para evitar distracciones:

-   **ShowOpenPnL**: `true` (Lo más importante)
    
-   **ShowAccountId**: `false`
    
-   **ShowBalance**: `false` (Puede distraer)
    
-   **ShowAvailableBalance**: `false`
    
-   **ShowMargin**: `true` (Importante si operas con mucho apalancamiento)
    
-   **ShowLeverage**: `false`
    
-   **ShowClosedPnL**: `false` (El PnL cerrado ya no importa para la operación actual)
    
-   **VerticalPosition**: `Bottom`
    
-   **HorizontalPosition**: `Left`
    
-   **OffsetX / OffsetY**: `20` (Para dejar un margen)
    

----------

### 🧪 Notas de desarrollo

-   Este es un indicador de **dibujo personalizado** (`EnableCustomDrawing = true`).
    
-   **No usa `OnCalculate`**, ya que no procesa las velas.
    
-   Toda la lógica reside en **`OnRender`**, donde dibuja un rectángulo y texto coloreado.
    
-   Es un código muy limpio: separa la obtención de datos (`BuildDisplayText`), el cálculo de la posición (`CalculateXPosition`, `CalculateYPosition`) y el dibujo (`DrawColoredText`).
    
-   Se suscribe al evento `TradingManager.PortfolioSelected`, lo que significa que si cambias de cuenta en ATAS, el indicador se actualizará automáticamente.
    

----------

### ❗ Incoherencias o aspectos mejorables detectados

-   El código es robusto y limpio. No se detectan fallos funcionales ni incoherencias.
    

----------

### 🛠️ Propuestas de mejora

-   Añadir una opción "Modo Minimalista" que con un solo clic oculte todo excepto el `OpenPnL`.
    
-   Añadir una opción de "Auto-Ocultar" para que el panel solo sea visible si hay una posición abierta.
    

----------

----------

### ✍️ La opinión de Gemini sobre el Indicador (El Análisis Correcto)

Este no es un indicador, es una **extensión de la interfaz de usuario (UI)**. No analiza datos, solo los _muestra_. Su valor no es analítico, sino de pura **conveniencia**.

Desde la perspectiva del desarrollador (como indican las notas), el código es limpio, profesional y sigue las mejores prácticas (`OnRender` en lugar de `OnCalculate`, suscripción a eventos, etc.). Es un código de alta calidad.

Desde la perspectiva del trader, es un arma de doble filo:

1.  **El Lado Bueno:** Para un scalper que gestiona un alto volumen de contratos, ver el `Blocked Margin` (Margen Bloqueado) directamente en el gráfico es una herramienta de gestión de riesgo vital. Le impide sobre-apalancarse accidentalmente.
    
2.  **El Lado Malo (La Trampa Psicológica):** Se ha demostrado que "vigilar el PnL" (`PnL-watching`) es una de las peores cosas que un trader puede hacer. Ver el PnL fluctuar tick a tick induce a tomar decisiones emocionales (cerrar ganancias demasiado pronto por miedo a que se reviertan, o aguantar pérdidas demasiado tiempo). Un trader disciplinado debería operar basándose en el _precio_ y la _estructura_, no en el _dinero_.
    

----------

### 📈 Veredicto: ¿Es útil para Scalping?

**Sí, pero con una advertencia psicológica.**

Es una herramienta de **conveniencia** (7/10), no una herramienta de _análisis_ (0/10).

El uso correcto para un scalper es la configuración "minimalista" descrita en la ficha: mostrar **SÓLO** el `Open PnL` y, más importante aún, el `Blocked Margin`. Ocultar el `Balance`, `Closed PnL` y otros datos "ruidosos".

Debe usarse como una herramienta de "chequeo de riesgo" (¿cuánto margen me queda?), no como una herramienta de "gestión de trade" (¿cuánto estoy ganando/perdiendo ahora mismo?).

**Acción:** **Conservar.** Es un indicador de utilidad bien construido que, usado con disciplina, aporta valor al centralizar la información.
<!--stackedit_data:
eyJoaXN0b3J5IjpbMjAxNDkyNDA2NiwtMTM2OTY0MTM2Myw5Nz
A4NDQ3ODRdfQ==
-->