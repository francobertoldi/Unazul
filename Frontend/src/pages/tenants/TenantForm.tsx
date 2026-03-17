import { useState, useMemo, useRef } from "react";
import { PhoneCodeSelect } from "@/components/shared/PhoneCodeSelect";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft, Building2, Pencil, Search, ChevronsUpDown, X, Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { DataTable } from "@/components/shared/DataTable";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { useTenantStore, useEntityStore, TenantInput } from "@/data/store";
import { useParameterStore } from "@/data/store";
import { Entity } from "@/data/types";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

const schema = z.object({
  name: z.string().trim().min(2, "Mínimo 2 caracteres").max(100),
  identifier: z.string().trim().min(5, "Mínimo 5 caracteres").max(20),
  description: z.string().trim().max(500).optional().default(""),
  status: z.enum(["active", "inactive"]),
  contactName: z.string().trim().min(2, "Mínimo 2 caracteres").max(100),
  contactEmail: z.string().trim().email("Email inválido").max(255),
  contactPhoneCode: z.string().min(1, "Seleccione código"),
  contactPhone: z.string().trim().min(5, "Mínimo 5 caracteres").max(30),
  country: z.string().trim().min(2, "Mínimo 2 caracteres").max(100),
});

export default function TenantForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { tenants, addTenant, updateTenant } = useTenantStore();
  const { entities } = useEntityStore();
  const { parameters } = useParameterStore();
  const tenant = id ? tenants.find(t => t.id === id) : null;
  const isEdit = !!tenant;
  const tenantEntities = isEdit ? entities.filter(e => e.tenantId === tenant!.id) : [];

  const countryOptions = useMemo(() =>
    parameters
      .filter(p => p.group === 'countries')
      .map(p => ({ value: p.value, label: p.value }))
      .sort((a, b) => a.label.localeCompare(b.label)),
    [parameters]
  );

  const [countryOpen, setCountryOpen] = useState(false);
  const [countrySearch, setCountrySearch] = useState("");

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<TenantInput>({
    resolver: zodResolver(schema),
    defaultValues: tenant
       ? { name: tenant.name, identifier: tenant.identifier, description: tenant.description, status: tenant.status, contactName: tenant.contactName, contactEmail: tenant.contactEmail, contactPhoneCode: tenant.contactPhoneCode, contactPhone: tenant.contactPhone, country: tenant.country }
      : { name: "", identifier: "", description: "", status: "active", contactName: "", contactEmail: "", contactPhoneCode: "+54", contactPhone: "", country: "Argentina" },
  });

  const selectedCountry = watch("country");
  const filteredCountries = useMemo(() => {
    if (!countrySearch.trim()) return countryOptions;
    const q = countrySearch.toLowerCase();
    return countryOptions.filter(c => c.label.toLowerCase().includes(q));
  }, [countryOptions, countrySearch]);

  const onSubmit = (data: TenantInput) => {
    if (isEdit) {
      updateTenant(tenant!.id, data);
      toast.success("Organización actualizada");
      navigate(`/tenants/${tenant!.id}`);
    } else {
      const newTenant = addTenant(data);
      toast.success("Organización creada");
      navigate(`/tenants/${newTenant.id}`);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate(isEdit ? `/tenants/${id}` : "/tenants")}><ArrowLeft className="h-4 w-4" /></Button>
        <h1 className="text-2xl font-bold tracking-tight">{isEdit ? "Editar Organización" : "Nueva Organización"}</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Datos Generales</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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
                <Label>País *</Label>
                <Popover open={countryOpen} onOpenChange={(o) => { setCountryOpen(o); if (!o) setCountrySearch(""); }}>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      role="combobox"
                      aria-expanded={countryOpen}
                      className={cn("w-full justify-between font-normal", !selectedCountry && "text-muted-foreground")}
                    >
                      <span className="truncate">{selectedCountry || "Seleccionar país..."}</span>
                      <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-[280px] p-0" align="start">
                    <div className="flex items-center border-b px-2">
                      <Search className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
                      <input
                        value={countrySearch}
                        onChange={(e) => setCountrySearch(e.target.value)}
                        placeholder="Buscar país..."
                        className="flex h-9 w-full bg-transparent py-2 px-2 text-sm outline-none placeholder:text-muted-foreground"
                      />
                      {countrySearch && (
                        <X className="h-3.5 w-3.5 shrink-0 opacity-50 cursor-pointer hover:opacity-100" onClick={() => setCountrySearch("")} />
                      )}
                    </div>
                    <div className="max-h-[200px] overflow-y-auto p-1">
                      {filteredCountries.length === 0 ? (
                        <p className="py-4 text-center text-sm text-muted-foreground">Sin resultados</p>
                      ) : (
                        filteredCountries.map((opt) => (
                          <div
                            key={opt.value}
                            className={cn(
                              "flex items-center gap-2 rounded-sm px-2 py-1.5 text-sm cursor-pointer hover:bg-accent hover:text-accent-foreground",
                              selectedCountry === opt.value && "bg-accent"
                            )}
                            onClick={() => {
                              setValue("country", opt.value, { shouldValidate: true });
                              setCountryOpen(false);
                              setCountrySearch("");
                            }}
                          >
                            <Check className={cn("h-3.5 w-3.5", selectedCountry === opt.value ? "opacity-100" : "opacity-0")} />
                            <span>{opt.label}</span>
                          </div>
                        ))
                      )}
                    </div>
                  </PopoverContent>
                </Popover>
                {errors.country && <p className="text-xs text-destructive">{errors.country.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Estado *</Label>
                <Select defaultValue={tenant?.status || "active"} onValueChange={v => setValue("status", v as any)}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="active">Activo</SelectItem>
                    <SelectItem value="inactive">Inactivo</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="space-y-1.5">
              <Label>Descripción</Label>
              <Textarea {...register("description")} rows={3} />
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Contacto</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-1.5">
                <Label>Nombre de contacto *</Label>
                <Input {...register("contactName")} />
                {errors.contactName && <p className="text-xs text-destructive">{errors.contactName.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Email de contacto *</Label>
                <Input {...register("contactEmail")} type="email" />
                {errors.contactEmail && <p className="text-xs text-destructive">{errors.contactEmail.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Teléfono *</Label>
                <div className="flex gap-2">
                  <PhoneCodeSelect value={watch("contactPhoneCode")} onValueChange={v => setValue("contactPhoneCode", v, { shouldValidate: true })} />
                  <Input {...register("contactPhone")} className="flex-1" />
                </div>
                {(errors.contactPhoneCode || errors.contactPhone) && <p className="text-xs text-destructive">{errors.contactPhoneCode?.message || errors.contactPhone?.message}</p>}
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate(isEdit ? `/tenants/${id}` : "/tenants")}>Cancelar</Button>
          <Button type="submit">{isEdit ? "Guardar Cambios" : "Crear Organización"}</Button>
        </div>
      </form>
      {isEdit && (
        <div className="border-t pt-6">
          <h2 className="text-lg font-semibold tracking-tight mb-4">Entidades de la Organización ({tenantEntities.length})</h2>
          <DataTable
            data={tenantEntities}
            columns={[
              {
                key: "name", header: "Entidad", searchable: true,
                render: (e: Entity) => (
                  <div className="flex items-center gap-3">
                    <div className="h-9 w-9 rounded-lg bg-primary/10 flex items-center justify-center shrink-0"><Building2 className="h-4 w-4 text-primary" /></div>
                    <div><p className="font-medium text-sm">{e.name}</p><p className="text-xs text-muted-foreground">{e.identifier}</p></div>
                  </div>
                ),
              },
              { key: "city", header: "Ciudad", searchable: true, render: (e: Entity) => <span className="text-sm">{e.city}</span> },
              { key: "status", header: "Estado", searchable: false, render: (e: Entity) => <StatusBadge status={e.status} /> },
              {
                key: "_actions", header: "", searchable: false,
                render: (e: Entity) => (
                  <Button variant="ghost" size="icon" className="h-8 w-8" onClick={(ev) => { ev.stopPropagation(); navigate(`/entidades/${e.id}/editar`); }}>
                    <Pencil className="h-3.5 w-3.5" />
                  </Button>
                ),
              },
            ]}
            searchPlaceholder="Buscar entidad..."
            onRowClick={(e) => navigate(`/entidades/${e.id}/editar`)}
          />
        </div>
      )}
    </div>
  );
}
