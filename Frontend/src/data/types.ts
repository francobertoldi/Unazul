export type EntityStatus = 'active' | 'inactive' | 'suspended';
export type UserStatus = 'active' | 'inactive' | 'locked';
export type ChannelType = 'web' | 'mobile' | 'api' | 'presencial';
export type ProductStatus = 'draft' | 'active' | 'inactive' | 'deprecated';
export type ApplicationStatus = 'draft' | 'pending' | 'in_review' | 'approved' | 'rejected' | 'cancelled' | 'settled';
export type WorkflowStatus = 'draft' | 'active' | 'inactive';

export type TenantStatus = 'active' | 'inactive';

export interface Tenant {
  id: string;
  name: string;
  identifier: string;
  description: string;
  status: TenantStatus;
  contactName: string;
   contactEmail: string;
   contactPhoneCode: string;
   contactPhone: string;
  country: string;
  createdAt: string;
}

export interface Entity {
  id: string;
  tenantId: string;
  tenantName: string;
  name: string;
  identifier: string;
  type: 'bank' | 'insurance' | 'fintech' | 'cooperative';
  status: EntityStatus;
   email: string;
   phoneCode: string;
   phone: string;
  address: string;
  city: string;
  province: string;
  country: string;
  createdAt: string;
  branches: Branch[];
  channels: ChannelType[];
}

export interface Branch {
  id: string;
  entityId: string;
  name: string;
  code: string;
  address: string;
  city: string;
  province: string;
  status: EntityStatus;
   manager: string;
   phoneCode: string;
   phone: string;
}

export interface UserAssignment {
  organizationIds: string[];
  entityIds: string[];
  branchIds: string[];
}

export interface User {
  id: string;
  username: string;
  password: string;
  email: string;
  firstName: string;
  lastName: string;
  entityId: string;
  entityName: string;
  roleIds: string[];
  roleNames: string[];
  status: UserStatus;
  lastLogin: string;
  createdAt: string;
  avatar?: string;
  /** Hierarchical assignments — multiple levels */
  assignments?: UserAssignment;
}

export interface Permission {
  id: string;
  module: string;
  action: string;
  description: string;
}

export interface Role {
  id: string;
  name: string;
  description: string;
  permissions: string[];
  userCount: number;
}

export type AuditOperationType = 'Crear' | 'Editar' | 'Eliminar' | 'Login' | 'Logout' | 'Cambiar Contraseña' | 'Cambiar Estado' | 'Liquidar' | 'Exportar' | 'Consultar' | 'Otro';

export interface AuditEntry {
  id: string;
  userId: string;
  userName: string;
  operationType: AuditOperationType;
  action: string;
  module: string;
  detail: string;
  timestamp: string;
  ip: string;
}

// ── Product Family ──

export interface ProductFamily {
  id: string;
  code: string;       // max 15 chars
  description: string; // max 30 chars
}

// ── Plan de Comisiones ──

/** Tipo de comisión (define cómo interpretar `value`) */
export type CommissionValueType =
  | 'fixed_per_sale'
  | 'percentage_capital'
  | 'percentage_total_loan';

type CommissionPlanBase = {
  id: string;
  code: string;        // max 15 chars
  description: string; // max 50 chars
  /**
   * Valor numérico de la comisión:
   * - fixed_per_sale: monto (moneda)
   * - percentage_*: porcentaje (0-100)
   */
  value: number;
  maxAmount?: number;
};

export type CommissionPlan =
  | (CommissionPlanBase & { valueType: 'fixed_per_sale' })
  | (CommissionPlanBase & { valueType: 'percentage_capital' | 'percentage_total_loan' });

// ── Product Catalog ──

export interface ProductRequirement {
  id: string;
  name: string;
  type: 'document' | 'data' | 'validation';
  mandatory: boolean;
  description: string;
}

export interface Coverage {
  id: string;
  name: string;
  description: string;
  sumInsured?: number;
  premium?: number;
}

export type AmortizationType = 'french' | 'german' | 'american' | 'bullet';

export interface LoanAttributes {
  amortizationType: AmortizationType;
  annualEffectiveRate: number;
  cftRate: number;
  adminFees: number;
}

export interface InsuranceAttributes {
  premium: number;
  sumInsured: number;
  gracePeriodDays: number;
  coverageType: 'individual' | 'group' | 'collective';
}

export interface AccountAttributes {
  maintenanceFee: number;
  minimumBalance: number;
  interestRate: number;
  accountType: 'savings' | 'checking' | 'money_market';
}

export type CardNetwork = 'visa' | 'mastercard' | 'amex' | 'cabal' | 'naranja';

export interface CardAttributes {
  creditLimit: number;
  annualFee: number;
  interestRate: number;
  network: CardNetwork;
  gracePeriodDays: number;
  level?: string;
}

export type RiskLevel = 'low' | 'medium' | 'high';
export type InstrumentType = 'fixed_term' | 'bond' | 'mutual_fund' | 'stock';

export interface InvestmentAttributes {
  minimumAmount: number;
  expectedReturn: number;
  termDays: number;
  riskLevel: RiskLevel;
  instrumentType: InstrumentType;
}

export interface ProductPlan {
  id: string;
  productId: string;
  name: string;
  description: string;
  coverages: Coverage[];
  price: number;
  currency: string;
  installments?: number;
  /** Relación 1:1 con Plan de comisiones */
  commissionPlanId: string;
  status: ProductStatus;
  otherCosts?: number;
  loanAttributes?: LoanAttributes;
  insuranceAttributes?: InsuranceAttributes;
  accountAttributes?: AccountAttributes;
  cardAttributes?: CardAttributes;
  investmentAttributes?: InvestmentAttributes;
}

export interface Product {
  id: string;
  entityId: string;
  entityName: string;
  familyId: string;
  familyName: string;
  name: string;
  code: string;
  description: string;
  version: number;
  status: ProductStatus;
  plans: ProductPlan[];
  requirements: ProductRequirement[];
  validFrom: string;
  validTo?: string;
  createdAt: string;
}

// ── Applications ──

export type Gender = 'male' | 'female' | 'other' | 'not_specified';

export interface Applicant {
  firstName: string;
  lastName: string;
  documentType: 'DNI' | 'CUIT' | 'passport';
  documentNumber: string;
   email: string;
   phoneCode: string;
   phone: string;
  birthDate?: string;
  gender?: Gender;
  occupation?: string;
}

export interface Beneficiary {
  firstName: string;
  lastName: string;
  relationship: string;
  percentage: number;
}

export interface ApplicationObservation {
  id: string;
  userId: string;
  userName: string;
  text: string;
  timestamp: string;
}

export interface ApplicationDocument {
  id: string;
  name: string;
  type: string;
  status: 'pending' | 'approved' | 'rejected';
  uploadedAt: string;
}

export interface ApplicationAddress {
  street: string;
  number: string;
  floor?: string;
  apartment?: string;
  city: string;
  province: string;
  postalCode: string;
  latitude?: number;
  longitude?: number;
}

export interface TraceEvent {
  id: string;
  workflowStateId: string;
  workflowStateName: string;
  action: string;
  userName: string;
  timestamp: string;
  detail?: string;
}

export interface Application {
  id: string;
  code: string;
  entityId: string;
  entityName: string;
  productId: string;
  productName: string;
  planId: string;
  planName: string;
  applicant: Applicant;
  beneficiaries: Beneficiary[];
  status: ApplicationStatus;
  currentWorkflowStateId: string;
  currentWorkflowStateName: string;
  observations: ApplicationObservation[];
  documents: ApplicationDocument[];
  traceEvents: TraceEvent[];
  address?: ApplicationAddress;
  assignedUserId?: string;
  assignedUserName?: string;
  createdAt: string;
  updatedAt: string;
}

// ── Workflow ──

export interface WorkflowTransition {
  id: string;
  fromStateId: string;
  toStateId: string;
  name: string;
  requiredRole?: string;
  autoTransition: boolean;
}

export interface WorkflowState {
  id: string;
  name: string;
  type: 'initial' | 'intermediate' | 'final';
  slaHours?: number;
  color: string;
}

export interface WorkflowDefinition {
  id: string;
  name: string;
  description: string;
  version: number;
  status: WorkflowStatus;
  states: WorkflowState[];
  transitions: WorkflowTransition[];
  productCategory: string;
  createdAt: string;
}

// ── Settlement History ──

export interface SettlementHistoryItem {
  applicationId: string;
  code: string;
  entityName: string;
  applicantName: string;
  applicantDoc: string;
  commissionPlanName: string;
  commissionFormula: string;
  commission: number;
  currency: string;
}

export interface SettlementHistoryEntry {
  id: string;
  settledAt: string;
  userId: string;
  userName: string;
  operationCount: number;
  totalsByCurrency: { currency: string; total: number }[];
  items: SettlementHistoryItem[];
  /** Base64 data URL of the generated Excel file */
  excelDataUrl?: string;
}

// Helper: compute merged permissions from multiple roles
export function getMergedPermissions(roleIds: string[], roles: Role[]): string[] {
  const permSet = new Set<string>();
  roleIds.forEach(rid => {
    const role = roles.find(r => r.id === rid);
    role?.permissions.forEach(p => permSet.add(p));
  });
  return [...permSet];
}
