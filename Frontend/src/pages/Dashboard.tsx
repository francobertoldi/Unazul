import { useMemo, useState } from "react";
import { Building2, Users, ClipboardList, Package, FileCheck, CalendarIcon, Landmark } from "lucide-react";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from "recharts";
import { MetricCard } from "@/components/shared/MetricCard";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { useEntityStore, useUserStore, useProductStore, useApplicationStore, useTenantStore } from "@/data/store";
import { mockAuditLog } from "@/data/mock";
import { format, startOfMonth, endOfMonth } from "date-fns";
import { es } from "date-fns/locale";
import { cn } from "@/lib/utils";

const STATUS_LABELS: Record<string, string> = {
  draft: "Borrador", pending: "Pendiente", in_review: "En Revisión",
  approved: "Aprobada", rejected: "Rechazada", cancelled: "Cancelada",
};

const STATUS_COLORS: Record<string, string> = {
  draft: "hsl(220 9% 46%)", pending: "hsl(38 92% 50%)", in_review: "hsl(221 83% 53%)",
  approved: "hsl(142 71% 45%)", rejected: "hsl(0 72% 51%)", cancelled: "hsl(262 83% 58%)",
};

/** Obtiene el "valor representativo" de un plan según el tipo de producto (familia) */
function getPlanValue(plan: { price: number; coverages: { sumInsured?: number; premium?: number }[]; loanAttributes?: { annualEffectiveRate: number }; cardAttributes?: { creditLimit: number }; accountAttributes?: { maintenanceFee: number }; investmentAttributes?: { minimumAmount: number } }): number {
  if (plan.loanAttributes) return plan.price || 0;
  if (plan.cardAttributes) return plan.cardAttributes.creditLimit;
  if (plan.accountAttributes) return plan.accountAttributes.maintenanceFee;
  if (plan.investmentAttributes) return plan.investmentAttributes.minimumAmount;
  // Seguros: sumar suma asegurada de coberturas
  const totalSumInsured = plan.coverages.reduce((s, c) => s + (c.sumInsured || 0), 0);
  if (totalSumInsured > 0) return totalSumInsured;
  return plan.price || 0;
}

export default function Dashboard() {
  const { entities } = useEntityStore();
  const { users } = useUserStore();
  const { products } = useProductStore();
  const { applications } = useApplicationStore();
  const { tenants } = useTenantStore();

  const activeEntities = entities.filter(e => e.status === "active").length;
  const activeUsers = users.filter(u => u.status === "active").length;
  const activeProducts = products.filter(p => p.status === "active").length;
  const pendingApps = applications.filter(a => ["draft", "pending", "in_review"].includes(a.status)).length;

  const entityCharts = useMemo(() => {
    const map = new Map<string, { entityName: string; products: Map<string, { name: string; count: number; value: number }> }>();
    for (const prod of products) {
      if (!map.has(prod.entityId)) map.set(prod.entityId, { entityName: prod.entityName, products: new Map() });
      const entry = map.get(prod.entityId)!;
      const existing = entry.products.get(prod.id) ?? { name: prod.name, count: 0, value: 0 };
      existing.count += 1;
      for (const plan of prod.plans) {
        existing.value += getPlanValue(plan as any);
      }
      entry.products.set(prod.id, existing);
    }
    return [...map.entries()].map(([id, data]) => ({
      entityId: id,
      entityName: data.entityName,
      chartData: [...data.products.values()],
    }));
  }, [products]);

  const orgCharts = useMemo(() => {
    const entityTenantMap = new Map(entities.map(e => [e.id, { tenantId: e.tenantId, tenantName: e.tenantName }]));
    const map = new Map<string, { orgName: string; statusCounts: Map<string, number>; totalAmount: number; entityBreakdown: Map<string, { entityName: string; count: number; amount: number }> }>();

    for (const app of applications) {
      const tenant = entityTenantMap.get(app.entityId);
      if (!tenant) continue;
      if (!map.has(tenant.tenantId)) {
        map.set(tenant.tenantId, { orgName: tenant.tenantName, statusCounts: new Map(), totalAmount: 0, entityBreakdown: new Map() });
      }
      const entry = map.get(tenant.tenantId)!;
      entry.statusCounts.set(app.status, (entry.statusCounts.get(app.status) || 0) + 1);
      const product = products.find(p => p.id === app.productId);
      const plan = product?.plans.find(pl => pl.id === app.planId);
      const amount = plan ? getPlanValue(plan as any) : 0;
      entry.totalAmount += amount;
      if (!entry.entityBreakdown.has(app.entityId)) {
        entry.entityBreakdown.set(app.entityId, { entityName: app.entityName, count: 0, amount: 0 });
      }
      const eb = entry.entityBreakdown.get(app.entityId)!;
      eb.count += 1;
      eb.amount += amount;
    }

    return [...map.entries()].map(([id, data]) => ({
      orgId: id,
      orgName: data.orgName,
      statusData: [...data.statusCounts.entries()].map(([status, count]) => ({ status: STATUS_LABELS[status] || status, count })),
      entityData: [...data.entityBreakdown.values()],
      totalAmount: data.totalAmount,
      totalApps: [...data.statusCounts.values()].reduce((a, b) => a + b, 0),
    }));
  }, [applications, entities, products]);

  const formatCurrency = (v: number) =>
    v >= 1_000_000 ? `$${(v / 1_000_000).toFixed(1)}M` : v >= 1_000 ? `$${(v / 1_000).toFixed(0)}K` : `$${v}`;

  const PRODUCT_COLORS = ["hsl(221 83% 53%)", "hsl(142 71% 45%)", "hsl(38 92% 50%)", "hsl(0 72% 51%)", "hsl(262 83% 58%)"];

  const now = new Date();
  const [dateFrom, setDateFrom] = useState<Date>(startOfMonth(now));
  const [dateTo, setDateTo] = useState<Date>(endOfMonth(now));

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground text-sm mt-1">Resumen general de la plataforma multi-entidad</p>
      </div>

      {/* 1. Análisis por Organización */}
      <Card>
        <CardHeader className="pb-4">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <CardTitle className="text-lg font-semibold">Análisis por Organización</CardTitle>
              <CardDescription className="text-xs mt-1">Consolidado de solicitudes por entidad dentro de cada organización</CardDescription>
            </div>
            <div className="flex flex-wrap items-center gap-3">
              <span className="text-sm font-medium text-muted-foreground">Rango:</span>
              <Popover>
                <PopoverTrigger asChild>
                  <Button variant="outline" size="sm" className={cn("w-[160px] justify-start text-left font-normal", !dateFrom && "text-muted-foreground")}>
                    <CalendarIcon className="mr-2 h-3.5 w-3.5" />
                    {dateFrom ? format(dateFrom, "dd/MM/yyyy", { locale: es }) : "Desde"}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar mode="single" selected={dateFrom} onSelect={(d) => d && setDateFrom(d)} initialFocus className={cn("p-3 pointer-events-auto")} />
                </PopoverContent>
              </Popover>
              <span className="text-sm text-muted-foreground">—</span>
              <Popover>
                <PopoverTrigger asChild>
                  <Button variant="outline" size="sm" className={cn("w-[160px] justify-start text-left font-normal", !dateTo && "text-muted-foreground")}>
                    <CalendarIcon className="mr-2 h-3.5 w-3.5" />
                    {dateTo ? format(dateTo, "dd/MM/yyyy", { locale: es }) : "Hasta"}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar mode="single" selected={dateTo} onSelect={(d) => d && setDateTo(d)} initialFocus className={cn("p-3 pointer-events-auto")} />
                </PopoverContent>
              </Popover>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 grid-cols-1 lg:grid-cols-2">
            {orgCharts.map((oc) => (
              <Card key={oc.orgId}>
                <CardHeader className="pb-2">
                  <div className="flex items-center gap-2">
                    <Landmark className="h-4 w-4 text-primary" />
                    <CardTitle className="text-base font-semibold">{oc.orgName}</CardTitle>
                  </div>
                  <CardDescription className="text-xs">{oc.totalApps} solicitudes · {formatCurrency(oc.totalAmount)} valor total</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="h-[260px] w-full">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={oc.entityData} margin={{ top: 5, right: 10, left: 5, bottom: 5 }}>
                        <CartesianGrid strokeDasharray="3 3" className="stroke-border/50" />
                        <XAxis dataKey="entityName" tick={{ fontSize: 10 }} className="fill-muted-foreground" angle={-15} textAnchor="end" height={45} />
                        <YAxis yAxisId="left" orientation="left" tick={{ fontSize: 10 }} className="fill-muted-foreground" label={{ value: "Cant.", angle: -90, position: "insideLeft", style: { fontSize: 9 } }} />
                        <YAxis yAxisId="right" orientation="right" tickFormatter={formatCurrency} tick={{ fontSize: 10 }} className="fill-muted-foreground" label={{ value: "Monto", angle: 90, position: "insideRight", style: { fontSize: 9 } }} />
                        <Tooltip contentStyle={{ borderRadius: 8, border: "1px solid hsl(var(--border))", background: "hsl(var(--background))", fontSize: 12 }} formatter={(value: number, name: string) => [name === "amount" ? formatCurrency(value) : value, name === "amount" ? "Monto" : "Cantidad"]} />
                        <Legend formatter={(v: string) => (v === "amount" ? "Monto" : "Cantidad")} />
                        <Bar yAxisId="left" dataKey="count" fill="hsl(221 83% 53%)" radius={[4, 4, 0, 0]} />
                        <Bar yAxisId="right" dataKey="amount" fill="hsl(221 83% 53%)" fillOpacity={0.35} radius={[4, 4, 0, 0]} />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* 2. Análisis por Entidad */}
      <Card>
        <CardHeader className="pb-4">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <CardTitle className="text-lg font-semibold">Análisis por Entidad</CardTitle>
              <CardDescription className="text-xs mt-1">Productos: cantidad y valor representativo por entidad</CardDescription>
            </div>
            <div className="flex flex-wrap items-center gap-3">
              <span className="text-sm font-medium text-muted-foreground">Rango:</span>
              <Popover>
                <PopoverTrigger asChild>
                  <Button variant="outline" size="sm" className={cn("w-[160px] justify-start text-left font-normal", !dateFrom && "text-muted-foreground")}>
                    <CalendarIcon className="mr-2 h-3.5 w-3.5" />
                    {dateFrom ? format(dateFrom, "dd/MM/yyyy", { locale: es }) : "Desde"}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar mode="single" selected={dateFrom} onSelect={(d) => d && setDateFrom(d)} initialFocus className={cn("p-3 pointer-events-auto")} />
                </PopoverContent>
              </Popover>
              <span className="text-sm text-muted-foreground">—</span>
              <Popover>
                <PopoverTrigger asChild>
                  <Button variant="outline" size="sm" className={cn("w-[160px] justify-start text-left font-normal", !dateTo && "text-muted-foreground")}>
                    <CalendarIcon className="mr-2 h-3.5 w-3.5" />
                    {dateTo ? format(dateTo, "dd/MM/yyyy", { locale: es }) : "Hasta"}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <Calendar mode="single" selected={dateTo} onSelect={(d) => d && setDateTo(d)} initialFocus className={cn("p-3 pointer-events-auto")} />
                </PopoverContent>
              </Popover>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 grid-cols-1 lg:grid-cols-2">
            {entityCharts.map((ec, idx) => (
              <Card key={ec.entityId}>
                <CardHeader className="pb-2">
                  <CardTitle className="text-base font-semibold">{ec.entityName}</CardTitle>
                  <CardDescription className="text-xs">Productos: cantidad y valor representativo</CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="h-[280px] w-full">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart data={ec.chartData} margin={{ top: 10, right: 20, left: 10, bottom: 5 }}>
                        <CartesianGrid strokeDasharray="3 3" className="stroke-border/50" />
                        <XAxis dataKey="name" tick={{ fontSize: 11 }} className="fill-muted-foreground" angle={-20} textAnchor="end" height={50} />
                        <YAxis yAxisId="left" orientation="left" tick={{ fontSize: 11 }} className="fill-muted-foreground" label={{ value: "Cant.", angle: -90, position: "insideLeft", style: { fontSize: 10 } }} />
                        <YAxis yAxisId="right" orientation="right" tickFormatter={formatCurrency} tick={{ fontSize: 11 }} className="fill-muted-foreground" label={{ value: "Valor", angle: 90, position: "insideRight", style: { fontSize: 10 } }} />
                        <Tooltip contentStyle={{ borderRadius: 8, border: "1px solid hsl(var(--border))", background: "hsl(var(--background))", fontSize: 12 }} formatter={(value: number, name: string) => [name === "value" ? formatCurrency(value) : value, name === "value" ? "Valor" : "Cantidad"]} />
                        <Legend formatter={(v: string) => (v === "value" ? "Valor" : "Cantidad")} />
                        <Bar yAxisId="left" dataKey="count" fill={PRODUCT_COLORS[idx % PRODUCT_COLORS.length]} radius={[4, 4, 0, 0]} />
                        <Bar yAxisId="right" dataKey="value" fill={PRODUCT_COLORS[idx % PRODUCT_COLORS.length]} fillOpacity={0.4} radius={[4, 4, 0, 0]} />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* 3. Métricas */}
      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard title="Entidades Activas" value={activeEntities} subtitle={`${entities.length} totales`} icon={Building2} trend={{ value: "+1 este mes", positive: true }} />
        <MetricCard title="Usuarios Activos" value={activeUsers} subtitle={`${users.length} registrados`} icon={Users} />
        <MetricCard title="Productos Activos" value={activeProducts} subtitle={`${products.length} en catálogo`} icon={Package} />
        <MetricCard title="Solicitudes Pendientes" value={pendingApps} subtitle={`${applications.length} totales`} icon={FileCheck} />
      </div>

      {/* 4. Actividad Reciente - ancho completo */}
      <Card>
        <CardHeader className="pb-3"><CardTitle className="text-base font-semibold">Actividad Reciente</CardTitle></CardHeader>
        <CardContent className="space-y-3">
          {mockAuditLog.slice(0, 5).map(entry => (
            <div key={entry.id} className="flex items-start gap-3 text-sm">
              <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center shrink-0 mt-0.5"><ClipboardList className="h-3.5 w-3.5 text-primary" /></div>
              <div className="flex-1 min-w-0">
                <p className="font-medium truncate">{entry.detail}</p>
                <p className="text-xs text-muted-foreground">{entry.userName} · {new Date(entry.timestamp).toLocaleString("es-AR", { dateStyle: "short", timeStyle: "short" })}</p>
              </div>
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  );
}
