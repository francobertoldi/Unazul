# 06 — Matriz de Pruebas RF

> **Proyecto:** Unazul Backoffice
> **Version:** 2.0.0
> **Fecha:** 2026-03-15

---

## 1. Resumen General

| Modulo | Archivo TP | RFs cubiertos | Tests totales | Estado |
|--------|-----------|---------------|---------------|--------|
| Seguridad e Identidad | [TP-SEC](pruebas/TP-SEC.md) | RF-SEC-01 a RF-SEC-12 | 159 | Documentado |
| Organizacion | [TP-ORG](pruebas/TP-ORG.md) | RF-ORG-01 a RF-ORG-10 | 135 | Documentado |
| Catalogo | [TP-CAT](pruebas/TP-CAT.md) | RF-CAT-01 a RF-CAT-09 | 126 | Documentado |
| Operaciones | [TP-OPS](pruebas/TP-OPS.md) | RF-OPS-01 a RF-OPS-18 | 195 | Documentado (FL-OPS-01, FL-OPS-02, FL-OPS-03) |
| Configuracion | [TP-CFG](pruebas/TP-CFG.md) | RF-CFG-01 a RF-CFG-18 | 171 | Documentado |
| Auditoria | [TP-AUD](pruebas/TP-AUD.md) | RF-AUD-01 a RF-AUD-03 | 50 | Documentado |

---

## 2. Cobertura por Tipo de Test

| Modulo | Positivos | Negativos | Integracion | E2E | Total |
|--------|-----------|-----------|-------------|-----|-------|
| SEC (auth+users) | 18 | 38 | 21 | 16 | 93 |
| SEC (roles) | 14 | 22 | 14 | 16 | 66 |
| ORG (organizaciones) | 13 | 23 | 9 | 5 | 50 |
| ORG (entidades+sucursales) | 27 | 30 | 15 | 13 | 85 |
| CFG (parametros+servicios) | 29 | 33 | 17 | 11 | 96 |
| CFG (workflows) | 22 | 32 | 14 | 7 | 75 |
| CAT (catalogo) | 41 | 51 | 23 | 11 | 126 |
| OPS (solicitudes FL-OPS-01) | 42 | 39 | 17 | 11 | 109 |
| OPS (liquidaciones FL-OPS-02) | 19 | 30 | 16 | 6 | 71 |
| OPS (mensajes FL-OPS-03) | 5 | 7 | 2 | 1 | 15 |
| AUD (auditoria) | 16 | 16 | 12 | 6 | 50 |
| **Total** | **246** | **321** | **160** | **103** | **836** |

---

## 3. Matriz RF → Test

| RF | TP-*-01 | TP-*-02 | TP-*-03 | TP-*-04 | TP-*-05+ | Total |
|----|---------|---------|---------|---------|----------|-------|
| RF-SEC-01 | Login OK | Reset attempts | 401 invalid | 401 not found | +9 | 13 |
| RF-SEC-02 | OTP OK | Resend | 401 wrong | 401 expired | +6 | 10 |
| RF-SEC-03 | Refresh OK | Perms updated | Reuse revoke | Expired | +6 | 10 |
| RF-SEC-04 | Send link | Reset OK | Opaque 200 | Token expired | +8 | 12 |
| RF-SEC-05 | Logout OK | Idempotent | Other user | No auth | +6 | 10 |
| RF-SEC-06 | List OK | Search | Filter status | No perm | +8 | 12 |
| RF-SEC-07 | Create OK | Update OK | Bcrypt | Dup username | +13 | 17 |
| RF-SEC-08 | Detail OK | Perms union | 404 | No perm | +5 | 9 |
| RF-SEC-09 | List roles | Counters | Search | Sort | +12 | 16 |
| RF-SEC-10 | Create role | Perms batch | Dup name | Empty perms | +14 | 18 |
| RF-SEC-11 | Edit role | Diff perms | Sys rename | Dup name | +15 | 19 |
| RF-SEC-12 | Delete role | 204 OK | Sys protect | In use 409 | +9 | 15 |
| RF-ORG-01 | List OK | Search ILIKE | Filter status | Export Excel | +8 | 12 |
| RF-ORG-02 | Create OK | Status inactive | 409 dup CUIT | 422 name empty | +7 | 11 |
| RF-ORG-03 | Edit OK | Detail+entities | Deactivate OK | 409 dup CUIT | +7 | 11 |
| RF-ORG-04 | Detail OK | Empty entities | 404 not found | 403 no perm | +4 | 8 |
| RF-ORG-05 | Delete OK | 409 has entities | 404 not found | 403 no perm | +4 | 8 |
| RF-ORG-06 | List entities | Search ILIKE | Filter type | Export xlsx | +11 | 15 |
| RF-ORG-07 | Create entity | Edit channels | 409 dup CUIT | Cascade prov | +18 | 22 |
| RF-ORG-08 | Detail entity | Channels+branches | user_count | 404 not found | +9 | 13 |
| RF-ORG-09 | Delete entity | 409 has branches | 204 no branches | RLS tenant | +9 | 13 |
| RF-ORG-10 | Create branch | Edit branch | Delete branch | 409 dup code | +18 | 22 |
| RF-CFG-01 | List groups | Sort order | 403 no perm | 401 no JWT | +2 | 6 |
| RF-CFG-02 | List params | Select opts | parent_key | 404 group | +4 | 8 |
| RF-CFG-03 | Create text | Select+opts | parent_key | 409 dup key | +6 | 10 |
| RF-CFG-04 | Edit value | updated_at | Select opts | 404 not found | +5 | 9 |
| RF-CFG-05 | Delete param | Cascade opts | 404 not found | 403 no perm | +3 | 7 |
| RF-CFG-06 | Create group | Delete group | 409 dup code | 409 has params | +4 | 8 |
| RF-CFG-07 | Filter hier | Empty result | 404 group | 403 no perm | +2 | 6 |
| RF-CFG-08 | List services | Test status | Empty list | 403 no perm | +4 | 8 |
| RF-CFG-09 | Create REST | Auth none | OAuth2 full | 409 dup name | +7 | 11 |
| RF-CFG-10 | Edit name | Keep creds | Change auth | 404 not found | +8 | 12 |
| RF-CFG-11 | Test success | Update tested | Restore active | Timeout fail | +8 | 12 |
| RF-CFG-12 | List wf OK | Search ILIKE | Filter status | 403 no perm | +6 | 10 |
| RF-CFG-13 | Create draft | Edit draft | Active→draft | GET complete | +9 | 13 |
| RF-CFG-14 | svc_call cfg | decision cfg | send_msg cfg | timer cfg | +7 | 11 |
| RF-CFG-15 | Panel 11 obj | Badges tipo | Click insert | Multi insert | +4 | 8 |
| RF-CFG-16 | Create trans | Label+cond | Bifurcacion | 422 reflexiva | +5 | 9 |
| RF-CFG-17 | Activate OK | Published evt | No start 422 | No end 422 | +12 | 16 |
| RF-CFG-18 | Deactivate OK | 409 draft | 409 inactive | 404 not found | +5 | 9 |
| RF-CAT-01 | List families | Create PREST | Create SEG | Edit desc | +12 | 16 |
| RF-CAT-02 | List products | Search ILIKE | Filter status | Filter family | +10 | 14 |
| RF-CAT-03 | Create product | Edit product | 422 entity | 403 admin ent | +12 | 16 |
| RF-CAT-04 | Detail loan | Detail insurance | Detail card | Empty plans | +7 | 11 |
| RF-CAT-05 | Plan loan | Plan insurance | Plan account | Plan card | +15 | 19 |
| RF-CAT-06 | Add coverage | Edit amounts | Delete cov | 422 non-ins | +8 | 12 |
| RF-CAT-07 | Add req doc | Add req data | Edit req | Delete req | +7 | 11 |
| RF-CAT-08 | List commissions | Create fixed | 409 dup code | 409 in use | +11 | 15 |
| RF-CAT-09 | Delete no plans | Deprecate OK | 409 has plans | 422 already dep | +8 | 12 |
| RF-OPS-01 | List OK | Filter status | Search ILIKE | Export Excel | +6 | 10 |
| RF-OPS-02 | Find existing | app_count OK | 404 not found | 422 doc_type | +3 | 7 |
| RF-OPS-03 | Create new app | Reuse applicant | 422 product | 503 catalog | +10 | 14 |
| RF-OPS-04 | Edit draft OK | Update fields | 409 not draft | 409 conflict | +5 | 9 |
| RF-OPS-05 | Transition OK | Optimistic lock | 422 invalid | 409 conflict | +9 | 13 |
| RF-OPS-06 | Detail 8 tabs | Lazy load | 404 not found | 403 no perm | +5 | 9 |
| RF-OPS-07 | Timeline OK | Grouped events | Empty trace | 404 not found | +4 | 8 |
| RF-OPS-08 | Add contact | Add address | Edit contact | Delete addr | +9 | 13 |
| RF-OPS-09 | Add beneficiary | Edit percent | 422 sum!=100 | Delete beneficiary | +5 | 9 |
| RF-OPS-10 | Upload doc | Download doc | 422 max size | Delete doc | +6 | 10 |
| RF-OPS-11 | Add observation | Internal flag | 403 no perm | 404 not found | +3 | 7 |
| RF-OPS-12 | Preview OK | Filter entity | Empty 200 | Multi currency | +10 | 14 |
| RF-OPS-13 | Confirm OK | Multi currency | Excel url 201 | 409 conflict | +11 | 15 |
| RF-OPS-14 | Excel 2 sheets | Path determ | Dir auto-create | FS fail NULL | +6 | 10 |
| RF-OPS-15 | Email vars OK | SMS override | WhatsApp OK | 422 template | +11 | 15 |
| RF-OPS-16 | List settle | Filter dates | Filter user | Paginate desc | +7 | 11 |
| RF-OPS-17 | Detail OK | Items JOIN | Multi currency | 404 not found | +6 | 10 |
| RF-OPS-18 | Download OK | Checksum file | 404 not found | 404 NULL url | +7 | 11 |
| RF-AUD-01 | List OK | Filter user | Filter operation | Filter module | +17 | 21 |
| RF-AUD-02 | Export xlsx | Export csv | Empty export | 422 limit 10k | +10 | 14 |
| RF-AUD-03 | Ingest OK | Optional nulls | ES down OK | tenant null DLQ | +11 | 15 |

---

## Changelog

### v2.0.0 (2026-03-15)
- RF-OPS-12 a RF-OPS-14, RF-OPS-16 a RF-OPS-18 agregados (71 tests para liquidacion de comisiones, FL-OPS-02)
- Total OPS: 195 tests. Total general: 836 tests

### v1.9.0 (2026-03-15)
- RF-OPS-15 agregado (15 tests para envio de mensajes, FL-OPS-03)
- Total OPS: 124 tests. Total general: 765 tests

### v1.8.0 (2026-03-15)
- RF-AUD-01 a RF-AUD-03 agregados (50 tests para auditoria)
- Total AUD: 50 tests. Total general: 750 tests
- Todos los modulos con plan de pruebas documentado

### v1.7.0 (2026-03-15)
- RF-OPS-01 a RF-OPS-11 agregados (109 tests para ciclo de vida de solicitud)
- Total OPS: 109 tests. Total general: 700 tests

### v1.6.0 (2026-03-15)
- RF-CAT-01 a RF-CAT-09 agregados (126 tests para catalogo de productos)
- Total CAT: 126 tests. Total general: 591 tests

### v1.5.0 (2026-03-15)
- RF-CFG-12 a RF-CFG-18 agregados (75 tests para workflows)
- Total CFG: 171 tests. Total general: 465 tests

### v1.4.0 (2026-03-15)
- RF-CFG-01 a RF-CFG-11 agregados (96 tests para parametros y servicios externos)
- Total general: 390 tests

### v1.3.0 (2026-03-15)
- RF-ORG-06 a RF-ORG-10 agregados (85 tests para entidades y sucursales)
- Total general: 294 tests

### v1.2.0 (2026-03-15)
- RF-ORG-01 a RF-ORG-05 agregados (50 tests para organizaciones)
- Total general: 209 tests

### v1.1.0 (2026-03-15)
- RF-SEC-09 a RF-SEC-12 agregados (66 tests para roles y permisos)
- Total SEC: 159 tests

### v1.0.0 (2026-03-15)
- Matriz inicial con modulo SEC (93 tests)
- Inventario de modulos pendientes
