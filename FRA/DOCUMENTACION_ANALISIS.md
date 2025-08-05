# ANÁLISIS DE APTITUD PARA SERVICIO SOCIAL

## Objetivo
Identificar qué personas son aptas para recibir un servicio social basado en:
- **Región** (municipio)
- **Clasificación** (población indígena, vulnerabilidad económica)
- **Índice económico** (para evitar que dejen la escuela)

## Metodología

### 1. Variables Analizadas

**Variables Demográficas:**
- `municipio`: Región geográfica
- `pob_indigena`: Porcentaje de población indígena
- `pob_autoadscrita_indigena`: Población que se identifica como indígena

**Variables Económicas:**
- `pobreza`: Condiciones de pobreza (%)

**Variables Educativas:**
- `asistencia_escolar_indigena`: Tasa de asistencia escolar indígena (%)
- `analfabetismo_indigena`: Tasa de analfabetismo indígena (%)
- `escolaridad_promedio_indigena`: Grado promedio de escolaridad (años)
- `educacion_basica_indigena`: Población con educación básica (%)

### 2. Indicadores de Aptitud

**Vulnerabilidad Económica:**
- Alta: Pobreza > mediana
- Baja: Pobreza ≤ mediana

**Riesgo de Abandono Escolar:**
- Alto: Asistencia escolar < mediana
- Bajo: Asistencia escolar ≥ mediana

**Necesidad Educativa:**
- Alta: Escolaridad promedio < mediana
- Baja: Escolaridad promedio ≥ mediana

### 3. Puntuación de Aptitud (0-100)

**Fórmula:**
```
Puntuación = (100 - pobreza) × 0.4 + 
             asistencia_escolar × 0.3 + 
             escolaridad_promedio × 0.2 + 
             pob_indigena × 0.1
```

**Clasificación:**
- 80-100: Muy Alta
- 60-79: Alta
- 40-59: Media
- 20-39: Baja
- 0-19: Muy Baja

### 4. Prioridad de Atención

**Crítica:** Alta vulnerabilidad económica + Alto riesgo de abandono
**Alta:** Alta vulnerabilidad económica O Alto riesgo de abandono
**Media:** Alta necesidad educativa
**Baja:** Resto de casos

## Resultados Esperados

### 1. Identificación de Municipios Críticos
- Municipios con mayor pobreza
- Menor asistencia escolar
- Mayor población indígena vulnerable

### 2. Análisis por Región
- Ranking de municipios por aptitud
- Distribución de prioridades
- Correlaciones entre variables

### 3. Modelo Predictivo
- Árbol de decisión para clasificar aptitud
- Evaluación de precisión del modelo
- Predicción de nuevos casos

## Visualizaciones

### 1. Distribución de Aptitud
- Gráfico de barras por nivel de aptitud
- Frecuencia de cada categoría

### 2. Relación Pobreza-Asistencia
- Gráfico de dispersión
- Línea de tendencia
- Coloreado por prioridad

### 3. Análisis por Municipio
- Distribución de prioridades por región
- Proporción de casos críticos

## Archivos de Salida

### 1. `resultados_aptitud_servicio_social.csv`
- Datos completos con indicadores
- Puntuaciones y clasificaciones
- Prioridades de atención

### 2. `municipios_criticos_servicio_social.csv`
- Municipios con prioridad crítica
- Estadísticas por municipio
- Casos más urgentes

### 3. `analisis_por_region.csv`
- Análisis agregado por municipio
- Promedios de indicadores
- Ranking de aptitud

## Interpretación de Resultados

### Correlación Pobreza-Asistencia
- **Correlación positiva:** Mayor pobreza = Menor asistencia
- **Significancia estadística:** p-value < 0.05
- **Magnitud:** Coeficiente de correlación (0.6-0.8)

### Municipios Prioritarios
1. **Alta pobreza + Baja asistencia:** Prioridad crítica
2. **Alta población indígena:** Mayor vulnerabilidad
3. **Baja escolaridad:** Necesidad educativa alta

## Recomendaciones

### 1. Intervención Inmediata
- Municipios con prioridad crítica
- Programas de apoyo económico
- Mejora de infraestructura educativa

### 2. Prevención
- Municipios con alta prioridad
- Programas de retención escolar
- Apoyo a familias vulnerables

### 3. Monitoreo
- Seguimiento de indicadores
- Evaluación de programas
- Actualización de datos

## Uso del Script

### 1. Preparación
```r
# Instalar librerías (si no están instaladas)
install.packages(c("tidyverse", "caret", "rpart", "ggplot2"))
```

### 2. Ejecución
```r
# Cargar el script
source("analisis_servicio_social.R")
```

### 3. Resultados
- Análisis automático de datos
- Generación de visualizaciones
- Exportación de archivos CSV
- Resumen ejecutivo en consola

## Consideraciones Técnicas

### 1. Limpieza de Datos
- Conversión de "n.a." y "n.d." a NA
- Transformación de tipos de datos
- Manejo de valores faltantes

### 2. Modelado
- División 80/20 (entrenamiento/prueba)
- Árbol de decisión para clasificación
- Matriz de confusión para evaluación

### 3. Visualización
- Gráficos con ggplot2
- Temas minimalistas
- Colores informativos

## Conclusiones

Este análisis permite:

1. **Identificar** personas aptas para servicio social
2. **Priorizar** municipios con mayor necesidad
3. **Predecir** aptitud con modelo estadístico
4. **Visualizar** patrones y correlaciones
5. **Recomendar** intervenciones específicas

El enfoque en región, clasificación e índice económico asegura que los recursos se dirijan a quienes más los necesitan para evitar el abandono escolar. 