import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { ArrowLeft, KeyRound, Smartphone, CheckCircle2, ShieldCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { InputOTP, InputOTPGroup, InputOTPSlot } from "@/components/ui/input-otp";
import { Badge } from "@/components/ui/badge";
import { useAuthStore } from "@/data/authStore";
import { mockParameters } from "@/data/parameters";
import { toast } from "sonner";

const passwordMask = mockParameters.find(p => p.key === "mask.password");
const passwordRegex = passwordMask
  ? new RegExp(passwordMask.value)
  : /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,50}$/;

const schema = z.object({
  currentPassword: z.string().min(1, "Ingrese su contraseña actual"),
  newPassword: z.string()
    .min(8, "Mínimo 8 caracteres")
    .max(50)
    .regex(passwordRegex, passwordMask?.description || "Debe contener mayúscula, minúscula, número y carácter especial"),
  confirmPassword: z.string().min(1, "Confirme la nueva contraseña"),
}).refine(d => d.newPassword === d.confirmPassword, {
  message: "Las contraseñas no coinciden",
  path: ["confirmPassword"],
});

type FormData = z.infer<typeof schema>;

type Step = "password" | "otp" | "done";

export default function ChangePasswordPage() {
  const navigate = useNavigate();
  const { user, sendOtp, verifyOtp, changePassword, otpSent, otpCode } = useAuthStore();
  const [step, setStep] = useState<Step>("password");
  const [otpValue, setOtpValue] = useState("");
  const [mockOtpDisplay, setMockOtpDisplay] = useState("");
  const [formData, setFormData] = useState<FormData | null>(null);

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = (data: FormData) => {
    if (!user) return;
    // Validate current password
    if (data.currentPassword !== user.password) {
      toast.error("La contraseña actual es incorrecta");
      return;
    }
    setFormData(data);
    // Send OTP
    const code = sendOtp();
    setMockOtpDisplay(code);
    setStep("otp");
    toast.info("Se envió un código OTP a su teléfono");
  };

  const handleVerifyOtp = () => {
    if (verifyOtp(otpValue)) {
      if (formData && user) {
        changePassword(user.id, formData.newPassword);
        setStep("done");
        toast.success("Contraseña actualizada exitosamente");
      }
    } else {
      toast.error("Código OTP incorrecto");
    }
  };

  if (!user) {
    navigate("/login");
    return null;
  }

  return (
    <div className="space-y-6 max-w-lg">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="icon" onClick={() => navigate("/")}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">Cambiar Contraseña</h1>
      </div>

      {/* Step indicator */}
      <div className="flex items-center gap-3">
        <Badge variant={step === "password" ? "default" : "secondary"} className="gap-1">
          <KeyRound className="h-3 w-3" /> 1. Contraseña
        </Badge>
        <div className="h-px w-6 bg-border" />
        <Badge variant={step === "otp" ? "default" : "secondary"} className="gap-1">
          <Smartphone className="h-3 w-3" /> 2. Verificación OTP
        </Badge>
        <div className="h-px w-6 bg-border" />
        <Badge variant={step === "done" ? "default" : "secondary"} className="gap-1">
          <CheckCircle2 className="h-3 w-3" /> 3. Listo
        </Badge>
      </div>

      {step === "password" && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              <KeyRound className="h-4 w-4 text-primary" />
              Ingrese las contraseñas
            </CardTitle>
            <p className="text-xs text-muted-foreground">
              Luego se le enviará un código OTP por SMS para confirmar el cambio.
            </p>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="space-y-1.5">
                <Label>Contraseña Actual *</Label>
                <Input {...register("currentPassword")} type="password" />
                {errors.currentPassword && <p className="text-xs text-destructive">{errors.currentPassword.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Nueva Contraseña * <span className="text-xs text-muted-foreground font-normal">({passwordMask?.description || "8+ caracteres, mayúscula, número, especial"})</span></Label>
                <Input {...register("newPassword")} type="password" />
                {errors.newPassword && <p className="text-xs text-destructive">{errors.newPassword.message}</p>}
              </div>
              <div className="space-y-1.5">
                <Label>Confirmar Nueva Contraseña *</Label>
                <Input {...register("confirmPassword")} type="password" />
                {errors.confirmPassword && <p className="text-xs text-destructive">{errors.confirmPassword.message}</p>}
              </div>
              <Button type="submit" className="w-full">
                <Smartphone className="h-4 w-4 mr-2" />
                Enviar Código OTP
              </Button>
            </form>
          </CardContent>
        </Card>
      )}

      {step === "otp" && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-base flex items-center gap-2">
              <ShieldCheck className="h-4 w-4 text-primary" />
              Verificación por SMS
            </CardTitle>
            <p className="text-xs text-muted-foreground">
              Ingrese el código de 6 dígitos enviado a su teléfono.
            </p>
          </CardHeader>
          <CardContent className="space-y-4">
            {/* Mock OTP display for demo */}
            <div className="bg-muted/50 border border-border rounded-lg p-3 text-center">
              <p className="text-xs text-muted-foreground mb-1">📱 Código SMS (demo)</p>
              <p className="text-2xl font-mono font-bold tracking-[0.3em] text-foreground">{mockOtpDisplay}</p>
            </div>

            <div className="flex justify-center">
              <InputOTP maxLength={6} value={otpValue} onChange={setOtpValue}>
                <InputOTPGroup>
                  <InputOTPSlot index={0} />
                  <InputOTPSlot index={1} />
                  <InputOTPSlot index={2} />
                  <InputOTPSlot index={3} />
                  <InputOTPSlot index={4} />
                  <InputOTPSlot index={5} />
                </InputOTPGroup>
              </InputOTP>
            </div>

            <Button
              onClick={handleVerifyOtp}
              className="w-full"
              disabled={otpValue.length !== 6}
            >
              <ShieldCheck className="h-4 w-4 mr-2" />
              Verificar y Cambiar Contraseña
            </Button>

            <Button variant="ghost" className="w-full text-xs" onClick={() => {
              const code = sendOtp();
              setMockOtpDisplay(code);
              setOtpValue("");
              toast.info("Nuevo código OTP enviado");
            }}>
              Reenviar código
            </Button>
          </CardContent>
        </Card>
      )}

      {step === "done" && (
        <Card>
          <CardContent className="py-8 text-center space-y-4">
            <div className="mx-auto h-16 w-16 rounded-full bg-success/10 flex items-center justify-center">
              <CheckCircle2 className="h-8 w-8 text-success" />
            </div>
            <div>
              <h3 className="text-lg font-semibold text-foreground">Contraseña actualizada</h3>
              <p className="text-sm text-muted-foreground mt-1">
                Su contraseña ha sido cambiada exitosamente.
              </p>
            </div>
            <Button onClick={() => navigate("/")} className="mt-4">
              Volver al Inicio
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
