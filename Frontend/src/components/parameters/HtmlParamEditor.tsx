import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { cn } from "@/lib/utils";
import {
  Bold, Italic, Underline, List, ListOrdered, Heading2, Code, Eye, Pencil,
  BookOpen, Search, Copy, Check
} from "lucide-react";

const variableDictionary = [
  {
    category: "Solicitante",
    variables: [
      { key: "{{solicitante.nombre}}", desc: "Nombre del solicitante" },
      { key: "{{solicitante.apellido}}", desc: "Apellido del solicitante" },
      { key: "{{solicitante.documento_tipo}}", desc: "Tipo de documento" },
      { key: "{{solicitante.documento_numero}}", desc: "Número de documento" },
      { key: "{{solicitante.email}}", desc: "Email del solicitante" },
      { key: "{{solicitante.telefono}}", desc: "Teléfono del solicitante" },
      { key: "{{solicitante.fecha_nacimiento}}", desc: "Fecha de nacimiento" },
      { key: "{{solicitante.genero}}", desc: "Género" },
      { key: "{{solicitante.ocupacion}}", desc: "Ocupación" },
    ],
  },
  {
    category: "Solicitud",
    variables: [
      { key: "{{solicitud.codigo}}", desc: "Código de la solicitud" },
      { key: "{{solicitud.estado}}", desc: "Estado actual" },
      { key: "{{solicitud.fecha_creacion}}", desc: "Fecha de creación" },
      { key: "{{solicitud.fecha_actualizacion}}", desc: "Última actualización" },
      { key: "{{solicitud.workflow_estado}}", desc: "Estado del workflow" },
      { key: "{{solicitud.usuario_asignado}}", desc: "Usuario asignado" },
    ],
  },
  {
    category: "Dirección",
    variables: [
      { key: "{{direccion.calle}}", desc: "Calle" },
      { key: "{{direccion.numero}}", desc: "Número" },
      { key: "{{direccion.piso}}", desc: "Piso" },
      { key: "{{direccion.departamento}}", desc: "Departamento" },
      { key: "{{direccion.ciudad}}", desc: "Ciudad" },
      { key: "{{direccion.provincia}}", desc: "Provincia" },
      { key: "{{direccion.codigo_postal}}", desc: "Código postal" },
    ],
  },
  {
    category: "Producto",
    variables: [
      { key: "{{producto.nombre}}", desc: "Nombre del producto" },
      { key: "{{producto.codigo}}", desc: "Código del producto" },
      { key: "{{producto.familia}}", desc: "Familia del producto" },
      { key: "{{producto.version}}", desc: "Versión" },
    ],
  },
  {
    category: "Plan",
    variables: [
      { key: "{{plan.nombre}}", desc: "Nombre del plan" },
      { key: "{{plan.precio}}", desc: "Precio del plan" },
      { key: "{{plan.moneda}}", desc: "Moneda" },
      { key: "{{plan.cuotas}}", desc: "Cantidad de cuotas" },
    ],
  },
  {
    category: "Organización",
    variables: [
      { key: "{{organizacion.nombre}}", desc: "Nombre de la organización" },
      { key: "{{organizacion.identificador}}", desc: "Identificador" },
      { key: "{{organizacion.pais}}", desc: "País" },
      { key: "{{organizacion.contacto_nombre}}", desc: "Nombre de contacto" },
      { key: "{{organizacion.contacto_email}}", desc: "Email de contacto" },
    ],
  },
  {
    category: "Entidad",
    variables: [
      { key: "{{entidad.nombre}}", desc: "Nombre de la entidad" },
      { key: "{{entidad.tipo}}", desc: "Tipo de entidad" },
      { key: "{{entidad.email}}", desc: "Email" },
      { key: "{{entidad.telefono}}", desc: "Teléfono" },
      { key: "{{entidad.direccion}}", desc: "Dirección" },
      { key: "{{entidad.ciudad}}", desc: "Ciudad" },
      { key: "{{entidad.provincia}}", desc: "Provincia" },
    ],
  },
  {
    category: "Sucursal",
    variables: [
      { key: "{{sucursal.nombre}}", desc: "Nombre de la sucursal" },
      { key: "{{sucursal.codigo}}", desc: "Código" },
      { key: "{{sucursal.direccion}}", desc: "Dirección" },
      { key: "{{sucursal.responsable}}", desc: "Responsable" },
    ],
  },
  {
    category: "Usuario",
    variables: [
      { key: "{{usuario.nombre}}", desc: "Nombre completo del usuario" },
      { key: "{{usuario.email}}", desc: "Email del usuario" },
      { key: "{{usuario.rol}}", desc: "Rol del usuario" },
    ],
  },
  {
    category: "Sistema",
    variables: [
      { key: "{{sistema.fecha}}", desc: "Fecha actual" },
      { key: "{{sistema.hora}}", desc: "Hora actual" },
      { key: "{{sistema.url}}", desc: "URL del sistema" },
      { key: "{{sistema.nombre}}", desc: "Nombre del sistema" },
    ],
  },
];

interface HtmlParamEditorProps {
  value: string;
  onChange: (value: string) => void;
}

export default function HtmlParamEditor({ value, onChange }: HtmlParamEditorProps) {
  const [viewMode, setViewMode] = useState<"edit" | "preview">("edit");
  const [varSearch, setVarSearch] = useState("");
  const [copiedVar, setCopiedVar] = useState<string | null>(null);

  const insertTag = (before: string, after: string) => {
    const textarea = document.getElementById("html-editor") as HTMLTextAreaElement | null;
    if (!textarea) return;
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selected = value.substring(start, end);
    const newValue = value.substring(0, start) + before + selected + after + value.substring(end);
    onChange(newValue);
    setTimeout(() => {
      textarea.focus();
      textarea.setSelectionRange(start + before.length, start + before.length + selected.length);
    }, 0);
  };

  const insertVariable = (varKey: string) => {
    const textarea = document.getElementById("html-editor") as HTMLTextAreaElement | null;
    if (!textarea) {
      onChange(value + varKey);
      return;
    }
    const start = textarea.selectionStart;
    const newValue = value.substring(0, start) + varKey + value.substring(start);
    onChange(newValue);
    setTimeout(() => {
      textarea.focus();
      textarea.setSelectionRange(start + varKey.length, start + varKey.length);
    }, 0);
  };

  const copyVariable = (varKey: string) => {
    navigator.clipboard.writeText(varKey);
    setCopiedVar(varKey);
    setTimeout(() => setCopiedVar(null), 1500);
  };

  const filteredVars = variableDictionary.map(cat => ({
    ...cat,
    variables: cat.variables.filter(
      v => v.key.toLowerCase().includes(varSearch.toLowerCase()) || v.desc.toLowerCase().includes(varSearch.toLowerCase())
    ),
  })).filter(cat => cat.variables.length > 0);

  return (
    <div className="space-y-2 w-full">
      <div className="flex items-center gap-1 flex-wrap">
        {/* Toolbar */}
        <div className="flex items-center gap-0.5 border border-border rounded-md p-0.5 bg-muted/30">
          <Button type="button" variant="ghost" size="icon" className="h-7 w-7" title="Negrita" onClick={() => insertTag("<strong>", "</strong>")}>
            <Bold className="h-3.5 w-3.5" />
          </Button>
          <Button type="button" variant="ghost" size="icon" className="h-7 w-7" title="Cursiva" onClick={() => insertTag("<em>", "</em>")}>
            <Italic className="h-3.5 w-3.5" />
          </Button>
          <Button type="button" variant="ghost" size="icon" className="h-7 w-7" title="Subrayado" onClick={() => insertTag("<u>", "</u>")}>
            <Underline className="h-3.5 w-3.5" />
          </Button>
          <div className="w-px h-5 bg-border mx-0.5" />
          <Button type="button" variant="ghost" size="icon" className="h-7 w-7" title="Título" onClick={() => insertTag("<h2>", "</h2>")}>
            <Heading2 className="h-3.5 w-3.5" />
          </Button>
          <Button type="button" variant="ghost" size="icon" className="h-7 w-7" title="Lista" onClick={() => insertTag("<ul>\n  <li>", "</li>\n</ul>")}>
            <List className="h-3.5 w-3.5" />
          </Button>
          <Button type="button" variant="ghost" size="icon" className="h-7 w-7" title="Lista numerada" onClick={() => insertTag("<ol>\n  <li>", "</li>\n</ol>")}>
            <ListOrdered className="h-3.5 w-3.5" />
          </Button>
          <Button type="button" variant="ghost" size="icon" className="h-7 w-7" title="Párrafo" onClick={() => insertTag("<p>", "</p>")}>
            <Code className="h-3.5 w-3.5" />
          </Button>
        </div>

        <div className="flex items-center gap-1 ml-auto">
          {/* Variable Dictionary */}
          <Popover>
            <PopoverTrigger asChild>
              <Button type="button" variant="outline" size="sm" className="h-7 gap-1.5 text-xs">
                <BookOpen className="h-3.5 w-3.5" />
                Variables
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-96 p-0" align="end">
              <div className="p-3 border-b border-border">
                <div className="relative">
                  <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground" />
                  <Input
                    placeholder="Buscar variable..."
                    value={varSearch}
                    onChange={e => setVarSearch(e.target.value)}
                    className="h-8 text-xs pl-8"
                  />
                </div>
              </div>
              <ScrollArea className="h-80">
                <div className="p-2 space-y-2">
                  {filteredVars.map(cat => (
                    <div key={cat.category}>
                      <p className="text-[10px] font-bold text-muted-foreground uppercase tracking-wider px-2 py-1">{cat.category}</p>
                      <div className="space-y-0.5">
                        {cat.variables.map(v => (
                          <div
                            key={v.key}
                            className="flex items-center gap-2 px-2 py-1.5 rounded-md hover:bg-muted group cursor-pointer"
                            onClick={() => insertVariable(v.key)}
                          >
                            <code className="text-[11px] font-mono bg-muted px-1.5 py-0.5 rounded text-primary flex-shrink-0">{v.key}</code>
                            <span className="text-[11px] text-muted-foreground truncate flex-1">{v.desc}</span>
                            <Button
                              type="button"
                              variant="ghost"
                              size="icon"
                              className="h-5 w-5 opacity-0 group-hover:opacity-100 shrink-0"
                              onClick={e => { e.stopPropagation(); copyVariable(v.key); }}
                            >
                              {copiedVar === v.key ? <Check className="h-3 w-3 text-green-500" /> : <Copy className="h-3 w-3" />}
                            </Button>
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              </ScrollArea>
              <div className="p-2 border-t border-border">
                <p className="text-[10px] text-muted-foreground text-center">
                  Clic para insertar · Las variables se reemplazan automáticamente al enviar
                </p>
              </div>
            </PopoverContent>
          </Popover>

          {/* View toggle */}
          <div className="flex items-center border border-border rounded-md overflow-hidden">
            <Button
              type="button"
              variant={viewMode === "edit" ? "default" : "ghost"}
              size="sm"
              className="h-7 rounded-none text-xs gap-1 px-2"
              onClick={() => setViewMode("edit")}
            >
              <Pencil className="h-3 w-3" /> Código
            </Button>
            <Button
              type="button"
              variant={viewMode === "preview" ? "default" : "ghost"}
              size="sm"
              className="h-7 rounded-none text-xs gap-1 px-2"
              onClick={() => setViewMode("preview")}
            >
              <Eye className="h-3 w-3" /> Vista previa
            </Button>
          </div>
        </div>
      </div>

      {viewMode === "edit" ? (
        <textarea
          id="html-editor"
          value={value}
          onChange={e => onChange(e.target.value)}
          className="w-full min-h-[200px] rounded-md border border-input bg-background px-3 py-2 text-sm font-mono ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 resize-y"
          placeholder="<p>Escriba el contenido HTML aquí...</p>"
        />
      ) : (
        <div className="w-full min-h-[200px] rounded-md border border-input bg-background px-4 py-3 text-sm overflow-auto">
          <div dangerouslySetInnerHTML={{ __html: value }} />
        </div>
      )}

      {/* Variable badges in content */}
      {value && (() => {
        const matches = value.match(/\{\{[^}]+\}\}/g);
        if (!matches || matches.length === 0) return null;
        const unique = [...new Set(matches)];
        return (
          <div className="flex items-center gap-1.5 flex-wrap">
            <span className="text-[10px] text-muted-foreground">Variables en uso:</span>
            {unique.map(v => (
              <Badge key={v} variant="secondary" className="text-[10px] font-mono px-1.5 py-0">{v}</Badge>
            ))}
          </div>
        );
      })()}
    </div>
  );
}
