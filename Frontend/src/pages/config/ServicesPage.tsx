import { useState, useMemo, useCallback } from "react";
import {
  Globe, Plus, Search, Pencil, Trash2, TestTube, CheckCircle2, XCircle,
  Eye, EyeOff, KeyRound, ShieldCheck, Lock, Unplug, Link2, ChevronDown,
} from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Separator } from "@/components/ui/separator";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import {
  ExternalService, ServiceAuthConfig, AuthType, ServiceType,
  authTypeLabels, serviceTypeLabels, mockServices,
} from "@/data/services";

const statusConfig: Record<string, { label: string; className: string }> = {
  active: { label: "Activo", className: "bg-emerald-500/10 text-emerald-600 border-emerald-200" },
  inactive: { label: "Inactivo", className: "bg-muted text-muted-foreground border-border" },
  error: { label: "Error", className: "bg-destructive/10 text-destructive border-destructive/20" },
};

const emptyAuth: ServiceAuthConfig = { type: "none" };

const emptyService: Omit<ExternalService, "id" | "createdAt" | "updatedAt"> = {
  name: "", description: "", type: "rest_api", baseUrl: "", status: "inactive",
  auth: { ...emptyAuth }, timeout: 30, retries: 3,
};

function generateId() {
  return `svc_${Date.now()}_${Math.random().toString(36).slice(2, 7)}`;
}

export default function ServicesPage() {
  const [services, setServices] = useState<ExternalService[]>(mockServices);
  const [search, setSearch] = useState("");
  const [filterType, setFilterType] = useState<string>("all");
  const [formOpen, setFormOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [editingService, setEditingService] = useState<ExternalService | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<ExternalService | null>(null);
  const [formData, setFormData] = useState(emptyService);
  const [showSecrets, setShowSecrets] = useState(false);

  const filtered = useMemo(() => {
    return services.filter(s => {
      const matchSearch = !search ||
        s.name.toLowerCase().includes(search.toLowerCase()) ||
        s.description.toLowerCase().includes(search.toLowerCase()) ||
        s.baseUrl.toLowerCase().includes(search.toLowerCase());
      const matchType = filterType === "all" || s.type === filterType;
      return matchSearch && matchType;
    });
  }, [services, search, filterType]);

  const openCreate = () => {
    setEditingService(null);
    setFormData({ ...emptyService, auth: { type: "none" } });
    setShowSecrets(false);
    setFormOpen(true);
  };

  const openEdit = (svc: ExternalService) => {
    setEditingService(svc);
    setFormData({
      name: svc.name, description: svc.description, type: svc.type,
      baseUrl: svc.baseUrl, status: svc.status, auth: { ...svc.auth },
      timeout: svc.timeout, retries: svc.retries,
    });
    setShowSecrets(false);
    setFormOpen(true);
  };

  const handleSave = () => {
    if (!formData.name || !formData.baseUrl) {
      toast.error("Nombre y URL base son obligatorios");
      return;
    }
    const now = new Date().toISOString().slice(0, 10);
    if (editingService) {
      setServices(prev => prev.map(s => s.id === editingService.id ? { ...s, ...formData, updatedAt: now } : s));
      toast.success("Servicio actualizado");
    } else {
      const newService: ExternalService = { ...formData, id: generateId(), createdAt: now, updatedAt: now };
      setServices(prev => [newService, ...prev]);
      toast.success("Servicio creado");
    }
    setFormOpen(false);
  };

  const handleDelete = () => {
    if (deleteTarget) {
      setServices(prev => prev.filter(s => s.id !== deleteTarget.id));
      toast.success("Servicio eliminado");
      setDeleteOpen(false);
      setDeleteTarget(null);
    }
  };

  const handleTest = (svc: ExternalService) => {
    const now = new Date().toISOString().slice(0, 10);
    const result = Math.random() > 0.3 ? "success" : "failure";
    setServices(prev => prev.map(s => s.id === svc.id ? { ...s, lastTestedAt: now, lastTestResult: result as 'success' | 'failure', status: result === "failure" ? "error" : s.status } : s));
    if (result === "success") toast.success(`Conexión a "${svc.name}" exitosa`);
    else toast.error(`Falló la conexión a "${svc.name}"`);
  };

  const updateAuth = (patch: Partial<ServiceAuthConfig>) => {
    setFormData(prev => ({ ...prev, auth: { ...prev.auth, ...patch } }));
  };

  const addCustomHeader = () => {
    const headers = [...(formData.auth.customHeaders || []), { key: "", value: "" }];
    updateAuth({ customHeaders: headers });
  };

  const updateCustomHeader = (idx: number, field: "key" | "value", val: string) => {
    const headers = [...(formData.auth.customHeaders || [])];
    headers[idx] = { ...headers[idx], [field]: val };
    updateAuth({ customHeaders: headers });
  };

  const removeCustomHeader = (idx: number) => {
    const headers = (formData.auth.customHeaders || []).filter((_, i) => i !== idx);
    updateAuth({ customHeaders: headers });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">Servicios Externos</h1>
          <p className="text-muted-foreground text-sm">Configuración de APIs y MCPs externos con autenticación</p>
        </div>
        <Button onClick={openCreate} className="gap-2">
          <Plus className="h-4 w-4" /> Nuevo Servicio
        </Button>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3 flex-wrap">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input placeholder="Buscar servicios..." value={search} onChange={e => setSearch(e.target.value)} className="pl-9" />
        </div>
        <Select value={filterType} onValueChange={setFilterType}>
          <SelectTrigger className="w-44"><SelectValue placeholder="Tipo" /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todos los tipos</SelectItem>
            {Object.entries(serviceTypeLabels).map(([k, v]) => (
              <SelectItem key={k} value={k}>{v}</SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Badge variant="outline" className="text-xs">{filtered.length} servicio{filtered.length !== 1 ? "s" : ""}</Badge>
      </div>

      {/* Service Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        {filtered.map(svc => {
          const st = statusConfig[svc.status];
          return (
            <Card key={svc.id} className="group hover:shadow-md transition-shadow">
              <CardHeader className="pb-3">
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-2.5 min-w-0">
                    <div className={cn("h-9 w-9 rounded-lg flex items-center justify-center shrink-0", svc.type === "mcp" ? "bg-accent/10 text-accent" : "bg-primary/10 text-primary")}>
                      {svc.type === "mcp" ? <Unplug className="h-4 w-4" /> : <Globe className="h-4 w-4" />}
                    </div>
                    <div className="min-w-0">
                      <CardTitle className="text-sm truncate">{svc.name}</CardTitle>
                      <Badge variant="outline" className="text-[10px] mt-0.5">{serviceTypeLabels[svc.type]}</Badge>
                    </div>
                  </div>
                  <Badge className={cn("text-[10px] shrink-0", st.className)}>{st.label}</Badge>
                </div>
              </CardHeader>
              <CardContent className="space-y-3">
                <p className="text-xs text-muted-foreground line-clamp-2">{svc.description}</p>

                <div className="space-y-1.5">
                  <div className="flex items-center gap-1.5 text-[11px]">
                    <Link2 className="h-3 w-3 text-muted-foreground" />
                    <code className="text-muted-foreground truncate block max-w-[200px]">{svc.baseUrl}</code>
                  </div>
                  <div className="flex items-center gap-1.5 text-[11px]">
                    <KeyRound className="h-3 w-3 text-muted-foreground" />
                    <span className="text-muted-foreground">{authTypeLabels[svc.auth.type]}</span>
                  </div>
                  {svc.lastTestedAt && (
                    <div className="flex items-center gap-1.5 text-[11px]">
                      {svc.lastTestResult === "success" ? <CheckCircle2 className="h-3 w-3 text-primary" /> : <XCircle className="h-3 w-3 text-destructive" />}
                      <span className="text-muted-foreground">Test: {svc.lastTestedAt}</span>
                    </div>
                  )}
                </div>

                <Separator />

                <div className="flex items-center gap-1.5">
                  <Button variant="ghost" size="sm" className="h-7 text-xs gap-1" onClick={() => handleTest(svc)}>
                    <TestTube className="h-3 w-3" /> Probar
                  </Button>
                  <Button variant="ghost" size="sm" className="h-7 text-xs gap-1" onClick={() => openEdit(svc)}>
                    <Pencil className="h-3 w-3" /> Editar
                  </Button>
                  <Button variant="ghost" size="sm" className="h-7 text-xs gap-1 text-destructive hover:text-destructive" onClick={() => { setDeleteTarget(svc); setDeleteOpen(true); }}>
                    <Trash2 className="h-3 w-3" /> Eliminar
                  </Button>
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {filtered.length === 0 && (
        <div className="text-center py-12 text-muted-foreground text-sm">No se encontraron servicios</div>
      )}

      {/* Form Dialog */}
      <Dialog open={formOpen} onOpenChange={setFormOpen}>
        <DialogContent className="max-w-2xl max-h-[85vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{editingService ? "Editar Servicio" : "Nuevo Servicio"}</DialogTitle>
          </DialogHeader>

          <Tabs defaultValue="general" className="mt-2">
            <TabsList className="grid grid-cols-3 w-full">
              <TabsTrigger value="general">General</TabsTrigger>
              <TabsTrigger value="auth">Autenticación</TabsTrigger>
              <TabsTrigger value="advanced">Avanzado</TabsTrigger>
            </TabsList>

            {/* General Tab */}
            <TabsContent value="general" className="space-y-4 mt-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Nombre *</Label>
                  <Input value={formData.name} onChange={e => setFormData(p => ({ ...p, name: e.target.value }))} placeholder="Ej: Servicio de Scoring" />
                </div>
                <div className="space-y-2">
                  <Label>Tipo</Label>
                  <Select value={formData.type} onValueChange={v => setFormData(p => ({ ...p, type: v as ServiceType }))}>
                    <SelectTrigger><SelectValue /></SelectTrigger>
                    <SelectContent>
                      {Object.entries(serviceTypeLabels).map(([k, v]) => (
                        <SelectItem key={k} value={k}>{v}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div className="space-y-2">
                <Label>URL Base *</Label>
                <Input value={formData.baseUrl} onChange={e => setFormData(p => ({ ...p, baseUrl: e.target.value }))} placeholder="https://api.example.com/v1" />
              </div>
              <div className="space-y-2">
                <Label>Descripción</Label>
                <Textarea value={formData.description} onChange={e => setFormData(p => ({ ...p, description: e.target.value }))} rows={3} placeholder="Descripción del servicio externo..." />
              </div>
              <div className="flex items-center gap-3">
                <Label>Estado</Label>
                <div className="flex items-center gap-2">
                  <Switch checked={formData.status === "active"} onCheckedChange={c => setFormData(p => ({ ...p, status: c ? "active" : "inactive" }))} />
                  <span className="text-sm text-muted-foreground">{formData.status === "active" ? "Activo" : "Inactivo"}</span>
                </div>
              </div>
            </TabsContent>

            {/* Auth Tab */}
            <TabsContent value="auth" className="space-y-4 mt-4">
              <div className="space-y-2">
                <Label>Método de autenticación</Label>
                <Select value={formData.auth.type} onValueChange={v => setFormData(p => ({ ...p, auth: { type: v as AuthType } }))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {Object.entries(authTypeLabels).map(([k, v]) => (
                      <SelectItem key={k} value={k}>{v}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <Separator />

              <div className="flex items-center justify-end gap-2">
                <Button variant="ghost" size="sm" className="text-xs gap-1" onClick={() => setShowSecrets(!showSecrets)}>
                  {showSecrets ? <EyeOff className="h-3 w-3" /> : <Eye className="h-3 w-3" />}
                  {showSecrets ? "Ocultar" : "Mostrar"} secretos
                </Button>
              </div>

              {/* API Key */}
              {formData.auth.type === "api_key" && (
                <div className="space-y-4 p-4 rounded-lg border border-border bg-muted/30">
                  <div className="flex items-center gap-2 text-sm font-medium text-foreground">
                    <KeyRound className="h-4 w-4" /> Configuración API Key
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label className="text-xs">Nombre del Header/Parámetro</Label>
                      <Input value={formData.auth.apiKeyHeader || ""} onChange={e => updateAuth({ apiKeyHeader: e.target.value })} placeholder="X-API-Key" />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-xs">Ubicación</Label>
                      <Select value={formData.auth.apiKeyLocation || "header"} onValueChange={v => updateAuth({ apiKeyLocation: v as 'header' | 'query' })}>
                        <SelectTrigger><SelectValue /></SelectTrigger>
                        <SelectContent>
                          <SelectItem value="header">Header</SelectItem>
                          <SelectItem value="query">Query Parameter</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs">Valor</Label>
                    <Input type={showSecrets ? "text" : "password"} value={formData.auth.apiKeyValue || ""} onChange={e => updateAuth({ apiKeyValue: e.target.value })} placeholder="sk_live_..." />
                  </div>
                </div>
              )}

              {/* Bearer Token */}
              {formData.auth.type === "bearer_token" && (
                <div className="space-y-4 p-4 rounded-lg border border-border bg-muted/30">
                  <div className="flex items-center gap-2 text-sm font-medium text-foreground">
                    <ShieldCheck className="h-4 w-4" /> Configuración Bearer Token
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs">Token</Label>
                    <Input type={showSecrets ? "text" : "password"} value={formData.auth.bearerToken || ""} onChange={e => updateAuth({ bearerToken: e.target.value })} placeholder="eyJhbGciOiJIUzI1NiJ9..." />
                  </div>
                </div>
              )}

              {/* Basic Auth */}
              {formData.auth.type === "basic_auth" && (
                <div className="space-y-4 p-4 rounded-lg border border-border bg-muted/30">
                  <div className="flex items-center gap-2 text-sm font-medium text-foreground">
                    <Lock className="h-4 w-4" /> Configuración Basic Auth
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label className="text-xs">Usuario</Label>
                      <Input value={formData.auth.basicUser || ""} onChange={e => updateAuth({ basicUser: e.target.value })} placeholder="usuario" />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-xs">Contraseña</Label>
                      <Input type={showSecrets ? "text" : "password"} value={formData.auth.basicPassword || ""} onChange={e => updateAuth({ basicPassword: e.target.value })} placeholder="••••••••" />
                    </div>
                  </div>
                </div>
              )}

              {/* OAuth 2.0 */}
              {formData.auth.type === "oauth2" && (
                <div className="space-y-4 p-4 rounded-lg border border-border bg-muted/30">
                  <div className="flex items-center gap-2 text-sm font-medium text-foreground">
                    <ShieldCheck className="h-4 w-4" /> Configuración OAuth 2.0
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs">Grant Type</Label>
                    <Select value={formData.auth.oauth2GrantType || "client_credentials"} onValueChange={v => updateAuth({ oauth2GrantType: v as 'client_credentials' | 'authorization_code' })}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="client_credentials">Client Credentials</SelectItem>
                        <SelectItem value="authorization_code">Authorization Code</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label className="text-xs">Client ID</Label>
                      <Input value={formData.auth.oauth2ClientId || ""} onChange={e => updateAuth({ oauth2ClientId: e.target.value })} placeholder="client_abc123" />
                    </div>
                    <div className="space-y-2">
                      <Label className="text-xs">Client Secret</Label>
                      <Input type={showSecrets ? "text" : "password"} value={formData.auth.oauth2ClientSecret || ""} onChange={e => updateAuth({ oauth2ClientSecret: e.target.value })} placeholder="••••••••" />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs">Token URL</Label>
                    <Input value={formData.auth.oauth2TokenUrl || ""} onChange={e => updateAuth({ oauth2TokenUrl: e.target.value })} placeholder="https://auth.example.com/oauth/token" />
                  </div>
                  <div className="space-y-2">
                    <Label className="text-xs">Scopes</Label>
                    <Input value={formData.auth.oauth2Scopes || ""} onChange={e => updateAuth({ oauth2Scopes: e.target.value })} placeholder="read write admin" />
                  </div>
                </div>
              )}

              {/* Custom Headers */}
              {formData.auth.type === "custom_header" && (
                <div className="space-y-4 p-4 rounded-lg border border-border bg-muted/30">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2 text-sm font-medium text-foreground">
                      <KeyRound className="h-4 w-4" /> Headers personalizados
                    </div>
                    <Button variant="outline" size="sm" className="text-xs gap-1" onClick={addCustomHeader}>
                      <Plus className="h-3 w-3" /> Agregar
                    </Button>
                  </div>
                  {(formData.auth.customHeaders || []).map((h, idx) => (
                    <div key={idx} className="flex items-center gap-2">
                      <Input value={h.key} onChange={e => updateCustomHeader(idx, "key", e.target.value)} placeholder="Header name" className="flex-1" />
                      <Input type={showSecrets ? "text" : "password"} value={h.value} onChange={e => updateCustomHeader(idx, "value", e.target.value)} placeholder="Valor" className="flex-1" />
                      <Button variant="ghost" size="icon" className="h-8 w-8 shrink-0 text-destructive" onClick={() => removeCustomHeader(idx)}>
                        <Trash2 className="h-3.5 w-3.5" />
                      </Button>
                    </div>
                  ))}
                  {(!formData.auth.customHeaders || formData.auth.customHeaders.length === 0) && (
                    <p className="text-xs text-muted-foreground text-center py-2">Sin headers configurados</p>
                  )}
                </div>
              )}

              {formData.auth.type === "none" && (
                <div className="p-4 rounded-lg border border-border bg-muted/30 text-center text-sm text-muted-foreground">
                  Este servicio no requiere autenticación
                </div>
              )}
            </TabsContent>

            {/* Advanced Tab */}
            <TabsContent value="advanced" className="space-y-4 mt-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Timeout (segundos)</Label>
                  <Input type="number" value={formData.timeout} onChange={e => setFormData(p => ({ ...p, timeout: parseInt(e.target.value) || 30 }))} />
                </div>
                <div className="space-y-2">
                  <Label>Reintentos</Label>
                  <Input type="number" value={formData.retries} onChange={e => setFormData(p => ({ ...p, retries: parseInt(e.target.value) || 0 }))} />
                </div>
              </div>
            </TabsContent>
          </Tabs>

          <DialogFooter className="mt-4">
            <Button variant="outline" onClick={() => setFormOpen(false)}>Cancelar</Button>
            <Button onClick={handleSave}>{editingService ? "Guardar cambios" : "Crear servicio"}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <DeleteConfirmDialog
        open={deleteOpen}
        onOpenChange={setDeleteOpen}
        onConfirm={handleDelete}
        title="Eliminar servicio"
        description={`¿Estás seguro de eliminar "${deleteTarget?.name}"? Esta acción no se puede deshacer.`}
      />
    </div>
  );
}
