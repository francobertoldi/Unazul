import { useState } from "react";
import { PhoneCodeSelect } from "@/components/shared/PhoneCodeSelect";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft, Plus, Pencil, Trash2, User as UserIcon, Globe } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import { DataTable } from "@/components/shared/DataTable";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { useEntityStore, useUserStore, useTenantStore, EntityInput } from "@/data/store";
import { ChannelType, Branch, User } from "@/data/types";
import { mockParameters } from "@/data/parameters";
import { toast } from "sonner";

// Get entity types, channels, countries, provinces and cities from parameters
const paramEntityTypes = mockParameters.filter(p => p.group === 'entity_types');
const paramChannels = mockParameters.filter(p => p.group === 'channels');
const paramCountries = mockParameters.filter(p => p.group === 'countries');
const paramProvinces = mockParameters.filter(p => p.group === 'provinces');
const paramCities = mockParameters.filter(p => p.group === 'cities');

const entityTypeValues = paramEntityTypes.map(p => p.value);
const channelValues = paramChannels.map(p => p.value);

function getProvincesForCountry(countryValue: string) {
  const country = paramCountries.find(p => p.value === countryValue);
  if (!country) return [];
  return paramProvinces.filter(pr => pr.parentKey === country.key);
}

function getCitiesForProvince(provinceValue: string) {
  const province = paramProvinces.find(p => p.value === provinceValue);
  if (!province) return [];
  return paramCities.filter(c => c.parentKey === province.key);
}

const schema = z.object({
  name: z.string().trim().min(2, "Mínimo 2 caracteres").max(100),
  identifier: z.string().trim().min(5, "Mínimo 5 caracteres").max(20),
  type: z.string().min(1, "Seleccione un tipo"),
  status: z.enum(["active", "inactive", "suspended"]),
  email: z.string().trim().email("Email inválido").max(255),
  phoneCode: z.string().min(1, "Seleccione código"),
  phone: z.string().trim().min(5, "Mínimo 5 caracteres").max(30),
  address: z.string().trim().min(3, "Mínimo 3 caracteres").max(200),
  city: z.string().min(1, "Seleccione una ciudad"),
  province: z.string().min(1, "Seleccione una provincia"),
  country: z.string().min(1, "Seleccione un país"),
  channels: z.array(z.string()).min(1, "Seleccione al menos un canal"),
});

const typeLabels: Record<string, string> = Object.fromEntries(paramEntityTypes.map(p => [p.value, p.description]));
const channelLabels: Record<string, string> = Object.fromEntries(paramChannels.map(p => [p.value, p.description]));

export default function EntityForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { entities, addEntity, updateEntity, deleteBranch } = useEntityStore();
  const { users } = useUserStore();
  const { tenants } = useTenantStore();
  const entity = id ? entities.find(e => e.id === id) : null;
  const isEdit = !!entity;

  const entityUsers = isEdit ? users.filter(u => u.entityId === entity!.id) : [];

  const [deleteBranchTarget, setDeleteBranchTarget] = useState<Branch | null>(null);

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<EntityInput>({
    resolver: zodResolver(schema),
    defaultValues: entity
      ? { tenantId: entity.tenantId, tenantName: entity.tenantName, name: entity.name, identifier: entity.identifier, type: entity.type, status: entity.status, email: entity.email, phoneCode: entity.phoneCode, phone: entity.phone, address: entity.address, city: entity.city, province: entity.province, country: entity.country, channels: entity.channels }
      : { tenantId: "", tenantName: "", name: "", identifier: "", type: "bank", status: "active", email: "", phoneCode: "+54", phone: "", address: "", city: "", province: "", country: "", channels: ["web"] },
  });

  const channels = watch("channels") || [];
  const selectedCountry = watch("country") || "";
  const selectedProvince = watch("province") || "";
  const filteredProvinces = getProvincesForCountry(selectedCountry);
  const filteredCities = getCitiesForProvince(selectedProvince);

  const onSubmit = (data: EntityInput) => {
    if (isEdit) {
      updateEntity(entity!.id, data);
      toast.success("Entidad actualizada");
      navigate(`/entidades/${entity!.id}`);
    } else {
      const newEntity = addEntity(data);
      toast.success("Entidad creada");
      navigate(`/entidades/${newEntity.id}`);
    }
  };

  // Branch columns
  const branchColumns = [
    {
      key: "name", header: "Sucursal", searchable: true,
      render: (b: Branch) => (<div><p className="font-medium text-sm">{b.name}</p><p className="text-xs text-muted-foreground">Código: {b.code}</p></div>),
    },
    { key: "address", header: "Dirección", searchable: true, render: (b: Branch) => <span className="text-sm">{b.address}</span> },
    { key: "city", header: "Ciudad", searchable: true, render: (b: Branch) => <span className="text-sm">{b.city}</span> },
    { key: "manager", header: "Responsable", searchable: true, render: (b: Branch) => <span className="text-sm">{b.manager}</span> },
    { key: "status", header: "Estado", searchable: false, render: (b: Branch) => <StatusBadge status={b.status} /> },
    {
      key: "_actions", header: "", searchable: false,
      render: (b: Branch) => (
        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/entidades/${id}/sucursales/${b.id}/editar`)}><Pencil className="h-3.5 w-3.5" /></Button>
          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteBranchTarget(b)}><Trash2 className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  // User columns
  const userColumns = [
    {
      key: "firstName", header: "Usuario", searchable: true,
      render: (u: User) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0"><UserIcon className="h-3.5 w-3.5 text-primary" /></div>
          <div><p className="font-medium text-sm">{u.firstName} {u.lastName}</p><p className="text-xs text-muted-foreground">{u.email}</p></div>
        </div>
      ),
    },
    {
      key: "roleNames", header: "Roles", searchable: false,
      render: (u: User) => <div className="flex gap-1 flex-wrap">{u.roleNames.map(n => <Badge key={n} variant="secondary" className="text-xs">{n}</Badge>)}</div>,
    },
    { key: "status", header: "Estado", searchable: false, render: (u: User) => <StatusBadge status={u.status} /> },
    {
      key: "_actions", header: "", searchable: false,
      render: (u: User) => (
        <div className="flex items-center gap-1" onClick={ev => ev.stopPropagation()}>
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => navigate(`/usuarios/${u.id}/editar`)}><Pencil className="h-3.5 w-3.5" /></Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate(isEdit ? `/entidades/${id}` : "/entidades")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">{isEdit ? "Editar Entidad" : "Nueva Entidad"}</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Datos Generales</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-1.5">
                <Label>Tenant *</Label>
                <Select defaultValue={entity?.tenantId || ""} onValueChange={v => {
                  const t = tenants.find(t => t.id === v);
                  setValue("tenantId", v);
                  setValue("tenantName", t?.name || "");
                }}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar tenant" /></SelectTrigger>
                  <SelectContent>
                    {tenants.filter(t => t.status === 'active').map(t => (<SelectItem key={t.id} value={t.id}>{t.name}</SelectItem>))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5">
                <Label>Nombre *</Label>
                <Input {...register("name")} />
                {errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Identificador (CUIT) *</Label>
                <Input {...register("identifier")} />
                {errors.identifier && <p className="text-xs text-destructive">{errors.identifier.message}</p>}
              </div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Tipo *</Label>
                <Select defaultValue={entity?.type || "bank"} onValueChange={v => setValue("type", v as any)}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {Object.entries(typeLabels).map(([k, v]) => (<SelectItem key={k} value={k}>{v}</SelectItem>))}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5">
                <Label>Estado *</Label>
                <Select defaultValue={entity?.status || "active"} onValueChange={v => setValue("status", v as any)}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="active">Activo</SelectItem>
                    <SelectItem value="inactive">Inactivo</SelectItem>
                    <SelectItem value="suspended">Suspendido</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Contacto</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Email *</Label>
                <Input {...register("email")} type="email" />
                {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
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
            <div className="space-y-1.5">
              <Label>Dirección *</Label>
              <Input {...register("address")} />
              {errors.address && <p className="text-xs text-destructive">{errors.address.message}</p>}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Posicionamiento</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-1.5">
                <Label>País *</Label>
                <Select value={selectedCountry} onValueChange={v => { setValue("country", v, { shouldValidate: true }); setValue("province", "", { shouldValidate: false }); setValue("city", "", { shouldValidate: false }); }}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar país..." /></SelectTrigger>
                  <SelectContent>
                    {paramCountries.map(p => <SelectItem key={p.id} value={p.value}>{p.value}</SelectItem>)}
                  </SelectContent>
                </Select>
                {errors.country && <p className="text-xs text-destructive">{errors.country.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Provincia *</Label>
                <Select value={selectedProvince} onValueChange={v => { setValue("province", v, { shouldValidate: true }); setValue("city", "", { shouldValidate: false }); }} disabled={!selectedCountry}>
                  <SelectTrigger><SelectValue placeholder={selectedCountry ? "Seleccionar provincia..." : "Seleccione país primero"} /></SelectTrigger>
                  <SelectContent>
                    {filteredProvinces.map(p => <SelectItem key={p.id} value={p.value}>{p.value}</SelectItem>)}
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
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Canales Habilitados</CardTitle></CardHeader>
          <CardContent>
            <div className="flex gap-6 flex-wrap">
              {paramChannels.map(ch => (
                <label key={ch.value} className="flex items-center gap-2 text-sm cursor-pointer">
                  <Checkbox
                    checked={channels.includes(ch.value as ChannelType)}
                    onCheckedChange={(checked) => {
                      const updated = checked ? [...channels, ch.value] : channels.filter(c => c !== ch.value);
                      setValue("channels", updated as ChannelType[], { shouldValidate: true });
                    }}
                  />
                  {ch.description}
                </label>
              ))}
            </div>
            {errors.channels && <p className="text-xs text-destructive mt-2">{errors.channels.message}</p>}
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate(isEdit ? `/entidades/${id}` : "/entidades")}>Cancelar</Button>
          <Button type="submit">{isEdit ? "Guardar Cambios" : "Crear Entidad"}</Button>
        </div>
      </form>

      {/* Only show management sections when editing an existing entity */}
      {isEdit && entity && (
        <>
          <div className="border-t pt-6">
            <h2 className="text-lg font-semibold tracking-tight mb-4">Sucursales ({entity.branches.length})</h2>
            <DataTable
              data={entity.branches}
              columns={branchColumns}
              searchPlaceholder="Buscar sucursal..."
              exportFileName={`sucursales_${entity.name.replace(/\s+/g, '_')}`}
              actions={
                <Button className="gap-2" size="sm" onClick={() => navigate(`/entidades/${id}/sucursales/nuevo`)}>
                  <Plus className="h-4 w-4" /> Nueva Sucursal
                </Button>
              }
            />
          </div>

          <div className="border-t pt-6">
            <h2 className="text-lg font-semibold tracking-tight mb-4">Usuarios de la Entidad ({entityUsers.length})</h2>
            <DataTable
              data={entityUsers}
              columns={userColumns}
              searchPlaceholder="Buscar usuario..."
              exportFileName={`usuarios_${entity.name.replace(/\s+/g, '_')}`}
              onRowClick={(u) => navigate(`/usuarios/${u.id}`)}
              actions={
                <Button className="gap-2" size="sm" onClick={() => navigate("/usuarios/nuevo")}>
                  <Plus className="h-4 w-4" /> Nuevo Usuario
                </Button>
              }
            />
          </div>

          <DeleteConfirmDialog
            open={!!deleteBranchTarget}
            onOpenChange={o => !o && setDeleteBranchTarget(null)}
            title="Eliminar Sucursal"
            description={`¿Está seguro de eliminar "${deleteBranchTarget?.name}"?`}
            onConfirm={() => {
              if (deleteBranchTarget) {
                deleteBranch(entity.id, deleteBranchTarget.id);
                toast.success("Sucursal eliminada");
                setDeleteBranchTarget(null);
              }
            }}
          />
        </>
      )}
    </div>
  );
}
