import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, User as UserIcon, Mail, Building2, Shield, Clock, Calendar, Pencil, Trash2, KeyRound } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { Badge } from "@/components/ui/badge";
import { useUserStore, useRoleStore } from "@/data/store";
import { mockPermissions } from "@/data/mock";
import { getMergedPermissions } from "@/data/types";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { toast } from "sonner";

export default function UserDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { users, deleteUser } = useUserStore();
  const { roles } = useRoleStore();
  const user = users.find(u => u.id === id);
  const [deleteOpen, setDeleteOpen] = useState(false);

  if (!user) {
    return <div className="flex items-center justify-center h-64"><p className="text-muted-foreground">Usuario no encontrado</p></div>;
  }

  const mergedPermissions = getMergedPermissions(user.roleIds, roles);
  const modules = [...new Set(mockPermissions.map(p => p.module))];

  const infoItems = [
    { icon: KeyRound, label: "Usuario", value: user.username },
    { icon: Mail, label: "Email", value: user.email },
    { icon: Building2, label: "Entidad", value: user.entityName },
    { icon: Clock, label: "Último Acceso", value: new Date(user.lastLogin).toLocaleString("es-AR") },
    { icon: Calendar, label: "Creado", value: new Date(user.createdAt).toLocaleDateString("es-AR") },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/usuarios")}><ArrowLeft className="h-4 w-4" /></Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center"><UserIcon className="h-5 w-5 text-primary" /></div>
            <div><h1 className="text-xl font-bold tracking-tight">{user.firstName} {user.lastName}</h1><p className="text-sm text-muted-foreground">{user.email}</p></div>
            <StatusBadge status={user.status} />
            {user.roleNames.map(name => <Badge key={name} variant="secondary">{name}</Badge>)}
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" className="gap-2" onClick={() => navigate(`/usuarios/${id}/editar`)}><Pencil className="h-4 w-4" /> Editar</Button>
          <Button variant="outline" className="gap-2 text-destructive hover:text-destructive" onClick={() => setDeleteOpen(true)}><Trash2 className="h-4 w-4" /> Eliminar</Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Información del Usuario</CardTitle></CardHeader>
          <CardContent>
            <div className="space-y-4">
              {infoItems.map(item => (
                <div key={item.label} className="flex items-start gap-3">
                  <div className="h-9 w-9 rounded-lg bg-secondary flex items-center justify-center shrink-0"><item.icon className="h-4 w-4 text-muted-foreground" /></div>
                  <div><p className="text-xs text-muted-foreground">{item.label}</p><p className="text-sm font-medium">{item.value}</p></div>
                </div>
              ))}
              <div className="flex items-start gap-3">
                <div className="h-9 w-9 rounded-lg bg-secondary flex items-center justify-center shrink-0"><Shield className="h-4 w-4 text-muted-foreground" /></div>
                <div>
                  <p className="text-xs text-muted-foreground">Roles</p>
                  <div className="flex gap-1 flex-wrap mt-0.5">{user.roleNames.map(name => <Badge key={name} variant="secondary" className="text-xs">{name}</Badge>)}</div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              Permisos Efectivos <Badge variant="outline" className="text-xs">{mergedPermissions.length}</Badge>
            </CardTitle>
            {user.roleIds.length > 1 && (
              <p className="text-xs text-muted-foreground">Unión de: {user.roleNames.join(", ")}</p>
            )}
          </CardHeader>
          <CardContent className="space-y-3">
            {modules.map(mod => {
              const perms = mockPermissions.filter(p => p.module === mod);
              const activePerms = perms.filter(p => mergedPermissions.includes(p.id));
              if (activePerms.length === 0) return null;
              return (
                <div key={mod}>
                  <p className="text-sm font-semibold mb-1">{mod}</p>
                  <div className="flex flex-wrap gap-1.5 ml-2">
                    {perms.map(p => (
                      <Badge key={p.id} variant={mergedPermissions.includes(p.id) ? "default" : "outline"} className={`text-xs ${mergedPermissions.includes(p.id) ? "" : "opacity-30"}`}>
                        {p.action}
                      </Badge>
                    ))}
                  </div>
                </div>
              );
            })}
          </CardContent>
        </Card>
      </div>

      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} title="Eliminar Usuario"
        description={`¿Está seguro de eliminar al usuario "${user.firstName} ${user.lastName}"?`}
        onConfirm={() => { deleteUser(user.id); toast.success("Usuario eliminado"); navigate("/usuarios"); }} />
    </div>
  );
}
