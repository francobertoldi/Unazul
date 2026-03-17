import { lazy, Suspense } from "react";
import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AppLayout } from "@/components/layout/AppLayout";
import { useAuthStore } from "@/data/authStore";

const LoginPage = lazy(() => import("@/pages/auth/LoginPage"));
const ForgotPasswordPage = lazy(() => import("@/pages/auth/ForgotPasswordPage"));
const ChangePasswordPage = lazy(() => import("@/pages/auth/ChangePasswordPage"));

const Dashboard = lazy(() => import("@/pages/Dashboard"));
const TenantList = lazy(() => import("@/pages/tenants/TenantList"));
const TenantDetail = lazy(() => import("@/pages/tenants/TenantDetail"));
const TenantForm = lazy(() => import("@/pages/tenants/TenantForm"));
const EntityList = lazy(() => import("@/pages/entities/EntityList"));
const EntityDetail = lazy(() => import("@/pages/entities/EntityDetail"));
const EntityForm = lazy(() => import("@/pages/entities/EntityForm"));
const BranchForm = lazy(() => import("@/pages/entities/BranchForm"));
const UserList = lazy(() => import("@/pages/security/UserList"));
const UserDetail = lazy(() => import("@/pages/security/UserDetail"));
const UserForm = lazy(() => import("@/pages/security/UserForm"));
const RolesPage = lazy(() => import("@/pages/security/RolesPage"));
const RoleForm = lazy(() => import("@/pages/security/RoleForm"));
const ProductList = lazy(() => import("@/pages/products/ProductList"));
const ProductDetail = lazy(() => import("@/pages/products/ProductDetail"));
const ProductForm = lazy(() => import("@/pages/products/ProductForm"));
const ProductFamilyList = lazy(() => import("@/pages/products/ProductFamilyList"));
const ApplicationList = lazy(() => import("@/pages/applications/ApplicationList"));
const ApplicationDetail = lazy(() => import("@/pages/applications/ApplicationDetail"));
const ApplicationForm = lazy(() => import("@/pages/applications/ApplicationForm"));
const WorkflowList = lazy(() => import("@/pages/workflows/WorkflowList"));
const WorkflowEditor = lazy(() => import("@/pages/workflows/WorkflowEditor"));
const AuditPage = lazy(() => import("@/pages/AuditPage"));
const OrgTreePage = lazy(() => import("@/pages/organization/OrgTreePage"));
const ParametersPage = lazy(() => import("@/pages/config/ParametersPage"));
const ServicesPage = lazy(() => import("@/pages/config/ServicesPage"));
const CommissionPlanList = lazy(() => import("@/pages/config/CommissionPlanList"));
const CommissionSettlementList = lazy(() => import("@/pages/commissions/CommissionSettlementList"));
const CommissionSettlementDetail = lazy(() => import("@/pages/commissions/CommissionSettlementDetail"));
const SettlementHistoryList = lazy(() => import("@/pages/commissions/SettlementHistoryList"));
const NotFound = lazy(() => import("./pages/NotFound"));

const queryClient = new QueryClient();

function ProtectedRoutes() {
  const { isAuthenticated } = useAuthStore();
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  return <AppLayout />;
}

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <BrowserRouter>
        <Suspense fallback={
          <div className="flex flex-col items-center justify-center h-screen gap-4 bg-background">
            <div className="h-10 w-10 animate-spin rounded-full border-4 border-muted border-t-primary" />
            <p className="text-sm text-muted-foreground animate-pulse">Aguarde un instante por favor…</p>
          </div>
        }>
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/recuperar-clave" element={<ForgotPasswordPage />} />

            {/* Protected routes */}
            <Route element={<ProtectedRoutes />}>
              <Route path="/" element={<Dashboard />} />
              <Route path="/cambiar-clave" element={<ChangePasswordPage />} />
              {/* Organización */}
              <Route path="/tenants" element={<TenantList />} />
              <Route path="/tenants/nuevo" element={<TenantForm />} />
              <Route path="/tenants/:id" element={<TenantDetail />} />
              <Route path="/tenants/:id/editar" element={<TenantForm />} />
              <Route path="/estructura" element={<OrgTreePage />} />
              <Route path="/entidades" element={<EntityList />} />
              <Route path="/entidades/nuevo" element={<EntityForm />} />
              <Route path="/entidades/:id" element={<EntityDetail />} />
              <Route path="/entidades/:id/editar" element={<EntityForm />} />
              <Route path="/entidades/:entityId/sucursales/nuevo" element={<BranchForm />} />
              <Route path="/entidades/:entityId/sucursales/:branchId/editar" element={<BranchForm />} />
              {/* Seguridad */}
              <Route path="/usuarios" element={<UserList />} />
              <Route path="/usuarios/nuevo" element={<UserForm />} />
              <Route path="/usuarios/:id" element={<UserDetail />} />
              <Route path="/usuarios/:id/editar" element={<UserForm />} />
              <Route path="/roles" element={<RolesPage />} />
              <Route path="/roles/nuevo" element={<RoleForm />} />
              <Route path="/roles/:id/editar" element={<RoleForm />} />
              {/* Catálogo */}
              <Route path="/familias-productos" element={<ProductFamilyList />} />
              <Route path="/productos" element={<ProductList />} />
              <Route path="/productos/nuevo" element={<ProductForm />} />
              <Route path="/productos/:id" element={<ProductDetail />} />
              <Route path="/productos/:id/editar" element={<ProductForm />} />
              {/* Solicitudes */}
              <Route path="/solicitudes" element={<ApplicationList />} />
              <Route path="/solicitudes/nuevo" element={<ApplicationForm />} />
              <Route path="/solicitudes/:id" element={<ApplicationDetail />} />
              <Route path="/solicitudes/:id/editar" element={<ApplicationForm />} />
              {/* Liquidación de Comisiones */}
              <Route path="/liquidacion-comisiones" element={<CommissionSettlementList />} />
              <Route path="/liquidacion-comisiones/:id" element={<CommissionSettlementDetail />} />
              <Route path="/historial-liquidaciones" element={<SettlementHistoryList />} />
              {/* Workflow */}
              <Route path="/workflows" element={<WorkflowList />} />
              <Route path="/workflows/nuevo" element={<WorkflowEditor />} />
              <Route path="/workflows/:id/editar" element={<WorkflowEditor />} />
              {/* Auditoría */}
              <Route path="/auditoria" element={<AuditPage />} />
              {/* Configuración */}
              <Route path="/parametros" element={<ParametersPage />} />
              <Route path="/servicios" element={<ServicesPage />} />
              <Route path="/planes-comisiones" element={<CommissionPlanList />} />
            </Route>
            <Route path="*" element={<NotFound />} />
          </Routes>
        </Suspense>
      </BrowserRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
