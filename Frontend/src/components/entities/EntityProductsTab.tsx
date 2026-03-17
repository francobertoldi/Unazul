import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, Pencil, Trash2, ChevronDown, ChevronRight, Shield, FileText, Package } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { DataTable } from "@/components/shared/DataTable";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import {
  useProductStore,
  useProductFamilyStore,
  useCommissionPlanStore,
  useParameterStore,
} from "@/data/store";
import {
  Product,
  ProductPlan,
  ProductRequirement,
  AmortizationType,
  Coverage,
  CommissionPlan,
  InsuranceAttributes,
  AccountAttributes,
  CardAttributes,
  InvestmentAttributes,
  RiskLevel,
  InstrumentType,
} from "@/data/types";
import { toast } from "sonner";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { X, ShieldCheck } from "lucide-react";
import { useEffect } from "react";

const statusLabels: Record<string, string> = { draft: "Borrador", active: "Activo", inactive: "Inactivo", deprecated: "Deprecado" };

// ── Inline Plan Dialog ──
function InlinePlanDialog({
  open, onOpenChange, onSave, initial, commissionPlans, blockedCommissionPlanIds, familyCode, cardNetworkOptions, cardLevelOptions, insuranceCoverageOptions,
}: {
  open: boolean; onOpenChange: (o: boolean) => void;
  onSave: (data: Omit<ProductPlan, "id" | "productId">) => void;
  initial?: ProductPlan | null; commissionPlans: CommissionPlan[];
  blockedCommissionPlanIds: Set<string>; familyCode?: string;
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

  const [amortizationType, setAmortizationType] = useState<AmortizationType>(initial?.loanAttributes?.amortizationType || "french");
  const [annualEffectiveRate, setAnnualEffectiveRate] = useState(initial?.loanAttributes?.annualEffectiveRate?.toString() || "0");
  const [cftRate, setCftRate] = useState(initial?.loanAttributes?.cftRate?.toString() || "0");
  const [adminFees, setAdminFees] = useState(initial?.loanAttributes?.adminFees?.toString() || "0");

  const [insurancePremium, setInsurancePremium] = useState(initial?.insuranceAttributes?.premium?.toString() || "0");
  const [insuranceSumInsured, setInsuranceSumInsured] = useState(initial?.insuranceAttributes?.sumInsured?.toString() || "0");
  const [insuranceGracePeriod, setInsuranceGracePeriod] = useState(initial?.insuranceAttributes?.gracePeriodDays?.toString() || "30");
  const [insuranceCoverageType, setInsuranceCoverageType] = useState<InsuranceAttributes['coverageType']>(initial?.insuranceAttributes?.coverageType || "individual");
  const [coverages, setCoverages] = useState<Coverage[]>(initial?.coverages || []);

  const [accountMaintenanceFee, setAccountMaintenanceFee] = useState(initial?.accountAttributes?.maintenanceFee?.toString() || "0");
  const [accountMinimumBalance, setAccountMinimumBalance] = useState(initial?.accountAttributes?.minimumBalance?.toString() || "0");
  const [accountInterestRate, setAccountInterestRate] = useState(initial?.accountAttributes?.interestRate?.toString() || "0");
  const [accountType, setAccountType] = useState<AccountAttributes['accountType']>(initial?.accountAttributes?.accountType || "savings");

  const [cardCreditLimit, setCardCreditLimit] = useState(initial?.cardAttributes?.creditLimit?.toString() || "0");
  const [cardAnnualFee, setCardAnnualFee] = useState(initial?.cardAttributes?.annualFee?.toString() || "0");
  const [cardInterestRate, setCardInterestRate] = useState(initial?.cardAttributes?.interestRate?.toString() || "0");
  const [cardNetwork, setCardNetwork] = useState(initial?.cardAttributes?.network || "visa" as string);
  const [cardLevel, setCardLevel] = useState(initial?.cardAttributes?.level || "");
  const [cardGracePeriod, setCardGracePeriod] = useState(initial?.cardAttributes?.gracePeriodDays?.toString() || "30");

  const [investmentMinimumAmount, setInvestmentMinimumAmount] = useState(initial?.investmentAttributes?.minimumAmount?.toString() || "0");
  const [investmentExpectedReturn, setInvestmentExpectedReturn] = useState(initial?.investmentAttributes?.expectedReturn?.toString() || "0");
  const [investmentTermDays, setInvestmentTermDays] = useState(initial?.investmentAttributes?.termDays?.toString() || "30");
  const [investmentRiskLevel, setInvestmentRiskLevel] = useState<RiskLevel>(initial?.investmentAttributes?.riskLevel || "low");
  const [investmentInstrumentType, setInvestmentInstrumentType] = useState<InstrumentType>(initial?.investmentAttributes?.instrumentType || "fixed_term");

  useEffect(() => {
    setName(initial?.name || ""); setDescription(initial?.description || "");
    setCurrency(initial?.currency || "ARS"); setInstallments(initial?.installments?.toString() || "1");
    setStatus(initial?.status || "draft"); setCommissionPlanId(initial?.commissionPlanId || "");
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
  const amortizationLabels: Record<AmortizationType, string> = { french: "Francés (cuota fija)", german: "Alemán (amortización fija)", american: "Americano (intereses periódicos)", bullet: "Bullet (pago al vencimiento)" };
  const selectableCommissionPlans = commissionPlans.filter(cp => cp.id === commissionPlanId || !blockedCommissionPlanIds.has(cp.id));

  const handleSave = () => {
    if (!name.trim()) { toast.error("El nombre es requerido"); return; }
    if (!commissionPlanId) { toast.error("Debe seleccionar un plan de comisiones"); return; }
    if (blockedCommissionPlanIds.has(commissionPlanId) && commissionPlanId !== initial?.commissionPlanId) { toast.error("Ese plan de comisiones ya está asignado"); return; }

    const loanAttributes = familyCategory === 'loan' ? { amortizationType, annualEffectiveRate: parseFloat(annualEffectiveRate) || 0, cftRate: parseFloat(cftRate) || 0, adminFees: parseFloat(adminFees) || 0 } : undefined;
    const insuranceAttributes = familyCategory === 'insurance' ? { premium: parseFloat(insurancePremium) || 0, sumInsured: parseFloat(insuranceSumInsured) || 0, gracePeriodDays: parseInt(insuranceGracePeriod) || 30, coverageType: insuranceCoverageType } : undefined;
    const accountAttributes = familyCategory === 'account' ? { maintenanceFee: parseFloat(accountMaintenanceFee) || 0, minimumBalance: parseFloat(accountMinimumBalance) || 0, interestRate: parseFloat(accountInterestRate) || 0, accountType } : undefined;
    const cardAttributes = familyCategory === 'card' ? { creditLimit: parseFloat(cardCreditLimit) || 0, annualFee: parseFloat(cardAnnualFee) || 0, interestRate: parseFloat(cardInterestRate) || 0, network: cardNetwork as any, gracePeriodDays: parseInt(cardGracePeriod) || 30, level: cardLevel || undefined } : undefined;
    const investmentAttributes = familyCategory === 'investment' ? { minimumAmount: parseFloat(investmentMinimumAmount) || 0, expectedReturn: parseFloat(investmentExpectedReturn) || 0, termDays: parseInt(investmentTermDays) || 30, riskLevel: investmentRiskLevel, instrumentType: investmentInstrumentType } : undefined;

    onSave({ name, description, price: 0, currency, installments: parseInt(installments) || 1, commissionPlanId, status, coverages: familyCategory === 'insurance' ? coverages : (initial?.coverages || []), otherCosts: parseFloat(otherCosts) || 0, loanAttributes, insuranceAttributes, accountAttributes, cardAttributes, investmentAttributes });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[85vh] overflow-y-auto">
        <DialogHeader><DialogTitle>{initial ? "Editar Sub producto" : "Nuevo Sub producto"}</DialogTitle></DialogHeader>
        <div className="space-y-4">
          <div className="space-y-1.5"><Label>Nombre *</Label><Input value={name} onChange={e => setName(e.target.value)} /></div>
          <div className="space-y-1.5"><Label>Descripción</Label><Textarea value={description} onChange={e => setDescription(e.target.value)} rows={2} /></div>
          <div className="space-y-1.5">
            <Label>Plan de comisiones *</Label>
            <Select value={commissionPlanId} onValueChange={setCommissionPlanId}>
              <SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger>
              <SelectContent>{selectableCommissionPlans.map(cp => <SelectItem key={cp.id} value={cp.id}>{cp.code} — {cp.description}</SelectItem>)}</SelectContent>
            </Select>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="space-y-1.5"><Label>Cuotas</Label><Input type="number" min="1" value={installments} onChange={e => setInstallments(e.target.value)} /></div>
            <div className="space-y-1.5">
              <Label>Moneda</Label>
              <Select value={currency} onValueChange={setCurrency}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="ARS">ARS</SelectItem><SelectItem value="USD">USD</SelectItem></SelectContent></Select>
            </div>
            <div className="space-y-1.5">
              <Label>Estado</Label>
              <Select value={status} onValueChange={v => setStatus(v as ProductPlan["status"])}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent>{Object.entries(statusLabels).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}</SelectContent></Select>
            </div>
          </div>
          <div className="space-y-1.5">
            <Label>Otros Costos</Label>
            <div className="relative"><span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">$</span><Input type="number" step="0.01" min="0" value={otherCosts} onChange={e => setOtherCosts(e.target.value)} className="pl-7" /></div>
          </div>

          {/* Family-specific attributes */}
          {familyCategory && (
            <div className="border-t pt-4 space-y-4">
              <span className="font-semibold text-muted-foreground uppercase tracking-wider text-xs">
                Atributos de {familyCategory === 'loan' ? 'Préstamo' : familyCategory === 'insurance' ? 'Seguro' : familyCategory === 'account' ? 'Cuenta' : familyCategory === 'card' ? 'Tarjeta' : 'Inversión'}
              </span>

              {familyCategory === 'loan' && (
                <div className="space-y-4">
                  <div className="space-y-1.5">
                    <Label>Tipo de Amortización</Label>
                    <Select value={amortizationType} onValueChange={v => setAmortizationType(v as AmortizationType)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent>{Object.entries(amortizationLabels).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}</SelectContent></Select>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5"><Label>TEA (%)</Label><Input type="number" step="0.01" value={annualEffectiveRate} onChange={e => setAnnualEffectiveRate(e.target.value)} /></div>
                    <div className="space-y-1.5"><Label>CFT (%)</Label><Input type="number" step="0.01" value={cftRate} onChange={e => setCftRate(e.target.value)} /></div>
                  </div>
                  <div className="space-y-1.5"><Label>Costos Adm. Emisión ($)</Label><Input type="number" step="0.01" value={adminFees} onChange={e => setAdminFees(e.target.value)} /></div>
                </div>
              )}

              {familyCategory === 'insurance' && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5"><Label>Prima ($)</Label><Input type="number" step="0.01" value={insurancePremium} onChange={e => setInsurancePremium(e.target.value)} /></div>
                    <div className="space-y-1.5"><Label>Suma Asegurada ($)</Label><Input type="number" step="0.01" value={insuranceSumInsured} onChange={e => setInsuranceSumInsured(e.target.value)} /></div>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5"><Label>Días de Gracia</Label><Input type="number" value={insuranceGracePeriod} onChange={e => setInsuranceGracePeriod(e.target.value)} /></div>
                    <div className="space-y-1.5">
                      <Label>Tipo de Cobertura</Label>
                      <Select value={insuranceCoverageType} onValueChange={v => setInsuranceCoverageType(v as InsuranceAttributes['coverageType'])}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="individual">Individual</SelectItem><SelectItem value="group">Grupo</SelectItem><SelectItem value="collective">Colectivo</SelectItem></SelectContent></Select>
                    </div>
                  </div>
                  {/* Coverages CRUD */}
                  <div className="border-t pt-3 space-y-2">
                    <span className="font-semibold text-muted-foreground uppercase tracking-wider text-xs">Coberturas ({coverages.length})</span>
                    <Select value="" onValueChange={covCode => {
                      const opt = insuranceCoverageOptions.find(o => o.code === covCode);
                      if (!opt) return;
                      if (coverages.some(c => c.name === opt.label)) { toast.error("Ya agregada"); return; }
                      setCoverages([...coverages, { id: `cov-${Date.now()}`, name: opt.label, description: opt.label, sumInsured: 0, premium: 0 }]);
                    }}>
                      <SelectTrigger><SelectValue placeholder="Agregar cobertura..." /></SelectTrigger>
                      <SelectContent>{insuranceCoverageOptions.filter(o => !coverages.some(c => c.name === o.label)).map(o => <SelectItem key={o.code} value={o.code}>{o.label}</SelectItem>)}</SelectContent>
                    </Select>
                    {coverages.map((cov, idx) => (
                      <div key={cov.id} className="flex items-center gap-2 rounded-md border bg-muted/30 px-3 py-2">
                        <ShieldCheck className="h-4 w-4 text-primary shrink-0" />
                        <span className="text-sm font-medium flex-1 truncate">{cov.name}</span>
                        <Input type="number" step="0.01" placeholder="Suma aseg." value={cov.sumInsured || ""} onChange={e => { const u = [...coverages]; u[idx] = { ...u[idx], sumInsured: parseFloat(e.target.value) || 0 }; setCoverages(u); }} className="h-8 text-xs w-24" />
                        <Input type="number" step="0.01" placeholder="Prima" value={cov.premium || ""} onChange={e => { const u = [...coverages]; u[idx] = { ...u[idx], premium: parseFloat(e.target.value) || 0 }; setCoverages(u); }} className="h-8 text-xs w-24" />
                        <Button type="button" variant="ghost" size="icon" className="h-7 w-7 text-destructive" onClick={() => setCoverages(coverages.filter((_, i) => i !== idx))}><X className="h-3.5 w-3.5" /></Button>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {familyCategory === 'account' && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5"><Label>Comisión Mantenimiento ($)</Label><Input type="number" step="0.01" value={accountMaintenanceFee} onChange={e => setAccountMaintenanceFee(e.target.value)} /></div>
                    <div className="space-y-1.5"><Label>Saldo Mínimo ($)</Label><Input type="number" step="0.01" value={accountMinimumBalance} onChange={e => setAccountMinimumBalance(e.target.value)} /></div>
                  </div>
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5"><Label>Tasa Interés (%)</Label><Input type="number" step="0.01" value={accountInterestRate} onChange={e => setAccountInterestRate(e.target.value)} /></div>
                    <div className="space-y-1.5">
                      <Label>Tipo de Cuenta</Label>
                      <Select value={accountType} onValueChange={v => setAccountType(v as AccountAttributes['accountType'])}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="savings">Caja de Ahorro</SelectItem><SelectItem value="checking">Cuenta Corriente</SelectItem><SelectItem value="money_market">Mercado de Dinero</SelectItem></SelectContent></Select>
                    </div>
                  </div>
                </div>
              )}

              {familyCategory === 'card' && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5"><Label>Límite de Crédito ($)</Label><Input type="number" step="0.01" value={cardCreditLimit} onChange={e => setCardCreditLimit(e.target.value)} /></div>
                    <div className="space-y-1.5">
                      <Label>Red</Label>
                      <Select value={cardNetwork} onValueChange={setCardNetwork}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent>{cardNetworkOptions.map(opt => <SelectItem key={opt.code} value={opt.code}>{opt.label}</SelectItem>)}</SelectContent></Select>
                    </div>
                  </div>
                  <div className="grid grid-cols-3 gap-4">
                    <div className="space-y-1.5"><Label>Cuota Anual ($)</Label><Input type="number" step="0.01" value={cardAnnualFee} onChange={e => setCardAnnualFee(e.target.value)} /></div>
                    <div className="space-y-1.5"><Label>Tasa Interés (%)</Label><Input type="number" step="0.01" value={cardInterestRate} onChange={e => setCardInterestRate(e.target.value)} /></div>
                    <div className="space-y-1.5"><Label>Días Gracia</Label><Input type="number" value={cardGracePeriod} onChange={e => setCardGracePeriod(e.target.value)} /></div>
                  </div>
                  <div className="space-y-1.5">
                    <Label>Nivel</Label>
                    <Select value={cardLevel} onValueChange={setCardLevel}><SelectTrigger><SelectValue placeholder="Seleccione nivel" /></SelectTrigger><SelectContent>{cardLevelOptions.map(opt => <SelectItem key={opt.code} value={opt.code}>{opt.label}</SelectItem>)}</SelectContent></Select>
                  </div>
                </div>
              )}

              {familyCategory === 'investment' && (
                <div className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1.5"><Label>Monto Mínimo ($)</Label><Input type="number" step="0.01" value={investmentMinimumAmount} onChange={e => setInvestmentMinimumAmount(e.target.value)} /></div>
                    <div className="space-y-1.5">
                      <Label>Instrumento</Label>
                      <Select value={investmentInstrumentType} onValueChange={v => setInvestmentInstrumentType(v as InstrumentType)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="fixed_term">Plazo Fijo</SelectItem><SelectItem value="bond">Bono</SelectItem><SelectItem value="mutual_fund">Fondo Común</SelectItem><SelectItem value="stock">Acciones</SelectItem></SelectContent></Select>
                    </div>
                  </div>
                  <div className="grid grid-cols-3 gap-4">
                    <div className="space-y-1.5"><Label>Retorno Esperado (%)</Label><Input type="number" step="0.01" value={investmentExpectedReturn} onChange={e => setInvestmentExpectedReturn(e.target.value)} /></div>
                    <div className="space-y-1.5"><Label>Plazo (Días)</Label><Input type="number" value={investmentTermDays} onChange={e => setInvestmentTermDays(e.target.value)} /></div>
                    <div className="space-y-1.5">
                      <Label>Riesgo</Label>
                      <Select value={investmentRiskLevel} onValueChange={v => setInvestmentRiskLevel(v as RiskLevel)}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="low">Bajo</SelectItem><SelectItem value="medium">Medio</SelectItem><SelectItem value="high">Alto</SelectItem></SelectContent></Select>
                    </div>
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancelar</Button>
          <Button onClick={handleSave}>{initial ? "Guardar" : "Crear"}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ── Requirement Dialog ──
function InlineRequirementDialog({ open, onOpenChange, onSave, initial }: {
  open: boolean; onOpenChange: (o: boolean) => void;
  onSave: (data: Omit<ProductRequirement, 'id'>) => void;
  initial?: ProductRequirement | null;
}) {
  const [name, setName] = useState(initial?.name || "");
  const [type, setType] = useState<ProductRequirement["type"]>(initial?.type || "document");
  const [mandatory, setMandatory] = useState(initial?.mandatory ?? true);
  const [description, setDescription] = useState(initial?.description || "");

  useEffect(() => {
    setName(initial?.name || ""); setType(initial?.type || "document");
    setMandatory(initial?.mandatory ?? true); setDescription(initial?.description || "");
  }, [initial, open]);

  const handleSave = () => {
    if (!name.trim()) { toast.error("El nombre es requerido"); return; }
    if (!description.trim()) { toast.error("La descripción es requerida"); return; }
    onSave({ name, type, mandatory, description });
    onOpenChange(false);
  };

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
              <Select value={type} onValueChange={v => setType(v as ProductRequirement["type"])}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem value="document">Documento</SelectItem><SelectItem value="data">Dato</SelectItem><SelectItem value="validation">Validación</SelectItem></SelectContent></Select>
            </div>
            <div className="space-y-1.5 flex items-end">
              <label className="flex items-center gap-2 text-sm cursor-pointer pb-2"><Checkbox checked={mandatory} onCheckedChange={v => setMandatory(!!v)} />Obligatorio</label>
            </div>
          </div>
        </div>
        <DialogFooter><Button variant="outline" onClick={() => onOpenChange(false)}>Cancelar</Button><Button onClick={handleSave}>{initial ? "Guardar" : "Crear"}</Button></DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ── Product Edit Dialog ──
function ProductEditDialog({ open, onOpenChange, onSave, initial, families, entityId, entityName }: {
  open: boolean; onOpenChange: (o: boolean) => void;
  onSave: (data: { name: string; code: string; familyId: string; familyName: string; description: string; status: string; validFrom: string; validTo?: string }) => void;
  initial?: Product | null;
  families: { id: string; code: string; description: string }[];
  entityId: string; entityName: string;
}) {
  const [name, setName] = useState(initial?.name || "");
  const [code, setCode] = useState(initial?.code || "");
  const [familyId, setFamilyId] = useState(initial?.familyId || "");
  const [description, setDescription] = useState(initial?.description || "");
  const [status, setStatus] = useState(initial?.status || "draft");
  const [validFrom, setValidFrom] = useState(initial?.validFrom || new Date().toISOString().slice(0, 10));
  const [validTo, setValidTo] = useState(initial?.validTo || "");

  useEffect(() => {
    setName(initial?.name || ""); setCode(initial?.code || "");
    setFamilyId(initial?.familyId || ""); setDescription(initial?.description || "");
    setStatus(initial?.status || "draft");
    setValidFrom(initial?.validFrom || new Date().toISOString().slice(0, 10));
    setValidTo(initial?.validTo || "");
  }, [initial, open]);

  const handleSave = () => {
    if (!name.trim() || !code.trim() || !familyId || !description.trim() || !validFrom) { toast.error("Complete todos los campos requeridos"); return; }
    const familyName = families.find(f => f.id === familyId)?.description || "";
    onSave({ name, code, familyId, familyName, description, status, validFrom, validTo: validTo || undefined });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader><DialogTitle>{initial ? "Editar Producto" : "Nuevo Producto"}</DialogTitle></DialogHeader>
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1.5"><Label>Nombre *</Label><Input value={name} onChange={e => setName(e.target.value)} /></div>
            <div className="space-y-1.5"><Label>Código *</Label><Input value={code} onChange={e => setCode(e.target.value)} /></div>
          </div>
          <div className="space-y-1.5">
            <Label>Familia de Producto *</Label>
            <Select value={familyId} onValueChange={setFamilyId}><SelectTrigger><SelectValue placeholder="Seleccionar..." /></SelectTrigger><SelectContent>{families.map(f => <SelectItem key={f.id} value={f.id}>{f.description} ({f.code})</SelectItem>)}</SelectContent></Select>
          </div>
          <div className="space-y-1.5"><Label>Descripción *</Label><Textarea value={description} onChange={e => setDescription(e.target.value)} rows={2} /></div>
          <div className="grid grid-cols-3 gap-4">
            <div className="space-y-1.5">
              <Label>Estado</Label>
              <Select value={status} onValueChange={v => setStatus(v as "draft" | "active" | "inactive" | "deprecated")}><SelectTrigger><SelectValue /></SelectTrigger><SelectContent>{Object.entries(statusLabels).map(([k, v]) => <SelectItem key={k} value={k}>{v}</SelectItem>)}</SelectContent></Select>
            </div>
            <div className="space-y-1.5"><Label>Vigencia desde *</Label><Input type="date" value={validFrom} onChange={e => setValidFrom(e.target.value)} /></div>
            <div className="space-y-1.5"><Label>Vigencia hasta</Label><Input type="date" value={validTo} onChange={e => setValidTo(e.target.value)} /></div>
          </div>
        </div>
        <DialogFooter><Button variant="outline" onClick={() => onOpenChange(false)}>Cancelar</Button><Button onClick={handleSave}>{initial ? "Guardar" : "Crear"}</Button></DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ── Main Component ──
interface EntityProductsTabProps {
  entityId: string;
  entityName: string;
}

export default function EntityProductsTab({ entityId, entityName }: EntityProductsTabProps) {
  const navigate = useNavigate();
  const { products, addProduct, updateProduct, deleteProduct, addPlan, updatePlan, deletePlan, addRequirement, updateRequirement, deleteRequirement } = useProductStore();
  const { families } = useProductFamilyStore();
  const { commissionPlans } = useCommissionPlanStore();
  const { getGroupValues } = useParameterStore();
  const cardNetworkOptions = getGroupValues('card_networks');
  const cardLevelOptions = getGroupValues('card_levels');
  const insuranceCoverageOptions = getGroupValues('insurance_coverages');

  const entityProducts = products.filter(p => p.entityId === entityId);

  // Product CRUD state
  const [productDialogOpen, setProductDialogOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [deleteProductTarget, setDeleteProductTarget] = useState<Product | null>(null);

  // Expanded product for sub-product management
  const [expandedProductId, setExpandedProductId] = useState<string | null>(null);

  // Plan CRUD state
  const [planDialogOpen, setPlanDialogOpen] = useState(false);
  const [editingPlan, setEditingPlan] = useState<ProductPlan | null>(null);
  const [planProductId, setPlanProductId] = useState<string>("");
  const [deletePlanTarget, setDeletePlanTarget] = useState<{ productId: string; plan: ProductPlan } | null>(null);

  // Requirement CRUD state
  const [reqDialogOpen, setReqDialogOpen] = useState(false);
  const [editingReq, setEditingReq] = useState<ProductRequirement | null>(null);
  const [reqProductId, setReqProductId] = useState<string>("");
  const [deleteReqTarget, setDeleteReqTarget] = useState<{ productId: string; req: ProductRequirement } | null>(null);

  const blockedCommissionPlanIds = (() => {
    const used = new Set<string>();
    products.forEach(p => p.plans.forEach(pl => { if (pl.commissionPlanId) used.add(pl.commissionPlanId); }));
    if (editingPlan?.commissionPlanId) used.delete(editingPlan.commissionPlanId);
    return used;
  })();

  const toggleExpand = (productId: string) => {
    setExpandedProductId(prev => prev === productId ? null : productId);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-base font-semibold">Productos de la entidad ({entityProducts.length})</h3>
        <Button className="gap-2" size="sm" onClick={() => { setEditingProduct(null); setProductDialogOpen(true); }}>
          <Plus className="h-4 w-4" /> Nuevo Producto
        </Button>
      </div>

      {entityProducts.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Package className="h-10 w-10 text-muted-foreground mb-3" />
            <p className="text-sm text-muted-foreground">No hay productos asociados a esta entidad</p>
            <Button className="gap-2 mt-4" size="sm" variant="outline" onClick={() => { setEditingProduct(null); setProductDialogOpen(true); }}>
              <Plus className="h-4 w-4" /> Crear primer producto
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {entityProducts.map(product => {
            const isExpanded = expandedProductId === product.id;
            const family = families.find(f => f.id === product.familyId);
            return (
              <Card key={product.id}>
                <CardHeader className="pb-2">
                  <div className="flex items-center gap-3">
                    <Button variant="ghost" size="icon" className="h-8 w-8 shrink-0" onClick={() => toggleExpand(product.id)}>
                      {isExpanded ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
                    </Button>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2">
                        <CardTitle className="text-sm">{product.name}</CardTitle>
                        <Badge variant="outline" className="text-[10px]">{product.code}</Badge>
                        <StatusBadge status={product.status} />
                        {family && <Badge variant="secondary" className="text-[10px]">{family.description}</Badge>}
                      </div>
                      <p className="text-xs text-muted-foreground mt-0.5 truncate">{product.description}</p>
                    </div>
                    <div className="flex items-center gap-1 shrink-0">
                      <Badge variant="outline" className="text-[10px]">{product.plans.length} sub productos</Badge>
                      <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => { setEditingProduct(product); setProductDialogOpen(true); }}><Pencil className="h-3.5 w-3.5" /></Button>
                      <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => setDeleteProductTarget(product)}><Trash2 className="h-3.5 w-3.5" /></Button>
                    </div>
                  </div>
                </CardHeader>

                {isExpanded && (
                  <CardContent className="pt-0 space-y-4">
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-sm border-t pt-3">
                      <div><span className="text-xs text-muted-foreground">Versión</span><p className="font-medium">v{product.version}</p></div>
                      <div><span className="text-xs text-muted-foreground">Vigencia desde</span><p className="font-medium">{product.validFrom}</p></div>
                      <div><span className="text-xs text-muted-foreground">Vigencia hasta</span><p className="font-medium">{product.validTo || "—"}</p></div>
                      <div><span className="text-xs text-muted-foreground">Creado</span><p className="font-medium">{product.createdAt}</p></div>
                    </div>

                    {/* Sub productos */}
                    <div className="border-t pt-3">
                      <div className="flex items-center justify-between mb-3">
                        <span className="text-xs font-semibold uppercase text-muted-foreground">Sub productos ({product.plans.length})</span>
                        <Button size="sm" variant="outline" className="gap-1 h-7 text-xs" onClick={() => { setPlanProductId(product.id); setEditingPlan(null); setPlanDialogOpen(true); }}>
                          <Plus className="h-3 w-3" /> Agregar
                        </Button>
                      </div>
                      {product.plans.length === 0 ? (
                        <p className="text-xs text-muted-foreground">Sin sub productos</p>
                      ) : (
                        <div className="space-y-2">
                          {product.plans.map(plan => {
                            const cp = commissionPlans.find(c => c.id === plan.commissionPlanId);
                            return (
                              <div key={plan.id} className="rounded-lg border bg-card p-3">
                                <div className="flex items-center justify-between">
                                  <div className="flex items-center gap-2">
                                    <span className="text-sm font-medium">{plan.name}</span>
                                    <StatusBadge status={plan.status} />
                                  </div>
                                  <div className="flex items-center gap-1">
                                    <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => { setPlanProductId(product.id); setEditingPlan(plan); setPlanDialogOpen(true); }}><Pencil className="h-3 w-3" /></Button>
                                    <Button variant="ghost" size="icon" className="h-7 w-7 text-destructive hover:text-destructive" onClick={() => setDeletePlanTarget({ productId: product.id, plan })}><Trash2 className="h-3 w-3" /></Button>
                                  </div>
                                </div>
                                <p className="text-xs text-muted-foreground mt-1">{plan.description}</p>
                                <div className="flex flex-wrap gap-3 mt-2 text-xs">
                                  {plan.installments && <span><strong>{plan.installments}</strong> cuotas · {plan.currency}</span>}
                                  {plan.otherCosts !== undefined && plan.otherCosts > 0 && <span>Otros costos: <strong>${plan.otherCosts.toLocaleString('es-AR')}</strong></span>}
                                  {cp && <span>Comisión: <strong>{cp.code}</strong></span>}
                                </div>
                                {plan.loanAttributes && <div className="mt-2 text-xs grid grid-cols-2 gap-1"><span className="text-muted-foreground">TEA: {plan.loanAttributes.annualEffectiveRate}%</span><span className="text-muted-foreground">CFT: {plan.loanAttributes.cftRate}%</span></div>}
                                {plan.insuranceAttributes && <div className="mt-2 text-xs"><span className="text-muted-foreground">Prima: ${plan.insuranceAttributes.premium.toLocaleString('es-AR')} · Suma: ${plan.insuranceAttributes.sumInsured.toLocaleString('es-AR')}</span></div>}
                                {plan.coverages.length > 0 && <div className="mt-2 flex flex-wrap gap-1">{plan.coverages.map(c => <Badge key={c.id} variant="secondary" className="text-[10px]"><Shield className="h-2.5 w-2.5 mr-1" />{c.name}</Badge>)}</div>}
                              </div>
                            );
                          })}
                        </div>
                      )}
                    </div>

                    {/* Requisitos */}
                    <div className="border-t pt-3">
                      <div className="flex items-center justify-between mb-3">
                        <span className="text-xs font-semibold uppercase text-muted-foreground">Requisitos ({product.requirements.length})</span>
                        <Button size="sm" variant="outline" className="gap-1 h-7 text-xs" onClick={() => { setReqProductId(product.id); setEditingReq(null); setReqDialogOpen(true); }}>
                          <Plus className="h-3 w-3" /> Agregar
                        </Button>
                      </div>
                      {product.requirements.length === 0 ? (
                        <p className="text-xs text-muted-foreground">Sin requisitos</p>
                      ) : (
                        <div className="space-y-1">
                          {product.requirements.map(req => (
                            <div key={req.id} className="flex items-center gap-2 py-1">
                              <FileText className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
                              <span className="text-sm flex-1">{req.name}</span>
                              <Badge variant={req.mandatory ? "default" : "outline"} className="text-[10px]">{req.mandatory ? "Obligatorio" : "Opcional"}</Badge>
                              <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => { setReqProductId(product.id); setEditingReq(req); setReqDialogOpen(true); }}><Pencil className="h-3 w-3" /></Button>
                              <Button variant="ghost" size="icon" className="h-7 w-7 text-destructive hover:text-destructive" onClick={() => setDeleteReqTarget({ productId: product.id, req })}><Trash2 className="h-3 w-3" /></Button>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  </CardContent>
                )}
              </Card>
            );
          })}
        </div>
      )}

      {/* Dialogs */}
      <ProductEditDialog
        open={productDialogOpen}
        onOpenChange={setProductDialogOpen}
        initial={editingProduct}
        families={families}
        entityId={entityId}
        entityName={entityName}
        onSave={(data) => {
          if (editingProduct) {
            updateProduct(editingProduct.id, { ...data, entityId, entityName } as any);
            toast.success("Producto actualizado");
          } else {
            addProduct({ ...data, entityId, entityName } as any);
            toast.success("Producto creado");
          }
        }}
      />

      <InlinePlanDialog
        open={planDialogOpen}
        onOpenChange={setPlanDialogOpen}
        initial={editingPlan}
        commissionPlans={commissionPlans}
        blockedCommissionPlanIds={blockedCommissionPlanIds}
        familyCode={(() => {
          const prod = products.find(p => p.id === planProductId);
          if (!prod) return undefined;
          return families.find(f => f.id === prod.familyId)?.code;
        })()}
        cardNetworkOptions={cardNetworkOptions}
        cardLevelOptions={cardLevelOptions}
        insuranceCoverageOptions={insuranceCoverageOptions}
        onSave={(data) => {
          if (editingPlan) {
            updatePlan(planProductId, editingPlan.id, data);
            toast.success("Sub producto actualizado");
          } else {
            addPlan(planProductId, data);
            toast.success("Sub producto creado");
          }
        }}
      />

      <InlineRequirementDialog
        open={reqDialogOpen}
        onOpenChange={setReqDialogOpen}
        initial={editingReq}
        onSave={(data) => {
          if (editingReq) {
            updateRequirement(reqProductId, editingReq.id, data);
            toast.success("Requisito actualizado");
          } else {
            addRequirement(reqProductId, data);
            toast.success("Requisito creado");
          }
        }}
      />

      <DeleteConfirmDialog open={!!deleteProductTarget} onOpenChange={o => !o && setDeleteProductTarget(null)} title="Eliminar Producto"
        description={`¿Está seguro de eliminar "${deleteProductTarget?.name}"? Se eliminarán también todos sus sub productos.`}
        onConfirm={() => { if (deleteProductTarget) { deleteProduct(deleteProductTarget.id); toast.success("Producto eliminado"); setDeleteProductTarget(null); } }} />

      <DeleteConfirmDialog open={!!deletePlanTarget} onOpenChange={o => !o && setDeletePlanTarget(null)} title="Eliminar Sub producto"
        description={`¿Está seguro de eliminar "${deletePlanTarget?.plan.name}"?`}
        onConfirm={() => { if (deletePlanTarget) { deletePlan(deletePlanTarget.productId, deletePlanTarget.plan.id); toast.success("Sub producto eliminado"); setDeletePlanTarget(null); } }} />

      <DeleteConfirmDialog open={!!deleteReqTarget} onOpenChange={o => !o && setDeleteReqTarget(null)} title="Eliminar Requisito"
        description={`¿Está seguro de eliminar "${deleteReqTarget?.req.name}"?`}
        onConfirm={() => { if (deleteReqTarget) { deleteRequirement(deleteReqTarget.productId, deleteReqTarget.req.id); toast.success("Requisito eliminado"); setDeleteReqTarget(null); } }} />
    </div>
  );
}
