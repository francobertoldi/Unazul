import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { LogIn, Eye, EyeOff, KeyRound, Shield } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { useAuthStore } from "@/data/authStore";
import { toast } from "sonner";

const schema = z.object({
  username: z.string().min(1, "Ingrese su usuario"),
  password: z.string().min(1, "Ingrese su contraseña"),
});

type FormData = z.infer<typeof schema>;

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuthStore();
  const [showPassword, setShowPassword] = useState(false);

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = (data: FormData) => {
    const result = login(data.username, data.password);
    if (result.success) {
      toast.success("Bienvenido al sistema");
      navigate("/");
    } else {
      toast.error(result.error);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-md space-y-6">
        {/* Logo / Brand */}
        <div className="text-center space-y-2">
          <div className="mx-auto h-16 w-16 rounded-2xl bg-primary flex items-center justify-center shadow-lg">
            <Shield className="h-8 w-8 text-primary-foreground" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground font-['Space_Grotesk']">
            Unazul Backoffice
          </h1>
          <p className="text-sm text-muted-foreground">Ingrese sus credenciales para acceder</p>
        </div>

        <Card className="border-border/50 shadow-xl">
          <CardHeader className="pb-2">
            <div className="flex items-center gap-2 text-sm font-medium text-foreground">
              <LogIn className="h-4 w-4 text-primary" />
              Iniciar Sesión
            </div>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="space-y-1.5">
                <Label htmlFor="username">Usuario</Label>
                <Input
                  id="username"
                  {...register("username")}
                  placeholder="nombre.usuario"
                  autoFocus
                  autoComplete="username"
                />
                {errors.username && <p className="text-xs text-destructive">{errors.username.message}</p>}
              </div>

              <div className="space-y-1.5">
                <div className="flex items-center justify-between">
                  <Label htmlFor="password">Contraseña</Label>
                  <button
                    type="button"
                    onClick={() => navigate("/recuperar-clave")}
                    className="text-xs text-primary hover:underline"
                  >
                    ¿Olvidó su contraseña?
                  </button>
                </div>
                <div className="relative">
                  <Input
                    id="password"
                    {...register("password")}
                    type={showPassword ? "text" : "password"}
                    placeholder="••••••••"
                    autoComplete="current-password"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                  >
                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
                {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
              </div>

              <Button type="submit" className="w-full" disabled={isSubmitting}>
                <KeyRound className="h-4 w-4 mr-2" />
                Ingresar
              </Button>
            </form>
          </CardContent>
        </Card>

        {/* Demo hint */}
        <div className="text-center">
          <p className="text-xs text-muted-foreground">
            Demo: usuario <code className="bg-muted px-1.5 py-0.5 rounded text-xs font-mono">admin</code> / contraseña <code className="bg-muted px-1.5 py-0.5 rounded text-xs font-mono">Admin2024!</code>
          </p>
        </div>
      </div>
    </div>
  );
}
