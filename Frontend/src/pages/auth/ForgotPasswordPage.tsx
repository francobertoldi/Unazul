import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft, Mail, CheckCircle2, Shield } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useAuthStore } from "@/data/authStore";
import { toast } from "sonner";

const schema = z.object({
  email: z.string().trim().email("Ingrese un email válido"),
});

type FormData = z.infer<typeof schema>;

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const { requestPasswordReset } = useAuthStore();
  const [sent, setSent] = useState(false);
  const [sentEmail, setSentEmail] = useState("");

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = (data: FormData) => {
    const result = requestPasswordReset(data.email);
    if (result.success) {
      setSentEmail(data.email);
      setSent(true);
      toast.success("Se envió el enlace de recuperación");
    } else {
      toast.error(result.error);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-md space-y-6">
        <div className="text-center space-y-2">
          <div className="mx-auto h-16 w-16 rounded-2xl bg-primary flex items-center justify-center shadow-lg">
            <Shield className="h-8 w-8 text-primary-foreground" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground font-['Space_Grotesk']">
            Recuperar Contraseña
          </h1>
        </div>

        {!sent ? (
          <Card className="border-border/50 shadow-xl">
            <CardHeader className="pb-3">
              <CardTitle className="text-base flex items-center gap-2">
                <Mail className="h-4 w-4 text-primary" />
                Ingrese su email
              </CardTitle>
              <p className="text-xs text-muted-foreground">
                Le enviaremos un enlace para restablecer su contraseña.
              </p>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div className="space-y-1.5">
                  <Label>Email registrado *</Label>
                  <Input {...register("email")} type="email" placeholder="usuario@empresa.com" autoFocus />
                  {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
                </div>
                <Button type="submit" className="w-full">
                  <Mail className="h-4 w-4 mr-2" />
                  Enviar Enlace
                </Button>
                <Button type="button" variant="ghost" className="w-full" onClick={() => navigate("/login")}>
                  <ArrowLeft className="h-4 w-4 mr-2" />
                  Volver al Login
                </Button>
              </form>
            </CardContent>
          </Card>
        ) : (
          <Card className="border-border/50 shadow-xl">
            <CardContent className="py-8 text-center space-y-4">
              <div className="mx-auto h-16 w-16 rounded-full bg-success/10 flex items-center justify-center">
                <CheckCircle2 className="h-8 w-8 text-success" />
              </div>
              <div>
                <h3 className="text-lg font-semibold text-foreground">Enlace enviado</h3>
                <p className="text-sm text-muted-foreground mt-1">
                  Se envió un enlace de recuperación a <strong>{sentEmail}</strong>.
                  Revise su bandeja de entrada.
                </p>
              </div>
              <p className="text-xs text-muted-foreground">
                Si no recibe el email en unos minutos, verifique su carpeta de spam.
              </p>
              <Button variant="outline" onClick={() => navigate("/login")}>
                Volver al Login
              </Button>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
