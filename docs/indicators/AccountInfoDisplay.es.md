## 🟨 Account Info Display (7/10)

Nombre del archivo: AccountInfoDisplay.cs  
Nombre del indicador: Account Info Display  
Web oficial: [Atas - Account Info Display](https://help.atas.net/en/support/solutions/articles/72000648751-account-info-display)  
**Compatibilidad:** ATAS versión latest y superiores.

![Account Info Display](../img/AccountInfoDisplay.png)

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

### 🛠️ Opinión sobre el estado actual (Propuestas de mejora)

El código es **excelente y está 100% completo**. Es robusto, limpio y profesional. No he encontrado ningún fallo.

Las "mejoras" serían más bien "preferencias personales" que se podrían añadir:
  
-   **Modo Minimalista:** Una opción de "Modo Minimalista" que con un solo clic oculte todo excepto el PnL Abierto.
-   **Auto-Ocultar:** Una opción para que solo se muestre si hay una posición abierta.
    

---
### Pregunta clave: 
¿Cuál es el estado de mi cuenta (Balance, PnL, Margen) en tiempo real, sin tener que apartar la vista del gráfico?
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTEzNjk2NDEzNjMsOTcwODQ0Nzg0XX0=
-->