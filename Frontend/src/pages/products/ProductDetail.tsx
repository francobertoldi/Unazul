import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Package, Pencil, Trash2, FileText, Shield } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { useProductStore } from "@/data/store";
import { DeleteConfirmDialog } from "@/components/crud/DeleteConfirmDialog";
import { useState } from "react";
import { toast } from "sonner";



export default function ProductDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { products, deleteProduct } = useProductStore();
  const product = products.find(p => p.id === id);
  const [deleteOpen, setDeleteOpen] = useState(false);

  if (!product) return <div className="flex items-center justify-center h-64"><p className="text-muted-foreground">Producto no encontrado</p></div>;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/productos")}><ArrowLeft className="h-4 w-4" /></Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <div className="h-10 w-10 rounded-xl bg-primary/10 flex items-center justify-center"><Package className="h-5 w-5 text-primary" /></div>
            <div>
              <h1 className="text-xl font-bold tracking-tight">{product.name}</h1>
              <p className="text-sm text-muted-foreground">{product.code} · v{product.version} · {product.entityName}</p>
            </div>
            <StatusBadge status={product.status} />
            {product.familyName && <Badge variant="outline">{product.familyName}</Badge>}
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" className="gap-2" onClick={() => navigate(`/productos/${id}/editar`)}><Pencil className="h-4 w-4" /> Editar</Button>
          <Button variant="outline" className="gap-2 text-destructive hover:text-destructive" onClick={() => setDeleteOpen(true)}><Trash2 className="h-4 w-4" /> Eliminar</Button>
        </div>
      </div>

      <p className="text-sm text-muted-foreground">{product.description}</p>
      <p className="text-xs text-muted-foreground">Vigencia desde: {product.validFrom}{product.validTo ? ` hasta ${product.validTo}` : ""}</p>

      <Tabs defaultValue="plans" className="space-y-4">
        <TabsList>
          <TabsTrigger value="plans">Sub productos ({product.plans.length})</TabsTrigger>
          <TabsTrigger value="requirements">Requisitos ({product.requirements.length})</TabsTrigger>
        </TabsList>

        <TabsContent value="plans">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {product.plans.map(plan => (
              <Card key={plan.id}>
                <CardHeader className="pb-2">
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-base">{plan.name}</CardTitle>
                    <StatusBadge status={plan.status} />
                  </div>
                  <p className="text-xs text-muted-foreground">{plan.description}</p>
                </CardHeader>
                <CardContent>
                  {plan.price > 0 && <p className="text-lg font-bold">{plan.currency} {plan.price.toLocaleString('es-AR')}<span className="text-xs text-muted-foreground font-normal">/mes</span></p>}
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
            {product.plans.length === 0 && <p className="text-sm text-muted-foreground col-span-2">No hay sub productos definidos</p>}
          </div>
        </TabsContent>

        <TabsContent value="requirements">
          <Card>
            <CardContent className="pt-6">
              <div className="space-y-3">
                {product.requirements.map(req => (
                  <div key={req.id} className="flex items-start gap-3">
                    <div className="h-8 w-8 rounded-lg bg-secondary flex items-center justify-center shrink-0">
                      <FileText className="h-4 w-4 text-muted-foreground" />
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <p className="text-sm font-medium">{req.name}</p>
                        <Badge variant={req.mandatory ? "default" : "outline"} className="text-[10px]">{req.mandatory ? "Obligatorio" : "Opcional"}</Badge>
                        <Badge variant="secondary" className="text-[10px]">{req.type === 'document' ? 'Documento' : req.type === 'data' ? 'Dato' : 'Validación'}</Badge>
                      </div>
                      <p className="text-xs text-muted-foreground">{req.description}</p>
                    </div>
                  </div>
                ))}
                {product.requirements.length === 0 && <p className="text-sm text-muted-foreground">No hay requisitos definidos</p>}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      <DeleteConfirmDialog open={deleteOpen} onOpenChange={setDeleteOpen} title="Eliminar Producto"
        description={`¿Está seguro de eliminar "${product.name}"?`}
        onConfirm={() => { deleteProduct(product.id); toast.success("Producto eliminado"); navigate("/productos"); }} />
    </div>
  );
}
