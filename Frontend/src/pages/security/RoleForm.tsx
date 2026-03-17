import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft, ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible";
import { useRoleStore, mockPermissions } from "@/data/store";
import { toast } from "sonner";

const schema = z.object({
  name: z.string().trim().min(2, "Mínimo 2 caracteres").max(50),
  description: z.string().trim().min(3, "Mínimo 3 caracteres").max(200),
  permissions: z.array(z.string()).min(1, "Seleccione al menos un permiso"),
});

type FormData = {
  name: string;
  description: string;
  permissions: string[];
};

export default function RoleForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { roles, addRole, updateRole } = useRoleStore();
  const role = id ? roles.find(r => r.id === id) : null;
  const isEdit = !!role;

  const modules = [...new Set(mockPermissions.map(p => p.module))];

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: role
      ? { name: role.name, description: role.description, permissions: role.permissions }
      : { name: "", description: "", permissions: [] },
  });

  const selectedPerms = watch("permissions") || [];

  const togglePerm = (permId: string) => {
    const updated = selectedPerms.includes(permId)
      ? selectedPerms.filter(p => p !== permId)
      : [...selectedPerms, permId];
    setValue("permissions", updated, { shouldValidate: true });
  };

  const toggleModule = (mod: string) => {
    const modPerms = mockPermissions.filter(p => p.module === mod).map(p => p.id);
    const allChecked = modPerms.every(id => selectedPerms.includes(id));
    const updated = allChecked
      ? selectedPerms.filter(id => !modPerms.includes(id))
      : [...new Set([...selectedPerms, ...modPerms])];
    setValue("permissions", updated, { shouldValidate: true });
  };

  const onSubmit = (data: FormData) => {
    if (isEdit) {
      updateRole(role!.id, data);
      toast.success("Rol actualizado");
    } else {
      addRole(data);
      toast.success("Rol creado");
    }
    navigate("/roles");
  };

  return (
    <div className="space-y-6 max-w-2xl">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/roles")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">{isEdit ? "Editar Rol" : "Nuevo Rol"}</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Datos del Rol</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-1.5">
              <Label>Nombre *</Label>
              <Input {...register("name")} />
              {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label>Descripción *</Label>
              <Input {...register("description")} />
              {errors.description && <p className="text-xs text-destructive">{errors.description.message}</p>}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">
              Permisos ({selectedPerms.length}/{mockPermissions.length})
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {modules.map(mod => {
              const perms = mockPermissions.filter(p => p.module === mod);
              const checkedCount = perms.filter(p => selectedPerms.includes(p.id)).length;
              const allChecked = checkedCount === perms.length;

              return (
                <Collapsible key={mod} defaultOpen>
                  <div className="flex items-center gap-2">
                    <Checkbox
                      checked={allChecked}
                      onCheckedChange={() => toggleModule(mod)}
                    />
                    <CollapsibleTrigger className="flex items-center gap-2 flex-1 text-sm font-semibold py-2 hover:text-primary transition-colors">
                      <ChevronDown className="h-4 w-4" />
                      {mod}
                      <span className="text-xs text-muted-foreground font-normal">
                        ({checkedCount}/{perms.length})
                      </span>
                    </CollapsibleTrigger>
                  </div>
                  <CollapsibleContent>
                    <div className="ml-8 space-y-2 pb-2">
                      {perms.map(p => (
                        <label key={p.id} className="flex items-center gap-3 cursor-pointer text-sm">
                          <Checkbox
                            checked={selectedPerms.includes(p.id)}
                            onCheckedChange={() => togglePerm(p.id)}
                          />
                          <div>
                            <span className="font-medium">{p.action}</span>
                            <span className="text-muted-foreground ml-2">— {p.description}</span>
                          </div>
                        </label>
                      ))}
                    </div>
                  </CollapsibleContent>
                </Collapsible>
              );
            })}
            {errors.permissions && <p className="text-xs text-destructive mt-2">{errors.permissions.message}</p>}
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate("/roles")}>Cancelar</Button>
          <Button type="submit">{isEdit ? "Guardar Cambios" : "Crear Rol"}</Button>
        </div>
      </form>
    </div>
  );
}
