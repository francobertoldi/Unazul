import { useState, useCallback } from "react";
import { User } from "./types";
import { mockUsers } from "./mock";
import { addAuditEntry, setAuditUserProvider } from "./auditLog";

export interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  otpSent: boolean;
  otpCode: string; // mock OTP
  otpVerified: boolean;
}

let authState: AuthState = {
  user: null,
  isAuthenticated: false,
  otpSent: false,
  otpCode: "",
  otpVerified: false,
};

/** Expose auth state for audit logging (avoids circular hook deps) */
export function __getAuthState() { return authState; }

// Register user provider for audit system
setAuditUserProvider(() => {
  if (authState.user) return { id: authState.user.id, name: `${authState.user.firstName} ${authState.user.lastName}` };
  return { id: "system", name: "Sistema" };
});

export function useAuthStore() {
  const [state, setState] = useState<AuthState>(authState);
  const refresh = useCallback(() => setState({ ...authState }), []);

  const login = useCallback((username: string, password: string): { success: boolean; error?: string } => {
    const user = mockUsers.find(u => u.username === username && u.password === password);
    if (!user) return { success: false, error: "Usuario o contraseña incorrectos" };
    if (user.status === "locked") return { success: false, error: "La cuenta se encuentra bloqueada" };
    if (user.status === "inactive") return { success: false, error: "La cuenta se encuentra inactiva" };
    authState = { ...authState, user, isAuthenticated: true };
    refresh();
    addAuditEntry("Login", "Iniciar Sesión", "Seguridad", `Inicio de sesión de "${user.firstName} ${user.lastName}"`);
    return { success: true };
  }, [refresh]);

  const logout = useCallback(() => {
    const userName = authState.user ? `${authState.user.firstName} ${authState.user.lastName}` : "Sistema";
    addAuditEntry("Logout", "Cerrar Sesión", "Seguridad", `Cierre de sesión de "${userName}"`);
    authState = { user: null, isAuthenticated: false, otpSent: false, otpCode: "", otpVerified: false };
    refresh();
  }, [refresh]);

  /** Simulates sending OTP via SMS. Returns mock code for demo. */
  const sendOtp = useCallback((): string => {
    const code = String(Math.floor(100000 + Math.random() * 900000));
    authState = { ...authState, otpSent: true, otpCode: code, otpVerified: false };
    refresh();
    return code;
  }, [refresh]);

  const verifyOtp = useCallback((code: string): boolean => {
    if (code === authState.otpCode) {
      authState = { ...authState, otpVerified: true };
      refresh();
      return true;
    }
    return false;
  }, [refresh]);

  const resetOtp = useCallback(() => {
    authState = { ...authState, otpSent: false, otpCode: "", otpVerified: false };
    refresh();
  }, [refresh]);

  const changePassword = useCallback((userId: string, newPassword: string) => {
    const idx = mockUsers.findIndex(u => u.id === userId);
    if (idx >= 0) mockUsers[idx] = { ...mockUsers[idx], password: newPassword };
    authState = { ...authState, otpSent: false, otpCode: "", otpVerified: false };
    refresh();
    addAuditEntry("Cambiar Contraseña", "Cambiar Contraseña", "Seguridad", "Cambió su contraseña");
  }, [refresh]);

  const requestPasswordReset = useCallback((email: string): { success: boolean; error?: string } => {
    const user = mockUsers.find(u => u.email === email);
    if (!user) return { success: false, error: "No se encontró un usuario con ese email" };
    return { success: true };
  }, []);

  return { ...state, login, logout, sendOtp, verifyOtp, resetOtp, changePassword, requestPasswordReset };
}
