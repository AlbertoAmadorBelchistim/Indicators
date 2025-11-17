## 🟦 Volume Zone Oscillator (VZO) (9 / 10)  
**Nombre del archivo:** `VolumeZone.cs`  
**Nombre del indicador:** Volume Zone Oscillator  
**Web oficial:** [https://help.atas.net/support/solutions/articles/72000602268](https://help.atas.net/support/solutions/articles/72000602268)

---

### ⚙️ Parámetros configurables  
- **Period**: Periodo para el cálculo de las EMAs (por defecto: `14`)  
- **DrawLines**: Mostrar líneas horizontales de sobrecompra/sobreventa  
- **OverboughtLine1 / 2 / 3**: Líneas de sobrecompra (`50`, `75`, `90`)  
- **OversoldLine1 / 2 / 3**: Líneas de sobreventa (`-50`, `-75`, `-90`)  
- **Colores y visibilidad** de cada línea personalizables individualmente

---

### 🧭 Clasificación  
📂 Volume — Oscilador de momentum basado en volumen relativo a dirección del precio

---

### 🧠 Uso más frecuente  
- Medir la **intensidad del flujo de volumen comprador o vendedor**  
- Identificar zonas de **sobrecompra o sobreventa por volumen**  
- Confirmar **dirección del mercado** según la proporción de volumen positivo/negativo

---

### 📊 Nivel de relevancia  
🔟 **9 / 10**  
✅ Integra volumen y dirección de precio de forma suave y eficaz  
✅ Muy útil para detectar **zonas críticas** y validar movimientos  
⛔ Puede ser más lento en detectar giros bruscos si el volumen es bajo

---

### 🎯 Estrategias de scalping donde se aplica  
- **Entradas en extremos**: Entrar en reversión cuando el VZO cruza niveles como ±75 o ±90  
- **Confirmación de tendencia**: Operar en la dirección del VZO si se mantiene en zona positiva o negativa  
- **Evitar entradas en zona neutra**: Si el VZO está entre -50 y 50, evitar señales débiles

---

### ⚙️ Parametrización óptima para scalping (1M, S&P 500)  
- **Period**: `14`  
- **DrawLines**: `true`  
- **OverboughtLine1/2/3**: `50 / 75 / 90`  
- **OversoldLine1/2/3**: `-50 / -75 / -90`

✅ Claramente visual y muy interpretativo  
✅ Compatible con otras herramientas de volumen o delta  
⛔ En lateralidad prolongada puede fluctuar sin señal clara

---

### 🧪 Notas de desarrollo  
- Calcula dos EMAs:  
  - Una del volumen total (`_emaTv`)  
  - Otra del volumen relativo a la dirección (`_emaVp`), positivo si cierra más alto que la vela anterior  
- El valor del VZO es `100 × _emaVp / _emaTv`  
- Si el divisor es cero, repite el valor anterior  
- Dibuja 6 líneas horizontales configurables para zonas clave

---

### ❗ Incoherencias o aspectos mejorables detectadas  
- No valida explícitamente la división por cero, aunque usa fallback  
- No incluye opciones de **alerta automática** si se cruzan los niveles  
- La lógica asume que **todo volumen en vela alcista es positivo**, lo cual puede ignorar absorciones o mechas largas

---

### 🛠️ Propuestas de mejora  
- Añadir soporte para **alertas visuales o sonoras** al cruzar niveles  
- Permitir usar **delta o volumen agresivo** como fuente alternativa  
- Incluir una **versión suavizada adicional** para evitar señales erráticas en zonas neutras
