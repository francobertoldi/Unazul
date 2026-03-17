import { useState } from "react";
import { Check, ChevronsUpDown } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from "@/components/ui/command";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { mockParameters } from "@/data/parameters";

const phoneCodes = mockParameters.filter(p => p.group === 'phone_codes');

interface PhoneCodeSelectProps {
  value?: string;
  onValueChange: (value: string) => void;
}

export function PhoneCodeSelect({ value, onValueChange }: PhoneCodeSelectProps) {
  const [open, setOpen] = useState(false);
  const selected = phoneCodes.find(p => p.value === value);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className="w-[200px] shrink-0 justify-between font-normal"
        >
          {selected ? (
            <span className="flex items-center gap-1.5 truncate">
              <span>{selected.icon}</span>
              <span className="truncate">{selected.description} ({selected.value})</span>
            </span>
          ) : (
            <span className="text-muted-foreground">Cód. país...</span>
          )}
          <ChevronsUpDown className="ml-1 h-3.5 w-3.5 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[280px] p-0" align="start">
        <Command>
          <CommandInput placeholder="Buscar país..." />
          <CommandList>
            <CommandEmpty>No se encontró.</CommandEmpty>
            <CommandGroup>
              {phoneCodes.map(p => (
                <CommandItem
                  key={p.id}
                  value={`${p.icon} ${p.description} ${p.value}`}
                  onSelect={() => {
                    onValueChange(p.value);
                    setOpen(false);
                  }}
                >
                  <Check className={cn("mr-2 h-3.5 w-3.5 shrink-0", value === p.value ? "opacity-100" : "opacity-0")} />
                  <span className="text-base mr-2">{p.icon}</span>
                  <span>{p.description}</span>
                  <span className="ml-auto text-muted-foreground">({p.value})</span>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
