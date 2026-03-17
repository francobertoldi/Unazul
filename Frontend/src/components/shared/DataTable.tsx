import { useState, useMemo, useCallback, useRef, useEffect } from "react";
import { format } from "date-fns";
import { es } from "date-fns/locale";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Search, ChevronLeft, ChevronRight, Download, CalendarIcon, Filter, X, ChevronsUpDown, Check, ArrowUp, ArrowDown, ArrowUpDown } from "lucide-react";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import * as XLSX from "xlsx";
import type { DateRange } from "react-day-picker";

export interface Column<T> {
  key: string;
  header: string;
  render: (item: T) => React.ReactNode;
  searchable?: boolean;
  exportValue?: (item: T) => string | number;
  filterType?: "text" | "date" | "select" | "multiselect";
  filterOptions?: { value: string; label: string }[];
  filterKey?: string;
  filterValue?: (item: T) => string;
  headerRender?: () => React.ReactNode;
  filterable?: boolean;
  /** Enable sorting on this column */
  sortable?: boolean;
  /** Sort comparison type: "text" (alphabetical) or "number" (numeric). Defaults to "text" */
  sortType?: "text" | "number";
  /** Extract raw value for sorting (defaults to item[key]) */
  sortValue?: (item: T) => string | number;
}

interface DataTableProps<T> {
  data: T[];
  columns: Column<T>[];
  searchPlaceholder?: string;
  pageSize?: number;
  onRowClick?: (item: T) => void;
  actions?: React.ReactNode;
  exportFileName?: string;
  /** Default column filter values applied on mount */
  defaultFilters?: Record<string, string | string[] | DateRange | undefined>;
  /** Callback fired whenever the filtered dataset changes */
  onFilteredDataChange?: (filteredData: T[]) => void;
}

function getExportRows<T>(filtered: T[], columns: Column<T>[]) {
  return filtered.map((item) => {
    const row: Record<string, string | number> = {};
    columns.forEach((col) => {
      if (col.exportValue) {
        row[col.header] = col.exportValue(item);
      } else {
        const val = (item as any)[col.key];
        row[col.header] = val != null ? String(val) : "";
      }
    });
    return row;
  });
}

// ── Date Range Picker ──
function DateRangeFilter({ value, onChange }: { value: DateRange | undefined; onChange: (v: DateRange | undefined) => void }) {
  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button variant="outline" size="sm" className={cn("h-7 w-full justify-start text-left text-xs font-normal gap-1 px-2", !value && "text-muted-foreground")}>
          <CalendarIcon className="h-3 w-3 shrink-0" />
          {value?.from ? (
            value.to ? (
              <span className="truncate">{format(value.from, "dd/MM/yy")} – {format(value.to, "dd/MM/yy")}</span>
            ) : (
              <span className="truncate">Desde {format(value.from, "dd/MM/yy")}</span>
            )
          ) : (
            <span>Rango…</span>
          )}
          {value && (
            <X className="h-3 w-3 ml-auto shrink-0 opacity-50 hover:opacity-100" onClick={(e) => { e.stopPropagation(); onChange(undefined); }} />
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          mode="range"
          selected={value}
          onSelect={onChange}
          numberOfMonths={2}
          locale={es}
          className={cn("p-3 pointer-events-auto")}
        />
      </PopoverContent>
    </Popover>
  );
}

// ── Multi-Select Combobox Filter ──
function MultiSelectFilter({
  options,
  value,
  onChange,
  placeholder,
}: {
  options: { value: string; label: string }[];
  value: string[];
  onChange: (v: string[]) => void;
  placeholder: string;
}) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const filteredOptions = useMemo(() => {
    if (!search.trim()) return options;
    const q = search.toLowerCase();
    return options.filter((o) => o.label.toLowerCase().includes(q));
  }, [options, search]);

  const toggle = (val: string) => {
    onChange(
      value.includes(val) ? value.filter((v) => v !== val) : [...value, val]
    );
  };

  const selectedLabels = value
    .map((v) => options.find((o) => o.value === v)?.label)
    .filter(Boolean);

  return (
    <Popover open={open} onOpenChange={(o) => { setOpen(o); if (!o) setSearch(""); }}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          size="sm"
          role="combobox"
          aria-expanded={open}
          className={cn(
            "h-7 w-full justify-between text-left text-xs font-normal gap-1 px-2",
            value.length === 0 && "text-muted-foreground"
          )}
        >
          <span className="truncate flex-1">
            {value.length === 0
              ? "Todos"
              : value.length === 1
              ? selectedLabels[0]
              : `${value.length} seleccionados`}
          </span>
          <ChevronsUpDown className="h-3 w-3 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[220px] p-0" align="start">
        <div className="flex items-center border-b px-2">
          <Search className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />
          <input
            ref={inputRef}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Buscar..."
            className="flex h-8 w-full bg-transparent py-2 px-2 text-xs outline-none placeholder:text-muted-foreground"
          />
          {search && (
            <X className="h-3 w-3 shrink-0 opacity-50 cursor-pointer hover:opacity-100" onClick={() => setSearch("")} />
          )}
        </div>
        <div className="max-h-[200px] overflow-y-auto p-1">
          {filteredOptions.length === 0 ? (
            <p className="py-4 text-center text-xs text-muted-foreground">Sin resultados</p>
          ) : (
            filteredOptions.map((opt) => (
              <div
                key={opt.value}
                className="flex items-center gap-2 rounded-sm px-2 py-1.5 text-xs cursor-pointer hover:bg-accent hover:text-accent-foreground"
                onClick={() => toggle(opt.value)}
              >
                <Checkbox
                  checked={value.includes(opt.value)}
                  className="h-3.5 w-3.5"
                  tabIndex={-1}
                />
                <span className="truncate">{opt.label}</span>
              </div>
            ))
          )}
        </div>
        {value.length > 0 && (
          <div className="border-t p-1">
            <Button
              variant="ghost"
              size="sm"
              className="h-7 w-full text-xs justify-center"
              onClick={() => onChange([])}
            >
              Limpiar selección
            </Button>
          </div>
        )}
      </PopoverContent>
    </Popover>
  );
}

type ColumnFilters = Record<string, string | string[] | DateRange | undefined>;


export function DataTable<T extends { id: string }>({
  data,
  columns,
  searchPlaceholder = "Buscar...",
  pageSize = 10,
  onRowClick,
  actions,
  exportFileName = "export",
  defaultFilters,
  onFilteredDataChange,
}: DataTableProps<T>) {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(0);
  const [columnFilters, setColumnFilters] = useState<ColumnFilters>(defaultFilters || {});
  const [sortKey, setSortKey] = useState<string | null>(null);
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");
  const [showFilters, setShowFilters] = useState(() => {
    if (defaultFilters) {
      return Object.values(defaultFilters).some(v => {
        if (Array.isArray(v)) return v.length > 0;
        return v !== undefined && v !== "";
      });
    }
    return false;
  });

  const handleSort = (col: Column<T>) => {
    if (!col.sortable) return;
    if (sortKey === col.key) {
      setSortDir(prev => prev === "asc" ? "desc" : "asc");
    } else {
      setSortKey(col.key);
      setSortDir("asc");
    }
    setPage(0);
  };

  const filterableColumns = columns.filter(col => col.header && col.key !== "_actions" && col.filterable !== false);

  const activeFilterCount = Object.values(columnFilters).filter(v => {
    if (Array.isArray(v)) return v.length > 0;
    return v !== undefined && v !== "";
  }).length;

  const setFilter = (key: string, value: string | string[] | DateRange | undefined) => {
    setColumnFilters(prev => ({ ...prev, [key]: value }));
    setPage(0);
  };

  const clearAllFilters = () => {
    setColumnFilters({});
    setSearch("");
    setPage(0);
  };

  const filtered = useMemo(() => {
    let result = data;

    // Global search
    if (search.trim()) {
      const q = search.toLowerCase();
      result = result.filter((item) =>
        columns.some((col) => {
          if (!col.searchable) return false;
          const val = col.filterValue ? col.filterValue(item) : (item as any)[col.key];
          return typeof val === "string" && val.toLowerCase().includes(q);
        })
      );
    }

    // Per-column filters
    for (const col of filterableColumns) {
      const filterVal = columnFilters[col.key];
      if (filterVal === undefined || filterVal === "" || (Array.isArray(filterVal) && filterVal.length === 0)) continue;

      const filterType = col.filterType || "text";
      const filterKey = col.filterKey || col.key;
      result = result.filter((item) => {
        const rawVal = col.filterValue ? col.filterValue(item) : (item as any)[filterKey];

        if (filterType === "date" && typeof filterVal === "object" && !Array.isArray(filterVal) && "from" in filterVal) {
          const dateRange = filterVal as DateRange;
          if (!rawVal) return false;
          const itemDate = new Date(rawVal);
          if (isNaN(itemDate.getTime())) return false;
          if (dateRange.from && itemDate < dateRange.from) return false;
          if (dateRange.to) {
            const endOfDay = new Date(dateRange.to);
            endOfDay.setHours(23, 59, 59, 999);
            if (itemDate > endOfDay) return false;
          }
          return true;
        }

        if (filterType === "multiselect" && Array.isArray(filterVal)) {
          return filterVal.includes(rawVal);
        }

        if (filterType === "select" && typeof filterVal === "string") {
          return rawVal === filterVal;
        }

        // text filter
        if (typeof filterVal === "string" && typeof rawVal === "string") {
          return rawVal.toLowerCase().includes(filterVal.toLowerCase());
        }
        return true;
      });
    }

    // Sorting
    if (sortKey) {
      const sortCol = columns.find(c => c.key === sortKey);
      if (sortCol) {
        const sortType = sortCol.sortType || "text";
        result = [...result].sort((a, b) => {
          const aVal = sortCol.sortValue ? sortCol.sortValue(a) : (a as any)[sortCol.key];
          const bVal = sortCol.sortValue ? sortCol.sortValue(b) : (b as any)[sortCol.key];

          let cmp = 0;
          if (sortType === "number") {
            const aNum = typeof aVal === "number" ? aVal : parseFloat(String(aVal ?? "0").replace(/[^0-9.-]/g, "")) || 0;
            const bNum = typeof bVal === "number" ? bVal : parseFloat(String(bVal ?? "0").replace(/[^0-9.-]/g, "")) || 0;
            cmp = aNum - bNum;
          } else {
            const aStr = String(aVal ?? "").toLowerCase();
            const bStr = String(bVal ?? "").toLowerCase();
            cmp = aStr.localeCompare(bStr, "es");
          }
          return sortDir === "asc" ? cmp : -cmp;
        });
      }
    }

    return result;
  }, [data, search, columns, columnFilters, filterableColumns, sortKey, sortDir]);

  useEffect(() => {
    onFilteredDataChange?.(filtered);
  }, [filtered, onFilteredDataChange]);

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice(page * pageSize, (page + 1) * pageSize);

  const handleExport = useCallback(
    (fmt: "xlsx" | "csv") => {
      const rows = getExportRows(filtered, columns);
      const ws = XLSX.utils.json_to_sheet(rows);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, "Datos");
      XLSX.writeFile(wb, `${exportFileName}.${fmt}`, fmt === "csv" ? { bookType: "csv" } : undefined);
    },
    [filtered, columns, exportFileName]
  );

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <div className="flex items-center gap-2 w-full sm:w-auto">
          <div className="relative flex-1 sm:w-72">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder={searchPlaceholder}
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(0); }}
              className="pl-9"
            />
          </div>
          <Button
            variant={showFilters ? "default" : "outline"}
            size="sm"
            className="gap-1.5 shrink-0"
            onClick={() => setShowFilters(!showFilters)}
          >
            <Filter className="h-3.5 w-3.5" />
            Filtros
            {activeFilterCount > 0 && (
              <span className="ml-1 h-5 w-5 rounded-full bg-primary-foreground text-primary text-xs flex items-center justify-center font-bold">
                {activeFilterCount}
              </span>
            )}
          </Button>
          {activeFilterCount > 0 && (
            <Button variant="ghost" size="sm" className="gap-1 text-xs" onClick={clearAllFilters}>
              <X className="h-3 w-3" /> Limpiar
            </Button>
          )}
        </div>
        <div className="flex items-center gap-2">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" size="sm" className="gap-2">
                <Download className="h-4 w-4" /> Exportar
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => handleExport("xlsx")}>Exportar a Excel (.xlsx)</DropdownMenuItem>
              <DropdownMenuItem onClick={() => handleExport("csv")}>Exportar a CSV (.csv)</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          {actions}
        </div>
      </div>

      {/* Per-column filters */}
      {showFilters && (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-3 p-4 rounded-lg border bg-muted/30">
          {filterableColumns.map((col) => {
            const filterType = col.filterType || "text";
            return (
              <div key={col.key} className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">{col.header}</label>
                {filterType === "date" ? (
                  <DateRangeFilter
                    value={columnFilters[col.key] as DateRange | undefined}
                    onChange={(v) => setFilter(col.key, v)}
                  />
                ) : filterType === "multiselect" && col.filterOptions ? (
                  <MultiSelectFilter
                    options={col.filterOptions}
                    value={(columnFilters[col.key] as string[]) || []}
                    onChange={(v) => setFilter(col.key, v)}
                    placeholder={col.header}
                  />
                ) : filterType === "select" && col.filterOptions ? (
                  <Select
                    value={(columnFilters[col.key] as string) || "__all__"}
                    onValueChange={(v) => setFilter(col.key, v === "__all__" ? "" : v)}
                  >
                    <SelectTrigger className="h-7 text-xs">
                      <SelectValue placeholder="Todos" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="__all__">Todos</SelectItem>
                      {col.filterOptions.map((opt) => (
                        <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                ) : (
                  <Input
                    className="h-7 text-xs"
                    placeholder={`Filtrar ${col.header.toLowerCase()}…`}
                    value={(columnFilters[col.key] as string) || ""}
                    onChange={(e) => setFilter(col.key, e.target.value)}
                  />
                )}
              </div>
            );
          })}
        </div>
      )}

      <div className="rounded-xl border bg-card overflow-hidden">
        <Table>
          <TableHeader>
            <TableRow className="bg-muted/50 hover:bg-muted/50">
              {columns.map((col) => (
                <TableHead
                  key={col.key}
                  className={cn(
                    "text-xs font-semibold uppercase tracking-wider text-muted-foreground",
                    col.sortable && "cursor-pointer select-none hover:text-foreground transition-colors"
                  )}
                  onClick={() => col.sortable && handleSort(col)}
                >
                  {col.headerRender ? col.headerRender() : (
                    <div className="flex items-center gap-1">
                      {col.header}
                      {col.sortable && (
                        sortKey === col.key ? (
                          sortDir === "asc" ? <ArrowUp className="h-3.5 w-3.5" /> : <ArrowDown className="h-3.5 w-3.5" />
                        ) : (
                          <ArrowUpDown className="h-3 w-3 opacity-40" />
                        )
                      )}
                    </div>
                  )}
                </TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {paginated.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 text-center text-muted-foreground">
                  No se encontraron resultados
                </TableCell>
              </TableRow>
            ) : (
              paginated.map((item) => (
                <TableRow
                  key={item.id}
                  className={onRowClick ? "cursor-pointer" : ""}
                  onClick={() => onRowClick?.(item)}
                >
                  {columns.map((col) => (
                    <TableCell key={col.key}>{col.render(item)}</TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>{filtered.length} registro(s)</span>
          <div className="flex items-center gap-1">
            <Button variant="ghost" size="icon" disabled={page === 0} onClick={() => setPage(p => p - 1)}>
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <span className="px-2">{page + 1} / {totalPages}</span>
            <Button variant="ghost" size="icon" disabled={page >= totalPages - 1} onClick={() => setPage(p => p + 1)}>
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
