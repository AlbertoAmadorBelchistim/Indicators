## 🟦 Trade Volume Index (TVI) (7/10)  
**Nombre del archivo:** `TVI.cs`  
**Nombre del indicador:** Trade Volume Index  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602296](https://help.atas.net/support/solutions/articles/72000602296)

---

### ⚙️ Parámetros configurables  
- No tiene parámetros configurables desde la UI.

---

### 🧭 Clasificación  
📂 Volume — Volumen clásico acumulado según movimiento del precio

---

### 🧠 Uso más frecuente  
- Medir la **presión compradora o vendedora** en función de cómo se acumula el volumen con respecto a los movimientos del precio  
- Detectar **tendencias basadas en volumen** en lugar de solo en cambios de precio  
- Confirmar movimientos sostenidos cuando el volumen acompaña el cambio de dirección del precio

---

### 📊 Nivel de relevancia  
🔟 **7 / 10**  
✅ Útil para observar si el volumen **confirma o contradice** el movimiento del precio  
✅ Puede emplearse para identificar **acumulación o distribución silenciosa**  
⛔ Menos efectivo en mercados con **bajo volumen o alta oscilación sin dirección clara**

---

### 🎯 Estrategias de scalping donde se aplica  
- **Confirmación de tendencia**: Si el precio sube y el TVI también, hay validación del movimiento  
- **Divergencias volumen-precio**: Señales de reversión si el precio sube pero el TVI baja  
- **Seguimiento de acumulación**: Ver si el volumen apoya un rompimiento o una consolidación

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- No tiene parámetros configurables; se recomienda usar como referencia en conjunto con otros indicadores

✅ Puede confirmar **presión compradora o vendedora sostenida**  
✅ Complemento ideal para **validar señales de otros indicadores**  
⛔ No configurable ni adaptable a condiciones particulares sin intervención en el código

---

### 🧪 Notas de desarrollo  
- Suma o resta el volumen según si el cierre actual supera o no el de la vela anterior  
- Si el cambio es exactamente un tick, no suma ni resta volumen  
- El cálculo es acumulativo y se reinicia con cada nueva sesión o carga de gráfico

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No permite **resetear o limitar el acumulado** del TVI en rangos específicos  
- No configurable desde la UI, lo que impide **ajustes de sensibilidad**  
- Puede presentar **comportamiento plano** en mercados laterales o poco líquidos

---

### 🛠️ Propuestas de mejora  
- Añadir opción de **reset por sesión** o por número de barras  
- Permitir **visualización adaptativa** (p. ej., porcentaje del volumen relativo)  
- Incluir **opciones de alerta** cuando se crucen ciertos umbrales de acumulación positiva o negativa
