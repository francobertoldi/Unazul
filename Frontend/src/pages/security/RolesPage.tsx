import { useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Shield, Users, Plus, Pencil, Trash2, ChevronDown, Search } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { useRoleStore, mockPermissions } from "@/data/store";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { Role } from "@/data/types";
import { toast } from "sonner";

export default function RolesPage() {
  const navigate = useNavigate();
  const { roles, deleteRole } = useRoleStore();
  const [selectedRole, setSelectedRole] = useState(roles[0]?.id || "");
  const [deleteTarget, setDeleteTarget] = useState<Role | null>(null);
  const [roleSearch, setRoleSearch] = useState("");
  const [permSearch, setPermSearch] = useState("");

  const role = roles.find(r => r.id === selectedRole);
  const modules = [...new Set(mockPermissions.map(p => p.module))];

  const filteredRoles = useMemo(() => {
    if (!roleSearch.trim()) return roles;
    const q = roleSearch.toLowerCase();
    return roles.filter(r => r.name.toLowerCase().includes(q) || r.description.toLowerCase().includes(q));
  }, [roles, roleSearch]);

  const filteredModules = useMemo(() => {
    if (!permSearch.trim()) return modules;
    const q = permSearch.toLowerCase();
    return modules.filter(mod => {
      if (mod.toLowerCase().includes(q)) return true;
      return mockPermissions.some(p => p.module === mod && (p.action.toLowerCase().includes(q) || p.description.toLowerCase().includes(q)));
    });
  }, [modules, permSearch]);

  const getFilteredPerms = (mod: string) => {
    const perms = mockPermissions.filter(p => p.module === mod);
    if (!permSearch.trim()) return perms;
    const q = permSearch.toLowerCase();
    if (mod.toLowerCase().includes(q)) return perms;
    return perms.filter(p => p.action.toLowerCase().includes(q) || p.description.toLowerCase().includes(q));
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div><h1 className="text-2xl font-bold tracking-tight">Roles y Permisos</h1><p className="text-muted-foreground text-sm mt-1">Administración de roles y permisos del sistema</p></div>
        <Button className="gap-2" onClick={() => navigate("/roles/nuevo")}><Plus className="h-4 w-4" /> Nuevo Rol</Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="space-y-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input placeholder="Buscar rol..." value={roleSearch} onChange={e => setRoleSearch(e.target.value)} className="pl-9" />
          </div>
          <div className="space-y-2">
            {filteredRoles.map(r => (
              <Card key={r.id} className={`cursor-pointer transition-all ${r.id === selectedRole ? "ring-2 ring-primary shadow-md" : "hover:shadow-sm"}`} onClick={() => setSelectedRole(r.id)}>
                <CardContent className="p-4">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center"><Shield className="h-4 w-4 text-primary" /></div>
                      <div><p className="font-medium text-sm">{r.name}</p><p className="text-xs text-muted-foreground">{r.description}</p></div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="secondary" className="text-xs gap-1"><Users className="h-3 w-3" />{r.userCount}</Badge>
                      <div className="flex" onClick={ev => ev.stopPropagation()}>
                        <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => navigate(`/roles/${r.id}/editar`)}><Pencil className="h-3 w-3" /></Button>
                        <Button variant="ghost" size="icon" className="h-7 w-7 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(r)}><Trash2 className="h-3 w-3" /></Button>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
            {filteredRoles.length === 0 && <p className="text-sm text-muted-foreground text-center py-4">No se encontraron roles</p>}
          </div>
        </div>

        <div className="lg:col-span-2">
          {role ? (
            <Card>
              <CardHeader className="pb-3">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-base flex items-center gap-2">Permisos de: <Badge>{role.name}</Badge></CardTitle>
                </div>
                <div className="relative mt-2">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input placeholder="Buscar permiso..." value={permSearch} onChange={e => setPermSearch(e.target.value)} className="pl-9" />
                </div>
              </CardHeader>
              <CardContent className="space-y-3">
                {filteredModules.map(mod => {
                  const perms = getFilteredPerms(mod);
                  if (perms.length === 0) return null;
                  return (
                    <Collapsible key={mod} defaultOpen>
                      <CollapsibleTrigger className="flex items-center gap-2 w-full text-sm font-semibold py-2 hover:text-primary transition-colors">
                        <ChevronDown className="h-4 w-4" />{mod}
                        <span className="text-xs text-muted-foreground font-normal">({perms.filter(p => role.permissions.includes(p.id)).length}/{perms.length})</span>
                      </CollapsibleTrigger>
                      <CollapsibleContent>
                        <div className="ml-6 space-y-2 pb-2">
                          {perms.map(p => (
                            <label key={p.id} className="flex items-center gap-3 cursor-pointer text-sm">
                              <Checkbox checked={role.permissions.includes(p.id)} disabled />
                              <div><span className="font-medium">{p.action}</span><span className="text-muted-foreground ml-2">— {p.description}</span></div>
                            </label>
                          ))}
                        </div>
                      </CollapsibleContent>
                    </Collapsible>
                  );
                })}
                {filteredModules.length === 0 && <p className="text-sm text-muted-foreground text-center py-4">No se encontraron permisos</p>}
              </CardContent>
            </Card>
          ) : (
            <div className="flex items-center justify-center h-40 text-muted-foreground">Seleccione un rol para ver sus permisos</div>
          )}
        </div>
      </div>

      <DeleteConfirmDialog open={!!deleteTarget} onOpenChange={o => !o && setDeleteTarget(null)} title="Eliminar Rol"
        description={`¿Está seguro de eliminar el rol "${deleteTarget?.name}"?`}
        onConfirm={() => { if (deleteTarget) { deleteRole(deleteTarget.id); if (selectedRole === deleteTarget.id) setSelectedRole(roles[0]?.id || ""); toast.success("Rol eliminado"); setDeleteTarget(null); } }} />
    </div>
  );
}
