import { useState, useMemo } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Label } from "@/components/ui/label";
import { Mail, MessageSquare, Smartphone, Send } from "lucide-react";
import { useParameterStore } from "@/data/store";
import { Application } from "@/data/types";
import { toast } from "sonner";

type MessageType = "email" | "sms" | "whatsapp";

interface SendMessageDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  type: MessageType;
  application: Application;
  productName?: string;
  planName?: string;
  entityName?: string;
}

const typeConfig: Record<MessageType, { label: string; icon: React.ElementType }> = {
  email: { label: "Email", icon: Mail },
  sms: { label: "SMS", icon: Smartphone },
  whatsapp: { label: "WhatsApp", icon: MessageSquare },
};

export default function SendMessageDialog({ open, onOpenChange, type, application, productName, planName, entityName }: SendMessageDialogProps) {
  const { parameters } = useParameterStore();
  const [selectedTemplate, setSelectedTemplate] = useState<string>("");

  const Icon = typeConfig[type].icon;

  // Get templates grouped by name, filter by format compatibility
  const templates = useMemo(() => {
    const tplParams = parameters.filter(p => p.group === "templates");
    // Extract unique template names (e.g., "bienvenida", "aprobacion")
    const names = new Set<string>();
    tplParams.forEach(p => {
      const match = p.key.match(/^templates\.(.+?)\.(titulo|contenido|formato)$/);
      if (match) names.add(match[1]);
    });

    return Array.from(names).map(name => {
      const titulo = tplParams.find(p => p.key === `templates.${name}.titulo`)?.value || name;
      const contenido = tplParams.find(p => p.key === `templates.${name}.contenido`)?.value || "";
      const formato = tplParams.find(p => p.key === `templates.${name}.formato`)?.value || "html";
      return { name, titulo, contenido, formato };
    }).filter(t => {
      // Email can use html templates; SMS/WhatsApp can use texto templates
      if (type === "email") return t.formato === "html";
      return t.formato === "texto";
    });
  }, [parameters, type]);

  // Build variable context from the application
  const variableContext = useMemo(() => {
    const app = application;
    return {
      "solicitante.nombre": app.applicant.firstName,
      "solicitante.apellido": app.applicant.lastName,
      "solicitante.email": app.applicant.email,
      "solicitante.telefono": app.applicant.phone,
      "solicitante.documento": `${app.applicant.documentType} ${app.applicant.documentNumber}`,
      "solicitud.codigo": app.code,
      "solicitud.estado": app.status,
      "producto.nombre": productName || app.productName,
      "plan.nombre": planName || app.planName,
      "plan.precio": "",
      "plan.moneda": "",
      "entidad.nombre": entityName || app.entityName,
      "organizacion.nombre": "BackOffice",
      "usuario.nombre": app.assignedUserName || "",
      "usuario.email": "",
      "sistema.fecha": new Date().toLocaleDateString("es-AR"),
      "sucursal.nombre": "",
    };
  }, [application, productName, planName, entityName]);

  const selectedTpl = templates.find(t => t.name === selectedTemplate);

  const resolvedContent = useMemo(() => {
    if (!selectedTpl) return "";
    let content = selectedTpl.contenido;
    Object.entries(variableContext).forEach(([key, value]) => {
      content = content.replace(new RegExp(`\\{\\{${key.replace(".", "\\.")}\\}\\}`, "g"), value || "—");
    });
    return content;
  }, [selectedTpl, variableContext]);

  const resolvedTitle = useMemo(() => {
    if (!selectedTpl) return "";
    let title = selectedTpl.titulo;
    Object.entries(variableContext).forEach(([key, value]) => {
      title = title.replace(new RegExp(`\\{\\{${key.replace(".", "\\.")}\\}\\}`, "g"), value || "—");
    });
    return title;
  }, [selectedTpl, variableContext]);

  const handleSend = () => {
    toast.success(`Mensaje de ${typeConfig[type].label} enviado correctamente`);
    setSelectedTemplate("");
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Icon className="h-5 w-5 text-primary" />
            Enviar {typeConfig[type].label}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <Label>Plantilla</Label>
            <Select value={selectedTemplate} onValueChange={setSelectedTemplate}>
              <SelectTrigger>
                <SelectValue placeholder="Seleccionar plantilla..." />
              </SelectTrigger>
              <SelectContent>
                {templates.length === 0 ? (
                  <div className="px-3 py-2 text-sm text-muted-foreground">
                    No hay plantillas de formato {type === "email" ? "HTML" : "Texto"} disponibles
                  </div>
                ) : (
                  templates.map(t => (
                    <SelectItem key={t.name} value={t.name}>{t.titulo}</SelectItem>
                  ))
                )}
              </SelectContent>
            </Select>
          </div>

          {selectedTpl && (
            <div className="space-y-3">
              <div>
                <Label className="text-xs text-muted-foreground">Asunto</Label>
                <p className="text-sm font-medium mt-1">{resolvedTitle}</p>
              </div>

              <div>
                <Label className="text-xs text-muted-foreground">Vista previa del mensaje</Label>
                <div className="mt-2 rounded-lg border p-4 max-h-[300px] overflow-y-auto bg-card">
                  {selectedTpl.formato === "html" ? (
                    <div className="prose prose-sm max-w-none" dangerouslySetInnerHTML={{ __html: resolvedContent }} />
                  ) : (
                    <p className="text-sm whitespace-pre-wrap">{resolvedContent}</p>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancelar</Button>
          <Button disabled={!selectedTemplate} onClick={handleSend} className="gap-2">
            <Send className="h-4 w-4" /> Enviar
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
