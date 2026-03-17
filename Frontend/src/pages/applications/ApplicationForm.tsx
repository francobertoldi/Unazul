import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useApplicationStore, useProductStore, useEntityStore, useUserStore, useParameterStore, ApplicationInput } from "@/data/store";
import { toast } from "sonner";
import { useState, useMemo } from "react";
import { PhoneCodeSelect } from "@/components/shared/PhoneCodeSelect";

const schema = z.object({
  entityId: z.string().min(1, "Seleccione una entidad"),
  productId: z.string().min(1, "Seleccione un producto"),
  planId: z.string().min(1, "Seleccione un plan"),
  firstName: z.string().trim().min(2, "Mínimo 2 caracteres"),
  lastName: z.string().trim().min(2, "Mínimo 2 caracteres"),
  documentType: z.enum(["DNI", "CUIT", "passport"]),
  documentNumber: z.string().trim().min(3, "Mínimo 3 caracteres"),
  email: z.string().trim().email("Email inválido"),
  phoneCode: z.string().min(1, "Seleccione código"),
  phone: z.string().trim().min(5, "Mínimo 5 caracteres"),
  assignedUserId: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

export default function ApplicationForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { applications, addApplication, updateApplication } = useApplicationStore();
  const { products } = useProductStore();
  const { entities } = useEntityStore();
  const { users } = useUserStore();
  const { parameters, getGroupValues } = useParameterStore();
  const app = id ? applications.find(a => a.id === id) : null;
  const isEdit = !!app;

  // Address state
  const [addrStreet, setAddrStreet] = useState(app?.address?.street || "");
  const [addrNumber, setAddrNumber] = useState(app?.address?.number || "");
  const [addrFloor, setAddrFloor] = useState(app?.address?.floor || "");
  const [addrApartment, setAddrApartment] = useState(app?.address?.apartment || "");
  const [addrProvince, setAddrProvince] = useState(app?.address?.province || "");
  const [addrCity, setAddrCity] = useState(app?.address?.city || "");
  const [addrPostalCode, setAddrPostalCode] = useState(app?.address?.postalCode || "");

  const provinceOptions = getGroupValues('provinces');
  const cityOptions = useMemo(() => {
    if (!addrProvince) return [];
    const provinceParam = parameters.find(p => p.group === 'provinces' && p.value === addrProvince);
    if (!provinceParam) return [];
    return parameters
      .filter(p => p.group === 'cities' && p.parentKey === provinceParam.key)
      .map(p => ({ code: p.value, label: p.value }));
  }, [addrProvince, parameters]);

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: app
      ? { entityId: app.entityId, productId: app.productId, planId: app.planId, firstName: app.applicant.firstName, lastName: app.applicant.lastName, documentType: app.applicant.documentType, documentNumber: app.applicant.documentNumber, email: app.applicant.email, phoneCode: app.applicant.phoneCode, phone: app.applicant.phone, assignedUserId: app.assignedUserId }
      : { entityId: "", productId: "", planId: "", firstName: "", lastName: "", documentType: "DNI", documentNumber: "", email: "", phoneCode: "+54", phone: "", assignedUserId: "" },
  });

  const selectedEntityId = watch("entityId");
  const selectedProductId = watch("productId");

  const entityProducts = useMemo(() => products.filter(p => p.entityId === selectedEntityId && p.status === 'active'), [products, selectedEntityId]);
  const selectedProduct = products.find(p => p.id === selectedProductId);
  const entityUsers = useMemo(() => users.filter(u => u.entityId === selectedEntityId || !u.entityId), [users, selectedEntityId]);

  const onSubmit = (data: FormValues) => {
    const entityName = entities.find(e => e.id === data.entityId)?.name || "";
    const product = products.find(p => p.id === data.productId);
    const plan = product?.plans.find(p => p.id === data.planId);
    const assignedUser = users.find(u => u.id === data.assignedUserId);

    const address = addrStreet.trim()
      ? {
          street: addrStreet.trim(),
          number: addrNumber.trim(),
          floor: addrFloor.trim() || undefined,
          apartment: addrApartment.trim() || undefined,
          city: addrCity,
          province: addrProvince,
          postalCode: addrPostalCode.trim(),
        }
      : undefined;

    const input: ApplicationInput = {
      entityId: data.entityId, entityName,
      productId: data.productId, productName: product?.name || "",
      planId: data.planId, planName: plan?.name || "",
      applicant: { firstName: data.firstName, lastName: data.lastName, documentType: data.documentType as any, documentNumber: data.documentNumber, email: data.email, phoneCode: data.phoneCode, phone: data.phone },
      status: "draft", currentWorkflowStateId: "ws1", currentWorkflowStateName: "Borrador",
      assignedUserId: data.assignedUserId || undefined,
      assignedUserName: assignedUser ? `${assignedUser.firstName} ${assignedUser.lastName}` : undefined,
    };

    if (isEdit) {
      updateApplication(app!.id, { ...input, applicant: input.applicant, address });
      toast.success("Solicitud actualizada");
      navigate(`/solicitudes/${app!.id}`);
    } else {
      const newApp = addApplication(input);
      if (address) updateApplication(newApp.id, { address });
      toast.success("Solicitud creada");
      navigate(`/solicitudes/${newApp.id}`);
    }
  };

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate(isEdit ? `/solicitudes/${id}` : "/solicitudes")}><ArrowLeft className="h-4 w-4" /></Button>
        <h1 className="text-2xl font-bold tracking-tight">{isEdit ? "Editar Solicitud" : "Nueva Solicitud"}</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Producto</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-1.5">
                <Label>Entidad *</Label>
                <Select defaultValue={app?.entityId} onValueChange={v => { setValue("entityId", v, { shouldValidate: true }); setValue("productId", ""); setValue("planId", ""); }}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger>
                  <SelectContent>{entities.filter(e => e.status === 'active').map(e => <SelectItem key={e.id} value={e.id}>{e.name}</SelectItem>)}</SelectContent>
                </Select>
                {errors.entityId && <p className="text-xs text-destructive">{errors.entityId.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Producto *</Label>
                <Select defaultValue={app?.productId} onValueChange={v => { setValue("productId", v, { shouldValidate: true }); setValue("planId", ""); }}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger>
                  <SelectContent>{entityProducts.map(p => <SelectItem key={p.id} value={p.id}>{p.name}</SelectItem>)}</SelectContent>
                </Select>
                {errors.productId && <p className="text-xs text-destructive">{errors.productId.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Plan *</Label>
                <Select defaultValue={app?.planId} onValueChange={v => setValue("planId", v, { shouldValidate: true })}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger>
                  <SelectContent>{(selectedProduct?.plans || []).map(p => <SelectItem key={p.id} value={p.id}>{p.name}</SelectItem>)}</SelectContent>
                </Select>
                {errors.planId && <p className="text-xs text-destructive">{errors.planId.message}</p>}
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Datos del Solicitante</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5"><Label>Nombre *</Label><Input {...register("firstName")} />{errors.firstName && <p className="text-xs text-destructive">{errors.firstName.message}</p>}</div>
              <div className="space-y-1.5"><Label>Apellido *</Label><Input {...register("lastName")} />{errors.lastName && <p className="text-xs text-destructive">{errors.lastName.message}</p>}</div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-1.5">
                <Label>Tipo Doc *</Label>
                <Select defaultValue={app?.applicant.documentType || "DNI"} onValueChange={v => setValue("documentType", v as any)}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent><SelectItem value="DNI">DNI</SelectItem><SelectItem value="CUIT">CUIT</SelectItem><SelectItem value="passport">Pasaporte</SelectItem></SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5 md:col-span-2"><Label>Número *</Label><Input {...register("documentNumber")} />{errors.documentNumber && <p className="text-xs text-destructive">{errors.documentNumber.message}</p>}</div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5"><Label>Email *</Label><Input {...register("email")} type="email" />{errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}</div>
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

        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Domicilio</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Calle</Label>
                <Input value={addrStreet} onChange={e => setAddrStreet(e.target.value)} placeholder="Nombre de la calle" />
              </div>
              <div className="grid grid-cols-3 gap-3">
                <div className="space-y-1.5">
                  <Label>Número</Label>
                  <Input value={addrNumber} onChange={e => setAddrNumber(e.target.value)} placeholder="1234" />
                </div>
                <div className="space-y-1.5">
                  <Label>Piso</Label>
                  <Input value={addrFloor} onChange={e => setAddrFloor(e.target.value)} placeholder="5" />
                </div>
                <div className="space-y-1.5">
                  <Label>Dpto</Label>
                  <Input value={addrApartment} onChange={e => setAddrApartment(e.target.value)} placeholder="B" />
                </div>
              </div>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-1.5">
                <Label>Provincia</Label>
                <Select value={addrProvince} onValueChange={v => { setAddrProvince(v); setAddrCity(""); }}>
                  <SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger>
                  <SelectContent>
                    {provinceOptions.map(p => <SelectItem key={p.code} value={p.label}>{p.label}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5">
                <Label>Ciudad</Label>
                <Select value={addrCity} onValueChange={setAddrCity} disabled={!addrProvince}>
                  <SelectTrigger><SelectValue placeholder={addrProvince ? "Seleccionar..." : "Seleccione provincia"} /></SelectTrigger>
                  <SelectContent>
                    {cityOptions.map(c => <SelectItem key={c.code} value={c.label}>{c.label}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5">
                <Label>Código Postal</Label>
                <Input value={addrPostalCode} onChange={e => setAddrPostalCode(e.target.value)} placeholder="C1043AAZ" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Asignación</CardTitle></CardHeader>
          <CardContent>
            <div className="space-y-1.5 max-w-sm">
              <Label>Asignar a usuario</Label>
              <Select defaultValue={app?.assignedUserId || ""} onValueChange={v => setValue("assignedUserId", v)}>
                <SelectTrigger><SelectValue placeholder="Sin asignar" /></SelectTrigger>
                <SelectContent>{entityUsers.map(u => <SelectItem key={u.id} value={u.id}>{u.firstName} {u.lastName}</SelectItem>)}</SelectContent>
              </Select>
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate(isEdit ? `/solicitudes/${id}` : "/solicitudes")}>Cancelar</Button>
          <Button type="submit">{isEdit ? "Guardar Cambios" : "Crear Solicitud"}</Button>
        </div>
      </form>
    </div>
  );
}
