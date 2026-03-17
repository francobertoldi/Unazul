import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useEntityStore, BranchInput } from "@/data/store";
import { mockParameters } from "@/data/parameters";
import { toast } from "sonner";
import { PhoneCodeSelect } from "@/components/shared/PhoneCodeSelect";

const paramProvinces = mockParameters.filter(p => p.group === 'provinces');
const paramCities = mockParameters.filter(p => p.group === 'cities');

function getCitiesForProvince(provinceName: string) {
  const province = paramProvinces.find(p => p.value === provinceName);
  if (!province) return [];
  return paramCities.filter(c => c.parentKey === province.key);
}

const schema = z.object({
  name: z.string().trim().min(2, "Mínimo 2 caracteres").max(100),
  code: z.string().trim().min(1, "Requerido").max(10),
  address: z.string().trim().min(3, "Mínimo 3 caracteres").max(200),
  city: z.string().trim().min(1, "Seleccione una ciudad").max(100),
  province: z.string().trim().min(1, "Seleccione una provincia").max(100),
  status: z.enum(["active", "inactive", "suspended"]),
  manager: z.string().trim().min(2, "Mínimo 2 caracteres").max(100),
  phoneCode: z.string().min(1, "Seleccione código"),
  phone: z.string().trim().min(5, "Mínimo 5 caracteres").max(30),
});

export default function BranchForm() {
  const { entityId, branchId } = useParams();
  const navigate = useNavigate();
  const { entities, addBranch, updateBranch } = useEntityStore();
  const entity = entities.find(e => e.id === entityId);
  const branch = branchId ? entity?.branches.find(b => b.id === branchId) : null;
  const isEdit = !!branch;

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<BranchInput>({
    resolver: zodResolver(schema),
    defaultValues: branch
      ? { name: branch.name, code: branch.code, address: branch.address, city: branch.city, province: branch.province, status: branch.status, manager: branch.manager, phoneCode: branch.phoneCode, phone: branch.phone }
      : { name: "", code: "", address: "", city: "", province: "", status: "active", manager: "", phoneCode: "+54", phone: "" },
  });

  const selectedProvince = watch("province") || "";
  const filteredCities = getCitiesForProvince(selectedProvince);

  if (!entity) {
    return <div className="flex items-center justify-center h-64"><p className="text-muted-foreground">Entidad no encontrada</p></div>;
  }

  const onSubmit = (data: BranchInput) => {
    if (isEdit) {
      updateBranch(entityId!, branchId!, data);
      toast.success("Sucursal actualizada");
    } else {
      addBranch(entityId!, data);
      toast.success("Sucursal creada");
    }
    navigate(`/entidades/${entityId}`);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate(`/entidades/${entityId}`)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold tracking-tight">{isEdit ? "Editar Sucursal" : "Nueva Sucursal"}</h1>
          <p className="text-sm text-muted-foreground">{entity.name}</p>
        </div>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base">Datos de la Sucursal</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Nombre *</Label>
                <Input {...register("name")} />
                {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Código *</Label>
                <Input {...register("code")} />
                {errors.code && <p className="text-xs text-destructive">{errors.code.message}</p>}
              </div>
            </div>
            <div className="space-y-1.5">
              <Label>Dirección *</Label>
              <Input {...register("address")} />
              {errors.address && <p className="text-xs text-destructive">{errors.address.message}</p>}
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Provincia *</Label>
                <Select value={selectedProvince} onValueChange={v => { setValue("province", v, { shouldValidate: true }); setValue("city", "", { shouldValidate: false }); }}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar provincia..." /></SelectTrigger>
                  <SelectContent>
                    {paramProvinces.map(p => <SelectItem key={p.id} value={p.value}>{p.value}</SelectItem>)}
                  </SelectContent>
                </Select>
                {errors.province && <p className="text-xs text-destructive">{errors.province.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Ciudad *</Label>
                <Select value={watch("city") || ""} onValueChange={v => setValue("city", v, { shouldValidate: true })} disabled={!selectedProvince}>
                  <SelectTrigger><SelectValue placeholder={selectedProvince ? "Seleccionar ciudad..." : "Seleccione provincia primero"} /></SelectTrigger>
                  <SelectContent>
                    {filteredCities.map(c => <SelectItem key={c.id} value={c.value}>{c.value}</SelectItem>)}
                  </SelectContent>
                </Select>
                {errors.city && <p className="text-xs text-destructive">{errors.city.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Estado *</Label>
                <Select defaultValue={branch?.status || "active"} onValueChange={v => setValue("status", v as any)}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="active">Activo</SelectItem>
                    <SelectItem value="inactive">Inactivo</SelectItem>
                    <SelectItem value="suspended">Suspendido</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Responsable *</Label>
                <Input {...register("manager")} />
                {errors.manager && <p className="text-xs text-destructive">{errors.manager.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Teléfono *</Label>
                <div className="flex gap-2">
                  <PhoneCodeSelect value={watch("phoneCode")} onValueChange={v => setValue("phoneCode", v, { shouldValidate: true })} />
                  <Input {...register("phone")} className="flex-1" />
                </div>
                {(errors.phoneCode || errors.phone) && <p className="text-xs text-destructive">{errors.phoneCode?.message || errors.phone?.message}</p>}
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate(`/entidades/${entityId}`)}>Cancelar</Button>
          <Button type="submit">{isEdit ? "Guardar Cambios" : "Crear Sucursal"}</Button>
        </div>
      </form>
    </div>
  );
}
