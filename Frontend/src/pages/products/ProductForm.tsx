import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft, Plus, Pencil, Trash2, Shield, FileText, ShieldCheck, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { StatusBadge } from "@/components/shared/StatusBadge";
import {
  useProductStore,
  useEntityStore,
  useProductFamilyStore,
  useCommissionPlanStore,
  useParameterStore,
  ProductInput,
} from "@/data/store";
import { ProductPlan, ProductRequirement, AmortizationType, LoanAttributes, CommissionPlan, InsuranceAttributes, AccountAttributes, CardAttributes, InvestmentAttributes, RiskLevel, InstrumentType, Coverage } from "@/data/types";
import { toast } from "sonner";

const schema = z.object({
  name: z.string().trim().min(2, "Mínimo 2 caracteres").max(100),
  code: z.string().trim().min(2, "Mínimo 2 caracteres").max(20),
  entityId: z.string().min(1, "Seleccione una entidad"),
  familyId: z.string().min(1, "Seleccione una familia"),
  description: z.string().trim().min(5, "Mínimo 5 caracteres").max(500),
  status: z.enum(["draft", "active", "inactive", "deprecated"]),
  validFrom: z.string().min(1, "Requerido"),
  validTo: z.string().optional(),
});

const statusLabels: Record<string, string> = { draft: "Borrador", active: "Activo", inactive: "Inactivo", deprecated: "Deprecado" };

// ── Sub producto Dialog ──
function PlanDialog({
  open,
  onOpenChange,
  onSave,
  initial,
  commissionPlans,
  blockedCommissionPlanIds,
  familyCode,
  cardNetworkOptions,
  cardLevelOptions,
  insuranceCoverageOptions,
}: {
  open: boolean;
  onOpenChange: (o: boolean) => void;
  onSave: (data: Omit<ProductPlan, "id" | "productId">) => void;
  initial?: ProductPlan | null;
  commissionPlans: CommissionPlan[];
  blockedCommissionPlanIds: Set<string>;
  familyCode?: string;
  cardNetworkOptions: { code: string; label: string }[];
  cardLevelOptions: { code: string; label: string }[];
  insuranceCoverageOptions: { code: string; label: string }[];
}) {
  const [name, setName] = useState(initial?.name || "");
  const [description, setDescription] = useState(initial?.description || "");
  const [currency, setCurrency] = useState(initial?.currency || "ARS");
  const [installments, setInstallments] = useState(initial?.installments?.toString() || "1");
  const [status, setStatus] = useState<ProductPlan["status"]>(initial?.status || "draft");
  const [commissionPlanId, setCommissionPlanId] = useState(initial?.commissionPlanId || "");
  const [otherCosts, setOtherCosts] = useState(initial?.otherCosts?.toString() || "0");

  // Loan-specific fields
  const [amortizationType, setAmortizationType] = useState<AmortizationType>(initial?.loanAttributes?.amortizationType || "french");
  const [annualEffectiveRate, setAnnualEffectiveRate] = useState(initial?.loanAttributes?.annualEffectiveRate?.toString() || "0");
  const [cftRate, setCftRate] = useState(initial?.loanAttributes?.cftRate?.toString() || "0");
  const [adminFees, setAdminFees] = useState(initial?.loanAttributes?.adminFees?.toString() || "0");

  // Insurance-specific fields
  const [insurancePremium, setInsurancePremium] = useState(initial?.insuranceAttributes?.premium?.toString() || "0");
  const [insuranceSumInsured, setInsuranceSumInsured] = useState(initial?.insuranceAttributes?.sumInsured?.toString() || "0");
  const [insuranceGracePeriod, setInsuranceGracePeriod] = useState(initial?.insuranceAttributes?.gracePeriodDays?.toString() || "30");
  const [insuranceCoverageType, setInsuranceCoverageType] = useState<InsuranceAttributes['coverageType']>(initial?.insuranceAttributes?.coverageType || "individual");
  const [coverages, setCoverages] = useState<Coverage[]>(initial?.coverages || []);

  // Account-specific fields
  const [accountMaintenanceFee, setAccountMaintenanceFee] = useState(initial?.accountAttributes?.maintenanceFee?.toString() || "0");
  const [accountMinimumBalance, setAccountMinimumBalance] = useState(initial?.accountAttributes?.minimumBalance?.toString() || "0");
  const [accountInterestRate, setAccountInterestRate] = useState(initial?.accountAttributes?.interestRate?.toString() || "0");
  const [accountType, setAccountType] = useState<AccountAttributes['accountType']>(initial?.accountAttributes?.accountType || "savings");

  // Card-specific fields
  const [cardCreditLimit, setCardCreditLimit] = useState(initial?.cardAttributes?.creditLimit?.toString() || "0");
  const [cardAnnualFee, setCardAnnualFee] = useState(initial?.cardAttributes?.annualFee?.toString() || "0");
  const [cardInterestRate, setCardInterestRate] = useState(initial?.cardAttributes?.interestRate?.toString() || "0");
  const [cardNetwork, setCardNetwork] = useState(initial?.cardAttributes?.network || "visa" as string);
  const [cardLevel, setCardLevel] = useState(initial?.cardAttributes?.level || "");
  const [cardGracePeriod, setCardGracePeriod] = useState(initial?.cardAttributes?.gracePeriodDays?.toString() || "30");

  // Investment-specific fields
  const [investmentMinimumAmount, setInvestmentMinimumAmount] = useState(initial?.investmentAttributes?.minimumAmount?.toString() || "0");
  const [investmentExpectedReturn, setInvestmentExpectedReturn] = useState(initial?.investmentAttributes?.expectedReturn?.toString() || "0");
  const [investmentTermDays, setInvestmentTermDays] = useState(initial?.investmentAttributes?.termDays?.toString() || "30");
  const [investmentRiskLevel, setInvestmentRiskLevel] = useState<RiskLevel>(initial?.investmentAttributes?.riskLevel || "low");
  const [investmentInstrumentType, setInvestmentInstrumentType] = useState<InstrumentType>(initial?.investmentAttributes?.instrumentType || "fixed_term");


  useEffect(() => {
    setName(initial?.name || "");
    setDescription(initial?.description || "");
    setCurrency(initial?.currency || "ARS");
    setInstallments(initial?.installments?.toString() || "1");
    setStatus(initial?.status || "draft");
    setCommissionPlanId(initial?.commissionPlanId || "");
    setOtherCosts(initial?.otherCosts?.toString() || "0");

    setAmortizationType(initial?.loanAttributes?.amortizationType || "french");
    setAnnualEffectiveRate(initial?.loanAttributes?.annualEffectiveRate?.toString() || "0");
    setCftRate(initial?.loanAttributes?.cftRate?.toString() || "0");
    setAdminFees(initial?.loanAttributes?.adminFees?.toString() || "0");
    
    setInsurancePremium(initial?.insuranceAttributes?.premium?.toString() || "0");
    setInsuranceSumInsured(initial?.insuranceAttributes?.sumInsured?.toString() || "0");
    setInsuranceGracePeriod(initial?.insuranceAttributes?.gracePeriodDays?.toString() || "30");
    setInsuranceCoverageType(initial?.insuranceAttributes?.coverageType || "individual");
    setCoverages(initial?.coverages || []);

    setAccountMaintenanceFee(initial?.accountAttributes?.maintenanceFee?.toString() || "0");
    setAccountMinimumBalance(initial?.accountAttributes?.minimumBalance?.toString() || "0");
    setAccountInterestRate(initial?.accountAttributes?.interestRate?.toString() || "0");
    setAccountType(initial?.accountAttributes?.accountType || "savings");

    setCardCreditLimit(initial?.cardAttributes?.creditLimit?.toString() || "0");
    setCardAnnualFee(initial?.cardAttributes?.annualFee?.toString() || "0");
    setCardInterestRate(initial?.cardAttributes?.interestRate?.toString() || "0");
    setCardNetwork(initial?.cardAttributes?.network || "visa");
    setCardLevel(initial?.cardAttributes?.level || "");
    setCardGracePeriod(initial?.cardAttributes?.gracePeriodDays?.toString() || "30");

    setInvestmentMinimumAmount(initial?.investmentAttributes?.minimumAmount?.toString() || "0");
    setInvestmentExpectedReturn(initial?.investmentAttributes?.expectedReturn?.toString() || "0");
    setInvestmentTermDays(initial?.investmentAttributes?.termDays?.toString() || "30");
    setInvestmentRiskLevel(initial?.investmentAttributes?.riskLevel || "low");
    setInvestmentInstrumentType(initial?.investmentAttributes?.instrumentType || "fixed_term");
  }, [initial, open]);

  const getFamilyCategory = () => {
    if (!familyCode) return null;
    if (familyCode.startsWith('PREST')) return 'loan';
    if (familyCode.startsWith('SEG')) return 'insurance';
    if (familyCode.startsWith('CTA')) return 'account';
    if (familyCode.startsWith('TARJETA')) return 'card';
    if (familyCode.startsWith('INV')) return 'investment';
    return null;
  };

  const familyCategory = getFamilyCategory();

  const amortizationLabels: Record<AmortizationType, string> = {
    french: "Francés (cuota fija)",
    german: "Alemán (amortización fija)",
    american: "Americano (intereses periódicos)",
    bullet: "Bullet (pago al vencimiento)",
  };

  const selectableCommissionPlans = commissionPlans.filter(
    (cp) => cp.id === commissionPlanId || !blockedCommissionPlanIds.has(cp.id)
  );

  const handleSave = () => {
    if (!name.trim()) {
      toast.error("El nombre es requerido");
      return;
    }
    if (!commissionPlanId) {
      toast.error("Debe seleccionar un plan de comisiones");
      return;
    }
    if (blockedCommissionPlanIds.has(commissionPlanId) && commissionPlanId !== initial?.commissionPlanId) {
      toast.error("Ese plan de comisiones ya está asignado a otro sub producto");
      return;
    }

    const loanAttributes: LoanAttributes | undefined = familyCategory === 'loan'
      ? {
          amortizationType,
          annualEffectiveRate: parseFloat(annualEffectiveRate) || 0,
          cftRate: parseFloat(cftRate) || 0,
          adminFees: parseFloat(adminFees) || 0,
        }
      : undefined;

    const insuranceAttributes: InsuranceAttributes | undefined = familyCategory === 'insurance'
      ? {
          premium: parseFloat(insurancePremium) || 0,
          sumInsured: parseFloat(insuranceSumInsured) || 0,
          gracePeriodDays: parseInt(insuranceGracePeriod) || 30,
          coverageType: insuranceCoverageType,
        }
      : undefined;

    const accountAttributes: AccountAttributes | undefined = familyCategory === 'account'
      ? {
          maintenanceFee: parseFloat(accountMaintenanceFee) || 0,
          minimumBalance: parseFloat(accountMinimumBalance) || 0,
          interestRate: parseFloat(accountInterestRate) || 0,
          accountType: accountType,
        }
      : undefined;

    const cardAttributes: CardAttributes | undefined = familyCategory === 'card'
      ? {
          creditLimit: parseFloat(cardCreditLimit) || 0,
          annualFee: parseFloat(cardAnnualFee) || 0,
          interestRate: parseFloat(cardInterestRate) || 0,
          network: cardNetwork as import("@/data/types").CardNetwork,
          gracePeriodDays: parseInt(cardGracePeriod) || 30,
          level: cardLevel || undefined,
        }
      : undefined;

    const investmentAttributes: InvestmentAttributes | undefined = familyCategory === 'investment'
      ? {
          minimumAmount: parseFloat(investmentMinimumAmount) || 0,
          expectedReturn: parseFloat(investmentExpectedReturn) || 0,
          termDays: parseInt(investmentTermDays) || 30,
          riskLevel: investmentRiskLevel,
          instrumentType: investmentInstrumentType,
        }
      : undefined;

    onSave({
      name,
      description,
      price: 0,
      currency,
      installments: parseInt(installments) || 1,
      commissionPlanId,
      status,
      coverages: familyCategory === 'insurance' ? coverages : (initial?.coverages || []),
      otherCosts: parseFloat(otherCosts) || 0,
      loanAttributes,
      insuranceAttributes,
      accountAttributes,
      cardAttributes,
      investmentAttributes,
    });
    onOpenChange(false);
  };

  if (!open) return null;

  return (
    <Card className="mb-6">
      <CardHeader className="pb-3">
        <CardTitle className="text-base">{initial ? "Editar Sub producto" : "Nuevo Sub producto"}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-1.5">
          <Label>Nombre *</Label>
          <Input value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="space-y-1.5">
          <Label>Descripción</Label>
          <Textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={2} />
        </div>

        <div className="space-y-1.5">
          <Label>Plan de comisiones *</Label>
          <Select value={commissionPlanId} onValueChange={setCommissionPlanId}>
            <SelectTrigger>
              <SelectValue placeholder="Seleccionar..." />
            </SelectTrigger>
            <SelectContent>
              {selectableCommissionPlans.map((cp) => (
                <SelectItem key={cp.id} value={cp.id}>
                  {cp.code} — {cp.description}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {selectableCommissionPlans.length === 0 && (
            <p className="text-xs text-muted-foreground">No hay planes disponibles (cree uno en Configuración → Planes de comisiones).</p>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <Label>Cantidad de Cuotas</Label>
            <Input type="number" min="1" value={installments} onChange={(e) => setInstallments(e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>Moneda</Label>
            <Select value={currency} onValueChange={setCurrency}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="ARS">ARS</SelectItem>
                <SelectItem value="USD">USD</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
        <div className="space-y-1.5">
          <Label>Estado</Label>
          <Select value={status} onValueChange={(v) => setStatus(v as ProductPlan["status"])}>
            <SelectTrigger>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {Object.entries(statusLabels).map(([k, v]) => (
                <SelectItem key={k} value={k}>
                  {v}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>



        {/* Family-specific fields */}
        {familyCategory && (
          <div className="border-t pt-4 space-y-4">
            <span className="font-semibold text-muted-foreground uppercase tracking-wider text-xs">
              Atributos de {familyCategory === 'loan' ? 'Préstamo' : familyCategory === 'insurance' ? 'Seguro' : familyCategory === 'account' ? 'Cuenta' : familyCategory === 'card' ? 'Tarjeta' : 'Inversión'}
            </span>

            {familyCategory === 'loan' && (
              <div className="space-y-4">
                <div className="space-y-1.5">
                  <Label>Tipo de Amortización</Label>
                  <Select value={amortizationType} onValueChange={(v) => setAmortizationType(v as AmortizationType)}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(amortizationLabels).map(([k, v]) => (
                        <SelectItem key={k} value={k}>
                          {v}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>TEA (Tasa Efectiva Anual)</Label>
                    <div className="relative">
                      <Input type="number" step="0.01" min="0" max="999" value={annualEffectiveRate} onChange={(e) => { const v = e.target.value; if (parseFloat(v) <= 999 || v === '') setAnnualEffectiveRate(v); }} className="pr-7" />
                      <span className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">%</span>
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>CFT (Costo Financiero Total)</Label>
                    <div className="relative">
                      <Input type="number" step="0.01" min="0" max="999" value={cftRate} onChange={(e) => { const v = e.target.value; if (parseFloat(v) <= 999 || v === '') setCftRate(v); }} className="pr-7" />
                      <span className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">%</span>
                    </div>
                  </div>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>Costos Adm. de Emisión</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span>
                      <Input type="number" step="0.01" min="0" value={adminFees} onChange={(e) => setAdminFees(e.target.value)} className="pl-7" />
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>Otros Costos</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span>
                      <Input type="number" step="0.01" min="0" value={otherCosts} onChange={(e) => setOtherCosts(e.target.value)} className="pl-7" />
                    </div>
                  </div>
                </div>
              </div>
            )}

            {familyCategory === 'insurance' && (
              <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>Prima (Premium)</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span>
                      <Input type="number" step="0.01" min="0" value={insurancePremium} onChange={(e) => setInsurancePremium(e.target.value)} className="pl-7" />
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>Suma Asegurada</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span>
                      <Input type="number" step="0.01" min="0" value={insuranceSumInsured} onChange={(e) => setInsuranceSumInsured(e.target.value)} className="pl-7" />
                    </div>
                  </div>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>Días de Gracia</Label>
                    <Input type="number" value={insuranceGracePeriod} onChange={(e) => setInsuranceGracePeriod(e.target.value)} />
                  </div>
                  <div className="space-y-1.5">
                    <Label>Tipo de Cobertura</Label>
                    <Select value={insuranceCoverageType} onValueChange={(v) => setInsuranceCoverageType(v as InsuranceAttributes['coverageType'])}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="individual">Individual</SelectItem>
                        <SelectItem value="group">Grupo</SelectItem>
                        <SelectItem value="collective">Colectivo</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                {/* Inline Coverages CRUD */}
                <div className="border-t pt-4 space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="font-semibold text-muted-foreground uppercase tracking-wider text-xs">Coberturas ({coverages.length})</span>
                  </div>
                  {/* Add new coverage row */}
                  <div className="flex items-end gap-2">
                    <div className="flex-1 space-y-1.5">
                      <Label className="text-xs">Tipo de Cobertura</Label>
                      <Select
                        value=""
                        onValueChange={(covCode) => {
                          const opt = insuranceCoverageOptions.find(o => o.code === covCode);
                          if (!opt) return;
                          if (coverages.some(c => c.name === opt.label)) {
                            toast.error("Esa cobertura ya fue agregada");
                            return;
                          }
                          setCoverages([...coverages, {
                            id: `cov-${Date.now()}`,
                            name: opt.label,
                            description: opt.label,
                            sumInsured: 0,
                            premium: 0,
                          }]);
                        }}
                      >
                        <SelectTrigger><SelectValue placeholder="Agregar cobertura..." /></SelectTrigger>
                        <SelectContent>
                          {insuranceCoverageOptions
                            .filter(o => !coverages.some(c => c.name === o.label))
                            .map(o => (
                              <SelectItem key={o.code} value={o.code}>{o.label}</SelectItem>
                            ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                  {/* Coverage list */}
                  {coverages.length > 0 && (
                    <div className="space-y-2">
                      {coverages.map((cov, idx) => (
                        <div key={cov.id} className="flex items-center gap-2 rounded-md border bg-muted/30 px-3 py-2">
                          <ShieldCheck className="h-4 w-4 text-primary shrink-0" />
                          <span className="text-sm font-medium flex-1 min-w-0 truncate">{cov.name}</span>
                          <div className="flex items-center gap-2 shrink-0">
                            <div className="relative w-28">
                              <span className="absolute left-2 top-1/2 -translate-y-1/2 text-muted-foreground text-xs">$</span>
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                placeholder="Suma aseg."
                                value={cov.sumInsured || ""}
                                onChange={(e) => {
                                  const updated = [...coverages];
                                  updated[idx] = { ...updated[idx], sumInsured: parseFloat(e.target.value) || 0 };
                                  setCoverages(updated);
                                }}
                                className="h-8 text-xs pl-5"
                              />
                            </div>
                            <div className="relative w-28">
                              <span className="absolute left-2 top-1/2 -translate-y-1/2 text-muted-foreground text-xs">$</span>
                              <Input
                                type="number"
                                step="0.01"
                                min="0"
                                placeholder="Prima"
                                value={cov.premium || ""}
                                onChange={(e) => {
                                  const updated = [...coverages];
                                  updated[idx] = { ...updated[idx], premium: parseFloat(e.target.value) || 0 };
                                  setCoverages(updated);
                                }}
                                className="h-8 text-xs pl-5"
                              />
                            </div>
                            <Button
                              type="button"
                              variant="ghost"
                              size="icon"
                              className="h-7 w-7 text-destructive hover:text-destructive shrink-0"
                              onClick={() => setCoverages(coverages.filter((_, i) => i !== idx))}
                            >
                              <X className="h-3.5 w-3.5" />
                            </Button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                  {coverages.length === 0 && (
                    <p className="text-xs text-muted-foreground">No se han agregado coberturas aún.</p>
                  )}
                </div>
              </div>
            )}

            {familyCategory === 'account' && (
              <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>Comisión Mantenimiento</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span>
                      <Input type="number" step="0.01" value={accountMaintenanceFee} onChange={(e) => setAccountMaintenanceFee(e.target.value)} className="pl-7" />
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>Saldo Mínimo</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span>
                      <Input type="number" step="0.01" value={accountMinimumBalance} onChange={(e) => setAccountMinimumBalance(e.target.value)} className="pl-7" />
                    </div>
                  </div>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>Tasa Interés Anual</Label>
                    <div className="relative">
                      <Input 
                        type="number" 
                        step="0.01" 
                        min="0"
                        max="100"
                        value={accountInterestRate} 
                        onChange={(e) => {
                          const val = e.target.value;
                          if (val.replace('.', '').length <= 3) {
                            setAccountInterestRate(val);
                          }
                        }} 
                        className="pr-7"
                      />
                      <span className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">%</span>
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>Tipo de Cuenta</Label>
                    <Select value={accountType} onValueChange={(v) => setAccountType(v as AccountAttributes['accountType'])}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="savings">Caja de Ahorro</SelectItem>
                        <SelectItem value="checking">Cuenta Corriente</SelectItem>
                        <SelectItem value="money_market">Mercado de Dinero</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </div>
            )}

            {familyCategory === 'card' && (
              <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>Límite de Crédito</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span>
                      <Input type="number" step="0.01" value={cardCreditLimit} onChange={(e) => setCardCreditLimit(e.target.value)} className="pl-7" />
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>Red</Label>
                    <Select value={cardNetwork} onValueChange={setCardNetwork}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        {cardNetworkOptions.length > 0 ? cardNetworkOptions.map((opt) => (
                          <SelectItem key={opt.code} value={opt.code}>{opt.label}</SelectItem>
                        )) : (
                          <SelectItem value="visa" disabled>Sin redes configuradas</SelectItem>
                        )}
                      </SelectContent>
                    </Select>
                  </div>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div className="space-y-1.5">
                    <Label>Cuota Anual</Label>
                    <div className="relative">
                      <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span>
                      <Input type="number" step="0.01" value={cardAnnualFee} onChange={(e) => setCardAnnualFee(e.target.value)} className="pl-7" />
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>Tasa Interés</Label>
                    <div className="relative">
                      <Input 
                        type="number" 
                        step="0.01" 
                        min="0"
                        max="100"
                        value={cardInterestRate} 
                        onChange={(e) => {
                          const val = e.target.value;
                          if (val.replace('.', '').length <= 3) {
                            setCardInterestRate(val);
                          }
                        }} 
                        className="pr-7"
                      />
                      <span className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">%</span>
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>Días Gracia</Label>
                    <Input type="number" value={cardGracePeriod} onChange={(e) => setCardGracePeriod(e.target.value)} />
                  </div>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>Nivel</Label>
                    <Select value={cardLevel} onValueChange={setCardLevel}>
                      <SelectTrigger><SelectValue placeholder="Seleccione nivel" /></SelectTrigger>
                      <SelectContent>
                        {cardLevelOptions.length > 0 ? cardLevelOptions.map((opt) => (
                          <SelectItem key={opt.code} value={opt.code}>{opt.label}</SelectItem>
                        )) : (
                          <SelectItem value="_" disabled>Sin niveles configurados</SelectItem>
                        )}
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </div>
            )}

            {familyCategory === 'investment' && (
              <div className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-1.5">
                    <Label>Monto Mínimo</Label>
                    <Input type="number" step="0.01" value={investmentMinimumAmount} onChange={(e) => setInvestmentMinimumAmount(e.target.value)} />
                  </div>
                  <div className="space-y-1.5">
                    <Label>Instrumento</Label>
                    <Select value={investmentInstrumentType} onValueChange={(v) => setInvestmentInstrumentType(v as InstrumentType)}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="fixed_term">Plazo Fijo</SelectItem>
                        <SelectItem value="bond">Bono</SelectItem>
                        <SelectItem value="mutual_fund">Fondo Común</SelectItem>
                        <SelectItem value="stock">Acciones</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div className="space-y-1.5">
                    <Label>Retorno Esperado %</Label>
                    <Input type="number" step="0.01" value={investmentExpectedReturn} onChange={(e) => setInvestmentExpectedReturn(e.target.value)} />
                  </div>
                  <div className="space-y-1.5">
                    <Label>Plazo (Días)</Label>
                    <Input type="number" value={investmentTermDays} onChange={(e) => setInvestmentTermDays(e.target.value)} />
                  </div>
                  <div className="space-y-1.5">
                    <Label>Riesgo</Label>
                    <Select value={investmentRiskLevel} onValueChange={(v) => setInvestmentRiskLevel(v as RiskLevel)}>
                      <SelectTrigger><SelectValue /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="low">Bajo</SelectItem>
                        <SelectItem value="medium">Medio</SelectItem>
                        <SelectItem value="high">Alto</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </div>
            )}
          </div>
        )}

        <div className="flex justify-end gap-3 pt-2">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancelar
          </Button>
          <Button onClick={handleSave}>{initial ? "Guardar" : "Crear"}</Button>
        </div>
      </CardContent>
    </Card>
  );
}

// ── Requirement Dialog ──
function RequirementDialog({ open, onOpenChange, onSave, initial }: {
  open: boolean; onOpenChange: (o: boolean) => void;
  onSave: (data: Omit<ProductRequirement, 'id'>) => void;
  initial?: ProductRequirement | null;
}) {
  const [name, setName] = useState(initial?.name || "");
  const [type, setType] = useState<ProductRequirement["type"]>(initial?.type || "document");
  const [mandatory, setMandatory] = useState(initial?.mandatory ?? true);
  const [description, setDescription] = useState(initial?.description || "");

  const handleSave = () => {
    if (!name.trim()) { toast.error("El nombre es requerido"); return; }
    if (!description.trim()) { toast.error("La descripción es requerida"); return; }
    onSave({ name, type, mandatory, description });
    onOpenChange(false);
  };

  const typeLabels: Record<string, string> = { document: "Documento", data: "Dato", validation: "Validación" };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader><DialogTitle>{initial ? "Editar Requisito" : "Nuevo Requisito"}</DialogTitle></DialogHeader>
        <div className="space-y-4">
          <div className="space-y-1.5"><Label>Nombre *</Label><Input value={name} onChange={e => setName(e.target.value)} /></div>
          <div className="space-y-1.5"><Label>Descripción *</Label><Textarea value={description} onChange={e => setDescription(e.target.value)} rows={2} /></div>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <Label>Tipo</Label>
              <Select value={type} onValueChange={v => setType(v as ProductRequirement["type"]) }>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  {Object.entries(typeLabels).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5 flex items-end">
              <label className="flex items-center gap-2 text-sm cursor-pointer pb-2">
                <Checkbox checked={mandatory} onCheckedChange={v => setMandatory(!!v)} />
                Obligatorio
              </label>
            </div>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancelar</Button>
          <Button onClick={handleSave}>{initial ? "Guardar" : "Crear"}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export default function ProductForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const {
    products,
    addProduct,
    updateProduct,
    addPlan,
    updatePlan,
    deletePlan,
    addRequirement,
    updateRequirement,
    deleteRequirement,
  } = useProductStore();
  const { entities } = useEntityStore();
  const { families } = useProductFamilyStore();
  const { commissionPlans } = useCommissionPlanStore();
  const { getGroupValues } = useParameterStore();
  const cardNetworkOptions = getGroupValues('card_networks');
  const cardLevelOptions = getGroupValues('card_levels');
  const insuranceCoverageOptions = getGroupValues('insurance_coverages');

  const product = id ? products.find((p) => p.id === id) : null;
  const isEdit = !!product;

  // Dialog states
  const [planDialogOpen, setPlanDialogOpen] = useState(false);
  const [editingPlan, setEditingPlan] = useState<ProductPlan | null>(null);
  const [reqDialogOpen, setReqDialogOpen] = useState(false);
  const [editingReq, setEditingReq] = useState<ProductRequirement | null>(null);
  const [deletePlanTarget, setDeletePlanTarget] = useState<ProductPlan | null>(null);
  const [deleteReqTarget, setDeleteReqTarget] = useState<ProductRequirement | null>(null);

  const { register, handleSubmit, formState: { errors }, setValue } = useForm<ProductInput>({
    resolver: zodResolver(schema),
    defaultValues: product
      ? { name: product.name, code: product.code, entityId: product.entityId, entityName: product.entityName, familyId: product.familyId, familyName: product.familyName, description: product.description, status: product.status, validFrom: product.validFrom, validTo: product.validTo }
      : { name: "", code: "", entityId: "", entityName: "", familyId: "", familyName: "", description: "", status: "draft", validFrom: new Date().toISOString().slice(0, 10) },
  });

  const blockedCommissionPlanIds = (() => {
    const used = new Set<string>();
    products.forEach((p) => {
      p.plans.forEach((pl) => {
        if (pl.commissionPlanId) used.add(pl.commissionPlanId);
      });
    });
    if (editingPlan?.commissionPlanId) used.delete(editingPlan.commissionPlanId);
    return used;
  })();

  const onSubmit = (data: ProductInput) => {
    const entityName = entities.find(e => e.id === data.entityId)?.name || "";
    const familyName = families.find(f => f.id === data.familyId)?.description || "";
    if (isEdit) {
      updateProduct(product!.id, { ...data, entityName, familyName });
      toast.success("Producto actualizado");
      navigate(`/productos/${product!.id}`);
    } else {
      const p = addProduct({ ...data, entityName, familyName });
      toast.success("Producto creado");
      navigate(`/productos/${p.id}`);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate(isEdit ? `/productos/${id}` : "/productos")}><ArrowLeft className="h-4 w-4" /></Button>
        <h1 className="text-2xl font-bold tracking-tight">{isEdit ? "Editar Producto" : "Nuevo Producto"}</h1>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader className="pb-3"><CardTitle className="text-base">Datos del Producto</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1.5"><Label>Nombre *</Label><Input {...register("name")} />{errors.name && <p className="text-xs text-destructive">{errors.name.message}</p>}</div>
              <div className="space-y-1.5"><Label>Código *</Label><Input {...register("code")} />{errors.code && <p className="text-xs text-destructive">{errors.code.message}</p>}</div>
            </div>
            <div className="space-y-1.5">
              <Label>Entidad *</Label>
              <Select defaultValue={product?.entityId || ""} onValueChange={v => setValue("entityId", v, { shouldValidate: true })}>
                <SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger>
                <SelectContent>{entities.map(e => <SelectItem key={e.id} value={e.id}>{e.name}</SelectItem>)}</SelectContent>
              </Select>
              {errors.entityId && <p className="text-xs text-destructive">{errors.entityId.message}</p>}
            </div>
            <div className="space-y-1.5">
              <Label>Familia de Producto *</Label>
              <Select defaultValue={product?.familyId || ""} onValueChange={v => setValue("familyId", v, { shouldValidate: true })}>
                <SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger>
                <SelectContent>{families.map(f => <SelectItem key={f.id} value={f.id}>{f.description} ({f.code})</SelectItem>)}</SelectContent>
              </Select>
              {errors.familyId && <p className="text-xs text-destructive">{errors.familyId.message}</p>}
            </div>
            <div className="space-y-1.5"><Label>Descripción *</Label><Textarea {...register("description")} rows={3} />{errors.description && <p className="text-xs text-destructive">{errors.description.message}</p>}</div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-1.5">
                <Label>Estado *</Label>
                <Select defaultValue={product?.status || "draft"} onValueChange={v => setValue("status", v as any)}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {Object.entries(statusLabels).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5"><Label>Vigencia desde *</Label><Input {...register("validFrom")} type="date" />{errors.validFrom && <p className="text-xs text-destructive">{errors.validFrom.message}</p>}</div>
              <div className="space-y-1.5"><Label>Vigencia hasta</Label><Input {...register("validTo")} type="date" /></div>
            </div>
          </CardContent>
        </Card>
        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={() => navigate(isEdit ? `/productos/${id}` : "/productos")}>Cancelar</Button>
          <Button type="submit">{isEdit ? "Guardar Cambios" : "Crear Producto"}</Button>
        </div>
      </form>

      {/* Plans & Requirements — only in edit mode */}
      {isEdit && product && (
        <>
          {/* ── Sub productos ── */}
          <div className="border-t pt-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold tracking-tight">Sub productos ({product.plans.length})</h2>
              <Button className="gap-2" size="sm" onClick={() => { setEditingPlan(null); setPlanDialogOpen(true); }}>
                <Plus className="h-4 w-4" /> Nuevo Sub producto
              </Button>
            </div>
            <PlanDialog
              open={planDialogOpen}
              onOpenChange={setPlanDialogOpen}
              initial={editingPlan}
              commissionPlans={commissionPlans}
              blockedCommissionPlanIds={blockedCommissionPlanIds}
              cardNetworkOptions={cardNetworkOptions}
              cardLevelOptions={cardLevelOptions}
              insuranceCoverageOptions={insuranceCoverageOptions}
              familyCode={product.familyName ? families.find(f => f.description === product.familyName || f.id === product.familyId)?.code : undefined}
              onSave={(data) => {
                if (editingPlan) {
                  updatePlan(product.id, editingPlan.id, data);
                  toast.success("Sub producto actualizado");
                } else {
                  addPlan(product.id, data);
                  toast.success("Sub producto creado");
                }
              }}
            />
            {product.plans.length === 0 ? (
              <p className="text-sm text-muted-foreground">No hay sub productos definidos</p>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {product.plans.map(plan => (
                  <Card key={plan.id}>
                    <CardHeader className="pb-2">
                      <div className="flex items-center justify-between">
                        <CardTitle className="text-base">{plan.name}</CardTitle>
                        <div className="flex items-center gap-1">
                          <StatusBadge status={plan.status} />
                          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => { setEditingPlan(plan); setPlanDialogOpen(true); }}><Pencil className="h-3.5 w-3.5" /></Button>
                          <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeletePlanTarget(plan)}><Trash2 className="h-3.5 w-3.5" /></Button>
                        </div>
                      </div>
                      <p className="text-xs text-muted-foreground">{plan.description}</p>
                    </CardHeader>
                    <CardContent>
                      {plan.installments && plan.installments > 0 && <p className="text-sm"><span className="font-bold">{plan.installments}</span> <span className="text-muted-foreground">cuotas</span> · <span className="font-medium">{plan.currency}</span></p>}
                      {plan.otherCosts !== undefined && plan.otherCosts > 0 && (
                        <p className="text-sm text-muted-foreground">Otros costos: <span className="font-medium text-foreground">$ {plan.otherCosts.toLocaleString('es-AR')}</span></p>
                      )}
                      {(() => {
                        const cp = commissionPlans.find(c => c.id === plan.commissionPlanId);
                        if (!cp) return null;
                        const isPercentage = cp.valueType !== 'fixed_per_sale';
                        const formattedValue = isPercentage
                          ? `${cp.value}%`
                          : `$${cp.value.toLocaleString('de-DE')}`;
                        return (
                          <div className="mt-2 flex items-center gap-2 rounded-md border bg-muted/40 px-3 py-2">
                            <span className="text-xs font-semibold uppercase text-muted-foreground shrink-0">Comisión:</span>
                            <span className="text-sm font-medium text-foreground">{cp.code}</span>
                            <span className="text-xs text-muted-foreground">—</span>
                            <span className="text-sm font-semibold text-primary">{formattedValue}</span>
                          </div>
                        );
                      })()}
                      {plan.loanAttributes && (
                        <div className="mt-3 space-y-1">
                          <p className="text-xs font-semibold uppercase text-muted-foreground">Atributos de Préstamo</p>
                          <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
                            <span className="text-muted-foreground">Amortización:</span>
                            <span className="font-medium">{plan.loanAttributes.amortizationType === 'french' ? 'Francés' : plan.loanAttributes.amortizationType === 'german' ? 'Alemán' : plan.loanAttributes.amortizationType === 'american' ? 'Americano' : 'Bullet'}</span>
                            <span className="text-muted-foreground">TEA:</span>
                            <span className="font-medium">{plan.loanAttributes.annualEffectiveRate}%</span>
                            <span className="text-muted-foreground">CFT:</span>
                            <span className="font-medium">{plan.loanAttributes.cftRate}%</span>
                            <span className="text-muted-foreground">Costos emisión:</span>
                            <span className="font-medium">$ {plan.loanAttributes.adminFees.toLocaleString('es-AR')}</span>
                          </div>
                        </div>
                      )}
                      
                      {plan.insuranceAttributes && (
                        <div className="mt-3 space-y-1">
                          <p className="text-xs font-semibold uppercase text-muted-foreground">Atributos de Seguro</p>
                          <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
                            <span className="text-muted-foreground">Prima:</span>
                            <span className="font-medium">$ {plan.insuranceAttributes.premium.toLocaleString('es-AR')}</span>
                            <span className="text-muted-foreground">Suma Asegurada:</span>
                            <span className="font-medium">$ {plan.insuranceAttributes.sumInsured.toLocaleString('es-AR')}</span>
                            <span className="text-muted-foreground">Tipo Cobertura:</span>
                            <span className="font-medium capitalize">{plan.insuranceAttributes.coverageType === 'individual' ? 'Individual' : plan.insuranceAttributes.coverageType === 'group' ? 'Grupo' : 'Colectivo'}</span>
                            <span className="text-muted-foreground">Días Gracia:</span>
                            <span className="font-medium">{plan.insuranceAttributes.gracePeriodDays} días</span>
                          </div>
                        </div>
                      )}

                      {plan.accountAttributes && (
                        <div className="mt-3 space-y-1">
                          <p className="text-xs font-semibold uppercase text-muted-foreground">Atributos de Cuenta</p>
                          <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
                            <span className="text-muted-foreground">Mantenimiento:</span>
                            <span className="font-medium">{plan.currency} {plan.accountAttributes.maintenanceFee.toLocaleString('es-AR')}</span>
                            <span className="text-muted-foreground">Saldo Mínimo:</span>
                            <span className="font-medium">{plan.currency} {plan.accountAttributes.minimumBalance.toLocaleString('es-AR')}</span>
                            <span className="text-muted-foreground">Tasa Interés:</span>
                            <span className="font-medium">{plan.accountAttributes.interestRate}%</span>
                            <span className="text-muted-foreground">Tipo Cuenta:</span>
                            <span className="font-medium">{plan.accountAttributes.accountType === 'savings' ? 'Caja Ahorro' : plan.accountAttributes.accountType === 'checking' ? 'Cta Corriente' : 'Mercado Dinero'}</span>
                          </div>
                        </div>
                      )}

                      {plan.cardAttributes && (
                        <div className="mt-3 space-y-1">
                          <p className="text-xs font-semibold uppercase text-muted-foreground">Atributos de Tarjeta</p>
                          <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
                            <span className="text-muted-foreground">Límite Crédito:</span>
                            <span className="font-medium">{plan.currency} {plan.cardAttributes.creditLimit.toLocaleString('es-AR')}</span>
                            <span className="text-muted-foreground">Red:</span>
                            <span className="font-medium capitalize">{plan.cardAttributes.network}</span>
                            <span className="text-muted-foreground">Tasa Interés:</span>
                            <span className="font-medium">{plan.cardAttributes.interestRate}%</span>
                            <span className="text-muted-foreground">Cuota Anual:</span>
                            <span className="font-medium">{plan.currency} {plan.cardAttributes.annualFee.toLocaleString('es-AR')}</span>
                          </div>
                        </div>
                      )}

                      {plan.investmentAttributes && (
                        <div className="mt-3 space-y-1">
                          <p className="text-xs font-semibold uppercase text-muted-foreground">Atributos de Inversión</p>
                          <div className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm">
                            <span className="text-muted-foreground">Monto Mínimo:</span>
                            <span className="font-medium">{plan.currency} {plan.investmentAttributes.minimumAmount.toLocaleString('es-AR')}</span>
                            <span className="text-muted-foreground">Retorno Esperado:</span>
                            <span className="font-medium">{plan.investmentAttributes.expectedReturn}%</span>
                            <span className="text-muted-foreground">Plazo:</span>
                            <span className="font-medium">{plan.investmentAttributes.termDays} días</span>
                            <span className="text-muted-foreground">Riesgo:</span>
                            <span className="font-medium">{plan.investmentAttributes.riskLevel === 'low' ? 'Bajo' : plan.investmentAttributes.riskLevel === 'medium' ? 'Medio' : 'Alto'}</span>
                          </div>
                        </div>
                      )}
                      {plan.coverages.length > 0 && (
                        <div className="mt-3 space-y-2">
                          <p className="text-xs font-semibold uppercase text-muted-foreground">Coberturas</p>
                          {plan.coverages.map(cov => (
                            <div key={cov.id} className="flex items-center gap-2 text-sm">
                              <Shield className="h-3.5 w-3.5 text-primary shrink-0" />
                              <span className="font-medium">{cov.name}</span>
                              {cov.sumInsured && <span className="text-xs text-muted-foreground">— Suma: ${cov.sumInsured.toLocaleString('es-AR')}</span>}
                            </div>
                          ))}
                        </div>
                      )}
                    </CardContent>
                  </Card>
                ))}
              </div>
            )}
          </div>

          {/* ── Requisitos ── */}
          <div className="border-t pt-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-semibold tracking-tight">Requisitos ({product.requirements.length})</h2>
              <Button className="gap-2" size="sm" onClick={() => { setEditingReq(null); setReqDialogOpen(true); }}>
                <Plus className="h-4 w-4" /> Nuevo Requisito
              </Button>
            </div>
            {product.requirements.length === 0 ? (
              <p className="text-sm text-muted-foreground">No hay requisitos definidos</p>
            ) : (
              <Card>
                <CardContent className="pt-6 space-y-3">
                  {product.requirements.map(req => (
                    <div key={req.id} className="flex items-start gap-3">
                      <div className="h-8 w-8 rounded-lg bg-secondary flex items-center justify-center shrink-0">
                        <FileText className="h-4 w-4 text-muted-foreground" />
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <p className="text-sm font-medium">{req.name}</p>
                          <Badge variant={req.mandatory ? "default" : "outline"} className="text-[10px]">{req.mandatory ? "Obligatorio" : "Opcional"}</Badge>
                          <Badge variant="secondary" className="text-[10px]">{req.type === 'document' ? 'Documento' : req.type === 'data' ? 'Dato' : 'Validación'}</Badge>
                        </div>
                        <p className="text-xs text-muted-foreground">{req.description}</p>
                      </div>
                      <div className="flex items-center gap-1">
                        <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => { setEditingReq(req); setReqDialogOpen(true); }}><Pencil className="h-3.5 w-3.5" /></Button>
                        <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteReqTarget(req)}><Trash2 className="h-3.5 w-3.5" /></Button>
                      </div>
                    </div>
                  ))}
                </CardContent>
              </Card>
            )}
          </div>

          {/* Dialogs */}
          <RequirementDialog
            open={reqDialogOpen}
            onOpenChange={setReqDialogOpen}
            initial={editingReq}
            onSave={(data) => {
              if (editingReq) {
                updateRequirement(product.id, editingReq.id, data);
                toast.success("Requisito actualizado");
              } else {
                addRequirement(product.id, data);
                toast.success("Requisito creado");
              }
            }}
          />
          <DeleteConfirmDialog
            open={!!deletePlanTarget}
            onOpenChange={(o) => !o && setDeletePlanTarget(null)}
            title="Eliminar Sub producto"
            description={`¿Está seguro de eliminar "${deletePlanTarget?.name}"?`}
            onConfirm={() => {
              if (deletePlanTarget) {
                deletePlan(product.id, deletePlanTarget.id);
                toast.success("Sub producto eliminado");
                setDeletePlanTarget(null);
              }
            }}
          />
          <DeleteConfirmDialog
            open={!!deleteReqTarget}
            onOpenChange={(o) => !o && setDeleteReqTarget(null)}
            title="Eliminar Requisito"
            description={`¿Está seguro de eliminar "${deleteReqTarget?.name}"?`}
            onConfirm={() => {
              if (deleteReqTarget) {
                deleteRequirement(product.id, deleteReqTarget.id);
                toast.success("Requisito eliminado");
                setDeleteReqTarget(null);
              }
            }}
          />
        </>
      )}
    </div>
  );
}
