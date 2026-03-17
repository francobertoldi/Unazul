import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, DollarSign, Building2, FileCheck, User, Package } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { useApplicationStore, useProductStore, useCommissionPlanStore, useEntityStore } from "@/data/store";
import { ApplicationStatus, CommissionPlan, CommissionValueType } from "@/data/types";
import { useMemo } from "react";
import { calculateCommission } from "./CommissionSettlementList";
import { Separator } from "@/components/ui/separator";

const statusLabels: Record<ApplicationStatus, string> = {
  draft: "Borrador", pending: "Pendiente", in_review: "En Revisión",
  approved: "Aprobada", rejected: "Rechazada", cancelled: "Cancelada", settled: "Liquidado",
};
const statusColors: Record<ApplicationStatus, string> = {
  draft: "bg-muted text-muted-foreground", pending: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200",
  in_review: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200",
  approved: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200",
  rejected: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200",
  cancelled: "bg-muted text-muted-foreground",
  settled: "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200",
};

const valueTypeLabels: Record<CommissionValueType, string> = {
  fixed_per_sale: "Monto fijo por venta",
  percentage_capital: "Porcentaje del capital",
  percentage_total_loan: "Porcentaje del préstamo total",
};

function fmtCurrency(v: number, currency: string = "ARS") {
  const prefix = currency === "USD" ? "US$" : "$";
  return `${prefix} ${v.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

export default function CommissionSettlementDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { applications } = useApplicationStore();
  const { products } = useProductStore();
  const { commissionPlans } = useCommissionPlanStore();
  const { entities } = useEntityStore();

  const app = applications.find(a => a.id === id);

  const data = useMemo(() => {
    if (!app) return null;
    const product = products.find(p => p.id === app.productId);
    const plan = product?.plans.find(pl => pl.id === app.planId);
    const cp = plan ? commissionPlans.find(c => c.id === plan.commissionPlanId) : undefined;
    const entity = entities.find(e => e.id === app.entityId);
    const commission = calculateCommission(plan, cp);
    return { product, plan, cp, entity, commission };
  }, [app, products, commissionPlans, entities]);

  if (!app || !data) {
    return <div className="flex items-center justify-center h-64"><p className="text-muted-foreground">Solicitud no encontrada</p></div>;
  }

  const { product, plan, cp, entity, commission } = data;
  const currency = plan?.currency || "ARS";

  // Build calculation explanation
  const getCalculationDetail = () => {
    if (!plan || !cp) return "Sin plan de comisión asignado";
    switch (cp.valueType) {
      case "fixed_per_sale":
        return `Monto fijo: ${fmtCurrency(cp.value, currency)}${cp.maxAmount ? ` (tope: ${fmtCurrency(cp.maxAmount, currency)})` : ""}`;
      case "percentage_capital":
        return `${cp.value}% × Capital (${fmtCurrency(plan.price, currency)}) = ${fmtCurrency((cp.value / 100) * plan.price, currency)}${cp.maxAmount ? ` → Tope: ${fmtCurrency(cp.maxAmount, currency)}` : ""}`;
      case "percentage_total_loan": {
        const total = plan.price * (plan.installments || 1);
        return `${cp.value}% × Total préstamo (${fmtCurrency(total, currency)}) = ${fmtCurrency((cp.value / 100) * total, currency)}${cp.maxAmount ? ` → Tope: ${fmtCurrency(cp.maxAmount, currency)}` : ""}`;
      }
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/liquidacion-comisiones")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center">
              <DollarSign className="h-5 w-5 text-primary" />
            </div>
            <div>
              <h1 className="text-xl font-bold tracking-tight">Comisión — {app.code}</h1>
              <p className="text-sm text-muted-foreground">{app.entityName} · {app.productName}</p>
            </div>
            <span className={`text-xs px-2.5 py-1 rounded-full font-medium ${statusColors[app.status]}`}>
              {statusLabels[app.status]}
            </span>
          </div>
        </div>
      </div>

      {/* Commission highlight */}
      <Card className="border-primary/30 bg-primary/5">
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium text-muted-foreground">Comisión a Cobrar</p>
              <p className="text-3xl font-bold text-primary">{fmtCurrency(commission, currency)}</p>
              <p className="text-xs text-muted-foreground mt-1">{getCalculationDetail()}</p>
            </div>
            <div className="h-16 w-16 rounded-2xl bg-primary/10 flex items-center justify-center">
              <DollarSign className="h-8 w-8 text-primary" />
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Solicitud */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              <FileCheck className="h-4 w-4 text-primary" /> Datos de la Solicitud
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4">
              {[
                { label: "Código", value: app.code },
                { label: "Estado", value: statusLabels[app.status] },
                { label: "Fecha Creación", value: new Date(app.createdAt).toLocaleDateString("es-AR") },
                { label: "Última Actualización", value: new Date(app.updatedAt).toLocaleDateString("es-AR") },
                { label: "Etapa Workflow", value: app.currentWorkflowStateName },
                { label: "Asignado a", value: app.assignedUserName || "—" },
              ].map(item => (
                <div key={item.label}>
                  <p className="text-xs text-muted-foreground">{item.label}</p>
                  <p className="text-sm font-medium">{item.value}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Solicitante */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              <User className="h-4 w-4 text-primary" /> Solicitante
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-4">
              {[
                { label: "Nombre", value: `${app.applicant.firstName} ${app.applicant.lastName}` },
                { label: "Documento", value: `${app.applicant.documentType} ${app.applicant.documentNumber}` },
                { label: "Email", value: app.applicant.email },
                { label: "Teléfono", value: `${app.applicant.phoneCode} ${app.applicant.phone}` },
                { label: "Ocupación", value: app.applicant.occupation || "—" },
                { label: "Fecha Nacimiento", value: app.applicant.birthDate ? new Date(app.applicant.birthDate).toLocaleDateString("es-AR") : "—" },
              ].map(item => (
                <div key={item.label}>
                  <p className="text-xs text-muted-foreground">{item.label}</p>
                  <p className="text-sm font-medium">{item.value}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Entidad */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              <Building2 className="h-4 w-4 text-primary" /> Entidad
            </CardTitle>
          </CardHeader>
          <CardContent>
            {entity ? (
              <div className="grid grid-cols-2 gap-4">
                {[
                  { label: "Nombre", value: entity.name },
                  { label: "CUIT", value: entity.identifier },
                  { label: "Tipo", value: { bank: "Banco", insurance: "Aseguradora", fintech: "Fintech", cooperative: "Cooperativa" }[entity.type] },
                  { label: "Email", value: entity.email },
                  { label: "Teléfono", value: `${entity.phoneCode} ${entity.phone}` },
                  { label: "Dirección", value: `${entity.address}, ${entity.city}` },
                  { label: "Provincia", value: entity.province },
                  { label: "País", value: entity.country },
                ].map(item => (
                  <div key={item.label}>
                    <p className="text-xs text-muted-foreground">{item.label}</p>
                    <p className="text-sm font-medium">{item.value}</p>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">Entidad no encontrada</p>
            )}
          </CardContent>
        </Card>

        {/* Producto y Plan */}
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              <Package className="h-4 w-4 text-primary" /> Producto y Plan
            </CardTitle>
          </CardHeader>
          <CardContent>
            {product && plan ? (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  {[
                    { label: "Producto", value: product.name },
                    { label: "Código", value: product.code },
                    { label: "Familia", value: product.familyName },
                    { label: "Plan", value: plan.name },
                    { label: "Descripción", value: plan.description },
                    { label: "Precio", value: fmtCurrency(plan.price, plan.currency) },
                    { label: "Moneda", value: plan.currency },
                    ...(plan.installments ? [{ label: "Cuotas", value: `${plan.installments}` }] : []),
                    ...(plan.otherCosts ? [{ label: "Otros Costos", value: fmtCurrency(plan.otherCosts, plan.currency) }] : []),
                  ].map(item => (
                    <div key={item.label}>
                      <p className="text-xs text-muted-foreground">{item.label}</p>
                      <p className="text-sm font-medium">{item.value}</p>
                    </div>
                  ))}
                </div>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">Producto no encontrado</p>
            )}
          </CardContent>
        </Card>

        {/* Plan de Comisión */}
        <Card className="lg:col-span-2">
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              <DollarSign className="h-4 w-4 text-primary" /> Plan de Comisión y Cálculo
            </CardTitle>
          </CardHeader>
          <CardContent>
            {cp ? (
              <div className="space-y-4">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {[
                    { label: "Código Plan", value: cp.code },
                    { label: "Descripción", value: cp.description },
                    { label: "Tipo", value: valueTypeLabels[cp.valueType] },
                    { label: "Valor", value: cp.valueType === "fixed_per_sale" ? fmtCurrency(cp.value, currency) : `${cp.value}%` },
                    ...(cp.maxAmount ? [{ label: "Monto Máximo", value: fmtCurrency(cp.maxAmount, currency) }] : []),
                  ].map(item => (
                    <div key={item.label}>
                      <p className="text-xs text-muted-foreground">{item.label}</p>
                      <p className="text-sm font-medium">{item.value}</p>
                    </div>
                  ))}
                </div>

                <Separator />

                <div className="bg-muted/50 rounded-lg p-4">
                  <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2">Detalle del Cálculo</p>
                  <p className="text-sm">{getCalculationDetail()}</p>
                  <div className="mt-3 flex items-center gap-2">
                    <Badge variant="default" className="text-sm px-3 py-1">
                      Resultado: {fmtCurrency(commission, currency)}
                    </Badge>
                  </div>
                </div>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">Sin plan de comisión asignado</p>
            )}
          </CardContent>
        </Card>

        {/* Domicilio */}
        {app.address && (
          <Card className="lg:col-span-2">
            <CardHeader className="pb-3">
              <CardTitle className="text-base">Domicilio del Solicitante</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {[
                  { label: "Calle", value: `${app.address.street} ${app.address.number}` },
                  ...(app.address.floor ? [{ label: "Piso / Depto", value: `${app.address.floor} ${app.address.apartment || ""}` }] : []),
                  { label: "Ciudad", value: app.address.city },
                  { label: "Provincia", value: app.address.province },
                  { label: "Código Postal", value: app.address.postalCode },
                ].map(item => (
                  <div key={item.label}>
                    <p className="text-xs text-muted-foreground">{item.label}</p>
                    <p className="text-sm font-medium">{item.value}</p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
