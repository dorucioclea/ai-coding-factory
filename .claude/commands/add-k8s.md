# /add-k8s - Add Kubernetes Configuration

Add Kubernetes manifests and Helm charts for deploying .NET applications.

## Usage
```
/add-k8s [options]
```

Options:
- `--helm` - Generate Helm chart (default: plain manifests)
- `--hpa` - Include Horizontal Pod Autoscaler
- `--ingress <nginx|traefik>` - Include Ingress configuration
- `--tls` - Include TLS/cert-manager configuration
- `--story <ACF-###>` - Link to story ID

## Instructions

When invoked:

### 1. Analyze Project Requirements

Determine:
- Application name and namespace
- Resource requirements (CPU, memory)
- Dependencies (database, cache, etc.)
- Scaling requirements

### 2. Generate Directory Structure

**Plain Manifests:**
```
k8s/
├── base/
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── configmap.yaml
│   └── secret.yaml
├── overlays/
│   ├── staging/
│   │   └── kustomization.yaml
│   └── production/
│       └── kustomization.yaml
└── kustomization.yaml
```

**Helm Chart:**
```
helm/{{ProjectName}}/
├── Chart.yaml
├── values.yaml
├── values-staging.yaml
├── values-production.yaml
└── templates/
    ├── _helpers.tpl
    ├── deployment.yaml
    ├── service.yaml
    ├── ingress.yaml
    ├── configmap.yaml
    ├── secret.yaml
    ├── hpa.yaml
    ├── pdb.yaml
    └── serviceaccount.yaml
```

### 3. Generate Deployment Manifest

**deployment.yaml**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ include "app.fullname" . }}
  labels:
    {{- include "app.labels" . | nindent 4 }}
spec:
  replicas: {{ .Values.replicaCount }}
  selector:
    matchLabels:
      {{- include "app.selectorLabels" . | nindent 6 }}
  template:
    metadata:
      labels:
        {{- include "app.selectorLabels" . | nindent 8 }}
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "8080"
        prometheus.io/path: "/metrics"
    spec:
      serviceAccountName: {{ include "app.serviceAccountName" . }}
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        fsGroup: 2000
        seccompProfile:
          type: RuntimeDefault
      containers:
        - name: {{ .Chart.Name }}
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag | default .Chart.AppVersion }}"
          imagePullPolicy: {{ .Values.image.pullPolicy }}
          ports:
            - name: http
              containerPort: 8080
              protocol: TCP
          env:
            - name: ASPNETCORE_ENVIRONMENT
              value: "{{ .Values.environment }}"
            - name: ASPNETCORE_URLS
              value: "http://+:8080"
            {{- range $key, $value := .Values.env }}
            - name: {{ $key }}
              value: {{ $value | quote }}
            {{- end }}
          envFrom:
            - configMapRef:
                name: {{ include "app.fullname" . }}-config
            - secretRef:
                name: {{ include "app.fullname" . }}-secrets
          resources:
            {{- toYaml .Values.resources | nindent 12 }}
          livenessProbe:
            httpGet:
              path: /health/live
              port: http
            initialDelaySeconds: {{ .Values.probes.liveness.initialDelaySeconds }}
            periodSeconds: {{ .Values.probes.liveness.periodSeconds }}
            timeoutSeconds: {{ .Values.probes.liveness.timeoutSeconds }}
            failureThreshold: {{ .Values.probes.liveness.failureThreshold }}
          readinessProbe:
            httpGet:
              path: /health/ready
              port: http
            initialDelaySeconds: {{ .Values.probes.readiness.initialDelaySeconds }}
            periodSeconds: {{ .Values.probes.readiness.periodSeconds }}
            timeoutSeconds: {{ .Values.probes.readiness.timeoutSeconds }}
            failureThreshold: {{ .Values.probes.readiness.failureThreshold }}
          securityContext:
            allowPrivilegeEscalation: false
            readOnlyRootFilesystem: true
            capabilities:
              drop:
                - ALL
          volumeMounts:
            - name: tmp
              mountPath: /tmp
      volumes:
        - name: tmp
          emptyDir: {}
      {{- with .Values.nodeSelector }}
      nodeSelector:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.affinity }}
      affinity:
        {{- toYaml . | nindent 8 }}
      {{- end }}
      {{- with .Values.tolerations }}
      tolerations:
        {{- toYaml . | nindent 8 }}
      {{- end }}
```

### 4. Generate Service Manifest

**service.yaml**:
```yaml
apiVersion: v1
kind: Service
metadata:
  name: {{ include "app.fullname" . }}
  labels:
    {{- include "app.labels" . | nindent 4 }}
spec:
  type: {{ .Values.service.type }}
  ports:
    - port: {{ .Values.service.port }}
      targetPort: http
      protocol: TCP
      name: http
  selector:
    {{- include "app.selectorLabels" . | nindent 4 }}
```

### 5. Generate Ingress Manifest (if --ingress)

**ingress.yaml**:
```yaml
{{- if .Values.ingress.enabled -}}
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: {{ include "app.fullname" . }}
  labels:
    {{- include "app.labels" . | nindent 4 }}
  annotations:
    {{- if eq .Values.ingress.className "nginx" }}
    kubernetes.io/ingress.class: nginx
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/proxy-body-size: "10m"
    nginx.ingress.kubernetes.io/proxy-read-timeout: "60"
    {{- end }}
    {{- if .Values.ingress.tls.enabled }}
    cert-manager.io/cluster-issuer: {{ .Values.ingress.tls.clusterIssuer }}
    {{- end }}
    {{- with .Values.ingress.annotations }}
    {{- toYaml . | nindent 4 }}
    {{- end }}
spec:
  ingressClassName: {{ .Values.ingress.className }}
  {{- if .Values.ingress.tls.enabled }}
  tls:
    - hosts:
        - {{ .Values.ingress.host }}
      secretName: {{ include "app.fullname" . }}-tls
  {{- end }}
  rules:
    - host: {{ .Values.ingress.host }}
      http:
        paths:
          - path: {{ .Values.ingress.path }}
            pathType: Prefix
            backend:
              service:
                name: {{ include "app.fullname" . }}
                port:
                  number: {{ .Values.service.port }}
{{- end }}
```

### 6. Generate HPA Manifest (if --hpa)

**hpa.yaml**:
```yaml
{{- if .Values.autoscaling.enabled }}
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: {{ include "app.fullname" . }}
  labels:
    {{- include "app.labels" . | nindent 4 }}
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: {{ include "app.fullname" . }}
  minReplicas: {{ .Values.autoscaling.minReplicas }}
  maxReplicas: {{ .Values.autoscaling.maxReplicas }}
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: {{ .Values.autoscaling.targetCPUUtilizationPercentage }}
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: {{ .Values.autoscaling.targetMemoryUtilizationPercentage }}
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
        - type: Percent
          value: 10
          periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
        - type: Percent
          value: 100
          periodSeconds: 15
        - type: Pods
          value: 4
          periodSeconds: 15
      selectPolicy: Max
{{- end }}
```

### 7. Generate Pod Disruption Budget

**pdb.yaml**:
```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: {{ include "app.fullname" . }}
  labels:
    {{- include "app.labels" . | nindent 4 }}
spec:
  minAvailable: {{ .Values.pdb.minAvailable }}
  selector:
    matchLabels:
      {{- include "app.selectorLabels" . | nindent 6 }}
```

### 8. Generate Values File

**values.yaml**:
```yaml
# Default values for {{ProjectName}}
replicaCount: 2

image:
  repository: ghcr.io/your-org/{{ProjectName}}
  pullPolicy: IfNotPresent
  tag: ""

environment: Production

service:
  type: ClusterIP
  port: 80

ingress:
  enabled: true
  className: nginx
  host: api.example.com
  path: /
  annotations: {}
  tls:
    enabled: true
    clusterIssuer: letsencrypt-prod

resources:
  limits:
    cpu: 500m
    memory: 512Mi
  requests:
    cpu: 100m
    memory: 256Mi

autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
  targetMemoryUtilizationPercentage: 80

pdb:
  minAvailable: 1

probes:
  liveness:
    initialDelaySeconds: 10
    periodSeconds: 15
    timeoutSeconds: 5
    failureThreshold: 3
  readiness:
    initialDelaySeconds: 5
    periodSeconds: 10
    timeoutSeconds: 5
    failureThreshold: 3

env: {}

nodeSelector: {}

tolerations: []

affinity: {}
```

**values-production.yaml**:
```yaml
replicaCount: 3

environment: Production

ingress:
  host: api.example.com

resources:
  limits:
    cpu: 1000m
    memory: 1Gi
  requests:
    cpu: 250m
    memory: 512Mi

autoscaling:
  minReplicas: 3
  maxReplicas: 20
```

## Security Best Practices

- [ ] Non-root user in containers
- [ ] Read-only root filesystem
- [ ] No privilege escalation
- [ ] Dropped ALL capabilities
- [ ] Resource limits set
- [ ] Network policies configured
- [ ] Secrets managed externally (Sealed Secrets or External Secrets)
- [ ] Pod Security Standards enforced

## Output

```markdown
## Kubernetes Configuration Created

### Files Created
**Helm Chart:**
- helm/{{ProjectName}}/Chart.yaml
- helm/{{ProjectName}}/values.yaml
- helm/{{ProjectName}}/values-staging.yaml
- helm/{{ProjectName}}/values-production.yaml
- helm/{{ProjectName}}/templates/*.yaml

### Deployment Commands
```bash
# Install to staging
helm install {{ProjectName}} ./helm/{{ProjectName}} \
  -f ./helm/{{ProjectName}}/values-staging.yaml \
  -n staging

# Upgrade production
helm upgrade {{ProjectName}} ./helm/{{ProjectName}} \
  -f ./helm/{{ProjectName}}/values-production.yaml \
  -n production

# Dry run
helm install {{ProjectName}} ./helm/{{ProjectName}} --dry-run --debug
```

### Prerequisites
- Kubernetes cluster 1.25+
- Helm 3.x
- NGINX Ingress Controller
- cert-manager (for TLS)
- External Secrets Operator (recommended)

### Next Steps
1. Update image repository in values.yaml
2. Configure ingress hostname
3. Set up secrets management
4. Configure network policies
```

## Example

```
User: /add-k8s --helm --hpa --ingress nginx --tls --story ACF-065

Claude: Creating Kubernetes configuration with Helm chart...

Generating Helm chart:
1. Chart.yaml with metadata
2. values.yaml (default configuration)
3. values-staging.yaml
4. values-production.yaml

Creating templates:
5. deployment.yaml (with security context)
6. service.yaml (ClusterIP)
7. ingress.yaml (NGINX with TLS)
8. hpa.yaml (CPU/Memory scaling)
9. pdb.yaml (Pod Disruption Budget)
10. configmap.yaml
11. secret.yaml

Kubernetes configuration created successfully.

Deploy with:
  helm install orderservice ./helm/orderservice -n staging
```
