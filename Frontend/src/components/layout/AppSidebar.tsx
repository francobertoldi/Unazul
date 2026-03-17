import {
  Building2,
  Users,
  Shield,
  LayoutDashboard,
  ClipboardList,
  Package,
  FileCheck,
  GitBranch,
  Settings,
  Unplug,
  Layers,
  FolderTree,
  BadgePercent,
  Network,
  Receipt,
  History,
} from "lucide-react";
import { NavLink } from "@/components/NavLink";
import { useLocation } from "react-router-dom";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarFooter,
  useSidebar,
} from "@/components/ui/sidebar";

const mainItems = [
  { title: "Dashboard", url: "/", icon: LayoutDashboard },
];

const orgItems = [
  { title: "Organizaciones", url: "/tenants", icon: Layers },
  { title: "Entidades", url: "/entidades", icon: Building2 },
  { title: "Estructura", url: "/estructura", icon: Network },
];

const opsItems = [
  { title: "Solicitudes", url: "/solicitudes", icon: FileCheck },
  { title: "Liquidación", url: "/liquidacion-comisiones", icon: Receipt },
  { title: "Historial de liquidaciones", url: "/historial-liquidaciones", icon: History },
];

const configItems = [
  { title: "Familia de productos", url: "/familias-productos", icon: FolderTree },
  { title: "Productos", url: "/productos", icon: Package },
  { title: "Parámetros", url: "/parametros", icon: Settings },
  { title: "Servicios", url: "/servicios", icon: Unplug },
  { title: "Planes de comisiones", url: "/planes-comisiones", icon: BadgePercent },
  { title: "Workflows", url: "/workflows", icon: GitBranch },
];

const secItems = [
  { title: "Usuarios", url: "/usuarios", icon: Users },
  { title: "Roles y Permisos", url: "/roles", icon: Shield },
  { title: "Auditoría", url: "/auditoria", icon: ClipboardList },
];
function SidebarSection({ label, items }: { label: string; items: typeof mainItems }) {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";
  const location = useLocation();

  return (
    <SidebarGroup>
      {!collapsed && label !== "General" && <SidebarGroupLabel className="text-sidebar-foreground/50 text-xs uppercase tracking-wider">{label}</SidebarGroupLabel>}
      <SidebarGroupContent>
        <SidebarMenu>
          {items.map((item) => (
            <SidebarMenuItem key={item.title}>
              <SidebarMenuButton asChild>
                <NavLink
                  to={item.url}
                  end={item.url === "/"}
                  className="flex items-center gap-3 rounded-lg px-3 py-2 text-sidebar-foreground/70 transition-all hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
                  activeClassName="bg-sidebar-primary text-sidebar-primary-foreground hover:bg-sidebar-primary hover:text-sidebar-primary-foreground shadow-md"
                >
                  <item.icon className="h-4 w-4 shrink-0" />
                  {!collapsed && <span className="text-sm font-medium">{item.title}</span>}
                </NavLink>
              </SidebarMenuButton>
            </SidebarMenuItem>
          ))}
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  );
}

export function AppSidebar() {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";

  return (
    <Sidebar collapsible="icon" className="border-r-0">
      <div className="flex h-14 items-center gap-2 px-4 border-b border-sidebar-border">
        <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground text-sm font-bold">
          U
        </div>
        {!collapsed && (
          <span className="text-sm font-semibold text-sidebar-foreground tracking-tight">
            Unazul
          </span>
        )}
      </div>
      <SidebarContent className="pt-2">
        <SidebarSection label="General" items={mainItems} />
        <SidebarSection label="Organización" items={orgItems} />
        <SidebarSection label="Operaciones" items={opsItems} />
        <SidebarSection label="Configuración" items={configItems} />
        <SidebarSection label="Seguridad" items={secItems} />
      </SidebarContent>
      <SidebarFooter className="p-3">
        {!collapsed && (
          <p className="text-[10px] text-sidebar-foreground/30 text-center">v1.0.0 · Backoffice</p>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
