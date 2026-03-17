import { AuditEntry, AuditOperationType } from "./types";
import { mockAuditLog } from "./mock";

/** Module-level mutable audit data */
let auditData = [...mockAuditLog];

/** Current user getter - set by authStore to avoid circular deps */
let _getCurrentUser: (() => { id: string; name: string }) | null = null;

export function setAuditUserProvider(fn: () => { id: string; name: string }) {
  _getCurrentUser = fn;
}

function generateAuditId() {
  return `a_${Date.now()}_${Math.random().toString(36).slice(2, 7)}`;
}

export function addAuditEntry(operationType: AuditOperationType, action: string, module: string, detail: string) {
  const user = _getCurrentUser?.() ?? { id: "system", name: "Sistema" };
  const entry: AuditEntry = {
    id: generateAuditId(),
    userId: user.id,
    userName: user.name,
    operationType,
    action,
    module,
    detail,
    timestamp: new Date().toISOString(),
    ip: "127.0.0.1",
  };
  auditData = [entry, ...auditData];
  auditListeners.forEach(fn => fn());
}

const auditListeners = new Set<() => void>();
export function subscribeAudit(fn: () => void) {
  auditListeners.add(fn);
  return () => { auditListeners.delete(fn); };
}

export function getAuditData(): AuditEntry[] {
  return auditData;
}
