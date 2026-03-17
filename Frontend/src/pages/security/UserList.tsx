import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, User as UserIcon, Pencil, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { DataTable } from "@/components/shared/DataTable";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { useUserStore, useEntityStore } from "@/data/store";
import { User } from "@/data/types";
import { Badge } from "@/components/ui/badge";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { toast } from "sonner";

const statusOptions = [
  { value: "active", label: "Activo" },
  { value: "inactive", label: "Inactivo" },
  { value: "locked", label: "Bloqueado" },
];

export default function UserList() {
  const navigate = useNavigate();
  const { users, deleteUser } = useUserStore();
  const { entities } = useEntityStore();
  const [deleteTarget, setDeleteTarget] = useState<User | null>(null);

  const entityOptions = entities.map(e => ({ value: e.name, label: e.name }));

  const columns = [
    {
      key: "firstName", header: "Usuario", searchable: true, filterType: "text" as const, sortable: true,
      filterValue: (u: User) => `${u.firstName} ${u.lastName} ${u.email}`,
      sortValue: (u: User) => `${u.lastName} ${u.firstName}`,
      render: (u: User) => (
        <div className="flex items-center gap-3">
          <div className="h-9 w-9 rounded-full bg-primary/10 flex items-center justify-center shrink-0"><UserIcon className="h-4 w-4 text-primary" /></div>
          <div><p className="font-medium text-sm">{u.firstName} {u.lastName}</p><p className="text-xs text-muted-foreground">{u.email}</p></div>
        </div>
      ),
    },
    { key: "entityName", header: "Entidad", searchable: true, filterType: "multiselect" as const, filterOptions: entityOptions, sortable: true, render: (u: User) => <span className="text-sm">{u.entityName}</span> },
    {
      key: "roleNames", header: "Roles", searchable: true, filterType: "text" as const, sortable: true,
      filterValue: (u: User) => u.roleNames.join(", "),
      sortValue: (u: User) => u.roleNames.join(", "),
      render: (u: User) => (
        <div className="flex gap-1 flex-wrap">
          {u.roleNames.map(name => <Badge key={name} variant="secondary" className="text-xs">{name}</Badge>)}
        </div>
      ),
      exportValue: (u: User) => u.roleNames.join(", "),
    },
    {
      key: "lastLogin", header: "Último Acceso", searchable: false, filterType: "date" as const, sortable: true,
      render: (u: User) => <span className="text-sm text-muted-foreground">{new Date(u.lastLogin).toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" })}</span>,
    },
    { key: "status", header: "Estado", searchable: false, filterType: "select" as const, filterOptions: statusOptions, sortable: true, render: (u: User) => <StatusBadge status={u.status} /> },
    {
      key: "_actions", header: "", searchable: false,
      render: (u: User) => (
        <div className="flex items-center gap-1" onClick={ev => ev.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/usuarios/${u.id}/editar`)}><Pencil className="h-3.5 w-3.5" /></Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteTarget(u)}><Trash2 className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div><h1 className="text-2xl font-bold tracking-tight">Usuarios</h1><p className="text-muted-foreground text-sm mt-1">Gestión de usuarios de la plataforma</p></div>
      <DataTable data={users} columns={columns} searchPlaceholder="Buscar usuario..." exportFileName="usuarios" onRowClick={(u) => navigate(`/usuarios/${u.id}`)}
        actions={<Button className="gap-2" onClick={() => navigate("/usuarios/nuevo")}><Plus className="h-4 w-4" />Nuevo Usuario</Button>} />
      <DeleteConfirmDialog open={!!deleteTarget} onOpenChange={o => !o && setDeleteTarget(null)} title="Eliminar Usuario"
        description={`¿Está seguro de eliminar al usuario "${deleteTarget?.firstName} ${deleteTarget?.lastName}"?`}
        onConfirm={() => { if (deleteTarget) { deleteUser(deleteTarget.id); toast.success("Usuario eliminado"); setDeleteTarget(null); } }} />
    </div>
  );
}
