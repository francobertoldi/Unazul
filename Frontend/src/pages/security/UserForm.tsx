import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft, Building2, Building, MapPin } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import { useUserStore, useEntityStore, useRoleStore, useTenantStore } from "@/data/store";
import { getMergedPermissions, UserAssignment } from "@/data/types";
import { mockPermissions } from "@/data/mock";
import { mockParameters } from "@/data/parameters";
import { toast } from "sonner";
import { useState } from "react";

// Get masks from parameters
const usernameMask = mockParameters.find(p => p.key === 'mask.username');
const passwordMask = mockParameters.find(p => p.key === 'mask.password');

const usernameRegex = usernameMask ? new RegExp(usernameMask.value) : /^[a-zA-Z0-9._-]{3,30}$/;
const passwordRegex = passwordMask ? new RegExp(passwordMask.value) : /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,50}$/;

const schema = z.object({
  username: z.string().trim().min(3, "Mínimo 3 caracteres").max(30, "Máximo 30 caracteres")
    .regex(usernameRegex, usernameMask?.description || "Solo letras, números, puntos, guiones"),
  password: z.string().min(8, "Mínimo 8 caracteres").max(50)
    .regex(passwordRegex, passwordMask?.description || "Debe contener mayúscula, minúscula, número y carácter especial"),
  firstName: z.string().trim().min(2, "Mínimo 2 caracteres").max(50),
  lastName: z.string().trim().min(2, "Mínimo 2 caracteres").max(50),
  email: z.string().trim().email("Email inválido").max(255),
  entityId: z.string().min(1, "Seleccione una entidad"),
  roleIds: z.array(z.string()).min(1, "Seleccione al menos un rol"),
  status: z.enum(["active", "inactive", "locked"]),
});

type FormData = {
  username: string;
  password: string;
  firstName: string;
  lastName: string;
  email: string;
  entityId: string;
  roleIds: string[];
  status: "active" | "inactive" | "locked";
};

export default function UserForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { users, addUser, updateUser } = useUserStore();
  const { entities } = useEntityStore();
  const { roles } = useRoleStore();
  const { tenants } = useTenantStore();
  const user = id ? users.find(u => u.id === id) : null;
  const isEdit = !!user;

  const entityOptions = [{ id: "_platform", name: "Plataforma" }, ...entities];

  // Hierarchical assignments state
  const [assignments, setAssignments] = useState<UserAssignment>(
    user?.assignments || { organizationIds: [], entityIds: [], branchIds: [] }
  );

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: user
      ? { username: user.username, password: user.password, firstName: user.firstName, lastName: user.lastName, email: user.email, entityId: user.entityId || "_platform", roleIds: user.roleIds, status: user.status }
      : { username: "", password: "", firstName: "", lastName: "", email: "", entityId: "", roleIds: [], status: "active" },
  });

  const selectedRoleIds = watch("roleIds") || [];
  const mergedPermissions = getMergedPermissions(selectedRoleIds, roles);
  const modules = [...new Set(mockPermissions.map(p => p.module))];

  // Get all branches from all entities
  const allBranches = entities.flatMap(e => e.branches.map(b => ({ ...b, entityName: e.name })));

  // Filter entities by selected organizations
  const filteredEntities = assignments.organizationIds.length > 0
    ? entities.filter(e => assignments.organizationIds.includes(e.tenantId))
    : entities;

  // Filter branches by selected entities
  const filteredBranches = assignments.entityIds.length > 0
    ? allBranches.filter(b => assignments.entityIds.includes(b.entityId))
    : assignments.organizationIds.length > 0
      ? allBranches.filter(b => filteredEntities.some(e => e.id === b.entityId))
      : allBranches;

  const toggleOrganization = (tenantId: string) => {
    const updated = assignments.organizationIds.includes(tenantId)
      ? assignments.organizationIds.filter(id => id !== tenantId)
      : [...assignments.organizationIds, tenantId];
    // Clean up child selections when removing org
    const validEntityIds = updated.length > 0
      ? assignments.entityIds.filter(eid => entities.find(e => e.id === eid && updated.includes(e.tenantId)))
      : assignments.entityIds;
    const validBranchIds = validEntityIds.length > 0
      ? assignments.branchIds.filter(bid => allBranches.find(b => b.id === bid && validEntityIds.includes(b.entityId)))
      : assignments.branchIds;
    setAssignments({ organizationIds: updated, entityIds: validEntityIds, branchIds: validBranchIds });
  };

  const toggleEntity = (entityId: string) => {
    const updated = assignments.entityIds.includes(entityId)
      ? assignments.entityIds.filter(id => id !== entityId)
      : [...assignments.entityIds, entityId];
    const validBranchIds = updated.length > 0
      ? assignments.branchIds.filter(bid => allBranches.find(b => b.id === bid && updated.includes(b.entityId)))
      : assignments.branchIds;
    setAssignments({ ...assignments, entityIds: updated, branchIds: validBranchIds });
  };

  const toggleBranch = (branchId: string) => {
    const updated = assignments.branchIds.includes(branchId)
      ? assignments.branchIds.filter(id => id !== branchId)
      : [...assignments.branchIds, branchId];
    setAssignments({ ...assignments, branchIds: updated });
  };

  const totalAssignments = assignments.organizationIds.length + assignments.entityIds.length + assignments.branchIds.length;

  const onSubmit = (data: FormData) => {
    const entityName = entityOptions.find(e => e.id === data.entityId)?.name || "Plataforma";
    const roleNames = roles.filter(r => data.roleIds.includes(r.id)).map(r => r.name);
    const entityId = data.entityId === "_platform" ? "" : data.entityId;

    if (isEdit) {
      updateUser(user!.id, { ...data, entityId, entityName, roleNames });
      toast.success("Usuario actualizado");
      navigate(`/usuarios/${user!.id}`);
    } else {
      const newUser = addUser({ ...data, entityId, entityName, roleNames });
      toast.success("Usuario creado");
      navigate(`/usuarios/${newUser.id}`);
    }
  };

  const toggleRole = (roleId: string) => {
    const updated = selectedRoleIds.includes(roleId)
      ? selectedRoleIds.filter(r => r !== roleId)
      : [...selectedRoleIds, roleId];
    setValue("roleIds", updated, { shouldValidate: true });
  };

  return (
    <div className="space-y-6 max-w-4xl">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate(isEdit ? `/usuarios/${id}` : "/usuarios")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">{isEdit ? "Editar Usuario" : "Nuevo Usuario"}</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Autenticación</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Usuario * <span className="text-xs text-muted-foreground font-normal">({usernameMask?.description || "letras, números, puntos, guiones"})</span></Label>
                <Input {...register("username")} placeholder="nombre.usuario" />
                {errors.username && <p className="text-xs text-destructive">{errors.username.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Contraseña * <span className="text-xs text-muted-foreground font-normal">({passwordMask?.description || "8+ caracteres, mayúscula, número, especial"})</span></Label>
                <Input {...register("password")} type="password" placeholder="Mínimo 8 caracteres" />
                {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Datos Personales</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Nombre *</Label>
                <Input {...register("firstName")} />
                {errors.firstName && <p className="text-xs text-destructive">{errors.firstName.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Apellido *</Label>
                <Input {...register("lastName")} />
                {errors.lastName && <p className="text-xs text-destructive">{errors.lastName.message}</p>}
              </div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Email *</Label>
                <Input {...register("email")} type="email" />
                {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Entidad Principal *</Label>
                <Select defaultValue={user?.entityId ? user.entityId : "_platform"} onValueChange={v => setValue("entityId", v, { shouldValidate: true })}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger>
                  <SelectContent>
                    {entityOptions.map(e => (
                      <SelectItem key={e.id} value={e.id}>{e.name}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.entityId && <p className="text-xs text-destructive">{errors.entityId.message}</p>}
              </div>
            </div>
            <div className="space-y-1.5">
              <Label>Estado *</Label>
              <Select defaultValue={user?.status || "active"} onValueChange={v => setValue("status", v as any)}>
                <SelectTrigger className="w-48"><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="active">Activo</SelectItem>
                  <SelectItem value="inactive">Inactivo</SelectItem>
                  <SelectItem value="locked">Bloqueado</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </CardContent>
        </Card>

        {/* Hierarchical Assignments */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              Asignación Jerárquica
              {totalAssignments > 0 && (
                <Badge variant="secondary" className="text-xs">{totalAssignments} asignaciones</Badge>
              )}
            </CardTitle>
            <p className="text-xs text-muted-foreground">
              Asocie el usuario a múltiples niveles de la jerarquía: Organizaciones, Entidades y Sucursales.
            </p>
          </CardHeader>
          <CardContent className="space-y-5">
            {/* Organizations */}
            <div>
              <div className="flex items-center gap-2 mb-2">
                <Building2 className="h-4 w-4 text-primary" />
                <p className="text-sm font-semibold">Organizaciones</p>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                {tenants.map(t => (
                  <label
                    key={t.id}
                    className={`flex items-center gap-3 p-2.5 rounded-lg border cursor-pointer transition-colors text-sm ${
                      assignments.organizationIds.includes(t.id)
                        ? "border-primary bg-primary/5"
                        : "border-border hover:bg-muted/50"
                    }`}
                  >
                    <Checkbox
                      checked={assignments.organizationIds.includes(t.id)}
                      onCheckedChange={() => toggleOrganization(t.id)}
                    />
                    <div>
                      <p className="font-medium">{t.name}</p>
                      <p className="text-xs text-muted-foreground">{t.identifier}</p>
                    </div>
                  </label>
                ))}
              </div>
            </div>

            {/* Entities - grouped by organization, only shown when orgs selected */}
            {assignments.organizationIds.length > 0 && (
              <div>
                <div className="flex items-center gap-2 mb-2">
                  <Building className="h-4 w-4 text-primary" />
                  <p className="text-sm font-semibold">Entidades</p>
                </div>
                {assignments.organizationIds.map(orgId => {
                  const org = tenants.find(t => t.id === orgId);
                  const orgEntities = entities.filter(e => e.tenantId === orgId);
                  if (orgEntities.length === 0) return null;
                  return (
                    <div key={orgId} className="mb-3">
                      <p className="text-xs font-medium text-muted-foreground mb-1.5 ml-1">{org?.name}</p>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                        {orgEntities.map(e => (
                          <label
                            key={e.id}
                            className={`flex items-center gap-3 p-2.5 rounded-lg border cursor-pointer transition-colors text-sm ${
                              assignments.entityIds.includes(e.id)
                                ? "border-primary bg-primary/5"
                                : "border-border hover:bg-muted/50"
                            }`}
                          >
                            <Checkbox
                              checked={assignments.entityIds.includes(e.id)}
                              onCheckedChange={() => toggleEntity(e.id)}
                            />
                            <div>
                              <p className="font-medium">{e.name}</p>
                              <p className="text-xs text-muted-foreground">{e.type}</p>
                            </div>
                          </label>
                        ))}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}

            {/* Branches - grouped by entity, only shown when entities selected */}
            {assignments.entityIds.length > 0 && (
              <div>
                <div className="flex items-center gap-2 mb-2">
                  <MapPin className="h-4 w-4 text-primary" />
                  <p className="text-sm font-semibold">Sucursales</p>
                </div>
                {assignments.entityIds.map(entityId => {
                  const entity = entities.find(e => e.id === entityId);
                  const entityBranches = allBranches.filter(b => b.entityId === entityId);
                  if (entityBranches.length === 0) return null;
                  return (
                    <div key={entityId} className="mb-3">
                      <p className="text-xs font-medium text-muted-foreground mb-1.5 ml-1">{entity?.name}</p>
                      <div className="grid grid-cols-1 md:grid-cols-3 gap-2">
                        {entityBranches.map(b => (
                          <label
                            key={b.id}
                            className={`flex items-center gap-3 p-2.5 rounded-lg border cursor-pointer transition-colors text-sm ${
                              assignments.branchIds.includes(b.id)
                                ? "border-primary bg-primary/5"
                                : "border-border hover:bg-muted/50"
                            }`}
                          >
                            <Checkbox
                              checked={assignments.branchIds.includes(b.id)}
                              onCheckedChange={() => toggleBranch(b.id)}
                            />
                            <div>
                              <p className="font-medium">{b.name}</p>
                              <p className="text-xs text-muted-foreground">{b.code}</p>
                            </div>
                          </label>
                        ))}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Roles Asignados</CardTitle>
            <p className="text-xs text-muted-foreground">Un usuario puede tener múltiples roles. Los permisos efectivos son la unión de todos los roles asignados.</p>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
              {roles.map(r => (
                <label
                  key={r.id}
                  className={`flex items-start gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${
                    selectedRoleIds.includes(r.id) ? "border-primary bg-primary/5" : "border-border hover:bg-muted/50"
                  }`}
                >
                  <Checkbox
                    checked={selectedRoleIds.includes(r.id)}
                    onCheckedChange={() => toggleRole(r.id)}
                    className="mt-0.5"
                  />
                  <div>
                    <p className="text-sm font-medium">{r.name}</p>
                    <p className="text-xs text-muted-foreground">{r.description}</p>
                    <p className="text-xs text-muted-foreground mt-1">{r.permissions.length} permisos</p>
                  </div>
                </label>
              ))}
            </div>
            {errors.roleIds && <p className="text-xs text-destructive mt-2">{errors.roleIds.message}</p>}
          </CardContent>
        </Card>

        {selectedRoleIds.length > 0 && (
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base flex items-center gap-2">
                Permisos Efectivos
                <Badge variant="secondary" className="text-xs">{mergedPermissions.length} permisos</Badge>
              </CardTitle>
              {selectedRoleIds.length > 1 && (
                <p className="text-xs text-muted-foreground">
                  Resultado de la unión de: {roles.filter(r => selectedRoleIds.includes(r.id)).map(r => r.name).join(", ")}
                </p>
              )}
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {modules.map(mod => {
                  const perms = mockPermissions.filter(p => p.module === mod);
                  const activePerms = perms.filter(p => mergedPermissions.includes(p.id));
                  if (activePerms.length === 0) return null;
                  return (
                    <div key={mod}>
                      <p className="text-sm font-semibold mb-1">{mod}</p>
                      <div className="flex flex-wrap gap-1.5 ml-2">
                        {perms.map(p => (
                          <Badge
                            key={p.id}
                            variant={mergedPermissions.includes(p.id) ? "default" : "outline"}
                            className={`text-xs ${mergedPermissions.includes(p.id) ? "" : "opacity-30"}`}
                          >
                            {p.action}
                          </Badge>
                        ))}
                      </div>
                    </div>
                  );
                })}
              </div>
            </CardContent>
          </Card>
        )}

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate(isEdit ? `/usuarios/${id}` : "/usuarios")}>
            Cancelar
          </Button>
          <Button type="submit">{isEdit ? "Guardar Cambios" : "Crear Usuario"}</Button>
        </div>
      </form>
    </div>
  );
}
