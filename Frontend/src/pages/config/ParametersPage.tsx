import { useState, useMemo } from "react";
import { Settings, Shield, Bell, GitBranch, Plug, Search, Save, ChevronRight, Regex, Plus, Trash2, Building2, Globe, MapPin, MapPinned, List, FolderPlus, Tag, Users, CreditCard, FileText, Mail, Lock, Database, Server, Cloud, Briefcase, Home, Star, Heart, Zap, Clock, Calendar, Phone, Bookmark, Flag, Award, Target, Layers, Package, Truck, ShoppingCart, Wallet, BarChart3, PieChart, TrendingUp, Activity, Key, Fingerprint, Eye, MessageSquare, Send, Inbox, Archive, Folder, File, Image, Video, Music, Headphones, Wifi, Monitor, Smartphone, Printer, HardDrive, Cpu, Code, Terminal, Bug, Wrench, Hammer, Palette, Paintbrush, Scissors, Crop, Grid, LayoutGrid, Table, Columns, AlignLeft, Type, Hash, AtSign, Link, Paperclip, Upload, Download, Share2, QrCode, Scan, CircleDollarSign, Receipt, Calculator, GraduationCap, BookOpen, Library, Landmark, Hospital, Stethoscope, Pill, Thermometer, Car, Plane, Ship, Train, Bike, Bus, Map as MapIcon, Navigation, Compass, Mountain, Trees, Umbrella, Sun, Moon, CloudRain, Snowflake, Flame, Droplets, Wind, Leaf, Banknote, ShieldCheck, MessageCircle } from "lucide-react";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { mockParameters, defaultParameterGroups, Parameter, ListItem, ParameterGroup } from "@/data/parameters";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import ListParamEditor from "@/components/parameters/ListParamEditor";
import HtmlParamEditor from "@/components/parameters/HtmlParamEditor";

const iconMap: Record<string, React.ReactNode> = {
  Settings: <Settings className="h-4 w-4" />,
  Shield: <Shield className="h-4 w-4" />,
  Bell: <Bell className="h-4 w-4" />,
  GitBranch: <GitBranch className="h-4 w-4" />,
  Plug: <Plug className="h-4 w-4" />,
  Regex: <Regex className="h-4 w-4" />,
  Building2: <Building2 className="h-4 w-4" />,
  Globe: <Globe className="h-4 w-4" />,
  MapPin: <MapPin className="h-4 w-4" />,
  MapPinned: <MapPinned className="h-4 w-4" />,
  List: <List className="h-4 w-4" />,
  Tag: <Tag className="h-4 w-4" />,
  Users: <Users className="h-4 w-4" />,
  CreditCard: <CreditCard className="h-4 w-4" />,
  FileText: <FileText className="h-4 w-4" />,
  Mail: <Mail className="h-4 w-4" />,
  Lock: <Lock className="h-4 w-4" />,
  Database: <Database className="h-4 w-4" />,
  Server: <Server className="h-4 w-4" />,
  Cloud: <Cloud className="h-4 w-4" />,
  Briefcase: <Briefcase className="h-4 w-4" />,
  Home: <Home className="h-4 w-4" />,
  Star: <Star className="h-4 w-4" />,
  Heart: <Heart className="h-4 w-4" />,
  Zap: <Zap className="h-4 w-4" />,
  Clock: <Clock className="h-4 w-4" />,
  Calendar: <Calendar className="h-4 w-4" />,
  Phone: <Phone className="h-4 w-4" />,
  Bookmark: <Bookmark className="h-4 w-4" />,
  Flag: <Flag className="h-4 w-4" />,
  Award: <Award className="h-4 w-4" />,
  Target: <Target className="h-4 w-4" />,
  Layers: <Layers className="h-4 w-4" />,
  Package: <Package className="h-4 w-4" />,
  Truck: <Truck className="h-4 w-4" />,
  ShoppingCart: <ShoppingCart className="h-4 w-4" />,
  Wallet: <Wallet className="h-4 w-4" />,
  BarChart3: <BarChart3 className="h-4 w-4" />,
  PieChart: <PieChart className="h-4 w-4" />,
  TrendingUp: <TrendingUp className="h-4 w-4" />,
  Activity: <Activity className="h-4 w-4" />,
  Key: <Key className="h-4 w-4" />,
  Fingerprint: <Fingerprint className="h-4 w-4" />,
  Eye: <Eye className="h-4 w-4" />,
  MessageSquare: <MessageSquare className="h-4 w-4" />,
  Send: <Send className="h-4 w-4" />,
  Inbox: <Inbox className="h-4 w-4" />,
  Archive: <Archive className="h-4 w-4" />,
  Folder: <Folder className="h-4 w-4" />,
  File: <File className="h-4 w-4" />,
  Image: <Image className="h-4 w-4" />,
  Video: <Video className="h-4 w-4" />,
  Music: <Music className="h-4 w-4" />,
  Headphones: <Headphones className="h-4 w-4" />,
  Wifi: <Wifi className="h-4 w-4" />,
  Monitor: <Monitor className="h-4 w-4" />,
  Smartphone: <Smartphone className="h-4 w-4" />,
  Printer: <Printer className="h-4 w-4" />,
  HardDrive: <HardDrive className="h-4 w-4" />,
  Cpu: <Cpu className="h-4 w-4" />,
  Code: <Code className="h-4 w-4" />,
  Terminal: <Terminal className="h-4 w-4" />,
  Bug: <Bug className="h-4 w-4" />,
  Wrench: <Wrench className="h-4 w-4" />,
  Hammer: <Hammer className="h-4 w-4" />,
  Palette: <Palette className="h-4 w-4" />,
  Paintbrush: <Paintbrush className="h-4 w-4" />,
  Scissors: <Scissors className="h-4 w-4" />,
  Crop: <Crop className="h-4 w-4" />,
  Grid: <Grid className="h-4 w-4" />,
  LayoutGrid: <LayoutGrid className="h-4 w-4" />,
  Table: <Table className="h-4 w-4" />,
  Columns: <Columns className="h-4 w-4" />,
  AlignLeft: <AlignLeft className="h-4 w-4" />,
  Type: <Type className="h-4 w-4" />,
  Hash: <Hash className="h-4 w-4" />,
  AtSign: <AtSign className="h-4 w-4" />,
  Link: <Link className="h-4 w-4" />,
  Paperclip: <Paperclip className="h-4 w-4" />,
  Upload: <Upload className="h-4 w-4" />,
  Download: <Download className="h-4 w-4" />,
  Share2: <Share2 className="h-4 w-4" />,
  QrCode: <QrCode className="h-4 w-4" />,
  Scan: <Scan className="h-4 w-4" />,
  CircleDollarSign: <CircleDollarSign className="h-4 w-4" />,
  Receipt: <Receipt className="h-4 w-4" />,
  Calculator: <Calculator className="h-4 w-4" />,
  GraduationCap: <GraduationCap className="h-4 w-4" />,
  BookOpen: <BookOpen className="h-4 w-4" />,
  Library: <Library className="h-4 w-4" />,
  Landmark: <Landmark className="h-4 w-4" />,
  Hospital: <Hospital className="h-4 w-4" />,
  Stethoscope: <Stethoscope className="h-4 w-4" />,
  Pill: <Pill className="h-4 w-4" />,
  Thermometer: <Thermometer className="h-4 w-4" />,
  Car: <Car className="h-4 w-4" />,
  Plane: <Plane className="h-4 w-4" />,
  Ship: <Ship className="h-4 w-4" />,
  Train: <Train className="h-4 w-4" />,
  Bike: <Bike className="h-4 w-4" />,
  Bus: <Bus className="h-4 w-4" />,
  Map: <MapIcon className="h-4 w-4" />,
  Navigation: <Navigation className="h-4 w-4" />,
  Compass: <Compass className="h-4 w-4" />,
  Mountain: <Mountain className="h-4 w-4" />,
  Trees: <Trees className="h-4 w-4" />,
  Umbrella: <Umbrella className="h-4 w-4" />,
  Sun: <Sun className="h-4 w-4" />,
  Moon: <Moon className="h-4 w-4" />,
  CloudRain: <CloudRain className="h-4 w-4" />,
  Snowflake: <Snowflake className="h-4 w-4" />,
  Flame: <Flame className="h-4 w-4" />,
  Droplets: <Droplets className="h-4 w-4" />,
  Wind: <Wind className="h-4 w-4" />,
  Leaf: <Leaf className="h-4 w-4" />,
  Globe2: <Globe className="h-4 w-4" />,
  Banknote: <Banknote className="h-4 w-4" />,
  ShieldCheck: <ShieldCheck className="h-4 w-4" />,
  MessageCircle: <MessageCircle className="h-4 w-4" />,
};

const availableIcons = Object.keys(iconMap);

export default function ParametersPage() {
  const [parameters, setParameters] = useState<Parameter[]>(mockParameters);
  const [groups, setGroups] = useState<ParameterGroup[]>(defaultParameterGroups);
  const [activeGroup, setActiveGroup] = useState<string>("general");
  const [search, setSearch] = useState("");
  const [parentFilter, setParentFilter] = useState<string>("all");
  const [editedValues, setEditedValues] = useState<Record<string, string>>({});
  const [editedDescriptions, setEditedDescriptions] = useState<Record<string, string>>({});
  const [editedListItems, setEditedListItems] = useState<Record<string, ListItem[]>>({});
  const [addOpen, setAddOpen] = useState(false);
  const [addGroupOpen, setAddGroupOpen] = useState(false);
  const [newGroup, setNewGroup] = useState({ label: '', icon: 'Tag' });
  const [iconSearch, setIconSearch] = useState('');
  const [newParam, setNewParam] = useState({ key: '', value: '', description: '', type: 'text' as Parameter['type'] });
  const filteredParams = useMemo(() => {
    return parameters.filter(p => {
      const matchGroup = p.group === activeGroup;
      const matchSearch = !search || p.key.toLowerCase().includes(search.toLowerCase()) || p.description.toLowerCase().includes(search.toLowerCase());
      const matchParent = parentFilter === 'all' || !p.parentKey || p.parentKey === parentFilter;
      return matchGroup && matchSearch && matchParent;
    });
  }, [parameters, activeGroup, search, parentFilter]);

  // Get parent options for hierarchy filters
  const parentFilterOptions = useMemo(() => {
    if (activeGroup === 'provinces') {
      return parameters.filter(p => p.group === 'countries').map(p => ({ key: p.key, label: p.value }));
    }
    if (activeGroup === 'cities') {
      return parameters.filter(p => p.group === 'provinces').map(p => ({ key: p.key, label: p.value }));
    }
    return [];
  }, [parameters, activeGroup]);

  const parentFilterLabel = activeGroup === 'provinces' ? 'País' : activeGroup === 'cities' ? 'Provincia' : '';

  const groupCounts = useMemo(() => {
    const counts: Record<string, number> = {};
    groups.forEach(g => { counts[g.id] = parameters.filter(p => p.group === g.id).length; });
    return counts;
  }, [parameters, groups]);

  const handleValueChange = (paramId: string, value: string) => {
    setEditedValues(prev => ({ ...prev, [paramId]: value }));
  };

  const handleDescriptionChange = (paramId: string, desc: string) => {
    setEditedDescriptions(prev => ({ ...prev, [paramId]: desc }));
  };

  const handleListItemsChange = (paramId: string, items: ListItem[]) => {
    setEditedListItems(prev => ({ ...prev, [paramId]: items }));
  };

  const handleSave = () => {
    setParameters(prev => prev.map(p => {
      const updated = { ...p };
      let changed = false;
      if (editedValues[p.id] !== undefined) { updated.value = editedValues[p.id]; changed = true; }
      if (editedDescriptions[p.id] !== undefined) { updated.description = editedDescriptions[p.id]; changed = true; }
      if (editedListItems[p.id] !== undefined) { updated.listItems = editedListItems[p.id]; changed = true; }
      if (changed) updated.updatedAt = new Date().toISOString().slice(0, 10);
      return updated;
    }));
    setEditedValues({});
    setEditedDescriptions({});
    setEditedListItems({});
    toast.success("Parámetros guardados correctamente");
  };

  const handleAddParam = () => {
    if (!newParam.key.trim() || !newParam.description.trim()) { toast.error("Clave y descripción son obligatorios"); return; }
    const id = `param_${Date.now()}_${Math.random().toString(36).slice(2, 5)}`;
    const param: Parameter = { id, group: activeGroup, key: newParam.key, value: newParam.value, description: newParam.description, type: newParam.type, listItems: newParam.type === 'list' ? [] : undefined, updatedAt: new Date().toISOString().slice(0, 10) };
    setParameters(prev => [...prev, param]);
    setNewParam({ key: '', value: '', description: '', type: 'text' });
    setAddOpen(false);
    toast.success("Parámetro agregado");
  };

  const handleDeleteParam = (paramId: string) => {
    setParameters(prev => prev.filter(p => p.id !== paramId));
    toast.success("Parámetro eliminado");
  };

  const hasChanges = Object.keys(editedValues).length > 0 || Object.keys(editedDescriptions).length > 0 || Object.keys(editedListItems).length > 0;
  const activeGroupLabel = groups.find(g => g.id === activeGroup)?.label ?? "";

  const handleAddGroup = () => {
    if (!newGroup.label.trim()) { toast.error("El nombre del grupo es obligatorio"); return; }
    const id = newGroup.label.toLowerCase().replace(/\s+/g, '_').replace(/[^a-z0-9_]/g, '');
    if (groups.some(g => g.id === id)) { toast.error("Ya existe un grupo con ese identificador"); return; }
    setGroups(prev => [...prev, { id, label: newGroup.label, icon: newGroup.icon }]);
    setActiveGroup(id);
    setNewGroup({ label: '', icon: 'Tag' });
    setAddGroupOpen(false);
    toast.success("Grupo agregado");
  };

  const handleDeleteGroup = (groupId: string) => {
    const hasParams = parameters.some(p => p.group === groupId);
    if (hasParams) { toast.error("No se puede eliminar un grupo que contiene parámetros"); return; }
    setGroups(prev => prev.filter(g => g.id !== groupId));
    if (activeGroup === groupId) setActiveGroup(groups[0]?.id ?? 'general');
    toast.success("Grupo eliminado");
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">Parámetros</h1>
          <p className="text-muted-foreground text-sm">Configuración general del sistema organizada por grupos</p>
        </div>
        {hasChanges && (
          <Button onClick={handleSave} className="gap-2">
            <Save className="h-4 w-4" /> Guardar cambios
          </Button>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-[260px_1fr] gap-6">
        {/* Groups sidebar */}
        <Card className="h-fit">
          <CardContent className="p-2 pt-4">
            <div className="space-y-3">
              {[
                { parent: 'General', children: ['channels', 'currencies', 'phone_codes', 'general', 'masks', 'security', 'entity_types'] },
                { parent: 'Técnicos', children: ['integrations', 'workflow'] },
                { parent: 'Notificaciones', children: ['notifications', 'whatsapp', 'sms', 'templates'] },
                { parent: 'Posicionamiento', children: ['countries', 'provinces', 'cities'] },
                { parent: 'Seguros', children: ['insurance_coverages'] },
                { parent: 'Tarjetas', children: ['card_networks', 'card_levels'] },
              ].map(section => {
                const sectionGroups = section.children
                  .map(id => groups.find(g => g.id === id))
                  .filter(Boolean) as typeof groups;
                // Also include any groups not in predefined sections
                if (sectionGroups.length === 0) return null;
                return (
                  <div key={section.parent}>
                    <p className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider px-3 py-1.5">{section.parent}</p>
                    <div className="space-y-0.5">
                      {sectionGroups.map(group => (
                        <button
                          key={group.id}
                          onClick={() => { setActiveGroup(group.id); setSearch(""); setParentFilter("all"); }}
                          className={cn(
                            "w-full flex items-center justify-between rounded-lg px-3 pl-6 py-2 text-sm transition-all",
                            activeGroup === group.id
                              ? "bg-primary text-primary-foreground shadow-sm"
                              : "text-foreground hover:bg-muted"
                          )}
                        >
                          <span className="flex items-center gap-2.5">
                            {iconMap[group.icon] ?? <Tag className="h-4 w-4" />}
                            {group.label}
                          </span>
                          <span className="flex items-center gap-1.5">
                            <Badge variant={activeGroup === group.id ? "secondary" : "outline"} className="text-[10px] px-1.5 py-0">
                              {groupCounts[group.id] ?? 0}
                            </Badge>
                            <ChevronRight className="h-3.5 w-3.5 opacity-50" />
                          </span>
                        </button>
                      ))}
                    </div>
                  </div>
                );
              })}
              {/* Groups not in any section */}
              {groups.filter(g => !['channels','currencies','phone_codes','general','masks','security','entity_types','integrations','notifications','whatsapp','sms','templates','workflow','countries','provinces','cities','insurance_coverages','card_networks','card_levels'].includes(g.id)).sort((a, b) => a.label.localeCompare(b.label, 'es')).map(group => (
                <button
                  key={group.id}
                  onClick={() => { setActiveGroup(group.id); setSearch(""); setParentFilter("all"); }}
                  className={cn(
                    "w-full flex items-center justify-between rounded-lg px-3 py-2 text-sm transition-all",
                    activeGroup === group.id
                      ? "bg-primary text-primary-foreground shadow-sm"
                      : "text-foreground hover:bg-muted"
                  )}
                >
                  <span className="flex items-center gap-2.5">
                    {iconMap[group.icon] ?? <Tag className="h-4 w-4" />}
                    {group.label}
                  </span>
                  <span className="flex items-center gap-1.5">
                    <Badge variant={activeGroup === group.id ? "secondary" : "outline"} className="text-[10px] px-1.5 py-0">
                      {groupCounts[group.id] ?? 0}
                    </Badge>
                    <ChevronRight className="h-3.5 w-3.5 opacity-50" />
                  </span>
                </button>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Parameters panel */}
        <div className="space-y-4">
          <div className="flex items-center gap-3">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder={`Buscar en ${activeGroupLabel}...`}
                value={search}
                onChange={e => setSearch(e.target.value)}
                className="pl-9"
              />
            </div>
            {parentFilterOptions.length > 0 && (
              <Select value={parentFilter} onValueChange={setParentFilter}>
                <SelectTrigger className="w-[200px] h-9">
                  <SelectValue placeholder={`Filtrar por ${parentFilterLabel}`} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos los {parentFilterLabel === 'País' ? 'países' : 'provincias'}</SelectItem>
                  {parentFilterOptions.map(opt => (
                    <SelectItem key={opt.key} value={opt.key}>{opt.label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
            <Badge variant="outline" className="text-xs">
              {filteredParams.length} parámetro{filteredParams.length !== 1 ? "s" : ""}
            </Badge>
            <Button size="sm" variant="outline" className="gap-1.5 text-xs" onClick={() => setAddOpen(true)}>
              <Plus className="h-3.5 w-3.5" /> Nuevo
            </Button>
          </div>

          <div className="space-y-3">
              {filteredParams.length === 0 ? (
                <Card><CardContent className="p-8 text-center text-muted-foreground text-sm">No se encontraron parámetros</CardContent></Card>
              ) : activeGroup === 'templates' ? (
                // Group template params by template name into cards
                (() => {
                  const templateGroups = new Map<string, Parameter[]>();
                  filteredParams.forEach(param => {
                    const match = param.key.match(/^templates\.(.+?)\.(titulo|formato|contenido)$/);
                    const groupKey = match ? match[1] : param.key;
                    if (!templateGroups.has(groupKey)) templateGroups.set(groupKey, []);
                    templateGroups.get(groupKey)!.push(param);
                  });

                  return Array.from(templateGroups.entries()).map(([tplName, tplParams]) => {
                    const tituloParam = tplParams.find(p => p.key.endsWith('.titulo'));
                    const tplLabel = tituloParam ? (editedValues[tituloParam.id] ?? tituloParam.value) : tplName;

                    return (
                      <Card key={tplName} className="border-2">
                        <CardHeader className="pb-2 pt-4 px-5">
                          <CardTitle className="text-sm flex items-center gap-2">
                            <FileText className="h-4 w-4 text-primary" />
                            {tplLabel}
                          </CardTitle>
                        </CardHeader>
                        <CardContent className="px-5 pb-4 space-y-3">
                          {tplParams.map(param => {
                            const currentValue = editedValues[param.id] ?? param.value;
                            const currentDescription = editedDescriptions[param.id] ?? param.description;
                            const currentListItems = editedListItems[param.id] ?? param.listItems ?? [];
                            const isEdited = editedValues[param.id] !== undefined || editedDescriptions[param.id] !== undefined || editedListItems[param.id] !== undefined;
                            const fieldLabel = param.key.split('.').pop() || '';

                            return (
                              <div key={param.id} className="group space-y-1.5 rounded-lg border p-3 bg-muted/30">
                                <div className="flex items-start justify-between gap-4">
                                  <div className="flex items-center gap-2 min-w-0">
                                    <Badge variant="secondary" className="text-[10px] px-1.5 py-0 capitalize">{fieldLabel}</Badge>
                                    <code className="text-xs font-mono bg-muted px-1.5 py-0.5 rounded text-foreground">{param.key}</code>
                                    {isEdited && <Badge variant="outline" className="text-[10px] px-1.5 py-0 border-accent text-accent">modificado</Badge>}
                                  </div>
                                  <div className="flex items-center gap-2 shrink-0">
                                    <p className="text-[11px] text-muted-foreground/60">Actualizado: {param.updatedAt}</p>
                                    <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive opacity-0 group-hover:opacity-100 transition-opacity" onClick={() => handleDeleteParam(param.id)}>
                                      <Trash2 className="h-3.5 w-3.5" />
                                    </Button>
                                  </div>
                                </div>
                                <Input
                                  value={currentDescription}
                                  onChange={e => handleDescriptionChange(param.id, e.target.value)}
                                  className="h-8 text-sm text-muted-foreground"
                                  placeholder="Descripción del parámetro"
                                />
                                <div className={cn((param.type === 'list' || param.type === 'html') ? 'w-full' : 'max-w-xs')}>
                                  {param.type === 'html' ? (
                                    <HtmlParamEditor value={currentValue} onChange={v => handleValueChange(param.id, v)} />
                                  ) : param.type === 'boolean' ? (
                                    <div className="flex items-center gap-2">
                                      <span className="text-xs text-muted-foreground">{currentValue === 'true' ? 'Activo' : 'Inactivo'}</span>
                                      <Switch checked={currentValue === 'true'} onCheckedChange={checked => handleValueChange(param.id, String(checked))} />
                                    </div>
                                  ) : param.type === 'select' && param.options ? (
                                    <Select value={currentValue} onValueChange={v => handleValueChange(param.id, v)}>
                                      <SelectTrigger className="h-9 text-sm"><SelectValue /></SelectTrigger>
                                      <SelectContent>{param.options.map(opt => <SelectItem key={opt} value={opt}>{opt}</SelectItem>)}</SelectContent>
                                    </Select>
                                  ) : (
                                    <Input type={param.type === 'number' ? 'number' : 'text'} value={currentValue} onChange={e => handleValueChange(param.id, e.target.value)} className="h-9 text-sm" />
                                  )}
                                </div>
                              </div>
                            );
                          })}
                        </CardContent>
                      </Card>
                    );
                  });
                })()
              ) : (
                filteredParams.map(param => {
                      const currentValue = editedValues[param.id] ?? param.value;
                      const currentDescription = editedDescriptions[param.id] ?? param.description;
                      const currentListItems = editedListItems[param.id] ?? param.listItems ?? [];
                      const isEdited = editedValues[param.id] !== undefined || editedDescriptions[param.id] !== undefined || editedListItems[param.id] !== undefined;

                  return (
                    <Card key={param.id} className="group">
                      <CardContent className="px-5 py-4 space-y-3">
                        <div className="flex items-start justify-between gap-4">
                          <div className="flex items-center gap-2 min-w-0">
                            <code className="text-xs font-mono bg-muted px-1.5 py-0.5 rounded text-foreground">{param.key}</code>
                            {isEdited && <Badge variant="outline" className="text-[10px] px-1.5 py-0 border-accent text-accent">modificado</Badge>}
                          </div>
                          <div className="flex items-center gap-2 shrink-0">
                            <p className="text-[11px] text-muted-foreground/60">Actualizado: {param.updatedAt}</p>
                            <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive opacity-0 group-hover:opacity-100 transition-opacity" onClick={() => handleDeleteParam(param.id)}>
                              <Trash2 className="h-3.5 w-3.5" />
                            </Button>
                          </div>
                        </div>
                        <Input
                          value={currentDescription}
                          onChange={e => handleDescriptionChange(param.id, e.target.value)}
                          className="h-8 text-sm text-muted-foreground"
                          placeholder="Descripción del parámetro"
                        />

                        <div className={cn((param.type === 'list' || param.type === 'html') ? 'w-full' : 'max-w-xs')}>
                          {param.type === 'html' ? (
                            <HtmlParamEditor
                              value={currentValue}
                              onChange={v => handleValueChange(param.id, v)}
                            />
                          ) : param.type === 'list' ? (
                            <ListParamEditor
                              items={currentListItems}
                              onChange={items => handleListItemsChange(param.id, items)}
                            />
                          ) : param.type === 'boolean' ? (
                            <div className="flex items-center gap-2">
                              <span className="text-xs text-muted-foreground">{currentValue === 'true' ? 'Activo' : 'Inactivo'}</span>
                              <Switch
                                checked={currentValue === 'true'}
                                onCheckedChange={checked => handleValueChange(param.id, String(checked))}
                              />
                            </div>
                          ) : param.type === 'select' && param.options ? (
                            <Select value={currentValue} onValueChange={v => handleValueChange(param.id, v)}>
                              <SelectTrigger className="h-9 text-sm">
                                <SelectValue />
                              </SelectTrigger>
                              <SelectContent>
                                {param.options.map(opt => (
                                  <SelectItem key={opt} value={opt}>{opt}</SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                          ) : (
                            <Input
                              type={param.type === 'number' ? 'number' : 'text'}
                              value={currentValue}
                              onChange={e => handleValueChange(param.id, e.target.value)}
                              className="h-9 text-sm"
                            />
                          )}
                        </div>
                      </CardContent>
                    </Card>
                  );
                })
              )}
          </div>
        </div>
      </div>

      {/* Add Parameter Dialog */}
      <Dialog open={addOpen} onOpenChange={setAddOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader><DialogTitle>Nuevo Parámetro — {activeGroupLabel}</DialogTitle></DialogHeader>
          <div className="space-y-4 mt-2">
            <div className="space-y-1.5">
              <label className="text-sm font-medium text-foreground">Clave *</label>
              <Input value={newParam.key} onChange={e => setNewParam(p => ({ ...p, key: e.target.value }))} placeholder={`${activeGroup}.nuevo_parametro`} />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-medium text-foreground">Descripción *</label>
              <Input value={newParam.description} onChange={e => setNewParam(p => ({ ...p, description: e.target.value }))} placeholder="Descripción del parámetro" />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-medium text-foreground">Tipo</label>
              <Select value={newParam.type} onValueChange={v => setNewParam(p => ({ ...p, type: v as Parameter['type'] }))}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="text">Texto</SelectItem>
                  <SelectItem value="number">Número</SelectItem>
                  <SelectItem value="boolean">Booleano</SelectItem>
                   <SelectItem value="list">Lista</SelectItem>
                   <SelectItem value="html">HTML</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-medium text-foreground">Valor inicial</label>
              <Input value={newParam.value} onChange={e => setNewParam(p => ({ ...p, value: e.target.value }))} placeholder="Valor por defecto" />
            </div>
          </div>
          <DialogFooter className="mt-4">
            <Button variant="outline" onClick={() => setAddOpen(false)}>Cancelar</Button>
            <Button onClick={handleAddParam}>Agregar</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Add Group Dialog */}
      <Dialog open={addGroupOpen} onOpenChange={setAddGroupOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader><DialogTitle>Nuevo Grupo de Parámetros</DialogTitle></DialogHeader>
          <div className="space-y-4 mt-2">
            <div className="space-y-1.5">
              <label className="text-sm font-medium text-foreground">Nombre *</label>
              <Input value={newGroup.label} onChange={e => setNewGroup(p => ({ ...p, label: e.target.value }))} placeholder="Ej: Reportes" />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-medium text-foreground">Ícono</label>
              <div className="border border-border rounded-lg p-2">
                <Input
                  placeholder="Buscar ícono..."
                  value={iconSearch}
                  onChange={e => setIconSearch(e.target.value)}
                  className="h-8 text-xs mb-2"
                />
                <div className="grid grid-cols-8 gap-1 max-h-40 overflow-y-auto">
                  {availableIcons
                    .filter(icon => icon.toLowerCase().includes(iconSearch.toLowerCase()))
                    .map(icon => (
                      <button
                        key={icon}
                        type="button"
                        onClick={() => setNewGroup(p => ({ ...p, icon }))}
                        title={icon}
                        className={cn(
                          "flex items-center justify-center h-8 w-full rounded transition-colors",
                          newGroup.icon === icon
                            ? "bg-primary text-primary-foreground"
                            : "hover:bg-muted text-foreground"
                        )}
                      >
                        {iconMap[icon]}
                      </button>
                    ))}
                </div>
              </div>
              {newGroup.icon && (
                <p className="text-xs text-muted-foreground flex items-center gap-1.5 mt-1">
                  Seleccionado: {iconMap[newGroup.icon]} <span>{newGroup.icon}</span>
                </p>
              )}
            </div>
          </div>
          <DialogFooter className="mt-4">
            <Button variant="outline" onClick={() => { setAddGroupOpen(false); setIconSearch(''); }}>Cancelar</Button>
            <Button onClick={() => { handleAddGroup(); setIconSearch(''); }}>Agregar</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
